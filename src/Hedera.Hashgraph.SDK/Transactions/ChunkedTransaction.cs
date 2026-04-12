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
    /// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="T:ChunkedTransaction"]/*' />
    public abstract class ChunkedTransaction<T> : Transaction<T> where T : ChunkedTransaction<T>
    {
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.#ctor"]/*' />
		public ChunkedTransaction() : base() { }
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal ChunkedTransaction(Proto.Services.TransactionBody txBody) : base(txBody) { }
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal ChunkedTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs) { }

		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.RequireNotFrozen"]/*' />
		public virtual ByteString Data 
        {
            get;
            set { RequireNotFrozen(); field = value; }
            
        } = ByteString.Empty;
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.RequireNotFrozen_2"]/*' />
		public virtual byte[] Data_Bytes
		{
			set
			{
				RequireNotFrozen();
				Data = ByteString.CopyFrom(value);
			}
		}
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.RequireNotFrozen_3"]/*' />
		public virtual string Data_String 
        {
            set
            {
				RequireNotFrozen();
				Data = ByteString.CopyFromUtf8(value);
			} 
        }
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.RequireNotFrozen_4"]/*' />
		public virtual int MaxChunks
        {
            get;
            set
            {
                RequireNotFrozen();
                field = value;
            }

        } = 20;
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.RequireNotFrozen_5"]/*' />
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

		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.GetAllTransactionHashesPerNode"]/*' />
		public List<IDictionary<AccountId, byte[]>> GetAllTransactionHashesPerNode()
		{
			if (!IsFrozen())
			{
				throw new InvalidOperationException("transaction must have been frozen before calculating the hash will be stable, try calling `freeze`");
			}

			TransactionIds.IsLocked = true;
			NodeAccountIds.IsLocked = true;
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
					hashes.Add(NodeAccountIds[nodeIndex], Transaction.GenerateHash(OuterTransactions[offset + nodeIndex].SignedTransactionBytes.ToByteArray()));
				}

				transactionHashes.Add(hashes);
			}

			return transactionHashes;
		}
		
		public override Dictionary<AccountId, byte[]> GetTransactionHashPerNode()
        {
            if (OuterTransactions.Count > NodeAccountIds.Count)
            {
                throw new InvalidOperationException("a single transaction hash can not be calculated for a chunked transaction, try calling `getAllTransactionHashesPerNode`");
            }

            return base.GetTransactionHashPerNode();
        }
        public override Dictionary<AccountId, Dictionary<PublicKey, byte[]>> GetSignatures()
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
			SigPairLists = new List<Proto.Services.SignatureMap>(requiredChunks * NodeAccountIds.Count);
			OuterTransactions = new List<Proto.Services.Transaction>(requiredChunks * NodeAccountIds.Count);
			InnerSignedTransactions = new List<Proto.Services.SignedTransaction>(requiredChunks * NodeAccountIds.Count);

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
					SigPairLists.Add(new Proto.Services.SignatureMap());
					FrozenBodyBuilder!.NodeAccountID = nodeId.ToProtobuf();
					FrozenBodyBuilder!.ToByteString();
					OuterTransactions.Add(null);
					InnerSignedTransactions.Add(new Proto.Services.SignedTransaction
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

		public override ScheduleCreateTransaction Schedule(Action<ScheduleCreateTransaction>? oncreate = null)
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
		public override TransactionResponse Execute(Client client, TimeSpan timeoutPerChunk)
        {
            return ExecuteAll(client, timeoutPerChunk)[0];
        }
		public override async Task<TransactionResponse> ExecuteAsync(Client client, TimeSpan timeoutPerChunk)
		{
			IList<TransactionResponse> responses = await ExecuteAllAsync(client, timeoutPerChunk);

			return responses[0];
		}

		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.BodySizeAllChunks"]/*' />
		public virtual List<int> BodySizeAllChunks()
		{
			List<int> list = [];
			int originalIndex = TransactionIds.Index;
			try
			{
				// Calculate size for each chunk
				for (int i = 0; i < GetRequiredChunks(); i++)
				{
					TransactionIds.Index = i;
					list.Add(GetTransactionBodySize());
				}
			}
			finally
			{
				TransactionIds.Index = originalIndex;
			}

			return list;
		}
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.GetAllSignatures"]/*' />
		public virtual List<Dictionary<AccountId, Dictionary<PublicKey, byte[]>>> GetAllSignatures()
		{
			if (PublicKeys.Any() is false)
			{
				return [];
			}

			BuildAllTransactions();
			var txCount = TransactionIds.Count;
			var nodeCount = NodeAccountIds.Count;
			var retval = new List<Dictionary<AccountId, Dictionary<PublicKey, byte[]>>>(txCount);
			for (int i = 0; i < txCount; i++)
			{
				retval.Add(GetSignaturesAtOffset(i * nodeCount));
			}

			return retval;
		}
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ShouldGetReceipt"]/*' />
		public virtual bool ShouldGetReceipt()
		{
			return false;
		}

		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ExecuteAll(Client)"]/*' />
		public virtual List<TransactionResponse> ExecuteAll(Client client)
        {
            return ExecuteAll(client, client.RequestTimeout);
        }
        /// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ExecuteAll(Client,System.TimeSpan)"]/*' />
        public virtual List<TransactionResponse> ExecuteAll(Client client, TimeSpan timeoutPerChunk)
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
        /// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ExecuteAllAsync(Client)"]/*' />
        public virtual Task<IList<TransactionResponse>> ExecuteAllAsync(Client client)
        {
            return ExecuteAllAsync(client, client.RequestTimeout);
        }
        /// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ExecuteAllAsync(Client,System.TimeSpan)"]/*' />
        public virtual async Task<IList<TransactionResponse>> ExecuteAllAsync(Client client, TimeSpan timeoutPerChunk)
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
        /// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ExecuteAllAsync(Client,System.Action{System.Collections.Generic.IList{TransactionResponse},System.Exception})"]/*' />
        public virtual async void ExecuteAllAsync(Client client, Action<IList<TransactionResponse>?, Exception?> callback)
        {
			Utils.ActionHelper.Action(ExecuteAllAsync(client), callback);
		}
        /// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ExecuteAllAsync(Client,System.TimeSpan,System.Action{System.Collections.Generic.IList{TransactionResponse},System.Exception})"]/*' />
        public virtual async void ExecuteAllAsync(Client client, TimeSpan timeout, Action<IList<TransactionResponse>?, Exception?> callback)
        {
			Utils.ActionHelper.Action(ExecuteAllAsync(client, timeout), callback);
		}
		/// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ExecuteAllAsync(Client,System.Action{System.Collections.Generic.IList{TransactionResponse}},System.Action{System.Exception})"]/*' />
		public virtual async void ExecuteAllAsync(Client client, Action<IList<TransactionResponse>> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(ExecuteAllAsync(client), onSuccess, onFailure);
		}
        /// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.ExecuteAllAsync(Client,System.TimeSpan,System.Action{System.Collections.Generic.IList{TransactionResponse}},System.Action{System.Exception})"]/*' />
        public virtual async void ExecuteAllAsync(Client client, TimeSpan timeout, Action<IList<TransactionResponse>> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(ExecuteAllAsync(client, timeout), onSuccess, onFailure);
		}

        /// <include file="ChunkedTransaction.cs.xml" path='docs/member[@name="M:ChunkedTransaction.OnFreezeChunk(Proto.Services.TransactionBody,Proto.Services.TransactionID,System.Int32,System.Int32,System.Int32,System.Int32)"]/*' />
        public abstract void OnFreezeChunk(Proto.Services.TransactionBody body, Proto.Services.TransactionID? initialTransactionId, int startIndex, int endIndex, int chunk, int total);       
    }
}
