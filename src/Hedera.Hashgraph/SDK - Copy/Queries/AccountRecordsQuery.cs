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

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = CryptoGetAccountRecordsQuery.NewBuilder();
            if (accountId != null)
            {
                builder.AccountID(accountId.ToProtobuf());
            }

            queryBuilder.SetCryptoGetAccountRecords(builder.Header(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetCryptoGetAccountRecords().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetCryptoGetAccountRecords().GetHeader();
        }

        override IList<TransactionRecord> MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            var rawTransactionRecords = response.GetCryptoGetAccountRecords().GetRecordsList();
            var transactionRecords = new List<TransactionRecord>(rawTransactionRecords.Count);
            foreach (var record in rawTransactionRecords)
            {
                transactionRecords.Add(TransactionRecord.FromProtobuf(record));
            }

            return transactionRecords;
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetAccountRecordsMethod();
        }
    }
}