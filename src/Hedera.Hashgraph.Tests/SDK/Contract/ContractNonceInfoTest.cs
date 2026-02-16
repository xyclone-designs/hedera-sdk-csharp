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

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractNonceInfoTest
    {
        private readonly Proto.ContractNonceInfo info = Proto.ContractNonceInfo.NewBuilder().SetContractId(new ContractId(0, 0, 1).ToProtobuf()).SetNonce(2).Build();
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
            SnapshotMatcher.Expect(ContractNonceInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(ContractNonceInfo.FromProtobuf(info).ToProtobuf()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(ContractNonceInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(ContractNonceInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }
    }
}