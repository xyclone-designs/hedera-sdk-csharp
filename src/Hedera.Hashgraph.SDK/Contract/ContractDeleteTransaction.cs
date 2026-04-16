// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="T:ContractDeleteTransaction"]/*' />
    public sealed class ContractDeleteTransaction : Transaction<ContractDeleteTransaction>
    {
        /// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.#ctor"]/*' />
        public ContractDeleteTransaction() { }
		/// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		public ContractDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }
        /// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
        internal ContractDeleteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

		/// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.RequireNotFrozen"]/*' />
		public ContractId? ContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.RequireNotFrozen_2"]/*' />
		public AccountId? TransferAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.RequireNotFrozen_3"]/*' />
		public ContractId? TransferContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
        /// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.RequireNotFrozen_4"]/*' />
        public bool? PermanentRemoval
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }

        /// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractDeleteInstance;

            if (body.ContractId is not null)
                ContractId = ContractId.FromProtobuf(body.ContractId);

            if (body.TransferContractId is not null)
                TransferContractId = ContractId.FromProtobuf(body.TransferContractId);

            if (body.TransferAccountId is not null)
                TransferAccountId = AccountId.FromProtobuf(body.TransferAccountId);

            if (body.PermanentRemoval)
				PermanentRemoval = true;
		}

        /// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.ToProtobuf"]/*' />
        public Proto.Services.ContractDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ContractDeleteTransactionBody();

            if (ContractId != null) builder.ContractId = ContractId.ToProtobuf();
            if (TransferAccountId != null) builder.TransferAccountId = TransferAccountId.ToProtobuf();
            if (TransferContractId != null) builder.TransferContractId = TransferContractId.ToProtobuf();
            if (PermanentRemoval != null) builder.PermanentRemoval = PermanentRemoval.Value;

            return builder;
        }

		/// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.ValidateChecksums(Client)"]/*' />
		public override void ValidateChecksums(Client client)
		{
			ContractId?.ValidateChecksum(client);
			TransferContractId?.ValidateChecksum(client);
			TransferAccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractDeleteInstance = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractDeleteInstance = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.deleteContract);

			return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
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
