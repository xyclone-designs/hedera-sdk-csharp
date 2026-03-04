// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Contract;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractNonceInfoTest
    {
        private readonly Proto.ContractNonceInfo info = new Proto.ContractNonceInfo
        {
			ContractId = new ContractId(0, 0, 1).ToProtobuf(),
			Nonce = 2,
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