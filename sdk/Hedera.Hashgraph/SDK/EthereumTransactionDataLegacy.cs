using Hedera.Hashgraph.SDK;
using System.Numerics;
using System.Text.Encodings.Web;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * The ethereum transaction data, in the legacy format
	 */
	public class EthereumTransactionDataLegacy : EthereumTransactionData
	{

		/**
		* ID of the chain
		*/
		public byte[] chainId = [];

		/**
		* Transaction's nonce
		*/
		public byte[] nonce;

		/**
		* The price for 1 gas
		*/
		public byte[] gasPrice;

		/**
		* The amount of gas available for the transaction
		*/
		public byte[] gasLimit;

		/**
		* The receiver of the transaction
		*/
		public byte[] to;

		/**
		* The transaction value
		*/
		public byte[] value;

		/**
		* The V value of the signature
		*/
		public byte[] v;

		/**
		* recovery parameter used to ease the signature verification
		*/
		public int recoveryId;

		/**
		* The R value of the signature
		*/
		public byte[] r;

		/**
		* The S value of the signature
		*/
		public byte[] s;

		EthereumTransactionDataLegacy(
				byte[] nonce,
				byte[] gasPrice,
				byte[] gasLimit,
				byte[] to,
				byte[] value,
				byte[] callData,
				byte[] v,
				byte[] r,
				byte[] s)
		{
			super(callData);

			this.nonce = nonce;
			this.gasPrice = gasPrice;
			this.gasLimit = gasLimit;
			this.to = to;
			this.value = value;
			this.v = v;
			this.r = r;
			this.s = s;

			var vBI = new BigInteger(1, this.v);
			this.recoveryId = vBI.testBit(0) ? 0 : 1;

			if (vBI.compareTo(BigInteger.valueOf(34)) > 0)
			{
				this.chainId = vBI.subtract(BigInteger.valueOf(35)).shiftRight(1).ToByteArray();
			}
		}

		/**
			* Convert a byte array to an ethereum transaction data.
			*
			* @param bytes                     the byte array
			* @return                          the ethereum transaction data
			*/
		public static EthereumTransactionDataLegacy FromBytes(byte[] bytes)
		{
			var decoder = RLPDecoder.RLP_STRICT.sequenceIterator(bytes);
			var rlpItem = decoder.next();

			List<RLPItem> rlpList = rlpItem.asRLPList().elements();
			if (rlpList.size() != 9)
			{
				throw new ArgumentException("expected 9 RLP encoded elements, found " + rlpList.size());
			}

			return new EthereumTransactionDataLegacy(
					rlpList.get(0).data(),
					rlpList.get(1).asBytes(),
					rlpList.get(2).data(),
					rlpList.get(3).data(),
					rlpList.get(4).data(),
					rlpList.get(5).data(),
					rlpList.get(6).asBytes(),
					rlpList.get(7).data(),
					rlpList.get(8).data());
		}

		public byte[] ToBytes()
		{
			return RLPEncoder.list(nonce, gasPrice, gasLimit, to, value, callData, v, r, s);
		}

		public string toString()
		{
			return MoreObjects.toStringHelper(this)
					.Add("chainId", Hex.toHexString(this.chainId))
					.Add("nonce", Hex.toHexString(this.nonce))
					.Add("gasPrice", Hex.toHexString(this.gasPrice))
					.Add("gasLimit", Hex.toHexString(this.gasLimit))
					.Add("to", Hex.toHexString(this.to))
					.Add("value", Hex.toHexString(this.value))
					.Add("recoveryId", this.recoveryId)
					.Add("v", Hex.toHexString(this.v))
					.Add("r", Hex.toHexString(this.r))
					.Add("s", Hex.toHexString(this.s))
					.toString();
		}
	}
}