// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// Result of invoking a contract via {@link ContractCallQuery}, or {@link ContractExecuteTransaction}, or the result of
    /// a contract constructor being called by {@link ContractCreateTransaction}.
    /// <p>
    /// If you require a type which is not supported here, please let us know on
    /// <a href="https://github.com/hashgraph/hedera-sdk-java/issues/298">this Github issue</a>.
    /// </summary>
    public sealed class ContractFunctionResult
    {
        // Java Source => (Byte are signed) private static readonly ByteString errorPrefix = ByteString.CopyFrom(new byte[] { 8, -61, 121, -96 });
        private static readonly ByteString errorPrefix =
            ByteString.CopyFrom(
            [
                0x08,
                0xC3, // -61 in Java
                0x79,
                0xA0  // -96 in Java
            ]);

        /// <summary>
        /// The ID of the contract that was invoked.
        /// </summary>
        public readonly ContractId contractId;
        /// <summary>
        /// The contract's 20-byte EVM address
        /// </summary>
        public readonly ContractId evmAddress;
        /// <summary>
        /// message in case there was an error during smart contract execution
        /// </summary>
        public readonly string? errorMessage;
        /// <summary>
        /// bloom filter for record
        /// </summary>
        public readonly ByteString bloom;
        /// <summary>
        /// units of gas used to execute contract
        /// </summary>
        public readonly ulong GasUsed;
        /// <summary>
        /// the log info for events returned by the function
        /// </summary>
        public readonly IList<ContractLogInfo> logs;
        /// <summary>
        /// The created ids will now _also_ be externalized through internal transaction records, where each record has its
        /// alias field populated with the new contract's EVM address. (This is needed for contracts created with CREATE2,
        /// since there is no longer a simple relationship between the new contract's 0.0.X id and its Solidity address.)
        /// </summary>
        public readonly IList<ContractId> createdContractIds;
        /// <summary>
        /// </summary>
        /// <remarks>@deprecated- Use mirror node for contract traceability instead</remarks>
        public readonly IList<ContractStateChange> stateChanges;
        /// <summary>
        /// The amount of gas available for the call, aka the gasLimit
        /// </summary>
        public readonly long gas;
        /// <summary>
        /// Number of tinybars sent (the function must be payable if this is nonzero).
        /// </summary>
        public readonly Hbar hbarAmount;
        /// <summary>
        /// The parameters passed into the contract call.
        /// <br>
        /// This field should only be populated when the paired TransactionBody in the record stream is not a
        /// ContractCreateTransactionBody or a ContractCallTransactionBody.
        /// </summary>
        public readonly byte[] contractFunctionParametersBytes;
        /// <summary>
        /// The account that is the "sender." If not present it is the accountId from the transactionId.
        /// </summary>
        public readonly AccountId senderAccountId;
        /// <summary>
        /// A list of updated contract account nonces containing the new nonce value for each contract account. This is
        /// always empty in a ContractCallLocalResponse#ContractFunctionResult message, since no internal creations can
        /// happen in a static EVM call.
        /// </summary>
        public readonly IList<ContractNonceInfo> ContractNonces;
        /// <summary>
        /// If not null this field specifies what the value of the signer account nonce is post transaction execution.
        /// For transactions that don't update the signer nonce (like HAPI ContractCall and ContractCreate transactions) this field should be null.
        /// </summary>
        public readonly long SignerNonce;
        private readonly ByteString RawResult;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inner">the protobuf</param>
        internal ContractFunctionResult(Proto.ContractFunctionResult inner)
        {
            contractId = ContractId.FromProtobuf(inner.ContractID);
            evmAddress = new ContractId(contractId.Shard, contractId.Realm, inner.EvmAddress.ToByteArray());
            string errMsg = inner.ErrorMessage;
            errorMessage = errMsg.Length > 0 ? errMsg : null;
            ByteString callResult = inner.ContractCallResult;

            // if an exception was thrown, the call result is encoded like the params
            // for a function `Error(string)`
            // https://solidity.readthedocs.io/en/v0.6.2/control-structures.html#revert
            if (errorMessage != null && callResult.StartsWith(errorPrefix))
            {
                // trim off the function selector bytes
                RawResult = callResult.Substring(4);
            }
            else RawResult = callResult;

			bloom = inner.Bloom;
            GasUsed = inner.GasUsed;
            logs = inner.LogInfo.Select(_ => ContractLogInfo.FromProtobuf(_)).ToList();
            createdContractIds = inner.CreatedContractIDs.Select(_ => ContractId.FromProtobuf(_)).ToList();
            stateChanges = [];

            // for (var stateChangeProto : inner.getStateChangesList()) {
            //     stateChanges.add(ContractStateChange.fromProtobuf(stateChangeProto));
            // }
            gas = inner.Gas;
            hbarAmount = Hbar.FromTinybars(inner.Amount);
            contractFunctionParametersBytes = inner.FunctionParameters.ToByteArray();
            senderAccountId = AccountId.FromProtobuf(inner.SenderId);
            ContractNonces = inner.ContractNonces.Select(_ => ContractNonceInfo.FromProtobuf(_)).ToList();
            SignerNonce = inner.SignerNonce.Value;
        }

        /// <summary>
        /// Get the whole raw function result.
        /// </summary>
        /// <returns>byte[]</returns>
        public byte[] AsBytes()
        {
            return RawResult.ToByteArray();
        }

        /// <summary>
        /// Get the nth returned value as a string
        /// </summary>
        /// <param name="valIndex">The index of the string to be retrieved</param>
        /// <returns>String</returns>
        public string GetString(int valIndex)
        {
            return GetDynamicBytes(valIndex).ToStringUtf8();
        }

        /// <summary>
        /// Get the nth returned value as a list of strings
        /// </summary>
        /// <param name="index">The index of the list of strings to be retrieved</param>
        /// <returns>A List of Strings</returns>
        public IList<string> GetStringArray(int index)
        {
            var offset = GetInt32(index);
            var count = GetIntValueAt(offset);
            var strings = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var strOffset = GetIntValueAt(offset + 32 + (i * 32));
                var len = GetIntValueAt(offset + strOffset + 32);
                var str = GetByteString(offset + strOffset + 32 + 32, offset + strOffset + 32 + 32 + len).ToStringUtf8();
                strings.Add(str);
            }

            return strings;
        }

        /// <summary>
        /// Get the nth value in the result as a dynamic byte array.
        /// </summary>
        /// <param name="valIndex">The index of the array of bytes to be retrieved</param>
        /// <returns>byte[]</returns>
        public byte[] GetBytes(int valIndex)
        {
            return GetDynamicBytes(valIndex).ToByteArray();
        }

        /// <summary>
        /// Get the nth fixed-width 32-byte value in the result.
        /// <p>
        /// This is the native word size for the Solidity ABI.
        /// </summary>
        /// <param name="valIndex">The index of the array of bytes to be retrieved</param>
        /// <returns>byte[]</returns>
        public byte[] GetBytes32(int valIndex)
        {
            return GetByteString(valIndex * 32, (valIndex + 1) * 32).ToByteArray();
        }

        private ByteString GetDynamicBytes(int valIndex)
        {
            int offset = GetInt32(valIndex);
            int len = GetIntValueAt(offset);
            return GetByteString(offset + 32, offset + 32 + len);
        }

        /// <summary>
        /// Get the nth value as a bool.
        /// </summary>
        /// <param name="valIndex">The index of the bool to be retrieved</param>
        /// <returns>bool</returns>
        public bool GetBool(int valIndex)
        {
            return GetInt8(valIndex) != 0;
        }

        /// <summary>
        /// Get the nth returned value as an 8-bit integer.
        /// <p>
        /// If the actual value is wider it will be truncated to the last byte (similar to Java's integer narrowing
        /// semantics).
        /// <p>
        /// If you are developing a contract and intending to return more than one of these values from a Solidity function,
        /// consider using the {@code bytes32} Solidity type instead as that will be a more compact representation which will
        /// save on gas. (Each individual {@code int8} value is padded to 32 bytes in the ABI.)
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>byte</returns>
        public byte GetInt8(int valIndex)
        {
            return GetByteBuffer(valIndex * 32 + 31).Get();
        }

        /// <summary>
        /// Get the nth returned value as a 32-bit integer.
        /// <p>
        /// If the actual value is wider it will be truncated to the last 4 bytes (similar to Java's integer narrowing
        /// semantics).
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>int</returns>
        public int GetInt32(int valIndex)
        {

            // int will be the last 4 bytes in the "value"
            return GetIntValueAt(valIndex * 32);
        }

        /// <summary>
        /// Get the nth returned value as a 64-bit integer.
        /// <p>
        /// If the actual value is wider it will be truncated to the last 8 bytes (similar to Java's integer narrowing
        /// semantics).
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>long</returns>
        public long GetInt64(int valIndex)
        {
            return GetByteBuffer(valIndex * 32 + 24).GetLong();
		}

        /// <summary>
        /// Get the nth returned value as a 256-bit integer.
        /// <p>
        /// This type can represent the full width of Solidity integers.
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>BigInteger</returns>
        public BigInteger GetInt256(int valIndex)
        {
            return new BigInteger(GetBytes32(valIndex));
        }

        /// <summary>
        /// Get the nth returned value as a 8-bit unsigned integer.
        /// <p>
        /// If the actual value is wider it will be truncated to the last byte (similar to Java's integer narrowing
        /// semantics).
        /// <p>
        /// Because Java does not have native unsigned integers, this is semantically identical to {@link #getInt8(int)}. To
        /// treat the value as unsigned in the range {@code [0, 255]}, use {@link Byte#toUnsignedInt(byte)} to widen to
        /// {@code int} without sign-extension.
        /// <p>
        /// If you are developing a contract and intending to return more than one of these values from a Solidity function,
        /// consider using the {@code bytes32} Solidity type instead as that will be a more compact representation which will
        /// save on gas. (Each individual {@code uint8} value is padded to 32 bytes in the ABI.)
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>byte</returns>
        public byte GetUint8(int valIndex)
        {
            return GetInt8(valIndex);
        }

        /// <summary>
        /// Get the nth returned value as a 32-bit unsigned integer.
        /// <p>
        /// If the actual value is wider it will be truncated to the last 4 bytes (similar to Java's integer narrowing
        /// semantics).
        /// <p>
        /// Because Java does not have native unsigned integers, this is semantically identical to {@link #getInt32(int)}.
        /// The {@link Integer} class has static methods for treating an {@code int} as unsigned where the difference between
        /// signed and unsigned actually matters (comparison, division, printing and widening to {@code long}).
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>int</returns>
        public int GetUint32(int valIndex)
        {
            return GetInt32(valIndex);
        }

        /// <summary>
        /// Get the nth returned value as a 64-bit integer.
        /// <p>
        /// If the actual value is wider it will be truncated to the last 8 bytes (similar to Java's integer narrowing
        /// semantics).
        /// <p>
        /// Because Java does not have native unsigned integers, this is semantically identical to {@link #getInt64(int)}.
        /// The {@link Long} class has static methods for treating a {@code long} as unsigned where the difference between
        /// signed and unsigned actually matters (comparison, division and printing).
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>long</returns>
        public long GetUint64(int valIndex)
        {
            return GetInt64(valIndex);
        }

        /// <summary>
        /// Get the nth returned value as a 256-bit unsigned integer.
        /// <p>
        /// The value will be padded with a leading zero-byte so that {@link BigInteger#BigInteger(byte[])} treats the value
        /// as positive regardless of whether the most significant bit is set or not.
        /// <p>
        /// This type can represent the full width of Solidity integers.
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>BigInteger</returns>
        public BigInteger GetUint256(int valIndex)
        {

            // prepend a zero byte so that `BigInteger` finds a zero sign bit and treats it as positive
            // `ByteString -> byte[]` requires copying anyway so we can amortize these two operations
            byte[] bytes = new byte[33];
            GetByteString(valIndex * 32, (valIndex + 1) * 32).CopyTo(bytes, 1);

            // there's a constructor that takes a signum but we would need to scan the array
            // to check that it's nonzero; this constructor does that work for us but requires
            // prepending a sign bit
            return new BigInteger(bytes);
        }

        /// <summary>
        /// Get the nth returned value as a Solidity address.
        /// </summary>
        /// <param name="valIndex">The index of the value to be retrieved</param>
        /// <returns>String</returns>
        public string GetAddress(int valIndex)
        {
            int offset = valIndex * 32;

            // address is a uint160
            return Hex.ToHexString(GetByteString(offset + 12, offset + 32).ToByteArray());
        }

        private int GetIntValueAt(int valueOffset)
        {
            return GetByteBuffer(valueOffset + 28).GetInt();
        }

		private ReadOnlyMemory<byte> GetByteBuffer(int offset)
		{
			/*
             private ByteBuffer GetByteBuffer(int offset)
            {
                // **NB** `.asReadOnlyByteBuffer()` on a substring returns a `ByteBuffer` with the
                // offset set as `position()`, so be sure to advance the buffer relative to that
                ByteBuffer byteBuffer = rawResult.AsReadOnlyByteBuffer();
                byteBuffer.Position(byteBuffer.Position() + offset);
                return byteBuffer;
            }
             */
			return RawResult.Memory[offset..];
		}


		private ByteString GetByteString(int startIndex, int endIndex)
        {
            return ByteString.CopyFrom(RawResult.ToByteArray(), startIndex, endIndex);
        }

        /// <summary>
        /// Create the protobuf representation.
        /// </summary>
        /// <returns>{@link Proto.ContractFunctionResult}</returns>
        public Proto.ContractFunctionResult ToProtobuf()
        {
			Proto.ContractFunctionResult proto = new()
            {
				ContractID = contractId.ToProtobuf(),
				ContractCallResult = RawResult,
				Bloom = bloom,
				GasUsed = GasUsed,
				SignerNonce = SignerNonce,
			};

            if (evmAddress != null)
				proto.EvmAddress = ByteString.CopyFrom(evmAddress.EVMAddress); // BytesValue.Parser.Par SetValue(.Build());

			if (errorMessage != null)
				proto.ErrorMessage = errorMessage;

			foreach (ContractLogInfo log in logs)
				proto.LogInfo.Add(log.ToProtobuf());

			foreach (var contractId in createdContractIds)
				proto.CreatedContractIDs.Add(contractId.ToProtobuf());

            if (senderAccountId != null)
				proto.SenderId = senderAccountId.ToProtobuf();


            // for (var stateChange : stateChanges) {
            //     contractFunctionResult.addStateChanges(stateChange.toProtobuf());
            // }
            foreach (var contractNonce in ContractNonces)
                proto.ContractNonces.Add(contractNonce.ToProtobuf());

            return proto;
        }
    }
}