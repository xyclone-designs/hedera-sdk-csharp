using System.Runtime.InteropServices;

namespace Hedera.Hashgraph.SDK
{
	/**
 * A transaction to delete a node from the network address book.
 *
 * This transaction body SHALL be considered a "privileged transaction".
 *
 * - A transaction MUST be signed by the governing council.
 * - Upon success, the address book entry SHALL enter a "pending delete"
 *   state.
 * - All address book entries pending deletion SHALL be removed from the
 *   active network configuration during the next `freeze` transaction with
 *   the field `freeze_type` set to `PREPARE_UPGRADE`.<br/>
 * - A deleted address book node SHALL be removed entirely from network state.
 * - A deleted address book node identifier SHALL NOT be reused.
 *
 * ### Record Stream Effects
 * Upon completion the "deleted" `node_id` SHALL be in the transaction
 * receipt.
 */
	public class NodeDeleteTransaction extends Transaction<NodeDeleteTransaction> {


	private long nodeId;

	/**
     * Constructor.
     */
	public NodeDeleteTransaction() { }

	/**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction) records
     * @ when there is an issue with the protobuf
     */
	NodeDeleteTransaction(
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
	NodeDeleteTransaction(TransactionBody txBody)
	{
		super(txBody);
		initFromTransactionBody();
	}

	/**
     * Extract the consensus node identifier in the network state.
     * @return the consensus node identifier in the network state.
     * @ when node is not being set
     */
	public long getNodeId()
	{
		if (nodeId == null)
		{
			throw new IllegalStateException("NodeDeleteTransaction: 'nodeId' has not been set");
		}
		return nodeId;
	}

	/**
     * A consensus node identifier in the network state.
     * <p>
     * The node identified MUST exist in the network address book.<br/>
     * The node identified MUST NOT be deleted.<br/>
     * This value is REQUIRED.
     *
     * @param nodeId the consensus node identifier in the network state.
     * @return {@code this}
     * @ if nodeId is negative
     */
	public NodeDeleteTransaction setNodeId(long nodeId)
	{
		requireNotFrozen();
		if (nodeId < 0)
		{
			throw new ArgumentException("NodeDeleteTransaction: 'nodeId' must be non-negative");
		}
		this.nodeId = nodeId;
		return this;
	}

	/**
     * Build the transaction body.
     *
     * @return {@link Proto.NodeDeleteTransactionBody}
     */
	NodeDeleteTransactionBody.Builder build()
	{
		var builder = NodeDeleteTransactionBody.newBuilder();
		if (nodeId != null)
		{
			builder.setNodeId(nodeId);
		}
		return builder;
	}

	/**
     * Initialize from the transaction body.
     */
	void initFromTransactionBody()
	{
		var body = sourceTransactionBody.getNodeDelete();
		nodeId = body.getNodeId();
	}

	@Override
	void validateChecksums(Client client) 
	{
		// no-op
	}

	@Override
	MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
		return AddressBookServiceGrpc.getDeleteNodeMethod();
	}

	@Override
	void onFreeze(TransactionBody.Builder bodyBuilder)
	{
		bodyBuilder.setNodeDelete(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		scheduled.setNodeDelete(build());
	}

	/**
     * Freeze this transaction with the given client.
     *
     * @param client the client to freeze with
     * @return this transaction
     * @ if nodeId is not set
     */
	@Override
	public NodeDeleteTransaction freezeWith(@Nullable Client client)
	{
		if (nodeId == null)
		{
			throw new IllegalStateException(
					"NodeDeleteTransaction: 'nodeId' must be explicitly set before calling freeze().");
		}
		return super.freezeWith(client);
	}
}

}