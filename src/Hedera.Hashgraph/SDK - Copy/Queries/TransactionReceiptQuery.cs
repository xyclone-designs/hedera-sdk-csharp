// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Queries
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
        internal TransactionId TransactionId = null;
        internal bool IncludeChildren = false;
        internal bool IncludeDuplicates = false;
        
        /// <summary>
        /// Extract the transaction id.
        /// </summary>
        /// <returns>                         the transaction id</returns>
        public override TransactionId GetTransactionIdInternal()
        {
            return TransactionId;
        }

        /// <summary>
        /// Set the ID of the transaction for which the receipt is being requested.
        /// </summary>
        /// <param name="transactionId">The TransactionId to be set</param>
        /// <returns>{@code this}</returns>
        public TransactionReceiptQuery SetTransactionId(TransactionId transactionId)
        {
            ArgumentNullException.ThrowIfNull(transactionId);
            transactionId = transactionId;
            return this;
        }

        /// <summary>
        /// Should the children be included?
        /// </summary>
        /// <returns>                         should children be included</returns>
        public bool GetIncludeChildren()
        {
            return IncludeChildren;
        }

        /// <summary>
        /// Whether the response should include the records of any child transactions spawned by the
        /// top-level transaction with the given transactionID.
        /// </summary>
        /// <param name="value">The value that includeChildren should be set to; true to include children, false to exclude</param>
        /// <returns>{@code this}</returns>
        public TransactionReceiptQuery SetIncludeChildren(bool value)
        {
            IncludeChildren = value;
            return this;
        }

        /// <summary>
        /// Should duplicates be included?
        /// </summary>
        /// <returns>                         should duplicates be included</returns>
        public bool GetIncludeDuplicates()
        {
            return IncludeDuplicates;
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
            IncludeDuplicates = value;
            return this;
        }

        override bool IsPaymentRequired()
        {
            return false;
        }

        override void ValidateChecksums(Client client)
        {
            if (TransactionId != null)
            {
				TransactionId.AccountId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.TransactionGetReceiptQuery
            {
                Header = header,
				IncludeChildReceipts = IncludeChildren,
				IncludeDuplicates = IncludeDuplicates,
			};

            if (TransactionId != null)
            {
                builder.TransactionID = TransactionId.ToProtobuf();
            }

            queryBuilder.TransactionGetReceipt = builder;
        }

        override Status MapResponseStatus(Proto.Response response)
        {
            return (Status)response.TransactionGetReceipt.Header.NodeTransactionPrecheckCode;
        }

        override TransactionReceipt MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            var receiptResponse = response.TransactionGetReceipt;
            var duplicates = MapReceiptList(receiptResponse.DuplicateTransactionReceipts);
            var children = MapReceiptList(receiptResponse.ChildTransactionReceipts);
            return TransactionReceipt.FromProtobuf(response.TransactionGetReceipt.Receipt, duplicates, children, TransactionId);
        }

        /// <summary>
        /// Create a list of transaction receipts from a protobuf.
        /// </summary>
        /// <param name="protoReceiptList">the protobuf</param>
        /// <returns>                         the list of transaction receipts</returns>
        private static IList<TransactionReceipt> MapReceiptList(List<Proto.TransactionReceipt> protoReceiptList)
        {
            IList<TransactionReceipt> outList = new List<TransactionReceipt>(protoReceiptList.Count);
            foreach (var protoReceipt in protoReceiptList)
            {
                outList.Add(TransactionReceipt.FromProtobuf(protoReceipt));
            }

            return outList;
        }

        override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.TransactionGetReceipt.Header;
        }

        override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.TransactionGetReceipt.Header;
        }

        override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetTransactionReceiptsMethod();
        }

        override ExecutionState GetExecutionState(Status status, Proto.Response response)
        {
            switch (status)
            {
                case Status.Busy:
				case Status.Unknown:
				case Status.ReceiptNotFound:
				case Status.RecordNotFound:
				case Status.PlatformNotActive:
					return ExecutionState.Retry;
                case Status.Ok:
                    break;
                default:
                    return ExecutionState.RequestError;
            }

            var receiptStatus = (Status)response.TransactionGetReceipt.Receipt.Status;

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