// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenRevokeKycIntegrationTest
    {
        public virtual void CanRevokeKycAccountWithToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction()
                {
					KeyWithoutAlias = key,
					InitialBalance = new Hbar(1),
				}
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
					FreezeDefault = false,
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
                new TokenRevokeKycTransaction
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

        public virtual void CannotRevokeKycToAccountOnTokenWhenTokenIDIsNotSet()
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
                    new TokenRevokeKycTransaction
                    {
						AccountId = accountId
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(key)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        public virtual void CannotRevokeKycToAccountOnTokenWhenAccountIDIsNotSet()
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
					FreezeDefault = false,
				}
                .Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenRevokeKycTransaction
                    {
						TokenId = tokenId
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(key)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
            }
        }

        public virtual void CannotRevokeKycToAccountOnTokenWhenAccountWasNotAssociatedWith()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					KeyWithoutAlias = key,
					InitialBalance = new Hbar(1),
				}
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
					FreezeDefault = false,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenRevokeKycTransaction
                    {
						AccountId = accountId,
						TokenId = tokenId,
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(key)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.TOKEN_NOT_ASSOCIATED_TO_ACCOUNT.ToString());
            }
        }
    }
}