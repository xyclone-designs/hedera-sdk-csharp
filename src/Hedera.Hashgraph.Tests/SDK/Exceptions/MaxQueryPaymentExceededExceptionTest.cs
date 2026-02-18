// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.Tests.SDK.Exceptions
{
    class MaxQueryPaymentExceededExceptionTest
    {
        public virtual void ShouldHaveMessage()
        {
            var e = new MaxQueryPaymentExceededException(new AccountBalanceQuery(), new Hbar(30), new Hbar(15));
            Assert.Equal(e.Message, "cost for AccountBalanceQuery, of 30 ℏ, without explicit payment is greater than the maximum allowed payment of 15 ℏ");
        }
    }
}