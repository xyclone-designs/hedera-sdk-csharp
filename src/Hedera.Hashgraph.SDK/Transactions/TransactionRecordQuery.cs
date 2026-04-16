// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <include file="TransactionRecordQuery.cs.xml" path='docs/member[@name="T:TransactionRecordQuery"]/*' />
    public sealed class TransactionRecordQuery : Query<TransactionRecord, TransactionRecordQuery>
    {
        /// <include file="TransactionRecordQuery.cs.xml" path='docs/member[@name="M:TransactionRecordQuery.#ctor"]/*' />
        public TransactionRecordQuery() { }

		private List<TransactionRecord> MapRecordList(IEnumerable<Proto.Services.TransactionRecord> protoRecordList)
		{
            return [.. protoRecordList.Select(_ => TransactionRecord.FromProtobuf(_))];
		}


		/// <include file="TransactionRecordQuery.cs.xml" path='docs/member[@name="P:TransactionRecordQuery.IncludeChildren"]/*' />
		public bool IncludeChildren { get; set; }
		/// <include file="TransactionRecordQuery.cs.xml" path='docs/member[@name="P:TransactionRecordQuery.IncludeDuplicates"]/*' />
		public bool IncludeDuplicates { get; set; }
		/// <include file="TransactionRecordQuery.cs.xml" path='docs/member[@name="P:TransactionRecordQuery.TransactionId"]/*' />
		public TransactionId? TransactionId { get; set; }
		/// <include file="TransactionRecordQuery.cs.xml" path='docs/member[@name="M:TransactionRecordQuery.ValidateChecksums(Client)"]/*' />
		public override TransactionId TransactionIdInternal
		{
			get => TransactionId;
		}

		public override void ValidateChecksums(Client client)
        {
			TransactionId?.AccountId?.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
            var builder = new Proto.Services.TransactionGetRecordQuery
            {
                Header = header,
				IncludeChildRecords = IncludeChildren,
				IncludeDuplicates = IncludeDuplicates,
			};           

            if (TransactionId != null)
            {
                builder.TransactionId = TransactionId.ToProtobuf();
            }

            queryBuilder.TransactionGetRecord = builder;
		}
		public override TransactionRecord MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
		{
			var recordResponse = response.TransactionGetRecord;

			IList<TransactionRecord> children = MapRecordList(recordResponse.ChildTransactionRecords);
			IList<TransactionRecord> duplicates = MapRecordList(recordResponse.DuplicateTransactionRecords);

			return TransactionRecord.FromProtobuf(recordResponse.TransactionRecord, children, duplicates, TransactionId);
		}
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.TransactionGetRecord.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.TransactionGetRecord.Header;
        }
       
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.CryptoService.CryptoServiceClient.getTxRecordByTxID);

			return Proto.Services.CryptoService.Descriptor.FindMethodByName(methodname);
		}

		public override ExecutionState GetExecutionState(ResponseStatus status, Proto.Services.Response response)
        {
            var retry = base.GetExecutionState(status, response);

            if (retry != ExecutionState.Success)
            {
                return retry;
            }

            switch (status)
            {
                case ResponseStatus.Busy:
                case ResponseStatus.Unknown:
                case ResponseStatus.ReceiptNotFound:
                case ResponseStatus.RecordNotFound:
                    return ExecutionState.Retry;
                case ResponseStatus.Ok:

                    // When fetching payment an `OK` in the query header means the cost is in the response
                    if (PaymentTransactions == null || PaymentTransactions.Count > 0)
                    {
                        return ExecutionState.Success;
                    }
                    else
                    {
                        break;
                    }

                default:
                    return ExecutionState.RequestError;
            }

            var receiptStatus = (ResponseStatus)response.TransactionGetRecord.TransactionRecord.Receipt.Status;

            switch (receiptStatus)
            {
                case ResponseStatus.Busy:
                case ResponseStatus.Unknown:
                case ResponseStatus.Ok:
                case ResponseStatus.ReceiptNotFound:
                case ResponseStatus.RecordNotFound:
                case ResponseStatus.PlatformNotActive:
                    return ExecutionState.Retry;
                default:
                    return ExecutionState.Success;
            }
        }
    }
}
