// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.HBar;

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
		public ContractId? ContractId { get; set; }
		/// <summary>
		/// Sets the amount of gas to use for the call.
		/// <p>
		/// All of the gas offered will be charged for.
		/// </summary>
		/// <param name="gas">The long to be set as gas</param>
		/// <returns>{@code this}</returns>
		public long Gas { get; set; }
        /// <summary>
        /// Sets the function parameters as their raw bytes.
        /// <p>
        /// Use this instead of {@link #setFunction(String, ContractFunctionParameters)} if you have already
        /// pre-encoded a solidity function call.
        /// </summary>
        /// <param name="functionParameters">The function parameters to be set</param>
        /// <returns>{@code this}</returns>
        public byte[] FunctionParameters
        {
            get => field.CopyArray();
            set => field = value.CopyArray();

        } = [];
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
		public long MaXResultSize { get; set; }
		/// <summary>
		/// Sets the account that is the "sender." If not present it is the accountId from the transactionId.
		/// Typically a different value than specified in the transactionId requires a valid signature
		/// over either the hedera transaction or foreign transaction data.
		/// </summary>
		/// <param name="senderAccountId">the account that is the "sender"</param>
		/// <returns>{@code this}</returns>
		public AccountId? SenderAccountId { get; set; }

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
			FunctionParameters = @params.ToBytes(name).ToByteArray();

			return this;
		}

		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// network bug: ContractCallLocal cost estimate is too low

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars((long)(cost.ToTinybars() * 1.1));

		}

		public override void ValidateChecksums(Client client)
        {
			ContractId?.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.ContractCallLocalQuery
            {
				Gas = Gas,
				Header = header,
				FunctionParameters = ByteString.CopyFrom(FunctionParameters)
			};

			if (ContractId != null)
                builder.ContractID = ContractId.ToProtobuf();

            if (SenderAccountId != null)
				builder.SenderId = SenderAccountId.ToProtobuf();

			queryBuilder.ContractCallLocal = builder;
        }
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.ContractCallLocal.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ContractCallLocal.Header;
        }
        public override ContractFunctionResult MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return new ContractFunctionResult(response.ContractCallLocal.FunctionResult);
        }

        public override MethodDescriptor GetMethodDescriptor()
        {
			string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.contractCallLocalMethod);

			return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
    }
}