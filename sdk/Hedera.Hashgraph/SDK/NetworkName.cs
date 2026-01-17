using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Enum for the network names.
	 */
	[Obsolete]
	public enum NetworkName
	{
		/**
		 * The mainnet network
		 */
		[Obsolete]
		Mainnet = 0,

		/**
		 * The testnet network
		 */
		[Obsolete]
		Testnet = 1,

		/**
		 * The previewnet network
		 */
		[Obsolete]
		PreviewNet = 2,
		/**
		 * Other network
		 */
		[Obsolete]
		Other = int.MaxValue,
	}
}