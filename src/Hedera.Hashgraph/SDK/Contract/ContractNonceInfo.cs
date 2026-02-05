// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ids;

using System;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// Info about a contract account's nonce value.
    /// A nonce of a contract is only incremented when that contract creates another contract.
    /// </summary>
    public sealed class ContractNonceInfo
    {
        /// <summary>
        /// Id of the contract
        /// </summary>
        public readonly ContractId ContractId;
        /// <summary>
        /// The current value of the contract account's nonce property
        /// </summary>
        public readonly long Nonce;
        public ContractNonceInfo(ContractId contractId, long nonce)
        {
            ContractId = contractId;
            Nonce = nonce;
        }

        /// <summary>
        /// Extract the contractNonce from the protobuf.
        /// </summary>
        /// <param name="contractNonceInfo">the protobuf</param>
        /// <returns>the contract object</returns>
        public static ContractNonceInfo FromProtobuf(Proto.ContractNonceInfo contractNonceInfo)
        {
            return new ContractNonceInfo(ContractId.FromProtobuf(contractNonceInfo.ContractId), contractNonceInfo.Nonce);
        }

        /// <summary>
        /// Extract the contractNonce from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>the extracted contract</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static ContractNonceInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.ContractNonceInfo.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Build the protobuf.
        /// </summary>
        /// <returns>the protobuf representation</returns>
        public Proto.ContractNonceInfo ToProtobuf()
        {
            return new Proto.ContractNonceInfo
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

        /// <summary>
        /// Create a byte array representation.
        /// </summary>
        /// <returns>the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}