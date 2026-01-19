using System.Net.NetworkInformation;
using System.Transactions;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Get the receipt of a transaction, given its transaction ID.
	 *
	 * <p>Once a transaction reaches consensus, then information about whether it succeeded or failed
	 * will be available until the end of the receipt period.
	 *
	 * <p>This query is free.
	 */
	public sealed class TransactionReceiptQuery : Query<TransactionReceipt, TransactionReceiptQuery> 
	{
		/**
		 * Whether the response should include the records of any child transactions spawned by the
		 * top-level transaction with the given transactionID.
		 */
		public bool IncludeChildren { get; set; }
		/**
		 * Whether records of processing duplicate transactions should be returned along with the record
		 * of processing the first consensus transaction with the given id whose status was neither
		 * INVALID_NODE_ACCOUNT nor INVALID_PAYER_SIGNATURE or, if no such
		 * record exists, the record of processing the first transaction to reach consensus with the
		 * given transaction id.
		 */
		public bool IncludeDuplicates { get; set; }
		/**
		 * Set the ID of the transaction for which the receipt is being requested.
		 */
		public TransactionId? TransactionId { get; set; }

		/**
		 * Constructor.
		 */
		public TransactionReceiptQuery() { }

		/**
		 * Extract the transaction id.
		 *
		 * @return                          the transaction id
		 */
		public override TransactionId? GetTransactionIdInternal()
		{
			return TransactionId;
		}

		
		public override bool IsPaymentRequired()
		{
			return false;
		}

		public override void ValidateChecksums(Client client) 
		{
			if (transactionId != null) {
				Objects.requireNonNull(transactionId.accountId).validateChecksum(client);
			}
		}

		public override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
		{
			var builder = TransactionGetReceiptQuery.newBuilder()
					.setIncludeChildReceipts(includeChildren)
					.setIncludeDuplicates(includeDuplicates);
			if (transactionId != null)
			{
				builder.setTransactionID(transactionId.ToProtobuf());
			}

			queryBuilder.setTransactionGetReceipt(builder.setHeader(header));
		}

		public override Status MapResponseStatus(Response response)
		{
			var preCheckCode = response.getTransactionGetReceipt().getHeader().getNodeTransactionPrecheckCode();

			return Status.valueOf(preCheckCode);
		}

		public override TransactionReceipt MapResponse(Response response, AccountId nodeId, Proto.Query request)
		{
			var receiptResponse = response.getTransactionGetReceipt();
			var duplicates = mapReceiptList(receiptResponse.getDuplicateTransactionReceiptsList());
			var children = mapReceiptList(receiptResponse.getChildTransactionReceiptsList());
			return TransactionReceipt.FromProtobuf(
					response.getTransactionGetReceipt().getReceipt(), duplicates, children, transactionId);
		}

		/**
		 * Create a list of transaction receipts from a protobuf.
		 *
		 * @param protoReceiptList          the protobuf
		 * @return                          the list of transaction receipts
		 */
		private static List<TransactionReceipt> mapReceiptList(
				List<Proto.TransactionReceipt> protoReceiptList)
		{
			List<TransactionReceipt> outList = new ArrayList<>(protoReceiptList.size());
			for (var protoReceipt : protoReceiptList)
			{
				outList.Add(TransactionReceipt.FromProtobuf(protoReceipt));
			}
			return outList;
		}

		public override QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.getTransactionGetReceipt().getHeader();
		}

		public override ResponseHeader MapResponseHeader(Response response)
		{
			return response.getTransactionGetReceipt().getHeader();
		}

		@Override
		MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
			return CryptoServiceGrpc.getGetTransactionReceiptsMethod();
		}

		public override ExecutionState GetExecutionState(Status status, Response response)
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
			}

			var receiptStatus =
					Status.valueOf(response.getTransactionGetReceipt().getReceipt().getStatus());

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
			}
		}
	}

}