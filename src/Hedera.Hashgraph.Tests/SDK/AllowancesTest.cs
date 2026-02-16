// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class AllowancesTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual TokenAllowance SpawnTokenAllowance()
        {
            return new TokenAllowance(TokenId.FromString("1.2.3"), AccountId.FromString("4.5.6"), AccountId.FromString("5.5.5"), 777);
        }

        public virtual TokenNftAllowance SpawnNftAllowance()
        {
            IList<long> serials = new List();
            serials.Add(123);
            serials.Add(456);
            return new TokenNftAllowance(TokenId.FromString("1.1.1"), AccountId.FromString("2.2.2"), AccountId.FromString("3.3.3"), null, serials, null);
        }

        public virtual TokenNftAllowance SpawnAllNftAllowance()
        {
            return new TokenNftAllowance(TokenId.FromString("1.1.1"), AccountId.FromString("2.2.2"), AccountId.FromString("3.3.3"), null, Collections.EmptyList(), true);
        }

        public virtual HbarAllowance SpawnHbarAllowance()
        {
            return new HbarAllowance(AccountId.FromString("1.1.1"), AccountId.FromString("2.2.2"), new Hbar(3));
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnHbarAllowance().ToString(), SpawnTokenAllowance().ToString(), SpawnNftAllowance().ToString(), SpawnAllNftAllowance().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytes()
        {
            var hbar1 = SpawnHbarAllowance();
            var token1 = SpawnTokenAllowance();
            var nft1 = SpawnNftAllowance();
            var allNft1 = SpawnAllNftAllowance();
            var hbar2 = HbarAllowance.FromBytes(hbar1.ToBytes());
            var token2 = TokenAllowance.FromBytes(token1.ToBytes());
            var nft2 = TokenNftAllowance.FromBytes(nft1.ToBytes());
            var allNft2 = TokenNftAllowance.FromBytes(allNft1.ToBytes());
            Assert.Equal(hbar2.ToString(), hbar1.ToString());
            Assert.Equal(token2.ToString(), token1.ToString());
            Assert.Equal(nft2.ToString(), nft1.ToString());
            Assert.Equal(allNft2.ToString(), allNft1.ToString());
        }
    }
}