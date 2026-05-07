// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Airdrops;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="T:TransactionRecord"]/*' />
    public sealed class TransactionRecord
    {
        internal TransactionRecord(
            TransactionReceipt transactionReceipt, 
            ByteString transactionHash, 
            DateTimeOffset consensusTimestamp, 
            TransactionId transactionId,
            string transactionMemo, 
            long transactionFee, 
            ContractFunctionResult? contractFunctionResult, 
            IEnumerable<Transfer> transfers, 
            IDictionary<TokenId, IDictionary<AccountId, long>> tokenTransfers, 
            IEnumerable<TokenTransfer> tokenTransferList, 
            IDictionary<TokenId, IList<TokenNftTransfer>> tokenNftTransfers, 
            ScheduleId scheduleRef, 
            IEnumerable<AssessedCustomFee> assessedCustomFees,
			IEnumerable<TokenAssociation> automaticTokenAssociations, 
            PublicKey? aliasKey,
			IEnumerable<TransactionRecord> children,
			IEnumerable<TransactionRecord> duplicates,
			DateTimeOffset parentConsensusTimestamp, 
            ByteString ethereumHash, 
            IEnumerable<Transfer> paidStakingRewards,
            ByteString prngBytes, 
            int prngNumber, 
            ByteString evmAddress,
			IEnumerable<PendingAirdropRecord> pendingAirdropRecords)
        {
            Receipt = transactionReceipt;
            TransactionHash = transactionHash;
            ConsensusTimestamp = consensusTimestamp;
            TransactionMemo = transactionMemo;
            TransactionId = transactionId;
            Transfers = [ ..transfers];
            ContractFunctionResult = contractFunctionResult;
            TransactionFee = Hbar.FromTinybars(transactionFee);
            TokenTransfers = tokenTransfers.ToDictionary(_ => _.Key, _ => new Dictionary<AccountId, long>(_.Value));
            TokenTransferList = [ ..tokenTransferList];
            TokenNftTransfers = tokenNftTransfers.ToDictionary(_ => _.Key, _ => new List<TokenNftTransfer>(_.Value));
            ScheduleRef = scheduleRef;
            AssessedCustomFees = [ ..assessedCustomFees];
            AutomaticTokenAssociations = [ ..automaticTokenAssociations];
            AliasKey = aliasKey;
            Children = [ ..children];
            Duplicates = [ ..duplicates];
            ParentConsensusTimestamp = parentConsensusTimestamp;
            EthereumHash = ethereumHash;
            PendingAirdropRecords = [ ..pendingAirdropRecords];
            HbarAllowanceAdjustments = [];
            TokenAllowanceAdjustments = [];
            TokenNftAllowanceAdjustments = [];
            PaidStakingRewards = [ ..paidStakingRewards];
            PrngBytes = prngBytes;
            PrngNumber = prngNumber;
            EvmAddress = evmAddress;
        }

		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.FromBytes(System.Byte[])"]/*' />
		public static TransactionRecord FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.TransactionRecord.Parser.ParseFrom(bytes));
		}
		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.FromProtobuf(Proto.Services.TransactionRecord)"]/*' />
		public static TransactionRecord FromProtobuf(Proto.Services.TransactionRecord transactionRecord)
		{
			return FromProtobuf(transactionRecord, [], [], null);
		}
		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.FromProtobuf(Proto.Services.TransactionRecord,System.Collections.Generic.IEnumerable{TransactionRecord},System.Collections.Generic.IEnumerable{TransactionRecord},TransactionId)"]/*' />
		public static TransactionRecord FromProtobuf(Proto.Services.TransactionRecord transactionRecord, IEnumerable<TransactionRecord> children, IEnumerable<TransactionRecord> duplicates, TransactionId? transactionId)
        {
            var transfers = new List<Transfer>(transactionRecord.TransferList.AccountAmounts.Count);

            foreach (var accountAmount in transactionRecord.TransferList.AccountAmounts)
            {
                transfers.Add(Transfer.FromProtobuf(accountAmount));
            }

            var tokenTransfers = new Dictionary<TokenId, IDictionary<AccountId, long>>();
            var tokenNftTransfers = new Dictionary<TokenId, IList<TokenNftTransfer>>();
            var allTokenTransfers = new List<TokenTransfer>();

            foreach (var transferList in transactionRecord.TokenTransferLists)
            {
                var tokenTransfersList = TokenTransfer.FromProtobuf(transferList);
                var nftTransfersList = TokenNftTransfer.FromProtobuf(transferList);

                foreach (var transfer in tokenTransfersList)
                {
                    var current = tokenTransfers.TryGetValue(transfer.TokenId, out IDictionary<AccountId, long>? value) ? value : new Dictionary<AccountId, long> { };

                    current.Add(transfer.AccountId, transfer.Amount);
                    tokenTransfers.Add(transfer.TokenId, current);
                }

                allTokenTransfers.AddRange(tokenTransfersList);
                foreach (var transfer in nftTransfersList)
                {
                    var current = tokenNftTransfers.TryGetValue(transfer.TokenId, out IList<TokenNftTransfer>? value) ? value : [];
                    current.Add(transfer);
                    tokenNftTransfers.Add(transfer.TokenId, current);
                }
            }

            var fees = new List<AssessedCustomFee>(transactionRecord.AssessedCustomFees.Count);
            foreach (var fee in transactionRecord.AssessedCustomFees)
            {
                fees.Add(AssessedCustomFee.FromProtobuf(fee));
            }

            // HACK: This is a bit bad, any takers to clean this up
            var contractFunctionResult = transactionRecord.ContractCallResult is not null 
                ? new ContractFunctionResult(transactionRecord.ContractCallResult) 
                : transactionRecord.ContractCreateResult is not null
                    ? new ContractFunctionResult(transactionRecord.ContractCreateResult) 
                    : null;
            var automaticTokenAssociations = new List<TokenAssociation>(transactionRecord.AutomaticTokenAssociations.Count);
            
            foreach (var tokenAssociation in transactionRecord.AutomaticTokenAssociations)
            {
                automaticTokenAssociations.Add(TokenAssociation.FromProtobuf(tokenAssociation));
            }

            var aliasKey = PublicKey.FromAliasBytes(transactionRecord.Alias);
            var paidStakingRewards = new List<Transfer>(transactionRecord.PaidStakingRewards.Count);
            
            foreach (var reward in transactionRecord.PaidStakingRewards)
            {
                paidStakingRewards.Add(Transfer.FromProtobuf(reward));
            }
            
            return new TransactionRecord(
                TransactionReceipt.FromProtobuf(transactionRecord.Receipt, [], [], transactionId), 
                transactionRecord.TransactionHash,
                transactionRecord.ConsensusTimestamp.ToDateTimeOffset(), 
                TransactionId.FromProtobuf(transactionRecord.TransactionId), 
                transactionRecord.Memo, 
                (long)transactionRecord.TransactionFee, 
                contractFunctionResult, 
                transfers, 
                tokenTransfers, 
                allTokenTransfers, 
                tokenNftTransfers, 
                ScheduleId.FromProtobuf(transactionRecord.ScheduleRef), 
                fees, 
                automaticTokenAssociations, 
                aliasKey, 
                children, 
                duplicates,
				transactionRecord.ParentConsensusTimestamp.ToDateTimeOffset(), 
                transactionRecord.EthereumHash, 
                paidStakingRewards,
				transactionRecord.PrngBytes, 
                transactionRecord.PrngNumber, 
                transactionRecord.EvmAddress, 
                [.. transactionRecord.NewPendingAirdrops.Select(_ => PendingAirdropRecord.FromProtobuf(_))]);
        }

        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.Receipt"]/*' />
        public TransactionReceipt Receipt { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TransactionHash"]/*' />
        public ByteString TransactionHash { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.ConsensusTimestamp"]/*' />
        public DateTimeOffset ConsensusTimestamp { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TransactionId"]/*' />
        public TransactionId TransactionId { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TransactionMemo"]/*' />
        public string TransactionMemo { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TransactionFee"]/*' />
        public Hbar TransactionFee { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.ContractFunctionResult"]/*' />
        public ContractFunctionResult? ContractFunctionResult { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.Transfers"]/*' />
        public List<Transfer> Transfers { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="T:TransactionRecord_2"]/*' />
        public Dictionary<TokenId, Dictionary<AccountId, long>> TokenTransfers { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TokenTransferList"]/*' />
        public List<TokenTransfer> TokenTransferList { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="T:TransactionRecord_3"]/*' />
        public Dictionary<TokenId, List<TokenNftTransfer>> TokenNftTransfers { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.ScheduleRef"]/*' />
        public ScheduleId ScheduleRef { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.AssessedCustomFees"]/*' />
        public List<AssessedCustomFee> AssessedCustomFees { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.AutomaticTokenAssociations"]/*' />
        public List<TokenAssociation> AutomaticTokenAssociations { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.AliasKey"]/*' />
        public PublicKey? AliasKey { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.Children"]/*' />
        public List<TransactionRecord> Children { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.Duplicates"]/*' />
        public List<TransactionRecord> Duplicates { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.ParentConsensusTimestamp"]/*' />
        public DateTimeOffset ParentConsensusTimestamp { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.EthereumHash"]/*' />
        public ByteString EthereumHash { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.HbarAllowanceAdjustments"]/*' />
        public List<HbarAllowance> HbarAllowanceAdjustments { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TokenAllowanceAdjustments"]/*' />
        public List<TokenAllowance> TokenAllowanceAdjustments { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TokenNftAllowanceAdjustments"]/*' />
        public List<TokenNftAllowance> TokenNftAllowanceAdjustments { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.PaidStakingRewards"]/*' />
        public List<Transfer> PaidStakingRewards { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.PrngBytes"]/*' />
        public ByteString PrngBytes { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.PrngNumber"]/*' />
        public int PrngNumber { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.EvmAddress"]/*' />
        public ByteString EvmAddress { get; }
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.PendingAirdropRecords"]/*' />
        public List<PendingAirdropRecord> PendingAirdropRecords { get; }

        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.ToBytes"]/*' />
        public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.ToProtobuf"]/*' />
		public Proto.Services.TransactionRecord ToProtobuf()
        {
			Proto.Services.TransactionRecord proto = new()
			{
				Receipt = Receipt.ToProtobuf(),
                TransactionHash = TransactionHash,
                ConsensusTimestamp = ConsensusTimestamp.ToProtoTimestamp(),
                ParentConsensusTimestamp = ParentConsensusTimestamp.ToProtoTimestamp(),
                TransactionId = TransactionId.ToProtobuf(),
                Memo = TransactionMemo,
                TransactionFee = (ulong)TransactionFee.ToTinybars(),
                TransferList = new Proto.Services.TransferList { },
                EthereumHash = EthereumHash,
                EvmAddress = EvmAddress,
				PrngNumber = PrngNumber,
                PrngBytes = PrngBytes,
                ScheduleRef = ScheduleRef.ToProtobuf(),
            };

            foreach (var tokenEntry in TokenTransfers)
            {
                Proto.Services.TokenTransferList tokenTransfersList = new()
				{
					Token = tokenEntry.Key.ToProtobuf(),
				};

				foreach (var aaEntry in tokenEntry.Value)
					tokenTransfersList.Transfers.Add(new Proto.Services.AccountAmount
					{
						AccountId = aaEntry.Key.ToProtobuf(),
						Amount = aaEntry.Value
					});

                proto.TokenTransferLists.Add(tokenTransfersList);
            }

			foreach (Transfer transfer in Transfers)
				proto.TransferList.AccountAmounts.Add(transfer.ToProtobuf());

			foreach (var fee in AssessedCustomFees)
				proto.AssessedCustomFees.Add(fee.ToProtobuf());
			
            foreach (var tokenAssociation in AutomaticTokenAssociations)
				proto.AutomaticTokenAssociations.Add(tokenAssociation.ToProtobuf());
			
            foreach (Transfer reward in PaidStakingRewards)
				proto.PaidStakingRewards.Add(reward.ToProtobuf());

			foreach (var nftEntry in TokenNftTransfers)
            {
				Proto.Services.TokenTransferList nftTransferList = new ()
                {
					Token = nftEntry.Key.ToProtobuf(),
				};

                foreach (var aaEntry in nftEntry.Value)
					nftTransferList.NftTransfers.Add(new Proto.Services.NftTransfer
					{
						SenderAccountId = aaEntry.Sender.ToProtobuf(),
						ReceiverAccountId = aaEntry.Receiver.ToProtobuf(),
						SerialNumber = aaEntry.Serial,
						IsApproval = aaEntry.IsApproved,
					});

                proto.TokenTransferLists.Add(nftTransferList);
            }

			if (PendingAirdropRecords != null)
				foreach (PendingAirdropRecord pendingAirdropRecord in PendingAirdropRecords)
                    proto.NewPendingAirdrops.Add(pendingAirdropRecord.ToProtobuf());

			if (ContractFunctionResult != null)
				proto.ContractCallResult = ContractFunctionResult.ToProtobuf();

            if (AliasKey != null)
                proto.Alias = AliasKey.ToProtobufKey().ToByteString();

			return proto;
        }
		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.ValidateReceiptStatus(System.Boolean)"]/*' />
		public TransactionRecord ValidateReceiptStatus(bool shouldValidate)
		{
			Receipt.ValidateStatus(shouldValidate);
			return this;
		}
	}
}
