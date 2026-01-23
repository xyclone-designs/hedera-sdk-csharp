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

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Get the receipt of a transaction, given its transaction ID.
    /// 
    /// <p>Once a transaction reaches consensus, then information about whether it succeeded or failed
    /// will be available until the end of the receipt period.
    /// 
    /// <p>This query is free.
    /// </summary>
    public sealed class TransactionReceiptQuery : Query<TransactionReceipt, TransactionReceiptQuery>
    {
        private TransactionId transactionId = null;
        private bool includeChildren = false;
        private bool includeDuplicates = false;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TransactionReceiptQuery()
        {
        }

        /// <summary>
        /// Extract the transaction id.
        /// </summary>
        /// <returns>                         the transaction id</returns>
        public override TransactionId GetTransactionIdInternal()
        {
            return transactionId;
        }

        /// <summary>
        /// Set the ID of the transaction for which the receipt is being requested.
        /// </summary>
        /// <param name="transactionId">The TransactionId to be set</param>
        /// <returns>{@code this}</returns>
        public TransactionReceiptQuery SetTransactionId(TransactionId transactionId)
        {
            Objects.RequireNonNull(transactionId);
            transactionId = transactionId;
            return this;
        }

        /// <summary>
        /// Should the children be included?
        /// </summary>
        /// <returns>                         should children be included</returns>
        public bool GetIncludeChildren()
        {
            return includeChildren;
        }

        /// <summary>
        /// Whether the response should include the records of any child transactions spawned by the
        /// top-level transaction with the given transactionID.
        /// </summary>
        /// <param name="value">The value that includeChildren should be set to; true to include children, false to exclude</param>
        /// <returns>{@code this}</returns>
        public TransactionReceiptQuery SetIncludeChildren(bool value)
        {
            includeChildren = value;
            return this;
        }

        /// <summary>
        /// Should duplicates be included?
        /// </summary>
        /// <returns>                         should duplicates be included</returns>
        public bool GetIncludeDuplicates()
        {
            return includeDuplicates;
        }

        /// <summary>
        /// Whether records of processing duplicate transactions should be returned along with the record
        /// of processing the first consensus transaction with the given id whose status was neither
        /// INVALID_NODE_ACCOUNT nor INVALID_PAYER_SIGNATURE or, if no such
        /// record exists, the record of processing the first transaction to reach consensus with the
        /// given transaction id.
        /// </summary>
        /// <param name="value">The value that includeDuplicates should be set to; true to include duplicates, false to exclude</param>
        /// <returns>{@code this}</returns>
        public TransactionReceiptQuery SetIncludeDuplicates(bool value)
        {
            includeDuplicates = value;
            return this;
        }

        override bool IsPaymentRequired()
        {
            return false;
        }

        override void ValidateChecksums(Client client)
        {
            if (transactionId != null)
            {
                Objects.RequireNonNull(transactionId.accountId).ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = TransactionGetReceiptQuery.NewBuilder().SetIncludeChildReceipts(includeChildren).SetIncludeDuplicates(includeDuplicates);
            if (transactionId != null)
            {
                builder.SetTransactionID(transactionId.ToProtobuf());
            }

            queryBuilder.SetTransactionGetReceipt(builder.SetHeader(header));
        }

        override Status MapResponseStatus(Response response)
        {
            var preCheckCode = response.GetTransactionGetReceipt().GetHeader().GetNodeTransactionPrecheckCode();
            return Status.ValueOf(preCheckCode);
        }

        override TransactionReceipt MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            var receiptResponse = response.GetTransactionGetReceipt();
            var duplicates = MapReceiptList(receiptResponse.GetDuplicateTransactionReceiptsList());
            var children = MapReceiptList(receiptResponse.GetChildTransactionReceiptsList());
            return TransactionReceipt.FromProtobuf(response.GetTransactionGetReceipt().GetReceipt(), duplicates, children, transactionId);
        }

        /// <summary>
        /// Create a list of transaction receipts from a protobuf.
        /// </summary>
        /// <param name="protoReceiptList">the protobuf</param>
        /// <returns>                         the list of transaction receipts</returns>
        private static IList<TransactionReceipt> MapReceiptList(List<Proto.TransactionReceipt> protoReceiptList)
        {
            IList<TransactionReceipt> outList = new List(protoReceiptList.Count);
            foreach (var protoReceipt in protoReceiptList)
            {
                outList.Add(TransactionReceipt.FromProtobuf(protoReceipt));
            }

            return outList;
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetTransactionGetReceipt().GetHeader();
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetTransactionGetReceipt().GetHeader();
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetTransactionReceiptsMethod();
        }

        override ExecutionState GetExecutionState(Status status, Response response)
        {
            switch (status)
            {
                case BUSY:
                case UNKNOWN:
                case RECEIPT_NOT_FOUND:
                case RECORD_NOT_FOUND:
                case PLATFORM_NOT_ACTIVE:
                    return ExecutionState.RETRY;
                case OK:
                    break;
                default:
                    return ExecutionState.REQUEST_ERROR;
                    break;
            }

            var receiptStatus = Status.ValueOf(response.GetTransactionGetReceipt().GetReceipt().GetStatus());
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