// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Token;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="CustomFixedFee.cs.xml" path='docs/member[@name="T:CustomFixedFee"]/*' />
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
        /// <include file="CustomFixedFee.cs.xml" path='docs/member[@name="M:CustomFixedFee.#ctor"]/*' />
        internal TokenId? DenominatingTokenId { get; set; }

        /// <include file="CustomFixedFee.cs.xml" path='docs/member[@name="M:CustomFixedFee.#ctor_2"]/*' />
        public CustomFixedFee() { }

		/// <include file="CustomFixedFee.cs.xml" path='docs/member[@name="M:CustomFixedFee.FromProtobuf(Proto.FixedFee)"]/*' />
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


        /// <include file="CustomFixedFee.cs.xml" path='docs/member[@name="M:CustomFixedFee.SetDenominatingTokenToSameToken"]/*' />
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

		/// <include file="CustomFixedFee.cs.xml" path='docs/member[@name="M:CustomFixedFee.ToFixedFeeProtobuf"]/*' />
		public virtual Proto.FixedFee ToFixedFeeProtobuf()
        {
			return new Proto.FixedFee
			{
				Amount = Amount,
				DenominatingTokenId = DenominatingTokenId?.ToProtobuf()
			};
        }
		/// <include file="CustomFixedFee.cs.xml" path='docs/member[@name="M:CustomFixedFee.ToFixedCustomFeeProtobuf"]/*' />
		public virtual Proto.FixedCustomFee ToFixedCustomFeeProtobuf()
		{
			return new Proto.FixedCustomFee
			{
                FixedFee = ToFixedFeeProtobuf()
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