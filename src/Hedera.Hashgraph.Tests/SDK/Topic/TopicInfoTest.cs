// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Org.BouncyCastle.Utilities.Encoders;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Account;

using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    public class TopicInfoTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PrivateKey feeScheduleKey = PrivateKey.FromString("302e020100300506032b657004220420aabbccddeeff00112233445566778899aabbccddeeff00112233445566778899");
        private static readonly List<PrivateKey> feeExemptKeys = 
        [
            PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10"), 
            PrivateKey.FromString("302e020100300506032b657004220420aabbccddeeff00112233445566778899aabbccddeeff00112233445566778899")
        ];
        private static readonly byte[] hash = [2];
        private static readonly List<CustomFixedFee> customFees = 
        [
            new CustomFixedFee 
            {
                Amount = 100,
                DenominatingTokenId = new TokenId(0, 0, 0)
            } 
        ];
        private static readonly Proto.ConsensusGetTopicInfoResponse info = new Proto.ConsensusGetTopicInfoResponse
        {
            TopicInfo = new Proto.ConsensusTopicInfo
            {
                Memo = "1",
                RunningHash = ByteString.CopyFrom(hash),
                SequenceNumber = 3,
                ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(4).ToProtoTimestamp(),
                AutoRenewPeriod = TimeSpan.FromDays(5).ToProtoDuration(),
                AdminKey = privateKey.GetPublicKey().ToProtobufKey(),
                SubmitKey = privateKey.GetPublicKey().ToProtobufKey(),
                FeeScheduleKey = feeScheduleKey.GetPublicKey().ToProtobufKey(),
                AutoRenewAccount = new AccountId(0, 0, 4).ToProtobuf(),
                LedgerId = LedgerId.TESTNET.ToByteString(),
                FeeExemptKeyList = [.. feeExemptKeys.Select(_ = _.GetPublicKey().ToProtobufKey())],
                CustomFees = [.. customFees.Select(_ => _.ToTopicFeeProtobuf())],
            }
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
            SnapshotMatcher.Expect(TopicInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(TopicInfo.FromProtobuf(info).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(TopicInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(TopicInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }
    }
}