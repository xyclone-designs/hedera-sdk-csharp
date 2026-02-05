// SPDX-License-Identifier: Apache-2.0
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TopicInfoTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PrivateKey feeScheduleKey = PrivateKey.FromString("302e020100300506032b657004220420aabbccddeeff00112233445566778899aabbccddeeff00112233445566778899");
        private static readonly IList<PrivateKey> feeExemptKeys = List.Of(PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10"), PrivateKey.FromString("302e020100300506032b657004220420aabbccddeeff00112233445566778899aabbccddeeff00112233445566778899"));
        private static readonly byte[] hash = new[]
        {
            2
        };
        private static readonly IList<CustomFixedFee> customFees = List.Of(new CustomFixedFee().SetAmount(100).SetDenominatingTokenId(new TokenId(0, 0, 0)));
        private static readonly ConsensusGetTopicInfoResponse info = ConsensusGetTopicInfoResponse.NewBuilder().SetTopicInfo(ConsensusTopicInfo.NewBuilder().SetMemo("1").SetRunningHash(ByteString.CopyFrom(hash)).SetSequenceNumber(3).SetExpirationTime(InstantConverter.ToProtobuf(Instant.OfEpochMilli(4))).SetAutoRenewPeriod(DurationConverter.ToProtobuf(Duration.OfDays(5))).SetAdminKey(privateKey.GetPublicKey().ToProtobufKey()).SetSubmitKey(privateKey.GetPublicKey().ToProtobufKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey().ToProtobufKey()).AddAllFeeExemptKeyList(feeExemptKeys.Stream().Map((k) => k.GetPublicKey().ToProtobufKey()).ToList()).AddAllCustomFees(customFees.Stream().Map(CustomFixedFee.ToTopicFeeProtobuf()).ToList()).SetAutoRenewAccount(new AccountId(0, 0, 4).ToProtobuf()).SetLedgerId(LedgerId.TESTNET.ToByteString())).Build();
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(TopicInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(TopicInfo.FromProtobuf(info).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        virtual void FromBytes()
        {
            SnapshotMatcher.Expect(TopicInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }

        virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(TopicInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }
    }
}