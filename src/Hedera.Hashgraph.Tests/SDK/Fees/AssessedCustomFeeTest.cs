// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Io.Github.JsonSnapshot;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class AssessedCustomFeeTest
    {
        private static readonly int amount = 1;
        private static readonly TokenId tokenId = new TokenId(2, 3, 4);
        private static readonly AccountId feeCollector = new AccountId(5, 6, 7);
        private static readonly List<AccountId> payerAccountIds = List.Of(new AccountId(8, 9, 10), new AccountId(11, 12, 13), new AccountId(14, 15, 16));
        private readonly Proto.AssessedCustomFee fee = Proto.AssessedCustomFee.NewBuilder().SetAmount(amount).SetTokenId(tokenId.ToProtobuf()).SetFeeCollectorAccountId(feeCollector.ToProtobuf()).AddAllEffectivePayerAccountId(payerAccountIds.Stream().Map(AccountId.ToProtobuf()).ToList()).Build();
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual AssessedCustomFee SpawnAssessedCustomFeeExample()
        {
            return new AssessedCustomFee(201, TokenId.FromString("1.2.3"), AccountId.FromString("4.5.6"), List.Of(AccountId.FromString("0.0.1"), AccountId.FromString("0.0.2"), AccountId.FromString("0.0.3")));
        }

        public virtual void ShouldSerialize()
        {
            var originalAssessedCustomFee = SpawnAssessedCustomFeeExample();
            byte[] assessedCustomFeeBytes = originalAssessedCustomFee.ToBytes();
            var copyAssessedCustomFee = AssessedCustomFee.FromBytes(assessedCustomFeeBytes);
            Assert.Equal(originalAssessedCustomFee.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), copyAssessedCustomFee.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalAssessedCustomFee.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        public virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(AssessedCustomFee.FromProtobuf(fee).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(AssessedCustomFee.FromProtobuf(fee).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytes()
        {
            var assessedCustomFee = SpawnAssessedCustomFeeExample();
            var tx2 = AssessedCustomFee.FromBytes(assessedCustomFee.ToBytes());
            Assert.Equal(tx2.ToString(), assessedCustomFee.ToString());
        }
    }
}