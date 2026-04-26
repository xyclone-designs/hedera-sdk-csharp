// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Networking;

using System;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountInfoTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly byte[] hash = [0, 1, 2];
        private static readonly Proto.Services.LiveHash liveHash = new ()
        {
			AccountId = new AccountId(0, 0, 10).ToProtobuf(),
			Duration = TimeSpan.FromDays(11).ToProtoDuration(),
			Hash = ByteString.CopyFrom(hash),

			// Keys = new Proto.KeyList([privateKey.GetPublicKey().ToProtobufKey()]) 
        };

        private static readonly Proto.Services.CryptoGetInfoResponse.Types.AccountInfo info = new()
        {
            AccountId = new AccountId(0, 0, 1).ToProtobuf(),
            Deleted = true,
            ProxyReceived = 2,
            Key = privateKey.GetPublicKey().ToProtobufKey(),
            Balance = 3,
            GenerateSendRecordThreshold = 4,
            GenerateReceiveRecordThreshold = 5,
            ReceiverSigRequired = true,
            ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(6).ToProtoTimestamp(),
            AutoRenewPeriod = TimeSpan.FromDays(7).ToProtoDuration(),
            ProxyAccountId = new AccountId(0, 0, 8).ToProtobuf(),
            LedgerId = LedgerId.PREVIEWNET.ToByteString(),
            EthereumNonce = 1001,
            // .AddLiveHashes(liveHash)
        };

        public virtual void FromProtobufWithOtherOptions()
        {
            Verifier.Verify(AccountInfo.FromProtobuf(info).ToString());
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(AccountInfo.FromBytes(info.ToByteArray()).ToString());
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(AccountInfo.FromBytes(info.ToByteArray()).ToBytes());
        }

        public virtual void ToProtobuf()
        {
            Verifier.Verify(AccountInfo.FromProtobuf(info).ToProtobuf().ToString());
        }
    }
}