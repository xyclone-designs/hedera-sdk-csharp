// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Token;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    /// <include file="test-fees-custom-fixed.cs.xml" path="docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Fees.CustomFixedFeeTest"]" />
    public class CustomFixedFeeTest
    {
        private static readonly bool allCollectorsAreExempt = true;
        private static readonly AccountId feeCollectorAccountId = new AccountId(1, 2, 3);
        private static readonly long amount = 4;
        private static readonly TokenId tokenId = new TokenId(5, 6, 7);
        private readonly Proto.Services.FixedFee fee = new Proto.Services.FixedFee
        {
			Amount = amount,
			DenominatingTokenId = tokenId.ToProtobuf(),
		};

        public virtual void FromProtobuf()
        {
            Verifier.Verify(CustomFixedFee.FromProtobuf(fee).ToString());
        }
        [Fact]
        /// <include file="test-fees-custom-fixed.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Fees.CustomFixedFeeTest.DeepCloneSubclass"]" />
        public virtual void DeepCloneSubclass()
        {
            var customFixedFee = new CustomFixedFee
            {
                FeeCollectorAccountId = feeCollectorAccountId,
                AllCollectorsAreExempt = allCollectorsAreExempt
            };
            var clonedCustomFixedFee = customFixedFee.DeepCloneSubclass();

            Assert.Equal(clonedCustomFixedFee.FeeCollectorAccountId, feeCollectorAccountId);
            Assert.Equal(clonedCustomFixedFee.AllCollectorsAreExempt, allCollectorsAreExempt);
        }

        public virtual void ToProtobuf()
        {
            Verifier.Verify(CustomFixedFee.FromProtobuf(fee).ToProtobuf().ToString());
        }

        public virtual void ToFixedFeeProtobuf()
        {
            Verifier.Verify(CustomFixedFee.FromProtobuf(fee).ToFixedFeeProtobuf().ToString());
        }
        [Fact]
        /// <include file="test-fees-custom-fixed.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Fees.CustomFixedFeeTest.GetSetAmount"]" />
        public virtual void GetSetAmount()
        {
            var customFixedFee1 = new CustomFixedFee { Amount = amount };
            var customFixedFee2 = new CustomFixedFee { AmountHbar = Hbar.FromTinybars(amount) };

            Assert.Equal(customFixedFee1.Amount, amount);
            Assert.Equal(customFixedFee2.AmountHbar.ToTinybars(), amount);
            Assert.Equal(customFixedFee1.AmountHbar.ToTinybars(), customFixedFee2.Amount);
        }
        [Fact]
        /// <include file="test-fees-custom-fixed.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Fees.CustomFixedFeeTest.GetSetDenominatingToken"]" />
        public virtual void GetSetDenominatingToken()
        {
            var customFixedFee = new CustomFixedFee
            {
				DenominatingTokenId = tokenId
			};

            Assert.Equal(customFixedFee.DenominatingTokenId, tokenId);
        }
        [Fact]
        /// <include file="test-fees-custom-fixed.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Fees.CustomFixedFeeTest.SetSentinelValueToken"]" />
        public virtual void SetSentinelValueToken()
        {
            var customFixedFee = new CustomFixedFee().SetDenominatingTokenToSameToken();

            Assert.Equal(customFixedFee.DenominatingTokenId, new TokenId(0, 0, 0));
        }
    }
}
