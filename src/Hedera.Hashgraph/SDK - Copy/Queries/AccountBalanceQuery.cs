// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
        private AccountId accountId = null;
        private ContractId contractId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountBalanceQuery()
        {
        }

        /// <summary>
        /// Return the account's id.
        /// </summary>
        /// <returns>{@code accountId}</returns>
        public AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// The account ID for which the balance is being requested.
        /// <p>
        /// This is mutually exclusive with {@link #setContractId(ContractId)}.
        /// </summary>
        /// <param name="accountId">The AccountId to set</param>
        /// <returns>{@code this}</returns>
        public AccountBalanceQuery SetAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            accountId = accountId;
            return this;
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
        /// The contract ID for which the balance is being requested.
        /// <p>
        /// This is mutually exclusive with {@link #setAccountId(AccountId)}.
        /// </summary>
        /// <param name="contractId">The ContractId to set</param>
        /// <returns>{@code this}</returns>
        public AccountBalanceQuery SetContractId(ContractId contractId)
        {
            ArgumentNullException.ThrowIfNull(contractId);
            contractId = contractId;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }

            if (contractId != null)
            {
                contractId.ValidateChecksum(client);
            }
        }

        override bool IsPaymentRequired()
        {
            return false;
        }

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = CryptoGetAccountBalanceQuery.NewBuilder();
            if (accountId != null)
            {
                builder.AccountID(accountId.ToProtobuf());
            }

            if (contractId != null)
            {
                builder.ContractID(contractId.ToProtobuf());
            }

            queryBuilder.SetCryptogetAccountBalance(builder.Header(header));
        }

        override AccountBalance MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return AccountBalance.FromProtobuf(response.GetCryptogetAccountBalance());
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetCryptogetAccountBalance().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetCryptogetAccountBalance().GetHeader();
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetCryptoGetBalanceMethod();
        }
    }
}