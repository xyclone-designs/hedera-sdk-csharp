// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class CustomFeeListTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private static IList<CustomFee> SpawnCustomFeeListExample()
        {
            return new List<CustomFee>
            {
				new CustomFixedFee { FeeCollectorAccountId = new AccountId(0, 0, 4322), DenominatingTokenId = new TokenId(0, 0, 483902), Amount = 10 },
			    new CustomFractionalFee { FeeCollectorAccountId = new AccountId(0, 0, 389042), Numerator = 3, Denominator = 7, Min = 3, Max = 100 },
			    new CustomRoyaltyFee { FeeCollectorAccountId = new AccountId(0, 0, 23423), Numerator = 5, Denominator = 8, FallbackFee = new CustomFixedFee { DenominatingTokenId = new TokenId(0, 0, 483902), Amount = 10 } },
			};
        }

        public virtual void ShouldSerialize()
        {
            var originalCustomFeeList = SpawnCustomFeeListExample();
            byte[] customFee0Bytes = originalCustomFeeList[0].ToBytes();
            byte[] customFee1Bytes = originalCustomFeeList[1].ToBytes();
            byte[] customFee2Bytes = originalCustomFeeList[2].ToBytes();
            var copyCustomFeeList = new List<CustomFee>
            {
                CustomFee.FromBytes(customFee0Bytes),
                CustomFee.FromBytes(customFee1Bytes),
                CustomFee.FromBytes(customFee2Bytes),
            };
            
            Assert.Equal(originalCustomFeeList.ToString(), copyCustomFeeList.ToString());

            SnapshotMatcher.Expect(originalCustomFeeList.ToString()).ToMatchSnapshot();
        }

        public virtual void DeepClone()
        {
            var originalCustomFeeList = SpawnCustomFeeListExample();
            var copyCustomFeeList = new List<CustomFee>();

            foreach (var fee in originalCustomFeeList)
				copyCustomFeeList.Add(fee.DeepClone());

			var originalCustomFeeListString = originalCustomFeeList.ToString();
            Assert.Equal(originalCustomFeeListString, copyCustomFeeList.ToString());

            // modifying clone doesn't affect original
            ((CustomFixedFee)copyCustomFeeList[0]).DenominatingTokenId = new TokenId(0, 0, 89803);
            Assert.Equal(originalCustomFeeListString, originalCustomFeeList.ToString());

            SnapshotMatcher.Expect(originalCustomFeeList.ToString()).ToMatchSnapshot();
        }
    }
}