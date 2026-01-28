// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Proto;
using Hedera.Hashgraph.SDK.Transactions;
using Io.Grpc;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

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
            if (transactionId != null)
            {
                ArgumentNullException.ThrowIfNull(transactionId.accountId).ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = TransactionGetRecordQuery.NewBuilder().SetIncludeChildRecords(includeChildren).SetIncludeDuplicates(includeDuplicates);
            if (transactionId != null)
            {
                builder.TransactionID(transactionId.ToProtobuf());
            }

            queryBuilder.SetTransactionGetRecord(builder.Header(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetTransactionGetRecord().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetTransactionGetRecord().GetHeader();
        }

        override TransactionRecord MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            var recordResponse = response.GetTransactionGetRecord();
            IList<TransactionRecord> children = MapRecordList(recordResponse.GetChildTransactionRecordsList());
            IList<TransactionRecord> duplicates = MapRecordList(recordResponse.GetDuplicateTransactionRecordsList());
            return TransactionRecord.FromProtobuf(recordResponse.GetTransactionRecord(), children, duplicates, transactionId);
        }

        private IList<TransactionRecord> MapRecordList(List<Proto.TransactionRecord> protoRecordList)
        {
            IList<TransactionRecord> outList = new List(protoRecordList.Count);
            foreach (var protoRecord in protoRecordList)
            {
                outList.Add(TransactionRecord.FromProtobuf(protoRecord));
            }

            return outList;
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetTxRecordByTxIDMethod();
        }

        override ExecutionState GetExecutionState(Status status, Response response)
        {
            var retry = base.GetExecutionState(status, response);
            if (retry != ExecutionState.SUCCESS)
            {
                return retry;
            }

            switch (status)
            {
                case BUSY:
                case UNKNOWN:
                case RECEIPT_NOT_FOUND:
                case RECORD_NOT_FOUND:
                    return ExecutionState.RETRY;
                case OK:

                    // When fetching payment an `OK` in the query header means the cost is in the response
                    if (paymentTransactions == null || paymentTransactions.IsEmpty)
                    {
                        return ExecutionState.SUCCESS;
                    }
                    else
                    {
                        break;
                    }

                default:
                    return ExecutionState.REQUEST_ERROR;
                    break;
            }

            var receiptStatus = Status.ValueOf(response.GetTransactionGetRecord().GetTransactionRecord().GetReceipt().GetStatus());
            switch (receiptStatus)
            {
                case BUSY:
                case UNKNOWN:
                case OK:
                case RECEIPT_NOT_FOUND:
                case RECORD_NOT_FOUND:
                case PLATFORM_NOT_ACTIVE:
                    return ExecutionState.RETRY;
                default:
                    return ExecutionState.SUCCESS;
                    break;
            }
        }
    }
}