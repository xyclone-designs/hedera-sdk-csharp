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
    public class TokenSupplyTypeTest
    {
        private readonly TokenSupplyType tokenSupplyTypeInfinite = TokenSupplyType.Infinite;
        private readonly TokenSupplyType tokenSupplyTypeFinite = TokenSupplyType.Finite;
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
            SnapshotMatcher.Expect(tokenSupplyTypeInfinite.ToString(), tokenSupplyTypeFinite.ToString()).ToMatchSnapshot();
		}

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect((Proto.TokenSupplyType)tokenSupplyTypeInfinite, (Proto.TokenSupplyType)tokenSupplyTypeFinite).ToMatchSnapshot();
        }

        public virtual void TokenSupplyTestToString()
        {
            Assert.Equal(TokenSupplyType.Infinite.ToString(), "INFINITE");
            Assert.Equal(TokenSupplyType.Finite.ToString(), "FINITE");
        }
    }
}