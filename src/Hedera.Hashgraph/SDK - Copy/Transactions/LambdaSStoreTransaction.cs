// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hapi.Node.Hooks;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Adds or removes key/value pairs in the storage of a lambda.
    /// </summary>
    public class LambdaSStoreTransaction : Transaction<LambdaSStoreTransaction>
    {
        private HookId hookId;
        private IList<LambdaStorageUpdate> storageUpdates = [];
        /// <summary>
        /// Create a new empty LambdaSStoreTransaction.
        /// </summary>
        public LambdaSStoreTransaction()
        {
        }

        LambdaSStoreTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        LambdaSStoreTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Set the id of the lambda whose storage is being updated.
        /// </summary>
        /// <param name="hookId">the hook id</param>
        /// <returns>this</returns>
        public virtual LambdaSStoreTransaction SetHookId(HookId hookId)
        {
            RequireNotFrozen();
            hookId = hookId;
            return this;
        }

        /// <summary>
        /// Get the hook id.
        /// </summary>
        /// <returns>the hook id</returns>
        public virtual HookId GetHookId()
        {
            return hookId;
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
            storageUpdates = [.. updates];
            return this;
        }

        /// <summary>
        /// Add a storage update.
        /// </summary>
        /// <param name="update">the update to add</param>
        /// <returns>this</returns>
        public virtual LambdaSStoreTransaction AddStorageUpdate(LambdaStorageUpdate update)
        {
            RequireNotFrozen();
            storageUpdates.Add(update);
            return this;
        }

        /// <summary>
        /// Get the storage updates.
        /// </summary>
        /// <returns>list of updates</returns>
        public virtual IList<LambdaStorageUpdate> GetStorageUpdates()
        {
            return storageUpdates;
        }

        public virtual LambdaSStoreTransactionBody Build()
        {
            var builder = new LambdaSStoreTransactionBody();

            if (hookId != null)
            {
                builder.HookId = hookId.ToProtobuf();
            }

            foreach (var update in storageUpdates)
            {
                builder.StorageUpdates.Add(update.ToProtobuf());
            }

            return builder;
        }

        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.LambdaSstore;
            if (body.HookId is not null)
            {
                hookId = HookId.FromProtobuf(body.HookId);
            }

            storageUpdates = [];
            foreach (var protoUpdate in body.StorageUpdates)
            {
                storageUpdates.Add(LambdaStorageUpdate.FromProtobuf(protoUpdate));
            }
        }

        override void ValidateChecksums(Client client)
        {
            if (hookId != null)
            {
                var entityId = hookId.EntityId;

                if (entityId.AccountId is not null)
                {
                    entityId.AccountId.ValidateChecksum(client);
                }
                else if (entityId.ContractId is not null)
                {
                    entityId.ContractId.ValidateChecksum(client);
                }
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetLambdaSStoreMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SetLambdaSstore = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("cannot schedule LambdaSStoreTransaction");
        }
    }
}