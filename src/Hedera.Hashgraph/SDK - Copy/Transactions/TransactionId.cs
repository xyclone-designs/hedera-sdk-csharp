// SPDX-License-Identifier: Apache-2.0
using Java.Util.Concurrent.Task;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Concurrent.Atomic;
using Java.Util.Function;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// The client-generated ID for a transaction.
    /// 
    /// <p>This is used for retrieving receipts and records for a transaction, for appending to a file
    /// right after creating it, for instantiating a smart contract with bytecode in a file just created,
    /// and internally by the network for detecting when duplicate transactions are submitted.
    /// </summary>
    public sealed class TransactionId : IComparable<TransactionId>
    {
        /// <summary>
        /// The Account ID that paid for this transaction.
        /// </summary>
        public readonly AccountId accountId;
        /// <summary>
        /// The time from when this transaction is valid.
        /// 
        /// <p>When a transaction is submitted there is additionally a validDuration (defaults to 120s)
        /// and together they define a time window that a transaction may be processed in.
        /// </summary>
        public readonly Timestamp validStart;
        private bool scheduled = false;
        private int nonce = null;
        private static readonly long NANOSECONDS_PER_MILLISECOND = 1000000;
        private static readonly long TIMESTAMP_INCREMENT_NANOSECONDS = 1000;
        private static readonly long NANOSECONDS_TO_REMOVE = 10000000000;
        private static readonly AtomicLong monotonicTime = new AtomicLong();
        /// <summary>
        /// No longer part of the public API. Use `Transaction.withValidStart()` instead.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="validStart">the valid start time</param>
        public TransactionId(AccountId accountId, Timestamp validStart)
        {
            accountId = accountId;
            validStart = validStart;
            scheduled = false;
        }

        /// <summary>
        /// Create a transaction id.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="validStart">the valid start time</param>
        /// <returns>                         the new transaction id</returns>
        public static TransactionId WithValidStart(AccountId accountId, Timestamp validStart)
        {
            return new TransactionId(accountId, validStart);
        }

        /// <summary>
        /// Generates a new transaction ID for the given account ID.
        /// 
        /// <p>Note that transaction IDs are made of the valid start of the transaction and the account
        /// that will be charged the transaction fees for the transaction.
        /// </summary>
        /// <param name="accountId">the ID of the Hedera account that will be charge the transaction fees.</param>
        /// <returns>{@link TransactionId}</returns>
        public static TransactionId Generate(AccountId accountId)
        {
            long currentTime;
            long lastTime;

            // Loop to ensure the generated timestamp is strictly increasing,
            // and it handles the case where the system clock appears to move backward
            // or if multiple threads attempt to generate a timestamp concurrently.
            do
            {

                // Get the current time in nanoseconds and remove a few seconds to allow for some time drift
                // between the client and the receiving node and prevented spurious INVALID_TRANSACTION_START.
                currentTime = System.CurrentTimeMillis() * NANOSECONDS_PER_MILLISECOND - NANOSECONDS_TO_REMOVE;

                // Get the last recorded timestamp.
                lastTime = monotonicTime.Get();

                // If the current time is less than or equal to the last recorded time,
                // adjust the timestamp to ensure it is strictly increasing.
                if (currentTime <= lastTime)
                {
                    currentTime = lastTime + TIMESTAMP_INCREMENT_NANOSECONDS;
                }
            }
            while (!monotonicTime.CompareAndSet(lastTime, currentTime));

            // NOTE: using ThreadLocalRandom because it's compatible with Android SDK version 26
            return new TransactionId(accountId, Timestamp.OfEpochSecond(0, currentTime + ThreadLocalRandom.Current().NextLong(1000)));
        }

        /// <summary>
        /// Create a transaction id from a protobuf.
        /// </summary>
        /// <param name="transactionID">the protobuf</param>
        /// <returns>                         the new transaction id</returns>
        public static TransactionId FromProtobuf(Proto.TransactionID transactionID)
        {
            var accountId = transactionID.HasAccountID() ? AccountId.FromProtobuf(transactionID.GetAccountID()) : null;
            var validStart = transactionID.HasTransactionValidStart() ? Utils.TimestampConverter.FromProtobuf(transactionID.GetTransactionValidStart()) : null;
            return new TransactionId(accountId, validStart).SetScheduled(transactionID.GetScheduled()).SetNonce((transactionID.GetNonce() != 0) ? transactionID.GetNonce() : null);
        }

        /// <summary>
        /// Create a new transaction id from a string.
        /// </summary>
        /// <param name="s">the string representing the transaction id</param>
        /// <returns>                         the new transaction id</returns>
        public static TransactionId FromString(string s)
        {
            var parts = s.Split("/", 2);
            var nonce = (parts.Length == 2) ? int.Parse(parts[1]) : null;
            parts = parts[0].Split("\\?", 2);
            var scheduled = parts.Length == 2 && parts[1].Equals("scheduled");
            parts = parts[0].Split("@", 2);
            if (parts.Length != 2)
            {
                throw new ArgumentException("expecting {account}@{seconds}.{nanos}[?scheduled][/nonce]");
            }

            AccountId accountId = AccountId.FromString(parts[0]);
            var validStartParts = parts[1].Split("\\.", 2);
            if (validStartParts.Length != 2)
            {
                throw new ArgumentException("expecting {account}@{seconds}.{nanos}");
            }

            Timestamp validStart = Timestamp.OfEpochSecond(long.Parse(validStartParts[0]), long.Parse(validStartParts[1]));
            return new TransactionId(accountId, validStart).SetScheduled(scheduled).SetNonce(nonce);
        }

        /// <summary>
        /// Create a new transaction id from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new transaction id</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TransactionId FromBytes(byte[] bytes)
        {
            return FromProtobuf(TransactionID.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Extract the scheduled status.
        /// </summary>
        /// <returns>                         the scheduled status</returns>
        public bool GetScheduled()
        {
            return scheduled;
        }

        /// <summary>
        /// Assign the scheduled status.
        /// </summary>
        /// <param name="scheduled">the scheduled status</param>
        /// <returns>{@code this}</returns>
        public TransactionId SetScheduled(bool scheduled)
        {
            scheduled = scheduled;
            return this;
        }

        /// <summary>
        /// Extract the nonce.
        /// </summary>
        /// <returns>                         the nonce value</returns>
        public int GetNonce()
        {
            return nonce;
        }

        /// <summary>
        /// Assign the nonce value.
        /// </summary>
        /// <param name="nonce">the nonce value</param>
        /// <returns>{@code this}</returns>
        public TransactionId SetNonce(int nonce)
        {
            nonce = nonce;
            return this;
        }

        /// <summary>
        /// Fetch the receipt of the transaction.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>                         the transaction receipt</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="ReceiptStatusException">when there is an issue with the receipt</exception>
        public TransactionReceipt GetReceipt(Client client)
        {
            return GetReceipt(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Fetch the receipt of the transaction.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>                         the transaction receipt</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="ReceiptStatusException">when there is an issue with the receipt</exception>
        public TransactionReceipt GetReceipt(Client client, Duration timeout)
        {
            var receipt = new TransactionReceiptQuery().SetTransactionId(this).Execute(client, timeout);
            if (receipt.status != Status.SUCCESS)
            {
                throw new ReceiptStatusException(this, receipt);
            }

            return receipt;
        }

        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>                         future result of the transaction receipt</returns>
        public Task<TransactionReceipt> GetReceiptAsync(Client client)
        {
            return GetReceiptAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>                         the transaction receipt</returns>
        public Task<TransactionReceipt> GetReceiptAsync(Client client, Duration timeout)
        {
            return new TransactionReceiptQuery().SetTransactionId(this).ExecuteAsync(client, timeout).ThenCompose((receipt) =>
            {
                if (receipt.status != Status.SUCCESS)
                {
                    return FailedFuture(new ReceiptStatusException(this, receipt));
                }

                return CompletedFuture(receipt);
            });
        }

        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void GetReceiptAsync(Client client, Action<TransactionReceipt, Exception> callback)
        {
            ActionHelper.Action(GetReceiptAsync(client), callback);
        }

        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void GetReceiptAsync(Client client, Duration timeout, Action<TransactionReceipt, Exception> callback)
        {
            ActionHelper.Action(GetReceiptAsync(client, timeout), callback);
        }

        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void GetReceiptAsync(Client client, Action<TransactionReceipt> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(GetReceiptAsync(client), onSuccess, onFailure);
        }

        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void GetReceiptAsync(Client client, Duration timeout, Action<TransactionReceipt> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(GetReceiptAsync(client, timeout), onSuccess, onFailure);
        }

        /// <summary>
        /// Fetch the record of the transaction.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>                         the transaction record</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="ReceiptStatusException">when there is an issue with the receipt</exception>
        public TransactionRecord GetRecord(Client client)
        {
            return GetRecord(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Fetch the record of the transaction.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>                         the transaction record</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="ReceiptStatusException">when there is an issue with the receipt</exception>
        public TransactionRecord GetRecord(Client client, Duration timeout)
        {
            GetReceipt(client, timeout);
            return new TransactionRecordQuery().SetTransactionId(this).Execute(client, timeout);
        }

        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>                         future result of the transaction record</returns>
        public Task<TransactionRecord> GetRecordAsync(Client client)
        {
            return GetRecordAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>                         future result of the transaction record</returns>
        public Task<TransactionRecord> GetRecordAsync(Client client, Duration timeout)
        {

            // note: we get the receipt first to ensure consensus has been reached
            return GetReceiptAsync(client, timeout).ThenCompose((receipt) => new TransactionRecordQuery().SetTransactionId(this).ExecuteAsync(client, timeout));
        }

        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void GetRecordAsync(Client client, Action<TransactionRecord, Exception> callback)
        {
            ActionHelper.Action(GetRecordAsync(client), callback);
        }

        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void GetRecordAsync(Client client, Duration timeout, Action<TransactionRecord, Exception> callback)
        {
            ActionHelper.Action(GetRecordAsync(client, timeout), callback);
        }

        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void GetRecordAsync(Client client, Action<TransactionRecord> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(GetRecordAsync(client), onSuccess, onFailure);
        }

        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void GetRecordAsync(Client client, Duration timeout, Action<TransactionRecord> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(GetRecordAsync(client, timeout), onSuccess, onFailure);
        }

        /// <summary>
        /// Extract the transaction id protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        TransactionID ToProtobuf()
        {
            var id = TransactionID.NewBuilder().SetScheduled(scheduled).SetNonce((nonce != null) ? nonce : 0);
            if (accountId != null)
            {
                id.SetAccountID(accountId.ToProtobuf());
            }

            if (validStart != null)
            {
                id.SetTransactionValidStart(Utils.TimestampConverter.ToProtobuf(validStart));
            }

            return id.Build();
        }

        private string ToStringPostfix()
        {
            Objects.RequireNonNull(validStart);
            return "@" + validStart.GetEpochSecond() + "." + String.Format("%09d", validStart.GetNano()) + (scheduled ? "?scheduled" : "") + ((nonce != null) ? "/" + nonce : "");
        }

        public override string ToString()
        {
            if (accountId != null && validStart != null)
            {
                return "" + accountId + ToStringPostfix();
            }
            else
            {
                throw new InvalidOperationException("`TransactionId.toString()` is non-exhaustive");
            }
        }

        /// <summary>
        /// Convert to a string representation with checksum.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <returns>                         the string representation with checksum</returns>
        public override string ToStringWithChecksum(Client client)
        {
            if (accountId != null && validStart != null)
            {
                return "" + accountId.ToStringWithChecksum(client) + ToStringPostfix();
            }
            else
            {
                throw new InvalidOperationException("`TransactionId.toStringWithChecksum()` is non-exhaustive");
            }
        }

        /// <summary>
        /// Extract the byte array representation.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override bool Equals(object @object)
        {
            if (!(@object is TransactionId))
            {
                return false;
            }

            var id = (TransactionId)@object;
            if (accountId != null && validStart != null && id.accountId != null && id.validStart != null)
            {
                return id.accountId.Equals(accountId) && id.validStart.Equals(validStart) && scheduled == id.scheduled;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public int CompareTo(TransactionId o)
        {
            Objects.RequireNonNull(o);
            if (scheduled != o.scheduled)
            {
                return scheduled ? 1 : -1;
            }

            var thisAccountIdIsNull = (accountId == null);
            var otherAccountIdIsNull = (o.accountId == null);
            if (thisAccountIdIsNull != otherAccountIdIsNull)
            {
                return thisAccountIdIsNull ? -1 : 1;
            }

            if (!thisAccountIdIsNull)
            {
                int accountIdComparison = accountId.CompareTo(o.accountId);
                if (accountIdComparison != 0)
                {
                    return accountIdComparison;
                }
            }

            var thisStartIsNull = (validStart == null);
            var otherStartIsNull = (o.validStart == null);
            if (thisStartIsNull != otherStartIsNull)
            {
                return thisAccountIdIsNull ? -1 : 1;
            }

            if (!thisStartIsNull)
            {
                return validStart.CompareTo(o.validStart);
            }

            return 0;
        }
    }
}