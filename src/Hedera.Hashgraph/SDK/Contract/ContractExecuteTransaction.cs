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
    /// <summary>
    /// Call a function of the given smart contract instance, giving it parameters as its inputs.
    /// <p>
    /// It can use the given amount of gas, and any unspent gas will be refunded to the paying account.
    /// <p>
    /// If this function stores information, it is charged gas to store it.
    /// There is a fee in hbars to maintain that storage until the expiration time, and that fee is
    /// added as part of the transaction fee.
    /// </summary>
    public sealed class ContractExecuteTransaction : Transaction<ContractExecuteTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContractExecuteTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal ContractExecuteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal ContractExecuteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A maximum limit to the amount of gas to use for this call.
        /// <p>
        /// The network SHALL charge the greater of the following, but
        /// SHALL NOT charge more than the value of this field.
        /// <ol>
        ///   <li>The actual gas consumed by the smart contract call.</li>
        ///   <li>`80%` of this value.</li>
        /// </ol>
        /// The `80%` factor encourages reasonable estimation, while allowing for
        /// some overage to ensure successful execution.
        /// </summary>
        /// <param name="gas">The long to be set as gas</param>
        /// <returns>{@code this}</returns>
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
		/// <summary>
		/// Sets the number of hbars sent with this function call.
		/// </summary>
		public Hbar PayableAmount
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        } = new Hbar(0);
		/// <summary>
		/// Sets the contract instance to call.
		/// </summary>
		/// <param name="contractId">The ContractId to be set</param>
		/// <returns>{@code this}</returns>
		public ContractId? ContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
        /// <summary>
        /// Sets the function parameters as their raw bytes.
        /// <p>
        /// Use this instead of {@link #setFunction(String, ContractFunctionParameters)} if you have already
        /// pre-encoded a solidity function call.
        /// 
        /// This MUST contain The application binary interface (ABI) encoding of the
        /// function call per the Ethereum contract ABI standard, giving the
        /// function signature and arguments being passed to the function.
        /// </summary>
        public ByteString FunctionParameters
        {
            get;
            set
            {
                RequireNotFrozen();
                field = ByteString.CopyFrom(value.ToByteArray());
            }
        } = ByteString.CopyFrom([]);

		/// <summary>
		/// Sets the function name to call.
		/// <p>
		/// The function will be called with no parameters. Use {@link #setFunction(String, ContractFunctionParameters)}
		/// to call a function with parameters.
		/// </summary>
		/// <param name="name">The String to be set as the function name</param>
		/// <returns>{@code this}</returns>
		public ContractExecuteTransaction SetFunction(string name)
        {
            return SetFunction(name, new ContractFunctionParameters());
        }
        /// <summary>
        /// Sets the function to call, and the parameters to pass to the function.
        /// </summary>
        /// <param name="name">The String to be set as the function name</param>
        /// <param name="params">The function parameters to be set</param>
        /// <returns>{@code this}</returns>
        public ContractExecuteTransaction SetFunction(string name, ContractFunctionParameters @params)
        {
            FunctionParameters = ByteString.CopyFrom(@params.ToBytes(name).ToByteArray());

            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractCall;

			ContractId = ContractId.FromProtobuf(body.ContractID);
			Gas = body.Gas;
            PayableAmount = Hbar.FromTinybars(body.Amount);
            FunctionParameters = body.FunctionParameters;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link ContractCallTransactionBody}</returns>
        public Proto.ContractCallTransactionBody ToProtobuf()
        {
            var builder = new Proto.ContractCallTransactionBody
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
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractCall = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractCall = ToProtobuf();
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.contractCallMethod);

			return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}