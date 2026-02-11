// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;

using System;

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <summary>
    /// The ID of an entity using a hook.
    /// <p>
    /// This class represents the HookEntityId protobuf message, which can be either
    /// an account ID or a contract ID.
    /// </summary>
    public class HookEntityId
    {
        /// <summary>
        /// Create a HookEntityId with an account ID.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        public HookEntityId(AccountId accountId)
        {
            AccountId = accountId;
            ContractId = null;
        }

        /// <summary>
        /// Create a HookEntityId with a contract ID.
        /// </summary>
        /// <param name="contractId">the contract ID</param>
        public HookEntityId(ContractId contractId)
        {
            AccountId = null;
            ContractId = contractId;
        }

		public AccountId? AccountId { get; }
		public ContractId? ContractId { get; }

		/// <summary>
		/// Convert this HookEntityId to a protobuf message.
		/// </summary>
		/// <returns>the protobuf HookEntityId</returns>
		public virtual Proto.HookEntityId ToProtobuf()
        {
			Proto.HookEntityId proto = new ();

            if (AccountId is not null) proto.AccountId = AccountId.ToProtobuf();
            if (ContractId is not null) proto.ContractId = ContractId.ToProtobuf();

            return proto;
        }

        /// <summary>
        /// Create a HookEntityId from a protobuf message.
        /// </summary>
        /// <param name="proto">the protobuf HookEntityId</param>
        /// <returns>a new HookEntityId instance</returns>
        public static HookEntityId FromProtobuf(Proto.HookEntityId proto)
        {
            if (proto.AccountId is not null)
				return new HookEntityId(AccountId.FromProtobuf(proto.AccountId));

			else return new HookEntityId(ContractId.FromProtobuf(proto.ContractId));
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(AccountId, ContractId);
		}
		public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o?.GetType())
                return false;
            HookEntityId that = (HookEntityId)o;

            return Equals(AccountId, that.AccountId) && Equals(ContractId, that.ContractId);
        }
    }
}