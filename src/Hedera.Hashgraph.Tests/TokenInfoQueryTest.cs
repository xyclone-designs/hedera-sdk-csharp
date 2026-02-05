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
    public class TokenInfoQueryTest
    {
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
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
            new TokenInfoQuery().SetTokenId(testTokenId).SetMaxQueryPayment(Hbar.FromTinybars(100000)).OnMakeRequest(builder, QueryHeader.NewBuilder().Build());
            SnapshotMatcher.Expect(builder.Build().ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        virtual void GetSetTokenId()
        {
            var tokenInfoQuery = new TokenInfoQuery().SetTokenId(testTokenId);
            Assert.Equal(tokenInfoQuery.GetTokenId(), testTokenId);
        }
    }
}