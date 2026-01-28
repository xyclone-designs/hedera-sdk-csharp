// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Proto;
using Java.Util;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
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
    /// The consensus result for a transaction, which might not be currently
    /// known, or may succeed or fail.
    /// </summary>
    public sealed class TransactionReceipt
    {
        /// <summary>
        /// The transaction's ID
        /// </summary>
        public readonly TransactionId transactionId;
        /// <summary>
        /// Whether the transaction succeeded or failed (or is unknown).
        /// </summary>
        public Status Status { get; }
        /// <summary>
        /// The exchange rate of Hbars to cents (USD).
        /// </summary>
        public readonly ExchangeRate exchangeRate;
        /// <summary>
        /// Next exchange rate which will take effect when current rate expires
        /// </summary>
        public readonly ExchangeRate nextExchangeRate;
        /// <summary>
        /// The account ID, if a new account was created.
        /// </summary>
        public readonly AccountId accountId;
        /// <summary>
        /// The file ID, if a new file was created.
        /// </summary>
        public readonly FileId fileId;
        /// <summary>
        /// The contract ID, if a new contract was created.
        /// </summary>
        public readonly ContractId contractId;
        /// <summary>
        /// The topic ID, if a new topic was created.
        /// </summary>
        public readonly TopicId topicId;
        /// <summary>
        /// The token ID, if a new token was created.
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// Updated sequence number for a consensus service topic.
        /// Set for {@link TopicMessageSubmitTransaction}.
        /// </summary>
        public readonly long topicSequenceNumber;
        /// <summary>
        /// Updated running hash for a consensus service topic.
        /// Set for {@link TopicMessageSubmitTransaction}.
        /// </summary>
        public readonly ByteString topicRunningHash;
        /// <summary>
        /// In the receipt of TokenMint, TokenWipe, TokenBurn, For fungible tokens - the current total
        /// supply of this token. For non fungible tokens - the total number of NFTs issued for a given
        /// tokenID
        /// </summary>
        public readonly long totalSupply;
        /// <summary>
        /// In the receipt of a ScheduleCreate, the id of the newly created Scheduled Entity
        /// </summary>
        public readonly ScheduleId scheduleId;
        /// <summary>
        /// In the receipt of a ScheduleCreate or ScheduleSign that resolves to SUCCESS, the
        /// TransactionID that should be used to query for the receipt or record of the relevant
        /// scheduled transaction
        /// </summary>
        public readonly TransactionId scheduledTransactionId;
        /// <summary>
        /// In the receipt of a TokenMint for tokens of type NON_FUNGIBLE_UNIQUE, the serial numbers of
        /// the newly created NFTs
        /// </summary>
        public readonly IList<long> serials;
        /// <summary>
        /// In the receipt of a NodeCreate, NodeUpdate, NodeDelete, the id of the newly created node.
        /// An affected node identifier.<br/>
        /// This value SHALL be set following a `createNode` transaction.<br/>
        /// This value SHALL be set following a `updateNode` transaction.<br/>
        /// This value SHALL be set following a `deleteNode` transaction.<br/>
        /// This value SHALL NOT be set following any other transaction.
        /// </summary>
        public readonly long nodeId;
        /// <summary>
        /// The receipts of processing all transactions with the given id, in consensus time order.
        /// </summary>
        public readonly IList<TransactionReceipt> Duplicates;
        /// <summary>
        /// The receipts (if any) of all child transactions spawned by the transaction with the
        /// given top-level id, in consensus order. Always empty if the top-level status is UNKNOWN.
        /// </summary>
        public readonly IList<TransactionReceipt> Children;
        TransactionReceipt(TransactionId transactionId, Status status, ExchangeRate exchangeRate, ExchangeRate nextExchangeRate, AccountId accountId, FileId fileId, ContractId contractId, TopicId topicId, TokenId tokenId, long topicSequenceNumber, ByteString topicRunningHash, long totalSupply, ScheduleId scheduleId, TransactionId scheduledTransactionId, IList<long> serials, long nodeId, IList<TransactionReceipt> duplicates, IList<TransactionReceipt> children)
        {
            transactionId = transactionId;
            status = status;
            exchangeRate = exchangeRate;
            nextExchangeRate = nextExchangeRate;
            accountId = accountId;
            fileId = fileId;
            contractId = contractId;
            topicId = topicId;
            tokenId = tokenId;
            topicSequenceNumber = topicSequenceNumber;
            topicRunningHash = topicRunningHash;
            totalSupply = totalSupply;
            scheduleId = scheduleId;
            scheduledTransactionId = scheduledTransactionId;
            serials = serials;
            nodeId = nodeId;
            duplicates = duplicates;
            children = children;
        }

        /// <summary>
        /// Create transaction receipt from protobuf.
        /// </summary>
        /// <param name="transactionReceipt">the protobuf</param>
        /// <param name="duplicates">list of duplicates</param>
        /// <param name="children">list of children</param>
        /// <returns>                         the new transaction receipt</returns>
        static TransactionReceipt FromProtobuf(Proto.TransactionReceipt transactionReceipt, IList<TransactionReceipt> duplicates, IList<TransactionReceipt> children, TransactionId transactionId)
        {
            var status = SDK.Status.ValueOf(transactionReceipt.GetStatus());
            var rate = transactionReceipt.GetExchangeRate();
            var exchangeRate = ExchangeRate.FromProtobuf(rate.GetCurrentRate());
            var nextExchangeRate = ExchangeRate.FromProtobuf(rate.GetNextRate());
            var accountId = transactionReceipt.HasAccountID() ? AccountId.FromProtobuf(transactionReceipt.GetAccountID()) : null;
            var fileId = transactionReceipt.HasFileID() ? FileId.FromProtobuf(transactionReceipt.GetFileID()) : null;
            var contractId = transactionReceipt.HasContractID() ? ContractId.FromProtobuf(transactionReceipt.GetContractID()) : null;
            var topicId = transactionReceipt.HasTopicID() ? TopicId.FromProtobuf(transactionReceipt.GetTopicID()) : null;
            var tokenId = transactionReceipt.HasTokenID() ? TokenId.FromProtobuf(transactionReceipt.GetTokenID()) : null;
            var topicSequenceNumber = transactionReceipt.GetTopicSequenceNumber() == 0 ? null : transactionReceipt.GetTopicSequenceNumber();
            var topicRunningHash = transactionReceipt.GetTopicRunningHash().IsEmpty ? null : transactionReceipt.GetTopicRunningHash();
            var totalSupply = transactionReceipt.GetNewTotalSupply();
            var scheduleId = transactionReceipt.HasScheduleID() ? ScheduleId.FromProtobuf(transactionReceipt.GetScheduleID()) : null;
            var scheduledTransactionId = transactionReceipt.HasScheduledTransactionID() ? TransactionId.FromProtobuf(transactionReceipt.GetScheduledTransactionID()) : null;
            var serials = transactionReceipt.GetSerialNumbersList();
            var nodeId = transactionReceipt.GetNodeId();

            return new TransactionReceipt(transactionId, status, exchangeRate, nextExchangeRate, accountId, fileId, contractId, topicId, tokenId, topicSequenceNumber, topicRunningHash, totalSupply, scheduleId, scheduledTransactionId, serials, nodeId, duplicates, children);
        }

        /// <summary>
        /// Create a transaction receipt from a protobuf.
        /// </summary>
        /// <param name="transactionReceipt">the protobuf</param>
        /// <returns>                         the new transaction receipt</returns>
        public static TransactionReceipt FromProtobuf(Proto.TransactionReceipt transactionReceipt)
        {
            return FromProtobuf(transactionReceipt, new (), new (), null);
        }

        static TransactionReceipt FromProtobuf(Proto.TransactionReceipt transactionReceipt, TransactionId transactionId)
        {
            return FromProtobuf(transactionReceipt, new (), new (), transactionId);
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
            if (shouldValidate && Status != Status.Success && Status != Status.FeeScheduleFilePartUploaded)
            {
                throw new ReceiptStatusException(transactionId, this);
            }

            return this;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        Proto.TransactionReceipt ToProtobuf()
        {
            var transactionReceiptBuilder = Proto.TransactionReceipt.NewBuilder().SetStatus(Status.code).SetExchangeRate(ExchangeRateSet.NewBuilder().SetCurrentRate(Proto.ExchangeRate.NewBuilder().SetHbarEquiv(exchangeRate.Hbars).SetCentEquiv(exchangeRate.Cents).SetExpirationTime(TimestampSeconds.NewBuilder().SetSeconds(exchangeRate.ExpirationTime.GetEpochSecond()))).SetNextRate(Proto.ExchangeRate.NewBuilder().SetHbarEquiv(nextExchangeRate.Hbars).SetCentEquiv(nextExchangeRate.Cents).SetExpirationTime(TimestampSeconds.NewBuilder().SetSeconds(nextExchangeRate.ExpirationTime.GetEpochSecond())))).SetNewTotalSupply(totalSupply);
            if (accountId != null)
            {
                transactionReceiptBuilder.SetAccountID(accountId.ToProtobuf());
            }

            if (fileId != null)
            {
                transactionReceiptBuilder.SetFileID(fileId.ToProtobuf());
            }

            if (contractId != null)
            {
                transactionReceiptBuilder.SetContractID(contractId.ToProtobuf());
            }

            if (topicId != null)
            {
                transactionReceiptBuilder.SetTopicID(topicId.ToProtobuf());
            }

            if (tokenId != null)
            {
                transactionReceiptBuilder.SetTokenID(tokenId.ToProtobuf());
            }

            if (topicSequenceNumber != null)
            {
                transactionReceiptBuilder.SetTopicSequenceNumber(topicSequenceNumber);
            }

            if (topicRunningHash != null)
            {
                transactionReceiptBuilder.SetTopicRunningHash(topicRunningHash);
            }

            if (scheduleId != null)
            {
                transactionReceiptBuilder.SetScheduleID(scheduleId.ToProtobuf());
            }

            if (scheduledTransactionId != null)
            {
                transactionReceiptBuilder.SetScheduledTransactionID(scheduledTransactionId.ToProtobuf());
            }

            foreach (var serial in serials)
            {
                transactionReceiptBuilder.AddSerialNumbers(serial);
            }

            transactionReceiptBuilder.SetNodeId(nodeId);
            return transactionReceiptBuilder.Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("transactionId", transactionId).Add("status", Status).Add("exchangeRate", exchangeRate).Add("nextExchangeRate", nextExchangeRate).Add("accountId", accountId).Add("fileId", fileId).Add("contractId", contractId).Add("topicId", topicId).Add("tokenId", tokenId).Add("topicSequenceNumber", topicSequenceNumber).Add("topicRunningHash", topicRunningHash != null ? Hex.Encode(topicRunningHash.ToByteArray()) : null).Add("totalSupply", totalSupply).Add("scheduleId", scheduleId).Add("scheduledTransactionId", scheduledTransactionId).Add("serials", serials).Add("nodeId", nodeId).Add("duplicates", Duplicates).Add("children", children).ToString();
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