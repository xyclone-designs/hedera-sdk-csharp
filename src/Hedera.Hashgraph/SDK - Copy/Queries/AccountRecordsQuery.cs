// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Get all the records for an account for any transfers into it and out of it,
    /// that were above the threshold, during the last 25 hours.
    /// </summary>
    public sealed class AccountRecordsQuery : Query<IList<TransactionRecord>, AccountRecordsQuery>
    {
        private AccountId accountId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountRecordsQuery()
        {
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        public AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// Sets the account ID for which the records should be retrieved.
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountRecordsQuery SetAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            accountId = accountId;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.CryptoGetAccountRecordsQuery
            {
                Header = header,
            };

            if (accountId != null)
            {
                builder.AccountID = accountId.ToProtobuf();
            }

            queryBuilder.CryptoGetAccountRecords = builder;
        }

        override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.CryptoGetAccountRecords.Header;
        }

        override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.CryptoGetAccountRecords.Header;
        }

        override IList<TransactionRecord> MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            var rawTransactionRecords = response.CryptoGetAccountRecords.RecordsList;

            var transactionRecords = new List<TransactionRecord>(rawTransactionRecords.Count);
            foreach (var record in rawTransactionRecords)
            {
                transactionRecords.Add(TransactionRecord.FromProtobuf(record));
            }

            return transactionRecords;
        }

        override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetAccountRecordsMethod();
        }
    }
}