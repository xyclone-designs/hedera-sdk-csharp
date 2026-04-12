// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="T:ContractExecuteTransaction"]/*' />
    public sealed class ContractExecuteTransaction : Transaction<ContractExecuteTransaction>
    {
        /// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.#ctor"]/*' />
        public ContractExecuteTransaction() { }
		/// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal ContractExecuteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal ContractExecuteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.RequireNotFrozen"]/*' />
        public long Gas
        {
            get;
            set 
            {
				RequireNotFrozen();

				if (value < 0)
					throw new ArgumentException("Gas must be non-negative");

				field = value;
			}
        }
		/// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.RequireNotFrozen_2"]/*' />
		public Hbar PayableAmount
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        } = new Hbar(0);
		/// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.RequireNotFrozen_3"]/*' />
		public ContractId? ContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
        /// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.RequireNotFrozen_4"]/*' />
        public ByteString FunctionParameters
        {
            get;
            set
            {
                RequireNotFrozen();
                field = ByteString.CopyFrom(value.ToByteArray());
            }
        } = ByteString.CopyFrom([]);

		/// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.SetFunction(System.String)"]/*' />
		public ContractExecuteTransaction SetFunction(string name)
        {
            return SetFunction(name, new ContractFunctionParameters());
        }
        /// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.SetFunction(System.String,ContractFunctionParameters @)"]/*' />
        public ContractExecuteTransaction SetFunction(string name, ContractFunctionParameters @params)
        {
            FunctionParameters = ByteString.CopyFrom(@params.ToBytes(name).ToByteArray());

            return this;
        }

        /// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractCall;

			ContractId = ContractId.FromProtobuf(body.ContractID);
			Gas = body.Gas;
            PayableAmount = Hbar.FromTinybars(body.Amount);
            FunctionParameters = body.FunctionParameters;
        }

        /// <include file="ContractExecuteTransaction.cs.xml" path='docs/member[@name="M:ContractExecuteTransaction.ToProtobuf"]/*' />
        public Proto.Services.ContractCallTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ContractCallTransactionBody
            {
                Gas = Gas,
                Amount = PayableAmount.ToTinybars(),
                FunctionParameters = FunctionParameters
            };

            if (ContractId != null)
				builder.ContractID = ContractId.ToProtobuf();

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			ContractId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractCall = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractCall = ToProtobuf();
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.contractCallMethod);

			return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
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
