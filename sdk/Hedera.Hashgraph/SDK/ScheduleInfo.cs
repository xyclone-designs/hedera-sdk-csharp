using System.Transactions;

namespace Hedera.Hashgraph.SDK
{
	/**
 * A query that returns information about the current state of a scheduled
 * transaction on a Hedera network.
 * <p>
 * See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/get-schedule-info">Hedera Documentation</a>
 */
	public sealed class ScheduleInfo
	{
		/**
		 * The ID of the schedule transaction
		 */
		public readonly ScheduleId scheduleId;
    /**
     * The Hedera account that created the schedule transaction in x.y.z format
     */
    public readonly AccountId creatorAccountId;
    /**
     * The Hedera account paying for the execution of the schedule transaction
     * in x.y.z format
     */
    public readonly AccountId payerAccountId;
    /**
     * The signatories that have provided signatures so far for the schedule
     * transaction
     */
    public readonly KeyList signatories;

    /**
     * The Key which is able to delete the schedule transaction if set
     */
    @Nullable
		public readonly Key adminKey;

    /**
     * The scheduled transaction
     */
    @Nullable
		public readonly TransactionId scheduledTransactionId;

    /**
     * Publicly visible information about the Schedule entity, up to
     * 100 bytes. No guarantee of uniqueness.
     */
    public readonly string memo;

    /**
     * The date and time the schedule transaction will expire
     */
    @Nullable
		public readonly DateTimeOffset expirationTime;

    /**
     * The time the schedule transaction was executed. If the schedule
     * transaction has not executed this field will be left null.
     */
    @Nullable
		public readonly DateTimeOffset executedAt;

    /**
     * The consensus time the schedule transaction was deleted. If the
     * schedule transaction was not deleted, this field will be left null.
     */
    @Nullable
		public readonly DateTimeOffset deletedAt;

    /**
     * The scheduled transaction (inner transaction).
     */
    readonly SchedulableTransactionBody transactionBody;

    /**
     * The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
     */
    @Nullable
		public readonly LedgerId ledgerId;

    /**
     * When set to true, the transaction will be evaluated for execution at expiration_time instead
     * of when all required signatures are received.
     * When set to false, the transaction will execute immediately after sufficient signatures are received
     * to sign the contained transaction. During the initial ScheduleCreate transaction or via ScheduleSign transactions.
     *
     * Note: this field is unused until long Term Scheduled Transactions are enabled.
     */
    public readonly bool waitForExpiry;

    /**
     * Constructor.
     *
     * @param scheduleId                the schedule id
     * @param creatorAccountId          the creator account id
     * @param payerAccountId            the payer account id
     * @param transactionBody           the transaction body
     * @param signers                   the signers key list
     * @param adminKey                  the admin key
     * @param scheduledTransactionId    the transaction id
     * @param memo                      the memo 100 bytes max
     * @param expirationTime            the expiration time
     * @param executed                  the time transaction was executed
     * @param deleted                   the time it was deleted
     * @param ledgerId                  the ledger id
     * @param waitForExpiry             the wait for expiry field
     */
    ScheduleInfo(
			ScheduleId scheduleId,
			AccountId creatorAccountId,
			AccountId payerAccountId,
			SchedulableTransactionBody transactionBody,
			KeyList signers,
			@Nullable Key adminKey,
			@Nullable TransactionId scheduledTransactionId,
			string memo,
			@Nullable DateTimeOffset expirationTime,
			@Nullable DateTimeOffset executed,
			@Nullable DateTimeOffset deleted,
			LedgerId ledgerId,
			bool waitForExpiry)
		{
			this.scheduleId = scheduleId;
			this.creatorAccountId = creatorAccountId;
			this.payerAccountId = payerAccountId;
			this.signatories = signers;
			this.adminKey = adminKey;
			this.transactionBody = transactionBody;
			this.scheduledTransactionId = scheduledTransactionId;
			this.memo = memo;
			this.expirationTime = expirationTime;
			this.executedAt = executed;
			this.deletedAt = deleted;
			this.ledgerId = ledgerId;
			this.waitForExpiry = waitForExpiry;
		}

