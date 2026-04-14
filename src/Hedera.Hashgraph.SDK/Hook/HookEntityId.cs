// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;

using System;

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="HookEntityId.cs.xml" path='docs/member[@name="T:HookEntityId"]/*' />
    public class HookEntityId
    {
        /// <include file="HookEntityId.cs.xml" path='docs/member[@name="M:HookEntityId.#ctor(AccountId)"]/*' />
        public HookEntityId(AccountId accountId)
        {
            AccountId = accountId;
            ContractId = null;
        }

        /// <include file="HookEntityId.cs.xml" path='docs/member[@name="M:HookEntityId.#ctor(ContractId)"]/*' />
        public HookEntityId(ContractId contractId)
        {
            AccountId = null;
            ContractId = contractId;
        }

		public AccountId? AccountId { get; }
		public ContractId? ContractId { get; }
        public virtual bool IsAccount { get => AccountId is not null; }
        public virtual bool IsContract { get => ContractId is not null; }

		/// <include file="HookEntityId.cs.xml" path='docs/member[@name="M:HookEntityId.ToProtobuf"]/*' />
		public virtual Proto.Services.HookEntityId ToProtobuf()
        {
			Proto.Services.HookEntityId proto = new ();

            if (AccountId is not null) proto.AccountId = AccountId.ToProtobuf();
            if (ContractId is not null) proto.ContractId = ContractId.ToProtobuf();

            return proto;
        }

        /// <include file="HookEntityId.cs.xml" path='docs/member[@name="M:HookEntityId.FromProtobuf(Proto.Services.HookEntityId)"]/*' />
        public static HookEntityId FromProtobuf(Proto.Services.HookEntityId proto)
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
