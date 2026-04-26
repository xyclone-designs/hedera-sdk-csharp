// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.Tests.SDK.Exceptions
{
    public class MaxQueryPaymentExceededExceptionTest
    {
        [Fact]
        public virtual void ShouldHaveMessage()
        {
            var e = new MaxQueryPaymentExceededException(typeof(AccountBalanceQuery), new Hbar(30), new Hbar(15));

            Assert.Equal(e.Message, "cost for AccountBalanceQuery, of 30 ℏ, without explicit payment is greater than the maximum allowed payment of 15 ℏ");
        }
    }
}