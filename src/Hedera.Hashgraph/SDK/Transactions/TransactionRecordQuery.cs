// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Get the record for a transaction.
    /// <p>
    /// If the transaction requested a record, then the record lasts for one hour, and a state proof is available for it.
    /// If the transaction created an account, file, or smart contract instance, then the record will contain the ID for
    /// what it created. If the transaction called a smart contract function, then the record contains the result of
    /// that call. If the transaction was a cryptocurrency transfer, then the record includes the TransferList
    /// which gives the details of that transfer. If the transaction didn't return anything that should be
    /// in the record, then the results field will be set to nothing.
    /// </summary>
    public sealed class TransactionRecordQuery : Query<TransactionRecord, TransactionRecordQuery>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TransactionRecordQuery() { }

		private IList<TransactionRecord> MapRecordList(IEnumerable<Proto.TransactionRecord> protoRecordList)
		{
            return [.. protoRecordList.Select(_ => TransactionRecord.FromProtobuf(_))];
		}


		/// <summary>
		/// Whether the response should include the records of any child transactions spawned by the
		/// top-level transaction with the given transactionID.
		/// </summary>
		/// <param name="value">The value that includeChildren should be set to; true to include children, false to exclude</param>
		/// <returns>{@code this}</returns>
		public bool IncludeChildren { get; }
		/// <summary>
		/// Whether records of processing duplicate transactions should be returned along with the record
		/// of processing the first consensus transaction with the given id whose status was neither
		/// INVALID_NODE_ACCOUNT nor INVALID_PAYER_SIGNATURE or, if no such
		/// record exists, the record of processing the first transaction to reach consensus with the
		/// given transaction id.
		/// </summary>
		public bool IncludeDuplicates { get; }
		/// <summary>
		/// Set the ID of the transaction for which the record is requested.
		/// </summary>
		public TransactionId? TransactionId { get; set; }
		/// <summary>
		/// Extract the transaction id.
		/// </summary>
		/// <returns>                         the transaction id</returns>
		public override TransactionId TransactionIdInternal
		{
			get => TransactionId;
		}

		public override void ValidateChecksums(Client client)
        {
			TransactionId?.AccountId?.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.TransactionGetRecordQuery
            {
                Header = header,
				IncludeChildRecords = IncludeChildren,
				IncludeDuplicates = IncludeDuplicates,
			};           

            if (TransactionId != null)
            {
                builder.TransactionID = TransactionId.ToProtobuf();
            }

            queryBuilder.TransactionGetRecord = builder;
		}
		public override TransactionRecord MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
		{
			var recordResponse = response.TransactionGetRecord;

			IList<TransactionRecord> children = MapRecordList(recordResponse.ChildTransactionRecords);
			IList<TransactionRecord> duplicates = MapRecordList(recordResponse.DuplicateTransactionRecords);

			return TransactionRecord.FromProtobuf(recordResponse.TransactionRecord, children, duplicates, TransactionId);
		}
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.TransactionGetRecord.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.TransactionGetRecord.Header;
        }
        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetTxRecordByTxIDMethod();
        }

        public override ExecutionState GetExecutionState(ResponseStatus status, Proto.Response response)
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