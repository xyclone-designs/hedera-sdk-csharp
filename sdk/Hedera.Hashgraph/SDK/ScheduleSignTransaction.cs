using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.SDK
{
	/**
 * A transaction that appends signatures to a schedule transaction.
 * You will need to know the schedule ID to reference the schedule
 * transaction to submit signatures to. A record will be generated
 * for each ScheduleSign transaction that is successful and the schedule
 * entity will subsequently update with the public keys that have signed
 * the schedule transaction. To view the keys that have signed the
 * schedule transaction, you can query the network for the schedule info.
 * Once a schedule transaction receives the last required signature, the
 * schedule transaction executes.
 *
 * See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/sign-a-schedule-transaction">Hedera Documentation</a>
 */
	public sealed class ScheduleSignTransaction extends Transaction<ScheduleSignTransaction> {

		@Nullable

	private ScheduleId scheduleId = null;

	/**
     * Constructor.
     */
	public ScheduleSignTransaction()
	{
		defaultMaxTransactionFee = new Hbar(5);
	}

	/**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction)
     *            records
     * @       when there is an issue with the protobuf
     */
	ScheduleSignTransaction(
			LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)

			
	{
		super(txs);
		initFromTransactionBody();
	}

	/**
     * Extract the schedule id.
     *
     * @return                          the schedule id
     */
	@Nullable
	public ScheduleId getScheduleId()
	{
		return scheduleId;
	}

	/**
     * A schedule identifier.
     * <p>
     * This MUST identify the schedule which SHALL be deleted.
     *
     * @param scheduleId                the schedule id
     * @return {@code this}
     */
	public ScheduleSignTransaction setScheduleId(ScheduleId scheduleId)
	{
		Objects.requireNonNull(scheduleId);
		requireNotFrozen();
		this.scheduleId = scheduleId;
		return this;
	}

	/**
     * Clears the schedule id
     *
     * @return {@code this}
     */
	[Obsolete]
	public ScheduleSignTransaction clearScheduleId()
	{
		requireNotFrozen();
		this.scheduleId = null;
		return this;
	}

	/**
     * Build the correct transaction body.
     *
     * @return {@link
     *         Proto.ScheduleSignTransactionBody
     *         builder }
     */
	ScheduleSignTransactionBody.Builder build()
	{
		var builder = ScheduleSignTransactionBody.newBuilder();
		if (scheduleId != null)
		{
			builder.setScheduleID(scheduleId.ToProtobuf());
		}

		return builder;
	}

	/**
     * Initialize from the transaction body.
     */
	void initFromTransactionBody()
	{
		var body = sourceTransactionBody.getScheduleSign();
		if (body.hasScheduleID())
		{
			scheduleId = ScheduleId.FromProtobuf(body.getScheduleID());
		}
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (scheduleId != null) {
			scheduleId.validateChecksum(client);
		}
	}

	@Override
	MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
		return ScheduleServiceGrpc.getSignScheduleMethod();
	}

	@Override
	void onFreeze(TransactionBody.Builder bodyBuilder)
	{
		bodyBuilder.setScheduleSign(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		throw new UnsupportedOperationException("cannot schedule ScheduleSignTransaction");
	}
}

}