namespace Hedera.Hashgraph.SDK
{
	/**
 * The complete record for a transaction on Hedera that has reached consensus.
 * <p>
 * This is not-free to request and is available for 1 hour after a transaction reaches consensus.
 * <p>
 * A {@link TransactionReceipt} can be thought of as a light-weight record which is free to ask for if you just
 * need what it contains. A receipt however lasts for only 180 seconds.
 */
	public sealed class TransactionRecord
	{
		/**
		 * The status (reach consensus, or failed, or is unknown) and the ID of
		 * any new account/file/instance created.
		 */
		public readonly TransactionReceipt receipt;

    /**
     * The hash of the Transaction that executed (not the hash of any Transaction that failed for
     * having a duplicate TransactionID).
     */
    public readonly ByteString transactionHash;

    /**
     * The consensus timestamp (or null if didn't reach consensus yet).
     */
    public readonly DateTimeOffset consensusTimestamp;

    /**
     * The ID of the transaction this record represents.
     */
    public readonly TransactionId transactionId;

    /**
     * The memo that was submitted as part of the transaction (max 100 bytes).
     */
    public readonly string transactionMemo;

    /**
     * The actual transaction fee charged, not the original
     * transactionFee value from TransactionBody.
     */
    public readonly Hbar transactionFee;

    /**
     * Record of the value returned by the smart contract
     * function or constructor.
     */
    @Nullable
		public readonly ContractFunctionResult contractFunctionResult;

    /**
     * All hbar transfers as a result of this transaction, such as fees, or
     * transfers performed by the transaction, or by a smart contract it calls,
     * or by the creation of threshold records that it triggers.
     */
    public readonly List<Transfer> transfers;

		/**
		 * All fungible token transfers as a result of this transaction as a map
		 */
		public readonly Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers;

		/**
		 * All fungible token transfers as a result of this transaction as a list
		 */
		public readonly List<TokenTransfer> tokenTransferList;

		/**
		 * All NFT Token transfers as a result of this transaction
		 */
		public readonly Dictionary<TokenId, List<TokenNftTransfer>> tokenNftTransfers;

		/**
		 * Reference to the scheduled transaction ID that this transaction record represents
		 */
		@Nullable
		public readonly ScheduleId scheduleRef;

    /**
     * All custom fees that were assessed during a CryptoTransfer, and must be paid if the
     * transaction status resolved to SUCCESS
     */
    public readonly List<AssessedCustomFee> assessedCustomFees;

		/**
		 * All token associations implicitly created while handling this transaction
		 */
		public readonly List<TokenAssociation> automaticTokenAssociations;

		/**
		 * In the record of an internal CryptoCreate transaction triggered by a user
		 * transaction with a (previously unused) alias, the new account's alias.
		 */
		@Nullable
		public readonly PublicKey aliasKey;

    /**
     * The records of processing all child transaction spawned by the transaction with the given
     * top-level id, in consensus order. Always empty if the top-level status is UNKNOWN.
     */
    public readonly List<TransactionRecord> children;

		/**
		 * The records of processing all consensus transaction with the same id as the distinguished
		 * record above, in chronological order.
		 */
		public readonly List<TransactionRecord> duplicates;

		/**
		 * In the record of an internal transaction, the consensus timestamp of the user
		 * transaction that spawned it.
		 */
		@Nullable
		public readonly DateTimeOffset parentConsensusTimestamp;

    /**
     * The keccak256 hash of the ethereumData. This field will only be populated for
     * EthereumTransaction.
     */
    public readonly ByteString ethereumHash;

    /**
     * An approved allowance of hbar transfers for a spender
     */
    [Obsolete]
		public readonly List<HbarAllowance> hbarAllowanceAdjustments;

		/**
		 * An approved allowance of token transfers for a spender
		 */
		[Obsolete]
		public readonly List<TokenAllowance> tokenAllowanceAdjustments;

		/**
		 * An approved allowance of NFT transfers for a spender
		 */
		[Obsolete]
		public readonly List<TokenNftAllowance> tokenNftAllowanceAdjustments;

		/**
		 * List of accounts with the corresponding staking rewards paid as a result of a transaction.
		 */
		public readonly List<Transfer> paidStakingRewards;

		/**
		 * In the record of a UtilPrng transaction with no output range, a pseudorandom 384-bit string.
		 */
		@Nullable
		public readonly ByteString prngBytes;

    /**
     * In the record of a PRNG transaction with an output range, the output of a PRNG whose input was a 384-bit string.
     */
    @Nullable
		public readonly Integer prngNumber;

    /**
     * The new default EVM address of the account created by this transaction.
     * This field is populated only when the EVM address is not specified in the related transaction body.
     */
    public readonly ByteString evmAddress;

