// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenUnfreezeIntegrationTest
    {
        public virtual void CanUnfreezeAccountWithToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					KeyWithoutAlias = key,
					InitialBalance = new Hbar(1),

				}.Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    Decimals = 3,
                    InitialSupply = 1000000,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    FreezeKey = testEnv.OperatorKey,
                    WipeKey = testEnv.OperatorKey,
                    KycKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                    FreezeDefault = false
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId],
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenUnfreezeTransaction
                {
					AccountId = accountId,
					TokenId = tokenId,
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotUnfreezeAccountOnTokenWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					KeyWithoutAlias = key,
					InitialBalance = new Hbar(1),
				
                }.Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenUnfreezeTransaction()
                        .SetAccountId(accountId)
                        .FreezeWith(testEnv.Client)
                        .Sign(key)
                        .Execute(testEnv.Client)
                        .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        public virtual void CannotUnfreezeAccountOnTokenWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new TokenCreateTransaction
                { 
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    Decimals = 3,
                    InitialSupply = 1000000,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    FreezeKey = testEnv.OperatorKey,
                    WipeKey = testEnv.OperatorKey,
                    KycKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                    FreezeDefault = false
               
                }.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenUnfreezeTransaction()
                        .SetTokenId(tokenId)
                        .FreezeWith(testEnv.Client)
                        .Sign(key)
                        .Execute(testEnv.Client)
                        .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
            }
        }

        public virtual void CannotUnfreezeAccountOnTokenWhenAccountWasNotAssociatedWith()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)
				}
                .SetKeyWithoutAlias(key)
                .Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                { 
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    Decimals = 3,
                    InitialSupply = 1000000,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    FreezeKey = testEnv.OperatorKey,
                    WipeKey = testEnv.OperatorKey,
                    KycKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                    FreezeDefault = false
               
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenUnfreezeTransaction()
                        .SetAccountId(accountId)
                        .SetTokenId(tokenId)
                        .FreezeWith(testEnv.Client)
                        .Sign(key)
                        .Execute(testEnv.Client)
                        .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.TOKEN_NOT_ASSOCIATED_TO_ACCOUNT.ToString());
            }
        }
    }
}