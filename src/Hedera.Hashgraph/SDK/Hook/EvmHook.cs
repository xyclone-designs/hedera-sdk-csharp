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
    public class EvmHook : EvmHookSpec
    {
        /// <summary>
        /// Create a new EvmHook with no initial storage updates.
        /// </summary>
        /// <param name="contractId">underlying contract of the hook</param>
        public EvmHook(ContractId contractId) : this(contractId, []) { }
        /// <summary>
        /// Create a new EvmHook with initial storage updates.
        /// </summary>
        /// <param name="contractId">underlying contract of the hook</param>
        /// <param name="storageUpdates">the initial storage updates for the lambda</param>
        public EvmHook(ContractId contractId, IList<EvmHookStorageUpdate> storageUpdates) : base(contractId)
        {
            StorageUpdates = [.. storageUpdates];
        }

		/// <summary>
		/// Create a EvmHook from a protobuf message.
		/// </summary>
		/// <param name="proto">the protobuf EvmHook</param>
		/// <returns>a new EvmHook instance</returns>
		public static EvmHook FromProtobuf(Proto.EvmHook proto)
		{
			var storageUpdates = new List<EvmHookStorageUpdate>();
			foreach (var protoUpdate in proto.StorageUpdates)
			{
				storageUpdates.Add(EvmHookStorageUpdate.FromProtobuf(protoUpdate));
			}

			return new EvmHook(ContractId.FromProtobuf(proto.Spec.ContractId), storageUpdates);
		}

		/// <summary>
		/// Convert this EvmHook to a protobuf message.
		/// </summary>
		/// <returns>the protobuf EvmHook</returns>
		public virtual Proto.EvmHook ToProtobuf()
		{
			Proto.EvmHook proto = new()
			{
				Spec = new Proto.EvmHookSpec
				{
					ContractId = ContractId.ToProtobuf(),
				}
			};

			foreach (EvmHookStorageUpdate update in StorageUpdates)
				proto.StorageUpdates.Add(update.ToProtobuf());

			return proto;
		}

		/// <summary>
		/// Get the initial storage updates for this lambda.
		/// </summary>
		/// <returns>an immutable list of storage updates</returns>
		public virtual IReadOnlyList<EvmHookStorageUpdate> StorageUpdates { get; }

		public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o?.GetType())
                return false;
            EvmHook that = (EvmHook)o;
            return base.Equals(o) && StorageUpdates.Equals(that.StorageUpdates);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), StorageUpdates);
        }
    }
}