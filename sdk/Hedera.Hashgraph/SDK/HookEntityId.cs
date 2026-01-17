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
        Proto.HookEntityId ToProtobuf() {
            var builder = Proto.HookEntityId.newBuilder();

            if (accountId != null) {
                builder.setAccountId(accountId.ToProtobuf());
            } else {
                builder.setContractId(contractId.ToProtobuf());
            }

            return builder.build();
        }

        /**
         * Create a HookEntityId from a protobuf message.
         *
         * @param proto the protobuf HookEntityId
         * @return a new HookEntityId instance
         */
        static HookEntityId FromProtobuf(Proto.HookEntityId proto) 
        {
            return proto.HasAccountId()
                ? new HookEntityId(AccountId.FromProtobuf(proto.getAccountId())
                : new HookEntityId(ContractId.FromProtobuf(proto.getContractId()));
        }

        public override bool Equals(object obj) 
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
        public override string ToString() {
            return MoreObjects
                .ToStringHelper(this)
                    .Add("accountId", AccountId)
                    .Add("contractId", ContractId)
                    .ToString();
        }
    }

}