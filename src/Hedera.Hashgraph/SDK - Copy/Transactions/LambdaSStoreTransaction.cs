// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Adds or removes key/value pairs in the storage of a lambda.
    /// </summary>
    public class LambdaSStoreTransaction : Transaction<LambdaSStoreTransaction>
    {
        private HookId hookId;
        private IList<LambdaStorageUpdate> storageUpdates = new ();
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
            hookId = ArgumentNullException.ThrowIfNull(hookId);
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
            storageUpdates = new List(updates);
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
            storageUpdates.Add(ArgumentNullException.ThrowIfNull(update));
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
            var builder = LambdaSStoreTransactionBody.NewBuilder();
            if (hookId != null)
            {
                builder.HookId(hookId.ToProtobuf());
            }

            foreach (var update in storageUpdates)
            {
                builder.AddStorageUpdates(update.ToProtobuf());
            }

            return proto;
        }

        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.LambdaSstore();
            if (body.HasHookId())
            {
                hookId = HookId.FromProtobuf(body.GetHookId());
            }

            storageUpdates = new ();
            foreach (var protoUpdate in body.GetStorageUpdatesList())
            {
                storageUpdates.Add(LambdaStorageUpdate.FromProtobuf(protoUpdate));
            }
        }

        override void ValidateChecksums(Client client)
        {
            if (hookId != null)
            {
                var entityId = hookId.GetEntityId();
                if (entityId.IsAccount())
                {
                    entityId.AccountId.ValidateChecksum(client);
                }
                else if (entityId.IsContract())
                {
                    entityId.GetContractId().ValidateChecksum(client);
                }
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetLambdaSStoreMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SetLambdaSstore(Build());
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("cannot schedule LambdaSStoreTransaction");
        }
    }
}