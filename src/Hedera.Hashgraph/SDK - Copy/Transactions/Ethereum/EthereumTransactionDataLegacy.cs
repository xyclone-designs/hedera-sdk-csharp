// SPDX-License-Identifier: Apache-2.0
using Com.Esaulpaugh.Headlong.Rlp;
using Com.Google.Common.Base;
using Java.Math;
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
    /// The ethereum transaction data, in the legacy format
    /// </summary>
    public class EthereumTransactionDataLegacy : EthereumTransactionData
    {
        /// <summary>
        /// ID of the chain
        /// </summary>
        public byte[] chainId = new byte[]
        {
        };
        /// <summary>
        /// Transaction's nonce
        /// </summary>
        public byte[] nonce;
        /// <summary>
        /// The price for 1 gas
        /// </summary>
        public byte[] gasPrice;
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
        /// The V value of the signature
        /// </summary>
        public byte[] v;
        /// <summary>
        /// recovery parameter used to ease the signature verification
        /// </summary>
        public int recoveryId;
        /// <summary>
        /// The R value of the signature
        /// </summary>
        public byte[] r;
        /// <summary>
        /// The S value of the signature
        /// </summary>
        public byte[] s;
        EthereumTransactionDataLegacy(byte[] nonce, byte[] gasPrice, byte[] gasLimit, byte[] to, byte[] value, byte[] callData, byte[] v, byte[] r, byte[] s) : base(callData)
        {
            nonce = nonce;
            gasPrice = gasPrice;
            gasLimit = gasLimit;
            to = to;
            value = value;
            v = v;
            r = r;
            s = s;
            var vBI = new BigInteger(1, v);
            recoveryId = vBI.TestBit(0) ? 0 : 1;
            if (vBI.CompareTo(BigInteger.ValueOf(34)) > 0)
            {
                chainId = vBI.Subtract(BigInteger.ValueOf(35)).ShiftRight(1).ToByteArray();
            }
        }

        /// <summary>
        /// Convert a byte array to an ethereum transaction data.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the ethereum transaction data</returns>
        public static EthereumTransactionDataLegacy FromBytes(byte[] bytes)
        {
            var decoder = RLPDecoder.RLP_STRICT.SequenceIterator(bytes);
            var rlpItem = decoder.Next();
            IList<RLPItem> rlpList = rlpItem.AsRLPList().Elements();
            if (rlpList.Count != 9)
            {
                throw new ArgumentException("expected 9 RLP encoded elements, found " + rlpList.Count);
            }

            return new EthereumTransactionDataLegacy(rlpList[0].Data(), rlpList[1].AsBytes(), rlpList[2].Data(), rlpList[3].Data(), rlpList[4].Data(), rlpList[5].Data(), rlpList[6].AsBytes(), rlpList[7].Data(), rlpList[8].Data());
        }

        public virtual byte[] ToBytes()
        {
            return RLPEncoder.List(nonce, gasPrice, gasLimit, to, value, callData, v, r, s);
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("chainId", Hex.ToHexString(chainId)).Add("nonce", Hex.ToHexString(nonce)).Add("gasPrice", Hex.ToHexString(gasPrice)).Add("gasLimit", Hex.ToHexString(gasLimit)).Add("to", Hex.ToHexString(to)).Add("value", Hex.ToHexString(value)).Add("recoveryId", recoveryId).Add("v", Hex.ToHexString(v)).Add("r", Hex.ToHexString(r)).Add("s", Hex.ToHexString(s)).ToString();
        }
    }
}