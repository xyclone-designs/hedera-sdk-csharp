// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Token;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class CustomFixedFeeTest
    {
        private static readonly bool allCollectorsAreExempt = true;
        private static readonly AccountId feeCollectorAccountId = new AccountId(1, 2, 3);
        private static readonly long amount = 4;
        private static readonly TokenId tokenId = new TokenId(5, 6, 7);
        private readonly Proto.FixedFee fee = new Proto.FixedFee
        {
			Amount = amount,
			DenominatingTokenId = tokenId.ToProtobuf(),
		};
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(CustomFixedFee.FromProtobuf(fee).ToString()).ToMatchSnapshot();
        }

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
            SnapshotMatcher.Expect(CustomFixedFee.FromProtobuf(fee).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void ToFixedFeeProtobuf()
        {
            SnapshotMatcher.Expect(CustomFixedFee.FromProtobuf(fee).ToFixedFeeProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void GetSetAmount()
        {
            var customFixedFee1 = new CustomFixedFee { Amount = amount };
            var customFixedFee2 = new CustomFixedFee { AmountHbar = Hbar.FromTinybars(amount) };

            Assert.Equal(customFixedFee1.Amount, amount);
            Assert.Equal(customFixedFee2.AmountHbar.ToTinybars(), amount);
            Assert.Equal(customFixedFee1.AmountHbar.ToTinybars(), customFixedFee2.Amount);
        }

        public virtual void GetSetDenominatingToken()
        {
            var customFixedFee = new CustomFixedFee
            {
				DenominatingTokenId = tokenId
			};

            Assert.Equal(customFixedFee.DenominatingTokenId, tokenId);
        }

        public virtual void SetSentinelValueToken()
        {
            var customFixedFee = new CustomFixedFee().SetDenominatingTokenToSameToken();

            Assert.Equal(customFixedFee.DenominatingTokenId, new TokenId(0, 0, 0));
        }
    }
}