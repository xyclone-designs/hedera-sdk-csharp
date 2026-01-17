namespace Hedera.Hashgraph.SDK
{
	/**
	* Random Number Generator Transaction.
	*/
	public class PrngTransaction : Transaction<PrngTransaction> 
	{

		/**
		 * If provided and is positive, returns a 32-bit pseudorandom number from the given range in the transaction record.
		 * If not set or set to zero, will return a 384-bit pseudorandom data in the record.
		 */
		public int? Range { get; set; }

		/**
		 * Constructor.
		 */
		public PrngTransaction() { }

	
		public override void OnFreeze(Proto.TransactionBody.Builder bodyBuilder)
		{
			bodyBuilder.setUtilPrng(build());
		}
		public override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
		{
			throw new UnsupportedOperationException("cannot schedule RngTransaction");
		}
	}

}