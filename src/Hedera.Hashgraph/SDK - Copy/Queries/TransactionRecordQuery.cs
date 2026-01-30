// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

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

        /// <summary>
        /// Extract the transaction id.
        /// </summary>
        /// <returns>                         the transaction id</returns>
        public override TransactionId? GetTransactionIdInternal()
        {
            return TransactionId;
        }

        /// <summary>
        /// Set the ID of the transaction for which the record is requested.
        /// </summary>
        public TransactionId TransactionId { get; set; }

		/// <summary>
		/// Should duplicates be included?
		/// </summary>
		/// <returns>                         should duplicates be included</returns>


		/// <summary>
		/// Whether records of processing duplicate transactions should be returned along with the record
		/// of processing the first consensus transaction with the given id whose status was neither
		/// INVALID_NODE_ACCOUNT nor INVALID_PAYER_SIGNATURE or, if no such
		/// record exists, the record of processing the first transaction to reach consensus with the
		/// given transaction id.
		/// </summary>
		public bool IncludeDuplicates { get; }

        /// <summary>
        /// Whether the response should include the records of any child transactions spawned by the
        /// top-level transaction with the given transactionID.
        /// </summary>
        /// <param name="value">The value that includeChildren should be set to; true to include children, false to exclude</param>
        /// <returns>{@code this}</returns>
        public bool IncludeChildren { get; }

        override void ValidateChecksums(Client client)
        {
            if (TransactionId.AccountId != null)
            {
				TransactionId.AccountId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
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

        override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.TransactionGetRecord.Header;
        }

        override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.TransactionGetRecord.Header;
        }

        override TransactionRecord MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            var recordResponse = response.TransactionGetRecord;

            IList<TransactionRecord> children = MapRecordList(recordResponse.ChildTransactionRecords);
            IList<TransactionRecord> duplicates = MapRecordList(recordResponse.DuplicateTransactionRecords);
            
            return TransactionRecord.FromProtobuf(recordResponse.TransactionRecord, children, duplicates, TransactionId);
        }

        private IList<TransactionRecord> MapRecordList(List<Proto.TransactionRecord> protoRecordList)
        {
            IList<TransactionRecord> outList = new List<TransactionRecord>(protoRecordList.Count);

            foreach (var protoRecord in protoRecordList)
            {
                outList.Add(TransactionRecord.FromProtobuf(protoRecord));
            }

            return outList;
        }

        override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetTxRecordByTxIDMethod();
        }

        override ExecutionState GetExecutionState(Status status, Proto.Response response)
        {
            var retry = base.GetExecutionState(status, response);

            if (retry != ExecutionState.Success)
            {
                return retry;
            }

            switch (status)
            {
                case Status.Busy:
                case Status.Unknown:
                case Status.ReceiptNotFound:
                case Status.RecordNotFound:
                    return ExecutionState.Retry;
                case Status.Ok:

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

            var receiptStatus = (Status)response.TransactionGetRecord.TransactionRecord.Receipt.Status;

            switch (receiptStatus)
            {
                case Status.Busy:
                case Status.Unknown:
                case Status.Ok:
                case Status.ReceiptNotFound:
                case Status.RecordNotFound:
                case Status.PlatformNotActive:
                    return ExecutionState.Retry;
                default:
                    return ExecutionState.Success;
            }
        }
    }
}