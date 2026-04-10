// SPDX-License-Identifier: Apache-2.0
using Nethereum.RLP;

using System;

namespace Hedera.Hashgraph.SDK.Ethereum
{
    /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="T:EthereumTransactionDataEip1559"]/*' />
    public class EthereumTransactionDataEip1559 : EthereumTransactionData
    {
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.ChainId"]/*' />
        public byte[] ChainId;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.Nonce"]/*' />
        public byte[] Nonce;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.MaxPriorityGas"]/*' />
        public byte[] MaxPriorityGas;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.MaxGas"]/*' />
        public byte[] MaxGas;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.GasLimit"]/*' />
        public byte[] GasLimit;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.To"]/*' />
        public byte[] To;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.Value"]/*' />
        public byte[] Value;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.AccessList"]/*' />
        public byte[] AccessList;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.RecoveryId"]/*' />
        public byte[] RecoveryId;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.R"]/*' />
        public byte[] R;
        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="F:EthereumTransactionDataEip1559.S"]/*' />
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

        /// <include file="EthereumTransactionDataEip1559.cs.xml" path='docs/member[@name="M:EthereumTransactionDataEip1559.FromBytes(System.Byte[])"]/*' />
        public new static EthereumTransactionDataEip1559 FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentException(null, nameof(bytes));

			// typed transaction?
			byte typeByte = bytes[0];
            if (typeByte != 0x02)
                throw new ArgumentException($"rlp type byte {typeByte} is not supported");

            // --- 2. Decode the RLP payload (after type byte) ---
            var payload = new byte[bytes.Length - 1];
            Buffer.BlockCopy(bytes, 1, payload, 0, payload.Length);

            var decoded = RLP.Decode(payload);

            if (decoded is not RLPCollection rlpList)
                throw new ArgumentException("expected RLP element list");

            if (rlpList.Count != 12)
                throw new ArgumentException($"expected 12 RLP encoded elements, found {rlpList.Count}");

            return new EthereumTransactionDataEip1559(
				chainId: rlpList[0].RLPData,
                nonce: rlpList[1].RLPData,
                maxPriorityGas: rlpList[2].RLPData,
                maxGas: rlpList[3].RLPData,
                gasLimit: rlpList[4].RLPData,
                to: rlpList[5].RLPData,
                value: rlpList[6].RLPData,
                callData: rlpList[7].RLPData,
                accessList : rlpList[8].RLPData,   // (raw RLP)
                recoveryId : rlpList[9].RLPData,   // (v)
                r: rlpList[10].RLPData,
                s: rlpList[11].RLPData
            );
        }


		public override byte[] ToBytes()
		{
			// Java Code 'RLPEncoder.Sequence(byte.Parse("0x02"), [ChainId, Nonce, MaxPriorityGas, MaxGas, GasLimit, To, Value, CallData, new List<string>(), RecoveryId, R, S]);'

			var rlpList = RLP.EncodeList(
				RLP.EncodeElement(ChainId),
				RLP.EncodeElement(Nonce),
				RLP.EncodeElement(MaxPriorityGas),
				RLP.EncodeElement(MaxGas),
				RLP.EncodeElement(GasLimit),
				RLP.EncodeElement(To),
				RLP.EncodeElement(Value),
				RLP.EncodeElement(CallData),
				RLP.EncodeList(), // empty access list
				RLP.EncodeElement(RecoveryId),
				RLP.EncodeElement(R),
				RLP.EncodeElement(S)
			);

			// Prefix with transaction type 0x02
			var result = new byte[1 + rlpList.Length];
			result[0] = 0x02;
			Buffer.BlockCopy(rlpList, 0, result, 1, rlpList.Length);

			return result;
		}

		public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}