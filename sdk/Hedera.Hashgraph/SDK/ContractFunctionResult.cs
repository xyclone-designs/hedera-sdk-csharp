using System.Diagnostics.Contracts;
using System.IO.Pipelines;
using System.Numerics;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Result of invoking a contract via {@link ContractCallQuery}, or {@link ContractExecuteTransaction}, or the result of
 * a contract constructor being called by {@link ContractCreateTransaction}.
 * <p>
 * If you require a type which is not supported here, please let us know on
 * <a href="https://github.com/hashgraph/hedera-sdk-java/issues/298">this Github issue</a>.
 */
	public sealed class ContractFunctionResult
	{
		private static readonly ByteString errorPrefix = ByteString.copyFrom(new byte[] {8, -61, 121, -96});

    /**
     * The ID of the contract that was invoked.
     */
    public readonly ContractId contractId;

    /**
     * The contract's 20-byte EVM address
     */
    @Nullable
	public readonly ContractId evmAddress;

    /**
     * message in case there was an error during smart contract execution
     */
    @Nullable
	public readonly string errorMessage;

    /**
     * bloom filter for record
     */
    public readonly ByteString bloom;

    /**
     * units of gas used to execute contract
     */
    public readonly long gasUsed;

	/**
     * the log info for events returned by the function
     */
	public readonly List<ContractLogInfo> logs;

	/**
     * The created ids will now _also_ be externalized through internal transaction records, where each record has its
     * alias field populated with the new contract's EVM address. (This is needed for contracts created with CREATE2,
     * since there is no longer a simple relationship between the new contract's 0.0.X id and its Solidity address.)
     */
	[Obsolete]
	public readonly List<ContractId> createdContractIds;

	/**
     * @deprecated - Use mirror node for contract traceability instead
     */
	[Obsolete]
	public readonly List<ContractStateChange> stateChanges;

	/**
     * The amount of gas available for the call, aka the gasLimit
     */
	public readonly long gas;

	/**
     * Number of tinybars sent (the function must be payable if this is nonzero).
     */
	public readonly Hbar hbarAmount;

    /**
     * The parameters passed into the contract call.
     * <br>
     * This field should only be populated when the paired TransactionBody in the record stream is not a
     * ContractCreateTransactionBody or a ContractCallTransactionBody.
     */
    public readonly byte[] contractFunctionParametersBytes;
	/**
     * The account that is the "sender." If not present it is the accountId from the transactionId.
     */
	@Nullable
	public readonly AccountId senderAccountId;
    /**
     * A list of updated contract account nonces containing the new nonce value for each contract account. This is
     * always empty in a ContractCallLocalResponse#ContractFunctionResult message, since no internal creations can
     * happen in a static EVM call.
     */
    public readonly List<ContractNonceInfo> contractNonces;
	/**
     * If not null this field specifies what the value of the signer account nonce is post transaction execution.
     * For transactions that don't update the signer nonce (like HAPI ContractCall and ContractCreate transactions) this field should be null.
     */
	public readonly long signerNonce;

	private readonly ByteString rawResult;

    /**
     * Constructor.
     *
     * @param inner the protobuf
     */
    ContractFunctionResult(ContractFunctionResultOrBuilder inner)
	{
		contractId = ContractId.FromProtobuf(inner.getContractID());

		evmAddress = inner.hasEvmAddress()
				? new ContractId(
						contractId.Shard,
						contractId.Realm,
						inner.getEvmAddress().getValue().ToByteArray())
				: null;

		string errMsg = inner.getErrorMessage();
		errorMessage = !errMsg.isEmpty() ? errMsg : null;

		ByteString callResult = inner.getContractCallResult();

		// if an exception was thrown, the call result is encoded like the params
		// for a function `Error(string)`
		// https://solidity.readthedocs.io/en/v0.6.2/control-structures.html#revert
		if (errorMessage != null && callResult.startsWith(errorPrefix))
		{
			// trim off the function selector bytes
			rawResult = callResult.substring(4);
		}
		else
		{
			rawResult = callResult;
		}

		bloom = inner.getBloom();

		gasUsed = inner.getGasUsed();

		logs = inner.getLogInfoList().stream()
				.map(ContractLogInfo::FromProtobuf)
				.collect(Collectors.toList());

		createdContractIds = inner.getCreatedContractIDsList().stream()
				.map(ContractId::FromProtobuf)
				.collect(Collectors.toList());

		stateChanges = new ArrayList<ContractStateChange>();
		// for (var stateChangeProto : inner.getStateChangesList()) {
		//     stateChanges.Add(ContractStateChange.FromProtobuf(stateChangeProto));
		// }

		gas = inner.getGas();

		hbarAmount = Hbar.FromTinybars(inner.getAmount());

		contractFunctionParametersBytes = inner.getFunctionParameters().ToByteArray();

		senderAccountId = inner.hasSenderId() ? AccountId.FromProtobuf(inner.getSenderId()) : null;

		contractNonces = inner.getContractNoncesList().stream()
				.map(ContractNonceInfo::FromProtobuf)
				.collect(Collectors.toList());

		signerNonce = inner.getSignerNonce().getValue();
	}

	/**
     * Get the whole raw function result.
     *
     * @return byte[]
     */
	public byte[] asBytes()
	{
		return rawResult.ToByteArray();
	}

	/**
     * Get the nth returned value as a string
     *
     * @param valIndex The index of the string to be retrieved
     * @return string
     */
	public string getString(int valIndex)
	{
		return getDynamicBytes(valIndex).toStringUtf8();
	}

	/**
     * Get the nth returned value as a list of strings
     *
     * @param index The index of the list of strings to be retrieved
     * @return A List of Strings
     */
	public List<string> getStringArray(int index)
	{
		var offset = getInt32(index);
		var count = getIntValueAt(offset);
		var strings = new ArrayList<string>();

		for (int i = 0; i < count; i++)
		{
			var strOffset = getIntValueAt(offset + 32 + (i * 32));
			var len = getIntValueAt(offset + strOffset + 32);
			var str = getByteString(offset + strOffset + 32 + 32, offset + strOffset + 32 + 32 + len)
					.toStringUtf8();
			strings.Add(str);
		}

		return strings;
	}

	/**
     * Get the nth value in the result as a dynamic byte array.
     *
     * @param valIndex The index of the array of bytes to be retrieved
     * @return byte[]
     */
	public byte[] getBytes(int valIndex)
	{
		return getDynamicBytes(valIndex).ToByteArray();
	}

	/**
     * Get the nth fixed-width 32-byte value in the result.
     * <p>
     * This is the native word size for the Solidity ABI.
     *
     * @param valIndex The index of the array of bytes to be retrieved
     * @return byte[]
     */
	public byte[] getBytes32(int valIndex)
	{
		return getByteString(valIndex * 32, (valIndex + 1) * 32).ToByteArray();
	}

	private ByteString getDynamicBytes(int valIndex)
	{
		int offset = getInt32(valIndex);
		int len = getIntValueAt(offset);
		return getByteString(offset + 32, offset + 32 + len);
	}

	/**
     * Get the nth value as a bool.
     *
     * @param valIndex The index of the bool to be retrieved
     * @return bool
     */
	public bool getBool(int valIndex)
	{
		return getInt8(valIndex) != 0;
	}

	/**
     * Get the nth returned value as an 8-bit integer.
     * <p>
     * If the actual value is wider it will be truncated to the last byte (similar to Java's integer narrowing
     * semantics).
     * <p>
     * If you are developing a contract and intending to return more than one of these values from a Solidity function,
     * consider using the {@code bytes32} Solidity type instead as that will be a more compact representation which will
     * save on gas. (Each individual {@code int8} value is padded to 32 bytes in the ABI.)
     *
     * @param valIndex The index of the value to be retrieved
     * @return byte
     */
	public byte getInt8(int valIndex)
	{
		return getByteBuffer(valIndex * 32 + 31).get();
	}

	/**
     * Get the nth returned value as a 32-bit integer.
     * <p>
     * If the actual value is wider it will be truncated to the last 4 bytes (similar to Java's integer narrowing
     * semantics).
     *
     * @param valIndex The index of the value to be retrieved
     * @return int
     */
	public int getInt32(int valIndex)
	{
		// int will be the last 4 bytes in the "value"
		return getIntValueAt(valIndex * 32);
	}

	/**
     * Get the nth returned value as a 64-bit integer.
     * <p>
     * If the actual value is wider it will be truncated to the last 8 bytes (similar to Java's integer narrowing
     * semantics).
     *
     * @param valIndex The index of the value to be retrieved
     * @return long
     */
	public long getInt64(int valIndex)
	{
		return getByteBuffer(valIndex * 32 + 24).getLong();
	}

	/**
     * Get the nth returned value as a 256-bit integer.
     * <p>
     * This type can represent the full width of Solidity integers.
     *
     * @param valIndex The index of the value to be retrieved
     * @return BigInteger
     */
	public BigInteger getInt256(int valIndex)
	{
		return new BigInteger(getBytes32(valIndex));
	}

	/**
     * Get the nth returned value as a 8-bit unsigned integer.
     * <p>
     * If the actual value is wider it will be truncated to the last byte (similar to Java's integer narrowing
     * semantics).
     * <p>
     * Because Java does not have native unsigned integers, this is semantically identical to {@link #getInt8(int)}. To
     * treat the value as unsigned in the range {@code [0, 255]}, use {@link Byte#toUnsignedInt(byte)} to widen to
     * {@code int} without sign-extension.
     * <p>
     * If you are developing a contract and intending to return more than one of these values from a Solidity function,
     * consider using the {@code bytes32} Solidity type instead as that will be a more compact representation which will
     * save on gas. (Each individual {@code uint8} value is padded to 32 bytes in the ABI.)
     *
     * @param valIndex The index of the value to be retrieved
     * @return byte
     */
	public byte getUint8(int valIndex)
	{
		return getInt8(valIndex);
	}

	/**
     * Get the nth returned value as a 32-bit unsigned integer.
     * <p>
     * If the actual value is wider it will be truncated to the last 4 bytes (similar to Java's integer narrowing
     * semantics).
     * <p>
     * Because Java does not have native unsigned integers, this is semantically identical to {@link #getInt32(int)}.
     * The {@link Integer} class has static methods for treating an {@code int} as unsigned where the difference between
     * signed and unsigned actually matters (comparison, division, printing and widening to {@code long}).
     *
     * @param valIndex The index of the value to be retrieved
     * @return int
     */
	public int getUint32(int valIndex)
	{
		return getInt32(valIndex);
	}

	/**
     * Get the nth returned value as a 64-bit integer.
     * <p>
     * If the actual value is wider it will be truncated to the last 8 bytes (similar to Java's integer narrowing
     * semantics).
     * <p>
     * Because Java does not have native unsigned integers, this is semantically identical to {@link #getInt64(int)}.
     * The {@link long} class has static methods for treating a {@code long} as unsigned where the difference between
     * signed and unsigned actually matters (comparison, division and printing).
     *
     * @param valIndex The index of the value to be retrieved
     * @return long
     */
	public long getUint64(int valIndex)
	{
		return getInt64(valIndex);
	}

	/**
     * Get the nth returned value as a 256-bit unsigned integer.
     * <p>
     * The value will be padded with a leading zero-byte so that {@link BigInteger#BigInteger(byte[])} treats the value
     * as positive regardless of whether the most significant bit is set or not.
     * <p>
     * This type can represent the full width of Solidity integers.
     *
     * @param valIndex The index of the value to be retrieved
     * @return BigInteger
     */
	public BigInteger getUint256(int valIndex)
	{
		// prepend a zero byte so that `BigInteger` finds a zero sign bit and treats it as positive
		// `ByteString -> byte[]` requires copying anyway so we can amortize these two operations
		byte[] bytes = new byte[33];
		getByteString(valIndex * 32, (valIndex + 1) * 32).copyTo(bytes, 1);

		// there's a constructor that takes a signum but we would need to scan the array
		// to check that it's nonzero; this constructor does that work for us but requires
		// prepending a sign bit
		return new BigInteger(bytes);
	}

	/**
     * Get the nth returned value as a Solidity address.
     *
     * @param valIndex The index of the value to be retrieved
     * @return string
     */
	public string getAddress(int valIndex)
	{
		int offset = valIndex * 32;
		// address is a uint160
		return Hex.toHexString(getByteString(offset + 12, offset + 32).ToByteArray());
	}

	private int getIntValueAt(int valueOffset)
	{
		return getByteBuffer(valueOffset + 28).getInt();
	}

	private ByteBuffer getByteBuffer(int offset)
	{
		// **NB** `.asReadOnlyByteBuffer()` on a substring returns a `ByteBuffer` with the
		// offset set as `position()`, so be sure to advance the buffer relative to that
		ByteBuffer byteBuffer = rawResult.asReadOnlyByteBuffer();
		byteBuffer.position(byteBuffer.position() + offset);

		return byteBuffer;
	}

	private ByteString getByteString(int startIndex, int endIndex)
	{
		return rawResult.substring(startIndex, endIndex);
	}

	/**
     * Create the protobuf representation.
     *
     * @return {@link Proto.ContractFunctionResult}
     */
	Proto.ContractFunctionResult ToProtobuf()
	{
		var contractFunctionResult = Proto.ContractFunctionResult.newBuilder()
				.setContractID(contractId.ToProtobuf())
				.setContractCallResult(rawResult)
				.setBloom(bloom)
				.setGasUsed(gasUsed)
				.setSignerNonce(Int64Value.of(this.signerNonce));

		if (evmAddress != null)
		{
			contractFunctionResult.setEvmAddress(BytesValue.newBuilder()
					.setValue(ByteString.copyFrom(Objects.requireNonNull(evmAddress.evmAddress)))
					.build());
		}

		if (errorMessage != null)
		{
			contractFunctionResult.setErrorMessage(errorMessage);
		}

		for (ContractLogInfo log : logs)
		{
			contractFunctionResult.AddLogInfo(log.ToProtobuf());
		}

		for (var contractId : createdContractIds)
		{
			contractFunctionResult.AddCreatedContractIDs(contractId.ToProtobuf());
		}

		if (senderAccountId != null)
		{
			contractFunctionResult.setSenderId(senderAccountId.ToProtobuf());
		}

		// for (var stateChange : stateChanges) {
		//     contractFunctionResult.AddStateChanges(stateChange.ToProtobuf());
		// }

		for (var contractNonce : contractNonces)
		{
			contractFunctionResult.AddContractNonces(contractNonce.ToProtobuf());
		}

		return contractFunctionResult.build();
	}

	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("contractId", contractId)
				.Add("evmAddress", evmAddress)
				.Add("errorMessage", errorMessage)
				.Add("bloom", Hex.toHexString(bloom.ToByteArray()))
				.Add("gasUsed", gasUsed)
				.Add("logs", logs)
				.Add("createdContractIds", createdContractIds)
				.Add("stateChanges", stateChanges)
				.Add("gas", gas)
				.Add("hbarAmount", hbarAmount)
				.Add("contractFunctionparametersBytes", Hex.toHexString(contractFunctionParametersBytes))
				.Add("rawResult", Hex.toHexString(rawResult.ToByteArray()))
				.Add("senderAccountId", senderAccountId)
				.Add("contractNonces", contractNonces)
				.Add("signerNonce", signerNonce)
				.toString();
	}

	public Tuple getResult(string types)
	{
		TupleType<Tuple> tupleType = TupleType.parse(types);
		return tupleType.decode(rawResult.ToByteArray());
	}
}

}