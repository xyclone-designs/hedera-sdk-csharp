// SPDX-License-Identifier: Apache-2.0
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Nio.Charset;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ContractLogInfoTest
    {
        private static readonly ContractLoginfo info = ContractLoginfo.NewBuilder().SetContractID(new ContractId(0, 0, 10).ToProtobuf()).SetBloom(ByteString.CopyFrom("bloom", StandardCharsets.UTF_8)).AddTopic(ByteString.CopyFrom("bloom", StandardCharsets.UTF_8)).SetData(ByteString.CopyFrom("data", StandardCharsets.UTF_8)).Build();
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
            SnapshotMatcher.Expect(ContractLogInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(ContractLogInfo.FromProtobuf(info).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        virtual void FromBytes()
        {
            SnapshotMatcher.Expect(ContractLogInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }

        virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(ContractLogInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }
    }
}