// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Token;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// Custom fixed fee utility class.
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#fixed-fee">Hedera Documentation</a>
    /// </summary>
    public class CustomFixedFee : CustomFeeBase<CustomFixedFee>
    {
        public long Amount { get; set; }
        public Hbar AmountHbar 
        {
            get => Hbar.FromTinybars(Amount);
            set
            {
				DenominatingTokenId = null;
				Amount = value.ToTinybars();
			}
        }
        /// <summary>
        /// The shard, realm, number of the tokens.
        /// </summary>
        private TokenId? DenominatingTokenId { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomFixedFee() { }

		/// <summary>
		/// Create a custom fixed fee from a fixed fee protobuf.
		/// </summary>
		/// <param name="fixedFee">the fixed fee protobuf</param>
		/// <returns>                         the new custom fixed fee object</returns>
		public static CustomFixedFee FromProtobuf(Proto.FixedFee fixedFee)
        {
			return new CustomFixedFee
			{
				Amount = fixedFee.Amount,
				DenominatingTokenId = TokenId.FromProtobuf(fixedFee.DenominatingTokenId)
			};
        }

		public virtual Proto.FixedCustomFee ToTopicFeeProtobuf()
        {
            Proto.FixedCustomFee proto = new()
            {
                FeeCollectorAccountId = FeeCollectorAccountId?.ToProtobuf(),
                FixedFee = new Proto.FixedFee { },
            };

            if (DenominatingTokenId != null)
				proto.FixedFee.DenominatingTokenId = DenominatingTokenId.ToProtobuf();

			return proto;
        }

		public override CustomFixedFee DeepCloneSubclass()
        {
            return new CustomFixedFee
            {
				Amount = Amount,
				DenominatingTokenId = DenominatingTokenId,

			}.FinishDeepClone(this);
        }


        /// <summary>
        /// Assign the default token 0.0.0.
        /// </summary>
        /// <returns>{@code this}</returns>
        public virtual CustomFixedFee SetDenominatingTokenToSameToken()
        {
            DenominatingTokenId = new TokenId(0, 0, 0);
            return this;
        }

		public override void ValidateChecksums(Client client)
        {
            base.ValidateChecksums(client);

			DenominatingTokenId?.ValidateChecksum(client);
		}

		/// <summary>
		/// Convert to a protobuf.
		/// </summary>
		/// <returns>                         the protobuf converted object</returns>
		public virtual Proto.FixedFee ToFixedFeeProtobuf()
        {
			return new Proto.FixedFee
			{
				Amount = Amount,
				DenominatingTokenId = DenominatingTokenId?.ToProtobuf()
			};
        }

        public override Proto.CustomFee ToProtobuf()
        {
            return FinishToProtobuf(new Proto.CustomFee
            {
				FixedFee = ToFixedFeeProtobuf()
			});
        }
    }
}