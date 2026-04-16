// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Schedule
{
    /// <include file="ScheduleDeleteTransaction.cs.xml" path='docs/member[@name="T:ScheduleDeleteTransaction"]/*' />
    public sealed class ScheduleDeleteTransaction : Transaction<ScheduleDeleteTransaction>
    {
        /// <include file="ScheduleDeleteTransaction.cs.xml" path='docs/member[@name="M:ScheduleDeleteTransaction.#ctor"]/*' />
        public ScheduleDeleteTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
		/// <include file="ScheduleDeleteTransaction.cs.xml" path='docs/member[@name="M:ScheduleDeleteTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal ScheduleDeleteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="ScheduleDeleteTransaction.cs.xml" path='docs/member[@name="M:ScheduleDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal ScheduleDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="ScheduleDeleteTransaction.cs.xml" path='docs/member[@name="M:ScheduleDeleteTransaction.RequireNotFrozen"]/*' />
        public ScheduleId? ScheduleId
		{
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }

        /// <include file="ScheduleDeleteTransaction.cs.xml" path='docs/member[@name="M:ScheduleDeleteTransaction.InitFromTransactionBody"]/*' />
        private void InitFromTransactionBody()
        {
            if (SourceTransactionBody.ScheduleDelete.ScheduleId is not null)
				ScheduleId = ScheduleId.FromProtobuf(SourceTransactionBody.ScheduleDelete.ScheduleId);
		}

        /// <include file="ScheduleDeleteTransaction.cs.xml" path='docs/member[@name="M:ScheduleDeleteTransaction.ToProtobuf"]/*' />
        public Proto.Services.ScheduleDeleteTransactionBody ToProtobuf()
        {
			Proto.Services.ScheduleDeleteTransactionBody proto =  new ();
            
            if (ScheduleId != null)
                proto.ScheduleId = ScheduleId.ToProtobuf();

			return proto;
        }

        public override void ValidateChecksums(Client client)
        {
			ScheduleId?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ScheduleDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.ScheduleDelete = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.ScheduleService.ScheduleServiceClient.deleteSchedule);

			return Proto.Services.ScheduleService.Descriptor.FindMethodByName(methodname);
		}

		public override void OnExecute(Client client)
        {
            throw new NotImplementedException();
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
