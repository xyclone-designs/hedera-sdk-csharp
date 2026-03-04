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
    /// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:Obsolete(&quot;Obsolete&quot;)"]/*' />
    [Obsolete("Obsolete")]
    public sealed class SystemUndeleteTransaction : Transaction<SystemUndeleteTransaction>
    {
        /// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:SystemUndeleteTransaction"]/*' />
        public SystemUndeleteTransaction() { }
		/// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:SystemUndeleteTransaction(Proto.TransactionBody)"]/*' />
		internal SystemUndeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:SystemUndeleteTransaction(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal SystemUndeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
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

            FileId = FileId.FromProtobuf(body.FileID);
            ContractId = ContractId.FromProtobuf(body.ContractID);
        }

        /// <include file="SystemUndeleteTransaction.cs.xml" path='docs/member[@name="M:ToProtobuf"]/*' />
        public Proto.SystemUndeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.SystemUndeleteTransactionBody();

            if (FileId is not null) builder.FileID = FileId.ToProtobuf();
            if (ContractId is not null) builder.ContractID = ContractId.ToProtobuf();

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
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SystemUndelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.SystemUndelete = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			if (FileId is not null)
			{
				string methodname = nameof(Proto.FileService.FileServiceClient.systemUndelete);

				return Proto.FileService.Descriptor.FindMethodByName(methodname);
			}
			else
			{
				string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.systemUndelete);

				return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
			}
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}