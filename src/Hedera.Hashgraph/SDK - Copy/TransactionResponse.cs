// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Queries;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// When the client sends the node a transaction of any kind, the node replies with this, which simply says that the
    /// transaction passed the pre-check (so the node will submit it to the network) or it failed (so it won't). To learn the
    /// consensus result, the client should later obtain a receipt (free), or can buy a more detailed record (not free).
    /// <br>
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/miscellaneous/transactionresponse">Hedera
    /// Documentation</a>
    /// </summary>
    public sealed class TransactionResponse
    {
        /// <summary>
        /// The maximum number of retry attempts for throttled transactions
        /// </summary>
        private static readonly int MAX_RETRY_ATTEMPTS = 5;
        /// <summary>
        /// The initial backoff delay in milliseconds
        /// </summary>
        private static readonly long INITIAL_BACKOFF_MS = 250;
        /// <summary>
        /// The maximum backoff delay in milliseconds
        /// </summary>
        private static readonly long MAX_BACKOFF_MS = 8000;
        /// <summary>
        /// The node ID
        /// </summary>
        public readonly AccountId nodeId;
        /// <summary>
        /// The transaction hash
        /// </summary>
        public readonly byte[] transactionHash;
        /// <summary>
        /// The transaction ID
        /// </summary>
        public readonly TransactionId transactionId;
        /// <summary>
        /// The scheduled transaction ID
        /// </summary>
        public readonly TransactionId scheduledTransactionId;
        private readonly Transaction transaction;
        private bool validateStatus = true;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nodeId">the node id</param>
        /// <param name="transactionId">the transaction id</param>
        /// <param name="transactionHash">the transaction hash</param>
        /// <param name="scheduledTransactionId">the scheduled transaction id</param>
        TransactionResponse(AccountId nodeId, TransactionId transactionId, byte[] transactionHash, TransactionId scheduledTransactionId, Transaction transaction)
        {
            nodeId = nodeId;
            transactionId = transactionId;
            transactionHash = transactionHash;
            scheduledTransactionId = scheduledTransactionId;
            transaction = transaction;
        }

        /// <summary>
        /// </summary>
        /// <returns>whether getReceipt() or getRecord() will throw an exception if the receipt status is not SUCCESS</returns>
        public bool GetValidateStatus()
        {
            return validateStatus;
        }

        /// <summary>
        /// </summary>
        /// <param name="validateStatus">whether getReceipt() or getRecord() will throw an exception if the receipt status is not
        ///                       SUCCESS</param>
        /// <returns>{@code this}</returns>
        public TransactionResponse SetValidateStatus(bool validateStatus)
        {
            validateStatus = validateStatus;
            return this;
        }

        /// <summary>
        /// Fetch the receipt of the transaction.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>the transaction receipt</returns>
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
        /// <returns>the transaction receipt</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="ReceiptStatusException">when there is an issue with the receipt</exception>
        public TransactionReceipt GetReceipt(Client client, Duration timeout)
        {
            int attempts = 0;
            ReceiptStatusException lastException = null;
            long backoffMs = INITIAL_BACKOFF_MS;
            while (attempts < MAX_RETRY_ATTEMPTS)
            {
                try
                {

                    // Attempt to execute the receipt query
                    return GetReceiptQuery().Execute(client, timeout).ValidateStatus(validateStatus);
                }
                catch (ReceiptStatusException e)
                {

                    // Check if the exception status indicates throttling or inner transaction throttling
                    if (e.Receipt.Status == Status.ThrottledAtConsensus)
                    {
                        lastException = e;
                        attempts++;
                        if (attempts < MAX_RETRY_ATTEMPTS)
                        {
                            try
                            {

                                // Wait with exponential backoff before retrying
                                Thread.Sleep(Math.Min(backoffMs, MAX_BACKOFF_MS));

                                // Double the backoff for next attempt
                                backoffMs *= 2;

                                // Retry the transaction
                                return RetryTransaction(client);
                            }
                            catch (InterruptedException ie)
                            {
                                Thread.CurrentThread().Interrupt();
                                throw new Exception("Retry on throttled status interrupted", ie);
                            }
                            catch (ReceiptStatusException retryException)
                            {

                                // Store the exception and continue with the next attempt
                                lastException = retryException;
                            }
                        }
                    }
                    else
                    {

                        // If not throttled, rethrow the exception immediately
                        throw e;
                    }
                }
            }


            // If we've exhausted all retries, throw the last exception
            throw lastException;
        }

        private TransactionReceipt RetryTransaction(Client client)
        {

            // reset the transaction body
            transaction.frozenBodyBuilder = null;

            // regenerate the transaction id
            transaction.RegenerateTransactionId(client);
            TransactionResponse transactionResponse = (TransactionResponse)transaction.Execute(client);
            return new TransactionReceiptQuery().SetTransactionId(transactionResponse.transactionId).SetNodeAccountIds(List.Of(transactionResponse.nodeId)).Execute(client).ValidateStatus(validateStatus);
        }

        /// <summary>
        /// Create receipt query from the {@link #transactionId} and {@link #transactionHash}
        /// </summary>
        /// <returns>{@link TransactionReceiptQuery}</returns>
        public TransactionReceiptQuery GetReceiptQuery()
        {
            return new TransactionReceiptQuery().SetTransactionId(transactionId).SetNodeAccountIds(Collections.SingletonList(nodeId));
        }

        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>future result of the transaction receipt</returns>
        public Task<TransactionReceipt> GetReceiptAsync(Client client)
        {
            return GetReceiptAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>the transaction receipt</returns>
        public Task<TransactionReceipt> GetReceiptAsync(Client client, Duration timeout)
        {
            return GetReceiptQuery().ExecuteAsync(client, timeout).ThenCompose((receipt) =>
            {
                try
                {
                    return Task.FromResult(receipt.ValidateStatus(validateStatus));
                }
                catch (ReceiptStatusException e)
                {
                    return Task.FailedFuture(e);
                }
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
        /// <returns>the transaction record</returns>
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
        /// <returns>the transaction record</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="ReceiptStatusException">when there is an issue with the receipt</exception>
        public TransactionRecord GetRecord(Client client, Duration timeout)
        {
            GetReceipt(client, timeout);
            return GetRecordQuery().Execute(client, timeout);
        }

        /// <summary>
        /// Create record query from the {@link #transactionId} and {@link #transactionHash}
        /// </summary>
        /// <returns>{@link TransactionRecordQuery}</returns>
        public TransactionRecordQuery GetRecordQuery()
        {
            return new TransactionRecordQuery
            {
				TransactionId = transactionId,
				NodeAccountIds = [nodeId],
			};
        }

        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>future result of the transaction record</returns>
        public Task<TransactionRecord> GetRecordAsync(Client client)
        {
            return GetRecordAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>future result of the transaction record</returns>
        public Task<TransactionRecord> GetRecordAsync(Client client, Duration timeout)
        {
            return GetReceiptAsync(client, timeout).ThenCompose((receipt) => GetRecordQuery().ExecuteAsync(client, timeout));
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
    }
}