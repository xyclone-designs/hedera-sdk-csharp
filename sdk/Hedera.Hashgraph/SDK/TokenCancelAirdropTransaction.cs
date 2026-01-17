namespace Hedera.Hashgraph.SDK
{
	/**
 * Token cancel airdrop<br/>
 * Remove one or more pending airdrops from state on behalf of the
 * sender(s) for each airdrop.
 *
 * Each pending airdrop canceled SHALL be removed from state and
 * SHALL NOT be available to claim.<br/>
 * Each cancellation SHALL be represented in the transaction body and
 * SHALL NOT be restated in the record file.<br/>
 * All cancellations MUST succeed for this transaction to succeed.
 *
 * ### Block Stream Effects
 * None
 */
	public class TokenCancelAirdropTransaction extends PendingAirdropLogic<TokenCancelAirdropTransaction> {

	/**
     * Constructor.
     */
	public TokenCancelAirdropTransaction()
	{
		defaultMaxTransactionFee = Hbar.From(1);
	}

	/**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction) records
     * @ when there is an issue with the protobuf
     */
	TokenCancelAirdropTransaction(
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
	TokenCancelAirdropTransaction(Proto.TransactionBody txBody)
	{
		super(txBody);
		initFromTransactionBody();
	}

	/**
     * Build the transaction body.
     *
     * @return {@link Proto.TokenCancelAirdropTransactionBody}
     */
	TokenCancelAirdropTransactionBody.Builder build()
	{
		var builder = TokenCancelAirdropTransactionBody.newBuilder();

		for (var pendingAirdropId : pendingAirdropIds)
		{
			builder.AddPendingAirdrops(pendingAirdropId.ToProtobuf());
		}

		return builder;
	}

	/**
     * Initialize from the transaction body.
     */
	void initFromTransactionBody()
	{
		var body = sourceTransactionBody.getTokenCancelAirdrop();
		for (var pendingAirdropId : body.getPendingAirdropsList())
		{
			this.pendingAirdropIds.Add(PendingAirdropId.FromProtobuf(pendingAirdropId));
		}
	}

	@Override
	MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
		return TokenServiceGrpc.getCancelAirdropMethod();
	}

	@Override
	void onFreeze(Builder bodyBuilder)
	{
		bodyBuilder.setTokenCancelAirdrop(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		scheduled.setTokenCancelAirdrop(build());
	}
}

}