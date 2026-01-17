namespace Hedera.Hashgraph.SDK
{
	/**
 * Current information on the smart contract instance, including its balance.
 */
public sealed class ContractInfo {
    /**
     * ID of the contract instance, in the format used in transactions.
     */
    public readonly ContractId contractId;

    /**
     * ID of the cryptocurrency account owned by the contract instance,
     * in the format used in transactions.
     */
    public readonly AccountId accountId;

    /**
     * ID of both the contract instance and the cryptocurrency account owned by the contract
     * instance, in the format used by Solidity.
     */
    public readonly string contractAccountId;

    /**
     * The state of the instance and its fields can be modified arbitrarily if this key signs a
     * transaction to modify it. If this is null, then such modifications are not possible,
     * and there is no administrator that can override the normal operation of this smart
     * contract instance. Note that if it is created with no admin keys, then there is no
     * administrator to authorize changing the admin keys, so there can never be any admin keys
     * for that instance.
     */
    @Nullable
    public readonly Key adminKey;

    /**
     * The current time at which this contract instance (and its account) is set to expire.
     */
    public readonly DateTimeOffset expirationTime;

    /**
     * The expiration time will extend every this many seconds. If there are insufficient funds,
     * then it extends as long as possible. If the account is empty when it expires,
     * then it is deleted.
     */
    public readonly Duration autoRenewPeriod;

    /**
     * ID of the an account to charge for auto-renewal of this contract. If not set, or set to
     * an account with zero hbar balance, the contract's own hbar balance will be used to cover
     * auto-renewal fees.
     */
    @Nullable
    public readonly AccountId autoRenewAccountId;

    /**
     * Number of bytes of storage being used by this instance (which affects the cost to
     * extend the expiration time).
     */
    public readonly long storage;

    /**
     * The memo associated with the contract (max 100 bytes).
     */
    public readonly string contractMemo;

    /**
     * The current balance of the contract.
     */
    public readonly Hbar balance;

    /**
     * Whether the contract has been deleted
     */
    public readonly bool isDeleted;

    /**
     * The tokens associated to the contract
     */
    public readonly Dictionary<TokenId, TokenRelationship> tokenRelationships;

    /**
     * The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
     */
    public readonly LedgerId ledgerId;

    /**
     * Staking metadata for this account.
     */
    @Nullable
    public readonly StakingInfo stakingInfo;

    /**
     *  Constructor.
     *
     * @param contractId                the contract id
     * @param accountId                 the account id
     * @param contractAccountId         the account id of the owner
     * @param adminKey                  the key that can modify the contract
     * @param expirationTime            the time that contract will expire
     * @param autoRenewPeriod           seconds before contract is renewed (funds must be available)
     * @param autoRenewAccountId        account ID which will be charged for renewing this account
     * @param storage                   number of bytes used by this contract
     * @param contractMemo              the memo field 100 bytes
     * @param balance                   current balance
     * @param isDeleted                 does it still exist
     * @param tokenRelationships        list of compound token id and relationship records
     * @param ledgerId                  the ledger id
     */
    private ContractInfo(
            ContractId contractId,
            AccountId accountId,
            string contractAccountId,
            @Nullable Key adminKey,
            DateTimeOffset expirationTime,
            Duration autoRenewPeriod,
            @Nullable AccountId autoRenewAccountId,
            long storage,
            string contractMemo,
            Hbar balance,
            bool isDeleted,
            Dictionary<TokenId, TokenRelationship> tokenRelationships,
            LedgerId ledgerId,
            @Nullable StakingInfo stakingInfo) {
        this.contractId = contractId;
        this.accountId = accountId;
        this.contractAccountId = contractAccountId;
        this.adminKey = adminKey;
        this.expirationTime = expirationTime;
        this.autoRenewPeriod = autoRenewPeriod;
        this.autoRenewAccountId = autoRenewAccountId;
        this.storage = storage;
        this.contractMemo = contractMemo;
        this.balance = balance;
        this.isDeleted = isDeleted;
        this.tokenRelationships = tokenRelationships;
        this.ledgerId = ledgerId;
        this.stakingInfo = stakingInfo;
    }

    /**
     * Extract the contract from the protobuf.
     *
     * @param contractInfo              the protobuf
     * @return                          the contract object
     */
    static ContractInfo FromProtobuf(ContractGetInfoResponse.ContractInfo contractInfo) {
        var adminKey = contractInfo.hasAdminKey() ? Key.FromProtobufKey(contractInfo.getAdminKey()) : null;

        var tokenRelationships = new HashMap<TokenId, TokenRelationship>(contractInfo.getTokenRelationshipsCount());

        for (var relationship : contractInfo.getTokenRelationshipsList()) {
            tokenRelationships.put(
                    TokenId.FromProtobuf(relationship.getTokenId()), TokenRelationship.FromProtobuf(relationship));
        }

        return new ContractInfo(
                ContractId.FromProtobuf(contractInfo.getContractID()),
                AccountId.FromProtobuf(contractInfo.getAccountID()),
                contractInfo.getContractAccountID(),
                adminKey,
                DateTimeOffsetConverter.FromProtobuf(contractInfo.getExpirationTime()),
                DurationConverter.FromProtobuf(contractInfo.getAutoRenewPeriod()),
                contractInfo.hasAutoRenewAccountId()
                        ? AccountId.FromProtobuf(contractInfo.getAutoRenewAccountId())
                        : null,
                contractInfo.getStorage(),
                contractInfo.getMemo(),
                Hbar.FromTinybars(contractInfo.getBalance()),
                contractInfo.getDeleted(),
                tokenRelationships,
                LedgerId.FromByteString(contractInfo.getLedgerId()),
                contractInfo.hasStakingInfo() ? StakingInfo.FromProtobuf(contractInfo.getStakingInfo()) : null);
    }

    /**
     * Extract the contract from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the extracted contract
     * @       when there is an issue with the protobuf
     */
    public static ContractInfo FromBytes(byte[] bytes)  {
        return FromProtobuf(ContractGetInfoResponse.ContractInfo.Parser.ParseFrom(bytes).toBuilder()
                .build());
    }

    /**
     * Build the protobuf.
     *
     * @return                          the protobuf representation
     */
    ContractGetInfoResponse.ContractInfo ToProtobuf() {
        var contractInfoBuilder = ContractGetInfoResponse.ContractInfo.newBuilder()
                .setContractID(contractId.ToProtobuf())
                .setAccountID(accountId.ToProtobuf())
                .setContractAccountID(contractAccountId)
                .setExpirationTime(DateTimeOffsetConverter.ToProtobuf(expirationTime))
                .setAutoRenewPeriod(DurationConverter.ToProtobuf(autoRenewPeriod))
                .setStorage(storage)
                .setMemo(contractMemo)
                .setBalance(balance.toTinybars())
                .setLedgerId(ledgerId.toByteString());

        if (adminKey != null) {
            contractInfoBuilder.setAdminKey(adminKey.ToProtobufKey());
        }

        if (stakingInfo != null) {
            contractInfoBuilder.setStakingInfo(stakingInfo.ToProtobuf());
        }

        if (autoRenewAccountId != null) {
            contractInfoBuilder.setAutoRenewAccountId(autoRenewAccountId.ToProtobuf());
        }

        return contractInfoBuilder.build();
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("contractId", contractId)
                .Add("accountId", accountId)
                .Add("contractAccountId", contractAccountId)
                .Add("adminKey", adminKey)
                .Add("expirationTime", expirationTime)
                .Add("autoRenewPeriod", autoRenewPeriod)
                .Add("autoRenewAccountId", autoRenewAccountId)
                .Add("storage", storage)
                .Add("contractMemo", contractMemo)
                .Add("balance", balance)
                .Add("isDeleted", isDeleted)
                .Add("tokenRelationships", tokenRelationships)
                .Add("ledgerId", ledgerId)
                .Add("stakingInfo", stakingInfo)
                .toString();
    }

    /**
     * Create a byte array representation.
     *
     * @return                          the byte array representation
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }
}

}