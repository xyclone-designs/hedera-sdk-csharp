// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.LiveHashes;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Systems;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Topic;

using Org.BouncyCastle.Crypto.Digests;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Transactions
{

	
	/// <include file="Transaction.cs.xml" path='docs/member[@name="T:Transaction"]/*' />
	public abstract partial class Transaction<T> : Executable<T, Proto.Transaction, Proto.TransactionResponse, TransactionResponse>, ITransaction where T : Transaction<T>
    {
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="T:Transaction_2"]/*' />
		protected Hbar DefaultMaxTransactionFee = new (2);
        
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.#ctor"]/*' />
        protected bool? regenerateTransactionId = null;
        private string Memo = "";
        protected IList<CustomFeeLimit> customFeeLimits = [];

		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.#ctor_2"]/*' />
		protected Transaction()
        {
            TransactionValidDuration = Transaction.DEFAULT_TRANSACTION_VALID_DURATION;
            SourceTransactionBody = new Proto.TransactionBody();
			TransactionIds = new ListGuarded<TransactionId>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};

		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.#ctor(Proto.TransactionBody)"]/*' />
		internal Transaction(Proto.TransactionBody txBody)
		{
			TransactionValidDuration = Transaction.DEFAULT_TRANSACTION_VALID_DURATION;
			MaxTransactionFee = Hbar.FromTinybars((long)txBody.TransactionFee);
			TransactionMemo = txBody.Memo;
			SourceTransactionBody = txBody;
			TransactionIds = new ListGuarded<TransactionId>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal Transaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs)
        {
			TransactionIds = new ListGuarded<TransactionId>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};

			DictionaryLinked<AccountId, Proto.Transaction> transactionMap = txs.First().Value;

            if (transactionMap.Count != 0 && transactionMap.Keys.First().Equals(Transaction.DUMMY_ACCOUNT_ID) && BatchKey != null)
            {
                // If the first account ID is a dummy account ID, then only the source TransactionBody needs to be copied.
                var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(transactionMap.Values.First().SignedTransactionBytes);
                SourceTransactionBody = Transaction.ParseTransactionBody(signedTransaction.BodyBytes);
            }
            else
            {
                var txCount = txs.Keys.Count;
                var nodeCount = txs.Values.First().Count;

                NodeAccountIds.EnsureCapacity(nodeCount);
                
				SigPairLists = new List<Proto.SignatureMap>(nodeCount * txCount);
                OuterTransactions = new List<Proto.Transaction>(nodeCount * txCount);
                InnerSignedTransactions = new List<Proto.SignedTransaction>(nodeCount * txCount);

                TransactionIds.EnsureCapacity(txCount);

                foreach (var transactionEntry in txs)
                {
                    if (!transactionEntry.Key.Equals(Transaction.DUMMY_TRANSACTION_ID))
                    {
                        TransactionIds.Add(transactionEntry.Key);
                    }

                    foreach (var nodeEntry in transactionEntry.Value)
                    {
                        if (NodeAccountIds.Count != nodeCount)
							NodeAccountIds.Add(nodeEntry.Key);

						var transaction = Proto.SignedTransaction.Parser.ParseFrom(nodeEntry.Value.SignedTransactionBytes);
                        OuterTransactions.Add(nodeEntry.Value);
                        SigPairLists.Add(transaction.SigMap);
                        InnerSignedTransactions.Add(transaction);
                        if (PublicKeys.Count == 0)
                        {
                            foreach (var sigPair in transaction.SigMap.SigPair)
                            {
                                PublicKeys.Add(PublicKey.FromBytes(sigPair.PubKeyPrefix.ToByteArray()));
                                Signers.Add(null);
                            }
                        }
                    }
                }

                NodeAccountIds.Remove(new AccountId(0, 0, 0));

                // Verify that transaction bodies match
                for (int i = 0; i < txCount; i++)
                {
                    Proto.TransactionBody? firstTxBody = null;
                    for (int j = 0; j < nodeCount; j++)
                    {
                        int k = i * nodeCount + j;
                        var txBody = Transaction.ParseTransactionBody(InnerSignedTransactions[k].BodyBytes);
                        if (firstTxBody == null)
							firstTxBody = txBody;
						else Transaction.RequireProtoMatches(firstTxBody, txBody, ["NodeAccountID"], "TransactionBody");
					}
                }

                SourceTransactionBody = Transaction.ParseTransactionBody(InnerSignedTransactions[0].BodyBytes);
            }

            TransactionValidDuration = SourceTransactionBody.TransactionValidDuration.ToTimeSpan();
            MaxTransactionFee = Hbar.FromTinybars(SourceTransactionBody.TransactionFee);
            TransactionMemo = SourceTransactionBody.Memo;
            
			customFeeLimits = [.. SourceTransactionBody.MaxCustomFees.Select(_ => CustomFeeLimit.FromProtobuf(_))];
            BatchKey = Key.FromProtobufKey(SourceTransactionBody.BatchKey);

			// The presence of signatures implies the Transaction should be frozen.
			if (PublicKeys.Count != 0)
				FrozenBodyBuilder = SourceTransactionBody;

		}

        
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.RequireNotFrozen"]/*' />
        public Hbar? MaxTransactionFee
        {
            get;
            set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.RequireNotFrozen_2"]/*' />
		public string TransactionMemo
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.RequireNotFrozen_3"]/*' />
		public TimeSpan TransactionValidDuration
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="P:Transaction.SourceTransactionBody"]/*' />
		public Proto.TransactionBody SourceTransactionBody { get; internal set; }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="P:Transaction.FrozenBodyBuilder"]/*' />
		public Proto.TransactionBody? FrozenBodyBuilder { get; internal set; }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.RequireNotFrozen_4"]/*' />
		public Key? BatchKey 
		{ 
			get;
			set 
			{ 
				RequireNotFrozen(); 
				field = value; 
			} 
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="P:Transaction.ShouldRegenerateTransactionId"]/*' />
		public bool ShouldRegenerateTransactionId { get; set; }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="T:Transaction_3"]/*' />
		public TransactionId TransactionId
		{
			get
			{
				if (TransactionIds.Count == 0 || !IsFrozen())
				{
					throw new InvalidOperationException("No transaction ID generated yet. Try freezing the transaction or manually setting the transaction ID.");
				}

				TransactionIds.IsLocked = true;
				return TransactionIds.Current;
			}
			set
			{
				TransactionIds.ClearAndSet(value);
				TransactionIds.IsLocked = true;
			}
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="P:Transaction.OuterTransactions"]/*' />
		public List<Proto.Transaction> OuterTransactions { get; internal set; }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="P:Transaction.InnerSignedTransactions"]/*' />
		public List<Proto.SignedTransaction> InnerSignedTransactions { get; internal set; }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="P:Transaction.SigPairLists"]/*' />
		public List<Proto.SignatureMap> SigPairLists { get; internal set; }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="P:Transaction.TransactionIds"]/*' />
		public ListGuarded<TransactionId> TransactionIds { get; internal set; }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="P:Transaction.PublicKeys"]/*' />
		public IList<PublicKey> PublicKeys { get; internal set; }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.freezeWith(Client)"]/*' />
		public List<Func<byte[], byte[]>?> Signers { get; internal set; }

		public override TransactionId TransactionIdInternal
		{
			get => TransactionIds.Current;
		}

		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.OnFreeze(Proto.TransactionBody)"]/*' />
		public abstract void OnFreeze(Proto.TransactionBody bodyBuilder);
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.OnScheduled(Proto.SchedulableTransactionBody)"]/*' />
		public abstract void OnScheduled(Proto.SchedulableTransactionBody scheduled);
		public abstract void ValidateChecksums(Client client);

		protected override bool IsBatchedAndNotBatchTransaction()
		{
			return BatchKey != null && this is not BatchTransaction;
		}

		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.DoSchedule(Proto.TransactionBody)"]/*' />
		protected virtual ScheduleCreateTransaction DoSchedule(Proto.TransactionBody bodyBuilder)
		{
			Proto.SchedulableTransactionBody proto = new ()
			{
				TransactionFee = bodyBuilder.TransactionFee,
				Memo = bodyBuilder.Memo,
			};
			
			proto.MaxCustomFees.AddRange(bodyBuilder.MaxCustomFees);

			OnScheduled(proto);

			var scheduled = new ScheduleCreateTransaction
			{
				ScheduledTransactionBody = proto
			};

			if (TransactionIds.Count > 0)
				scheduled.TransactionId = TransactionIds[0];

			return scheduled;
		}
		protected virtual IDictionary<AccountId, IDictionary<PublicKey, byte[]>> GetSignaturesAtOffset(int offset)
		{
			var map = new Dictionary<AccountId, IDictionary<PublicKey, byte[]>>(NodeAccountIds.Count);

			for (int i = 0; i < NodeAccountIds.Count; i++)
			{
				var sigMap = SigPairLists[i + offset];
				var nodeAccountId = NodeAccountIds[i];
				var keyMap = map.TryGetValue(nodeAccountId, out IDictionary<PublicKey, byte[]>? value) 
					? value 
					: new Dictionary<PublicKey, byte[]>();
				
				map.Add(nodeAccountId, keyMap);

				foreach (var sigPair in sigMap.SigPair)
				{
					keyMap.Add(PublicKey.FromBytes(sigPair.PubKeyPrefix.ToByteArray()), sigPair.Ed25519.ToByteArray());
				}
			}

			return map;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.KeyAlreadySigned(PublicKey)"]/*' />
		protected virtual bool KeyAlreadySigned(PublicKey key)
		{
			return PublicKeys.Contains(key);
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.RequireNotFrozen_5"]/*' />
		internal virtual void RequireNotFrozen()
		{
			if (IsFrozen())
			{
				throw new InvalidOperationException("transaction is immutable; it has at least one signature or has been explicitly frozen");
			}
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.RequireOneNodeAccountId"]/*' />
		internal virtual void RequireOneNodeAccountId()
		{
			if (NodeAccountIds.Count != 1)
				throw new InvalidOperationException("transaction did not have exactly one node ID set");
		}
		protected virtual Proto.TransactionBody SpawnBodyBuilder(Client? client)
		{
			var builder = new Proto.TransactionBody
			{
				Memo = Memo,
				TransactionFee = (ulong)(MaxTransactionFee ?? client?.DefaultMaxTransactionFee ?? DefaultMaxTransactionFee).ToTinybars(),
				TransactionValidDuration = TransactionValidDuration.ToProtoDuration(),
			};

			if (BatchKey is not null)
				builder.BatchKey = BatchKey.ToProtobufKey();

			builder.MaxCustomFees.AddRange(customFeeLimits.Select(_ => _.ToProtobuf()));

			return builder;
		}

		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.AddSignature(PublicKey,System.Byte[])"]/*' />
		public virtual T AddSignature(PublicKey publicKey, byte[] signature)
		{
			RequireOneNodeAccountId();

			if (!IsFrozen())
				Freeze();

			if (KeyAlreadySigned(publicKey))
			{
				// noinspection unchecked
				return (T)this;
			}

			TransactionIds.IsLocked = true;
			NodeAccountIds.IsLocked = true;

			for (int i = 0; i < OuterTransactions.Count; i++)
				OuterTransactions[i] = null;

			PublicKeys.Add(publicKey);
			Signers.Add(null);
			SigPairLists[0].SigPair.Add(publicKey.ToSignaturePairProtobuf(signature));

			// noinspection unchecked
			return (T)this;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.AddSignature(PublicKey,System.Byte[],TransactionId,AccountId)"]/*' />
		public virtual T AddSignature(PublicKey publicKey, byte[] signature, TransactionId transactionID, AccountId nodeID)
		{
			if (InnerSignedTransactions.Count == 0)
			{
				// noinspection unchecked
				return (T)this;
			}

			TransactionIds.IsLocked = true;

			for (int index = 0; index < InnerSignedTransactions.Count; index++)
				if (ProcessedSignatureForTransaction(index, publicKey, signature, transactionID, nodeID))
					UpdateTransactionState(publicKey);


			// noinspection unchecked
			return (T)this;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.BuildAllTransactions"]/*' />
		public virtual void BuildAllTransactions()
        {
            TransactionIds.IsLocked = true;
            NodeAccountIds.IsLocked = true;

            for (var i = 0; i < InnerSignedTransactions.Count; ++i)
				BuildTransaction(i);
		}
        
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.BuildTransaction(System.Int32)"]/*' />
        public virtual void BuildTransaction(int index)
        {
            // Check if transaction is already built.
            // Every time a signer is added via sign() or signWith(), all outerTransactions are nullified.
            if (OuterTransactions[index] != null && OuterTransactions[index].SignedTransactionBytes.Length != 0)
				return;

			SignTransaction(index);

            OuterTransactions[index] = new Proto.Transaction
			{
				SigMap = SigPairLists[index],
				SignedTransactionBytes = InnerSignedTransactions[index].ToByteString(),
			};
        }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.Freeze"]/*' />
		public virtual T Freeze()
		{
			return FreezeWith(null);
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.FreezeWith(Client)"]/*' />
		public virtual T FreezeWith(Client? client)
		{
			if (IsFrozen())
			{
				// noinspection unchecked
				return (T)this;
			}

			if (TransactionIds.Count == 0)
			{
				if (client != null)
				{
					var @operator = client.Operator_;

					if (@operator != null)
					{
						// Set a default transaction ID, generated from the operator account ID
						TransactionIds.ClearAndSet([TransactionId.Generate(@operator.AccountId)]);
					}
					else
					{
						// no client means there must be an explicitly set node ID and transaction ID
						throw new InvalidOperationException("`client` must have an `operator` or `transactionId` must be set");
					}
				}
				else
				{
					throw new InvalidOperationException("Transaction ID must be set, or operator must be provided via freezeWith()");
				}
			}

			if (NodeAccountIds.Count == 0)
			{
				if (client == null)
					throw new InvalidOperationException("`client` must be provided or both `nodeId` and `transactionId` must be set");

				try
				{
					if (BatchKey == null)
						NodeAccountIds.ClearAndSet(client.Network_.GetNodeAccountIdsForExecute());
					else 
						NodeAccountIds.ClearAndSet([AccountId.FromString(Transaction.ATOMIC_BATCH_NODE_ACCOUNT_ID)]);
				}
				catch (ThreadInterruptedException e)
				{
					throw new Exception(string.Empty, e);
				}
			}

			FrozenBodyBuilder = SpawnBodyBuilder(client);
			FrozenBodyBuilder.TransactionID = TransactionIds[0].ToProtobuf();

			int requiredChunks = GetRequiredChunks();
			
			OnFreeze(FrozenBodyBuilder);
			GenerateTransactionIds(TransactionIds[0], requiredChunks);
			WipeTransactionLists(requiredChunks);

			regenerateTransactionId = regenerateTransactionId != null ? regenerateTransactionId : client?.DefaultRegenerateTransactionId;

			// noinspection unchecked
			return (T)this;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.GetRequiredChunks"]/*' />
		public virtual int GetRequiredChunks()
		{
			return 1;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.GetSignatures"]/*' />
		public virtual IDictionary<AccountId, IDictionary<PublicKey, byte[]>> GetSignatures()
		{
			if (!IsFrozen())
				throw new InvalidOperationException("Transaction must be frozen in order to have signatures.");

			if (PublicKeys.Count == 0)
				return new Dictionary<AccountId, IDictionary<PublicKey, byte[]>>();

			BuildAllTransactions();

			return GetSignaturesAtOffset(0);
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.GetSignableNodeBodyBytesList"]/*' />
		public virtual List<SignableNodeTransactionBodyBytes> GetSignableNodeBodyBytesList()
		{
			if (!IsFrozen())
				throw new Exception("Transaction is not frozen");

			List<SignableNodeTransactionBodyBytes> signableNodeTransactionBodyBytesList = new();

			for (int i = 0; i < InnerSignedTransactions.Count; i++)
			{
				Proto.SignedTransaction signableNodeTransactionBodyBytes = InnerSignedTransactions[i];
				Proto.TransactionBody body = Transaction.ParseTransactionBody(signableNodeTransactionBodyBytes.BodyBytes);
				
				AccountId nodeID = AccountId.FromProtobuf(body.NodeAccountID);
				TransactionId transactionID = TransactionId.FromProtobuf(body.TransactionID);

				signableNodeTransactionBodyBytesList.Add(new SignableNodeTransactionBodyBytes(nodeID, transactionID, signableNodeTransactionBodyBytes.BodyBytes.ToByteArray()));
			}

			return signableNodeTransactionBodyBytesList;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.GetTransactionSize"]/*' />
		public virtual int GetTransactionSize()
		{
			if (!IsFrozen())
			{
				throw new InvalidOperationException("transaction must have been frozen before getting it's size, try calling `freeze`");
			}

			return MakeRequest().CalculateSize();
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.GetTransactionBodySize"]/*' />
		public virtual int GetTransactionBodySize()
		{
			if (!IsFrozen())
				throw new InvalidOperationException("transaction must have been frozen before getting it's body size, try calling `freeze`");

			return FrozenBodyBuilder?.CalculateSize() ?? 0;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.GetTransactionHash"]/*' />
		public virtual byte[] GetTransactionHash()
		{
			if (!IsFrozen())
			{
				throw new InvalidOperationException("transaction must have been frozen before calculating the hash will be stable, try calling `freeze`");
			}

			TransactionIds.IsLocked = true;
			NodeAccountIds.IsLocked = true;

			var index = TransactionIds.Index * NodeAccountIds.Count + NodeAccountIds.Index;

			BuildTransaction(index);

			return Transaction.GenerateHash(OuterTransactions[index].SignedTransactionBytes.ToByteArray());
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.GetTransactionHashPerNode"]/*' />
		public virtual IDictionary<AccountId, byte[]> GetTransactionHashPerNode()
		{
			if (!IsFrozen())
			{
				throw new InvalidOperationException("transaction must have been frozen before calculating the hash will be stable, try calling `freeze`");
			}

			BuildAllTransactions();
			var hashes = new Dictionary<AccountId, byte[]>();
			for (var i = 0; i < OuterTransactions.Count; i++)
			{
				hashes.Add(NodeAccountIds[i], Transaction.GenerateHash(OuterTransactions[i].SignedTransactionBytes.ToByteArray()));
			}

			return hashes;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.GenerateTransactionIds(TransactionId,System.Int32)"]/*' />
		public virtual void GenerateTransactionIds(TransactionId initialTransactionId, int count)
		{
			var locked = TransactionIds.IsLocked;

			TransactionIds.IsLocked = false;

			if (count == 1)
			{
				TransactionIds.ClearAndSet([initialTransactionId]);
				return;
			}

			var nextTransactionId = initialTransactionId.ToProtobuf();
			TransactionIds.EnsureCapacity(count);
			TransactionIds.Clear();
			for (int i = 0; i < count; i++)
			{
				TransactionIds.Add(TransactionId.FromProtobuf(nextTransactionId));

				// add 1 ns to the validStart to make cascading transaction IDs
				var nextValidStart = nextTransactionId.TransactionValidStart;
				nextValidStart.Nanos = nextValidStart.Nanos + 1;
				nextTransactionId.TransactionValidStart = nextValidStart;
			}

			TransactionIds.IsLocked = locked;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.IsFrozen"]/*' />
		public virtual bool IsFrozen()
		{
			return FrozenBodyBuilder != null;
		}
		public override TransactionResponse MapResponse(Proto.TransactionResponse transactionResponse, AccountId nodeId, Proto.Transaction request)
		{
			var transactionId = TransactionIdInternal;
			var hash = Transaction.GenerateHash(request.SignedTransactionBytes.ToByteArray());

			// advance is needed for chunked transactions
			TransactionIds.Advance();

			return TransactionResponse.Init(nodeId, transactionId, hash, null, this);
		}
		public virtual ResponseStatus MapResponseStatus(Proto.TransactionResponse transactionResponse)
		{
			return (ResponseStatus)transactionResponse.NodeTransactionPrecheckCode;
		}
		public virtual Transaction<T> RegenerateTransactionId(Client client)
		{
			ArgumentNullException.ThrowIfNull(client.OperatorAccountId);
			TransactionIds.IsLocked = false;
			var newTransactionID = TransactionId.Generate(client.OperatorAccountId);
			TransactionIds[TransactionIds.Index] = newTransactionID;
			TransactionIds.IsLocked = true;
			return this;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.Schedule"]/*' />
		public virtual ScheduleCreateTransaction Schedule(Action<ScheduleCreateTransaction>? onCreate = null)
		{
			RequireNotFrozen();
			if (NodeAccountIds.Count != 0)
			{
				throw new InvalidOperationException("The underlying transaction for a scheduled transaction cannot have node account IDs set");
			}

			var bodyBuilder = SpawnBodyBuilder(null);
			OnFreeze(bodyBuilder);

			ScheduleCreateTransaction scheduleCreateTransaction = DoSchedule(bodyBuilder);
			onCreate?.Invoke(scheduleCreateTransaction);
			return scheduleCreateTransaction;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.SignTransaction(System.Int32)"]/*' />
		public virtual void SignTransaction(int index)
        {
            var bodyBytes = InnerSignedTransactions[index].BodyBytes.ToByteArray();
            var thisSigPairList = SigPairLists[index].SigPair;
            for (var i = 0; i < PublicKeys.Count; i++)
            {
                if (Signers[i] == null)
                {
                    continue;
                }

                if (Transaction.PublicKeyIsInSigPairList(ByteString.CopyFrom(PublicKeys[i].ToBytesRaw()), thisSigPairList))
                {
                    continue;
                }

                var signatureBytes = Signers[i].Invoke(bodyBytes);
                SigPairLists[index].SigPair.Add(PublicKeys[i].ToSignaturePairProtobuf(signatureBytes));
            }
        }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.SignWithOperator(Client)"]/*' />
		public virtual T SignWithOperator(Client client)
		{
			if (client.Operator_ == null)
				throw new InvalidOperationException("`client` must have an `operator` to sign with the operator");

			if (!IsFrozen())
				FreezeWith(client);

			return SignWith(client.Operator_.PublicKey, client.Operator_.TransactionSigner);
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.SignWith(PublicKey,System.Func{System.Byte[],System.Byte[]})"]/*' />
		public virtual T SignWith(PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
		{
			if (!IsFrozen())
			{
				throw new InvalidOperationException("Signing requires transaction to be frozen");
			}

			if (KeyAlreadySigned(publicKey))
			{

				// noinspection unchecked
				return (T)this;
			}

			for (int i = 0; i < OuterTransactions.Count; i++)
			{
				OuterTransactions[i] = null;
			}

			PublicKeys.Add(publicKey);
			Signers.Add(transactionSigner);

			// noinspection unchecked
			return (T)this;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			var list = new Proto.TransactionList();

			// If no nodes have been selected yet,
			// the new TransactionBody can be used to build a Transaction protobuf object.
			if (NodeAccountIds.Count == 0)
			{
				var bodyBuilder = SpawnBodyBuilder(null);
				if (TransactionIds.Count != 0)
				{
					bodyBuilder.TransactionID = TransactionIds[0].ToProtobuf();
				}

				OnFreeze(bodyBuilder);

				list.TransactionList_.Add(new Proto.Transaction
				{
					SignedTransactionBytes = new Proto.SignedTransaction
					{
						BodyBytes = bodyBuilder.ToByteString()

					}.ToByteString()
				});
			}
			else
			{

				// Generate the SignedTransaction protobuf objects if the Transaction's not frozen.
				if (!IsFrozen())
				{
					FrozenBodyBuilder = SpawnBodyBuilder(null);
					if (TransactionIds.Count != 0)
					{
						FrozenBodyBuilder.TransactionID = TransactionIds[0].ToProtobuf();
					}

					OnFreeze(FrozenBodyBuilder);
					int requiredChunks = GetRequiredChunks();
					if (TransactionIds.Count != 0)
					{
						GenerateTransactionIds(TransactionIds[0], requiredChunks);
					}

					WipeTransactionLists(requiredChunks);
				}


				// Build all the Transaction protobuf objects and add them to the TransactionList protobuf object.
				BuildAllTransactions();

				foreach (var transaction in OuterTransactions)
					list.TransactionList_.Add(transaction);
			}

			return list.ToByteArray();
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.WipeTransactionLists(System.Int32)"]/*' />
		public virtual void WipeTransactionLists(int requiredChunks)
		{
			if (TransactionIds.Count != 0)
				FrozenBodyBuilder.TransactionID = TransactionIdInternal.ToProtobuf();

			OuterTransactions = new List<Proto.Transaction>(NodeAccountIds.Count);
			SigPairLists = new List<Proto.SignatureMap>(NodeAccountIds.Count);
			InnerSignedTransactions = new List<Proto.SignedTransaction>(NodeAccountIds.Count);

			foreach (AccountId nodeId in NodeAccountIds)
			{
				FrozenBodyBuilder.NodeAccountID = nodeId.ToProtobuf();

				SigPairLists.Add(new Proto.SignatureMap());
				OuterTransactions.Add(null);
				InnerSignedTransactions.Add(new Proto.SignedTransaction
				{
					BodyBytes = FrozenBodyBuilder.ToByteString()
				});
			}
		}

		public override Method<Proto.Transaction, Proto.TransactionResponse> GetMethod()
		{
			MethodDescriptor methoddescriptor = GetMethodDescriptor();

			IMessage input = (IMessage)Activator.CreateInstance(methoddescriptor.InputType.ClrType)!;
			IMessage output = (IMessage)Activator.CreateInstance(methoddescriptor.OutputType.ClrType)!;

			return new Method<Proto.Transaction, Proto.TransactionResponse>(
				type: MethodType.Unary,
				name: methoddescriptor.Name,
				serviceName: methoddescriptor.Service.FullName,
				requestMarshaller: Marshallers.Create(r => r.ToByteArray(), data => Proto.Transaction.Parser.ParseFrom(data)),
				responseMarshaller: Marshallers.Create(r => r.ToByteArray(), data => Proto.TransactionResponse.Parser.ParseFrom(data)));
		}

		public override ExecutionState GetExecutionState(ResponseStatus status, Proto.TransactionResponse response)
		{
			if (status == ResponseStatus.TransactionExpired)
			{
				if (regenerateTransactionId ?? false || TransactionIds.IsLocked)
					return ExecutionState.RequestError;
				else
				{
					var firstTransactionId = TransactionIds[0];
					var accountId = firstTransactionId.AccountId;

					GenerateTransactionIds(TransactionId.Generate(accountId), TransactionIds.Count);
					WipeTransactionLists(TransactionIds.Count);
					
					return ExecutionState.Retry;
				}
			}

			return base.GetExecutionState(status, response);
		}
		public override void OnExecute(Client client)
		{
			if (!IsFrozen())
				FreezeWith(client);

			var accountId = TransactionIds[0].AccountId;

			if (client.AutoValidateChecksums)
				try
				{
					accountId.ValidateChecksum(client);
					ValidateChecksums(client);
				}
				catch (BadEntityIdException exc)
				{
					throw new ArgumentException(exc.Message);
				}

			var operatorId = client.OperatorAccountId;

			if (operatorId != null && operatorId.Equals(accountId))
			{
				// on execute, sign each transaction with the operator, if present
				// and we are signing a transaction that used the default transaction ID
				SignWithOperator(client);
			}
		}
		public override Task OnExecuteAsync(Client client)
		{
			OnExecute(client);

			return Task.CompletedTask;
		}
		public override Proto.Transaction MakeRequest()
        {
            var index = NodeAccountIds.Index + (TransactionIds.Index * NodeAccountIds.Count);

            BuildTransaction(index);
            return OuterTransactions[index];
        }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.SetNodeAccountIds(System.Collections.Generic.IEnumerable{AccountId})"]/*' />
		public virtual T SetNodeAccountIds(IEnumerable<AccountId> nodeaccountids)
		{
			RequireNotFrozen();

			NodeAccountIds = [.. nodeaccountids];

			return (T)this;
		}
		public override string ToString()
		{
			// NOTE: regex is for removing the instance address from the default debug output
			Proto.TransactionBody body = SpawnBodyBuilder(null);
			
			if (TransactionIds.Count != 0)
				body.TransactionID = TransactionIds[0].ToProtobuf();

			if (NodeAccountIds.Count != 0)
				body.NodeAccountID = NodeAccountIds[0].ToProtobuf();

			OnFreeze(body);

			return Regex.Replace(body.ToString(), "@[A-Za-z0-9]+", string.Empty);
		}

		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.Batchify(Client,Key)"]/*' />
		public T Batchify(Client client, Key batchKey)
		{
			RequireNotFrozen();
			ArgumentNullException.ThrowIfNull(batchKey);
			this.BatchKey = batchKey;
			SignWithOperator(client);

			// noinspection unchecked
			return (T)this;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.Sign(PrivateKey)"]/*' />
		public T Sign(PrivateKey privateKey)
		{
			return SignWith(privateKey.GetPublicKey(), privateKey.Sign);
		}

		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.AddSignatureIfNotExists(System.Int32,PublicKey,System.Byte[])"]/*' />
		private bool AddSignatureIfNotExists(int index, PublicKey publicKey, byte[] signature)
		{
			Proto.SignatureMap sigMapBuilder = SigPairLists[index];

			// Check if the signature is already in the signature map
			if (IsSignatureAlreadyPresent(sigMapBuilder, publicKey))
				return false;

			// Add the signature to the signature map
			Proto.SignaturePair newSigPair = publicKey.ToSignaturePairProtobuf(signature);
			sigMapBuilder.SigPair.Add(newSigPair);

			return true;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.IsSignatureAlreadyPresent(Proto.SignatureMap,PublicKey)"]/*' />
		private bool IsSignatureAlreadyPresent(Proto.SignatureMap sigMapBuilder, PublicKey publicKey)
		{
			foreach (Proto.SignaturePair sig in sigMapBuilder.SigPair)
				if (Equals(sig.PubKeyPrefix.ToByteArray(), publicKey.ToBytesRaw()))
					return true;

			return false;
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.MatchesTargetTransactionAndNode(Proto.TransactionBody,TransactionId,AccountId)"]/*' />
		private bool MatchesTargetTransactionAndNode(Proto.TransactionBody body, TransactionId targetTransactionID, AccountId targetNodeID)
        {
            TransactionId bodyTxID = TransactionId.FromProtobuf(body.TransactionID);
            AccountId bodyNodeID = AccountId.FromProtobuf(body.NodeAccountID);

            return bodyTxID.ToString().Equals(targetTransactionID.ToString()) && bodyNodeID.ToString().Equals(targetNodeID.ToString());
        }
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.ProcessedSignatureForTransaction(System.Int32,PublicKey,System.Byte[],TransactionId,AccountId)"]/*' />
		private bool ProcessedSignatureForTransaction(int index, PublicKey publicKey, byte[] signature, TransactionId transactionID, AccountId nodeID)
		{
			Proto.SignedTransaction temp = InnerSignedTransactions[index];
			Proto.TransactionBody body = Transaction.ParseTransactionBody(temp);

			if (body == null)
				return false;

			if (!MatchesTargetTransactionAndNode(body, transactionID, nodeID))
				return false;

			return AddSignatureIfNotExists(index, publicKey, signature);
		}
		
		/// <include file="Transaction.cs.xml" path='docs/member[@name="M:Transaction.UpdateTransactionState(PublicKey)"]/*' />
		private void UpdateTransactionState(PublicKey publicKey)
        {
            PublicKeys.Add(publicKey);
            Signers.Add(null);
        }
    }
}