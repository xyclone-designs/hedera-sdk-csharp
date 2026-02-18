// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Schedule;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// A common base for file and topic message transactions.
    /// </summary>
    public abstract class ChunkedTransaction<T> : Transaction<T> where T : ChunkedTransaction<T>
    {
		/// <summary>
		/// Constructor.
		/// </summary>
		public ChunkedTransaction() : base() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal ChunkedTransaction(Proto.TransactionBody txBody) : base(txBody) { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal ChunkedTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs) { }

		/// <summary>
		/// Assign the Data via a byte string.
		/// </summary>
		/// <param name="Data">the byte string</param>
		/// <returns>{@code this}</returns>
		public virtual ByteString Data 
        {
            get;
            set { RequireNotFrozen(); field = value; }
            
        } = ByteString.Empty;
		/// <summary>
		/// Assign the Data via a byte array.
		/// </summary>
		/// <param name="Data">the byte array</param>
		/// <returns>{@code this}</returns>
		public virtual byte[] Data_Bytes
		{
			set
			{
				RequireNotFrozen();
				Data = ByteString.CopyFrom(value);
			}
		}
		/// <summary>
		/// Assign the Data via a string.
		/// </summary>
		/// <param name="text">the byte array</param>
		/// <returns>{@code this}</returns>
		public virtual string Data_String 
        {
            set
            {
				RequireNotFrozen();
				Data = ByteString.CopyFromUtf8(value);
			} 
        }
		/// <summary>
		/// Maximum number of chunks this message will get broken up into when
		/// it's frozen.
		/// </summary>
		public virtual int MaxChunks
        {
            get;
            set
            {
                RequireNotFrozen();
                field = value;
            }

        } = 20;
		/// <summary>
		/// Assign the chunk size.
		/// </summary>
		/// <param name="ChunkSize">the chunk size</param>
		/// <returns>{@code this}</returns>
		public virtual int ChunkSize
		{
            get;
            set
            {
                RequireNotFrozen();
                field = value;
            }

        } = 1024;

		private void FreezeAndSign(Client client)
		{
			if (!IsFrozen())
			{
				FreezeWith(client);
			}

			var operatorId = client.OperatorAccountId;
			if (operatorId != null && operatorId.Equals(TransactionIdInternal.AccountId))
			{

				// on execute, sign each transaction with the operator, if present
				// and we are signing a transaction that used the default transaction ID
				SignWithOperator(client);
			}
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

			TransactionIds.SetLocked(true);
			NodeAccountIds.SetLocked(true);
			BuildAllTransactions();
			var txCount = TransactionIds.Count;
			var nodeCount = NodeAccountIds.Count;
			var transactionHashes = new List<IDictionary<AccountId, byte[]>>(txCount);
			for (var txIndex = 0; txIndex < txCount; ++txIndex)
			{
				var hashes = new Dictionary<AccountId, byte[]>();
				var offset = txIndex * nodeCount;
				for (var nodeIndex = 0; nodeIndex < nodeCount; ++nodeIndex)
				{
					hashes.Add(NodeAccountIds[nodeIndex], GenerateHash(OuterTransactions[offset + nodeIndex].SignedTransactionBytes.ToByteArray()));
				}

				transactionHashes.Add(hashes);
			}

			return transactionHashes;
		}
		
		public override IDictionary<AccountId, byte[]> GetTransactionHashPerNode()
        {
            if (OuterTransactions.Count > NodeAccountIds.Count)
            {
                throw new InvalidOperationException("a single transaction hash can not be calculated for a chunked transaction, try calling `getAllTransactionHashesPerNode`");
            }

            return base.GetTransactionHashPerNode();
        }
        public override IDictionary<AccountId, IDictionary<PublicKey, byte[]>> GetSignatures()
        {
            if (Data.Length > ChunkSize)
            {
                throw new InvalidOperationException("Cannot call getSignatures() on a chunked transaction with length greater than " + ChunkSize);
            }

            return base.GetSignatures();
        }

		public override int GetRequiredChunks()
		{
			if (Data.Length == 0)
			{
				throw new ArgumentException("message cannot be empty");
			}

			var requiredChunks = (Data.Length + (ChunkSize - 1)) / ChunkSize;
			if (requiredChunks == 0)
			{
				requiredChunks = 1;
			}

			if (requiredChunks > MaxChunks)
			{
				throw new ArgumentException("message of " + Data.Length + " bytes requires " + requiredChunks + " chunks but the maximum allowed chunks is " + MaxChunks + ", try using setMaxChunks");
			}

			return requiredChunks;
		}
		public override byte[] GetTransactionHash()
		{
			if (OuterTransactions.Count > NodeAccountIds.Count)
			{
				throw new InvalidOperationException("a single transaction hash can not be calculated for a chunked transaction, try calling `getAllTransactionHashesPerNode`");
			}

			return base.GetTransactionHash();
		}
		public override void WipeTransactionLists(int requiredChunks)
		{
			SigPairLists = new List<Proto.SignatureMap>(requiredChunks * NodeAccountIds.Count);
			OuterTransactions = new List<Proto.Transaction>(requiredChunks * NodeAccountIds.Count);
			InnerSignedTransactions = new List<Proto.SignedTransaction>(requiredChunks * NodeAccountIds.Count);

			for (int i = 0; i < requiredChunks; i++)
			{
				if (TransactionIds.Count != 0)
				{
					var startIndex = i * ChunkSize;
					var endIndex = startIndex + ChunkSize;
					if (endIndex > Data.Length)
					{
						endIndex = Data.Length;
					}

					FrozenBodyBuilder!.TransactionID = TransactionIds[i].ToProtobuf();

					OnFreezeChunk(FrozenBodyBuilder, TransactionIds[0].ToProtobuf(), startIndex, endIndex, i, requiredChunks);
				}

				// For each node we add a transaction with that node
				foreach (var nodeId in NodeAccountIds)
				{
					SigPairLists.Add(new Proto.SignatureMap());
					FrozenBodyBuilder!.NodeAccountID = nodeId.ToProtobuf();
					FrozenBodyBuilder!.ToByteString();
					OuterTransactions.Add(null);
					InnerSignedTransactions.Add(new Proto.SignedTransaction
					{
						BodyBytes = FrozenBodyBuilder.ToByteString()
					});
				}
			}
		}
		public override T AddSignature(PublicKey publicKey, byte[] signature)
		{
			if (Data.Length > ChunkSize)
			{
				throw new InvalidOperationException("Cannot manually add signature to chunked transaction with length greater than " + ChunkSize);
			}

			return base.AddSignature(publicKey, signature);
		}

		public override ScheduleCreateTransaction Schedule()
		{
			RequireNotFrozen();

			if (NodeAccountIds.Count != 0)
			{
				throw new InvalidOperationException("The underlying transaction for a scheduled transaction cannot have node account IDs set");
			}

			if (Data.Length > ChunkSize)
			{
				throw new InvalidOperationException("Cannot schedule a chunked transaction with length greater than " + ChunkSize);
			}

			var bodyBuilder = SpawnBodyBuilder(null);
			OnFreeze(bodyBuilder);
			OnFreezeChunk(bodyBuilder, null, 0, Data.Length, 1, 1);
			return DoSchedule(bodyBuilder);
		}
		public override TransactionResponse Execute(Client client, Duration timeoutPerChunk)
        {
            return ExecuteAll(client, timeoutPerChunk)[0];
        }
		public override async Task<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerChunk)
		{
			IList<TransactionResponse> responses = await ExecuteAllAsync(client, timeoutPerChunk);

			return responses[0];
		}

		/// <summary>
		/// Get the body sizes for all chunks in a FileAppendTransaction.
		/// For transactions with multiple chunks (like large file appends),
		/// this returns an array containing the size of each chunk's transaction body.
		/// The size is calculated by encoding the transaction body to protobuf format.
		/// </summary>
		/// <returns>List of integers that represent the size of each chunk</returns>
		public virtual List<int> BodySizeAllChunks()
		{
			IList<int> list = [];
			int originalIndex = TransactionIds.Index;
			try
			{
				// Calculate size for each chunk
				for (int i = 0; i < GetRequiredChunks(); i++)
				{
					TransactionIds.SetIndex(i);
					list.Add(GetTransactionBodySize());
				}
			}
			finally
			{
				TransactionIds.SetIndex(originalIndex);
			}

			return list;
		}
		/// <summary>
		/// Extract the list of all signers.
		/// </summary>
		/// <returns>                         the list of all signatures</returns>
		public virtual List<IDictionary<AccountId, IDictionary<PublicKey, byte[]>>> GetAllSignatures()
		{
			if (PublicKeys.Any() is false)
			{
				return [];
			}

			BuildAllTransactions();
			var txCount = TransactionIds.Count;
			var nodeCount = NodeAccountIds.Count;
			var retval = new List<IDictionary<AccountId, IDictionary<PublicKey, byte[]>>>(txCount);
			for (int i = 0; i < txCount; i++)
			{
				retval.Add(GetSignaturesAtOffset(i * nodeCount));
			}

			return retval;
		}
		/// <summary>
		/// Should the receipt be retrieved?
		/// </summary>
		/// <returns>                         by default do not get a receipt</returns>
		public virtual bool ShouldGetReceipt()
		{
			return false;
		}

		/// <summary>
		/// Execute this transaction or query
		/// </summary>
		/// <param name="client">The client with which this will be executed.</param>
		/// <returns>Result of execution for each chunk</returns>
		/// <exception cref="TimeoutException">when the transaction times out</exception>
		/// <exception cref="PrecheckStatusException">when the precheck fails</exception>
		public virtual List<TransactionResponse> ExecuteAll(Client client)
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
        public virtual List<TransactionResponse> ExecuteAll(Client client, Duration timeoutPerChunk)
        {
            FreezeAndSign(client);
            var responses = new List<TransactionResponse>(TransactionIds.Count);
            for (var i = 0; i < TransactionIds.Count; i++)
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
        public virtual async Task<IList<TransactionResponse>> ExecuteAllAsync(Client client, Duration timeoutPerChunk)
        {
            FreezeAndSign(client);

			List<TransactionResponse> list = new (TransactionIds.Count);

			for (var i = 0; i < TransactionIds.Count; i++)
			{
				TransactionResponse response = await base.ExecuteAsync(client, timeoutPerChunk).ConfigureAwait(false);

				if (ShouldGetReceipt())
					await response.GetReceiptAsync(client, timeoutPerChunk).ConfigureAwait(false);

				list.Add(response);
			}

			return list;
        }
        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual async void ExecuteAllAsync(Client client, Action<IList<TransactionResponse>?, Exception?> callback)
        {
			Utils.ActionHelper.Action(ExecuteAllAsync(client), callback);
		}
        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual async void ExecuteAllAsync(Client client, Duration timeout, Action<IList<TransactionResponse>?, Exception?> callback)
        {
			Utils.ActionHelper.Action(ExecuteAllAsync(client, timeout), callback);
		}
		/// <summary>
		/// Execute this transaction or query asynchronously.
		/// </summary>
		/// <param name="client">The client with which this will be executed.</param>
		/// <param name="onSuccess">a Action which consumes the result on success.</param>
		/// <param name="onFailure">a Action which consumes the error on failure.</param>
		public virtual async void ExecuteAllAsync(Client client, Action<IList<TransactionResponse>> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(ExecuteAllAsync(client), onSuccess, onFailure);
		}
        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual async void ExecuteAllAsync(Client client, Duration timeout, Action<IList<TransactionResponse>> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(ExecuteAllAsync(client, timeout), onSuccess, onFailure);
		}

        /// <summary>
        /// A common base for file and topic message transactions.
        /// </summary>
        public abstract void OnFreezeChunk(Proto.TransactionBody body, Proto.TransactionID? initialTransactionId, int startIndex, int endIndex, int chunk, int total);       
    }
}