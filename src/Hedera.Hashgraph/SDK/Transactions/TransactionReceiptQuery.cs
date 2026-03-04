// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <include file="TransactionReceiptQuery.cs.xml" path='docs/member[@name="T:TransactionReceiptQuery"]/*' />
    public sealed class TransactionReceiptQuery : Query<TransactionReceipt, TransactionReceiptQuery>
    {
		/// <include file="TransactionReceiptQuery.cs.xml" path='docs/member[@name="M:TransactionReceiptQuery.MapReceiptList(System.Collections.Generic.IEnumerable{Proto.TransactionReceipt})"]/*' />
		private static IList<TransactionReceipt> MapReceiptList(IEnumerable<Proto.TransactionReceipt> protoReceiptList)
		{
			return [.. protoReceiptList.Select(_ => TransactionReceipt.FromProtobuf(_))];
		}

		public override bool IsPaymentRequired
		{
			get => false;
		}
		/// <include file="TransactionReceiptQuery.cs.xml" path='docs/member[@name="T:TransactionReceiptQuery_2"]/*' />
		public override TransactionId TransactionIdInternal
        {
            get => TransactionId;
        }

		/// <include file="TransactionReceiptQuery.cs.xml" path='docs/member[@name="P:TransactionReceiptQuery.TransactionId"]/*' />
		public TransactionId? TransactionId { set; internal get; }
		/// <include file="TransactionReceiptQuery.cs.xml" path='docs/member[@name="P:TransactionReceiptQuery.IncludeChildren"]/*' />
		public bool IncludeChildren { get; set; }
		/// <include file="TransactionReceiptQuery.cs.xml" path='docs/member[@name="P:TransactionReceiptQuery.IncludeDuplicates"]/*' />
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