		/**
		 * Create a schedule info object from a protobuf.
		 *
		 * @param info              the protobuf
		 * @return                          the new schedule info object
		 */
		static ScheduleInfo FromProtobuf(Proto.ScheduleInfo info)
		{
			var scheduleId = ScheduleId.FromProtobuf(info.getScheduleID());
			var creatorAccountId = AccountId.FromProtobuf(info.getCreatorAccountID());
			var payerAccountId = AccountId.FromProtobuf(info.getPayerAccountID());
			var adminKey = info.hasAdminKey() ? Key.FromProtobufKey(info.getAdminKey()) : null;
			var scheduledTransactionId =
					info.hasScheduledTransactionID() ? TransactionId.FromProtobuf(info.getScheduledTransactionID()) : null;

			return new ScheduleInfo(
					scheduleId,
					creatorAccountId,
					payerAccountId,
					info.getScheduledTransactionBody(),
					KeyList.FromProtobuf(info.getSigners(), null),
					adminKey,
					scheduledTransactionId,
					info.getMemo(),
					info.hasExpirationTime() ? DateTimeOffsetConverter.FromProtobuf(info.getExpirationTime()) : null,
					info.hasExecutionTime() ? DateTimeOffsetConverter.FromProtobuf(info.getExecutionTime()) : null,
					info.hasDeletionTime() ? DateTimeOffsetConverter.FromProtobuf(info.getDeletionTime()) : null,
					LedgerId.FromByteString(info.getLedgerId()),
					info.getWaitForExpiry());
		}

		/**
		 * Create a schedule info object from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new schedule info object
		 * @       when there is an issue with the protobuf
		 */
		public static ScheduleInfo FromBytes(byte[] bytes) 
		{
        return FromProtobuf(Proto.ScheduleInfo.Parser.ParseFrom(bytes).toBuilder()
	                .build());
    }

    /**
     * Create the protobuf.
     *
     * @return                          the protobuf representation
     */
    Proto.ScheduleInfo ToProtobuf()
		{
			var info = Proto.ScheduleInfo.newBuilder();

			if (adminKey != null)
			{
				info.setAdminKey(adminKey.ToProtobufKey());
			}

			if (scheduledTransactionId != null)
			{
				info.setScheduledTransactionID(scheduledTransactionId.ToProtobuf());
			}

			if (expirationTime != null)
			{
				info.setExpirationTime(DateTimeOffsetConverter.ToProtobuf(expirationTime));
			}

			if (executedAt != null)
			{
				info.setExecutionTime(DateTimeOffsetConverter.ToProtobuf(executedAt));
			}

			if (deletedAt != null)
			{
				info.setDeletionTime(DateTimeOffsetConverter.ToProtobuf(deletedAt));
			}

			return info.setScheduleID(scheduleId.ToProtobuf())
					.setCreatorAccountID(creatorAccountId.ToProtobuf())
					.setScheduledTransactionBody(transactionBody)
					.setPayerAccountID(payerAccountId.ToProtobuf())
					.setSigners(signatories.ToProtobuf())
					.setMemo(memo)
					.setLedgerId(ledgerId.toByteString())
					.setWaitForExpiry(waitForExpiry)
					.build();
		}

		/**
		 * Extract the transaction.
		 *
		 * @return                          the transaction
		 */
		public Transaction<?> getScheduledTransaction()
		{
			return Transaction.FromScheduledTransaction(transactionBody);
		}


	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("scheduleId", scheduleId)
				.Add("scheduledTransactionId", scheduledTransactionId)
				.Add("creatorAccountId", creatorAccountId)
				.Add("payerAccountId", payerAccountId)
				.Add("signatories", signatories)
				.Add("adminKey", adminKey)
				.Add("expirationTime", expirationTime)
				.Add("memo", memo)
				.Add("executedAt", executedAt)
				.Add("deletedAt", deletedAt)
				.Add("ledgerId", ledgerId)
				.Add("waitForExpiry", waitForExpiry)
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