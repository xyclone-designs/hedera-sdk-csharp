// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    public abstract class CustomFeeBase<F> : CustomFee where F : CustomFeeBase<F>
    {
        public abstract F DeepCloneSubclass();

        /// <summary>
        /// Finishes the deep clone by setting the fields of the {@link CustomFeeBase} class
        /// </summary>
        /// <param name="source">the source object</param>
        /// <returns>the cloned object</returns>
        protected virtual F FinishDeepClone(CustomFeeBase<F> source)
        {
            FeeCollectorAccountId = source.FeeCollectorAccountId;
            AllCollectorsAreExempt = source.AllCollectorsAreExempt;

            // noinspection unchecked
            return (F)this;
        }
        public override CustomFee DeepClone()
        {
            return DeepCloneSubclass();
        }
    }
}