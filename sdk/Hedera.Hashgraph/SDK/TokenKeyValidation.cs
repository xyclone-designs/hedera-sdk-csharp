namespace Hedera.Hashgraph.SDK
{
	/**
	 * Types of validation strategies for token keys.
	 *
	 */
	public enum TokenKeyValidation
	{
		/**
		 * Currently the default behaviour. It will perform all token key validations.
		 */
		FullValidation = Proto.TokenKeyValidation.FullValidation,

		/**
		 * Perform no validations at all for all passed token keys.
		 */
		NoValidation = Proto.TokenKeyValidation.NoValidation,
	}
}