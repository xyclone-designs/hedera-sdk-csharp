// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Proto;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class CustomFixedFeeTest
    {
        private static readonly bool allCollectorsAreExempt = true;
        private static readonly AccountId feeCollectorAccountId = new AccountId(1, 2, 3);
        private static readonly long amount = 4;
        private static readonly TokenId tokenId = new TokenId(5, 6, 7);
        private readonly FixedFee fee = FixedFee.NewBuilder().SetAmount(amount).SetDenominatingTokenId(tokenId.ToProtobuf()).Build();
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
            var customFixedFee = new CustomFixedFee().SetFeeCollectorAccountId(feeCollectorAccountId).SetAllCollectorsAreExempt(allCollectorsAreExempt);
            var clonedCustomFixedFee = customFixedFee.DeepCloneSubclass();
            Assert.Equal(clonedCustomFixedFee.GetFeeCollectorAccountId(), feeCollectorAccountId);
            Assert.Equal(clonedCustomFixedFee.GetAllCollectorsAreExempt(), allCollectorsAreExempt);
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
            var customFixedFee1 = new CustomFixedFee().SetAmount(amount);
            var customFixedFee2 = new CustomFixedFee().SetHbarAmount(Hbar.FromTinybars(amount));
            Assert.Equal(customFixedFee1.GetAmount(), amount);
            Assert.Equal(customFixedFee2.GetHbarAmount().ToTinybars(), amount);
            Assert.Equal(customFixedFee1.GetHbarAmount().ToTinybars(), customFixedFee2.GetAmount());
        }

        public virtual void GetSetDenominatingToken()
        {
            var customFixedFee = new CustomFixedFee().SetDenominatingTokenId(tokenId);
            Assert.Equal(customFixedFee.GetDenominatingTokenId(), tokenId);
        }

        public virtual void SetSentinelValueToken()
        {
            var customFixedFee = new CustomFixedFee().SetDenominatingTokenToSameToken();
            Assert.Equal(customFixedFee.GetDenominatingTokenId(), new TokenId(0, 0, 0));
        }
    }
}