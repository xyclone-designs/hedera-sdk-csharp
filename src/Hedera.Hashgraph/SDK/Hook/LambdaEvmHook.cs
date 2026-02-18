// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Hedera.Hashgraph.SDK.Contract;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <summary>
    /// Definition of a lambda EVM hook.
    /// <p>
    /// This class represents a hook implementation that is programmed in EVM bytecode
    /// and can access state or interact with external contracts. It includes the
    /// hook specification and any initial storage updates.
    /// </summary>
    public class LambdaEvmHook : EvmHookSpec
    {
        private readonly List<LambdaStorageUpdate> StorageUpdates;

        /// <summary>
        /// Create a new LambdaEvmHook with no initial storage updates.
        /// </summary>
        /// <param name="contractId">underlying contract of the hook</param>
        public LambdaEvmHook(ContractId contractId) : this(contractId, []) { }
        /// <summary>
        /// Create a new LambdaEvmHook with initial storage updates.
        /// </summary>
        /// <param name="contractId">underlying contract of the hook</param>
        /// <param name="storageUpdates">the initial storage updates for the lambda</param>
        public LambdaEvmHook(ContractId contractId, IList<LambdaStorageUpdate> storageUpdates) : base(contractId)
        {
            StorageUpdates = [.. storageUpdates];
        }

		/// <summary>
		/// Create a LambdaEvmHook from a protobuf message.
		/// </summary>
		/// <param name="proto">the protobuf LambdaEvmHook</param>
		/// <returns>a new LambdaEvmHook instance</returns>
		public static LambdaEvmHook FromProtobuf(Proto.LambdaEvmHook proto)
		{
			var storageUpdates = new List<LambdaStorageUpdate>();
			foreach (var protoUpdate in proto.StorageUpdates)
			{
				storageUpdates.Add(LambdaStorageUpdate.FromProtobuf(protoUpdate));
			}

			return new LambdaEvmHook(ContractId.FromProtobuf(proto.Spec.ContractId), storageUpdates);
		}

		/// <summary>
		/// Convert this LambdaEvmHook to a protobuf message.
		/// </summary>
		/// <returns>the protobuf LambdaEvmHook</returns>
		public virtual Proto.LambdaEvmHook ToProtobuf()
		{
			Proto.LambdaEvmHook proto = new()
			{
				Spec = new Proto.EvmHookSpec
				{
					ContractId = ContractId.ToProtobuf(),
				}
			};

			foreach (LambdaStorageUpdate update in StorageUpdates)
				proto.StorageUpdates.Add(update.ToProtobuf());

			return proto;
		}
		/// <summary>
		/// Get the initial storage updates for this lambda.
		/// </summary>
		/// <returns>an immutable list of storage updates</returns>
		public virtual IReadOnlyList<LambdaStorageUpdate> GetStorageUpdates()
        {
            return StorageUpdates.AsReadOnly();
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o?.GetType())
                return false;
            LambdaEvmHook that = (LambdaEvmHook)o;
            return base.Equals(o) && StorageUpdates.Equals(that.StorageUpdates);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), StorageUpdates);
        }
    }
}