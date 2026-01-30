// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Call a function of the given smart contract instance, giving it functionParameters as its inputs.
    /// It will consume the entire given amount of gas.
    /// <p>
    /// This is performed locally on the particular node that the client is communicating with.
    /// It cannot change the state of the contract instance (and so, cannot spend
    /// anything from the instance's cryptocurrency account).
    /// <p>
    /// It will not have a consensus timestamp. It cannot generate a record or a receipt.
    /// The response will contain the output returned by the function call.
    /// This is useful for calling getter functions, which purely read the state and don't change it.
    /// It is faster and cheaper than a normal call, because it is purely local to a single  node.
    /// </summary>
    public sealed class ContractCallQuery : Query<ContractFunctionResult, ContractCallQuery>
    {
        private ContractId contractId = null;
        private long gas = 0;
        private byte[] functionParameters = [];
        private long maxResultSize = 0;
        private AccountId senderAccountId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContractCallQuery()
        {
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
        /// Sets the contract instance to call, in the format used in transactions.
        /// </summary>
        /// <param name="contractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractCallQuery SetContractId(ContractId contractId)
        {
            ArgumentNullException.ThrowIfNull(contractId);
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
        /// Sets the amount of gas to use for the call.
        /// <p>
        /// All of the gas offered will be charged for.
        /// </summary>
        /// <param name="gas">The long to be set as gas</param>
        /// <returns>{@code this}</returns>
        public ContractCallQuery SetGas(long gas)
        {
            gas = gas;
            return this;
        }

        public override Task<Hbar> GetCostAsync(Client client)
        {

            // network bug: ContractCallLocal cost estimate is too low
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars((long)(cost.ToTinybars() * 1.1)));
        }

        /// <summary>
        /// Extract the function parameters.
        /// </summary>
        /// <returns>                         the byte string representation</returns>
        public ByteString GetFunctionParameters()
        {
            return ByteString.CopyFrom(functionParameters);
        }

        /// <summary>
        /// Sets the function parameters as their raw bytes.
        /// <p>
        /// Use this instead of {@link #setFunction(String, ContractFunctionParameters)} if you have already
        /// pre-encoded a solidity function call.
        /// </summary>
        /// <param name="functionParameters">The function parameters to be set</param>
        /// <returns>{@code this}</returns>
        public ContractCallQuery SetFunctionParameters(byte[] functionParameters)
        {
            functionParameters = functionParameters.CopyArray();
            return this;
        }

        /// <summary>
        /// Sets the function name to call.
        /// <p>
        /// The function will be called with no parameters. Use {@link #setFunction(String, ContractFunctionParameters)}
        /// to call a function with parameters.
        /// </summary>
        /// <param name="name">The function name to be set</param>
        /// <returns>{@code this}</returns>
        public ContractCallQuery SetFunction(string name)
        {
            return SetFunction(name, new ContractFunctionParameters());
        }

        /// <summary>
        /// Sets the function to call, and the parameters to pass to the function.
        /// </summary>
        /// <param name="name">The function name to be set</param>
        /// <param name="params">The parameters to pass</param>
        /// <returns>{@code this}</returns>
        public ContractCallQuery SetFunction(string name, ContractFunctionParameters @params)
        {
            ArgumentNullException.ThrowIfNull(@params);
            SetFunctionParameters(@params.ToBytes(name).ToByteArray());
            return this;
        }

        /// <summary>
        /// Get the max number of bytes that the result might include
        /// </summary>
        /// <returns>the max number of byes</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public long GetMaxResultSize()
        {
            return maxResultSize;
        }

        /// <summary>
        /// </summary>
        /// <param name="size">The long to be set as size</param>
        /// <returns>{@code this}</returns>
        /// <remarks>
        /// @deprecatedwith no replacement
        /// <br>
        /// Sets the max number of bytes that the result might include.
        /// The run will fail if it had returned more than this number of bytes.
        /// </remarks>
        public ContractCallQuery SetMaxResultSize(long size)
        {
            maxResultSize = size;
            return this;
        }

        /// <summary>
        /// Get the sender account ID
        /// </summary>
        /// <returns>the account that is the "sender"</returns>
        public AccountId GetSenderAccountId()
        {
            return senderAccountId;
        }

        /// <summary>
        /// Sets the account that is the "sender." If not present it is the accountId from the transactionId.
        /// Typically a different value than specified in the transactionId requires a valid signature
        /// over either the hedera transaction or foreign transaction data.
        /// </summary>
        /// <param name="senderAccountId">the account that is the "sender"</param>
        /// <returns>{@code this}</returns>
        public ContractCallQuery SetSenderAccountId(AccountId senderAccountId)
        {
            ArgumentNullException.ThrowIfNull(senderAccountId);
            senderAccountId = senderAccountId;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (contractId != null)
            {
                contractId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.ContractCallLocalQuery
            {
                Header = header
            };

            if (contractId != null)
            {
                builder.ContractID = contractId.ToProtobuf();
            }

            builder.Gas = gas;
            builder.FunctionParameters = ByteString.CopyFrom(functionParameters);
            
            if (senderAccountId != null)
            {
                builder.SenderId = senderAccountId.ToProtobuf();
            }

            queryBuilder.ContractCallLocal = builder;
        }

        override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ContractCallLocal.Header;
        }

        override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.ContractCallLocal.Header;
        }

        override ContractFunctionResult MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return new ContractFunctionResult(response.ContractCallLocal.FunctionResult);
        }

        override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetContractCallLocalMethodMethod();
        }
    }
}