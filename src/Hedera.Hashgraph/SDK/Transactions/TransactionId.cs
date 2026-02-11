// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Threading.Tasks;

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
		private static readonly long NANOSECONDS_PER_MILLISECOND = 1000000;
		private static readonly long TIMESTAMP_INCREMENT_NANOSECONDS = 1000;
		private static readonly long NANOSECONDS_TO_REMOVE = 10000000000;
		private static readonly AtomicLong monotonicTime = new ();

        
        /// <summary>
        /// No longer part of the public API. Use `Transaction.withValidStart()` instead.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="ValidStart">the valid start time</param>
        public TransactionId(AccountId accountId, Timestamp validStart)
        {
            AccountId = accountId;
            ValidStart = validStart;
            Scheduled = false;
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
				currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * NANOSECONDS_PER_MILLISECOND - NANOSECONDS_TO_REMOVE;

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

			return new TransactionId(accountId, new Timestamp
			{
				Seconds = 0,
				Nanos = (int)(currentTime + Random.Shared.NextInt64(1000))
			});
		}
		/// <summary>
		/// Create a transaction id.
		/// </summary>
		/// <param name="accountId">the account id</param>
		/// <param name="ValidStart">the valid start time</param>
		/// <returns>                         the new transaction id</returns>
		public static TransactionId WithValidStart(AccountId accountId, Timestamp ValidStart)
        {
            return new TransactionId(accountId, ValidStart);
        }

        /// <summary>
        /// Create a new transaction id from a string.
        /// </summary>
        /// <param name="s">the string representing the transaction id</param>
        /// <returns>                         the new transaction id</returns>
        public static TransactionId FromString(string s)
        {
            var parts = s.Split("/", 2);
            int? nonce = (parts.Length == 2) ? int.Parse(parts[1]) : null;
            parts = parts[0].Split("\\?", 2);
            var scheduled = parts.Length == 2 && parts[1].Equals("scheduled");
            parts = parts[0].Split("@", 2);
            if (parts.Length != 2)
            {
                throw new ArgumentException("expecting {account}@{seconds}.{nanos}[?scheduled][/nonce]");
            }

            AccountId accountId = AccountId.FromString(parts[0]);
            var ValidStartParts = parts[1].Split("\\.", 2);
            if (ValidStartParts.Length != 2)
            {
                throw new ArgumentException("expecting {account}@{seconds}.{nanos}");
            }

            Timestamp ValidStart = new ()
			{
				Seconds = long.Parse(ValidStartParts[0]),
				Nanos = int.Parse(ValidStartParts[1])
			};
            return new TransactionId(accountId, ValidStart)
            {
				Scheduled = scheduled,
				Nonce = nonce
			};
        }
        /// <summary>
        /// Create a new transaction id from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new transaction id</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TransactionId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.TransactionID.Parser.ParseFrom(bytes));
        }
		/// <summary>
		/// Create a transaction id from a protobuf.
		/// </summary>
		/// <param name="transactionID">the protobuf</param>
		/// <returns>                         the new transaction id</returns>
		public static TransactionId FromProtobuf(Proto.TransactionID transactionID)
		{
			var accountId = transactionID.AccountID is not null ? AccountId.FromProtobuf(transactionID.AccountID) : null;
			var ValidStart = transactionID.TransactionValidStart is not null ? Utils.TimestampConverter.FromProtobuf(transactionID.TransactionValidStart) : null;

			return new TransactionId(accountId, ValidStart)
			{
				Scheduled = transactionID.Scheduled,
				Nonce = transactionID.Nonce != 0 ? transactionID.Nonce : null
			};
		}

		/// <summary>
		/// Extract the nonce.
		/// </summary>
		public int? Nonce { get; set; }
		/// <summary>
		/// Extract the scheduled status.
		/// </summary>
		public bool Scheduled { get; set; }
		/// <summary>
		/// The Account ID that paid for this transaction.
		/// </summary>
		public AccountId AccountId { get; }
		/// <summary>
		/// The time from when this transaction is valid.
		/// 
		/// <p>When a transaction is submitted there is additionally a validDuration (defaults to 120s)
		/// and together they define a time window that a transaction may be processed in.
		/// </summary>
		public Timestamp ValidStart { get; }

		private string ToStringPostfix()
		{
			return "@" + ValidStart.Seconds + "." + ValidStart.Nanos + (Scheduled ? "?scheduled" : "") + (Nonce != null ? "/" + Nonce : "");
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
            return GetReceipt(client, client.RequestTimeout);
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
            var receipt = new TransactionReceiptQuery
            {
				PaymentTransactionId = this

			}.Execute(client, timeout);

            if (receipt.Status != ResponseStatus.Success)
				throw new ReceiptStatusException(this, receipt);

			return receipt;
        }
        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>                         future result of the transaction receipt</returns>
        public Task<TransactionReceipt> GetReceiptAsync(Client client)
        {
            return GetReceiptAsync(client, client.RequestTimeout);
        }
        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>                         the transaction receipt</returns>
        public async Task<TransactionReceipt> GetReceiptAsync(Client client, Duration timeout)
        {
            TransactionReceipt transactionreceipt = await new TransactionReceiptQuery
            {
                PaymentTransactionId = this

            }.ExecuteAsync(client, timeout);

			if (transactionreceipt.Status != ResponseStatus.Success)
				throw new ReceiptStatusException(this, transactionreceipt);

			return transactionreceipt;
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
            return GetRecord(client, client.RequestTimeout);
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

            return new TransactionRecordQuery
            {
                PaymentTransactionId = this

			}.Execute(client, timeout);
        }
        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>                         future result of the transaction record</returns>
        public Task<TransactionRecord> GetRecordAsync(Client client)
        {
            return GetRecordAsync(client, client.RequestTimeout);
        }
        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>                         future result of the transaction record</returns>
        public async Task<TransactionRecord> GetRecordAsync(Client client, Duration timeout)
        {
            // note: we get the receipt first to ensure consensus has been reached
            TransactionReceipt _ = await GetReceiptAsync(client, timeout);
			
            return await new TransactionRecordQuery
			{
				TransactionId = this

			}.ExecuteAsync(client, timeout);
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

		public int CompareTo(TransactionId? o)
		{
			if (Scheduled != o?.Scheduled)
			{
				return Scheduled ? 1 : -1;
			}

			var thisAccountIdIsNull = (AccountId == null);
			var otherAccountIdIsNull = (o?.AccountId == null);
			if (thisAccountIdIsNull != otherAccountIdIsNull)
			{
				return thisAccountIdIsNull ? -1 : 1;
			}

			if (!thisAccountIdIsNull)
			{
				int accountIdComparison = AccountId!.CompareTo(o?.AccountId);
				if (accountIdComparison != 0)
				{
					return accountIdComparison;
				}
			}

			var thisStartIsNull = (ValidStart == null);
			var otherStartIsNull = (o?.ValidStart == null);
			if (thisStartIsNull != otherStartIsNull)
			{
				return thisAccountIdIsNull ? -1 : 1;
			}

			if (!thisStartIsNull)
			{
				return ValidStart!.CompareTo(o?.ValidStart);
			}

			return 0;
		}

		/// <summary>
		/// Extract the byte array representation.
		/// </summary>
		/// <returns>                         the byte array representation</returns>
		public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <summary>
		/// Extract the transaction id protobuf.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public Proto.TransactionID ToProtobuf()
		{
			Proto.TransactionID proto = new()
			{
				Scheduled = Scheduled,
				Nonce = Nonce ?? 0
			};

			if (AccountId != null)
				proto.AccountID = AccountId.ToProtobuf();

			if (ValidStart != null)
				proto.TransactionValidStart = Utils.TimestampConverter.ToProtobuf(ValidStart);

			return proto;
		}
		/// <summary>
		/// Convert to a string representation with checksum.
		/// </summary>
		/// <param name="client">the configured client</param>
		/// <returns>                         the string representation with checksum</returns>
		public string ToStringWithChecksum(Client client)
		{
			if (AccountId != null && ValidStart != null)
			{
				return "" + AccountId.ToStringWithChecksum(client) + ToStringPostfix();
			}
			else
			{
				throw new InvalidOperationException("`TransactionId.toStringWithChecksum()` is non-exhaustive");
			}
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
		public override string ToString()
		{
			if (AccountId != null && ValidStart != null)
			{
				return "" + AccountId + ToStringPostfix();
			}
			else
			{
				throw new InvalidOperationException("`TransactionId.toString()` is non-exhaustive");
			}
		}
		public override bool Equals(object? @object)
        {
            if (@object is not TransactionId id)
            {
                return false;
            }

            if (AccountId != null && ValidStart != null && id.AccountId != null && id.ValidStart != null)
            {
                return id.AccountId.Equals(AccountId) && id.ValidStart.Equals(ValidStart) && Scheduled == id.Scheduled;
            }
            else
            {
                return false;
            }
        }        
    }
}