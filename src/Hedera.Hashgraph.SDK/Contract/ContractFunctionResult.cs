// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Numerics;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="T:ContractFunctionResult"]/*' />
    public sealed class ContractFunctionResult
    {
		private static string[] SplitTopLevelTypes(string input)
		{
			var types = new List<string>();
			int depth = 0;
			int start = 0;

			for (int i = 0; i < input.Length; i++)
				if (input[i] == '(') depth++;
				else if (input[i] == ')') depth--;
				else if (input[i] == ',' && depth == 0)
				{
					types.Add(input[start..i]);
					start = i + 1;
				}

			types.Add(input[start..]);

			return [.. types];
		}
		// Java Source => (Byte are signed) private static readonly ByteString errorPrefix = ByteString.CopyFrom(new byte[] { 8, -61, 121, -96 });
		private static readonly ByteString errorPrefix =
            ByteString.CopyFrom(
            [
                0x08,
                0xC3, // -61 in Java
                0x79,
                0xA0  // -96 in Java
            ]);

		/// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.#ctor(Proto.Services.ContractFunctionResult)"]/*' />
		internal ContractFunctionResult(Proto.Services.ContractFunctionResult inner)
		{
			ContractId = ContractId.FromProtobuf(inner.ContractID);
			EvmAddress = new ContractId(ContractId.Shard, ContractId.Realm, inner.EvmAddress.ToByteArray());
			string errMsg = inner.ErrorMessage;
			ErrorMessage = errMsg.Length > 0 ? errMsg : null;
			ByteString callResult = inner.ContractCallResult;

			// if an exception was thrown, the call result is encoded like the params
			// for a function `Error(string)`
			// https://solidity.readthedocs.io/en/v0.6.2/control-structures.html#revert
			if (ErrorMessage != null && callResult.StartsWith(errorPrefix))
			{
				// trim off the function selector bytes
				RawResult = callResult.Substring(4);
			}
			else RawResult = callResult;

			Bloom = inner.Bloom;
			GasUsed = inner.GasUsed;
			logs = inner.LogInfo.Select(_ => ContractLogInfo.FromProtobuf(_)).ToList();
			CreatedContractIds = inner.CreatedContractIDs.Select(_ => ContractId.FromProtobuf(_)).ToList();
			StateChanges = [];

			// for (var stateChangeProto : inner.getStateChangesList()) {
			//     stateChanges.add(ContractStateChange.fromProtobuf(stateChangeProto));
			// }
			Gas = inner.Gas;
			HbarAmount = Hbar.FromTinybars(inner.Amount);
			ContractFunctionParametersBytes = inner.FunctionParameters.ToByteArray();
			SenderAccountId = AccountId.FromProtobuf(inner.SenderId);
			ContractNonces = inner.ContractNonces.Select(_ => ContractNonceInfo.FromProtobuf(_)).ToList();
			SignerNonce = inner.SignerNonce.Value;
		}

		/// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.ContractId"]/*' />
		public readonly ContractId ContractId;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.EvmAddress"]/*' />
        public readonly ContractId EvmAddress;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.ErrorMessage"]/*' />
        public readonly string? ErrorMessage;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.Bloom"]/*' />
        public readonly ByteString Bloom;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.GasUsed"]/*' />
        public readonly ulong GasUsed;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.logs"]/*' />
        public readonly List<ContractLogInfo> logs;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.CreatedContractIds"]/*' />
        public readonly List<ContractId> CreatedContractIds;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.StateChanges"]/*' />
        public readonly List<ContractStateChange> StateChanges;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.Gas"]/*' />
        public readonly long Gas;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.HbarAmount"]/*' />
        public readonly Hbar HbarAmount;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.ContractFunctionParametersBytes"]/*' />
        public readonly byte[] ContractFunctionParametersBytes;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.SenderAccountId"]/*' />
        public readonly AccountId SenderAccountId;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.ContractNonces"]/*' />
        public readonly List<ContractNonceInfo> ContractNonces;
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="F:ContractFunctionResult.SignerNonce"]/*' />
        public readonly long SignerNonce;
        private readonly ByteString RawResult;

        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.AsBytes"]/*' />
        public byte[] AsBytes()
        {
            return RawResult.ToByteArray();
        }
		/// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetBytes(System.Int32)"]/*' />
		public byte[] GetBytes(int valIndex)
		{
			return GetDynamicBytes(valIndex).ToByteArray();
		}
		/// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetBytes32(System.Int32)"]/*' />
		public byte[] GetBytes32(int valIndex)
		{
			return GetByteString(valIndex * 32, (valIndex + 1) * 32).ToByteArray();
		}
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetBool(System.Int32)"]/*' />
        public bool GetBool(int valIndex)
        {
            return GetInt8(valIndex) != 0;
        }
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetInt8(System.Int32)"]/*' />
        public byte GetInt8(int valIndex)
        {
            return GetByteBuffer(valIndex * 32).Span[31];
		}
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetInt32(System.Int32)"]/*' />
        public int GetInt32(int valIndex)
        {

            // int will be the last 4 bytes in the "value"
            return GetIntValueAt(valIndex * 32);
        }
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetInt64(System.Int32)"]/*' />
        public long GetInt64(int valIndex)
        {
            ReadOnlySpan<byte> bytes = GetByteBuffer(valIndex * 32 + 24).Span;
			
            return BinaryPrimitives.ReadInt64BigEndian(bytes);
		}
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetInt256(System.Int32)"]/*' />
        public BigInteger GetInt256(int valIndex)
        {
            return new BigInteger(GetBytes32(valIndex));
        }
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetUint8(System.Int32)"]/*' />
        public byte GetUint8(int valIndex)
        {
            return GetInt8(valIndex);
        }
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetUint32(System.Int32)"]/*' />
        public uint GetUint32(int valIndex)
        {
            return (uint)GetInt32(valIndex);
        }
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetUint64(System.Int32)"]/*' />
        public ulong GetUint64(int valIndex)
        {
            return (ulong)GetInt64(valIndex);
        }
        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetUint256(System.Int32)"]/*' />
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
		/// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetAddress(System.Int32)"]/*' />
		public string GetAddress(int valIndex)
		{
			int offset = valIndex * 32;

			// address is a uint160
			return Hex.ToHexString(GetByteString(offset + 12, offset + 32).ToByteArray());
		}
		/// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetString(System.Int32)"]/*' />
		public string GetString(int valIndex)
		{
			return GetDynamicBytes(valIndex).ToStringUtf8();
		}
		/// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.GetStringArray(System.Int32)"]/*' />
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

		public object[] GetResult(string types)
		{
			if (string.IsNullOrWhiteSpace(types))
				throw new ArgumentException("Types string cannot be null or empty.", nameof(types));

			// Remove outer parentheses if present
			var trimmed = types.Trim();
			if (trimmed.StartsWith('(') && trimmed.EndsWith(')'))
				trimmed = trimmed[1..^1];

			var typeList = SplitTopLevelTypes(trimmed);

			// Build parameter definitions
			var parameters = typeList
				.Select((t, i) => new Parameter(t.Trim(), i + 1))
				.ToArray();

			var decoder = new ParameterDecoder();

			// rawResult should be byte[]
			var decoded = decoder.DecodeDefaultData(RawResult.ToByteArray(), parameters);

			return [decoded.Select(d => d.Result)];
		}

		private int GetIntValueAt(int valueOffset)
		{
            ReadOnlySpan<byte> bytes = GetByteBuffer(valueOffset + 28).Span;

			return BinaryPrimitives.ReadInt32BigEndian(bytes);
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

		private ByteString GetDynamicBytes(int valIndex)
		{
			int offset = GetInt32(valIndex);
			int len = GetIntValueAt(offset);
			return GetByteString(offset + 32, offset + 32 + len);
		}
		private ByteString GetByteString(int startIndex, int endIndex)
        {
            return ByteString.CopyFrom(RawResult.ToByteArray(), startIndex, endIndex);
        }

        /// <include file="ContractFunctionResult.cs.xml" path='docs/member[@name="M:ContractFunctionResult.ToProtobuf"]/*' />
        public Proto.Services.ContractFunctionResult ToProtobuf()
        {
			Proto.Services.ContractFunctionResult proto = new()
            {
				ContractID = ContractId.ToProtobuf(),
				ContractCallResult = RawResult,
				Bloom = Bloom,
				GasUsed = GasUsed,
				SignerNonce = SignerNonce,
			};

            if (EvmAddress != null)
				Proto.Services.EvmAddress = ByteString.CopyFrom(EvmAddress.EvmAddress); // BytesValue.Parser.Par SetValue(.Build());

			if (ErrorMessage != null)
				Proto.Services.ErrorMessage = ErrorMessage;

			foreach (ContractLogInfo log in logs)
				Proto.Services.LogInfo.Add(log.ToProtobuf());

			foreach (var contractId in CreatedContractIds)
				Proto.Services.CreatedContractIDs.Add(contractId.ToProtobuf());

            if (SenderAccountId != null)
				Proto.Services.SenderId = SenderAccountId.ToProtobuf();


            // for (var stateChange : stateChanges) {
            //     contractFunctionResult.addStateChanges(stateChange.toProtobuf());
            // }
            foreach (var contractNonce in ContractNonces)
                Proto.Services.ContractNonces.Add(contractNonce.ToProtobuf());

            return proto;
        }		
	}
}
