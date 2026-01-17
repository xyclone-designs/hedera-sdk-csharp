using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Mark a schedule in the network state as deleted.
 *
 * This transaction MUST be signed by the `adminKey` for the
 * identified schedule.<br/>
 * If a schedule does not have `adminKey` set or if `adminKey` is an empty
 * `KeyList`, that schedule SHALL be immutable and MUST NOT be deleted.<br/>
 * A deleted schedule SHALL not be executed.<br/>
 * A deleted schedule MUST NOT be the subject of a subsequent
 * `scheduleSign` transaction.
 *
 * ### Block Stream Effects
 * None
 */
	public sealed class ScheduleDeleteTransaction extends Transaction<ScheduleDeleteTransaction> {

		@Nullable

	private ScheduleId scheduleId = null;

	/**
     * Constructor.
     */
	public ScheduleDeleteTransaction()
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
	ScheduleDeleteTransaction(
			LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)

			
	{
		super(txs);
		initFromTransactionBody();
	}

	/**
     * Constructor.
     *
     * @param txBody protobuf TransactionBody
     */
	ScheduleDeleteTransaction(Proto.TransactionBody txBody)
	{
		super(txBody);
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
	public ScheduleDeleteTransaction setScheduleId(ScheduleId scheduleId)
	{
		Objects.requireNonNull(scheduleId);
		requireNotFrozen();
		this.scheduleId = scheduleId;
		return this;
	}

	/**
     * Initialize from the transaction body.
     */
	void initFromTransactionBody()
	{
		var body = sourceTransactionBody.getScheduleDelete();
		if (body.hasScheduleID())
		{
			scheduleId = ScheduleId.FromProtobuf(body.getScheduleID());
		}
	}

	/**
     * Build the correct transaction body.
     *
     * @return {@link Proto.ScheduleDeleteTransactionBody builder }
     */
	ScheduleDeleteTransactionBody.Builder build()
	{
		var builder = ScheduleDeleteTransactionBody.newBuilder();
		if (scheduleId != null)
		{
			builder.setScheduleID(scheduleId.ToProtobuf());
		}

		return builder;
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
		return ScheduleServiceGrpc.getDeleteScheduleMethod();
	}

	@Override
	void onFreeze(TransactionBody.Builder bodyBuilder)
	{
		bodyBuilder.setScheduleDelete(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		scheduled.setScheduleDelete(build());
	}
}

}