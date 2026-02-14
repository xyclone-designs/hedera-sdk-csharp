// SPDX-License-Identifier: Apache-2.0
using Nethereum.RLP;

using System;

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