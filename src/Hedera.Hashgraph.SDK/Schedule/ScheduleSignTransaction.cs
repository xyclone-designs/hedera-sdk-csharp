// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Schedule
{
    /// <include file="ScheduleSignTransaction.cs.xml" path='docs/member[@name="T:ScheduleSignTransaction"]/*' />
    public sealed class ScheduleSignTransaction : Transaction<ScheduleSignTransaction>
    {
        /// <include file="ScheduleSignTransaction.cs.xml" path='docs/member[@name="M:ScheduleSignTransaction.#ctor"]/*' />
        public ScheduleSignTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
        /// <include file="ScheduleSignTransaction.cs.xml" path='docs/member[@name="M:ScheduleSignTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
        internal ScheduleSignTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="ScheduleSignTransaction.cs.xml" path='docs/member[@name="M:ScheduleSignTransaction.RequireNotFrozen"]/*' />
		public ScheduleId? ScheduleId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

        /// <include file="ScheduleSignTransaction.cs.xml" path='docs/member[@name="M:ScheduleSignTransaction.ToProtobuf"]/*' />
        public Proto.Services.ScheduleSignTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ScheduleSignTransactionBody();
            
            if (ScheduleId != null)
            {
                builder.ScheduleId = ScheduleId.ToProtobuf();
            }

            return builder;
        }

        /// <include file="ScheduleSignTransaction.cs.xml" path='docs/member[@name="M:ScheduleSignTransaction.InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ScheduleSign;

            if (body.ScheduleId is not null)
            {
                ScheduleId = ScheduleId.FromProtobuf(body.ScheduleId);
            }
        }

        public override void ValidateChecksums(Client client)
        {
			ScheduleId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ScheduleSign = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("cannot schedule ScheduleSignTransaction");
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.ScheduleService.ScheduleServiceClient.signSchedule);

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
