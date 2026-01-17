using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	* Shared specifications for an EVM hook.
	* <p>
	* This class defines the source of EVM bytecode for a hook implementation.
	* Currently, hooks can only be implemented by referencing an existing contract
	* that implements the extension point API.
	*/
	public abstract class EvmHookSpec
	{
		/**
		 * Create a new EvmHookSpec that references a contract.
		 *
		 * @param contractId the ID of the contract that implements the hook
		 */
		protected EvmHookSpec(ContractId contractId)
		{
			ContractId = contractId;
		}

		/**
		 * Get the contract ID that implements this hook.
		 *
		 * @return the contract ID
		 */
		public ContractId ContractId { get; }

        public override bool Equals(object? obj)
        {
			if (this == obj) return true;
			if (obj == null || GetType() != obj.GetType()) return false;

			EvmHookSpec that = (EvmHookSpec)obj;
			
			return ContractId.Equals(that.ContractId);
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(ContractId);
		}
		public override string ToString()
        {
            return "EvmHookSpec{contractId=" + ContractId + "}";
		}
	}
}