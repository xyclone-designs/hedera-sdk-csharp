// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Hedera.Hashgraph.SDK.Contract;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Shared specifications for an EVM hook.
    /// <p>
    /// This class defines the source of EVM bytecode for a hook implementation.
    /// Currently, hooks can only be implemented by referencing an existing contract
    /// that implements the extension point API.
    /// </summary>
    public abstract class EvmHookSpec
    {
        /// <summary>
        /// Create a new EvmHookSpec that references a contract.
        /// </summary>
        /// <param name="contractId">the ID of the contract that implements the hook</param>
        protected EvmHookSpec(ContractId contractId)
        {
            ContractId = contractId;
        }

        /// <summary>
        /// Get the contract ID that implements this hook.
        /// </summary>
        /// <returns>the contract ID</returns>
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