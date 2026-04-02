// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Fees;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class FeeAssessmentMethodTest
    {
        [Fact]
        public virtual void FeeAssessmentMethodToString()
        {
            Assert.Equal(true.ToFeeAssessmentMethod().ToString(), FeeAssessmentMethod.Exclusive.ToString());
            Assert.Equal(false.ToFeeAssessmentMethod().ToString(), FeeAssessmentMethod.Inclusive.ToString());
        }
    }
}