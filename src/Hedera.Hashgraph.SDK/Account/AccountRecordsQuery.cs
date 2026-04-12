// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Account
{
	/// <include file="AccountRecordsQuery.cs.xml" path='docs/member[@name="T:AccountRecordsQuery"]/*' />
	public sealed class AccountRecordsQuery : Query<IList<TransactionRecord>, AccountRecordsQuery>
    {
        /// <include file="AccountRecordsQuery.cs.xml" path='docs/member[@name="P:AccountRecordsQuery.AccountId"]/*' />
        public AccountId? AccountId { get; set; }

		public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
            var builder = new Proto.Services.CryptoGetAccountRecordsQuery
            {
                Header = header,
            };

            if (AccountId != null)
				builder.AccountID = AccountId.ToProtobuf();

			queryBuilder.CryptoGetAccountRecords = builder;
        }
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.CryptoGetAccountRecords.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.CryptoGetAccountRecords.Header;
        }
        public override IList<TransactionRecord> MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
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
			string methodname = nameof(Proto.Services.CryptoService.CryptoServiceClient.getAccountRecords);

			return Proto.Services.CryptoService.Descriptor.FindMethodByName(methodname);
		}
	}
}
