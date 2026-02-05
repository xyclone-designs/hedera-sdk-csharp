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

namespace Com.Hedera.Hashgraph.Sdk
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
            var returnList = new List<CustomFee>();
            returnList.Add(new CustomFixedFee().SetFeeCollectorAccountId(new AccountId(0, 0, 4322)).SetDenominatingTokenId(new TokenId(0, 0, 483902)).SetAmount(10));
            returnList.Add(new CustomFractionalFee().SetFeeCollectorAccountId(new AccountId(0, 0, 389042)).SetNumerator(3).SetDenominator(7).SetMin(3).SetMax(100));
            returnList.Add(new CustomRoyaltyFee().SetFeeCollectorAccountId(new AccountId(0, 0, 23423)).SetNumerator(5).SetDenominator(8).SetFallbackFee(new CustomFixedFee().SetDenominatingTokenId(new TokenId(0, 0, 483902)).SetAmount(10)));
            return returnList;
        }

        virtual void ShouldSerialize()
        {
            var originalCustomFeeList = SpawnCustomFeeListExample();
            byte[] customFee0Bytes = originalCustomFeeList[0].ToBytes();
            byte[] customFee1Bytes = originalCustomFeeList[1].ToBytes();
            byte[] customFee2Bytes = originalCustomFeeList[2].ToBytes();
            var copyCustomFeeList = new List<CustomFee>();
            copyCustomFeeList.Add(CustomFee.FromBytes(customFee0Bytes));
            copyCustomFeeList.Add(CustomFee.FromBytes(customFee1Bytes));
            copyCustomFeeList.Add(CustomFee.FromBytes(customFee2Bytes));
            Assert.Equal(originalCustomFeeList.ToString(), copyCustomFeeList.ToString());
            SnapshotMatcher.Expect(originalCustomFeeList.ToString()).ToMatchSnapshot();
        }

        virtual void DeepClone()
        {
            var originalCustomFeeList = SpawnCustomFeeListExample();
            var copyCustomFeeList = new List<CustomFee>();
            foreach (var fee in originalCustomFeeList)
            {
                copyCustomFeeList.Add(fee.DeepClone());
            }

            var originalCustomFeeListString = originalCustomFeeList.ToString();
            Assert.Equal(originalCustomFeeListString, copyCustomFeeList.ToString());

            // modifying clone doesn't affect original
            ((CustomFixedFee)copyCustomFeeList[0]).SetDenominatingTokenId(new TokenId(0, 0, 89803));
            Assert.Equal(originalCustomFeeListString, originalCustomFeeList.ToString());
            SnapshotMatcher.Expect(originalCustomFeeList.ToString()).ToMatchSnapshot();
        }
    }
}