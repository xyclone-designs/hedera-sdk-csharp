// SPDX-License-Identifier: Apache-2.0
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ContractNonceInfoTest
    {
        private readonly com.hedera.hashgraph.sdk.proto.ContractNonceInfo info = com.hedera.hashgraph.sdk.proto.ContractNonceInfo.NewBuilder().SetContractId(new ContractId(0, 0, 1).ToProtobuf()).SetNonce(2).Build();
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
            SnapshotMatcher.Expect(ContractNonceInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(ContractNonceInfo.FromProtobuf(info).ToProtobuf()).ToMatchSnapshot();
        }

        virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(ContractNonceInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }

        virtual void FromBytes()
        {
            SnapshotMatcher.Expect(ContractNonceInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }
    }
}