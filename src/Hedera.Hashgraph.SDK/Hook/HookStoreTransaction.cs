// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="HookStoreTransaction.cs.xml" path='docs/member[@name="T:HookStoreTransaction"]/*' />
    public class HookStoreTransaction : Transaction<HookStoreTransaction>
    {
        public HookStoreTransaction()
        {
        }
		/// <include file="HookStoreTransaction.cs.xml" path='docs/member[@name="M:HookStoreTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal HookStoreTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="HookStoreTransaction.cs.xml" path='docs/member[@name="M:HookStoreTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal HookStoreTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
		{
			InitFromTransactionBody();
		}

		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.HookStore;

			HookId = HookId.FromProtobuf(body.HookId);
			StorageUpdates.ClearAndSet(body.StorageUpdates.Select(_ => EvmHookStorageUpdate.FromProtobuf(_)));
		}

		public virtual HookId? HookId { get; set { RequireNotFrozen(); field = value; } }
		public virtual ListGuarded<EvmHookStorageUpdate> StorageUpdates
		{
			init; get => field ??= new ListGuarded<EvmHookStorageUpdate>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}

		/// <include file="HookStoreTransaction.cs.xml" path='docs/member[@name="M:HookStoreTransaction.ToProtobuf"]/*' />
		public Proto.HookStoreTransactionBody ToProtobuf()
		{
			Proto.HookStoreTransactionBody builder = new ();

			if (HookId != null)
				builder.HookId = HookId.ToProtobuf();

			foreach (var update in StorageUpdates)
				builder.StorageUpdates.Add(update.ToProtobuf());

			return builder;
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.hookStore);

			return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
		public override void ValidateChecksums(Client client)
		{
			if (HookId != null)
			{
				var entityId = HookId.EntityId;

				if (entityId.IsAccount)
					entityId.AccountId?.ValidateChecksum(client);
				else if (entityId.IsContract)
					entityId.ContractId?.ValidateChecksum(client);
			}
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
		{
			bodyBuilder.HookStore = ToProtobuf();
		}
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			throw new NotSupportedException("cannot schedule HookStoreTransaction");
		}

        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
    }
}