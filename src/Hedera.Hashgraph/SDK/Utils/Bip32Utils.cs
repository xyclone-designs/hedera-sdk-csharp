namespace Hedera.Hashgraph.SDK.Utils
{
	/**
	 * Utility class for BIP32 functionalities
	 */
	public static class Bip32Utils
	{

		/**
		 * Indicates if the index is hardened
		 */
		public static readonly uint HARDENED_BIT = 0x80000000;


		/**
		 * Harden the index
		 *
		 * @param index         the derivation index
		 * @return              the hardened index
		 */
		public static uint ToHardenedIndex(int index)
		{
			return index | HARDENED_BIT;
		}

		/**
		 * Check if the index is hardened
		 *
		 * @param index         the derivation index
		 * @return              true if the index is hardened
		 */
		public static bool IsHardenedIndex(int index)
		{
			return (index & HARDENED_BIT) != 0;
		}
	}

}