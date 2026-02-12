// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Hook;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions
{
	/// <summary>
	/// Adds or removes key/value pairs in the storage of a lambda.
	/// </summary>
	public class LambdaSStoreTransaction : Transaction<LambdaSStoreTransaction>
	{
		/// <summary>
		/// Create a new empty LambdaSStoreTransaction.
		/// </summary>
		public LambdaSStoreTransaction() { }
		public LambdaSStoreTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		public LambdaSStoreTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
		{
			InitFromTransactionBody();
		}

		/// <summary>
		/// Set the id of the lambda whose storage is being updated.
		/// </summary>
		/// <param name="HookId">the hook id</param>
		/// <returns>this</returns>
		public virtual HookId? HookId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>
		/// Add a storage update.
		/// </summary>
		/// <param name="update">the update to add</param>
		/// <returns>this</returns>
		public virtual LambdaSStoreTransaction AddStorageUpdate(LambdaStorageUpdate update)
		{
			RequireNotFrozen();
			StorageUpdates.Add(update);
			return this;
		}
		/// <summary>
		/// Replace the list of storage updates.
		/// </summary>
		/// <param name="updates">list of updates</param>
		/// <returns>this</returns>
		public virtual LambdaSStoreTransaction SetStorageUpdates(IList<LambdaStorageUpdate> updates)
		{
			RequireNotFrozen();
			ArgumentNullException.ThrowIfNull(updates);
			StorageUpdates = [.. updates];
			return this;
		}

		public virtual IList<LambdaStorageUpdate> StorageUpdates { get; private set; } = [];

		public virtual Proto.LambdaSStoreTransactionBody ToProtobuf()
		{
			var builder = new Proto.LambdaSStoreTransactionBody();

			if (HookId != null)
			{
				builder.HookId = HookId.ToProtobuf();
			}

			foreach (var update in StorageUpdates)
			{
				builder.StorageUpdates.Add(update.ToProtobuf());
			}

			return builder;
		}

		public virtual void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.LambdaSstore;
			if (body.HookId is not null)
			{
				HookId = HookId.FromProtobuf(body.HookId);
			}

			StorageUpdates = [];
			foreach (var protoUpdate in body.StorageUpdates)
			{
				StorageUpdates.Add(LambdaStorageUpdate.FromProtobuf(protoUpdate));
			}
		}

		public override void ValidateChecksums(Client client)
		{
			if (HookId != null)
			{
				var entityId = HookId.EntityId;

				if (entityId.AccountId is not null)
				{
					entityId.AccountId.ValidateChecksum(client);
				}
				else entityId.ContractId?.ValidateChecksum(client);
			}
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
		{
			bodyBuilder.LambdaSstore = ToProtobuf();
		}
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			throw new NotSupportedException("cannot schedule LambdaSStoreTransaction");
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.lambdaSStore);

			return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
		}

		public override void OnExecute(Client client)
        {
            throw new NotImplementedException();
        }
        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}