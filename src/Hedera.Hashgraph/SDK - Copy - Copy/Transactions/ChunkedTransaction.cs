// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions.Schedule;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// A common base for file and topic message transactions.
    /// </summary>
    abstract class ChunkedTransaction<T> : Transaction<T> where T : ChunkedTransaction<T>
    {
        private int chunkSize = 1024;
        /// <summary>
        /// The transaction data
        /// </summary>
        protected ByteString data = ByteString.Empty;
        /// <summary>
        /// Maximum number of chunks this message will get broken up into when
        /// it's frozen.
        /// </summary>
        private int maxChunks = 20;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        ChunkedTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        ChunkedTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        ChunkedTransaction() : base()
        {
        }

        /// <summary>
        /// Extract the data.
        /// </summary>
        /// <returns>                         the data</returns>
        public virtual ByteString GetData()
        {
            return data;
        }

        /// <summary>
        /// Assign the data via a byte array.
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <returns>{@code this}</returns>
        public virtual T SetData(byte[] data)
        {
            RequireNotFrozen();
            data = ByteString.CopyFrom(data).ToByteArray();

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Assign the data via a byte string.
        /// </summary>
        /// <param name="data">the byte string</param>
        /// <returns>{@code this}</returns>
        public virtual T SetData(ByteString data)
        {
            RequireNotFrozen();
            data = data;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Assign the data via a string.
        /// </summary>
        /// <param name="text">the byte array</param>
        /// <returns>{@code this}</returns>
        public virtual T SetData(string text)
        {
            RequireNotFrozen();
            data = ByteString.CopyFromUtf8(text);

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Retrieve the maximum number of chunks.
        /// </summary>
        /// <returns>                         the number of chunks</returns>
        public virtual int GetMaxChunks()
        {
            return maxChunks;
        }

        /// <summary>
        /// Assign the max number of chunks.
        /// </summary>
        /// <param name="maxChunks">the number of chunks</param>
        /// <returns>{@code this}</returns>
        public virtual T SetMaxChunks(int maxChunks)
        {
            RequireNotFrozen();
            maxChunks = maxChunks;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Retrieve the chunk size.
        /// </summary>
        /// <returns>                         the chunk size</returns>
        public virtual int GetChunkSize()
        {
            return chunkSize;
        }

        /// <summary>
        /// Assign the chunk size.
        /// </summary>
        /// <param name="chunkSize">the chunk size</param>
        /// <returns>{@code this}</returns>
        public virtual T SetChunkSize(int chunkSize)
        {
            RequireNotFrozen();
            chunkSize = chunkSize;

            // noinspection unchecked
            return (T)this;
        }

        public override byte[] GetTransactionHash()
        {
            if (outerTransactions.Count > nodeAccountIds.Count)
            {
                throw new InvalidOperationException("a single transaction hash can not be calculated for a chunked transaction, try calling `getAllTransactionHashesPerNode`");
            }

            return base.GetTransactionHash();
        }

        public override IDictionary<AccountId, byte[]> GetTransactionHashPerNode()
        {
            if (outerTransactions.Count > nodeAccountIds.Count)
            {
                throw new InvalidOperationException("a single transaction hash can not be calculated for a chunked transaction, try calling `getAllTransactionHashesPerNode`");
            }

            return base.GetTransactionHashPerNode();
        }

        /// <summary>
        /// Extract the list of transaction hashes.
        /// </summary>
        /// <returns>                         the list of transaction hashes</returns>
        public List<IDictionary<AccountId, byte[]>> GetAllTransactionHashesPerNode()
        {
            if (!IsFrozen())
            {
                throw new InvalidOperationException("transaction must have been frozen before calculating the hash will be stable, try calling `freeze`");
            }

            transactionIds.SetLocked(true);
            nodeAccountIds.SetLocked(true);
            BuildAllTransactions();
            var txCount = transactionIds.Count;
            var nodeCount = nodeAccountIds.Count;
            var transactionHashes = new List<IDictionary<AccountId, byte[]>>(txCount);
            for (var txIndex = 0; txIndex < txCount; ++txIndex)
            {
                var hashes = new Dictionary<AccountId, byte[]>();
                var offset = txIndex * nodeCount;
                for (var nodeIndex = 0; nodeIndex < nodeCount; ++nodeIndex)
                {
                    hashes.Add(nodeAccountIds[nodeIndex], Hash(outerTransactions[offset + nodeIndex].SignedTransactionBytes.ToByteArray()));
                }

                transactionHashes.Add(hashes);
            }

            return transactionHashes;
        }

        public override T AddSignature(PublicKey publicKey, byte[] signature)
        {
            if (data.Length > chunkSize)
            {
                throw new InvalidOperationException("Cannot manually add signature to chunked transaction with length greater than " + chunkSize);
            }

            return base.AddSignature(publicKey, signature);
        }

        public override IDictionary<AccountId, IDictionary<PublicKey, byte[]>> GetSignatures()
        {
            if (data.Length > chunkSize)
            {
                throw new InvalidOperationException("Cannot call getSignatures() on a chunked transaction with length greater than " + chunkSize);
            }

            return base.GetSignatures();
        }

        /// <summary>
        /// Extract the list of all signers.
        /// </summary>
        /// <returns>                         the list of all signatures</returns>
        public virtual List<IDictionary<AccountId, IDictionary<PublicKey, byte[]>>> GetAllSignatures()
        {
            if (publicKeys.Any() is false)
            {
                return [];
            }

            BuildAllTransactions();
            var txCount = transactionIds.Count;
            var nodeCount = nodeAccountIds.Count;
            var retval = new List<IDictionary<AccountId, IDictionary<PublicKey, byte[]>>>(txCount);
            for (int i = 0; i < txCount; i++)
            {
                retval.Add(GetSignaturesAtOffset(i * nodeCount));
            }

            return retval;
        }

        private void FreezeAndSign(Client client)
        {
            if (!IsFrozen())
            {
                FreezeWith(client);
            }

            var operatorId = client.GetOperatorAccountId();
            if (operatorId != null && operatorId.Equals(TransactionIdInternal().AccountId))
            {

                // on execute, sign each transaction with the operator, if present
                // and we are signing a transaction that used the default transaction ID
                SignWithOperator(client);
            }
        }

        public override TransactionResponse Execute(Client client, Duration timeoutPerChunk)
        {
            return ExecuteAll(client, timeoutPerChunk)[0];
        }

        /// <summary>
        /// Execute this transaction or query
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>Result of execution for each chunk</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public virtual IList<TransactionResponse> ExecuteAll(Client client)
        {
            return ExecuteAll(client, client.RequestTimeout);
        }

        /// <summary>
        /// Execute this transaction or query
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeoutPerChunk">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>Result of execution for each chunk</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public virtual IList<TransactionResponse> ExecuteAll(Client client, Duration timeoutPerChunk)
        {
            FreezeAndSign(client);
            var responses = new List<TransactionResponse>(transactionIds.Count);
            for (var i = 0; i < transactionIds.Count; i++)
            {
                var response = base.Execute(client, timeoutPerChunk);
                if (ShouldGetReceipt())
                {
                    new TransactionReceiptQuery
                    {
						NodeAccountIds = [response.NodeId],
						TransactionId = response.TransactionId,

					}.Execute(client, timeoutPerChunk);
                }

                responses.Add(response);
            }

            return responses;
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>Future result of execution for each chunk</returns>
        public virtual Task<IList<TransactionResponse>> ExecuteAllAsync(Client client)
        {
            return ExecuteAllAsync(client, client.RequestTimeout);
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeoutPerChunk">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>Future result of execution for each chunk</returns>
        public virtual Task<IList<TransactionResponse>> ExecuteAllAsync(Client client, Duration timeoutPerChunk)
        {
            FreezeAndSign(client);
            Task<List<TransactionResponse>> future = Task.Run(() => new List<TransactionResponse>(transactionIds.Count));
            for (var i = 0; i < transactionIds.Count; i++)
            {
                future = future.ThenCompose((list) =>
                {
                    var responseFuture = base.ExecuteAsync(client, timeoutPerChunk);
                    Function<TransactionResponse, TWildcardTodoCompletionStage<TransactionResponse>> receiptFuture = (TransactionResponse response) => response.ReceiptAsync(client, timeoutPerChunk).ThenApply((receipt) => response);
                    Function<TransactionResponse, IList<TransactionResponse>> addToList = (response) =>
                    {
                        list.Add(response);
                        return list;
                    };
                    if (ShouldGetReceipt())
                    {
                        return responseFuture.ThenCompose(receiptFuture).ThenApply(addToList);
                    }
                    else
                    {
                        return responseFuture.ThenApply(addToList);
                    }
                });
            }

            return future;
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAllAsync(Client client, Action<IList<TransactionResponse>, Exception> callback)
        {
            ActionHelper.Action(ExecuteAllAsync(client), callback);
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAllAsync(Client client, Duration timeout, Action<IList<TransactionResponse>, Exception> callback)
        {
            ActionHelper.Action(ExecuteAllAsync(client, timeout), callback);
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAllAsync(Client client, Action<IList<TransactionResponse>> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAllAsync(client), onSuccess, onFailure);
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAllAsync(Client client, Duration timeout, Action<IList<TransactionResponse>> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAllAsync(client, timeout), onSuccess, onFailure);
        }

        public override Task<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerChunk)
        {
            return ExecuteAllAsync(client, timeoutPerChunk).ThenApply((responses) => responses[0]);
        }

        public override ScheduleCreateTransaction Schedule()
        {
            RequireNotFrozen();
            if (!nodeAccountIds.Length == 0)
            {
                throw new InvalidOperationException("The underlying transaction for a scheduled transaction cannot have node account IDs set");
            }

            if (data.Length > chunkSize)
            {
                throw new InvalidOperationException("Cannot schedule a chunked transaction with length greater than " + chunkSize);
            }

            var bodyBuilder = SpawnBodyBuilder(null);
            OnFreeze(bodyBuilder);
            OnFreezeChunk(bodyBuilder, null, 0, data.Length, 1, 1);
            return DoSchedule(bodyBuilder);
        }

        override int GetRequiredChunks()
        {
            if (data.Length == 0)
            {
                throw new ArgumentException("message cannot be empty");
            }

            var requiredChunks = (data.Length + (chunkSize - 1)) / chunkSize;
            if (requiredChunks == 0)
            {
                requiredChunks = 1;
            }

            if (requiredChunks > maxChunks)
            {
                throw new ArgumentException("message of " + data.Length + " bytes requires " + requiredChunks + " chunks but the maximum allowed chunks is " + maxChunks + ", try using setMaxChunks");
            }

            return requiredChunks;
        }

        override void WipeTransactionLists(int requiredChunks)
        {
            sigPairLists = new List<Proto.SignatureMap>(requiredChunks * nodeAccountIds.Count);
            outerTransactions = new List<Proto.Transaction>(requiredChunks * nodeAccountIds.Count);
            innerSignedTransactions = new List<Proto.SignedTransaction>(requiredChunks * nodeAccountIds.Count);
            for (int i = 0; i < requiredChunks; i++)
            {
                if (!transactionIds.Length == 0)
                {
                    var startIndex = i * chunkSize;
                    var endIndex = startIndex + chunkSize;
                    if (endIndex > data.Length)
                    {
                        endIndex = data.Length;
                    }

                    OnFreezeChunk(ArgumentNullException.ThrowIfNull(FrozenBodyBuilder).SetTransactionID(transactionIds[i].ToProtobuf()), transactionIds[0].ToProtobuf(), startIndex, endIndex, i, requiredChunks);
                }


                // For each node we add a transaction with that node
                foreach (var nodeId in nodeAccountIds)
                {
                    sigPairLists.Add(new Proto.SignatureMap());
                    FrozenBodyBuilder.NodeAccountID = nodeId.ToProtobuf();
                    FrozenBodyBuilder.ToByteString();
					outerTransactions.Add(null);
					innerSignedTransactions.Add(new Proto.SignedTransaction 
                    {
						BodyBytes = FrozenBodyBuilder.ToByteString()
					});
                }
            }
        }

        /// <summary>
        /// A common base for file and topic message transactions.
        /// </summary>
        public abstract void OnFreezeChunk(Proto.TransactionBody body, Proto.TransactionID initialTransactionId, int startIndex, int endIndex, int chunk, int total);
        /// <summary>
        /// Should the receipt be retrieved?
        /// </summary>
        /// <returns>                         by default do not get a receipt</returns>
        public virtual bool ShouldGetReceipt()
        {
            return false;
        }

        /// <summary>
        /// Get the body sizes for all chunks in a FileAppendTransaction.
        /// For transactions with multiple chunks (like large file appends),
        /// this returns an array containing the size of each chunk's transaction body.
        /// The size is calculated by encoding the transaction body to protobuf format.
        /// </summary>
        /// <returns>List of integers that represent the size of each chunk</returns>
        public virtual IList<int> BodySizeAllChunks()
        {
            IList<int> list = [];
            int originalIndex = transactionIds.Index;
            try
            {
                // Calculate size for each chunk
                for (int i = 0; i < GetRequiredChunks(); i++)
                {
                    transactionIds.SetIndex(i);
                    list.Add(GetTransactionBodySize());
                }
            }
            finally
            {
                transactionIds.SetIndex(originalIndex);
            }

            return list;
        }
    }
}