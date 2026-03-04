// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using System.Text.RegularExpressions;
using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class AssessedCustomFeeTest
    {
        private static readonly int amount = 1;
        private static readonly TokenId tokenId = new TokenId(2, 3, 4);
        private static readonly AccountId feeCollector = new AccountId(5, 6, 7);
        private static readonly List<AccountId> payerAccountIds = [new AccountId(8, 9, 10), new AccountId(11, 12, 13), new AccountId(14, 15, 16)];
        private readonly Proto.AssessedCustomFee fee = new Proto.AssessedCustomFee
        {
			Amount = amount,
			TokenId = tokenId.ToProtobuf(),
			FeeCollectorAccountId = feeCollector.ToProtobuf(),
            // EffectivePayerAccountId = [payerAccountIds.Select(_ => _.ToProtobuf())]
        };
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
            return new AssessedCustomFee(201, TokenId.FromString("1.2.3"), AccountId.FromString("4.5.6"), [ AccountId.FromString("0.0.1"), AccountId.FromString("0.0.2"), AccountId.FromString("0.0.3") ]);
        }

        public virtual void ShouldSerialize()
        {
            var originalAssessedCustomFee = SpawnAssessedCustomFeeExample();
            byte[] assessedCustomFeeBytes = originalAssessedCustomFee.ToBytes();
            var copyAssessedCustomFee = AssessedCustomFee.FromBytes(assessedCustomFeeBytes);
            
            Assert.Equal(Regex.Replace(originalAssessedCustomFee.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(copyAssessedCustomFee.ToString(), "@[A-Za-z0-9]+", ""));
            
            SnapshotMatcher.Expect(Regex.Replace(originalAssessedCustomFee.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
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