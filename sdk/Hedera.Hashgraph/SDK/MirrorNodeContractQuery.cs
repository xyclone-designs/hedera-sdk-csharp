namespace Hedera.Hashgraph.SDK
{
	/**
     * MirrorNodeContractQuery returns a result from EVM execution such as cost-free execution of read-only smart contract
     * queries, gas estimation, and transient simulation of read-write operations.
     */
    public abstract class MirrorNodeContractQuery<T> where T : MirrorNodeContractQuery<T>
	{
        // The contract we are sending the transaction to
        private ContractId contractId = null;
        private string contractEvmAddress = null;
        // The account we are sending the transaction from
        private AccountId sender = null;
        private string senderEvmAddress = null;
        // The transaction callData
        private byte[] callData;
        // The amount we are sending to the contract
        private long value;
        // The gas limit
        private long gasLimit;
        // The gas price
        private long gasPrice;
        // The block number for the simulation
        private long blockNumber;

        @SuppressWarnings("unchecked")
        protected T self() {
            return (T) this;
        }

        public ContractId getContractId() {
            return this.contractId;
        }

        /**
         * Sets the contract instance to call.
         *
         * @param contractId The ContractId to be set
         * @return {@code this}
         */
        public T setContractId(ContractId contractId) {
            Objects.requireNonNull(contractId);
            this.contractId = contractId;
            return self();
        }

        public string getContractEvmAddress() {
            return this.contractEvmAddress;
        }

        /**
         * Set the 20-byte EVM address of the contract to call.
         *
         * @param contractEvmAddress
         * @return {@code this}
         */
        public T setContractEvmAddress(string contractEvmAddress) {
            Objects.requireNonNull(contractEvmAddress);
            this.contractEvmAddress = contractEvmAddress;
            this.contractId = null;
            return self();
        }

        public AccountId getSender() {
            return this.sender;
        }

        /**
         * Sets the sender of the transaction simulation.
         *
         * @param sender The AccountId to be set
         * @return {@code this}
         */
        public T setSender(AccountId sender) {
            Objects.requireNonNull(sender);
            this.sender = sender;
            return self();
        }

        public string getSenderEvmAddress() {
            return this.senderEvmAddress;
        }

        /**
         * Set the 20-byte EVM address of the sender.
         *
         * @param senderEvmAddress
         * @return {@code this}
         */
        public T setSenderEvmAddress(string senderEvmAddress) {
            Objects.requireNonNull(senderEvmAddress);
            this.senderEvmAddress = senderEvmAddress;
            this.sender = null;
            return self();
        }

        public byte[] getCallData() {
            return this.callData;
        }

        /**
         * Sets the function to call, and the parameters to pass to the function.
         *
         * @param name   The string to be set as the function name
         * @param params The function parameters to be set
         * @return {@code this}
         */
        public T SetFunction(string name, ContractFunctionParameters @params) 
    {
            return SetFunctionParameters(@params.ToBytes(name));
        }

        /**
         * Sets the function name to call.
         * <p>
         * The function will be called with no parameters. Use {@link #setFunction(string, ContractFunctionParameters)} to
         * call a function with parameters.
         *
         * @param name The string to be set as the function name
         * @return {@code this}
         */
        public T setFunction(string name) {
            return setFunction(name, new ContractFunctionParameters());
        }

        /**
         * Sets the function parameters as their raw bytes.
         * <p>
         * Use this instead of {@link #setFunction(string, ContractFunctionParameters)} if you have already pre-encoded a
         * solidity function call.
         *
         * @param functionParameters The function parameters to be set
         * @return {@code this}
         */
        public T setFunctionParameters(ByteString functionParameters) {
            Objects.requireNonNull(functionParameters);
            this.callData = functionParameters.ToByteArray();
            return self();
        }

        public long getValue() {
            return this.value;
        }

        /**
         * Sets the amount of value (in tinybars or wei) to be sent to the contract in the transaction.
         * <p>
         * Use this to specify an amount for a payable function call.
         *
         * @param value the amount of value to send, in tinybars or wei
         * @return {@code this}
         */
        public T setValue(long value) {
            this.value = value;
            return self();
        }

        public long getGasLimit() {
            return this.gasLimit;
        }

        /**
         * Sets the gas limit for the contract call.
         * <p>
         * This specifies the maximum amount of gas that the transaction can consume.
         *
         * @param gasLimit the maximum gas allowed for the transaction
         * @return {@code this}
         */
        public T setGasLimit(long gasLimit) {
            this.gasLimit = gasLimit;
            return self();
        }

        public long getGasPrice() {
            return gasPrice;
        }

        /**
         * Sets the gas price to be used for the contract call.
         * <p>
         * This specifies the price of each unit of gas used in the transaction.
         *
         * @param gasPrice the gas price, in tinybars or wei, for each unit of gas
         * @return {@code this}
         */
        public T setGasPrice(long gasPrice) {
            this.gasPrice = gasPrice;
            return self();
        }

        public long getBlockNumber() {
            return this.blockNumber;
        }

        /**
         * Sets the block number for the simulation of the contract call.
         * <p>
         * The block number determines the context of the contract call simulation within the blockchain.
         *
         * @param blockNumber the block number at which to simulate the contract call
         * @return {@code this}
         */
        public T setBlockNumber(long blockNumber) {
            this.blockNumber = blockNumber;
            return self();
        }

        /**
         * Returns gas estimation for the EVM execution
         *
         * @param client
         * @
         * @
         */
        protected long estimate(Client client) {
            fillEvmAddresses();
            return getEstimateGasFromMirrorNodeAsync(client).get();
        }

        /**
         * Does transient simulation of read-write operations and returns the result in hexadecimal string format. The
         * result can be any solidity type.
         *
         * @param client
         * @
         * @
         */
        protected string call(Client client) {
            fillEvmAddresses();
            var blockNum = this.blockNumber == 0 ? "latest" : string.valueOf(this.blockNumber);
            return getContractCallResultFromMirrorNodeAsync(client, blockNum).get();
        }

        private void fillEvmAddresses() {
            if (this.contractEvmAddress == null) {
                Objects.requireNonNull(this.contractId);
                this.contractEvmAddress = contractId.toEvmAddress();
            }

            if (this.senderEvmAddress == null && this.sender != null) {
                this.senderEvmAddress = sender.toEvmAddress();
            }
        }

        private Task<string> getContractCallResultFromMirrorNodeAsync(Client client, string blockNumber) {
            return executeMirrorNodeRequest(client, blockNumber, false)
                    .thenApply(MirrorNodeContractQuery::parseContractCallResult);
        }

        private Task<long> getEstimateGasFromMirrorNodeAsync(Client client) {
            return executeMirrorNodeRequest(client, "latest", true)
                    .thenApply(MirrorNodeContractQuery::parseHexEstimateToLong);
        }

        private Task<string> executeMirrorNodeRequest(Client client, string blockNumber, bool estimate) {
            string apiEndpoint = "/contracts/call";
            string jsonPayload = createJsonPayload(
                    this.callData,
                    this.senderEvmAddress,
                    this.contractEvmAddress,
                    this.gasLimit,
                    this.gasPrice,
                    this.value,
                    blockNumber,
                    estimate);

            string baseUrl = client.getMirrorRestBaseUrl();

            // For localhost contract calls, override to use port 8545 unless system property overrides
            if (baseUrl.contains("localhost:5551") || baseUrl.contains("127.0.0.1:5551")) {
                string contractPort = System.getProperty("hedera.mirror.contract.port");
                if (contractPort != null && !contractPort.isEmpty()) {
                    baseUrl = baseUrl.replace(":5551", ":" + contractPort);
                } else {
                    baseUrl = baseUrl.replace(":5551", ":8545");
                }
            }

            return performQueryToMirrorNodeAsync(baseUrl, apiEndpoint, jsonPayload).exceptionally(ex -> {
                client.getLogger().error("Error while performing post request to Mirror Node: " + ex.getMessage());
                throw new CompletionException(ex);
            });
        }

        static string createJsonPayload(
                byte[] data,
                string senderAddress,
                string contractAddress,
                long gas,
                long gasPrice,
                long value,
                string blockNumber,
                bool estimate) {
            string hexData = Hex.toHexString(data);

            JsonObject jsonObject = new JsonObject();
            jsonObject.AddProperty("data", hexData);
            jsonObject.AddProperty("to", contractAddress);
            jsonObject.AddProperty("estimate", estimate);
            jsonObject.AddProperty("blockNumber", blockNumber);

            // Conditionally add fields if they are set to non-default values
            if (senderAddress != null && !senderAddress.isEmpty()) {
                jsonObject.AddProperty("from", senderAddress);
            }
            if (gas > 0) {
                jsonObject.AddProperty("gas", gas);
            }
            if (gasPrice > 0) {
                jsonObject.AddProperty("gasPrice", gasPrice);
            }
            if (value > 0) {
                jsonObject.AddProperty("value", value);
            }

            return jsonObject.toString();
        }

        static string parseContractCallResult(string responseBody) {
            JsonObject jsonObject = JsonParser.parseString(responseBody).getAsJsonObject();
            return jsonObject.get("result").getAsString();
        }

        static long parseHexEstimateToLong(string responseBody) {
            return Integer.parseInt(parseContractCallResult(responseBody).substring(2), 16);
        }

        @Override
        public string toString() {
            return "{" + "contractId="
                    + contractId + ", contractEvmAddress='"
                    + contractEvmAddress + '\'' + ", sender="
                    + sender + ", senderEvmAddress='"
                    + senderEvmAddress + '\'' + ", callData="
                    + Arrays.toString(callData) + ", value="
                    + value + ", gasLimit="
                    + gasLimit + ", gasPrice="
                    + gasPrice + ", blockNumber="
                    + blockNumber + '}';
        }
    }

}