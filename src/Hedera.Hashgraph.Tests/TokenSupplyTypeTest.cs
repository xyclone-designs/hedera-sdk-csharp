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
    public class TokenSupplyTypeTest
    {
        private readonly TokenSupplyType tokenSupplyTypeInfinite = TokenSupplyType.INFINITE;
        private readonly TokenSupplyType tokenSupplyTypeFinite = TokenSupplyType.FINITE;
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
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.TokenSupplyType.ValueOf(tokenSupplyTypeInfinite).ToString(), com.hedera.hashgraph.sdk.TokenSupplyType.ValueOf(tokenSupplyTypeFinite).ToString()).ToMatchSnapshot();
        }

        virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.TokenSupplyType.ValueOf(tokenSupplyTypeInfinite).ToProtobuf(), com.hedera.hashgraph.sdk.TokenSupplyType.ValueOf(tokenSupplyTypeFinite).ToProtobuf()).ToMatchSnapshot();
        }

        virtual void TokenSupplyTestToString()
        {
            AssertThat(com.hedera.hashgraph.sdk.TokenSupplyType.INFINITE).HasToString("INFINITE");
            AssertThat(com.hedera.hashgraph.sdk.TokenSupplyType.FINITE).HasToString("FINITE");
        }
    }
}