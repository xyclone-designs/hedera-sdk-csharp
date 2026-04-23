// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicInfo.cs.xml" path='docs/member[@name="T:TopicInfo"]/*' />
    public sealed class TopicInfo
    {
        private TopicInfo(TopicId topicId, string topicMemo, ByteString runningHash, ulong sequenceNumber, DateTimeOffset expirationTime, Key? adminKey, Key? submitKey, TimeSpan autoRenewPeriod, AccountId autoRenewAccountId, LedgerId ledgerId, Key? feeScheduleKey, IEnumerable<Key> feeExemptKeys, IEnumerable<CustomFixedFee> customFees)
        {
            TopicId = topicId;
            TopicMemo = topicMemo;
            RunningHash = runningHash;
            SequenceNumber = sequenceNumber;
            ExpirationTime = expirationTime;
            AdminKey = adminKey;
            SubmitKey = submitKey;
            AutoRenewPeriod = autoRenewPeriod;
            AutoRenewAccountId = autoRenewAccountId;
            LedgerId = ledgerId;
            FeeScheduleKey = feeScheduleKey;
            FeeExemptKeys = [ .. feeExemptKeys];
            CustomFees = [ .. customFees];
        }

		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="M:TopicInfo.FromBytes(System.Byte[])"]/*' />
		public static TopicInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.ConsensusGetTopicInfoResponse.Parser.ParseFrom(bytes));
		}
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="M:TopicInfo.FromProtobuf(Proto.Services.ConsensusGetTopicInfoResponse)"]/*' />
		public static TopicInfo FromProtobuf(Proto.Services.ConsensusGetTopicInfoResponse topicInfoResponse)
        {
            return new TopicInfo(TopicId.FromProtobuf(
                topicInfoResponse.TopicId),
				topicInfoResponse.TopicInfo.Memo, 
                topicInfoResponse.TopicInfo.RunningHash, 
                topicInfoResponse.TopicInfo.SequenceNumber,
                topicInfoResponse.TopicInfo.ExpirationTime.ToDateTimeOffset(),
				Key.FromProtobufKey(topicInfoResponse.TopicInfo.AdminKey),
				Key.FromProtobufKey(topicInfoResponse.TopicInfo.SubmitKey), 
                topicInfoResponse.TopicInfo.AutoRenewPeriod.ToTimeSpan(),
				AccountId.FromProtobuf(topicInfoResponse.TopicInfo.AutoRenewAccount), 
                LedgerId.FromByteString(topicInfoResponse.TopicInfo.LedgerId),
				Key.FromProtobufKey(topicInfoResponse.TopicInfo.FeeScheduleKey), 
                [.. topicInfoResponse.TopicInfo.FeeExemptKeyList.Select(_ => Key.FromProtobufKey(_)).OfType<Key>()], 
                [.. topicInfoResponse.TopicInfo.CustomFees.Select(_ => CustomFixedFee.FromProtobuf(_.FixedFee))]);
        }

		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.TopicId"]/*' />
		public TopicId TopicId { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.TopicMemo"]/*' />
		public string TopicMemo { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.RunningHash"]/*' />
		public ByteString RunningHash { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.SequenceNumber"]/*' />
		public ulong SequenceNumber { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.ExpirationTime"]/*' />
		public DateTimeOffset ExpirationTime { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.AdminKey"]/*' />
		public Key? AdminKey { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.SubmitKey"]/*' />
		public Key? SubmitKey { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.AutoRenewPeriod"]/*' />
		public TimeSpan AutoRenewPeriod { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.AutoRenewAccountId"]/*' />
		public AccountId AutoRenewAccountId { get; }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="P:TopicInfo.LedgerId"]/*' />
		public LedgerId LedgerId { get; }
		public Key? FeeScheduleKey { get; }
		public IList<Key> FeeExemptKeys { get; }
		public IList<CustomFixedFee> CustomFees { get; }

		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="M:TopicInfo.ToBytes"]/*' />
		public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <include file="TopicInfo.cs.xml" path='docs/member[@name="M:TopicInfo.ToProtobuf"]/*' />
		public Proto.Services.ConsensusGetTopicInfoResponse ToProtobuf()
		{
			Proto.Services.ConsensusGetTopicInfoResponse proto = new()
			{
				TopicId = TopicId.ToProtobuf(),
				TopicInfo = new Proto.Services.ConsensusTopicInfo
				{
					Memo = TopicMemo,
					RunningHash = RunningHash,
					SequenceNumber = SequenceNumber,
					ExpirationTime = ExpirationTime.ToProtoTimestamp(),
					AutoRenewPeriod = AutoRenewPeriod.ToProtoDuration(),
					LedgerId = LedgerId.ToByteString(),
					AutoRenewAccount = AutoRenewAccountId.ToProtobuf(),
				}
			};

			if (AdminKey is not null)
				proto.TopicInfo.AdminKey = AdminKey.ToProtobufKey();

			if (SubmitKey is not null)
				proto.TopicInfo.SubmitKey = SubmitKey.ToProtobufKey();

			if (FeeScheduleKey is not null)
				proto.TopicInfo.FeeScheduleKey = FeeScheduleKey.ToProtobufKey();

			proto.TopicInfo.CustomFees.AddRange(CustomFees.Select(_ => _.ToFixedCustomFeeProtobuf()));
            proto.TopicInfo.FeeExemptKeyList.AddRange(FeeExemptKeys.Select(_ => _.ToProtobufKey()));

			return proto;
		}
	}
}
