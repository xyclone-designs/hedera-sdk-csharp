using Google.Protobuf;

using System;

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
		 * Publicly visible information about the Schedule entity, up to
		 * 100 bytes. No guarantee of uniqueness.
		 */
		public string Memo { get; }
		/**
		 * The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
		 */
		public LedgerId LedgerId { get; }
		/**
		 * When set to true, the transaction will be evaluated for execution at expiration_time instead
		 * of when all required signatures are received.
		 * When set to false, the transaction will execute immediately after sufficient signatures are received
		 * to sign the contained transaction. During the initial ScheduleCreate transaction or via ScheduleSign transactions.
		 *
		 * Note: this field is unused until long Term Scheduled Transactions are enabled.
		 */
		public bool WaitForExpiry { get; }
		/**
		 * The ID of the schedule transaction
		 */
		public ScheduleId ScheduleId { get; }
		/**
		 * The Hedera account that created the schedule transaction in x.y.z format
		 */
		public AccountId CreatorAccountId { get; }
		/**
		 * The Hedera account paying for the execution of the schedule transaction
		 * in x.y.z format
		 */
		public AccountId PayerAccountId { get; }
		/**
		 * The signatories that have provided signatures so far for the schedule
		 * transaction
		 */
		public KeyList Signatories { get; }
		/**
		 * The Key which is able to delete the schedule transaction if set
		 */
		public Key? AdminKey { get; }
		/**
		 * The scheduled transaction
		 */
		public TransactionId? ScheduledTransactionId { get; }
		/**
		 * The date and time the schedule transaction will expire
		 */
		public DateTimeOffset? ExpirationTime { get; }
		/**
		 * The time the schedule transaction was executed. If the schedule
		 * transaction has not executed this field will be left null.
		 */
		public DateTimeOffset? ExecutedAt { get; }
		/**
		 * The consensus time the schedule transaction was deleted. If the
		 * schedule transaction was not deleted, this field will be left null.
		 */
		public DateTimeOffset? DeletedAt { get; }

		/**
		 * The scheduled transaction (inner transaction).
		 */
		private Proto.SchedulableTransactionBody TransactionBody { get; }

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
			Proto.SchedulableTransactionBody transactionBody,
			KeyList signers,
			Key? adminKey,
			TransactionId? scheduledTransactionId,
			string memo,
			DateTimeOffset? expirationTime,
			DateTimeOffset? executed,
			DateTimeOffset? deleted,
			LedgerId ledgerId,
			bool waitForExpiry)
			{
				ScheduleId = scheduleId;
				CreatorAccountId = creatorAccountId;
				PayerAccountId = payerAccountId;
				Signatories = signers;
				AdminKey = adminKey;
				TransactionBody = transactionBody;
				ScheduledTransactionId = scheduledTransactionId;
				Memo = memo;
				ExpirationTime = expirationTime;
				ExecutedAt = executed;
				DeletedAt = deleted;
				LedgerId = ledgerId;
				WaitForExpiry = waitForExpiry;
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
			return FromProtobuf(Proto.ScheduleInfo.Parser.ParseFrom(bytes));
		}
		/**
		 * Create a schedule info object from a protobuf.
		 *
		 * @param info              the protobuf
		 * @return                          the new schedule info object
		 */
		public static ScheduleInfo FromProtobuf(Proto.ScheduleInfo info)
			{
				return new ScheduleInfo(
					ScheduleId.FromProtobuf(info.ScheduleID),
					AccountId.FromProtobuf(info.CreatorAccountID),
					AccountId.FromProtobuf(info.PayerAccountID),
					info.ScheduledTransactionBody,
					KeyList.FromProtobuf(info.Signers, null),
					Key.FromProtobufKey(info.AdminKey),
					TransactionId.FromProtobuf(info.ScheduledTransactionID),
					info.Memo,
					DateTimeOffsetConverter.FromProtobuf(info.ExpirationTime),
					DateTimeOffsetConverter.FromProtobuf(info.ExecutionTime),
					DateTimeOffsetConverter.FromProtobuf(info.DeletionTime),
					LedgerId.FromByteString(info.LedgerId),
					info.WaitForExpiry);
			}

		/**
		 * Create the protobuf.
		 *
		 * @return                          the protobuf representation
		 */
		public Proto.ScheduleInfo ToProtobuf()
		{
			Proto.ScheduleInfo protobuf = new ()
			{
				ScheduleID = ScheduleId.ToProtobuf(),
				CreatorAccountID = CreatorAccountId.ToProtobuf(),
				ScheduledTransactionBody = TransactionBody,
				PayerAccountID = PayerAccountId.ToProtobuf(),
				Signers = Signatories.ToProtobuf(),
				Memo = Memo,
				LedgerId = LedgerId.ToByteString(),
				WaitForExpiry = WaitForExpiry,
			};
			
			if (AdminKey != null) protobuf.AdminKey = AdminKey.ToProtobufKey();
			if (ScheduledTransactionId != null) protobuf.ScheduledTransactionID = ScheduledTransactionId.ToProtobuf();
			if (ExpirationTime != null) protobuf.ExpirationTime = DateTimeOffsetConverter.ToProtobuf(ExpirationTime.Value);
			if (ExecutedAt != null) protobuf.ExecutionTime = DateTimeOffsetConverter.ToProtobuf(ExecutedAt.Value);
			if (DeletedAt != null) protobuf.DeletionTime = DateTimeOffsetConverter.ToProtobuf(DeletedAt.Value);

			return protobuf;
		}

		/**
			* Extract the transaction.
			*
			* @return                          the transaction
			*/
		public Transaction<T> GetScheduledTransaction<T>()
		{
			return Transaction<T>.FromScheduledTransaction(TransactionBody);
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