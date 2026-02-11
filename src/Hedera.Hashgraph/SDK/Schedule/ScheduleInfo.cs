// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.SDK.Schedule
{
    /// <summary>
    /// A query that returns information about the current state of a scheduled
    /// transaction on a Hedera network.
    /// <p>
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/get-schedule-info">Hedera Documentation</a>
    /// </summary>
    public sealed class ScheduleInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scheduleId">the schedule id</param>
        /// <param name="creatorAccountId">the creator account id</param>
        /// <param name="payerAccountId">the payer account id</param>
        /// <param name="transactionBody">the transaction body</param>
        /// <param name="signers">the signers key list</param>
        /// <param name="adminKey">the admin key</param>
        /// <param name="scheduledTransactionId">the transaction id</param>
        /// <param name="memo">the memo 100 bytes max</param>
        /// <param name="expirationTime">the expiration time</param>
        /// <param name="executed">the time transaction was executed</param>
        /// <param name="deleted">the time it was deleted</param>
        /// <param name="ledgerId">the ledger id</param>
        /// <param name="waitForExpiry">the wait for expiry field</param>
        protected ScheduleInfo(ScheduleId scheduleId, AccountId creatorAccountId, AccountId payerAccountId, Proto.SchedulableTransactionBody transactionBody, KeyList signers, Key adminKey, TransactionId scheduledTransactionId, string memo, Timestamp expirationTime, Timestamp executed, Timestamp deleted, LedgerId ledgerId, bool waitForExpiry)
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

		/// <summary>
		/// Create a schedule info object from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new schedule info object</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static ScheduleInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.ScheduleInfo.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a schedule info object from a protobuf.
		/// </summary>
		/// <param name="info">the protobuf</param>
		/// <returns>                         the new schedule info object</returns>
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
                Utils.TimestampConverter.FromProtobuf(info.ExpirationTime),
                Utils.TimestampConverter.FromProtobuf(info.ExecutionTime), 
                Utils.TimestampConverter.FromProtobuf(info.DeletionTime), 
                LedgerId.FromByteString(info.LedgerId), 
                info.WaitForExpiry);
        }

		/// <summary>
		/// The ID of the schedule transaction
		/// </summary>
		public ScheduleId ScheduleId { get; }
		/// <summary>
		/// The Hedera account that created the schedule transaction in x.y.z format
		/// </summary>
		public AccountId CreatorAccountId { get; }
		/// <summary>
		/// The Hedera account paying for the execution of the schedule transaction
		/// in x.y.z format
		/// </summary>
		public AccountId PayerAccountId { get; }
		/// <summary>
		/// The signatories that have provided signatures so far for the schedule
		/// transaction
		/// </summary>
		public KeyList Signatories { get; }
		/// <summary>
		/// The Key which is able to delete the schedule transaction if set
		/// </summary>
		public Key AdminKey { get; }
		/// <summary>
		/// The scheduled transaction
		/// </summary>
		public TransactionId ScheduledTransactionId { get; }
		/// <summary>
		/// Publicly visible information about the Schedule entity, up to
		/// 100 bytes. No guarantee of uniqueness.
		/// </summary>
		public string Memo { get; }
		/// <summary>
		/// The date and time the schedule transaction will expire
		/// </summary>
		public Timestamp ExpirationTime { get; }
		/// <summary>
		/// The time the schedule transaction was executed. If the schedule
		/// transaction has not executed this field will be left null.
		/// </summary>
		public Timestamp ExecutedAt { get; }
		/// <summary>
		/// The consensus time the schedule transaction was deleted. If the
		/// schedule transaction was not deleted, this field will be left null.
		/// </summary>
		public Timestamp DeletedAt { get; }
		/// <summary>
		/// The scheduled transaction (inner transaction).
		/// </summary>
		public Proto.SchedulableTransactionBody TransactionBody { get; }
		/// <summary>
		/// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
		/// </summary>
		public LedgerId LedgerId { get; }
		/// <summary>
		/// When set to true, the transaction will be evaluated for execution at expiration_time instead
		/// of when all required signatures are received.
		/// When set to false, the transaction will execute immediately after sufficient signatures are received
		/// to sign the contained transaction. During the initial ScheduleCreate transaction or via ScheduleSign transactions.
		/// 
		/// Note: this field is unused until Long Term Scheduled Transactions are enabled.
		/// </summary>
		public bool WaitForExpiry { get; }

		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public Proto.ScheduleInfo ToProtobuf()
        {
			Proto.ScheduleInfo proto = new ()
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

            if (AdminKey != null)
                proto.AdminKey = AdminKey.ToProtobufKey();

            if (ScheduledTransactionId != null)
                proto.ScheduledTransactionID = ScheduledTransactionId.ToProtobuf();

            if (ExpirationTime != null)
                proto.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime);

            if (ExecutedAt != null)
                proto.ExecutionTime = Utils.TimestampConverter.ToProtobuf(ExecutedAt);

            if (DeletedAt != null)
                proto.DeletionTime = Utils.TimestampConverter.ToProtobuf(DeletedAt);

            return proto;
		}

        /// <summary>
        /// Extract the transaction.
        /// </summary>
        /// <returns>                         the transaction</returns>
        public Transaction<T> GetScheduledTransaction<T>() where T : Transaction<T>
        {
            return Transaction<T>.FromScheduledTransaction(TransactionBody);
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}