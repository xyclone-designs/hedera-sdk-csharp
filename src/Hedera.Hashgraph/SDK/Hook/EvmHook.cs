// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Hedera.Hashgraph.SDK.Contract;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Hook
{
    public class EvmHook : EvmHookSpec
    {
        public EvmHook(ContractId contractId) : this(contractId, []) { }
        public EvmHook(ContractId contractId, IList<EvmHookStorageUpdate> storageUpdates) : base(contractId)
        {
            StorageUpdates = [.. storageUpdates];
        }

		public static EvmHook FromProtobuf(Proto.EvmHook proto)
		{
			var StorageUpdates = new List<EvmHookStorageUpdate>();

			foreach (var protoUpdate in proto.GetStorageUpdatesList())
			{
				StorageUpdates.Add(EvmHookStorageUpdate.FromProtobuf(protoUpdate));
			}

			return new EvmHook(ContractId.FromProtobuf(proto.GetSpec().GetContractId()), StorageUpdates);
		}

		public virtual IReadOnlyList<EvmHookStorageUpdate> StorageUpdates { get; }

        public virtual Proto.EvmHook ToProtobuf()
        {
            var builder = new Proto.EvmHook
            {
                Spec = new Proto.EvmHookSpec
				{
					ContractId = ContractId.ToProtobuf()
				}
			};

            foreach (EvmHookStorageUpdate update in StorageUpdates)
            {
                builder.AddStorageUpdates(update.ToProtobuf());
            }

            return builder.Build();
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
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