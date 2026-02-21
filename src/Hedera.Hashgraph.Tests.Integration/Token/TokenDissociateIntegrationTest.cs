// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenDissociateIntegrationTest
    {
        public virtual void CanAssociateAccountWithToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					Key = key,
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

                new TokenDissociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId],
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenDissociateTransactionEvenWhenTokenIDsAreNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(1),
				
                }.Execute(testEnv.Client);

                var accountId = response.GetReceipt(testEnv.Client).AccountId;

                new TokenDissociateTransaction
                {
					AccountId = accountId,
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotDissociateAccountWithTokensWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
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
                    new TokenDissociateTransaction()
                        .FreezeWith(testEnv.Client)
                        .Sign(key)
                        .Execute(testEnv.Client)
                        .GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.InvalidAccountId.ToString(), exception.Message);
            }
        }

        public virtual void CannotDissociateAccountWhenAccountDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					Key = key,
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
					FreezeDefault = false,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenDissociateTransaction
                    {
						AccountId = accountId,
						TokenIds = [tokenId],
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }

        public virtual void CannotDissociateAccountFromTokenWhenAccountWasNotAssociatedWith()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					Key = key,
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
                
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenDissociateTransaction
                    {
						AccountId = accountId,
						TokenIds = [tokenId],
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(key)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.TokenNotAssociatedToAccount.ToString(), exception.Message);
            }
        }
    }
}