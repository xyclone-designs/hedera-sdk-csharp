// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// The complete record for a transaction on Hedera that has reached consensus.
    /// <p>
    /// This is not-free to request and is available for 1 hour after a transaction reaches consensus.
    /// <p>
    /// A {@link TransactionReceipt} can be thought of as a light-weight record which is free to ask for if you just
    /// need what it contains. A receipt however lasts for only 180 seconds.
    /// </summary>
    public sealed class TransactionRecord
    {
        /// <summary>
        /// The status (reach consensus, or failed, or is unknown) and the ID of
        /// any new account/file/instance created.
        /// </summary>
        public readonly TransactionReceipt Receipt;
        /// <summary>
        /// The hash of the Transaction that executed (not the hash of any Transaction that failed for
        /// having a duplicate TransactionID).
        /// </summary>
        public readonly ByteString TransactionHash;
        /// <summary>
        /// The consensus timestamp (or null if didn't reach consensus yet).
        /// </summary>
        public readonly Timestamp ConsensusTimestamp;
        /// <summary>
        /// The ID of the transaction this record represents.
        /// </summary>
        public readonly TransactionId TransactionId;
        /// <summary>
        /// The memo that was submitted as part of the transaction (max 100 bytes).
        /// </summary>
        public readonly string TransactionMemo;
        /// <summary>
        /// The actual transaction fee charged, not the original
        /// transactionFee value from TransactionBody.
        /// </summary>
        public readonly Hbar TransactionFee;
        /// <summary>
        /// Record of the value returned by the smart contract
        /// function or constructor.
        /// </summary>
        public readonly ContractFunctionResult ContractFunctionResult;
        /// <summary>
        /// All hbar transfers as a result of this transaction, such as fees, or
        /// transfers performed by the transaction, or by a smart contract it calls,
        /// or by the creation of threshold records that it triggers.
        /// </summary>
        public readonly IList<Transfer> Transfers;
        /// <summary>
        /// All fungible token transfers as a result of this transaction as a map
        /// </summary>
        public readonly Dictionary<TokenId, Dictionary<AccountId, long>> TokenTransfers;
        /// <summary>
        /// All fungible token transfers as a result of this transaction as a list
        /// </summary>
        public readonly IList<TokenTransfer> TokenTransferList;
        /// <summary>
        /// All NFT Token transfers as a result of this transaction
        /// </summary>
        public readonly Dictionary<TokenId, IList<TokenNftTransfer>> TokenNftTransfers;
        /// <summary>
        /// Reference to the scheduled transaction ID that this transaction record represents
        /// </summary>
        public readonly ScheduleId ScheduleRef;
        /// <summary>
        /// All custom fees that were assessed during a CryptoTransfer, and must be paid if the
        /// transaction status resolved to SUCCESS
        /// </summary>
        public readonly IList<AssessedCustomFee> AssessedCustomFees;
        /// <summary>
        /// All token associations implicitly created while handling this transaction
        /// </summary>
        public readonly IList<TokenAssociation> AutomaticTokenAssociations;
        /// <summary>
        /// In the record of an internal CryptoCreate transaction triggered by a user
        /// transaction with a (previously unused) alias, the new account's alias.
        /// </summary>
        public readonly PublicKey AliasKey;
        /// <summary>
        /// The records of processing all child transaction spawned by the transaction with the given
        /// top-level id, in consensus order. Always empty if the top-level status is UNKNOWN.
        /// </summary>
        public readonly IList<TransactionRecord> Children;
        /// <summary>
        /// The records of processing all consensus transaction with the same id as the distinguished
        /// record above, in chronological order.
        /// </summary>
        public readonly IList<TransactionRecord> Duplicates;
        /// <summary>
        /// In the record of an internal transaction, the consensus timestamp of the user
        /// transaction that spawned it.
        /// </summary>
        public readonly Timestamp ParentConsensusTimestamp;
        /// <summary>
        /// The keccak256 hash of the ethereumData. This field will only be populated for
        /// EthereumTransaction.
        /// </summary>
        public readonly ByteString EthereumHash;
        /// <summary>
        /// An approved allowance of hbar transfers for a spender
        /// </summary>
        public readonly IList<HbarAllowance> HbarAllowanceAdjustments;
        /// <summary>
        /// An approved allowance of token transfers for a spender
        /// </summary>
        public readonly IList<TokenAllowance> TokenAllowanceAdjustments;
        /// <summary>
        /// An approved allowance of NFT transfers for a spender
        /// </summary>
        public readonly IList<TokenNftAllowance> TokenNftAllowanceAdjustments;
        /// <summary>
        /// List of accounts with the corresponding staking rewards paid as a result of a transaction.
        /// </summary>
        public readonly IList<Transfer> PaidStakingRewards;
        /// <summary>
        /// In the record of a UtilPrng transaction with no output range, a pseudorandom 384-bit string.
        /// </summary>
        public readonly ByteString PrngBytes;
        /// <summary>
        /// In the record of a PRNG transaction with an output range, the output of a PRNG whose input was a 384-bit string.
        /// </summary>
        public readonly int PrngNumber;
        /// <summary>
        /// The new default EVM address of the account created by this transaction.
        /// This field is populated only when the EVM address is not specified in the related transaction body.
        /// </summary>
        public readonly ByteString EvmAddress;
        /// <summary>
        /// A list of pending token airdrops.
        /// Each pending airdrop represents a single requested transfer from a
        /// sending account to a recipient account. These pending transfers are
        /// issued unilaterally by the sending account, and MUST be claimed by the
        /// recipient account before the transfer MAY complete.
        /// A sender MAY cancel a pending airdrop before it is claimed.
        /// An airdrop transaction SHALL emit a pending airdrop when the recipient has no
        /// available automatic association slots available or when the recipient
        /// has set `receiver_sig_required`.
        /// </summary>
        public readonly IList<PendingAirdropRecord> PendingAirdropRecords;

        TransactionRecord(TransactionReceipt transactionReceipt, ByteString transactionHash, Timestamp consensusTimestamp, TransactionId transactionId, string transactionMemo, long transactionFee, ContractFunctionResult contractFunctionResult, IList<Transfer> transfers, Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers, IList<TokenTransfer> tokenTransferList, Dictionary<TokenId, IList<TokenNftTransfer>> tokenNftTransfers, ScheduleId scheduleRef, IList<AssessedCustomFee> assessedCustomFees, IList<TokenAssociation> automaticTokenAssociations, PublicKey aliasKey, IList<TransactionRecord> children, IList<TransactionRecord> duplicates, Timestamp parentConsensusTimestamp, ByteString ethereumHash, IList<Transfer> paidStakingRewards, ByteString prngBytes, int prngNumber, ByteString evmAddress, IList<PendingAirdropRecord> pendingAirdropRecords)
        {
            Receipt = transactionReceipt;
            TransactionHash = transactionHash;
            ConsensusTimestamp = consensusTimestamp;
            TransactionMemo = transactionMemo;
            TransactionId = transactionId;
            Transfers = transfers;
            ContractFunctionResult = contractFunctionResult;
            TransactionFee = Hbar.FromTinybars(transactionFee);
            TokenTransfers = tokenTransfers;
            TokenTransferList = tokenTransferList;
            TokenNftTransfers = tokenNftTransfers;
            ScheduleRef = scheduleRef;
            AssessedCustomFees = assessedCustomFees;
            AutomaticTokenAssociations = automaticTokenAssociations;
            AliasKey = aliasKey;
            Children = children;
            Duplicates = duplicates;
            ParentConsensusTimestamp = parentConsensusTimestamp;
            EthereumHash = ethereumHash;
            PendingAirdropRecords = pendingAirdropRecords;
            HbarAllowanceAdjustments = [];
            TokenAllowanceAdjustments = [];
            TokenNftAllowanceAdjustments = [];
            PaidStakingRewards = paidStakingRewards;
            PrngBytes = prngBytes;
            PrngNumber = prngNumber;
            EvmAddress = evmAddress;
        }

		/// <summary>
		/// Create a transaction record from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>the new transaction record</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static TransactionRecord FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TransactionRecord.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a transaction record from a protobuf.
		/// </summary>
		/// <param name="transactionRecord">the protobuf</param>
		/// <returns>the new transaction record</returns>
		public static TransactionRecord FromProtobuf(Proto.TransactionRecord transactionRecord)
		{
			return FromProtobuf(transactionRecord, [], [], null);
		}
		/// <summary>
		/// Create a transaction record from a protobuf.
		/// </summary>
		/// <param name="transactionRecord">the protobuf</param>
		/// <param name="children">the list of children</param>
		/// <param name="duplicates">the list of duplicates</param>
		/// <returns>the new transaction record</returns>
		public static TransactionRecord FromProtobuf(Proto.TransactionRecord transactionRecord, IList<TransactionRecord> children, IList<TransactionRecord> duplicates, TransactionId? transactionId)
        {
            var transfers = new List<Transfer>(transactionRecord.TransferList.AccountAmounts.Count);

            foreach (var accountAmount in transactionRecord.TransferList.AccountAmounts)
            {
                transfers.Add(Transfer.FromProtobuf(accountAmount));
            }

            var tokenTransfers = new Dictionary<TokenId, Dictionary<AccountId, long>>();
            var tokenNftTransfers = new Dictionary<TokenId, IList<TokenNftTransfer>>();
            var allTokenTransfers = new List<TokenTransfer>();

            foreach (var transferList in transactionRecord.TokenTransferLists)
            {
                var tokenTransfersList = TokenTransfer.FromProtobuf(transferList);
                var nftTransfersList = TokenNftTransfer.FromProtobuf(transferList);

                foreach (var transfer in tokenTransfersList)
                {
                    var current = tokenTransfers.TryGetValue(transfer.TokenId, out Dictionary<AccountId, long>? value) ? value : [];

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
                Utils.TimestampConverter.FromProtobuf(transactionRecord.ConsensusTimestamp), 
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
				Utils.TimestampConverter.FromProtobuf(transactionRecord.ParentConsensusTimestamp), 
                transactionRecord.EthereumHash, 
                paidStakingRewards,
				transactionRecord.PrngBytes, 
                transactionRecord.PrngNumber, 
                transactionRecord.EvmAddress, 
                [.. transactionRecord.NewPendingAirdrops.Select(_ => PendingAirdropRecord.FromProtobuf(_))]);
        }

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>the byte array representation</returns>
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>the protobuf representation</returns>
		public Proto.TransactionRecord ToProtobuf()
        {
			Proto.TransactionRecord proto = new()
			{
				Receipt = Receipt.ToProtobuf(),
                TransactionHash = TransactionHash,
                ConsensusTimestamp = Utils.TimestampConverter.ToProtobuf(ConsensusTimestamp),
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
				proto.ParentConsensusTimestamp = Utils.TimestampConverter.ToProtobuf(ParentConsensusTimestamp);

			if (PrngBytes != null)
				proto.PrngBytes = PrngBytes;

			return proto;
        }
		/// <summary>
		/// Validate the transaction status in the receipt.
		/// </summary>
		/// <param name="shouldValidate">Whether to perform transaction status validation</param>
		/// <returns>{@code this}</returns>
		/// <exception cref="ReceiptStatusException">when shouldValidate is true and the transaction status is not SUCCESS</exception>
		public TransactionRecord ValidateReceiptStatus(bool shouldValidate)
		{
			Receipt.ValidateStatus(shouldValidate);
			return this;
		}
	}
}