// SPDX-License-Identifier: Apache-2.0
using Proto;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenTypeTest
    {
        private readonly TokenType tokenTypeFungible = TokenType.FungibleCommon;
        private readonly TokenType tokenTypeNonFungible = TokenType.NonFungibleUnique;
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
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.TokenType.ValueOf(tokenTypeFungible).ToString(), com.hedera.hashgraph.sdk.TokenType.ValueOf(tokenTypeNonFungible).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.TokenType.ValueOf(tokenTypeFungible).ToProtobuf(), com.hedera.hashgraph.sdk.TokenType.ValueOf(tokenTypeNonFungible).ToProtobuf()).ToMatchSnapshot();
        }
    }
}