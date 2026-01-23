// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Time;
using Java.Util;
using Java.Util.Stream;
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
        public readonly TransactionReceipt receipt;
        /// <summary>
        /// The hash of the Transaction that executed (not the hash of any Transaction that failed for
        /// having a duplicate TransactionID).
        /// </summary>
        public readonly ByteString transactionHash;
        /// <summary>
        /// The consensus timestamp (or null if didn't reach consensus yet).
        /// </summary>
        public readonly Timestamp consensusTimestamp;
        /// <summary>
        /// The ID of the transaction this record represents.
        /// </summary>
        public readonly TransactionId transactionId;
        /// <summary>
        /// The memo that was submitted as part of the transaction (max 100 bytes).
        /// </summary>
        public readonly string transactionMemo;
        /// <summary>
        /// The actual transaction fee charged, not the original
        /// transactionFee value from TransactionBody.
        /// </summary>
        public readonly Hbar transactionFee;
        /// <summary>
        /// Record of the value returned by the smart contract
        /// function or constructor.
        /// </summary>
        public readonly ContractFunctionResult contractFunctionResult;
        /// <summary>
        /// All hbar transfers as a result of this transaction, such as fees, or
        /// transfers performed by the transaction, or by a smart contract it calls,
        /// or by the creation of threshold records that it triggers.
        /// </summary>
        public readonly IList<Transfer> transfers;
        /// <summary>
        /// All fungible token transfers as a result of this transaction as a map
        /// </summary>
        public readonly Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers;
        /// <summary>
        /// All fungible token transfers as a result of this transaction as a list
        /// </summary>
        public readonly IList<TokenTransfer> tokenTransferList;
        /// <summary>
        /// All NFT Token transfers as a result of this transaction
        /// </summary>
        public readonly Dictionary<TokenId, IList<TokenNftTransfer>> tokenNftTransfers;
        /// <summary>
        /// Reference to the scheduled transaction ID that this transaction record represents
        /// </summary>
        public readonly ScheduleId scheduleRef;
        /// <summary>
        /// All custom fees that were assessed during a CryptoTransfer, and must be paid if the
        /// transaction status resolved to SUCCESS
        /// </summary>
        public readonly IList<AssessedCustomFee> assessedCustomFees;
        /// <summary>
        /// All token associations implicitly created while handling this transaction
        /// </summary>
        public readonly IList<TokenAssociation> automaticTokenAssociations;
        /// <summary>
        /// In the record of an internal CryptoCreate transaction triggered by a user
        /// transaction with a (previously unused) alias, the new account's alias.
        /// </summary>
        public readonly PublicKey aliasKey;
        /// <summary>
        /// The records of processing all child transaction spawned by the transaction with the given
        /// top-level id, in consensus order. Always empty if the top-level status is UNKNOWN.
        /// </summary>
        public readonly IList<TransactionRecord> children;
        /// <summary>
        /// The records of processing all consensus transaction with the same id as the distinguished
        /// record above, in chronological order.
        /// </summary>
        public readonly IList<TransactionRecord> duplicates;
        /// <summary>
        /// In the record of an internal transaction, the consensus timestamp of the user
        /// transaction that spawned it.
        /// </summary>
        public readonly Timestamp parentConsensusTimestamp;
        /// <summary>
        /// The keccak256 hash of the ethereumData. This field will only be populated for
        /// EthereumTransaction.
        /// </summary>
        public readonly ByteString ethereumHash;
        /// <summary>
        /// An approved allowance of hbar transfers for a spender
        /// </summary>
        public readonly IList<HbarAllowance> hbarAllowanceAdjustments;
        /// <summary>
        /// An approved allowance of token transfers for a spender
        /// </summary>
        public readonly IList<TokenAllowance> tokenAllowanceAdjustments;
        /// <summary>
        /// An approved allowance of NFT transfers for a spender
        /// </summary>
        public readonly IList<TokenNftAllowance> tokenNftAllowanceAdjustments;
        /// <summary>
        /// List of accounts with the corresponding staking rewards paid as a result of a transaction.
        /// </summary>
        public readonly IList<Transfer> paidStakingRewards;
        /// <summary>
        /// In the record of a UtilPrng transaction with no output range, a pseudorandom 384-bit string.
        /// </summary>
        public readonly ByteString prngBytes;
        /// <summary>
        /// In the record of a PRNG transaction with an output range, the output of a PRNG whose input was a 384-bit string.
        /// </summary>
        public readonly int prngNumber;
        /// <summary>
        /// The new default EVM address of the account created by this transaction.
        /// This field is populated only when the EVM address is not specified in the related transaction body.
        /// </summary>
        public readonly ByteString evmAddress;
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
        public readonly IList<PendingAirdropRecord> pendingAirdropRecords;
        TransactionRecord(TransactionReceipt transactionReceipt, ByteString transactionHash, Timestamp consensusTimestamp, TransactionId transactionId, string transactionMemo, long transactionFee, ContractFunctionResult contractFunctionResult, IList<Transfer> transfers, Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers, IList<TokenTransfer> tokenTransferList, Dictionary<TokenId, IList<TokenNftTransfer>> tokenNftTransfers, ScheduleId scheduleRef, IList<AssessedCustomFee> assessedCustomFees, IList<TokenAssociation> automaticTokenAssociations, PublicKey aliasKey, IList<TransactionRecord> children, IList<TransactionRecord> duplicates, Timestamp parentConsensusTimestamp, ByteString ethereumHash, IList<Transfer> paidStakingRewards, ByteString prngBytes, int prngNumber, ByteString evmAddress, IList<PendingAirdropRecord> pendingAirdropRecords)
        {
            receipt = transactionReceipt;
            transactionHash = transactionHash;
            consensusTimestamp = consensusTimestamp;
            transactionMemo = transactionMemo;
            transactionId = transactionId;
            transfers = transfers;
            contractFunctionResult = contractFunctionResult;
            transactionFee = Hbar.FromTinybars(transactionFee);
            tokenTransfers = tokenTransfers;
            tokenTransferList = tokenTransferList;
            tokenNftTransfers = tokenNftTransfers;
            scheduleRef = scheduleRef;
            assessedCustomFees = assessedCustomFees;
            automaticTokenAssociations = automaticTokenAssociations;
            aliasKey = aliasKey;
            children = children;
            duplicates = duplicates;
            parentConsensusTimestamp = parentConsensusTimestamp;
            ethereumHash = ethereumHash;
            pendingAirdropRecords = pendingAirdropRecords;
            hbarAllowanceAdjustments = Collections.EmptyList();
            tokenAllowanceAdjustments = Collections.EmptyList();
            tokenNftAllowanceAdjustments = Collections.EmptyList();
            paidStakingRewards = paidStakingRewards;
            prngBytes = prngBytes;
            prngNumber = prngNumber;
            evmAddress = evmAddress;
        }

        /// <summary>
        /// Create a transaction record from a protobuf.
        /// </summary>
        /// <param name="transactionRecord">the protobuf</param>
        /// <param name="children">the list of children</param>
        /// <param name="duplicates">the list of duplicates</param>
        /// <returns>the new transaction record</returns>
        static TransactionRecord FromProtobuf(Proto.TransactionRecord transactionRecord, IList<TransactionRecord> children, IList<TransactionRecord> duplicates, TransactionId transactionId)
        {
            var transfers = new List<Transfer>(transactionRecord.GetTransferList().GetAccountAmountsCount());
            foreach (var accountAmount in transactionRecord.GetTransferList().GetAccountAmountsList())
            {
                transfers.Add(Transfer.FromProtobuf(accountAmount));
            }

            var tokenTransfers = new HashMap<TokenId, Dictionary<AccountId, long>>();
            var tokenNftTransfers = new HashMap<TokenId, IList<TokenNftTransfer>>();
            var allTokenTransfers = new List<TokenTransfer>();
            foreach (var transferList in transactionRecord.GetTokenTransferListsList())
            {
                var tokenTransfersList = TokenTransfer.FromProtobuf(transferList);
                var nftTransfersList = TokenNftTransfer.FromProtobuf(transferList);
                foreach (var transfer in tokenTransfersList)
                {
                    var current = tokenTransfers.ContainsKey(transfer.tokenId) ? tokenTransfers[transfer.tokenId] : new HashMap<AccountId, long>();
                    current.Put(transfer.accountId, transfer.amount);
                    tokenTransfers.Put(transfer.tokenId, current);
                }

                allTokenTransfers.AddAll(tokenTransfersList);
                foreach (var transfer in nftTransfersList)
                {
                    var current = tokenNftTransfers.ContainsKey(transfer.tokenId) ? tokenNftTransfers[transfer.tokenId] : new List<TokenNftTransfer>();
                    current.Add(transfer);
                    tokenNftTransfers.Put(transfer.tokenId, current);
                }
            }

            var fees = new List<AssessedCustomFee>(transactionRecord.GetAssessedCustomFeesCount());
            foreach (var fee in transactionRecord.GetAssessedCustomFeesList())
            {
                fees.Add(AssessedCustomFee.FromProtobuf(fee));
            }


            // HACK: This is a bit bad, any takers to clean this up
            var contractFunctionResult = transactionRecord.HasContractCallResult() ? new ContractFunctionResult(transactionRecord.GetContractCallResult()) : transactionRecord.HasContractCreateResult() ? new ContractFunctionResult(transactionRecord.GetContractCreateResult()) : null;
            var automaticTokenAssociations = new List<TokenAssociation>(transactionRecord.GetAutomaticTokenAssociationsCount());
            foreach (var tokenAssociation in transactionRecord.GetAutomaticTokenAssociationsList())
            {
                automaticTokenAssociations.Add(TokenAssociation.FromProtobuf(tokenAssociation));
            }

            var aliasKey = PublicKey.FromAliasBytes(transactionRecord.GetAlias());
            var paidStakingRewards = new List<Transfer>(transactionRecord.GetPaidStakingRewardsCount());
            foreach (var reward in transactionRecord.GetPaidStakingRewardsList())
            {
                paidStakingRewards.Add(Transfer.FromProtobuf(reward));
            }

            IList<PendingAirdropRecord> pendingAirdropRecords = transactionRecord.GetNewPendingAirdropsList().Stream().Map(PendingAirdropRecord.FromProtobuf()).Collect(Collectors.ToList());
            return new TransactionRecord(TransactionReceipt.FromProtobuf(transactionRecord.GetReceipt(), transactionId), transactionRecord.GetTransactionHash(), Utils.TimestampConverter.FromProtobuf(transactionRecord.GetConsensusTimestamp()), TransactionId.FromProtobuf(transactionRecord.GetTransactionID()), transactionRecord.GetMemo(), transactionRecord.GetTransactionFee(), contractFunctionResult, transfers, tokenTransfers, allTokenTransfers, tokenNftTransfers, transactionRecord.HasScheduleRef() ? ScheduleId.FromProtobuf(transactionRecord.GetScheduleRef()) : null, fees, automaticTokenAssociations, aliasKey, children, duplicates, transactionRecord.HasParentConsensusTimestamp() ? Utils.TimestampConverter.FromProtobuf(transactionRecord.GetParentConsensusTimestamp()) : null, transactionRecord.GetEthereumHash(), paidStakingRewards, transactionRecord.HasPrngBytes() ? transactionRecord.GetPrngBytes() : null, transactionRecord.HasPrngNumber() ? transactionRecord.GetPrngNumber() : null, transactionRecord.EvmAddress, pendingAirdropRecords);
        }

        /// <summary>
        /// Create a transaction record from a protobuf.
        /// </summary>
        /// <param name="transactionRecord">the protobuf</param>
        /// <returns>the new transaction record</returns>
        static TransactionRecord FromProtobuf(Proto.TransactionRecord transactionRecord)
        {
            return FromProtobuf(transactionRecord, new (), new (), null);
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
        /// Validate the transaction status in the receipt.
        /// </summary>
        /// <param name="shouldValidate">Whether to perform transaction status validation</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="ReceiptStatusException">when shouldValidate is true and the transaction status is not SUCCESS</exception>
        public TransactionRecord ValidateReceiptStatus(bool shouldValidate)
        {
            receipt.ValidateStatus(shouldValidate);
            return this;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>the protobuf representation</returns>
        Proto.TransactionRecord ToProtobuf()
        {
            var transferList = TransferList.NewBuilder();
            foreach (Transfer transfer in transfers)
            {
                transferList.AddAccountAmounts(transfer.ToProtobuf());
            }

            var transactionRecord = Proto.TransactionRecord.NewBuilder().SetReceipt(receipt.ToProtobuf()).SetTransactionHash(transactionHash).SetConsensusTimestamp(Utils.TimestampConverter.ToProtobuf(consensusTimestamp)).SetTransactionID(transactionId.ToProtobuf()).SetMemo(transactionMemo).SetTransactionFee(transactionFee.ToTinybars()).SetTransferList(transferList).SetEthereumHash(ethereumHash).SetEvmAddress(evmAddress);
            foreach (var tokenEntry in tokenTransfers.EntrySet())
            {
                var tokenTransfersList = TokenTransferList.NewBuilder().SetToken(tokenEntry.GetKey().ToProtobuf());
                foreach (var aaEntry in tokenEntry.GetValue().EntrySet())
                {
                    tokenTransfersList.AddTransfers(AccountAmount.NewBuilder().SetAccountID(aaEntry.GetKey().ToProtobuf()).SetAmount(aaEntry.GetValue()).Build());
                }

                transactionRecord.AddTokenTransferLists(tokenTransfersList);
            }

            foreach (var nftEntry in tokenNftTransfers.EntrySet())
            {
                var nftTransferList = TokenTransferList.NewBuilder().SetToken(nftEntry.GetKey().ToProtobuf());
                foreach (var aaEntry in nftEntry.GetValue())
                {
                    nftTransferList.AddNftTransfers(NftTransfer.NewBuilder().SetSenderAccountID(aaEntry.sender.ToProtobuf()).SetReceiverAccountID(aaEntry.receiver.ToProtobuf()).SetSerialNumber(aaEntry.serial).SetIsApproval(aaEntry.isApproved).Build());
                }

                transactionRecord.AddTokenTransferLists(nftTransferList);
            }

            if (contractFunctionResult != null)
            {
                transactionRecord.SetContractCallResult(contractFunctionResult.ToProtobuf());
            }

            if (scheduleRef != null)
            {
                transactionRecord.SetScheduleRef(scheduleRef.ToProtobuf());
            }

            foreach (var fee in assessedCustomFees)
            {
                transactionRecord.AddAssessedCustomFees(fee.ToProtobuf());
            }

            foreach (var tokenAssociation in automaticTokenAssociations)
            {
                transactionRecord.AddAutomaticTokenAssociations(tokenAssociation.ToProtobuf());
            }

            if (aliasKey != null)
            {
                transactionRecord.SetAlias(aliasKey.ToProtobufKey().ToByteString());
            }

            if (parentConsensusTimestamp != null)
            {
                transactionRecord.SetParentConsensusTimestamp(Utils.TimestampConverter.ToProtobuf(parentConsensusTimestamp));
            }

            foreach (Transfer reward in paidStakingRewards)
            {
                transactionRecord.AddPaidStakingRewards(reward.ToProtobuf());
            }

            if (prngBytes != null)
            {
                transactionRecord.SetPrngBytes(prngBytes);
            }

            if (prngNumber != null)
            {
                transactionRecord.SetPrngNumber(prngNumber);
            }

            if (pendingAirdropRecords != null)
            {
                foreach (PendingAirdropRecord pendingAirdropRecord in pendingAirdropRecords)
                {
                    transactionRecord.AddNewPendingAirdrops(pendingAirdropRecords.IndexOf(pendingAirdropRecord), pendingAirdropRecord.ToProtobuf());
                }
            }

            return transactionRecord.Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("receipt", receipt).Add("transactionHash", Hex.ToHexString(transactionHash.ToByteArray())).Add("consensusTimestamp", consensusTimestamp).Add("transactionId", transactionId).Add("transactionMemo", transactionMemo).Add("transactionFee", transactionFee).Add("contractFunctionResult", contractFunctionResult).Add("transfers", transfers).Add("tokenTransfers", tokenTransfers).Add("tokenNftTransfers", tokenNftTransfers).Add("scheduleRef", scheduleRef).Add("assessedCustomFees", assessedCustomFees).Add("automaticTokenAssociations", automaticTokenAssociations).Add("aliasKey", aliasKey).Add("children", children).Add("duplicates", duplicates).Add("parentConsensusTimestamp", parentConsensusTimestamp).Add("ethereumHash", Hex.ToHexString(ethereumHash.ToByteArray())).Add("paidStakingRewards", paidStakingRewards).Add("prngBytes", prngBytes != null ? Hex.ToHexString(prngBytes.ToByteArray()) : null).Add("prngNumber", prngNumber).Add("evmAddress", Hex.ToHexString(evmAddress.ToByteArray())).Add("pendingAirdropRecords", pendingAirdropRecords.ToString()).ToString();
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}