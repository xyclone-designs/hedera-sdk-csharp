namespace Hedera.Hashgraph.SDK
{
	/**
     * Builder class for Solidity function selectors.
     */
    public sealed class ContractFunctionSelector 
    {
        @Nullable
        private Keccak.Digest256 digest;

        private bool needsComma = false;

        @Nullable
        private byte[] finished = null;

        /**
         * Start building a selector for a function with a given name.
         *
         * @param funcName The name of the function
         */
        public ContractFunctionSelector(string funcName) {
            digest = new Keccak.Digest256();
            digest.update(funcName.getBytes(US_ASCII));
            digest.update((byte) '(');
        }

        public ContractFunctionSelector AddString() {
            return AddParamType("string");
        }
        public ContractFunctionSelector AddStringArray() {
            return AddParamType("string[]");
        }
        public ContractFunctionSelector AddBytes() {
            return AddParamType("bytes");
        }
        public ContractFunctionSelector AddBytesArray() {
            return AddParamType("bytes[]");
        }
        public ContractFunctionSelector AddBytes32() {
            return AddParamType("bytes32");
        }
        public ContractFunctionSelector AddBytes32Array() {
            return AddParamType("bytes32[]");
        }
        public ContractFunctionSelector AddBool() {
            return AddParamType("bool");
        }
        public ContractFunctionSelector AddInt8() {
            return AddParamType("int8");
        }
        public ContractFunctionSelector AddInt32() {
            return AddParamType("int32");
        }
        public ContractFunctionSelector AddInt64() {
            return AddParamType("int64");
        }
        public ContractFunctionSelector AddInt256() {
            return AddParamType("int256");
        }
        public ContractFunctionSelector AddInt8Array() {
            return AddParamType("int8[]");
        }
        public ContractFunctionSelector AddInt32Array() {
            return AddParamType("int32[]");
        }
        public ContractFunctionSelector AddInt64Array() {
            return AddParamType("int64[]");
        }
        public ContractFunctionSelector AddInt256Array() {
            return AddParamType("int256[]");
        }
        public ContractFunctionSelector AddUint8() {
            return AddParamType("uint8");
        }
        public ContractFunctionSelector AddUint32() {
            return AddParamType("uint32");
        }
        public ContractFunctionSelector AddUint64() {
            return AddParamType("uint64");
        }
        public ContractFunctionSelector AddUint256() {
            return AddParamType("uint256");
        }
        public ContractFunctionSelector AddUint8Array() {
            return AddParamType("uint8[]");
        }
        public ContractFunctionSelector AddUint32Array() {
            return AddParamType("uint32[]");
        }
        public ContractFunctionSelector AddUint64Array() {
            return AddParamType("uint64[]");
        }
        public ContractFunctionSelector AddUint256Array() {
            return AddParamType("uint256[]");
        }
        public ContractFunctionSelector AddAddress() {
            return AddParamType("address");
        }
        public ContractFunctionSelector AddAddressArray() {
            return AddParamType("address[]");
        }
        public ContractFunctionSelector AddFunction() {
            return AddParamType("function");
        }

        /**
         * Add a Solidity type name to this selector;
         * {@see https://solidity.readthedocs.io/en/v0.5.9/types.html}
         *
         * @param typeName the name of the Solidity type for a parameter.
         * @return {@code this}
         * @ if {@link #finish()} has already been called.
         */
        ContractFunctionSelector AddParamType(string typeName) {
            if (finished != null)
				throw new InvalidOperationException("FunctionSelector already finished");

			Objects.requireNonNull(digest);

            if (needsComma) {
                digest.update((byte) ',');
            }

            digest.update(typeName.getBytes(US_ASCII));
            needsComma = true;

            return this;
        }

        /**
         * Complete the function selector after all parameters have been added and get the selector
         * bytes.
         * <p>
         * No more parameters may be added after this method call.
         * <p>
         * However, this can be called multiple times; it will always return the same result.
         *
         * @return the computed selector bytes.
         */
        byte[] Finish() {
            if (finished == null) {
                Objects.requireNonNull(digest);
                digest.update((byte) ')');
                finished = Arrays.copyOf(digest.digest(), 4);
                // release digest state
                digest = null;
            }

            return finished;
        }
    }

}