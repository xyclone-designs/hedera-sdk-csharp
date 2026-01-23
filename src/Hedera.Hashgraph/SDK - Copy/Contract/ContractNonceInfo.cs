// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

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
        public readonly ContractId contractId;
        /// <summary>
        /// The current value of the contract account's nonce property
        /// </summary>
        public readonly long nonce;
        public ContractNonceInfo(ContractId contractId, long nonce)
        {
            contractId = contractId;
            nonce = nonce;
        }

        /// <summary>
        /// Extract the contractNonce from the protobuf.
        /// </summary>
        /// <param name="contractNonceInfo">the protobuf</param>
        /// <returns>the contract object</returns>
        static ContractNonceInfo FromProtobuf(Proto.ContractNonceInfo contractNonceInfo)
        {
            return new ContractNonceInfo(ContractId.FromProtobuf(contractNonceInfo.GetContractId()), contractNonceInfo.GetNonce());
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
        Proto.ContractNonceInfo ToProtobuf()
        {
            return Proto.ContractNonceInfo.SetContractId(contractId.ToProtobuf()).SetNonce(nonce).Build();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(contractId, nonce);
        }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is ContractNonceInfo))
            {
                return false;
            }

            return contractId.Equals(otherInfo.contractId) && nonce.Equals(otherInfo.nonce);
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