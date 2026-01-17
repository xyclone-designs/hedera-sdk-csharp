namespace Hedera.Hashgraph.SDK
{
	/**
 * This class represents the data of an Ethereum transaction.
 * <p>
 * It may be of subclass {@link EthereumTransactionDataLegacy} or of subclass {@link EthereumTransactionDataEip1559}
 */
	public abstract class EthereumTransactionData
	{
		/**
		 * The raw call data.
		 */
		public byte[] callData;

		EthereumTransactionData(byte[] callData)
		{
			this.callData = callData;
		}

		public static EthereumTransactionData FromBytes(byte[] bytes)
		{
			var decoder = RLPDecoder.RLP_STRICT.sequenceIterator(bytes);
			var rlpItem = decoder.next();

			return rlpItem.isList()
				? EthereumTransactionDataLegacy.FromBytes(bytes)
				: EthereumTransactionDataEip1559.FromBytes(bytes);
		}

		/**
		 * Serialize the ethereum transaction data into bytes using RLP
		 *
		 * @return the serialized transaction as a byte array
		 */
		public abstract byte[] ToBytes();
	}
}