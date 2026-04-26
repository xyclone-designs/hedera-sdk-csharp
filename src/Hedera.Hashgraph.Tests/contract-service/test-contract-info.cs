// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Networking;

using Google.Protobuf;

using VerifyXunit;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractInfoTest
    {
        private readonly Proto.Services.ContractGetInfoResponse.Types.ContractInfo info = new Proto.Services.ContractGetInfoResponse.Types.ContractInfo
        {
			ContractId = new ContractId(0, 0, 1).ToProtobuf(),
			AccountId = new AccountId(0, 0, 2).ToProtobuf(),
			ContractAccountId = "3",
			ExpirationTime = DateTimeOffset.UnixEpoch.AddMilliseconds(4).ToProtoTimestamp(),
			AutoRenewPeriod = TimeSpan.FromDays(5).ToProtoDuration(),
			Storage = 6,
			Memo = "7",
			Balance = 8,
			LedgerId = LedgerId.TESTNET.ToByteString()
		};

        public virtual void FromProtobuf()
        {
            Verifier.Verify(ContractInfo.FromProtobuf(info).ToString());
        }

        public virtual void ToProtobuf()
        {
            Verifier.Verify(ContractInfo.FromProtobuf(info).ToProtobuf());
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(ContractInfo.FromProtobuf(info).ToBytes()));
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(ContractInfo.FromBytes(info.ToByteArray()).ToString());
        }
    }
}