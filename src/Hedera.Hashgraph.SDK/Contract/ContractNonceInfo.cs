// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractNonceInfo.cs.xml" path='docs/member[@name="T:ContractNonceInfo"]/*' />
    public sealed class ContractNonceInfo
    {
        /// <include file="ContractNonceInfo.cs.xml" path='docs/member[@name="M:ContractNonceInfo.#ctor(ContractId,System.Int64)"]/*' />
        public readonly ContractId ContractId;
        /// <include file="ContractNonceInfo.cs.xml" path='docs/member[@name="M:ContractNonceInfo.#ctor(ContractId,System.Int64)_2"]/*' />
        public readonly long Nonce;
        public ContractNonceInfo(ContractId contractId, long nonce)
        {
            ContractId = contractId;
            Nonce = nonce;
        }

		/// <include file="ContractNonceInfo.cs.xml" path='docs/member[@name="M:ContractNonceInfo.FromBytes(System.Byte[])"]/*' />
		public static ContractNonceInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.ContractNonceInfo.Parser.ParseFrom(bytes));
		}
		/// <include file="ContractNonceInfo.cs.xml" path='docs/member[@name="M:ContractNonceInfo.FromProtobuf(Proto.Services.ContractNonceInfo)"]/*' />
		public static ContractNonceInfo FromProtobuf(Proto.Services.ContractNonceInfo contractNonceInfo)
        {
            return new ContractNonceInfo(ContractId.FromProtobuf(contractNonceInfo.ContractId), contractNonceInfo.Nonce);
        }

        /// <include file="ContractNonceInfo.cs.xml" path='docs/member[@name="M:ContractNonceInfo.ToProtobuf"]/*' />
        public Proto.Services.ContractNonceInfo ToProtobuf()
        {
            return new Proto.Services.ContractNonceInfo
            {
				Nonce = Nonce,
				ContractId = ContractId.ToProtobuf()
			};
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ContractId, Nonce);
        }
        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o is not ContractNonceInfo otherInfo)
            {
                return false;
            }

            return ContractId.Equals(otherInfo.ContractId) && Nonce.Equals(otherInfo.Nonce);
        }

        /// <include file="ContractNonceInfo.cs.xml" path='docs/member[@name="M:ContractNonceInfo.ToBytes"]/*' />
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}
