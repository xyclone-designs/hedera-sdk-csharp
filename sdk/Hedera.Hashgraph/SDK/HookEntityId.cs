using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * The ID of an entity using a hook.
     * <p>
     * This class represents the HookEntityId protobuf message, which can be either
     * an account ID or a contract ID.
     */
    public class HookEntityId 
    {
        /**
         * Create a HookEntityId with an account ID.
         *
         * @param accountId the account ID
         */
        public HookEntityId(AccountId accountId) 
        {
            AccountId = accountId;
            ContractId = null;
        }

        /**
         * Create a HookEntityId with a contract ID.
         *
         * @param contractId the contract ID
         */
        public HookEntityId(ContractId contractId)
        {
			AccountId = null;
			ContractId = contractId;
		}

		public AccountId? AccountId { get; }
		public ContractId? ContractId { get; }

		public bool IsAccountId { get => AccountId is not null; }
		public bool IsContractId { get => ContractId is not null; }

        /**
         * Convert this HookEntityId to a protobuf message.
         *
         * @return the protobuf HookEntityId
         */
        public Proto.HookEntityId ToProtobuf() {
			Proto.HookEntityId protobuf = new ();

            if (AccountId != null) protobuf.AccountId = AccountId.ToProtobuf();
			if (ContractId != null) protobuf.ContractId = ContractId.ToProtobuf();

			return protobuf;
        }

        /**
         * Create a HookEntityId from a protobuf message.
         *
         * @param proto the protobuf HookEntityId
         * @return a new HookEntityId instance
         */
        public static HookEntityId FromProtobuf(Proto.HookEntityId proto) 
        {
            return proto.AccountId is not null
                ? new HookEntityId(AccountId.FromProtobuf(proto.AccountId))
                : new HookEntityId(ContractId.FromProtobuf(proto.ContractId));
        }

        public override bool Equals(object? obj) 
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            HookEntityId that = (HookEntityId) obj;

            return 
                Equals(AccountId, that.AccountId) && 
                Equals(ContractId, that.ContractId);
        }
        public override int GetHashCode()
        {
			return HashCode.Combine(AccountId, ContractId);
		}
    }
}