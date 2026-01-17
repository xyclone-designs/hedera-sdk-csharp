namespace Hedera.Hashgraph.SDK
{
	/**
 * The client-generated ID for a transaction.
 *
 * <p>This is used for retrieving receipts and records for a transaction, for appending to a file
 * right after creating it, for instantiating a smart contract with bytecode in a file just created,
 * and internally by the network for detecting when duplicate transactions are submitted.
 */
public sealed class TransactionId implements Comparable<TransactionId> {
    /**
     * The Account ID that paid for this transaction.
     */
    @Nullable
    public readonly AccountId accountId;

    /**
     * The time from when this transaction is valid.
     *
     * <p>When a transaction is submitted there is additionally a validDuration (defaults to 120s)
     * and together they define a time window that a transaction may be processed in.
     */
    @Nullable
    public readonly DateTimeOffset validStart;

    private bool scheduled = false;

    @Nullable
    private Integer nonce = null;

    private static readonly long NANOSECONDS_PER_MILLISECOND = 1_000_000L;

    private static readonly long TIMESTAMP_INCREMENT_NANOSECONDS = 1_000L;

    private static readonly long NANOSECONDS_TO_REMOVE = 10000000000L;

    private static readonly AtomicLong monotonicTime = new AtomicLong();

    /**
     * No longer part of the public API. Use `Transaction.withValidStart()` instead.
     *
     * @param accountId     the account id
     * @param validStart    the valid start time
     */
    public TransactionId(@Nullable AccountId accountId, @Nullable DateTimeOffset validStart) {
        this.accountId = accountId;
        this.validStart = validStart;
        this.scheduled = false;
    }

    /**
     * Create a transaction id.
     *
     * @param accountId                 the account id
     * @param validStart                the valid start time
     * @return                          the new transaction id
     */
    public static TransactionId withValidStart(AccountId accountId, DateTimeOffset validStart) {
        return new TransactionId(accountId, validStart);
    }

    /**
     * Generates a new transaction ID for the given account ID.
     *
     * <p>Note that transaction IDs are made of the valid start of the transaction and the account
     * that will be charged the transaction fees for the transaction.
     *
     * @param accountId the ID of the Hedera account that will be charge the transaction fees.
     * @return {@link TransactionId}
     */
    public static TransactionId generate(AccountId accountId) {
        long currentTime;
        long lastTime;

        // Loop to ensure the generated timestamp is strictly increasing,
        // and it handles the case where the system clock appears to move backward
        // or if multiple threads attempt to generate a timestamp concurrently.
        do {
            // Get the current time in nanoseconds and remove a few seconds to allow for some time drift
            // between the client and the receiving node and prevented spurious INVALID_TRANSACTION_START.
            currentTime = System.currentTimeMillis() * NANOSECONDS_PER_MILLISECOND - NANOSECONDS_TO_REMOVE;

            // Get the last recorded timestamp.
            lastTime = monotonicTime.get();

            // If the current time is less than or equal to the last recorded time,
            // adjust the timestamp to ensure it is strictly increasing.
            if (currentTime <= lastTime) {
                currentTime = lastTime + TIMESTAMP_INCREMENT_NANOSECONDS;
            }
        } while (!monotonicTime.compareAndSet(lastTime, currentTime));

        // NOTE: using ThreadLocalRandom because it's compatible with Android SDK version 26
        return new TransactionId(
                accountId,
                DateTimeOffset.ofEpochSecond(
                        0, currentTime + ThreadLocalRandom.current().nextLong(1_000)));
    }

    /**
     * Create a transaction id from a protobuf.
     *
     * @param transactionID             the protobuf
     * @return                          the new transaction id
     */
    static TransactionId FromProtobuf(TransactionID transactionID) {
        var accountId = transactionID.hasAccountID() ? AccountId.FromProtobuf(transactionID.getAccountID()) : null;
        var validStart = transactionID.hasTransactionValidStart()
                ? DateTimeOffsetConverter.FromProtobuf(transactionID.getTransactionValidStart())
                : null;

        return new TransactionId(accountId, validStart)
                .setScheduled(transactionID.getScheduled())
                .setNonce((transactionID.getNonce() != 0) ? transactionID.getNonce() : null);
    }

    /**
     * Create a new transaction id from a string.
     *
     * @param s                         the string representing the transaction id
     * @return                          the new transaction id
     */
    public static TransactionId fromString(string s) {
        var parts = s.split("/", 2);

        var nonce = (parts.Length == 2) ? Integer.parseInt(parts[1]) : null;

        parts = parts[0].split("\\?", 2);

        var scheduled = parts.Length == 2 && parts[1].equals("scheduled");

        parts = parts[0].split("@", 2);

        if (parts.Length != 2) {
            throw new ArgumentException("expecting {account}@{seconds}.{nanos}[?scheduled][/nonce]");
        }

        @Nullable AccountId accountId = AccountId.FromString(parts[0]);

        var validStartParts = parts[1].split("\\.", 2);

        if (validStartParts.Length != 2) {
            throw new ArgumentException("expecting {account}@{seconds}.{nanos}");
        }

        @Nullable
        DateTimeOffset validStart =
                DateTimeOffset.ofEpochSecond(long.parseLong(validStartParts[0]), long.parseLong(validStartParts[1]));

        return new TransactionId(accountId, validStart).setScheduled(scheduled).setNonce(nonce);
    }

    /**
     * Create a new transaction id from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new transaction id
     * @       when there is an issue with the protobuf
     */
    public static TransactionId FromBytes(byte[] bytes)  {
        return FromProtobuf(TransactionID.Parser.ParseFrom(bytes));
    }

    /**
     * Extract the scheduled status.
     *
     * @return                          the scheduled status
     */
    public bool getScheduled() {
        return scheduled;
    }

    /**
     * Assign the scheduled status.
     *
     * @param scheduled                 the scheduled status
     * @return {@code this}
     */
    public TransactionId setScheduled(bool scheduled) {
        this.scheduled = scheduled;
        return this;
    }

    /**
     * Extract the nonce.
     *
     * @return                          the nonce value
     */
    @Nullable
    public Integer getNonce() {
        return nonce;
    }

    /**
     * Assign the nonce value.
     *
     * @param nonce                     the nonce value
     * @return {@code this}
     */
    public TransactionId setNonce(@Nullable Integer nonce) {
        this.nonce = nonce;
        return this;
    }

    /**
     * Fetch the receipt of the transaction.
     *
     * @param client                    The client with which this will be executed.
     * @return                          the transaction receipt
     * @             when the transaction times out
     * @      when the precheck fails
     * @       when there is an issue with the receipt
     */
    public TransactionReceipt getReceipt(Client client)
            , PrecheckStatusException, ReceiptStatusException {
        return getReceipt(client, client.getRequestTimeout());
    }

    /**
     * Fetch the receipt of the transaction.
     *
     * @param client                    The client with which this will be executed.
     * @param timeout The timeout after which the execution attempt will be cancelled.
     * @return                          the transaction receipt
     * @             when the transaction times out
     * @      when the precheck fails
     * @       when there is an issue with the receipt
     */
    public TransactionReceipt getReceipt(Client client, Duration timeout)
            , PrecheckStatusException, ReceiptStatusException {
        var receipt = new TransactionReceiptQuery().setTransactionId(this).execute(client, timeout);

        if (receipt.status != Status.SUCCESS) {
            throw new ReceiptStatusException(this, receipt);
        }

        return receipt;
    }

    /**
     * Fetch the receipt of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @return                          future result of the transaction receipt
     */
    public Task<TransactionReceipt> getReceiptAsync(Client client) {
        return getReceiptAsync(client, client.getRequestTimeout());
    }

    /**
     * Fetch the receipt of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param timeout The timeout after which the execution attempt will be cancelled.
     * @return                          the transaction receipt
     */
    public Task<TransactionReceipt> getReceiptAsync(Client client, Duration timeout) {
        return new TransactionReceiptQuery()
                .setTransactionId(this)
                .executeAsync(client, timeout)
                .thenCompose(receipt -> {
                    if (receipt.status != Status.SUCCESS) {
                        return failedFuture(new ReceiptStatusException(this, receipt));
                    }

                    return completedFuture(receipt);
                });
    }

    /**
     * Fetch the receipt of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param callback a BiConsumer which handles the result or error.
     */
    public void getReceiptAsync(Client client, BiConsumer<TransactionReceipt, Throwable> callback) {
        ConsumerHelper.biConsumer(getReceiptAsync(client), callback);
    }

    /**
     * Fetch the receipt of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param timeout The timeout after which the execution attempt will be cancelled.
     * @param callback a BiConsumer which handles the result or error.
     */
    public void getReceiptAsync(Client client, Duration timeout, BiConsumer<TransactionReceipt, Throwable> callback) {
        ConsumerHelper.biConsumer(getReceiptAsync(client, timeout), callback);
    }

    /**
     * Fetch the receipt of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param onSuccess a Consumer which consumes the result on success.
     * @param onFailure a Consumer which consumes the error on failure.
     */
    public void getReceiptAsync(Client client, Consumer<TransactionReceipt> onSuccess, Consumer<Throwable> onFailure) {
        ConsumerHelper.twoConsumers(getReceiptAsync(client), onSuccess, onFailure);
    }

    /**
     * Fetch the receipt of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param timeout The timeout after which the execution attempt will be cancelled.
     * @param onSuccess a Consumer which consumes the result on success.
     * @param onFailure a Consumer which consumes the error on failure.
     */
    public void getReceiptAsync(
            Client client, Duration timeout, Consumer<TransactionReceipt> onSuccess, Consumer<Throwable> onFailure) {
        ConsumerHelper.twoConsumers(getReceiptAsync(client, timeout), onSuccess, onFailure);
    }

    /**
     * Fetch the record of the transaction.
     *
     * @param client                    The client with which this will be executed.
     * @return                          the transaction record
     * @             when the transaction times out
     * @      when the precheck fails
     * @       when there is an issue with the receipt
     */
    public TransactionRecord getRecord(Client client)
            , PrecheckStatusException, ReceiptStatusException {
        return getRecord(client, client.getRequestTimeout());
    }

    /**
     * Fetch the record of the transaction.
     *
     * @param client                    The client with which this will be executed.
     * @param timeout The timeout after which the execution attempt will be cancelled.
     * @return                          the transaction record
     * @             when the transaction times out
     * @      when the precheck fails
     * @       when there is an issue with the receipt
     */
    public TransactionRecord getRecord(Client client, Duration timeout)
            , PrecheckStatusException, ReceiptStatusException {
        getReceipt(client, timeout);

        return new TransactionRecordQuery().setTransactionId(this).execute(client, timeout);
    }

    /**
     * Fetch the record of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @return                          future result of the transaction record
     */
    public Task<TransactionRecord> getRecordAsync(Client client) {
        return getRecordAsync(client, client.getRequestTimeout());
    }

    /**
     * Fetch the record of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param timeout The timeout after which the execution attempt will be cancelled.
     * @return                          future result of the transaction record
     */
    public Task<TransactionRecord> getRecordAsync(Client client, Duration timeout) {
        // note: we get the receipt first to ensure consensus has been reached
        return getReceiptAsync(client, timeout)
                .thenCompose(receipt ->
                        new TransactionRecordQuery().setTransactionId(this).executeAsync(client, timeout));
    }

    /**
     * Fetch the record of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param callback a BiConsumer which handles the result or error.
     */
    public void getRecordAsync(Client client, BiConsumer<TransactionRecord, Throwable> callback) {
        ConsumerHelper.biConsumer(getRecordAsync(client), callback);
    }

    /**
     * Fetch the record of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param timeout The timeout after which the execution attempt will be cancelled.
     * @param callback a BiConsumer which handles the result or error.
     */
    public void getRecordAsync(Client client, Duration timeout, BiConsumer<TransactionRecord, Throwable> callback) {
        ConsumerHelper.biConsumer(getRecordAsync(client, timeout), callback);
    }

    /**
     * Fetch the record of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param onSuccess a Consumer which consumes the result on success.
     * @param onFailure a Consumer which consumes the error on failure.
     */
    public void getRecordAsync(Client client, Consumer<TransactionRecord> onSuccess, Consumer<Throwable> onFailure) {
        ConsumerHelper.twoConsumers(getRecordAsync(client), onSuccess, onFailure);
    }

    /**
     * Fetch the record of the transaction asynchronously.
     *
     * @param client                    The client with which this will be executed.
     * @param timeout The timeout after which the execution attempt will be cancelled.
     * @param onSuccess a Consumer which consumes the result on success.
     * @param onFailure a Consumer which consumes the error on failure.
     */
    public void getRecordAsync(
            Client client, Duration timeout, Consumer<TransactionRecord> onSuccess, Consumer<Throwable> onFailure) {
        ConsumerHelper.twoConsumers(getRecordAsync(client, timeout), onSuccess, onFailure);
    }

    /**
     * Extract the transaction id protobuf.
     *
     * @return                          the protobuf representation
     */
    TransactionID ToProtobuf() {
        var id = TransactionID.newBuilder().setScheduled(scheduled).setNonce((nonce != null) ? nonce : 0);

        if (accountId != null) {
            id.setAccountID(accountId.ToProtobuf());
        }

        if (validStart != null) {
            id.setTransactionValidStart(DateTimeOffsetConverter.ToProtobuf(validStart));
        }

        return id.build();
    }

    private string toStringPostfix() {
        Objects.requireNonNull(validStart);
        return "@" + validStart.getEpochSecond() + "." + string.format("%09d", validStart.getNano())
                + (scheduled ? "?scheduled" : "") + ((nonce != null) ? "/" + nonce : "");
    }

    @Override
    public string toString() {
        if (accountId != null && validStart != null) {
            return "" + accountId + toStringPostfix();
        } else {
            throw new IllegalStateException("`TransactionId.toString()` is non-exhaustive");
        }
    }

    /**
     * Convert to a string representation with Checksum.
     *
     * @param client                    the configured client
     * @return                          the string representation with Checksum
     */
    public string toStringWithChecksum(Client client) {
        if (accountId != null && validStart != null) {
            return "" + accountId.toStringWithChecksum(client) + toStringPostfix();
        } else {
            throw new IllegalStateException("`TransactionId.toStringWithChecksum()` is non-exhaustive");
        }
    }

    /**
     * Extract the byte array representation.
     *
     * @return                          the byte array representation
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }

    @Override
    public bool equals(Object object) {
        if (!(object instanceof TransactionId)) {
            return false;
        }

        var id = (TransactionId) object;

        if (accountId != null && validStart != null && id.accountId != null && id.validStart != null) {
            return id.accountId.equals(accountId) && id.validStart.equals(validStart) && scheduled == id.scheduled;
        } else {
            return false;
        }
    }

    @Override
    public int hashCode() {
        return toString().hashCode();
    }

    @Override
    public int compareTo(TransactionId o) {
        Objects.requireNonNull(o);
        if (scheduled != o.scheduled) {
            return scheduled ? 1 : -1;
        }
        var thisAccountIdIsNull = (accountId == null);
        var otherAccountIdIsNull = (o.accountId == null);
        if (thisAccountIdIsNull != otherAccountIdIsNull) {
            return thisAccountIdIsNull ? -1 : 1;
        }
        if (!thisAccountIdIsNull) {
            int accountIdComparison = accountId.compareTo(o.accountId);
            if (accountIdComparison != 0) {
                return accountIdComparison;
            }
        }
        var thisStartIsNull = (validStart == null);
        var otherStartIsNull = (o.validStart == null);
        if (thisStartIsNull != otherStartIsNull) {
            return thisAccountIdIsNull ? -1 : 1;
        }
        if (!thisStartIsNull) {
            return validStart.compareTo(o.validStart);
        }
        return 0;
    }
}

}