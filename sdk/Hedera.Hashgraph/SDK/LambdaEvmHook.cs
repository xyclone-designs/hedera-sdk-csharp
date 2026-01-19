using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Definition of a lambda EVM hook.
     * <p>
     * This class represents a hook implementation that is programmed in EVM bytecode
     * and can access state or interact with external contracts. It includes the
     * hook specification and any initial storage updates.
     */
    public class LambdaEvmHook : EvmHookSpec 
    {
        private List<LambdaStorageUpdate> StorageUpdates { get; }
        private ReadOnlyCollection<LambdaStorageUpdate> StorageUpdatesRead { get => StorageUpdates.AsReadOnly(); }

        /**
         * Create a new LambdaEvmHook with no initial storage updates.
         *
         * @param contractId underlying contract of the hook
         */
        public LambdaEvmHook(ContractId contractId) : this(contractId, []) { }

		/**
         * Create a new LambdaEvmHook with initial storage updates.
         *
         * @param contractId underlying contract of the hook
         * @param storageUpdates the initial storage updates for the lambda
         */
		public LambdaEvmHook(ContractId contractId, List<LambdaStorageUpdate> storageUpdates) : base(contractId)
		{
            StorageUpdates = storageUpdates;

		}

        /**
         * Convert this LambdaEvmHook to a protobuf message.
         *
         * @return the protobuf LambdaEvmHook
         */
        public Proto.LambdaEvmHook ToProtobuf() 
        {
			Proto.LambdaEvmHook builder = new ()
            {
                Spec = new Proto.EvmHookSpec
				{
					ContractId = ContractId.ToProtobuf(),
				},
            };

            builder.StorageUpdates.AddRange(StorageUpdates.Select(_ => _.ToProtobuf()));

            return builder;
        }

        /**
         * Create a LambdaEvmHook from a protobuf message.
         *
         * @param proto the protobuf LambdaEvmHook
         * @return a new LambdaEvmHook instance
         */
        public static LambdaEvmHook FromProtobuf(Proto.LambdaEvmHook proto) 
        {
            return new LambdaEvmHook(ContractId.FromProtobuf(proto.Spec.ContractId), proto.StorageUpdates
				.Select(_ => LambdaStorageUpdate.FromProtobuf(_))
				.ToList());
        }

        public override bool Equals(object? obj) 
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            LambdaEvmHook that = (LambdaEvmHook) obj;
            
            return base.Equals(obj) && StorageUpdates.Equals(that.StorageUpdates);
        }

        public override int GetHashCode() 
        {
            return HashCode.Combine(base.GetHashCode(), StorageUpdates);
        }
        public override string ToString() 
        {
            return "LambdaEvmHook{contractId=" + ContractId + ", storageUpdates=" + StorageUpdates + "}";
        }
    }

}