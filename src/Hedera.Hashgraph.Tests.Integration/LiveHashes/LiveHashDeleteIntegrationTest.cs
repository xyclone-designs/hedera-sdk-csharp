// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.LiveHashes;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.Integration
{
    /// <include file="LiveHashDeleteIntegrationTest.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.Integration.LiveHashDeleteIntegrationTest"]' />
    public class LiveHashDeleteIntegrationTest
    {
        private static readonly byte[] HASH = Hex.Decode("100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002");

        [Fact]
        /// <include file="LiveHashDeleteIntegrationTest.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.Integration.LiveHashDeleteIntegrationTest.CannotDeleteLiveHashBecauseItsNotSupported"]' />
        public virtual void CannotDeleteLiveHashBecauseItsNotSupported()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(1),
				
                }.Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;

                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new LiveHashDeleteTransaction
                    {
						AccountId = accountId,
                        Hash = HASH
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); 
                
                Assert.Contains(ResponseStatus.NotSupported.ToString(), exception.Message);
            }
        }
    }
}
