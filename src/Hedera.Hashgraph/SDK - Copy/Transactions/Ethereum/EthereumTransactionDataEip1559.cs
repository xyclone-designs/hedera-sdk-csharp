// SPDX-License-Identifier: Apache-2.0
using Com.Esaulpaugh.Headlong.Rlp;
using Com.Esaulpaugh.Headlong.Util;
using Com.Google.Common.Base;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Transactions.Ethereum
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
        public byte[] chainId;
        /// <summary>
        /// Transaction's nonce
        /// </summary>
        public byte[] nonce;
        /// <summary>
        /// An 'optional' additional fee in Ethereum that is paid directly to miners in order to incentivize them to include
        /// your transaction in a block. Not used in Hedera
        /// </summary>
        public byte[] maxPriorityGas;
        /// <summary>
        /// The maximum amount, in tinybars, that the payer of the hedera transaction is willing to pay to complete the
        /// transaction
        /// </summary>
        public byte[] maxGas;
        /// <summary>
        /// The amount of gas available for the transaction
        /// </summary>
        public byte[] gasLimit;
        /// <summary>
        /// The receiver of the transaction
        /// </summary>
        public byte[] to;
        /// <summary>
        /// The transaction value
        /// </summary>
        public byte[] value;
        /// <summary>
        /// specifies an array of addresses and storage keys that the transaction plans to access
        /// </summary>
        public byte[] accessList;
        /// <summary>
        /// recovery parameter used to ease the signature verification
        /// </summary>
        public byte[] recoveryId;
        /// <summary>
        /// The R value of the signature
        /// </summary>
        public byte[] r;
        /// <summary>
        /// The S value of the signature
        /// </summary>
        public byte[] s;
        EthereumTransactionDataEip1559(byte[] chainId, byte[] nonce, byte[] maxPriorityGas, byte[] maxGas, byte[] gasLimit, byte[] to, byte[] value, byte[] callData, byte[] accessList, byte[] recoveryId, byte[] r, byte[] s) : base(callData)
        {
            chainId = chainId;
            nonce = nonce;
            maxPriorityGas = maxPriorityGas;
            maxGas = maxGas;
            gasLimit = gasLimit;
            to = to;
            value = value;
            accessList = accessList;
            recoveryId = recoveryId;
            r = r;
            s = s;
        }

        /// <summary>
        /// Convert a byte array to an ethereum transaction data.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>the ethereum transaction data</returns>
        public static EthereumTransactionDataEip1559 FromBytes(byte[] bytes)
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

        public virtual byte[] ToBytes()
        {
            return RLPEncoder.Sequence(Integers.ToBytes(0x02), List.Of(chainId, nonce, maxPriorityGas, maxGas, gasLimit, to, value, callData, new List<string>(), recoveryId, r, s));
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("chainId", Hex.ToHexString(chainId)).Add("nonce", Hex.ToHexString(nonce)).Add("maxPriorityGas", Hex.ToHexString(maxPriorityGas)).Add("maxGas", Hex.ToHexString(maxGas)).Add("gasLimit", Hex.ToHexString(gasLimit)).Add("to", Hex.ToHexString(to)).Add("value", Hex.ToHexString(value)).Add("accessList", Hex.ToHexString(accessList)).Add("recoveryId", Hex.ToHexString(recoveryId)).Add("r", Hex.ToHexString(r)).Add("s", Hex.ToHexString(s)).ToString();
        }
    }
}