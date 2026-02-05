// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Current state of a topic.
    /// </summary>
    public sealed class TopicInfo
    {
        private TopicInfo(TopicId topicId, string topicMemo, ByteString runningHash, ulong sequenceNumber, Timestamp expirationTime, Key adminKey, Key submitKey, Duration autoRenewPeriod, AccountId autoRenewAccountId, LedgerId ledgerId, Key feeScheduleKey, IList<Key> feeExemptKeys, IList<CustomFixedFee> customFees)
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
            FeeExemptKeys = feeExemptKeys;
            CustomFees = customFees;
        }

		/// <summary>
		/// Create a topic info object from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new topic info object</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static TopicInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.ConsensusGetTopicInfoResponse.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a topic info object from a protobuf.
		/// </summary>
		/// <param name="topicInfoResponse">the protobuf</param>
		/// <returns>                         the new topic info object</returns>
		public static TopicInfo FromProtobuf(Proto.ConsensusGetTopicInfoResponse topicInfoResponse)
        {
            return new TopicInfo(TopicId.FromProtobuf(
                topicInfoResponse.TopicID),
				topicInfoResponse.TopicInfo.Memo, 
                topicInfoResponse.TopicInfo.RunningHash, 
                topicInfoResponse.TopicInfo.SequenceNumber,
                Utils.TimestampConverter.FromProtobuf(topicInfoResponse.TopicInfo.ExpirationTime),
				Key.FromProtobufKey(topicInfoResponse.TopicInfo.AdminKey),
				Key.FromProtobufKey(topicInfoResponse.TopicInfo.SubmitKey), 
                Utils.DurationConverter.FromProtobuf(topicInfoResponse.TopicInfo.AutoRenewPeriod),
				AccountId.FromProtobuf(topicInfoResponse.TopicInfo.AutoRenewAccount), 
                LedgerId.FromByteString(topicInfoResponse.TopicInfo.LedgerId),
				Key.FromProtobufKey(topicInfoResponse.TopicInfo.FeeScheduleKey), 
                [.. topicInfoResponse.TopicInfo.FeeExemptKeyList.Select(_ => Key.FromProtobufKey(_))], 
                [.. topicInfoResponse.TopicInfo.CustomFees.Select(_ => CustomFixedFee.FromProtobuf(_.FixedFee))]);
        }

		/// <summary>
		/// The ID of the topic for which information is requested.
		/// </summary>
		public TopicId TopicId { get; }
		/// <summary>
		/// Short publicly visible memo about the topic. No guarantee of uniqueness.
		/// </summary>
		public string TopicMemo { get; }
		/// <summary>
		/// SHA-384 running hash of (previousRunningHash, topicId, consensusTimestamp, sequenceNumber, message).
		/// </summary>
		public ByteString RunningHash { get; }
		/// <summary>
		/// Sequence number (starting at 1 for the first submitMessage) of messages on the topic.
		/// </summary>
		public ulong SequenceNumber { get; }
		/// <summary>
		/// Effective consensus timestamp at (and after) which submitMessage calls will no longer succeed on the topic.
		/// </summary>
		public Timestamp ExpirationTime { get; }
		/// <summary>
		/// Access control for update/delete of the topic. Null if there is no key.
		/// </summary>
		public Key AdminKey { get; }
		/// <summary>
		/// Access control for ConsensusService.submitMessage. Null if there is no key.
		/// </summary>
		public Key SubmitKey { get; }
		/// <summary>
		/// If an auto-renew account is specified, when the topic expires, its lifetime will be extended
		/// by up to this duration (depending on the solvency of the auto-renew account). If the
		/// auto-renew account has no funds at all, the topic will be deleted instead.
		/// </summary>
		public Duration AutoRenewPeriod { get; }
		/// <summary>
		/// The account, if any, to charge for automatic renewal of the topic's lifetime upon expiry.
		/// </summary>
		public AccountId AutoRenewAccountId { get; }
		/// <summary>
		/// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
		/// </summary>
		public LedgerId LedgerId { get; }
		public Key FeeScheduleKey { get; }
		public IList<Key> FeeExemptKeys { get; }
		public IList<CustomFixedFee> CustomFees { get; }

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>                         the byte array representation</returns>
		public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public Proto.ConsensusGetTopicInfoResponse ToProtobuf()
		{
			Proto.ConsensusGetTopicInfoResponse proto = new()
			{
				TopicID = TopicId.ToProtobuf(),
				TopicInfo = new Proto.ConsensusTopicInfo
				{
					Memo = TopicMemo,
					RunningHash = RunningHash,
					SequenceNumber = SequenceNumber,
					ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime),
					AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(AutoRenewPeriod),
					LedgerId = LedgerId.ToByteString(),
					AdminKey = AdminKey.ToProtobufKey(),
					SubmitKey = SubmitKey.ToProtobufKey(),
					AutoRenewAccount = AutoRenewAccountId.ToProtobuf(),
					FeeScheduleKey = FeeScheduleKey.ToProtobufKey(),
				}
			};

			proto.TopicInfo.CustomFees.AddRange(CustomFees.Select(_ => _.ToFixedCustomFeeProtobuf()));
			proto.TopicInfo.FeeExemptKeyList.AddRange(FeeExemptKeys.Select(_ => _.ToProtobufKey()));

			return proto;
		}
	}
}