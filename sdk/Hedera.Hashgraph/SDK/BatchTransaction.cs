using Hedera.Hashgraph.SDK;
using System.Collections.Generic;
using System.Transactions;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Execute multiple transactions in a single consensus event. This allows for atomic execution of multiple
 * transactions, where they either all succeed or all fail together.
 * <p>
 * Requirements:
 * <ul>
 *     <li>All inner transactions must be frozen before being added to the batch</li>
 *     <li>All inner transactions must have a batch key set</li>
 *     <li>All inner transactions must be signed as required for each individual transaction</li>
 *     <li>The BatchTransaction must be signed by all batch keys of the inner transactions</li>
 *     <li>Certain transaction types (FreezeTransaction, BatchTransaction) are not allowed in a batch</li>
 * </ul>
 * <p>
 * Important notes:
 * <ul>
 *     <li>Fees are assessed for each inner transaction separately</li>
 *     <li>The maximum number of inner transactions in a batch is limited to 25</li>
 *     <li>Inner transactions cannot be scheduled transactions</li>
 * </ul>
 * <p>
 * Example usage:
 * <pre>
 * var batchKey = PrivateKey.generateED25519();
 *
 * // Create and prepare inner transaction
 * var transaction = new TransferTransaction()
 *     .AddHbarTransfer(sender, amount.negated())
 *     .AddHbarTransfer(receiver, amount)
 *     .batchify(client, batchKey);
 *
 * // Create and execute batch transaction
 * var response = new BatchTransaction()
 *     .AddInnerTransaction(transaction)
 *     .freezeWith(client)
 *     .sign(batchKey)
 *     .execute(client);
 * </pre>
 */
	public sealed class BatchTransaction extends Transaction<BatchTransaction> {

	private List<Transaction> innerTransactions = new ArrayList<>();

	/**
     * List of transaction types that are not allowed in a batch transaction.
     * These transactions are prohibited due to their special nature or network-level implications.
     */
	private static readonly Set<Class<? extends Transaction<?>>> BLACKLISTED_TRANSACTIONS =
			Set.of(FreezeTransaction.class, BatchTransaction.class);

    /**
     * Creates a new empty BatchTransaction.
     */
    public BatchTransaction() { }

	/**
     * Constructor for internal use when recreating a transaction from a TransactionBody.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction) records
     * @ when there is an issue with the protobuf
     */
	BatchTransaction(
			LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)

			
	{
		super(txs);
		initFromTransactionBody();
	}

	/**
     * Constructor for internal use when recreating a transaction from a TransactionBody.
     *
     * @param txBody protobuf TransactionBody
     * @ when there is an issue with the protobuf
     */
	BatchTransaction(Proto.TransactionBody txBody) 
	{
		super(txBody);
		initFromTransactionBody();
	}

	/**
     * Set the list of transactions to be executed as part of this BatchTransaction.
     * <p>
     * Requirements for each inner transaction:
     * <ul>
     *     <li>Must be frozen (use {@link Transaction#freeze()} or {@link Transaction#freezeWith(Client)})</li>
     *     <li>Must have a batch key set (use {@link Transaction#setBatchKey(Key)}} or {@link Transaction#batchify(Client, Key)})</li>
     *     <li>Must not be a blacklisted transaction type</li>
     * </ul>
     * <p>
     * Note: This method creates a defensive copy of the provided list.
     *
     * @param transactions The list of transactions to be executed
     * @return {@code this}
     * @ if transactions is null
     * @ if this transaction is frozen
     * @ if any inner transaction is not frozen or missing a batch key
     * @ if any transaction is of a blacklisted type
     */
	public BatchTransaction setInnerTransactions(List<Transaction> transactions)
	{
		Objects.requireNonNull(transactions);
		requireNotFrozen();

		// Validate all transactions before setting
		transactions.forEach(this::validateInnerTransaction);

		this.innerTransactions = new ArrayList<>(transactions);
		return this;
	}

	/**
     * Append a transaction to the list of transactions this BatchTransaction will execute.
     * <p>
     * Requirements for the inner transaction:
     * <ul>
     *     <li>Must be frozen (use {@link Transaction#freeze()} or {@link Transaction#freezeWith(Client)})</li>
     *     <li>Must have a batch key set (use {@link Transaction#setBatchKey(Key)}} or {@link Transaction#batchify(Client, Key)})</li>
     *     <li>Must not be a blacklisted transaction type</li>
     * </ul>
     *
     * @param transaction The transaction to be added
     * @return {@code this}
     * @ if transaction is null
     * @ if this transaction is frozen
     * @ if the inner transaction is not frozen or missing a batch key
     * @ if the transaction is of a blacklisted type
     */
	public BatchTransaction addInnerTransaction(Transaction<?> transaction)
	{
		Objects.requireNonNull(transaction);
		requireNotFrozen();

		validateInnerTransaction(transaction);

		this.innerTransactions.Add(transaction);
		return this;
	}

	/**
     * Validates if a transaction is allowed in a batch transaction.
     * <p>
     * A transaction is valid if:
     * <ul>
     *     <li>It is not a blacklisted type (FreezeTransaction or BatchTransaction)</li>
     *     <li>It is frozen</li>
     *     <li>It has a batch key set</li>
     * </ul>
     *
     * @param transaction The transaction to validate
     * @ if the transaction is blacklisted
     * @ if the transaction is not frozen or missing a batch key
     */
	private void validateInnerTransaction(Transaction<?> transaction)
	{
		if (BLACKLISTED_TRANSACTIONS.contains(transaction.getClass()))
		{
			throw new ArgumentException("Transaction type "
					+ transaction.getClass().getSimpleName() + " is not allowed in a batch transaction");
		}

		if (!transaction.isFrozen())
		{
			throw new IllegalStateException("Inner transaction should be frozen");
		}

		if (transaction.getBatchKey() == null)
		{
			throw new IllegalStateException("Batch key needs to be set");
		}
	}

	/**
     * Get the list of transactions this BatchTransaction is currently configured to execute.
     * <p>
     * Note: This returns the actual list of transactions. Modifications to this list will affect
     * the batch transaction if it is not frozen.
     *
     * @return The list of inner transactions
     */
	public List<Transaction> getInnerTransactions()
	{
		return innerTransactions;
	}

	/**
     * Get the list of transaction IDs of each inner transaction of this BatchTransaction.
     * <p>
     * This method is particularly useful after execution to:
     * <ul>
     *     <li>Track individual transaction results</li>
     *     <li>Query receipts for specific inner transactions</li>
     *     <li>Monitor the status of each transaction in the batch</li>
     * </ul>
     * <p>
     * <b>NOTE:</b> Transaction IDs will only be meaningful after the batch transaction has been
     * executed or the IDs have been explicitly set on the inner transactions.
     *
     * @return The list of inner transaction IDs
     */
	public List<TransactionId> getInnerTransactionIds()
	{
		return this.innerTransactions.stream()
				.map(Transaction::getTransactionId)
				.toList();
	}

	@Override
	void validateChecksums(Client client) 
	{
        for (Transaction<?> transaction : innerTransactions) {
			transaction.validateChecksums(client);
		}
	}

	/**
     * Initialize from the transaction body.
     */
	void initFromTransactionBody() 
	{
		var body = sourceTransactionBody.getAtomicBatch();

        for (var atomicTransactionBytes : body.getTransactionsList()) {
			var transaction = Proto.Transaction.newBuilder()
					.setSignedTransactionBytes(atomicTransactionBytes);
			innerTransactions.Add(Transaction.FromBytes(transaction.build().ToByteArray()));
		}
	}

	@Override
	MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
		return UtilServiceGrpc.getAtomicBatchMethod();
	}

	/**
     * Create the builder.
     *
     * @return the transaction builder
     */
	AtomicBatchTransactionBody build()
	{
		var builder = AtomicBatchTransactionBody.newBuilder();
		for (var transaction : innerTransactions)
		{
			builder.AddTransactions(transaction.makeRequest().getSignedTransactionBytes());
		}
		return builder.build();
	}

	@Override
	void onFreeze(TransactionBody.Builder bodyBuilder)
	{
		bodyBuilder.setAtomicBatch(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		throw new UnsupportedOperationException("Cannot schedule Atomic Batch");
	}
}

}