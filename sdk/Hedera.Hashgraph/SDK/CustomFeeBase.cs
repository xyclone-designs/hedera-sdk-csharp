namespace Hedera.Hashgraph.SDK
{
	internal abstract class CustomFeeBase<F> : CustomFee
	{
		/**
		 * Finishes the deep clone by setting the fields of the {@link CustomFeeBase} class
		 *
		 * @param source    the source object
		 * @return the cloned object
		 */
		protected CustomFeeBase<F> FinishDeepClone(CustomFeeBase<F> source)
		{
			FeeCollectorAccountId = source.FeeCollectorAccountId;
			AllCollectorsAreExempt = source.AllCollectorsAreExempt;

			// noinspection unchecked
			return this;
		}
		public abstract F DeepCloneSubclass();
		public override CustomFee DeepClone()
		{
			return DeepCloneSubclass();
		}
	}
}