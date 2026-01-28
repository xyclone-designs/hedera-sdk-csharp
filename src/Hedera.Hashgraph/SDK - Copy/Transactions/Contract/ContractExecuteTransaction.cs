// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Transactions.Contract
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
        private ContractId contractId = null;
        private long gas = 0;
        private Hbar payableAmount = new Hbar(0);
        private byte[] functionParameters = new[]
        {
        };
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContractExecuteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        ContractExecuteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        ContractExecuteTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the contract id.
        /// </summary>
        /// <returns>                         the contract id</returns>
        public ContractId GetContractId()
        {
            return contractId;
        }

        /// <summary>
        /// Sets the contract instance to call.
        /// </summary>
        /// <param name="contractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractExecuteTransaction SetContractId(ContractId contractId)
        {
            ArgumentNullException.ThrowIfNull(contractId);
            RequireNotFrozen();
            contractId = contractId;
            return this;
        }

        /// <summary>
        /// Extract the gas.
        /// </summary>
        /// <returns>                         the gas</returns>
        public long GetGas()
        {
            return gas;
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
        public ContractExecuteTransaction SetGas(long gas)
        {
            RequireNotFrozen();
            if (gas < 0)
            {
                throw new ArgumentException("Gas must be non-negative");
            }

            gas = gas;
            return this;
        }

        /// <summary>
        /// Extract the payable amount.
        /// </summary>
        /// <returns>                         the payable amount in hbar</returns>
        public Hbar GetPayableAmount()
        {
            return payableAmount;
        }

        /// <summary>
        /// Sets the number of hbars sent with this function call.
        /// </summary>
        /// <param name="amount">The Hbar to be set</param>
        /// <returns>{@code this}</returns>
        public ContractExecuteTransaction SetPayableAmount(Hbar amount)
        {
            ArgumentNullException.ThrowIfNull(amount);
            RequireNotFrozen();
            payableAmount = amount;
            return this;
        }

        /// <summary>
        /// Extract the function parameters.
        /// </summary>
        /// <returns>                         the function parameters</returns>
        public ByteString GetFunctionParameters()
        {
            return ByteString.CopyFrom(functionParameters);
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
        /// <param name="functionParameters">The function parameters to be set</param>
        /// <returns>{@code this}</returns>
        public ContractExecuteTransaction SetFunctionParameters(ByteString functionParameters)
        {
            ArgumentNullException.ThrowIfNull(functionParameters);
            RequireNotFrozen();
            functionParameters = functionParameters.ToByteArray();
            return this;
        }

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
            ArgumentNullException.ThrowIfNull(@params);
            return SetFunctionParameters(@params.ToBytes(name));
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.ContractCall();
            if (body.HasContractID())
            {
                contractId = ContractId.FromProtobuf(body.GetContractID());
            }

            gas = body.GetGas();
            payableAmount = Hbar.FromTinybars(body.GetAmount());
            functionParameters = body.GetFunctionParameters().ToByteArray();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link ContractCallTransactionBody}</returns>
        ContractCallTransactionBody.Builder Build()
        {
            var builder = ContractCallTransactionBody.NewBuilder();
            if (contractId != null)
            {
                builder.ContractID(contractId.ToProtobuf());
            }

            builder.Gas(gas);
            builder.Amount(payableAmount.ToTinybars());
            builder.FunctionParameters(ByteString.CopyFrom(functionParameters));
            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (contractId != null)
            {
                contractId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetContractCallMethodMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SetContractCall(Build());
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.SetContractCall(Build());
        }
    }
}