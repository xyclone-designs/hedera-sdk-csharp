// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Systems
{
	/// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="T:SystemDeleteTransaction"]/*' />
	[Obsolete("Obsolete")]
    public sealed class SystemDeleteTransaction : Transaction<SystemDeleteTransaction>
    {
        /// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="M:SystemDeleteTransaction"]/*' />
        public SystemDeleteTransaction() { }
		/// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="M:SystemDeleteTransaction(Proto.Services.TransactionBody)"]/*' />
		internal SystemDeleteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="M:SystemDeleteTransaction(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal SystemDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen"]/*' />
		public FileId? FileId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				ContractId = null; // Reset ContractId
			}
		}
        /// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_2"]/*' />
        public ContractId? ContractId
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
				FileId = null; // Reset FileIds
			}
        }
		/// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_3"]/*' />
		public DateTimeOffset? ExpirationTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

        /// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="M:ToProtobuf"]/*' />
        public Proto.Services.SystemDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.SystemDeleteTransactionBody();

            if (FileId != null)
            {
                builder.FileId = FileId.ToProtobuf();
            }

            if (ContractId != null)
            {
                builder.ContractId = ContractId.ToProtobuf();
            }

            if (ExpirationTime != null)
            {
                builder.ExpirationTime = ExpirationTime.Value.ToProtoTimestampSeconds();
            }

            return builder;
        }

        /// <include file="SystemDeleteTransaction.cs.xml" path='docs/member[@name="M:InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
			var body = SourceTransactionBody.SystemDelete;

			FileId = FileId.FromProtobuf(body.FileId);
			ContractId = ContractId.FromProtobuf(body.ContractId);
			ExpirationTime = body.ExpirationTime.ToDateTimeOffset();
		}

        public override void ValidateChecksums(Client client)
        {
            FileId?.ValidateChecksum(client);
            ContractId?.ValidateChecksum(client);
        }
        public override Task OnExecuteAsync(Client client)
        {
            int modesEnabled = (FileId != null ? 1 : 0) + (ContractId != null ? 1 : 0);
            if (modesEnabled != 1)
            {
                throw new InvalidOperationException("SystemDeleteTransaction must have exactly 1 of the following fields set: ContractId, FileId");
            }

            return Task.CompletedTask;
        }
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.SystemDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
			scheduled.SystemDelete = ToProtobuf();
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
            if (FileId is not null)
            {
                string methodname = nameof(Proto.Services.FileService.FileServiceClient.systemDelete);

                return Proto.Services.FileService.Descriptor.FindMethodByName(methodname);
            }
            else
            {
				string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.systemDelete);

				return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
			}
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
