// SPDX-License-Identifier: Apache-2.0
using Nethereum.RLP;

using Org.BouncyCastle.Math;

using System;

namespace Hedera.Hashgraph.SDK.Ethereum
{
    /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="T:EthereumTransactionDataLegacy"]/*' />
    public class EthereumTransactionDataLegacy : EthereumTransactionData
    {
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="F:EthereumTransactionDataLegacy.ChainId"]/*' />
        public byte[] ChainId = [];
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="F:EthereumTransactionDataLegacy.Nonce"]/*' />
        public byte[] Nonce;
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="F:EthereumTransactionDataLegacy.GasPrice"]/*' />
        public byte[] GasPrice;
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="F:EthereumTransactionDataLegacy.GasLimit"]/*' />
        public byte[] GasLimit;
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="F:EthereumTransactionDataLegacy.To"]/*' />
        public byte[] To;
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="F:EthereumTransactionDataLegacy.Value"]/*' />
        public byte[] Value;
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="M:EthereumTransactionDataLegacy.#ctor(System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[])"]/*' />
        public byte[] V;
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="M:EthereumTransactionDataLegacy.#ctor(System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[])_2"]/*' />
        public int RecoveryId;
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="M:EthereumTransactionDataLegacy.#ctor(System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[])_3"]/*' />
        public byte[] R;
        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="M:EthereumTransactionDataLegacy.#ctor(System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[])_4"]/*' />
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

        /// <include file="EthereumTransactionDataLegacy.cs.xml" path='docs/member[@name="M:EthereumTransactionDataLegacy.FromBytes(System.Byte[])"]/*' />
        public new static EthereumTransactionDataLegacy FromBytes(byte[] bytes)
	    {
		    if (bytes == null || bytes.Length == 0)
			    throw new ArgumentException(null, nameof(bytes));

		    if (RLP.Decode(bytes) is not RLPCollection rlpList)
			    throw new ArgumentException("Expected RLP list for legacy transaction.");

		    if (rlpList.Count != 9)
			    throw new ArgumentException($"expected 9 RLP encoded elements, found {rlpList.Count}");

		    return new EthereumTransactionDataLegacy(
				nonce: rlpList[0].RLPData,
			    gasPrice: rlpList[1].RLPData,
			    gasLimit: rlpList[2].RLPData,
			    to: rlpList[3].RLPData,
			    value: rlpList[4].RLPData,
			    callData: rlpList[5].RLPData,
			    v: rlpList[6].RLPData,
			    r: rlpList[7].RLPData,
			    s: rlpList[8].RLPData 
		    );
	    }

		public override byte[] ToBytes()
		{
			return RLP.EncodeList(
				RLP.EncodeElement(Nonce),
				RLP.EncodeElement(GasPrice),
				RLP.EncodeElement(GasLimit),
				RLP.EncodeElement(To),
				RLP.EncodeElement(Value),
				RLP.EncodeElement(CallData),
				RLP.EncodeElement(V),
				RLP.EncodeElement(R),
				RLP.EncodeElement(S)
			);
		}
		public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}