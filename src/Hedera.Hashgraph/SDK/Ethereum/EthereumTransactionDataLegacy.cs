// SPDX-License-Identifier: Apache-2.0
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Ethereum
{
    /// <summary>
    /// The ethereum transaction data, in the legacy format
    /// </summary>
    public class EthereumTransactionDataLegacy : EthereumTransactionData
    {
        /// <summary>
        /// ID of the chain
        /// </summary>
        public byte[] ChainId = [];
        /// <summary>
        /// Transaction's nonce
        /// </summary>
        public byte[] Nonce;
        /// <summary>
        /// The price for 1 gas
        /// </summary>
        public byte[] GasPrice;
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
        /// The V value of the signature
        /// </summary>
        public byte[] V;
        /// <summary>
        /// recovery parameter used to ease the signature verification
        /// </summary>
        public int RecoveryId;
        /// <summary>
        /// The R value of the signature
        /// </summary>
        public byte[] R;
        /// <summary>
        /// The S value of the signature
        /// </summary>
        public byte[] S;

        public EthereumTransactionDataLegacy(byte[] nonce, byte[] gasPrice, byte[] gasLimit, byte[] to, byte[] value, byte[] callData, byte[] v, byte[] r, byte[] s) : base(callData)
        {
            Nonce = nonce;
            GasPrice = gasPrice;
            GasLimit = gasLimit;
            To = to;
            Value = value;
            V = v;
            R = r;
            S = s;
            var vBI = new BigInteger(1, v);
            RecoveryId = vBI.TestBit(0) ? 0 : 1;
            if (vBI.CompareTo(BigInteger.ValueOf(34)) > 0)
            {
                ChainId = vBI.Subtract(BigInteger.ValueOf(35)).ShiftRight(1).ToByteArray();
            }
        }

		/// <summary>
		/// Convert a byte array to an ethereum transaction data.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the ethereum transaction data</returns>
		public new static EthereumTransactionDataLegacy FromBytes(byte[] bytes)
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

        public override byte[] ToBytes()
        {
            return RLPEncoder.List(Nonce, GasPrice, GasLimit, To, Value, callData, V, R, S);
        }
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}