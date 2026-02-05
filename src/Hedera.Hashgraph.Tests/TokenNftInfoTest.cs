// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Io.Github.JsonSnapshot;
using Java.Time;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TokenNftInfoTest
    {
        static readonly Instant creationTime = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private static TokenNftInfo SpawnTokenNftInfoExample(AccountId spenderAccountId)
        {
            return new TokenNftInfo(TokenId.FromString("1.2.3").Nft(4), AccountId.FromString("5.6.7"), creationTime, Hex.Decode("deadbeef"), LedgerId.MAINNET, spenderAccountId);
        }

        virtual void ShouldSerialize()
        {
            var originalTokenInfo = SpawnTokenNftInfoExample(AccountId.FromString("8.9.10"));
            byte[] tokenInfoBytes = originalTokenInfo.ToBytes();
            var copyTokenInfo = TokenNftInfo.FromBytes(tokenInfoBytes);
            Assert.Equal(copyTokenInfo.ToString(), originalTokenInfo.ToString());
            SnapshotMatcher.Expect(originalTokenInfo.ToString()).ToMatchSnapshot();
        }

        virtual void ShouldSerializeNullSpender()
        {
            var originalTokenInfo = SpawnTokenNftInfoExample(null);
            byte[] tokenInfoBytes = originalTokenInfo.ToBytes();
            var copyTokenInfo = TokenNftInfo.FromBytes(tokenInfoBytes);
            Assert.Equal(copyTokenInfo.ToString(), originalTokenInfo.ToString());
            SnapshotMatcher.Expect(originalTokenInfo.ToString()).ToMatchSnapshot();
        }
    }
}