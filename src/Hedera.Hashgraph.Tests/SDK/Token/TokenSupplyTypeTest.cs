// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
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
    public class TokenSupplyTypeTest
    {
        private readonly TokenSupplyType tokenSupplyTypeInfinite = TokenSupplyType.INFINITE;
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
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.TokenSupplyType.ValueOf(tokenSupplyTypeInfinite).ToString(), com.hedera.hashgraph.sdk.TokenSupplyType.ValueOf(tokenSupplyTypeFinite).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.TokenSupplyType.ValueOf(tokenSupplyTypeInfinite).ToProtobuf(), com.hedera.hashgraph.sdk.TokenSupplyType.ValueOf(tokenSupplyTypeFinite).ToProtobuf()).ToMatchSnapshot();
        }

        public virtual void TokenSupplyTestToString()
        {
            Assert.Equal(com.hedera.hashgraph.sdk.TokenSupplyType.INFINITE.ToString(), "INFINITE");
            Assert.Equal(com.hedera.hashgraph.sdk.TokenSupplyType.Finite.ToString(), "FINITE");
        }
    }
}