    /**
     * A list of pending token airdrops.
     * Each pending airdrop represents a single requested transfer from a
     * sending account to a recipient account. These pending transfers are
     * issued unilaterally by the sending account, and MUST be claimed by the
     * recipient account before the transfer MAY complete.
     * A sender MAY cancel a pending airdrop before it is claimed.
     * An airdrop transaction SHALL emit a pending airdrop when the recipient has no
     * available automatic association slots available or when the recipient
     * has set `receiver_sig_required`.
     */
    public readonly List<PendingAirdropRecord> pendingAirdropRecords;

		TransactionRecord(
				TransactionReceipt transactionReceipt,
				ByteString transactionHash,
				DateTimeOffset consensusTimestamp,
				TransactionId transactionId,
				string transactionMemo,
				long transactionFee,
				@Nullable ContractFunctionResult contractFunctionResult,
				List<Transfer> transfers,
				Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers,
				List<TokenTransfer> tokenTransferList,
				Dictionary<TokenId, List<TokenNftTransfer>> tokenNftTransfers,
				@Nullable ScheduleId scheduleRef,
				List<AssessedCustomFee> assessedCustomFees,
				List<TokenAssociation> automaticTokenAssociations,
				@Nullable PublicKey aliasKey,
				List<TransactionRecord> children,
				List<TransactionRecord> duplicates,
				@Nullable DateTimeOffset parentConsensusTimestamp,
				ByteString ethereumHash,
				List<Transfer> paidStakingRewards,
				@Nullable ByteString prngBytes,
				@Nullable Integer prngNumber,
				ByteString evmAddress,
				List<PendingAirdropRecord> pendingAirdropRecords)
		{
			this.receipt = transactionReceipt;
			this.transactionHash = transactionHash;
			this.consensusTimestamp = consensusTimestamp;
			this.transactionMemo = transactionMemo;
			this.transactionId = transactionId;
			this.transfers = transfers;
			this.contractFunctionResult = contractFunctionResult;
			this.transactionFee = Hbar.FromTinybars(transactionFee);
			this.tokenTransfers = tokenTransfers;
			this.tokenTransferList = tokenTransferList;
			this.tokenNftTransfers = tokenNftTransfers;
			this.scheduleRef = scheduleRef;
			this.assessedCustomFees = assessedCustomFees;
			this.automaticTokenAssociations = automaticTokenAssociations;
			this.aliasKey = aliasKey;
			this.children = children;
			this.duplicates = duplicates;
			this.parentConsensusTimestamp = parentConsensusTimestamp;
			this.ethereumHash = ethereumHash;
			this.pendingAirdropRecords = pendingAirdropRecords;
			this.hbarAllowanceAdjustments = Collections.emptyList();
			this.tokenAllowanceAdjustments = Collections.emptyList();
			this.tokenNftAllowanceAdjustments = Collections.emptyList();
			this.paidStakingRewards = paidStakingRewards;
			this.prngBytes = prngBytes;
			this.prngNumber = prngNumber;
			this.evmAddress = evmAddress;
		}

