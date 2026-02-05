// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// When the client sends the node a transaction of any kind, the node replies with this, which simply says that the
    /// transaction passed the pre-check (so the node will submit it to the network) or it failed (so it won't). To learn the
    /// consensus result, the client should later obtain a receipt (free), or can buy a more detailed record (not free).
    /// <br>
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/miscellaneous/transactionresponse">Hedera
    /// Documentation</a>
    /// </summary>
    public sealed class TransactionResponse<T> where T : Transaction<T>
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
        public readonly AccountId NodeId;
        /// <summary>
        /// The transaction hash
        /// </summary>
        public readonly byte[] TransactionHash;
        /// <summary>
        /// The transaction ID
        /// </summary>
        public readonly TransactionId TransactionId;
        /// <summary>
        /// The scheduled transaction ID
        /// </summary>
        public readonly TransactionId ScheduledTransactionId;
        private readonly Transaction<T> Transaction;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nodeId">the node id</param>
        /// <param name="transactionId">the transaction id</param>
        /// <param name="transactionHash">the transaction hash</param>
        /// <param name="scheduledTransactionId">the scheduled transaction id</param>
        TransactionResponse(AccountId nodeId, TransactionId transactionId, byte[] transactionHash, TransactionId scheduledTransactionId, Transaction<T> transaction)
        {
            this.NodeId = nodeId;
			this.Transaction = transaction;
			this.TransactionId = transactionId;
            this.TransactionHash = transactionHash;
            this.ScheduledTransactionId = scheduledTransactionId;
        }

		private TransactionReceipt RetryTransaction(Client client)
		{
			// reset the transaction body
			Transaction.FrozenBodyBuilder = null;

			// regenerate the transaction id
			Transaction.RegenerateTransactionId(client);

			TransactionResponse<T> transactionResponse = (TransactionResponse<T>)Transaction.Execute(client);

			return new TransactionReceiptQuery
			{
				TransactionId = transactionResponse.TransactionId,
				NodeAccountIds = [transactionResponse.NodeId]

			}.Execute(client).ValidateStatus(ValidateStatus);
		}

		/// <summary>
		/// </summary>
		/// <returns>whether getReceipt() or getRecord() will throw an exception if the receipt status is not SUCCESS</returns>
		public bool ValidateStatus { get; set; } = true;

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
            return GetReceipt(client, client.RequestTimeout);
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
            ReceiptStatusException? lastException = null;
            long backoffMs = INITIAL_BACKOFF_MS;
            while (attempts < MAX_RETRY_ATTEMPTS)
            {
                try
                {
                    // Attempt to execute the receipt query
                    return GetReceiptQuery().Execute(client, timeout).ValidateStatus(ValidateStatus);
                }
                catch (ReceiptStatusException e)
                {
                    // Check if the exception status indicates throttling or inner transaction throttling
                    if (e.Receipt.Status == ResponseStatus.ThrottledAtConsensus)
                    {
                        lastException = e;
                        attempts++;
                        if (attempts < MAX_RETRY_ATTEMPTS)
                        {
                            try
                            {

                                // Wait with exponential backoff before retrying
                                Thread.Sleep((int)Math.Min(backoffMs, MAX_BACKOFF_MS));

                                // Double the backoff for next attempt
                                backoffMs *= 2;

                                // Retry the transaction
                                return RetryTransaction(client);
                            }
                            catch (ThreadInterruptedException ie)
                            {
                                Thread.CurrentThread.Interrupt();
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
                        throw;
                    }
                }
            }


            // If we've exhausted all retries, throw the last exception
            if (lastException is not null) throw lastException;
        }

        /// <summary>
        /// Create receipt query from the {@link #transactionId} and {@link #transactionHash}
        /// </summary>
        /// <returns>{@link TransactionReceiptQuery}</returns>
        public TransactionReceiptQuery GetReceiptQuery()
        {
            return new TransactionReceiptQuery
            {
				TransactionId = TransactionId,
				NodeAccountIds = [NodeId]
			};
        }
        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>future result of the transaction receipt</returns>
        public Task<TransactionReceipt> GetReceiptAsync(Client client)
        {
            return GetReceiptAsync(client, client.RequestTimeout);
        }
        /// <summary>
        /// Fetch the receipt of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>the transaction receipt</returns>
        public async Task<TransactionReceipt> GetReceiptAsync(Client client, Duration timeout)
        {
            TransactionReceipt transactionreceipt = await GetReceiptQuery().ExecuteAsync(client, timeout);

            return transactionreceipt.ValidateStatus(ValidateStatus);
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
            return GetRecord(client, client.RequestTimeout);
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
				TransactionId = TransactionId,
				NodeAccountIds = [NodeId],
			};
        }
        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>future result of the transaction record</returns>
        public Task<TransactionRecord> GetRecordAsync(Client client)
        {
            return GetRecordAsync(client, client.RequestTimeout);
        }
        /// <summary>
        /// Fetch the record of the transaction asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>future result of the transaction record</returns>
        public async Task<TransactionRecord> GetRecordAsync(Client client, Duration timeout)
        {
            await GetReceiptAsync(client, timeout);

			return await GetRecordQuery().ExecuteAsync(client, timeout);
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