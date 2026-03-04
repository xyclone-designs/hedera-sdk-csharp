// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.SDK.Schedule
{
    /// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="T:ScheduleInfo"]/*' />
    public sealed class ScheduleInfo
    {
        /// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="M:ScheduleInfo.#ctor(ScheduleId,AccountId,AccountId,Proto.SchedulableTransactionBody,KeyList,Key,TransactionId,System.String,DateTimeOffset,DateTimeOffset,DateTimeOffset,LedgerId,System.Boolean)"]/*' />
        public ScheduleInfo(ScheduleId scheduleId, AccountId creatorAccountId, AccountId payerAccountId, Proto.SchedulableTransactionBody transactionBody, KeyList signers, Key? adminKey, TransactionId scheduledTransactionId, string memo, DateTimeOffset expirationTime, DateTimeOffset executed, DateTimeOffset deleted, LedgerId ledgerId, bool waitForExpiry)
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

		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="M:ScheduleInfo.FromBytes(System.Byte[])"]/*' />
		public static ScheduleInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.ScheduleInfo.Parser.ParseFrom(bytes));
		}
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="M:ScheduleInfo.FromProtobuf(Proto.ScheduleInfo)"]/*' />
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
                info.ExpirationTime.ToDateTimeOffset(),
                info.ExecutionTime.ToDateTimeOffset(), 
                info.DeletionTime.ToDateTimeOffset(), 
                LedgerId.FromByteString(info.LedgerId), 
                info.WaitForExpiry);
        }

		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.ScheduleId"]/*' />
		public ScheduleId ScheduleId { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.CreatorAccountId"]/*' />
		public AccountId CreatorAccountId { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.PayerAccountId"]/*' />
		public AccountId PayerAccountId { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.Signatories"]/*' />
		public KeyList Signatories { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.AdminKey"]/*' />
		public Key? AdminKey { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.ScheduledTransactionId"]/*' />
		public TransactionId ScheduledTransactionId { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.Memo"]/*' />
		public string Memo { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.ExpirationTime"]/*' />
		public DateTimeOffset ExpirationTime { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.ExecutedAt"]/*' />
		public DateTimeOffset ExecutedAt { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.DeletedAt"]/*' />
		public DateTimeOffset DeletedAt { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.TransactionBody"]/*' />
		public Proto.SchedulableTransactionBody TransactionBody { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.LedgerId"]/*' />
		public LedgerId LedgerId { get; }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="P:ScheduleInfo.WaitForExpiry"]/*' />
		public bool WaitForExpiry { get; }

		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="M:ScheduleInfo.ToProtobuf"]/*' />
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
                proto.ExpirationTime = ExpirationTime.ToProtoTimestamp();

            if (ExecutedAt != null)
                proto.ExecutionTime = ExecutedAt.ToProtoTimestamp();

            if (DeletedAt != null)
                proto.DeletionTime = DeletedAt.ToProtoTimestamp();

            return proto;
		}

        /// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="M:ScheduleInfo.GetScheduledTransaction"]/*' />
        public object GetScheduledTransaction()
        {
            return Transaction.FromScheduledTransaction(TransactionBody);
        }
		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="M:ScheduleInfo.GetScheduledTransaction``1"]/*' />
		public Transaction<T> GetScheduledTransaction<T>() where T : Transaction<T>
		{
			return Transaction.FromScheduledTransaction<T>(TransactionBody);
		}

		/// <include file="ScheduleInfo.cs.xml" path='docs/member[@name="M:ScheduleInfo.ToBytes"]/*' />
		public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}