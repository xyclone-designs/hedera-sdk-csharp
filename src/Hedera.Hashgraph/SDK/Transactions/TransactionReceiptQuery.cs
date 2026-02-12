// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;
using System.Linq;

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
		/// <summary>
		/// Create a list of transaction receipts from a protobuf.
		/// </summary>
		/// <param name="protoReceiptList">the protobuf</param>
		/// <returns>                         the list of transaction receipts</returns>
		private static IList<TransactionReceipt> MapReceiptList(IEnumerable<Proto.TransactionReceipt> protoReceiptList)
		{
			return [.. protoReceiptList.Select(_ => TransactionReceipt.FromProtobuf(_))];
		}

		public override bool IsPaymentRequired
		{
			get => false;
		}
		/// <summary>
		/// Extract the transaction id.
		/// </summary>
		/// <returns>                         the transaction id</returns>
		public override TransactionId TransactionIdInternal
        {
            get => TransactionId;
        }

		/// <summary>
		/// Set the ID of the transaction for which the receipt is being requested.
		/// </summary>
		/// <param name="transactionId">The TransactionId to be set</param>
		/// <returns>{@code this}</returns>
		public TransactionId? TransactionId { set; internal get; }
		/// <summary>
		/// Whether the response should include the records of any child transactions spawned by the
		/// top-level transaction with the given transactionID.
		/// </summary>
		/// <param name="value">The value that includeChildren should be set to; true to include children, false to exclude</param>
		/// <returns>{@code this}</returns>
		public bool IncludeChildren { get; set; }
		/// <summary>
		/// Whether records of processing duplicate transactions should be returned along with the record
		/// of processing the first consensus transaction with the given id whose status was neither
		/// INVALID_NODE_ACCOUNT nor INVALID_PAYER_SIGNATURE or, if no such
		/// record exists, the record of processing the first transaction to reach consensus with the
		/// given transaction id.
		/// </summary>
		public bool IncludeDuplicates { get; set; }

		public override void ValidateChecksums(Client client)
        {
			TransactionId?.AccountId.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
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
        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            return (ResponseStatus)response.TransactionGetReceipt.Header.NodeTransactionPrecheckCode;
        }
        public override TransactionReceipt MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            var receiptResponse = response.TransactionGetReceipt;
			var children = MapReceiptList(receiptResponse.ChildTransactionReceipts);
			var duplicates = MapReceiptList(receiptResponse.DuplicateTransactionReceipts);

            return TransactionReceipt.FromProtobuf(response.TransactionGetReceipt.Receipt, duplicates, children, TransactionId);
        }
        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.TransactionGetReceipt.Header;
        }
        public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.TransactionGetReceipt.Header;
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.getTransactionReceipts);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}
		public override ExecutionState GetExecutionState(ResponseStatus status, Proto.Response response)
        {
            switch (status)
            {
                case ResponseStatus.Busy:
				case ResponseStatus.Unknown:
				case ResponseStatus.ReceiptNotFound:
				case ResponseStatus.RecordNotFound:
				case ResponseStatus.PlatformNotActive:
					return ExecutionState.Retry;
                case ResponseStatus.Ok:
                    break;
                default:
                    return ExecutionState.RequestError;
            }

            return (ResponseStatus)response.TransactionGetReceipt.Receipt.Status switch
            {
                ResponseStatus.Busy or 
                ResponseStatus.Unknown or 
                ResponseStatus.Ok or 
                ResponseStatus.ReceiptNotFound or 
                ResponseStatus.RecordNotFound or 
                ResponseStatus.PlatformNotActive => ExecutionState.Retry,

                _ => ExecutionState.Success,
            };
        }
    }
}