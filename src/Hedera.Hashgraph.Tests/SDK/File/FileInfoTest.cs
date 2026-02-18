// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK.File
{
    public class FileInfoTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly Proto.FileGetInfoResponse.Types.FileInfo info = new ()
        {
			FileID = new FileId(0, 0, 1).ToProtobuf(),
			Size = 2,
			ExpirationTime = InstantConverter.ToProtobuf(Instant.OfEpochMilli(3)),
			Deleted = true,
			Keys = Proto.KeyList.Parser.ParseFrom(Keys),
			LedgerId = LedgerId.MAINNET.ToByteString(),
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
            SnapshotMatcher.Expect(FileInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(FileInfo.FromProtobuf(info).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(FileInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(FileInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }
    }
}