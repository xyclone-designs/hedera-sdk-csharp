// SPDX-License-Identifier: Apache-2.0
using Org.BouncyCastle.Crypto.Digests;

using System;
using System.Text;

namespace Hedera.Hashgraph.SDK
{
	/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="T:ContractFunctionSelector"]/*' />
	public sealed class ContractFunctionSelector
	{
		private KeccakDigest? digest;
		private bool needsComma = false;
		private byte[]? finished = null;

		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.#ctor(System.String)"]/*' />
		public ContractFunctionSelector(string funcName)
		{
			digest = new KeccakDigest(256);

			var funcBytes = Encoding.ASCII.GetBytes(funcName);

			digest.BlockUpdate(funcBytes, 0, funcBytes.Length);
			digest.Update((byte)'(');
		}

		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddString"]/*' />
		public ContractFunctionSelector AddString()
		{
			return AddParamType("string");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddStringArray"]/*' />
		public ContractFunctionSelector AddStringArray()
		{
			return AddParamType("string[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddBytes"]/*' />
		public ContractFunctionSelector AddBytes()
		{
			return AddParamType("bytes");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddBytesArray"]/*' />
		public ContractFunctionSelector AddBytesArray()
		{
			return AddParamType("bytes[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddBytes32"]/*' />
		public ContractFunctionSelector AddBytes32()
		{
			return AddParamType("bytes32");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddBytes32Array"]/*' />
		public ContractFunctionSelector AddBytes32Array()
		{
			return AddParamType("bytes32[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddBool"]/*' />
		public ContractFunctionSelector AddBool()
		{
			return AddParamType("bool");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddInt8"]/*' />
		public ContractFunctionSelector AddInt8()
		{
			return AddParamType("int8");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddInt32"]/*' />
		public ContractFunctionSelector AddInt32()
		{
			return AddParamType("int32");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddInt64"]/*' />
		public ContractFunctionSelector AddInt64()
		{
			return AddParamType("int64");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddInt256"]/*' />
		public ContractFunctionSelector AddInt256()
		{
			return AddParamType("int256");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddInt8Array"]/*' />
		public ContractFunctionSelector AddInt8Array()
		{
			return AddParamType("int8[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddInt32Array"]/*' />
		public ContractFunctionSelector AddInt32Array()
		{
			return AddParamType("int32[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddInt64Array"]/*' />
		public ContractFunctionSelector AddInt64Array()
		{
			return AddParamType("int64[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddInt256Array"]/*' />
		public ContractFunctionSelector AddInt256Array()
		{
			return AddParamType("int256[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddUint8"]/*' />
		public ContractFunctionSelector AddUint8()
		{
			return AddParamType("uint8");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddUint32"]/*' />
		public ContractFunctionSelector AddUint32()
		{
			return AddParamType("uint32");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddUint64"]/*' />
		public ContractFunctionSelector AddUint64()
		{
			return AddParamType("uint64");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddUint256"]/*' />
		public ContractFunctionSelector AddUint256()
		{
			return AddParamType("uint256");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddUint8Array"]/*' />
		public ContractFunctionSelector AddUint8Array()
		{
			return AddParamType("uint8[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddUint32Array"]/*' />
		public ContractFunctionSelector AddUint32Array()
		{
			return AddParamType("uint32[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddUint64Array"]/*' />
		public ContractFunctionSelector AddUint64Array()
		{
			return AddParamType("uint64[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddUint256Array"]/*' />
		public ContractFunctionSelector AddUint256Array()
		{
			return AddParamType("uint256[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddAddress"]/*' />
		public ContractFunctionSelector AddAddress()
		{
			return AddParamType("address");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddAddressArray"]/*' />
		public ContractFunctionSelector AddAddressArray()
		{
			return AddParamType("address[]");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddFunction"]/*' />
		public ContractFunctionSelector AddFunction()
		{
			return AddParamType("function");
		}
		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.AddParamType(System.String)"]/*' />
		public ContractFunctionSelector AddParamType(string typeName)
		{
			if (digest is null) throw new InvalidOperationException("Finish() has been called");
			if (finished is not null) throw new InvalidOperationException("FunctionSelector already finished");
			if (needsComma) digest.Update((byte)',');

			var typeNameBytes = Encoding.ASCII.GetBytes(typeName);

			digest.BlockUpdate(typeNameBytes, 0, typeNameBytes.Length);

			needsComma = true;

			return this;
		}

		/// <include file="ContractFunctionSelector.cs.xml" path='docs/member[@name="M:ContractFunctionSelector.Finish"]/*' />
		public byte[] Finish()
		{
			if (digest is null) throw new InvalidOperationException("Finish() has been called");
			if (finished is null)
			{
				// If you need the final hash:
				finished = new byte[digest.GetDigestSize()];
				digest.DoFinal(finished, 0);
				digest = null;
			}

			return finished;
		}
	}
}