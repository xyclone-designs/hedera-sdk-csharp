// SPDX-License-Identifier: Apache-2.0
using Com.Google.Protobuf;
using Proto;
using Io.Github.JsonSnapshot;
using Java.Nio.Charset;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Contract
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

        public virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(ContractLogInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(ContractLogInfo.FromProtobuf(info).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(ContractLogInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(ContractLogInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }
    }
}