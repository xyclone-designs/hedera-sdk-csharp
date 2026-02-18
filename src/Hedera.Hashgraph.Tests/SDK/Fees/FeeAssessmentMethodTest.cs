// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    class FeeAssessmentMethodTest
    {
        public virtual void FeeAssessmentMethodToString()
        {
            Assert.Equal(FeeAssessmentMethod.ValueOf(true).ToString(), FeeAssessmentMethod.EXCLUSIVE.ToString());
            Assert.Equal(FeeAssessmentMethod.ValueOf(false).ToString(), FeeAssessmentMethod.INCLUSIVE.ToString());
        }
    }
}