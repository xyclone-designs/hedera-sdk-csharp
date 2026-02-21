// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TokenPauseIntegrationTest
    {
        public virtual void CanExecuteTokenPauseTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var testTokenAmount = 10;
                var accountId = new AccountCreateTransaction
                {
					Key = accountKey,
					InitialBalance = new Hbar(2)
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					InitialSupply = 1000000,
					Decimals = 3,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
                    PauseKey = testEnv.OperatorKey,
					FreezeDefault = false,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId]
				}
                .FreezeWith(testEnv.Client)
                .Sign(accountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TransferTransaction()
                    .AddTokenTransfer(tokenId, accountId, testTokenAmount)
                    .AddTokenTransfer(tokenId, testEnv.OperatorId, -testTokenAmount)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                new TokenPauseTransaction
                {
					TokenId = tokenId
				}
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TransferTransaction()
                        .AddTokenTransfer(tokenId, accountId, testTokenAmount)
                        .AddTokenTransfer(tokenId, testEnv.OperatorId, -testTokenAmount)
                        .FreezeWith(testEnv.Client)
                        .Sign(accountKey)
                        .Execute(testEnv.Client)
                        .GetReceipt(testEnv.Client);
                });
            }
        }

        public virtual void CannotPauseWithNoTokenId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenPauseTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
            }
        }
    }
}