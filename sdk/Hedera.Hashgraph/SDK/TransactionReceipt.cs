using Google.Protobuf;
using Google.Protobuf.Collections;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hedera.Hashgraph.SDK
{
	/**
 * The consensus result for a transaction, which might not be currently
 * known, or may succeed or fail.
 */
	public sealed class TransactionReceipt
	{
		TransactionReceipt(
			TransactionId? transactionId,
			Status status,
			ExchangeRate exchangeRate,
			ExchangeRate nextExchangeRate,
			AccountId? accountId,
			FileId? fileId,
			ContractId? contractId,
			TopicId? topicId,
			TokenId? tokenId,
			long? topicSequenceNumber,
			ByteString? topicRunningHash,
			long totalSupply,
			ScheduleId? scheduleId,
			TransactionId? scheduledTransactionId,
			List<long> serials,
			long nodeId,
			List<TransactionReceipt> duplicates,
			List<TransactionReceipt> children)
		{
			TransactionId = transactionId;
			Status = status;
			ExchangeRate = exchangeRate;
			NextExchangeRate = nextExchangeRate;
			AccountId = accountId;
			FileId = fileId;
			ContractId = contractId;
			TopicId = topicId;
			TokenId = tokenId;
			TopicSequenceNumber = topicSequenceNumber;
			TopicRunningHash = topicRunningHash;
			TotalSupply = totalSupply;
			ScheduleId = scheduleId;
			ScheduledTransactionId = scheduledTransactionId;
			Serials = serials;
			NodeId = nodeId;
			Duplicates = duplicates;
			Children = children;
		}

		/**
		* In the receipt of a NodeCreate, NodeUpdate, NodeDelete, the id of the newly created node.
		* An affected node identifier.<br/>
		* This value SHALL be set following a `createNode` transaction.<br/>
		* This value SHALL be set following a `updateNode` transaction.<br/>
		* This value SHALL be set following a `deleteNode` transaction.<br/>
		* This value SHALL NOT be set following any other transaction.
		*/
		public long NodeId { get; }
		/**
		* In the receipt of TokenMint, TokenWipe, TokenBurn, For fungible tokens - the current total
		* supply of this token. For non fungible tokens - the total number of NFTs issued for a given
		* tokenID
		*/
		public long TotalSupply { get; }
		/**
		* Updated sequence number for a consensus service topic.
		* Set for {@link TopicMessageSubmitTransaction}.
		*/
		public long? TopicSequenceNumber { get; }
		/**
		 * Whether the transaction succeeded or failed (or is unknown).
		 */
		public Status Status { get; }

		/**
		 * The transaction's ID
		 */
		public TransactionId? TransactionId { get; }
		/**
		 * The exchange rate of Hbars to cents (USD).
		 */
		public ExchangeRate ExchangeRate { get; }
		/**
		 * Next exchange rate which will take effect when current rate expires
		 */
		public ExchangeRate NextExchangeRate { get; }
		/**
		 * The account ID, if a new account was created.
		 */
		public AccountId? AccountId { get; }
		/**
		 * The file ID, if a new file was created.
		 */
		public FileId? FileId { get; }
		/**
		 * The contract ID, if a new contract was created.
		 */
		public ContractId? ContractId { get; }
		/**
		* The topic ID, if a new topic was created.
		*/
		public TopicId? TopicId { get; }
		/**
		* The token ID, if a new token was created.
		*/
		public TokenId? TokenId { get; }
		/**
		* Updated running hash for a consensus service topic.
		* Set for {@link TopicMessageSubmitTransaction}.
		*/
		public ByteString? TopicRunningHash { get; }
		/**
		* In the receipt of a ScheduleCreate, the id of the newly created Scheduled Entity
		*/
		public ScheduleId? ScheduleId { get; }
		/**
		* In the receipt of a ScheduleCreate or ScheduleSign that resolves to SUCCESS, the
		* TransactionID that should be used to query for the receipt or record of the relevant
		* scheduled transaction
		*/
		public TransactionId? ScheduledTransactionId { get; }

		/**
		* In the receipt of a TokenMint for tokens of type NON_FUNGIBLE_UNIQUE, the serial numbers of
		* the newly created NFTs
		*/
		public List<long> Serials { get; }
		/**
			* The receipts of processing all transactions with the given id, in consensus time order.
			*/
		public List<TransactionReceipt> Duplicates { get; }
		/**
			* The receipts (if any) of all child transactions spawned by the transaction with the
			* given top-level id, in consensus order. Always empty if the top-level status is UNKNOWN.
			*/
		public List<TransactionReceipt> Children { get; }


		/**
			* Create a transaction receipt from a byte array.
			*
			* @param bytes                     the byte array
			* @return                          the new transaction receipt
			* @       when there is an issue with the protobuf
			*/
		public static TransactionReceipt FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TransactionReceipt.Parser.ParseFrom(bytes));
		}
		/**
			* Create a transaction receipt from a protobuf.
			*
			* @param transactionReceipt        the protobuf
			* @return                          the new transaction receipt
			*/
		public static TransactionReceipt FromProtobuf(Proto.TransactionReceipt transactionReceipt)
		{
			return FromProtobuf(transactionReceipt, [], [], null);
		}
		static TransactionReceipt FromProtobuf(Proto.TransactionReceipt transactionReceipt, TransactionId? transactionId)
		{
			return FromProtobuf(transactionReceipt, [], [], transactionId);
		}
		/**
			* Create transaction receipt from protobuf.
			*
			* @param transactionReceipt        the protobuf
			* @param duplicates                list of duplicates
			* @param children                  list of children
			* @return                          the new transaction receipt
			*/
		static TransactionReceipt FromProtobuf(
			Proto.TransactionReceipt transactionReceipt,
			List<TransactionReceipt> duplicates,
			List<TransactionReceipt> children,
			TransactionId? transactionId)
		{
			var status = Status.valueOf(transactionReceipt.getStatus());

			var rate = transactionReceipt.getExchangeRate();
			var exchangeRate = ExchangeRate.FromProtobuf(rate.getCurrentRate());
			var nextExchangeRate = ExchangeRate.FromProtobuf(rate.getNextRate());

			var accountId =
					transactionReceipt.hasAccountID() ? AccountId.FromProtobuf(transactionReceipt.getAccountID()) : null;

			var fileId = transactionReceipt.hasFileID() ? FileId.FromProtobuf(transactionReceipt.getFileID()) : null;

			var contractId =
					transactionReceipt.hasContractID() ? ContractId.FromProtobuf(transactionReceipt.getContractID()) : null;

			var topicId = transactionReceipt.hasTopicID() ? TopicId.FromProtobuf(transactionReceipt.getTopicID()) : null;

			var tokenId = transactionReceipt.Tok() ? TokenId.FromProtobuf(transactionReceipt.getTokenID()) : null;

			var topicSequenceNumber =
					transactionReceipt.getTopicSequenceNumber() == 0 ? null : transactionReceipt.getTopicSequenceNumber();

			var topicRunningHash =
					transactionReceipt.TopicRunningHashZ .getTopicRunningHash().isEmpty() ? null : transactionReceipt.getTopicRunningHash();

			var totalSupply = transactionReceipt.getNewTotalSupply();

			var scheduleId =
					transactionReceipt.hasScheduleID() ? ScheduleId.FromProtobuf(transactionReceipt.getScheduleID()) : null;

			var scheduledTransactionId = transactionReceipt.hasScheduledTransactionID()
					? TransactionId.FromProtobuf(transactionReceipt.getScheduledTransactionID())
					: null;

			var serials = transactionReceipt.getSerialNumbersList();
			var nodeId = transactionReceipt.getNodeId();

			return new TransactionReceipt(
					transactionId,
					status,
					exchangeRate,
					nextExchangeRate,
					accountId,
					fileId,
					contractId,
					topicId,
					tokenId,
					topicSequenceNumber,
					topicRunningHash,
					totalSupply,
					scheduleId,
					scheduledTransactionId,
					serials,
					nodeId,
					duplicates,
					children);
		}

		

		/**
		 * Validate the transaction status in the receipt.
		 *
		 * @param shouldValidate Whether to perform transaction status validation
		 * @return {@code this}
		 * @ when shouldValidate is true and the transaction status is not SUCCESS
		 */
		public TransactionReceipt validateStatus(bool shouldValidate) 
			{
			if (shouldValidate && status != Status.SUCCESS && status != Status.FEE_SCHEDULE_FILE_PART_UPLOADED) {
					throw new ReceiptStatusException(transactionId, this);
				}
			return this;
			}

		/**
		 * Create the byte array.
		 *
		 * @return                          the byte array representation
		 */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/**
			* Create the protobuf.
			*
			* @return                          the protobuf representation
			*/
		public Proto.TransactionReceipt ToProtobuf()
		{
			Proto.TransactionReceipt proto = new ()
			{
				NodeId = NodeId,
				Status = Status,
				NewTotalSupply = TotalSupply,
				ExchangeRate = new Proto.ExchangeRateSet
				{
					CurrentRate = new Proto.ExchangeRate
					{
						HbarEquiv = ExchangeRate.Hbars,
						CentEquiv = ExchangeRate.Cents,
						ExpirationTime = new Proto.TimestampSeconds
						{
							Seconds = ExchangeRate.ExpirationTime.ToUnixTimeSeconds(),
						},
					},
					NextRate = new Proto.ExchangeRate
					{
						HbarEquiv = ExchangeRate.Hbars,
						CentEquiv = ExchangeRate.Cents,
						ExpirationTime = new Proto.TimestampSeconds
						{
							Seconds = ExchangeRate.ExpirationTime.ToUnixTimeSeconds(),
						},
					},
				},

				AccountID = AccountId.ToProtobuf(),
				FileID = FileId.ToProtobuf(),
				ContractID = ContractId.ToProtobuf(),
				TopicID = TopicId.ToProtobuf(),
				TokenID = TokenId.ToProtobuf(),
				TopicSequenceNumber = TopicSequenceNumber,
				TopicRunningHash = TopicRunningHash,
				ScheduleID = ScheduleId.ToProtobuf(),
				ScheduledTransactionID = ScheduledTransactionId.ToProtobuf(),
				SerialNumbers = new RepeatedField<long>().Add(Serials) 

			};

			if (accountId != null)
			{
				;
			}


			return transactionReceiptBuilder.build();
		}		
	}
}