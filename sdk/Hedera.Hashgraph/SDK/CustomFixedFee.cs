
namespace Hedera.Hashgraph.SDK
{
	/**
     * Custom fixed fee utility class.
     * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#fixed-fee">Hedera Documentation</a>
     */
    public class CustomFixedFee : CustomFeeBase<CustomFixedFee> 
    {
		/**
         * The Shard, Realm, number of the tokens.
         */
		public TokenId? DenominatingTokenId { get; set; }
		public long Amount { get; set; }
		public Hbar AmountHbars 
        {
            get => Hbar.FromTinybars(Amount); 
            set
			{
				DenominatingTokenId = null;
				Amount = value.ToTinybars();
            }
        }


        /**
         * Constructor.
         */
        public CustomFixedFee() {}

		/**
         * Create a custom fixed fee from a fixed fee protobuf.
         *
         * @param fixedFee                  the fixed fee protobuf
         * @return                          the new custom fixed fee object
         */
		public static CustomFixedFee FromProtobuf(Proto.FixedFee fixedFee) 
        {
            return new CustomFixedFee
            {
                Amount = fixedFee.Amount,
				DenominatingTokenId = TokenId.FromProtobuf(fixedFee.DenominatingTokenId),

			};
        }

        /**
         * Assign the default token 0.0.0.
         *
         * @return {@code this}
         */
        public CustomFixedFee DenominatingTokenToSameToken() {
            DenominatingTokenId = new TokenId(0, 0, 0);
            return this;
        }

		/**
         * Convert to a protobuf.
         *
         * @return                          the protobuf converted object
         */
		public Proto.FixedFee ToFixedFeeProtobuf()
		{
			return new Proto.FixedFee
			{
				Amount = Amount,
				DenominatingTokenId = DenominatingTokenId?.ToProtobuf(),
			};
		}
		public Proto.FixedCustomFee ToTopicFeeProtobuf()
		{
			return new Proto.FixedCustomFee
			{
				FeeCollectorAccountId = FeeCollectorAccountId?.ToProtobuf(),
				FixedFee = new Proto.FixedFee
				{
					Amount = Amount,
					DenominatingTokenId = DenominatingTokenId?.ToProtobuf(),
				}
			};
		}
		public override Proto.CustomFee ToProtobuf()
		{
			return FinishToProtobuf(new Proto.CustomFee
			{
				FixedFee = ToFixedFeeProtobuf()
			});
		}
		public override CustomFixedFee DeepCloneSubclass()
		{
			return (CustomFixedFee)new CustomFixedFee
			{
				Amount = Amount,
				DenominatingTokenId = DenominatingTokenId,

			}.FinishDeepClone(this);
		}
		public override void ValidateChecksums(Client client) 
        {
            base.ValidateChecksums(client);

			DenominatingTokenId?.ValidateChecksum(client);
		}        
    }
}