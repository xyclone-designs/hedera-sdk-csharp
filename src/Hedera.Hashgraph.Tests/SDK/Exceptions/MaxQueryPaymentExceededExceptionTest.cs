// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Exceptions
{
    class MaxQueryPaymentExceededExceptionTest
    {
        public virtual void ShouldHaveMessage()
        {
            var e = new MaxQueryPaymentExceededException(new AccountBalanceQuery(), new Hbar(30), new Hbar(15));
            Assert.Equal(e.GetMessage(), "cost for AccountBalanceQuery, of 30 ℏ, without explicit payment is greater than the maximum allowed payment of 15 ℏ");
        }
    }
}