		/**
		 * Create a transaction record from a protobuf.
		 *
		 * @param transactionRecord the protobuf
		 * @param children          the list of children
		 * @param duplicates        the list of duplicates
		 * @return the new transaction record
		 */
		static TransactionRecord FromProtobuf(
				Proto.TransactionRecord transactionRecord,
				List<TransactionRecord> children,
				List<TransactionRecord> duplicates,
				@Nullable TransactionId transactionId)
		{
			var transfers =
					new ArrayList<Transfer>(transactionRecord.getTransferList().getAccountAmountsCount());
			for (var accountAmount : transactionRecord.getTransferList().getAccountAmountsList())
			{
				transfers.Add(Transfer.FromProtobuf(accountAmount));
			}

			var tokenTransfers = new HashMap<TokenId, Dictionary<AccountId, long>>();
			var tokenNftTransfers = new HashMap<TokenId, List<TokenNftTransfer>>();
			var allTokenTransfers = new ArrayList<TokenTransfer>();

			for (var transferList : transactionRecord.getTokenTransferListsList())
			{
				var tokenTransfersList = TokenTransfer.FromProtobuf(transferList);
				var nftTransfersList = TokenNftTransfer.FromProtobuf(transferList);
				for (var transfer : tokenTransfersList)
				{
					var current = tokenTransfers.containsKey(transfer.tokenId)
							? tokenTransfers.get(transfer.tokenId)
							: new HashMap<AccountId, long>();
					current.put(transfer.accountId, transfer.amount);
					tokenTransfers.put(transfer.tokenId, current);
				}
				allTokenTransfers.AddAll(tokenTransfersList);

				for (var transfer : nftTransfersList)
				{
					var current = tokenNftTransfers.containsKey(transfer.tokenId)
							? tokenNftTransfers.get(transfer.tokenId)
							: new ArrayList<TokenNftTransfer>();
					current.Add(transfer);
					tokenNftTransfers.put(transfer.tokenId, current);
				}
			}

			var fees = new ArrayList<AssessedCustomFee>(transactionRecord.getAssessedCustomFeesCount());
			for (var fee : transactionRecord.getAssessedCustomFeesList())
			{
				fees.Add(AssessedCustomFee.FromProtobuf(fee));
			}

			// HACK: This is a bit bad, any takers to clean this up
			var contractFunctionResult = transactionRecord.hasContractCallResult()
					? new ContractFunctionResult(transactionRecord.getContractCallResult())
					: transactionRecord.hasContractCreateResult()
							? new ContractFunctionResult(transactionRecord.getContractCreateResult())
							: null;

			var automaticTokenAssociations =
					new ArrayList<TokenAssociation>(transactionRecord.getAutomaticTokenAssociationsCount());
			for (var tokenAssociation : transactionRecord.getAutomaticTokenAssociationsList())
			{
				automaticTokenAssociations.Add(TokenAssociation.FromProtobuf(tokenAssociation));
			}

			var aliasKey = PublicKey.FromAliasBytes(transactionRecord.getAlias());

			var paidStakingRewards = new ArrayList<Transfer>(transactionRecord.getPaidStakingRewardsCount());
			for (var reward : transactionRecord.getPaidStakingRewardsList())
			{
				paidStakingRewards.Add(Transfer.FromProtobuf(reward));
			}

			List<PendingAirdropRecord> pendingAirdropRecords = transactionRecord.getNewPendingAirdropsList().stream()
					.map(PendingAirdropRecord::FromProtobuf)
					.collect(Collectors.toList());

			return new TransactionRecord(
					TransactionReceipt.FromProtobuf(transactionRecord.getReceipt(), transactionId),
					transactionRecord.getTransactionHash(),
					DateTimeOffsetConverter.FromProtobuf(transactionRecord.getConsensusTimestamp()),
					TransactionId.FromProtobuf(transactionRecord.getTransactionID()),
					transactionRecord.getMemo(),
					transactionRecord.getTransactionFee(),
					contractFunctionResult,
					transfers,
					tokenTransfers,
					allTokenTransfers,
					tokenNftTransfers,
					transactionRecord.hasScheduleRef() ? ScheduleId.FromProtobuf(transactionRecord.getScheduleRef()) : null,
					fees,
					automaticTokenAssociations,
					aliasKey,
					children,
					duplicates,
					transactionRecord.hasParentConsensusTimestamp()
							? DateTimeOffsetConverter.FromProtobuf(transactionRecord.getParentConsensusTimestamp())
							: null,
					transactionRecord.getEthereumHash(),
					paidStakingRewards,
					transactionRecord.hasPrngBytes() ? transactionRecord.getPrngBytes() : null,
					transactionRecord.hasPrngNumber() ? transactionRecord.getPrngNumber() : null,
					transactionRecord.getEvmAddress(),
					pendingAirdropRecords);
		}

		/**
		 * Create a transaction record from a protobuf.
		 *
		 * @param transactionRecord the protobuf
		 * @return the new transaction record
		 */
		static TransactionRecord FromProtobuf(Proto.TransactionRecord transactionRecord)
		{
			return FromProtobuf(transactionRecord, new ArrayList<>(), new ArrayList<>(), null);
		}

		/**
		 * Create a transaction record from a byte array.
		 *
		 * @param bytes the byte array
		 * @return the new transaction record
		 * @ when there is an issue with the protobuf
		 */
		public static TransactionRecord FromBytes(byte[] bytes) 
		{
        return FromProtobuf(Proto.TransactionRecord.Parser.ParseFrom(bytes).toBuilder()
	                .build());
    }

    /**
     * Validate the transaction status in the receipt.
     *
     * @param shouldValidate Whether to perform transaction status validation
     * @return {@code this}
     * @ when shouldValidate is true and the transaction status is not SUCCESS
     */
    public TransactionRecord validateReceiptStatus(bool shouldValidate) 
		{
			receipt.validateStatus(shouldValidate);
        return this;
		}

