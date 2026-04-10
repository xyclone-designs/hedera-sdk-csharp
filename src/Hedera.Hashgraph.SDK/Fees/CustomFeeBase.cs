// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    public abstract class CustomFeeBase<F> : CustomFee where F : CustomFeeBase<F>
    {
        public abstract F DeepCloneSubclass();

        /// <include file="CustomFeeBase.cs.xml" path='docs/member[@name="M:FinishDeepClone(CustomFeeBase{F})"]/*' />
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