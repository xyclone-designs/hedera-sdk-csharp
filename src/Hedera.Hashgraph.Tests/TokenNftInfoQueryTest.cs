// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TokenNftInfoQueryTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ShouldSerialize()
        {
            var builder = com.hedera.hashgraph.sdk.proto.Query.NewBuilder();
            new TokenNftInfoQuery().SetNftId(TokenId.FromString("0.0.5005").Nft(101)).SetMaxQueryPayment(Hbar.FromTinybars(100000)).OnMakeRequest(builder, QueryHeader.NewBuilder().Build());
            SnapshotMatcher.Expect(builder.Build().ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        virtual void PropertiesTest()
        {
            var tokenId = TokenId.FromString("0.0.5005");
            var query = new TokenNftInfoQuery().ByAccountId(AccountId.FromString("0.0.123")).ByTokenId(tokenId).SetStart(5).SetEnd(8).SetNftId(tokenId.Nft(101)).SetMaxQueryPayment(Hbar.FromTinybars(100000));
            AssertThat(query.GetNftId()).HasToString("0.0.5005/101");
            Assert.Equal(query.GetTokenId(), tokenId);
            AssertThat(query.GetAccountId()).HasToString("0.0.123");
            Assert.Equal(query.GetStart(), 5);
            Assert.Equal(query.GetEnd(), 8);
        }
    }
}