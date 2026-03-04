// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountInfoTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly byte[] hash = [0, 1, 2];
        private static readonly Proto.LiveHash liveHash = new ()
        {
			AccountId = new AccountId(0, 0, 10).ToProtobuf(),
			Duration = TimeSpan.FromDays(11).ToProtoDuration(),
			Hash = ByteString.CopyFrom(hash),

			// Keys = new Proto.KeyList([privateKey.GetPublicKey().ToProtobufKey()]) 
        };

        private static readonly Proto.CryptoGetInfoResponse.Types.AccountInfo info = new()
        {
            AccountID = new AccountId(0, 0, 1).ToProtobuf(),
            Deleted = true,
            ProxyReceived = 2,
            Key = privateKey.GetPublicKey().ToProtobufKey(),
            Balance = 3,
            GenerateSendRecordThreshold = 4,
            GenerateReceiveRecordThreshold = 5,
            ReceiverSigRequired = true,
            ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(6).ToProtoTimestamp(),
            AutoRenewPeriod = TimeSpan.FromDays(7).ToProtoDuration(),
            ProxyAccountID = new AccountId(0, 0, 8).ToProtobuf(),
            LedgerId = LedgerId.PREVIEWNET.ToByteString(),
            EthereumNonce = 1001,
            // .AddLiveHashes(liveHash)
        };

		public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void FromProtobufWithOtherOptions()
        {
            SnapshotMatcher.Expect(AccountInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(AccountInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(AccountInfo.FromBytes(info.ToByteArray()).ToBytes()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(AccountInfo.FromProtobuf(info).ToProtobuf().ToString()).ToMatchSnapshot();
        }
    }
}