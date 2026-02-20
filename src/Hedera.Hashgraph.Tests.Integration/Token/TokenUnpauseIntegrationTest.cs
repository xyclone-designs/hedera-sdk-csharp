// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TokenUnpauseIntegrationTest
    {
        public virtual void CanExecuteTokenUnpauseTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var testTokenAmount = 10L;
                var accountId = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(2),
				}
                Key = accountKey,
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
                    WipeKey = testEnv.OperatorKey,
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
                .Sign(accountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new TokenUnpauseTransaction
                {
					TokenId = tokenId
				}
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new TransferTransaction()
                    .AddTokenTransfer(tokenId, accountId, testTokenAmount)
                    .AddTokenTransfer(tokenId, testEnv.OperatorId, -testTokenAmount)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                new TokenWipeTransaction
                {
					TokenId = tokenId,
					AccountId = accountId,
					Amount = (ulong)testTokenAmount,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new TokenDeleteTransaction
                {
					TokenId = tokenId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new AccountDeleteTransaction
                {
					TransferAccountId = testEnv.OperatorId,
					AccountId = accountId,
				}
                .FreezeWith(testEnv.Client)
                .Sign(accountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotUnpauseWithNoTokenId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUnpauseTransaction()
                        .Execute(testEnv.Client)
                        .GetReceipt(testEnv.Client);
                });
            }
        }
    }
}