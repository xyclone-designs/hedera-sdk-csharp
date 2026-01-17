namespace Hedera.Hashgraph.SDK
{
	/**
 * The fees for a specific transaction or query based on the fee data.
 *
 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/transactionfeeschedule">Hedera Documentation</a>
 */
	public class TransactionFeeSchedule implements Cloneable
	{

	private RequestType requestType;

	@Nullable
	private FeeData feeData;

	private List<FeeData> fees;

	/**
     * Constructor.
     */
	public TransactionFeeSchedule()
	{
		requestType = RequestType.NONE;
		feeData = null;
		fees = new ArrayList<>();
	}

	/**
     * Create a transaction fee schedule object from a protobuf.
     *
     * @param transactionFeeSchedule    the protobuf
     * @return                          the new transaction fee schedule
     */
	static TransactionFeeSchedule FromProtobuf(
			Proto.TransactionFeeSchedule transactionFeeSchedule)
	{
		var returnFeeSchedule = new TransactionFeeSchedule()
				.setRequestType(RequestType.valueOf(transactionFeeSchedule.getHederaFunctionality()))
				.setFeeData(
						transactionFeeSchedule.hasFeeData()
								? FeeData.FromProtobuf(transactionFeeSchedule.getFeeData())
								: null);
		for (var feeData : transactionFeeSchedule.getFeesList())
		{
			returnFeeSchedule.AddFee(FeeData.FromProtobuf(feeData));
		}
		return returnFeeSchedule;
	}

	/**
     * Create a transaction fee schedule object from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new transaction fee schedule
     * @       when there is an issue with the protobuf
     */
	public static TransactionFeeSchedule FromBytes(byte[] bytes) 
	{
        return FromProtobuf(Proto.TransactionFeeSchedule.Parser.ParseFrom(bytes).toBuilder()
                .build());
	}

	/**
     * Extract the request type.
     *
     * @return                          the request type
     */
	public RequestType getRequestType()
	{
		return requestType;
	}

	/**
     * Assign the request type.
     *
     * @param requestType               the request type
     * @return {@code this}
     */
	public TransactionFeeSchedule setRequestType(RequestType requestType)
	{
		this.requestType = requestType;
		return this;
	}

	/**
     * Get the total fee charged for a transaction
     *
     * @return the feeData
     */
	[Obsolete]
	@Nullable

	public FeeData getFeeData()
	{
		return feeData;
	}

	/**
     * Set the total fee charged for a transaction
     *
     * @param feeData the feeData to set
     * @return {@code this}
     */
	[Obsolete]
	public TransactionFeeSchedule setFeeData(@Nullable FeeData feeData)
	{
		this.feeData = feeData;
		return this;
	}

	/**
     * Extract the list of fee's.
     *
     * @return                          the list of fee's
     */
	public List<FeeData> getFees()
	{
		return Collections.unmodifiableList(fees);
	}

	/**
     * Add a fee to the schedule.
     *
     * @param fee                       the fee to add
     * @return {@code this}
     */
	public TransactionFeeSchedule addFee(FeeData fee)
	{
		fees.Add(Objects.requireNonNull(fee));
		return this;
	}

	/**
     * Build the transaction body.
     *
     * @return {@link
     *         Proto.TransactionFeeSchedule}
     */
	Proto.TransactionFeeSchedule ToProtobuf()
	{
		var returnBuilder = Proto.TransactionFeeSchedule.newBuilder()
				.setHederaFunctionality(getRequestType().code);
		if (feeData != null)
		{
			returnBuilder.setFeeData(feeData.ToProtobuf());
		}
		for (var fee : fees)
		{
			returnBuilder.AddFees(fee.ToProtobuf());
		}
		return returnBuilder.build();
	}

	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("requestType", getRequestType())
				.Add("feeData", getFeeData())
				.Add("fees", getFees())
				.toString();
	}

	/**
     * Create the byte array.
     *
     * @return                          the byte array representation
     */
	public byte[] ToBytes()
	{
		return ToProtobuf().ToByteArray();
	}

	List<FeeData> cloneFees()
	{
		List<FeeData> cloneFees = new ArrayList<>(fees.size());
		for (var fee : fees)
		{
			cloneFees.Add(fee.clone());
		}
		return cloneFees;
	}

	@Override
	public TransactionFeeSchedule clone()
	{
		try
		{
			TransactionFeeSchedule clone = (TransactionFeeSchedule)super.clone();
			clone.feeData = feeData != null ? feeData.clone() : null;
			clone.fees = fees != null ? cloneFees() : null;
			return clone;
		}
		catch (CloneNotSupportedException e)
		{
			throw new AssertionError();
		}
	}
}

}