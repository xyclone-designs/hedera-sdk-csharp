// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;

using Org.BouncyCastle.Utilities.Encoders;

using System;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.File
{
    public class FileInfoTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly Proto.FileGetInfoResponse.Types.FileInfo info = new ()
        {
			FileID = new FileId(0, 0, 1).ToProtobuf(),
			Size = 2,
			ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(3).ToProtoTimestamp(),
			Deleted = true,
			// Keys = [.. keys],
			LedgerId = LedgerId.MAINNET.ToByteString(),
		};

        public virtual void FromProtobuf()
        {
            Verifier.Verify(FileInfo.FromProtobuf(info).ToString());
        }

        public virtual void ToProtobuf()
        {
            Verifier.Verify(FileInfo.FromProtobuf(info).ToProtobuf().ToString());
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(FileInfo.FromBytes(info.ToByteArray()).ToString());
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(FileInfo.FromProtobuf(info).ToBytes()));
        }
    }
}