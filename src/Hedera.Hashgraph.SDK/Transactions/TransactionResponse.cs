// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="T:TransactionResponse"]/*' />
    public sealed class TransactionResponse
    {
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="F:TransactionResponse.MAX_RETRY_ATTEMPTS"]/*' />
        private static readonly int MAX_RETRY_ATTEMPTS = 5;
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="F:TransactionResponse.INITIAL_BACKOFF_MS"]/*' />
        private static readonly long INITIAL_BACKOFF_MS = 250;
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="F:TransactionResponse.MAX_BACKOFF_MS"]/*' />
        private static readonly long MAX_BACKOFF_MS = 8000;
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="F:TransactionResponse.NodeId"]/*' />
        public readonly AccountId NodeId;
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="F:TransactionResponse.TransactionHash"]/*' />
        public readonly byte[] TransactionHash;
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.#ctor(AccountId,TransactionId,System.Byte[],TransactionId,System.Func{Client,TransactionResponse,TransactionReceipt})"]/*' />
        public readonly TransactionId TransactionId;
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.#ctor(AccountId,TransactionId,System.Byte[],TransactionId,System.Func{Client,TransactionResponse,TransactionReceipt})_2"]/*' />
        public readonly TransactionId? ScheduledTransactionId;

        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.#ctor(AccountId,TransactionId,System.Byte[],TransactionId,System.Func{Client,TransactionResponse,TransactionReceipt})_3"]/*' />
        internal TransactionResponse(AccountId nodeId, TransactionId transactionId, byte[] transactionHash, TransactionId scheduledTransactionId, Func<Client, TransactionResponse, TransactionReceipt> onretry)
        {
            this.NodeId = nodeId;
			this.TransactionId = transactionId;
            this.TransactionHash = transactionHash;
            this.ScheduledTransactionId = scheduledTransactionId;
            this.OnRetry = onretry;

		}

        internal static TransactionResponse Init<T>(AccountId nodeId, TransactionId transactionId, byte[] transactionHash, TransactionId? scheduledTransactionId, Transaction<T> transaction) where T : Transaction<T>
        {
            return new TransactionResponse(nodeId, transactionId, transactionHash, scheduledTransactionId, (client, oldresponse) =>
            {
                // reset the transaction body
                transaction.FrozenBodyBuilder = null;

                // regenerate the transaction id
                transaction.RegenerateTransactionId(client);

                TransactionResponse transactionResponse = transaction.Execute(client);

                return new TransactionReceiptQuery
                {
                    TransactionId = transactionResponse.TransactionId,
                    NodeAccountIds = [transactionResponse.NodeId]

                }.Execute(client).ValidateStatus(oldresponse.ValidateStatus);
            });   
        }

		/// <include file="TransactionResponse.cs.xml" path='docs/member[@name="P:TransactionResponse.ValidateStatus"]/*' />
		public bool ValidateStatus { get; set; } = true;
		public Func<Client, TransactionResponse, TransactionReceipt> OnRetry { get; set; }

		/// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceipt(Client)"]/*' />
		public TransactionReceipt GetReceipt(Client client)
        {
            return GetReceipt(client, client.RequestTimeout);
        }
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceipt(Client,System.TimeSpan)"]/*' />
        public TransactionReceipt GetReceipt(Client client, TimeSpan timeout)
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
                                return OnRetry.Invoke(client, this);
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

			if (lastException is not null)
                throw lastException;

			throw new TransactionException("Could not get receipt");
        }

        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceiptQuery"]/*' />
        public TransactionReceiptQuery GetReceiptQuery()
        {
            return new TransactionReceiptQuery
            {
				TransactionId = TransactionId,
				NodeAccountIds = [NodeId]
			};
        }
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceiptAsync(Client)"]/*' />
        public Task<TransactionReceipt> GetReceiptAsync(Client client)
        {
            return GetReceiptAsync(client, client.RequestTimeout);
        }
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceiptAsync(Client,System.TimeSpan)"]/*' />
        public async Task<TransactionReceipt> GetReceiptAsync(Client client, TimeSpan timeout)
        {
            TransactionReceipt transactionreceipt = await GetReceiptQuery().ExecuteAsync(client, timeout);

            return transactionreceipt.ValidateStatus(ValidateStatus);
        }

        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceiptAsync(Client,System.Action{TransactionReceipt,System.Exception})"]/*' />
        public async void GetReceiptAsync(Client client, Action<TransactionReceipt?, Exception?> callback)
		{
			Utils.ActionHelper.Action(GetReceiptAsync(client), callback);
		}
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceiptAsync(Client,System.TimeSpan,System.Action{TransactionReceipt,System.Exception})"]/*' />
        public async void GetReceiptAsync(Client client, TimeSpan timeout, Action<TransactionReceipt?, Exception?> callback)
        {
			Utils.ActionHelper.Action(GetReceiptAsync(client, timeout), callback);
		}
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceiptAsync(Client,System.Action{TransactionReceipt},System.Action{System.Exception})"]/*' />
        public async void GetReceiptAsync(Client client, Action<TransactionReceipt> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(GetReceiptAsync(client), onSuccess, onFailure);
		}
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetReceiptAsync(Client,System.TimeSpan,System.Action{TransactionReceipt},System.Action{System.Exception})"]/*' />
        public async void GetReceiptAsync(Client client, TimeSpan timeout, Action<TransactionReceipt> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(GetReceiptAsync(client, timeout), onSuccess, onFailure);
		}

        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecord(Client)"]/*' />
        public TransactionRecord GetRecord(Client client)
        {
            return GetRecord(client, client.RequestTimeout);
        }
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecord(Client,System.TimeSpan)"]/*' />
        public TransactionRecord GetRecord(Client client, TimeSpan timeout)
        {
            GetReceipt(client, timeout);

            return GetRecordQuery().Execute(client, timeout);
        }
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecordQuery"]/*' />
        public TransactionRecordQuery GetRecordQuery()
        {
            return new TransactionRecordQuery
            {
				TransactionId = TransactionId,
				NodeAccountIds = [NodeId],
			};
        }
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecordAsync(Client)"]/*' />
        public Task<TransactionRecord> GetRecordAsync(Client client)
        {
            return GetRecordAsync(client, client.RequestTimeout);
        }
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecordAsync(Client,System.TimeSpan)"]/*' />
        public async Task<TransactionRecord> GetRecordAsync(Client client, TimeSpan timeout)
        {
            await GetReceiptAsync(client, timeout);

			return await GetRecordQuery().ExecuteAsync(client, timeout);
		}

        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecordAsync(Client,System.Action{TransactionRecord,System.Exception})"]/*' />
        public async void GetRecordAsync(Client client, Action<TransactionRecord?, Exception?> callback)
        {
			Utils.ActionHelper.Action(GetRecordAsync(client), callback);
		}
		/// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecordAsync(Client,System.TimeSpan,System.Action{TransactionRecord,System.Exception})"]/*' />
		public async void GetRecordAsync(Client client, TimeSpan timeout, Action<TransactionRecord?, Exception?> callback)
        {
			Utils.ActionHelper.Action(GetRecordAsync(client, timeout), callback);
		}
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecordAsync(Client,System.Action{TransactionRecord},System.Action{System.Exception})"]/*' />
        public async void GetRecordAsync(Client client, Action<TransactionRecord> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(GetRecordAsync(client), onSuccess, onFailure);
		}
        /// <include file="TransactionResponse.cs.xml" path='docs/member[@name="M:TransactionResponse.GetRecordAsync(Client,System.TimeSpan,System.Action{TransactionRecord},System.Action{System.Exception})"]/*' />
        public async void GetRecordAsync(Client client, TimeSpan timeout, Action<TransactionRecord> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(GetRecordAsync(client, timeout), onSuccess, onFailure);
		}
    }
}