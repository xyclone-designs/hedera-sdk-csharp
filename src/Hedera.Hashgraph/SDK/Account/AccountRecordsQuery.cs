// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Account
{
	/// <summary>
	/// Get all the records for an account for any transfers into it and out of it,
	/// that were above the threshold, during the last 25 hours.
	/// </summary>
	public sealed class AccountRecordsQuery : Query<IList<TransactionRecord>, AccountRecordsQuery>
    {
        /// <summary>
        /// Sets the account ID for which the records should be retrieved.
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountId? AccountId { get; set; }

		public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.CryptoGetAccountRecordsQuery
            {
                Header = header,
            };

            if (AccountId != null)
				builder.AccountID = AccountId.ToProtobuf();

			queryBuilder.CryptoGetAccountRecords = builder;
        }
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.CryptoGetAccountRecords.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.CryptoGetAccountRecords.Header;
        }
        public override IList<TransactionRecord> MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            var rawTransactionRecords = response.CryptoGetAccountRecords.Records;

            var transactionRecords = new List<TransactionRecord>(rawTransactionRecords.Count);
            foreach (var record in rawTransactionRecords)
            {
                transactionRecords.Add(TransactionRecord.FromProtobuf(record));
            }

            return transactionRecords;
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.getAccountRecords);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}
	}
}