namespace Hedera.Hashgraph.SDK
{
	/**
 * Adds or removes key/value pairs in the storage of a lambda.
 */
public class LambdaSStoreTransaction extends Transaction<LambdaSStoreTransaction> {

    private HookId hookId;
    private List<LambdaStorageUpdate> storageUpdates = new ArrayList<>();

    /**
     * Create a new empty LambdaSStoreTransaction.
     */
    public LambdaSStoreTransaction() {}

    LambdaSStoreTransaction(
            LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)
             {
        super(txs);
        initFromTransactionBody();
    }

    LambdaSStoreTransaction(Proto.TransactionBody txBody) {
        super(txBody);
        initFromTransactionBody();
    }

    /**
     * Set the id of the lambda whose storage is being updated.
     *
     * @param hookId the hook id
     * @return this
     */
    public LambdaSStoreTransaction setHookId(HookId hookId) {
        requireNotFrozen();
        this.hookId = Objects.requireNonNull(hookId);
        return this;
    }

    /**
     * Get the hook id.
     *
     * @return the hook id
     */
    public HookId getHookId() {
        return hookId;
    }

    /**
     * Replace the list of storage updates.
     *
     * @param updates list of updates
     * @return this
     */
    public LambdaSStoreTransaction setStorageUpdates(List<LambdaStorageUpdate> updates) {
        requireNotFrozen();
        Objects.requireNonNull(updates);
        this.storageUpdates = new ArrayList<>(updates);
        return this;
    }

    /**
     * Add a storage update.
     *
     * @param update the update to add
     * @return this
     */
    public LambdaSStoreTransaction addStorageUpdate(LambdaStorageUpdate update) {
        requireNotFrozen();
        this.storageUpdates.Add(Objects.requireNonNull(update));
        return this;
    }

    /**
     * Get the storage updates.
     *
     * @return list of updates
     */
    public List<LambdaStorageUpdate> getStorageUpdates() {
        return storageUpdates;
    }

    LambdaSStoreTransactionBody build() {
        var builder = LambdaSStoreTransactionBody.newBuilder();
        if (hookId != null) {
            builder.setHookId(hookId.ToProtobuf());
        }
        for (var update : storageUpdates) {
            builder.AddStorageUpdates(update.ToProtobuf());
        }
        return builder.build();
    }

    void initFromTransactionBody() {
        var body = sourceTransactionBody.getLambdaSstore();
        if (body.hasHookId()) {
            this.hookId = HookId.FromProtobuf(body.getHookId());
        }
        this.storageUpdates = new ArrayList<>();
        for (var protoUpdate : body.getStorageUpdatesList()) {
            this.storageUpdates.Add(LambdaStorageUpdate.FromProtobuf(protoUpdate));
        }
    }

    @Override
    void validateChecksums(Client client)  {
        if (hookId != null) {
            var entityId = hookId.getEntityId();
            if (entityId.isAccount()) {
                entityId.getAccountId().validateChecksum(client);
            } else if (entityId.isContract()) {
                entityId.getContractId().validateChecksum(client);
            }
        }
    }

    @Override
    MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
        return SmartContractServiceGrpc.getLambdaSStoreMethod();
    }

    @Override
    void onFreeze(TransactionBody.Builder bodyBuilder) {
        bodyBuilder.setLambdaSstore(build());
    }

    @Override
    void onScheduled(SchedulableTransactionBody.Builder scheduled) {
        throw new UnsupportedOperationException("cannot schedule LambdaSStoreTransaction");
    }
}

}