		/**
		 * Create the protobuf.
		 *
		 * @return the protobuf representation
		 */
		Proto.TransactionRecord ToProtobuf()
		{
			var transferList = TransferList.newBuilder();
			for (Transfer transfer : transfers)
			{
				transferList.AddAccountAmounts(transfer.ToProtobuf());
			}

			var transactionRecord = Proto.TransactionRecord.newBuilder()
					.setReceipt(receipt.ToProtobuf())
					.setTransactionHash(transactionHash)
					.setConsensusTimestamp(DateTimeOffsetConverter.ToProtobuf(consensusTimestamp))
					.setTransactionID(transactionId.ToProtobuf())
					.setMemo(transactionMemo)
					.setTransactionFee(transactionFee.toTinybars())
					.setTransferList(transferList)
					.setEthereumHash(ethereumHash)
					.setEvmAddress(evmAddress);

			for (var tokenEntry : tokenTransfers.entrySet())
			{
				var tokenTransfersList =
						TokenTransferList.newBuilder().setToken(tokenEntry.getKey().ToProtobuf());
				for (var aaEntry : tokenEntry.getValue().entrySet())
				{
					tokenTransfersList.AddTransfers(AccountAmount.newBuilder()
							.setAccountID(aaEntry.getKey().ToProtobuf())
							.setAmount(aaEntry.getValue())
							.build());
				}

				transactionRecord.AddTokenTransferLists(tokenTransfersList);
			}

			for (var nftEntry : tokenNftTransfers.entrySet())
			{
				var nftTransferList =
						TokenTransferList.newBuilder().setToken(nftEntry.getKey().ToProtobuf());
				for (var aaEntry : nftEntry.getValue())
				{
					nftTransferList.AddNftTransfers(NftTransfer.newBuilder()
							.setSenderAccountID(aaEntry.sender.ToProtobuf())
							.setReceiverAccountID(aaEntry.receiver.ToProtobuf())
							.setSerialNumber(aaEntry.serial)
							.setIsApproval(aaEntry.isApproved)
							.build());
				}

				transactionRecord.AddTokenTransferLists(nftTransferList);
			}

			if (contractFunctionResult != null)
			{
				transactionRecord.setContractCallResult(contractFunctionResult.ToProtobuf());
			}

			if (scheduleRef != null)
			{
				transactionRecord.setScheduleRef(scheduleRef.ToProtobuf());
			}

			for (var fee : assessedCustomFees)
			{
				transactionRecord.AddAssessedCustomFees(fee.ToProtobuf());
			}

			for (var tokenAssociation : automaticTokenAssociations)
			{
				transactionRecord.AddAutomaticTokenAssociations(tokenAssociation.ToProtobuf());
			}

			if (aliasKey != null)
			{
				transactionRecord.setAlias(aliasKey.ToProtobufKey().toByteString());
			}

			if (parentConsensusTimestamp != null)
			{
				transactionRecord.setParentConsensusTimestamp(DateTimeOffsetConverter.ToProtobuf(parentConsensusTimestamp));
			}

			for (Transfer reward : paidStakingRewards)
			{
				transactionRecord.AddPaidStakingRewards(reward.ToProtobuf());
			}

			if (prngBytes != null)
			{
				transactionRecord.setPrngBytes(prngBytes);
			}

			if (prngNumber != null)
			{
				transactionRecord.setPrngNumber(prngNumber);
			}

			if (pendingAirdropRecords != null)
			{
				for (PendingAirdropRecord pendingAirdropRecord : pendingAirdropRecords)
				{
					transactionRecord.AddNewPendingAirdrops(
							pendingAirdropRecords.indexOf(pendingAirdropRecord), pendingAirdropRecord.ToProtobuf());
				}
			}

			return transactionRecord.build();
		}


	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("receipt", receipt)
				.Add("transactionHash", Hex.toHexString(transactionHash.ToByteArray()))
				.Add("consensusTimestamp", consensusTimestamp)
				.Add("transactionId", transactionId)
				.Add("transactionMemo", transactionMemo)
				.Add("transactionFee", transactionFee)
				.Add("contractFunctionResult", contractFunctionResult)
				.Add("transfers", transfers)
				.Add("tokenTransfers", tokenTransfers)
				.Add("tokenNftTransfers", tokenNftTransfers)
				.Add("scheduleRef", scheduleRef)
				.Add("assessedCustomFees", assessedCustomFees)
				.Add("automaticTokenAssociations", automaticTokenAssociations)
				.Add("aliasKey", aliasKey)
				.Add("children", children)
				.Add("duplicates", duplicates)
				.Add("parentConsensusTimestamp", parentConsensusTimestamp)
				.Add("ethereumHash", Hex.toHexString(ethereumHash.ToByteArray()))
				.Add("paidStakingRewards", paidStakingRewards)
				.Add("prngBytes", prngBytes != null ? Hex.toHexString(prngBytes.ToByteArray()) : null)
				.Add("prngNumber", prngNumber)
				.Add("evmAddress", Hex.toHexString(evmAddress.ToByteArray()))
				.Add("pendingAirdropRecords", pendingAirdropRecords.toString())
				.toString();
	}

	/**
     * Create the byte array.
     *
     * @return the byte array representation
     */
	public byte[] ToBytes()
	{
		return ToProtobuf().ToByteArray();
	}
}

}