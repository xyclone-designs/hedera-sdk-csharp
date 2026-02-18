// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Org.BouncyCastle.Utilities.Encoders;
using System;

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    public class TokenNftInfoTest
    {
        static readonly DateTimeOffset creationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
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

        public virtual void ShouldSerialize()
        {
            var originalTokenInfo = SpawnTokenNftInfoExample(AccountId.FromString("8.9.10"));
            byte[] tokenInfoBytes = originalTokenInfo.ToBytes();
            var copyTokenInfo = TokenNftInfo.FromBytes(tokenInfoBytes);
            Assert.Equal(copyTokenInfo.ToString(), originalTokenInfo.ToString());
            SnapshotMatcher.Expect(originalTokenInfo.ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldSerializeNullSpender()
        {
            var originalTokenInfo = SpawnTokenNftInfoExample(null);
            byte[] tokenInfoBytes = originalTokenInfo.ToBytes();
            var copyTokenInfo = TokenNftInfo.FromBytes(tokenInfoBytes);
            Assert.Equal(copyTokenInfo.ToString(), originalTokenInfo.ToString());
            SnapshotMatcher.Expect(originalTokenInfo.ToString()).ToMatchSnapshot();
        }
    }
}