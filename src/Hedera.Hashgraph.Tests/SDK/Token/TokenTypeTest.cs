// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;

using Google.Protobuf.WellKnownTypes;

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
            SnapshotMatcher.Expect(tokenTypeFungible.ToString(), tokenTypeNonFungible.ToString()).ToMatchSnapshot();

        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect((Proto.TokenType)tokenTypeFungible, (Proto.TokenType)tokenTypeNonFungible).ToMatchSnapshot();
        }
    }
}