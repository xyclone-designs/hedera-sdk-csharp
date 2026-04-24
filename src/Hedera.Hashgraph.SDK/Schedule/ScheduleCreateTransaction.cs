// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Schedule
{
    /// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="T:ScheduleCreateTransaction"]/*' />
    public sealed class ScheduleCreateTransaction : Transaction<ScheduleCreateTransaction>
    {
        /// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.#ctor"]/*' />
        public ScheduleCreateTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
        /// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
        internal ScheduleCreateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.RequireNotFrozen"]/*' />
		public DateTimeOffset? ExpirationTime 
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
				ExpirationTimeDuration = null;
			}
        }
		/// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.RequireNotFrozen_2"]/*' />
		public TimeSpan? ExpirationTimeDuration
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				ExpirationTime = null;
			}
		}
		/// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="P:ScheduleCreateTransaction.WaitForExpiry"]/*' />
		public bool WaitForExpiry { get; set; }
		/// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.RequireNotFrozen_3"]/*' />
		public AccountId? PayerAccountId 
        {
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.RequireNotFrozen_4"]/*' />
		public Proto.Services.SchedulableTransactionBody? ScheduledTransactionBody
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.RequireNotFrozen_5"]/*' />
		public Key? AdminKey
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.RequireNotFrozen_6"]/*' />
		public string ScheduleMemo 
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        } = string.Empty;

		/// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.ToProtobuf"]/*' />
		public Proto.Services.ScheduleCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ScheduleCreateTransactionBody();

            if (PayerAccountId != null)
				builder.PayerAccountId = PayerAccountId.ToProtobuf();

			if (ScheduledTransactionBody != null)
				builder.ScheduledTransactionBody = ScheduledTransactionBody;

			if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

			if (ExpirationTime != null)
				builder.ExpirationTime = ExpirationTime.Value.ToProtoTimestamp();
			else if (ExpirationTimeDuration != null)
				builder.ExpirationTime = ExpirationTimeDuration.Value.ToProtoTimestamp();

			builder.Memo = ScheduleMemo;
            builder.WaitForExpiry = WaitForExpiry;

            return builder;
        }

        /// <include file="ScheduleCreateTransaction.cs.xml" path='docs/member[@name="M:ScheduleCreateTransaction.InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ScheduleCreate;

			ScheduleMemo = body.Memo;
			AdminKey = Key.FromProtobufKey(body.AdminKey);
			ScheduledTransactionBody = body.ScheduledTransactionBody;
			PayerAccountId = AccountId.FromProtobuf(body.PayerAccountId);
			ExpirationTime = body.ExpirationTime.ToDateTimeOffset();
        }

        public override void ValidateChecksums(Client client)
        {
            PayerAccountId?.ValidateChecksum(client);
        }
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ScheduleCreate = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule ScheduleCreateTransaction");
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.ScheduleService.ScheduleServiceClient.createSchedule);

			return Proto.Services.ScheduleService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
