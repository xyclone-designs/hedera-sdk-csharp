// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Contract;

using Org.BouncyCastle.Utilities.Encoders;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractLogInfoTest
    {
        private static readonly Proto.Services.ContractLoginfo info = new Proto.Services.ContractLoginfo
        {
			ContractId = new ContractId(0, 0, 10).ToProtobuf(),
			Bloom = ByteString.CopyFromUtf8("bloom"),
			//Topic = [ByteString.CopyFromUtf8("bloom")],
			Data = ByteString.CopyFromUtf8("data"),
		};

        public virtual void FromProtobuf()
        {
            Verifier.Verify(ContractLogInfo.FromProtobuf(info).ToString());
        }

        public virtual void ToProtobuf()
        {
            Verifier.Verify(ContractLogInfo.FromProtobuf(info).ToProtobuf().ToString());
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(ContractLogInfo.FromBytes(info.ToByteArray()).ToString());
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(ContractLogInfo.FromProtobuf(info).ToBytes()));
        }
    }
}