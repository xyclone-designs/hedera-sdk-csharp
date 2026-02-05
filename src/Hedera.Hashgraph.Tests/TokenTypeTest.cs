// SPDX-License-Identifier: Apache-2.0
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
    public class TokenTypeTest
    {
        private readonly TokenType tokenTypeFungible = TokenType.FUNGIBLE_COMMON;
        private readonly TokenType tokenTypeNonFungible = TokenType.NON_FUNGIBLE_UNIQUE;
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.TokenType.ValueOf(tokenTypeFungible).ToString(), com.hedera.hashgraph.sdk.TokenType.ValueOf(tokenTypeNonFungible).ToString()).ToMatchSnapshot();
        }

        virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.TokenType.ValueOf(tokenTypeFungible).ToProtobuf(), com.hedera.hashgraph.sdk.TokenType.ValueOf(tokenTypeNonFungible).ToProtobuf()).ToMatchSnapshot();
        }
    }
}