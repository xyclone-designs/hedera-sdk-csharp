// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Airdrops;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
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
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.Receipt"]/*' />
        public readonly TransactionReceipt Receipt;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TransactionHash"]/*' />
        public readonly ByteString TransactionHash;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.ConsensusTimestamp"]/*' />
        public readonly DateTimeOffset ConsensusTimestamp;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TransactionId"]/*' />
        public readonly TransactionId TransactionId;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TransactionMemo"]/*' />
        public readonly string TransactionMemo;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TransactionFee"]/*' />
        public readonly Hbar TransactionFee;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.ContractFunctionResult"]/*' />
        public readonly ContractFunctionResult? ContractFunctionResult;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.Transfers"]/*' />
        public readonly List<Transfer> Transfers;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="T:TransactionRecord_2"]/*' />
        public readonly Dictionary<TokenId, Dictionary<AccountId, long>> TokenTransfers;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TokenTransferList"]/*' />
        public readonly List<TokenTransfer> TokenTransferList;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="T:TransactionRecord_3"]/*' />
        public readonly Dictionary<TokenId, List<TokenNftTransfer>> TokenNftTransfers;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.ScheduleRef"]/*' />
        public readonly ScheduleId ScheduleRef;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.AssessedCustomFees"]/*' />
        public readonly List<AssessedCustomFee> AssessedCustomFees;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.AutomaticTokenAssociations"]/*' />
        public readonly List<TokenAssociation> AutomaticTokenAssociations;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.AliasKey"]/*' />
        public readonly PublicKey? AliasKey;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.Children"]/*' />
        public readonly List<TransactionRecord> Children;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.Duplicates"]/*' />
        public readonly List<TransactionRecord> Duplicates;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.ParentConsensusTimestamp"]/*' />
        public readonly DateTimeOffset ParentConsensusTimestamp;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.EthereumHash"]/*' />
        public readonly ByteString EthereumHash;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.HbarAllowanceAdjustments"]/*' />
        public readonly List<HbarAllowance> HbarAllowanceAdjustments;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TokenAllowanceAdjustments"]/*' />
        public readonly List<TokenAllowance> TokenAllowanceAdjustments;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.TokenNftAllowanceAdjustments"]/*' />
        public readonly List<TokenNftAllowance> TokenNftAllowanceAdjustments;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.PaidStakingRewards"]/*' />
        public readonly List<Transfer> PaidStakingRewards;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.PrngBytes"]/*' />
        public readonly ByteString PrngBytes;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.PrngNumber"]/*' />
        public readonly int PrngNumber;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.EvmAddress"]/*' />
        public readonly ByteString EvmAddress;
        /// <include file="TransactionRecord.cs.xml" path='docs/member[@name="F:TransactionRecord.PendingAirdropRecords"]/*' />
        public readonly List<PendingAirdropRecord> PendingAirdropRecords;

        TransactionRecord(
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
			return FromProtobuf(Proto.TransactionRecord.Parser.ParseFrom(bytes));
		}
		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.FromProtobuf(Proto.TransactionRecord)"]/*' />
		public static TransactionRecord FromProtobuf(Proto.TransactionRecord transactionRecord)
		{
			return FromProtobuf(transactionRecord, [], [], null);
		}
		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.FromProtobuf(Proto.TransactionRecord,System.Collections.Generic.IEnumerable{TransactionRecord},System.Collections.Generic.IEnumerable{TransactionRecord},TransactionId)"]/*' />
		public static TransactionRecord FromProtobuf(Proto.TransactionRecord transactionRecord, IEnumerable<TransactionRecord> children, IEnumerable<TransactionRecord> duplicates, TransactionId? transactionId)
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
                TransactionId.FromProtobuf(transactionRecord.TransactionID), 
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

		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.ToBytes"]/*' />
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="TransactionRecord.cs.xml" path='docs/member[@name="M:TransactionRecord.ToProtobuf"]/*' />
		public Proto.TransactionRecord ToProtobuf()
        {
			Proto.TransactionRecord proto = new()
			{
				Receipt = Receipt.ToProtobuf(),
                TransactionHash = TransactionHash,
                ConsensusTimestamp = ConsensusTimestamp.ToProtoTimestamp(),
                TransactionID = TransactionId.ToProtobuf(),
                Memo = TransactionMemo,
                TransactionFee = (ulong)TransactionFee.ToTinybars(),
                TransferList = new Proto.TransferList { },
                EthereumHash = EthereumHash,
                EvmAddress = EvmAddress,
				PrngNumber = PrngNumber,
			};
                
            
            foreach (var tokenEntry in TokenTransfers)
            {
                Proto.TokenTransferList tokenTransfersList = new()
				{
					Token = tokenEntry.Key.ToProtobuf(),
				};

				foreach (var aaEntry in tokenEntry.Value)
					tokenTransfersList.Transfers.Add(new Proto.AccountAmount
					{
						AccountID = aaEntry.Key.ToProtobuf(),
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
				Proto.TokenTransferList nftTransferList = new ()
                {
					Token = nftEntry.Key.ToProtobuf(),
				};

                foreach (var aaEntry in nftEntry.Value)
					nftTransferList.NftTransfers.Add(new Proto.NftTransfer
					{
						SenderAccountID = aaEntry.Sender.ToProtobuf(),
						ReceiverAccountID = aaEntry.Receiver.ToProtobuf(),
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

            if (ScheduleRef != null)
				proto.ScheduleRef = ScheduleRef.ToProtobuf();

			if (AliasKey != null)
				proto.Alias = AliasKey.ToProtobufKey().ToByteString();

            if (ParentConsensusTimestamp != null)
				proto.ParentConsensusTimestamp = ParentConsensusTimestamp.ToProtoTimestamp();

			if (PrngBytes != null)
				proto.PrngBytes = PrngBytes;

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