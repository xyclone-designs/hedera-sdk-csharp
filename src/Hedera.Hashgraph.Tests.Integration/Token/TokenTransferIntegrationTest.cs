// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenTransferIntegrationTest
    {
        public virtual void TokenTransferTest()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                TransactionResponse response = new AccountCreateTransaction()
                {
					Key = key,
					InitialBalance = new Hbar(1),
				}
                .Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                response = new TokenCreateTransaction()
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
				}.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                Assert.NotNull(tokenId);
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId]
				}
                .FreezeWith(testEnv.Client)
                .SignWithOperator(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction
                {
					AccountId = accountId,
					TokenId = tokenId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new TransferTransaction()
                    .AddTokenTransfer(tokenId, testEnv.OperatorId, -10)
                    .AddTokenTransfer(tokenId, accountId, 10)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void InsufficientBalanceForFee()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                PrivateKey key2 = PrivateKey.GenerateED25519();
                var accountId1 = new AccountCreateTransaction()
                {
                    Key = key1,
                    InitialBalance = new Hbar(2)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;
                var accountId2 = new AccountCreateTransaction()
                {
                    Key = key2,
                    InitialBalance = new Hbar(2)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					InitialSupply = 1,
					CustomFees = [new CustomFixedFee
					{
						Amount = 5000000000,
						FeeCollectorAccountId = testEnv.OperatorId
					}],
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FeeScheduleKey = testEnv.OperatorKey,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                new TokenAssociateTransaction
                {
					AccountId = accountId1,
					TokenIds = [tokenId],
				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenAssociateTransaction
                {
					AccountId = accountId2,
					TokenIds = [tokenId],
				}
                .FreezeWith(testEnv.Client)
                .Sign(key2)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TransferTransaction()
                    .AddTokenTransfer(tokenId, testEnv.OperatorId, -1)
                    .AddTokenTransfer(tokenId, accountId1, 1)
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TransferTransaction()
                        .AddTokenTransfer(tokenId, accountId1, -1)
                        .AddTokenTransfer(tokenId, accountId2, 1)
                    .FreezeWith(testEnv.Client)
                    .Sign(key1)
                    .Sign(key2)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).Satisfies((error) => AssertThat(error.GetMessage()).ContainsAnyOf(ResponseStatus.INSUFFICIENT_SENDER_ACCOUNT_BALANCE_FOR_CUSTOM_FEE.ToString(), Status.INSUFFICIENT_PAYER_BALANCE_FOR_CUSTOM_FEE.ToString()));
            }
        }
    }
}