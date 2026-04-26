// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Contract;

using Org.BouncyCastle.Utilities.Encoders;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractNonceInfoTest
    {
        private readonly Proto.Services.ContractNonceInfo info = new Proto.Services.ContractNonceInfo
        {
			ContractId = new ContractId(0, 0, 1).ToProtobuf(),
			Nonce = 2,
		};

        public virtual void FromProtobuf()
        {
            Verifier.Verify(ContractNonceInfo.FromProtobuf(info).ToString());
        }

        public virtual void ToProtobuf()
        {
            Verifier.Verify(ContractNonceInfo.FromProtobuf(info).ToProtobuf());
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(ContractNonceInfo.FromProtobuf(info).ToBytes()));
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(ContractNonceInfo.FromBytes(info.ToByteArray()).ToString());
        }
    }
}