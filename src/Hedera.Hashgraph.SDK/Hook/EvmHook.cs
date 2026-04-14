// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Hedera.Hashgraph.SDK.Contract;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="EvmHook.cs.xml" path='docs/member[@name="T:EvmHook"]/*' />
    public class EvmHook : EvmHookSpec
    {
        /// <include file="EvmHook.cs.xml" path='docs/member[@name="M:EvmHook.#ctor(ContractId)"]/*' />
        public EvmHook(ContractId contractId) : this(contractId, []) { }
        /// <include file="EvmHook.cs.xml" path='docs/member[@name="M:EvmHook.#ctor(ContractId,System.Collections.Generic.IEnumerable{EvmHookStorageUpdate})"]/*' />
        public EvmHook(ContractId contractId, IEnumerable<EvmHookStorageUpdate> storageUpdates) : base(contractId)
        {
            StorageUpdates = [.. storageUpdates];
        }

		/// <include file="EvmHook.cs.xml" path='docs/member[@name="M:EvmHook.FromProtobuf(Proto.Services.EvmHook)"]/*' />
		public static EvmHook FromProtobuf(Proto.Services.EvmHook proto)
		{
			var storageUpdates = new List<EvmHookStorageUpdate>();
			foreach (var protoUpdate in proto.StorageUpdates)
			{
				storageUpdates.Add(EvmHookStorageUpdate.FromProtobuf(protoUpdate));
			}

			return new EvmHook(ContractId.FromProtobuf(proto.Spec.ContractId), storageUpdates);
		}

		/// <include file="EvmHook.cs.xml" path='docs/member[@name="M:EvmHook.ToProtobuf"]/*' />
		public virtual Proto.Services.EvmHook ToProtobuf()
		{
			Proto.Services.EvmHook proto = new()
			{
				Spec = new Proto.Services.EvmHookSpec
				{
					ContractId = ContractId.ToProtobuf(),
				}
			};

			foreach (EvmHookStorageUpdate update in StorageUpdates)
				proto.StorageUpdates.Add(update.ToProtobuf());

			return proto;
		}

		/// <include file="EvmHook.cs.xml" path='docs/member[@name="P:EvmHook.StorageUpdates"]/*' />
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
