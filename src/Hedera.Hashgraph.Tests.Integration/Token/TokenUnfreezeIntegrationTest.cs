// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

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
                    InitialBalance = new Hbar(1),
				}
                Key = key,
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
					InitialBalance = new Hbar(1),
                }
				Key = key,
				.Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUnfreezeTransaction
                    {
						AccountId = accountId
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(key)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidTokenId.ToString(), exception.Message);
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
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
					new TokenUnfreezeTransaction
					{
						TokenId = tokenId
					}
					.FreezeWith(testEnv.Client)
					.Sign(key)
					.Execute(testEnv.Client)
					.GetReceipt(testEnv.Client);

				}); Assert.Contains(ResponseStatus.InvalidAccountId.ToString(), exception.Message);
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
                Key = key,
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

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
					new TokenUnfreezeTransaction
					{
						AccountId = accountId,
						TokenId = tokenId,
					}
					.FreezeWith(testEnv.Client)
					.Sign(key)
					.Execute(testEnv.Client)
					.GetReceipt(testEnv.Client);

				}); Assert.Contains(ResponseStatus.TokenNotAssociatedToAccount.ToString(), exception.Message);
            }
        }
    }
}