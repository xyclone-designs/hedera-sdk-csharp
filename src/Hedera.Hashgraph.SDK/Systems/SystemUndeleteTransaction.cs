// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Systems
{
	/// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="T:SystemUndeleteTransaction"]/*' />
	[Obsolete("Obsolete")]
    public sealed class SystemUndeleteTransaction : Transaction<SystemUndeleteTransaction>
    {
        /// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:SystemUndeleteTransaction"]/*' />
        public SystemUndeleteTransaction() { }
		/// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:SystemUndeleteTransaction(Proto.Services.TransactionBody)"]/*' />
		internal SystemUndeleteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:SystemUndeleteTransaction(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal SystemUndeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen"]/*' />
		public FileId? FileId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
        /// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_2"]/*' />
        public ContractId? ContractId 
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }

        /// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.SystemUndelete;

            FileId = FileId.FromProtobuf(body.FileId);
            ContractId = ContractId.FromProtobuf(body.ContractId);
        }

        /// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:ToProtobuf"]/*' />
        public Proto.Services.SystemUndeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.SystemUndeleteTransactionBody();

            if (FileId is not null) builder.FileId = FileId.ToProtobuf();
            if (ContractId is not null) builder.ContractId = ContractId.ToProtobuf();

            return builder;
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
                throw new InvalidOperationException("SystemDeleteTransaction must have exactly 1 of the following fields set: contractId, fileId");
            }

            return Task.CompletedTask;
        }
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.SystemUndelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.SystemUndelete = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			if (FileId is not null)
			{
				string methodname = nameof(Proto.Services.FileService.FileServiceClient.systemUndelete);

				return Proto.Services.FileService.Descriptor.FindMethodByName(methodname);
			}
			else
			{
				string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.systemUndelete);

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
