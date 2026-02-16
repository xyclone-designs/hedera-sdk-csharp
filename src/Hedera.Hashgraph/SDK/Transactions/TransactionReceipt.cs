// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// The consensus result for a transaction, which might not be currently
    /// known, or may succeed or fail.
    /// </summary>
    public sealed class TransactionReceipt
    {
        /// <summary>
        /// The transaction's ID
        /// </summary>
        public readonly TransactionId? TransactionId;
        /// <summary>
        /// Whether the transaction succeeded or failed (or is unknown).
        /// </summary>
        public ResponseStatus Status { get; }
        /// <summary>
        /// The exchange rate of Hbars to cents (USD).
        /// </summary>
        public readonly ExchangeRate ExchangeRate;
        /// <summary>
        /// Next exchange rate which will take effect when current rate expires
        /// </summary>
        public readonly ExchangeRate NextExchangeRate;
        /// <summary>
        /// The account ID, if a new account was created.
        /// </summary>
        public readonly AccountId AccountId;
        /// <summary>
        /// The file ID, if a new file was created.
        /// </summary>
        public readonly FileId FileId;
        /// <summary>
        /// The contract ID, if a new contract was created.
        /// </summary>
        public readonly ContractId ContractId;
        /// <summary>
        /// The topic ID, if a new topic was created.
        /// </summary>
        public readonly TopicId TopicId;
        /// <summary>
        /// The token ID, if a new token was created.
        /// </summary>
        public readonly TokenId TokenId;
        /// <summary>
        /// Updated sequence number for a consensus service topic.
        /// Set for {@link TopicMessageSubmitTransaction}.
        /// </summary>
        public readonly ulong TopicSequenceNumber;
        /// <summary>
        /// Updated running hash for a consensus service topic.
        /// Set for {@link TopicMessageSubmitTransaction}.
        /// </summary>
        public readonly ByteString? TopicRunningHash;
        /// <summary>
        /// In the receipt of TokenMint, TokenWipe, TokenBurn, For fungible tokens - the current total
        /// supply of this token. For non fungible tokens - the total number of NFTs issued for a given
        /// tokenID
        /// </summary>
        public readonly ulong TotalSupply;
        /// <summary>
        /// In the receipt of a ScheduleCreate, the id of the newly created Scheduled Entity
        /// </summary>
        public readonly ScheduleId ScheduleId;
        /// <summary>
        /// In the receipt of a ScheduleCreate or ScheduleSign that resolves to SUCCESS, the
        /// TransactionID that should be used to query for the receipt or record of the relevant
        /// scheduled transaction
        /// </summary>
        public readonly TransactionId ScheduledTransactionId;
        /// <summary>
        /// In the receipt of a TokenMint for tokens of type NON_FUNGIBLE_UNIQUE, the serial numbers of
        /// the newly created NFTs
        /// </summary>
        public readonly IList<long> Serials;
        /// <summary>
        /// In the receipt of a NodeCreate, NodeUpdate, NodeDelete, the id of the newly created node.
        /// An affected node identifier.<br/>
        /// This value SHALL be set following a `createNode` transaction.<br/>
        /// This value SHALL be set following a `updateNode` transaction.<br/>
        /// This value SHALL be set following a `deleteNode` transaction.<br/>
        /// This value SHALL NOT be set following any other transaction.
        /// </summary>
        public readonly ulong NodeId;
        /// <summary>
        /// The receipts of processing all transactions with the given id, in consensus time order.
        /// </summary>
        public readonly IList<TransactionReceipt> Duplicates;
        /// <summary>
        /// The receipts (if any) of all child transactions spawned by the transaction with the
        /// given top-level id, in consensus order. Always empty if the top-level status is UNKNOWN.
        /// </summary>
        public readonly IList<TransactionReceipt> Children;
        public TransactionReceipt(TransactionId? transactionId, ResponseStatus status, ExchangeRate exchangeRate, ExchangeRate nextExchangeRate, AccountId accountId, FileId fileId, ContractId contractId, TopicId topicId, TokenId tokenId, ulong topicSequenceNumber, ByteString? topicRunningHash, ulong totalSupply, ScheduleId scheduleId, TransactionId scheduledTransactionId, IList<long> serials, ulong nodeId, IList<TransactionReceipt> duplicates, IList<TransactionReceipt> children)
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

        /// <summary>
        /// Create transaction receipt from protobuf.
        /// </summary>
        /// <param name="transactionReceipt">the protobuf</param>
        /// <param name="duplicates">list of duplicates</param>
        /// <param name="children">list of children</param>
        /// <returns>                         the new transaction receipt</returns>
        public static TransactionReceipt FromProtobuf(Proto.TransactionReceipt transactionReceipt, IList<TransactionReceipt> duplicates, IList<TransactionReceipt> children, TransactionId? transactionId)
        {
            var status = (ResponseStatus)transactionReceipt.Status;
            var rate = transactionReceipt.ExchangeRate;
            var exchangeRate = ExchangeRate.FromProtobuf(rate.CurrentRate);
            var nextExchangeRate = ExchangeRate.FromProtobuf(rate.NextRate);
            var accountId = AccountId.FromProtobuf(transactionReceipt.AccountID);
            var fileId = FileId.FromProtobuf(transactionReceipt.FileID);
            var contractId = ContractId.FromProtobuf(transactionReceipt.ContractID);
            var topicId = TopicId.FromProtobuf(transactionReceipt.TopicID);
            var tokenId = TokenId.FromProtobuf(transactionReceipt.TokenID);
            var topicSequenceNumber = transactionReceipt.TopicSequenceNumber;
            var topicRunningHash = transactionReceipt.TopicRunningHash.Length == 0 ? null : transactionReceipt.TopicRunningHash;
            var totalSupply = transactionReceipt.NewTotalSupply;
            var scheduleId = ScheduleId.FromProtobuf(transactionReceipt.ScheduleID);
            var scheduledTransactionId = TransactionId.FromProtobuf(transactionReceipt.ScheduledTransactionID);
            var serials = transactionReceipt.SerialNumbers;
            var nodeId = transactionReceipt.NodeId;

            return new TransactionReceipt(transactionId, status, exchangeRate, nextExchangeRate, accountId, fileId, contractId, topicId, tokenId, topicSequenceNumber, topicRunningHash, totalSupply, scheduleId, scheduledTransactionId, serials, nodeId, duplicates, children);
        }

        /// <summary>
        /// Create a transaction receipt from a protobuf.
        /// </summary>
        /// <param name="transactionReceipt">the protobuf</param>
        /// <returns>                         the new transaction receipt</returns>
        public static TransactionReceipt FromProtobuf(Proto.TransactionReceipt transactionReceipt)
        {
            return FromProtobuf(transactionReceipt, [], [], null);
        }
        public static TransactionReceipt FromProtobuf(Proto.TransactionReceipt transactionReceipt, TransactionId transactionId)
        {
            return FromProtobuf(transactionReceipt, [], [], transactionId);
        }

        /// <summary>
        /// Create a transaction receipt from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new transaction receipt</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TransactionReceipt FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.TransactionReceipt.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Validate the transaction status in the receipt.
        /// </summary>
        /// <param name="shouldValidate">Whether to perform transaction status validation</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="ReceiptStatusException">when shouldValidate is true and the transaction status is not SUCCESS</exception>
        public TransactionReceipt ValidateStatus(bool shouldValidate)
        {
            if (shouldValidate && Status != ResponseStatus.Success && Status != ResponseStatus.FeeScheduleFilePartUploaded)
            {
                throw new ReceiptStatusException(TransactionId, this);
            }

            return this;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        public Proto.TransactionReceipt ToProtobuf()
        {
            Proto.TransactionReceipt proto = new()
            {
				NodeId = NodeId,
                NewTotalSupply = TotalSupply,
				Status = (Proto.ResponseCodeEnum)Status,
				TopicSequenceNumber = TopicSequenceNumber,
				ExchangeRate = new Proto.ExchangeRateSet
                {
                    CurrentRate = new Proto.ExchangeRate
                    {
						HbarEquiv = ExchangeRate.Hbars,
						CentEquiv = ExchangeRate.Cents,
                        ExpirationTime = new Proto.TimestampSeconds
                        {
                            Seconds = ExchangeRate.ExpirationTime.ToUnixTimeSeconds()
                        }
					},
                    NextRate = new Proto.ExchangeRate
					{
						HbarEquiv = NextExchangeRate.Hbars,
						CentEquiv = NextExchangeRate.Cents,
						ExpirationTime = new Proto.TimestampSeconds
						{
							Seconds = NextExchangeRate.ExpirationTime.ToUnixTimeSeconds()
						}
					},
				}
			};

            if (AccountId != null) proto.AccountID = AccountId.ToProtobuf();
            if (FileId != null) proto.FileID = FileId.ToProtobuf();
            if (ContractId != null) proto.ContractID = ContractId.ToProtobuf();
            if (TopicId != null) proto.TopicID = TopicId.ToProtobuf();
            if (TokenId != null) proto.TokenID = TokenId.ToProtobuf();
            if (TopicRunningHash != null) proto.TopicRunningHash = TopicRunningHash;
            if (ScheduleId != null) proto.ScheduleID = ScheduleId.ToProtobuf();
            if (ScheduledTransactionId != null) proto.ScheduledTransactionID = ScheduledTransactionId.ToProtobuf();

            foreach (var serial in Serials)
				proto.SerialNumbers.Add(serial);

			return proto;
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}