// SPDX-License-Identifier: Apache-2.0
using Nethereum.RLP;

namespace Hedera.Hashgraph.SDK.Ethereum
{
    /// <include file="EthereumTransactionData.cs.xml" path='docs/member[@name="T:EthereumTransactionData"]/*' />
    public abstract class EthereumTransactionData
    {
        public EthereumTransactionData(byte[] callData)
        {
            CallData = callData;
        }

        public static EthereumTransactionData FromBytes(byte[] bytes)
        {
			IRLPElement decoded = RLP.Decode(bytes);

			if (decoded is RLPCollection)
				return EthereumTransactionDataLegacy.FromBytes(bytes);
			else
				return EthereumTransactionDataEip1559.FromBytes(bytes);
		}

		/// <include file="EthereumTransactionData.cs.xml" path='docs/member[@name="P:EthereumTransactionData.CallData"]/*' />
		public byte[] CallData { get; }

        /// <include file="EthereumTransactionData.cs.xml" path='docs/member[@name="M:EthereumTransactionData.ToBytes"]/*' />
        public abstract byte[] ToBytes();
        /// <include file="EthereumTransactionData.cs.xml" path='docs/member[@name="M:EthereumTransactionData.ToString"]/*' />
        public new abstract string ToString();
    }
}