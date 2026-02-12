// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Ids;
using System;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Get the balance of a Hedera™ crypto-currency account. This returns only the balance, so it is a
    /// smaller and faster reply than {@link AccountInfoQuery}.
    /// 
    /// <p>This query is free.
    /// </summary>
    public sealed class AccountBalanceQuery : Query<AccountBalance, AccountBalanceQuery>
    {
        /// <summary>
        /// The account ID for which the balance is being requested.
        /// <p>
        /// This is mutually exclusive with {@link #setContractId(ContractId)}.
        /// </summary>
        public AccountId? AccountId { get; set; }
		/// <summary>
		/// The contract ID for which the balance is being requested.
		/// <p>
		/// This is mutually exclusive with {@link #setAccountId(AccountId)}.
		/// </summary>
		public ContractId? ContractId { get; set; }

		public override bool IsPaymentRequired
		{
			get => false;
		}

		public override void ValidateChecksums(Client client)
        {
            AccountId?.ValidateChecksum(client);
            ContractId?.ValidateChecksum(client);
        }
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
			Proto.CryptoGetAccountBalanceQuery builder = new ()
            {
                Header = header
            };

            if (AccountId != null)
                builder.AccountID = AccountId.ToProtobuf();

            if (ContractId != null)
                builder.ContractID = ContractId.ToProtobuf();

            queryBuilder.CryptogetAccountBalance = builder;
        }
        public override AccountBalance MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return AccountBalance.FromProtobuf(response.CryptogetAccountBalance);
        }
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.CryptogetAccountBalance.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.CryptogetAccountBalance.Header;
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.cryptoGetBalance);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}
	}
}