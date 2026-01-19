using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/**
     * The client-generated ID for a transaction.
     *
     * <p>This is used for retrieving receipts and records for a transaction, for appending to a file
     * right after creating it, for instantiating a smart contract with bytecode in a file just created,
     * and internally by the network for detecting when duplicate transactions are submitted.
     */
    public sealed class TransactionId : IComparable<TransactionId> 
    {
		private static readonly long NANOSECONDS_PER_MILLISECOND = 1_000_000L;
		private static readonly long TIMESTAMP_INCREMENT_NANOSECONDS = 1_000L;
		private static readonly long NANOSECONDS_TO_REMOVE = 10000000000L;
		private static readonly AtomicLong monotonicTime = new AtomicLong();

		public int? Nonce { get; set; }
		public bool Scheduled { get; set; }
		/**
         * The Account ID that paid for this transaction.
         */
		public AccountId? AccountId { get; }
        /**
         * The time from when this transaction is valid.
         *
         * <p>When a transaction is submitted there is additionally a validDuration (defaults to 120s)
         * and together they define a time window that a transaction may be processed in.
         */
        public DateTimeOffset? ValidStart { get; }

        /**
         * No longer part of the public API. Use `Transaction.withValidStart()` instead.
         *
         * @param accountId     the account id
         * @param validStart    the valid start time
         */
        public TransactionId(AccountId? accountId, DateTimeOffset? validStart) 
        {
            AccountId = accountId;
            ValidStart = validStart;
            Scheduled = false;
        }

        /**
         * Create a transaction id.
         *
         * @param accountId                 the account id
         * @param validStart                the valid start time
         * @return                          the new transaction id
         */
        public static TransactionId WithValidStart(AccountId accountId, DateTimeOffset validStart) 
        {
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
        public static TransactionId Generate(AccountId accountId) {
            long currentTime;
            long lastTime;

            // Loop to ensure the generated timestamp is strictly increasing,
            // and it handles the case where the system clock appears to move backward
            // or if multiple threads attempt to generate a timestamp concurrently.
            do {
                // Get the current time in nanoseconds and remove a few seconds to allow for some time drift
                // between the client and the receiving node and prevented spurious INVALID_TRANSACTION_START.
                currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * NANOSECONDS_PER_MILLISECOND - NANOSECONDS_TO_REMOVE;

                // Get the last recorded timestamp.
                lastTime = monotonicTime.get();

                // If the current time is less than or equal to the last recorded time,
                // adjust the timestamp to ensure it is strictly increasing.
                if (currentTime <= lastTime) {
                    currentTime = lastTime + TIMESTAMP_INCREMENT_NANOSECONDS;
                }
            } while (!monotonicTime.compareAndSet(lastTime, currentTime));

			return new TransactionId(
				accountId,
				DateTimeOffset.FromUnixTimeMilliseconds(currentTime + Random.Shared.NextInt64(1_000)));

		}

		/**
         * Create a transaction id from a protobuf.
         *
         * @param transactionID             the protobuf
         * @return                          the new transaction id
         */
		public static TransactionId FromProtobuf(Proto.TransactionID transactionID) {
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
        public static TransactionId FromString(string s) {
            var parts = s.Split("/", 2);
            var nonce = parts.Length == 2 ? int.Parse(parts[1]) : null;

            parts = parts[0].Split("\\?", 2);
            parts = parts[0].Split("@", 2);

            if (parts.Length != 2) throw new ArgumentException("expecting {account}@{seconds}.{nanos}[?scheduled][/nonce]");

			AccountId accountId = AccountId.FromString(parts[0]);

            var validStartParts = parts[1].Split("\\.", 2);

            if (validStartParts.Length != 2) throw new ArgumentException("expecting {account}@{seconds}.{nanos}");

            DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(validStartParts[0]), long.Parse(validStartParts[1]));

            return new TransactionId(accountId, validStart) 
            {
				Nonce = nonce,
				Scheduled = parts.Length == 2 && parts[1].Equals("scheduled"),
			};
        }
        /**
         * Create a new transaction id from a byte array.
         *
         * @param bytes                     the byte array
         * @return                          the new transaction id
         * @       when there is an issue with the protobuf
         */
        public static TransactionId FromBytes(byte[] bytes) 
        {
            return FromProtobuf(Proto.TransactionID.Parser.ParseFrom(bytes));
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
        {
            return getReceipt(client, client.GetRequestTimeout());
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
        {
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
        public void getReceiptAsync(Client client, Action<TransactionReceipt, Exception> callback) {
            ConsumerHelper.biConsumer(getReceiptAsync(client), callback);
        }

        /**
         * Fetch the receipt of the transaction asynchronously.
         *
         * @param client                    The client with which this will be executed.
         * @param timeout The timeout after which the execution attempt will be cancelled.
         * @param callback a BiConsumer which handles the result or error.
         */
        public void getReceiptAsync(Client client, Duration timeout, Action<TransactionReceipt, Exception> callback) {
            ConsumerHelper.biConsumer(getReceiptAsync(client, timeout), callback);
        }

        /**
         * Fetch the receipt of the transaction asynchronously.
         *
         * @param client                    The client with which this will be executed.
         * @param onSuccess a Consumer which consumes the result on success.
         * @param onFailure a Consumer which consumes the error on failure.
         */
        public void getReceiptAsync(Client client, Action<TransactionReceipt> onSuccess, Action<Exception> onFailure) {
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
                Client client, Duration timeout, Action<TransactionReceipt> onSuccess, Action<Exception> onFailure) {
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
        {
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
        {
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
        public void getRecordAsync(Client client, Action<TransactionRecord, Exception> callback) {
            ConsumerHelper.biConsumer(getRecordAsync(client), callback);
        }

        /**
         * Fetch the record of the transaction asynchronously.
         *
         * @param client                    The client with which this will be executed.
         * @param timeout The timeout after which the execution attempt will be cancelled.
         * @param callback a BiConsumer which handles the result or error.
         */
        public void getRecordAsync(Client client, Duration timeout, Action<TransactionRecord, Exception> callback) {
            ConsumerHelper.biConsumer(getRecordAsync(client, timeout), callback);
        }

        /**
         * Fetch the record of the transaction asynchronously.
         *
         * @param client                    The client with which this will be executed.
         * @param onSuccess a Consumer which consumes the result on success.
         * @param onFailure a Consumer which consumes the error on failure.
         */
        public void getRecordAsync(Client client, Action<TransactionRecord> onSuccess, Action<Exception> onFailure) {
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
                Client client, Duration timeout, Action<TransactionRecord> onSuccess, Action<Exception> onFailure) {
            ConsumerHelper.twoConsumers(getRecordAsync(client, timeout), onSuccess, onFailure);
        }

        /**
         * Extract the transaction id protobuf.
         *
         * @return                          the protobuf representation
         */
        public Proto.TransactionID ToProtobuf() 
        {
            var id = TransactionID.newBuilder().setScheduled(scheduled).setNonce((nonce != null) ? nonce : 0);

            if (accountId != null) {
                id.setAccountID(accountId.ToProtobuf());
            }

            if (validStart != null) {
                id.setTransactionValidStart(DateTimeOffsetConverter.ToProtobuf(validStart));
            }

            return id.build();
        }

        private string toStringPostfix() 
        {
            Objects.requireNonNull(validStart);
            return "@" + validStart.getEpochSecond() + "." + string.format("%09d", validStart.getNano())
                    + (scheduled ? "?scheduled" : "") + ((nonce != null) ? "/" + nonce : "");
        }

        public override string ToString() 
        {
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

        public override bool Equals(Object object) {
            if (!(object is TransactionId)) {
                return false;
            }

            var id = (TransactionId) object;

            if (accountId != null && validStart != null && id.accountId != null && id.validStart != null) {
                return id.accountId.equals(accountId) && id.validStart.equals(validStart) && scheduled == id.scheduled;
            } else {
                return false;
            }
        }

        public override int HashCode() {
            return toString().hashCode();
        }

        public override int CompareTo(TransactionId o) {
            Objects.requireNonNull(o);
            if (Scheduled != o.Scheduled) return Scheduled ? 1 : -1;

			var thisAccountIdIsNull = (accountId == null);
            var otherAccountIdIsNull = (o.accountId == null);
            if (thisAccountIdIsNull != otherAccountIdIsNull) {
                return thisAccountIdIsNull ? -1 : 1;
            }
            if (!thisAccountIdIsNull) {
                int accountIdComparison = accountId.CompareTo(o.accountId);
                if (accountIdComparison != 0) return accountIdComparison;
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