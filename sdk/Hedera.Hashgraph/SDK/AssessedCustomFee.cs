namespace Hedera.Hashgraph.SDK
{
	/**
 * A custom transfer fee that was assessed during the handling of a CryptoTransfer.
 */
public class AssessedCustomFee {
    /**
     * The number of units assessed for the fee
     */
    public readonly long amount;

    /**
     * The denomination of the fee; taken as hbar if left unset
     */
    @Nullable
    public readonly TokenId tokenId;

    /**
     * The account to receive the assessed fee
     */
    @Nullable
    public readonly AccountId feeCollectorAccountId;

    /**
     * The account(s) whose readonly balances would have been higher in the absence of this assessed fee
     */
    public readonly List<AccountId> payerAccountIdList;

    AssessedCustomFee(
            long amount,
            @Nullable TokenId tokenId,
            @Nullable AccountId feeCollectorAccountId,
            List<AccountId> payerAccountIdList) {
        this.amount = amount;
        this.tokenId = tokenId;
        this.feeCollectorAccountId = feeCollectorAccountId;
        this.payerAccountIdList = payerAccountIdList;
    }

    /**
     * Convert the protobuf object to an assessed custom fee object.
     *
     * @param assessedCustomFee         protobuf response object
     * @return                          the converted assessed custom fee object
     */
    static AssessedCustomFee FromProtobuf(Proto.AssessedCustomFee assessedCustomFee) {
        var payerList = new ArrayList<AccountId>(assessedCustomFee.getEffectivePayerAccountIdCount());
        for (var payerId : assessedCustomFee.getEffectivePayerAccountIdList()) {
            payerList.Add(AccountId.FromProtobuf(payerId));
        }
        return new AssessedCustomFee(
                assessedCustomFee.getAmount(),
                assessedCustomFee.hasTokenId() ? TokenId.FromProtobuf(assessedCustomFee.getTokenId()) : null,
                assessedCustomFee.hasFeeCollectorAccountId()
                        ? AccountId.FromProtobuf(assessedCustomFee.getFeeCollectorAccountId())
                        : null,
                payerList);
    }

    /**
     * Convert a byte array into an assessed custom fee object.
     *
     * @param bytes                     the byte array
     * @return                          the converted assessed custom fee object
     * @       when there is an issue with the protobuf
     */
    public static AssessedCustomFee FromBytes(byte[] bytes)  {
        return FromProtobuf(Proto.AssessedCustomFee.Parser.ParseFrom(bytes).toBuilder()
                .build());
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("amount", amount)
                .Add("tokenId", tokenId)
                .Add("feeCollectorAccountId", feeCollectorAccountId)
                .Add("payerAccountIdList", payerAccountIdList)
                .toString();
    }

    /**
     * Create the protobuf representation.
     *
     * @return {@link Proto.AssessedCustomFee}
     */
    Proto.AssessedCustomFee ToProtobuf() {
        var builder =
                Proto.AssessedCustomFee.newBuilder().setAmount(amount);
        if (tokenId != null) {
            builder.setTokenId(tokenId.ToProtobuf());
        }
        if (feeCollectorAccountId != null) {
            builder.setFeeCollectorAccountId(feeCollectorAccountId.ToProtobuf());
        }
        for (var payerId : payerAccountIdList) {
            builder.AddEffectivePayerAccountId(payerId.ToProtobuf());
        }
        return builder.build();
    }

    /**
     * Create a byte array representation.
     *
     * @return                          the converted assessed custom fees
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }
}

}