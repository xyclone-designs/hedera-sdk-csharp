// SPDX-License-Identifier: Apache-2.0
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Ethereum
{
    /// <summary>
    /// The ethereum transaction data, in the format defined in <a
    /// href="https://github.com/ethereum/EIPs/blob/master/EIPS/eip-1559.md">EIP-1559</a>
    /// </summary>
    public class EthereumTransactionDataEip1559 : EthereumTransactionData
    {
        /// <summary>
        /// ID of the chain
        /// </summary>
        public byte[] ChainId;
        /// <summary>
        /// Transaction's nonce
        /// </summary>
        public byte[] Nonce;
        /// <summary>
        /// An 'optional' additional fee in Ethereum that is paid directly to miners in order to incentivize them to include
        /// your transaction in a block. Not used in Hedera
        /// </summary>
        public byte[] MaxPriorityGas;
        /// <summary>
        /// The maximum amount, in tinybars, that the payer of the hedera transaction is willing to pay to complete the
        /// transaction
        /// </summary>
        public byte[] MaxGas;
        /// <summary>
        /// The amount of gas available for the transaction
        /// </summary>
        public byte[] GasLimit;
        /// <summary>
        /// The receiver of the transaction
        /// </summary>
        public byte[] To;
        /// <summary>
        /// The transaction value
        /// </summary>
        public byte[] Value;
        /// <summary>
        /// specifies an array of addresses and storage keys that the transaction plans to access
        /// </summary>
        public byte[] AccessList;
        /// <summary>
        /// recovery parameter used to ease the signature verification
        /// </summary>
        public byte[] RecoveryId;
        /// <summary>
        /// The R value of the signature
        /// </summary>
        public byte[] R;
        /// <summary>
        /// The S value of the signature
        /// </summary>
        public byte[] S;
        EthereumTransactionDataEip1559(byte[] chainId, byte[] nonce, byte[] maxPriorityGas, byte[] maxGas, byte[] gasLimit, byte[] to, byte[] value, byte[] callData, byte[] accessList, byte[] recoveryId, byte[] r, byte[] s) : base(callData)
        {
            ChainId = chainId;
            Nonce = nonce;
            MaxPriorityGas = maxPriorityGas;
            MaxGas = maxGas;
            GasLimit = gasLimit;
            To = to;
            Value = value;
            AccessList = accessList;
            RecoveryId = recoveryId;
            R = r;
            S = s;
        }

        /// <summary>
        /// Convert a byte array to an ethereum transaction data.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>the ethereum transaction data</returns>
        public new static EthereumTransactionDataEip1559 FromBytes(byte[] bytes)
        {
            var decoder = RLPDecoder.RLP_STRICT.SequenceIterator(bytes);
            var rlpItem = decoder.Next();

            // typed transaction?
            byte typeByte = rlpItem.AsByte();
            if (typeByte != 2)
            {
                throw new ArgumentException("rlp type byte " + typeByte + "is not supported");
            }

            rlpItem = decoder.Next();
            if (!rlpItem.IsList())
            {
                throw new ArgumentException("expected RLP element list");
            }

            IList<RLPItem> rlpList = rlpItem.AsRLPList().Elements();
            if (rlpList.Count != 12)
            {
                throw new ArgumentException("expected 12 RLP encoded elements, found " + rlpList.Count);
            }

            return new EthereumTransactionDataEip1559(rlpList[0].Data(), rlpList[1].Data(), rlpList[2].Data(), rlpList[3].Data(), rlpList[4].Data(), rlpList[5].Data(), rlpList[6].Data(), rlpList[7].Data(), rlpList[8].Data(), rlpList[9].Data(), rlpList[10].Data(), rlpList[11].Data());
        }

        public override byte[] ToBytes()
        {
            return RLPEncoder.Sequence(byte.Parse("0x02"), [ChainId, Nonce, MaxPriorityGas, MaxGas, GasLimit, To, Value, CallData, new List<string>(), RecoveryId, R, S]);
        }
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}