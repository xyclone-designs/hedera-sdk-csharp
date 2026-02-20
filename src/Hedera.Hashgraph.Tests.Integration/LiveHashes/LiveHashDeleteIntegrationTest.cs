// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.LiveHashes;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class LiveHashDeleteIntegrationTest
    {
        private static readonly byte[] HASH = Hex.Decode("100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002");
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