using System.Reflection.Metadata;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Current state of a topic.
 */
	public sealed class TopicInfo
	{
		/**
		 * The ID of the topic for which information is requested.
		 */
		public readonly TopicId topicId;

    /**
     * Short publicly visible memo about the topic. No guarantee of uniqueness.
     */
    public readonly string topicMemo;

    /**
     * SHA-384 running hash of (previousRunningHash, topicId, consensusTimestamp, sequenceNumber, message).
     */
    public readonly ByteString runningHash;

    /**
     * Sequence number (starting at 1 for the first submitMessage) of messages on the topic.
     */
    public readonly long sequenceNumber;

		/**
		 * Effective consensus timestamp at (and after) which submitMessage calls will no longer succeed on the topic.
		 */
		public readonly DateTimeOffset expirationTime;

    /**
     * Access control for update/delete of the topic. Null if there is no key.
     */
    @Nullable
		public readonly Key adminKey;

    /**
     * Access control for ConsensusService.submitMessage. Null if there is no key.
     */
    @Nullable
		public readonly Key submitKey;

    /**
     * If an auto-renew account is specified, when the topic expires, its lifetime will be extended
     * by up to this duration (depending on the solvency of the auto-renew account). If the
     * auto-renew account has no funds at all, the topic will be deleted instead.
     */
    public readonly Duration autoRenewPeriod;

    /**
     * The account, if any, to charge for automatic renewal of the topic's lifetime upon expiry.
     */
    @Nullable
		public readonly AccountId autoRenewAccountId;

    /**
     * The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
     */
    public readonly LedgerId ledgerId;

    public readonly Key feeScheduleKey;

    public readonly List<Key> feeExemptKeys;

		public readonly List<CustomFixedFee> customFees;

		private TopicInfo(
				TopicId topicId,
				string topicMemo,
				ByteString runningHash,
				long sequenceNumber,
				DateTimeOffset expirationTime,
				@Nullable Key adminKey,
				@Nullable Key submitKey,
				Duration autoRenewPeriod,
				@Nullable AccountId autoRenewAccountId,
				LedgerId ledgerId,
				Key feeScheduleKey,
				List<Key> feeExemptKeys,
				List<CustomFixedFee> customFees)
		{
			this.topicId = topicId;
			this.topicMemo = topicMemo;
			this.runningHash = runningHash;
			this.sequenceNumber = sequenceNumber;
			this.expirationTime = expirationTime;
			this.adminKey = adminKey;
			this.submitKey = submitKey;
			this.autoRenewPeriod = autoRenewPeriod;
			this.autoRenewAccountId = autoRenewAccountId;
			this.ledgerId = ledgerId;
			this.feeScheduleKey = feeScheduleKey;
			this.feeExemptKeys = feeExemptKeys;
			this.customFees = customFees;
		}

		/**
		 * Create a topic info object from a protobuf.
		 *
		 * @param topicInfoResponse         the protobuf
		 * @return                          the new topic info object
		 */
		static TopicInfo FromProtobuf(ConsensusGetTopicInfoResponse topicInfoResponse)
		{
			var topicInfo = topicInfoResponse.getTopicInfo();

			var adminKey = topicInfo.hasAdminKey() ? Key.FromProtobufKey(topicInfo.getAdminKey()) : null;

			var submitKey = topicInfo.hasSubmitKey() ? Key.FromProtobufKey(topicInfo.getSubmitKey()) : null;

			var autoRenewAccountId =
					topicInfo.hasAutoRenewAccount() ? AccountId.FromProtobuf(topicInfo.getAutoRenewAccount()) : null;

			var feeScheduleKey = topicInfo.hasFeeScheduleKey() ? Key.FromProtobufKey(topicInfo.getFeeScheduleKey()) : null;

			var feeExemptKeys = topicInfo.getFeeExemptKeyListList() != null
					? topicInfo.getFeeExemptKeyListList().stream()
							.map(Key::fromProtobufKey)
							.toList()
					: null;

			var customFees = topicInfo.getCustomFeesList() != null
					? topicInfo.getCustomFeesList().stream()
							.map(x->CustomFixedFee.FromProtobuf(x.getFixedFee()))
							.toList()
					: null;

			return new TopicInfo(
					TopicId.FromProtobuf(topicInfoResponse.getTopicID()),
					topicInfo.getMemo(),
					topicInfo.getRunningHash(),
					topicInfo.getSequenceNumber(),
					DateTimeOffsetConverter.FromProtobuf(topicInfo.getExpirationTime()),
					adminKey,
					submitKey,
					DurationConverter.FromProtobuf(topicInfo.getAutoRenewPeriod()),
					autoRenewAccountId,
					LedgerId.FromByteString(topicInfo.getLedgerId()),
					feeScheduleKey,
					feeExemptKeys,
					customFees);
		}

		/**
		 * Create a topic info object from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new topic info object
		 * @       when there is an issue with the protobuf
		 */
		public static TopicInfo FromBytes(byte[] bytes) 
		{
        return FromProtobuf(
				ConsensusGetTopicInfoResponse.Parser.ParseFrom(bytes));
    }

    /**
     * Create the protobuf.
     *
     * @return                          the protobuf representation
     */
    ConsensusGetTopicInfoResponse ToProtobuf()
		{
			var topicInfoResponseBuilder =
					ConsensusGetTopicInfoResponse.newBuilder().setTopicID(topicId.ToProtobuf());

			var topicInfoBuilder = ConsensusTopicInfo.newBuilder()
					.setMemo(topicMemo)
					.setRunningHash(runningHash)
					.setSequenceNumber(sequenceNumber)
					.setExpirationTime(DateTimeOffsetConverter.ToProtobuf(expirationTime))
					.setAutoRenewPeriod(DurationConverter.ToProtobuf(autoRenewPeriod))
					.setLedgerId(ledgerId.toByteString());

			if (adminKey != null)
			{
				topicInfoBuilder.setAdminKey(adminKey.ToProtobufKey());
			}

			if (submitKey != null)
			{
				topicInfoBuilder.setSubmitKey(submitKey.ToProtobufKey());
			}

			if (autoRenewAccountId != null)
			{
				topicInfoBuilder.setAutoRenewAccount(autoRenewAccountId.ToProtobuf());
			}

			if (feeScheduleKey != null)
			{
				topicInfoBuilder.setFeeScheduleKey(feeScheduleKey.ToProtobufKey());
			}

			if (feeExemptKeys != null)
			{
				for (Key feeExemptKey : feeExemptKeys)
				{
					topicInfoBuilder.AddFeeExemptKeyList(feeExemptKey.ToProtobufKey());
				}
			}

			if (customFees != null)
			{
				for (CustomFixedFee customFee : customFees)
				{
					topicInfoBuilder.AddCustomFees(customFee.toTopicFeeProtobuf());
				}
			}

			return topicInfoResponseBuilder.setTopicInfo(topicInfoBuilder).build();
		}


	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("topicId", topicId)
				.Add("topicMemo", topicMemo)
				.Add("runningHash", runningHash.ToByteArray())
				.Add("sequenceNumber", sequenceNumber)
				.Add("expirationTime", expirationTime)
				.Add("adminKey", adminKey)
				.Add("submitKey", submitKey)
				.Add("autoRenewPeriod", autoRenewPeriod)
				.Add("autoRenewAccountId", autoRenewAccountId)
				.Add("ledgerId", ledgerId)
				.toString();
	}

	/**
     * Create the byte array.
     *
     * @return                          the byte array representation
     */
	public byte[] ToBytes()
	{
		return ToProtobuf().ToByteArray();
	}
}

}