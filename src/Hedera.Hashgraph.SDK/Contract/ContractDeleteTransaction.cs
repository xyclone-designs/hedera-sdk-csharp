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
		/// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		public ContractDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }
        /// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.#ctor(Proto.TransactionBody)"]/*' />
        internal ContractDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
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

            if (body.ContractID is not null)
                ContractId = ContractId.FromProtobuf(body.ContractID);

            if (body.TransferContractID is not null)
                TransferContractId = ContractId.FromProtobuf(body.TransferContractID);

            if (body.TransferAccountID is not null)
                TransferAccountId = AccountId.FromProtobuf(body.TransferAccountID);

            if (body.PermanentRemoval)
				PermanentRemoval = true;
		}

        /// <include file="ContractDeleteTransaction.cs.xml" path='docs/member[@name="M:ContractDeleteTransaction.ToProtobuf"]/*' />
        public Proto.ContractDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.ContractDeleteTransactionBody();

            if (ContractId != null) builder.ContractID = ContractId.ToProtobuf();
            if (TransferAccountId != null) builder.TransferAccountID = TransferAccountId.ToProtobuf();
            if (TransferContractId != null) builder.TransferContractID = TransferContractId.ToProtobuf();
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
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractDeleteInstance = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractDeleteInstance = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.deleteContract);

			return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
		}

		public override void OnExecute(Client client)
        {
            throw new NotImplementedException();
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