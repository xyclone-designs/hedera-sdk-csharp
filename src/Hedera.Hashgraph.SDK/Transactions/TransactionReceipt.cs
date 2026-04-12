// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="T:TransactionReceipt"]/*' />
    public sealed class TransactionReceipt
    {
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.TransactionId"]/*' />
        public readonly TransactionId? TransactionId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="P:TransactionReceipt.Status"]/*' />
        public ResponseStatus Status { get; }
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.ExchangeRate"]/*' />
        public readonly ExchangeRate ExchangeRate;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.NextExchangeRate"]/*' />
        public readonly ExchangeRate NextExchangeRate;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.AccountId"]/*' />
        public readonly AccountId AccountId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.FileId"]/*' />
        public readonly FileId FileId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.ContractId"]/*' />
        public readonly ContractId ContractId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.TopicId"]/*' />
        public readonly TopicId TopicId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.TokenId"]/*' />
        public readonly TokenId TokenId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.TopicSequenceNumber"]/*' />
        public readonly ulong TopicSequenceNumber;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.TopicRunningHash"]/*' />
        public readonly ByteString? TopicRunningHash;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.TotalSupply"]/*' />
        public readonly ulong TotalSupply;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.ScheduleId"]/*' />
        public readonly ScheduleId ScheduleId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.ScheduledTransactionId"]/*' />
        public readonly TransactionId ScheduledTransactionId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.Serials"]/*' />
        public readonly List<long> Serials;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.NodeId"]/*' />
        public readonly ulong NodeId;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="F:TransactionReceipt.Duplicates"]/*' />
        public readonly List<TransactionReceipt> Duplicates;
        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="M:TransactionReceipt.#ctor(TransactionId,ResponseStatus,ExchangeRate,ExchangeRate,AccountId,FileId,ContractId,TopicId,TokenId,System.UInt64,ByteString,System.UInt64,ScheduleId,TransactionId,System.Collections.Generic.IEnumerable{System.Int64},System.UInt64,System.Collections.Generic.IEnumerable{TransactionReceipt},System.Collections.Generic.IEnumerable{TransactionReceipt})"]/*' />
        public readonly List<TransactionReceipt> Children;
        public TransactionReceipt(TransactionId? transactionId, ResponseStatus status, ExchangeRate exchangeRate, ExchangeRate nextExchangeRate, AccountId accountId, FileId fileId, ContractId contractId, TopicId topicId, TokenId tokenId, ulong topicSequenceNumber, ByteString? topicRunningHash, ulong totalSupply, ScheduleId scheduleId, TransactionId scheduledTransactionId, IEnumerable<long> serials, ulong nodeId, IEnumerable<TransactionReceipt> duplicates, IEnumerable<TransactionReceipt> children)
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
            Serials = [.. serials];
            NodeId = nodeId;
            Duplicates = [ ..duplicates];
            Children = [ ..children];
        }

        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="M:TransactionReceipt.FromProtobuf(Proto.Services.TransactionReceipt,System.Collections.Generic.IList{TransactionReceipt},System.Collections.Generic.IList{TransactionReceipt},TransactionId)"]/*' />
        public static TransactionReceipt FromProtobuf(Proto.Services.TransactionReceipt transactionReceipt, IList<TransactionReceipt> duplicates, IList<TransactionReceipt> children, TransactionId? transactionId)
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

        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="M:TransactionReceipt.FromProtobuf(Proto.Services.TransactionReceipt)"]/*' />
        public static TransactionReceipt FromProtobuf(Proto.Services.TransactionReceipt transactionReceipt)
        {
            return FromProtobuf(transactionReceipt, [], [], null);
        }
        public static TransactionReceipt FromProtobuf(Proto.Services.TransactionReceipt transactionReceipt, TransactionId transactionId)
        {
            return FromProtobuf(transactionReceipt, [], [], transactionId);
        }

        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="M:TransactionReceipt.FromBytes(System.Byte[])"]/*' />
        public static TransactionReceipt FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.TransactionReceipt.Parser.ParseFrom(bytes));
        }

        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="M:TransactionReceipt.ValidateStatus(System.Boolean)"]/*' />
        public TransactionReceipt ValidateStatus(bool shouldValidate)
        {
            if (shouldValidate && Status != ResponseStatus.Success && Status != ResponseStatus.FeeScheduleFilePartUploaded)
            {
                throw new ReceiptStatusException(TransactionId, this);
            }

            return this;
        }

        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="M:TransactionReceipt.ToProtobuf"]/*' />
        public Proto.Services.TransactionReceipt ToProtobuf()
        {
            Proto.Services.TransactionReceipt proto = new()
            {
				NodeId = NodeId,
                NewTotalSupply = TotalSupply,
				Status = (Proto.Services.ResponseCodeEnum)Status,
				TopicSequenceNumber = TopicSequenceNumber,
				ExchangeRate = new Proto.Services.ExchangeRateSet
                {
                    CurrentRate = new Proto.Services.ExchangeRate
                    {
						HbarEquiv = ExchangeRate.Hbars,
						CentEquiv = ExchangeRate.Cents,
                        ExpirationTime = new Proto.Services.TimestampSeconds
                        {
                            Seconds = ExchangeRate.ExpirationTime.ToUnixTimeSeconds()
                        }
					},
                    NextRate = new Proto.Services.ExchangeRate
					{
						HbarEquiv = NextExchangeRate.Hbars,
						CentEquiv = NextExchangeRate.Cents,
						ExpirationTime = new Proto.Services.TimestampSeconds
						{
							Seconds = NextExchangeRate.ExpirationTime.ToUnixTimeSeconds()
						}
					},
				}
			};

            if (AccountId != null) Proto.Services.AccountID = AccountId.ToProtobuf();
            if (FileId != null) Proto.Services.FileID = FileId.ToProtobuf();
            if (ContractId != null) Proto.Services.ContractID = ContractId.ToProtobuf();
            if (TopicId != null) Proto.Services.TopicID = TopicId.ToProtobuf();
            if (TokenId != null) Proto.Services.TokenID = TokenId.ToProtobuf();
            if (TopicRunningHash != null) Proto.Services.TopicRunningHash = TopicRunningHash;
            if (ScheduleId != null) Proto.Services.ScheduleID = ScheduleId.ToProtobuf();
            if (ScheduledTransactionId != null) Proto.Services.ScheduledTransactionID = ScheduledTransactionId.ToProtobuf();

            foreach (var serial in Serials)
				Proto.Services.SerialNumbers.Add(serial);

			return proto;
        }

        /// <include file="TransactionReceipt.cs.xml" path='docs/member[@name="M:TransactionReceipt.ToBytes"]/*' />
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}
