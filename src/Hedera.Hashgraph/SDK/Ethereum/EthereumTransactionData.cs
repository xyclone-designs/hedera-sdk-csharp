// SPDX-License-Identifier: Apache-2.0
using Nethereum.RLP;

namespace Hedera.Hashgraph.SDK.Ethereum
{
    /// <summary>
    /// This class represents the data of an Ethereum transaction.
    /// <p>
    /// It may be of subclass {@link EthereumTransactionDataLegacy} or of subclass {@link EthereumTransactionDataEip1559}
    /// </summary>
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

		/// <summary>
		/// The raw call data.
		/// </summary>
		public byte[] CallData { get; }

        /// <summary>
        /// Serialize the ethereum transaction data into bytes using RLP
        /// </summary>
        /// <returns>the serialized transaction as a byte array</returns>
        public abstract byte[] ToBytes();
        /// <summary>
        /// Serialize the ethereum transaction data into a string
        /// </summary>
        /// <returns>the serialized transaction as a string</returns>
        public new abstract string ToString();
    }
}