// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Hedera.Hashgraph.SDK.Contract;

using System;

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="EvmHookSpec.cs.xml" path='docs/member[@name="T:EvmHookSpec"]/*' />
    public abstract class EvmHookSpec
    {
        /// <include file="EvmHookSpec.cs.xml" path='docs/member[@name="M:EvmHookSpec.#ctor(ContractId)"]/*' />
        protected EvmHookSpec(ContractId contractId)
        {
            ContractId = contractId;
        }

        /// <include file="EvmHookSpec.cs.xml" path='docs/member[@name="P:EvmHookSpec.ContractId"]/*' />
        public virtual ContractId ContractId { get; }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o?.GetType())
                return false;
            EvmHookSpec that = (EvmHookSpec)o;

            return ContractId.Equals(that.ContractId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ContractId);
        }
    }
}