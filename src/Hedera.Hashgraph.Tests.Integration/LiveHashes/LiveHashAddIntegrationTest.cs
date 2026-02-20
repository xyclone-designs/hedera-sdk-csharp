// SPDX-License-Identifier: Apache-2.0
using System;

using Org.BouncyCastle.Utilities.Encoders;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.LiveHashes;

using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class LiveHashAddIntegrationTest
    {
        private static readonly byte[] HASH = Hex.Decode("100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002");
        public virtual void CannotCreateLiveHashBecauseItsNotSupported()
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
                    new LiveHashAddTransaction
                    {
						AccountId = accountId,
						Duration = Duration.FromTimeSpan(TimeSpan.FromDays(30)),
						Hash = HASH,
						Keys = [key]
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.NotSupported.ToString(), exception.Message);
            }
        }
    }
}