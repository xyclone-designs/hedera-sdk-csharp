// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Contract;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractLogInfoTest
    {
        private static readonly Proto.ContractLoginfo info = new Proto.ContractLoginfo
        {
			ContractID = new ContractId(0, 0, 10).ToProtobuf(),
			Bloom = ByteString.CopyFromUtf8("bloom"),
			//Topic = [ByteString.CopyFromUtf8("bloom")],
			Data = ByteString.CopyFromUtf8("data"),
		};
        
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