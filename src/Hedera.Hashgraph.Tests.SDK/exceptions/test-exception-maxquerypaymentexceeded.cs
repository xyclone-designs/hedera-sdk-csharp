// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.Tests.SDK.Exceptions
{
    /// <include file="test-exception-maxquerypaymentexceeded.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Exceptions.MaxQueryPaymentExceededExceptionTest"]' />
    public class MaxQueryPaymentExceededExceptionTest
    {
        [Fact]
        /// <include file="test-exception-maxquerypaymentexceeded.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Exceptions.MaxQueryPaymentExceededExceptionTest.ShouldHaveMessage"]' />
        public virtual void ShouldHaveMessage()
        {
            var e = new MaxQueryPaymentExceededException(typeof(AccountBalanceQuery), new Hbar(30), new Hbar(15));

            Assert.Equal(e.Message, "cost for AccountBalanceQuery, of 30 ℏ, without explicit payment is greater than the maximum allowed payment of 15 ℏ");
        }
    }
}
