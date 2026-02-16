// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountInfoTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly byte[] hash = new[]
        {
            0,
            1,
            2
        };
        private static readonly LiveHash liveHash = LiveHash.NewBuilder().SetAccountId(new AccountId(0, 0, 10).ToProtobuf()).SetDuration(DurationConverter.ToProtobuf(Duration.OfDays(11))).SetHash(ByteString.CopyFrom(hash)).SetKeys(KeyList.NewBuilder().AddKeys(privateKey.GetPublicKey().ToProtobufKey())).Build();
        private static readonly CryptoGetInfoResponse.AccountInfo info = CryptoGetInfoResponse.AccountInfo.NewBuilder().SetAccountID(new AccountId(0, 0, 1).ToProtobuf()).SetDeleted(true).SetProxyReceived(2).SetKey(privateKey.GetPublicKey().ToProtobufKey()).SetBalance(3).SetGenerateSendRecordThreshold(4).SetGenerateReceiveRecordThreshold(5).SetReceiverSigRequired(true).SetExpirationTime(InstantConverter.ToProtobuf(Instant.OfEpochMilli(6))).SetAutoRenewPeriod(DurationConverter.ToProtobuf(Duration.OfDays(7))).SetProxyAccountID(new AccountId(0, 0, 8).ToProtobuf()).AddLiveHashes(liveHash).SetLedgerId(LedgerId.PREVIEWNET.ToByteString()).SetEthereumNonce(1001).Build();
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