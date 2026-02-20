// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Token;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class ScheduleTransactionIntegrationTest
    {
        public virtual void ShouldChargeHbarsWithLimitUsingScheduledTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbar = 100000000; // 1 HBAR in tinybars
                var customFixedFee = new CustomFixedFee
                {
					FeeCollectorAccountId = testEnv.Client.OperatorAccountId,
					Amount = hbar / 2
				};

                // Create a revenue generating topic
                var topicResponse = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    FeeScheduleKey = testEnv.OperatorKey,
                    CustomFees = [customFixedFee]
                }
                .Execute(testEnv.Client);
                var topicId = topicResponse.GetReceipt(testEnv.Client).TopicId;

                // Create payer account
                var payerKey = PrivateKey.GenerateED25519();
                var payerResponse = new AccountCreateTransaction
                {
					Key = payerKey,
					InitialBalance = Hbar.FromTinybars(hbar)
				}
                .Execute(testEnv.Client);
                var payerAccountId = payerResponse.GetReceipt(testEnv.Client).AccountId;
                var customFeeLimit = new CustomFeeLimit
                {
					PayerId = payerAccountId,
					CustomFees = [customFixedFee]
				};

                // Submit a message to the revenue generating topic with custom fee limit using scheduled transaction
                // Create a new client with the payer account as operator
                var payerClient = Client.ForNetwork(testEnv.Client.Network);
                payerClient.SetMirrorNetwork(testEnv.Client.GetMirrorNetwork());
                payerClient.OperatorSet(payerAccountId, payerKey);
                var submitMessageTransaction = new TopicMessageSubmitTransaction
                {
					Message = "hello!",
					TopicId = topicId,
					CustomFeeLimits = [customFeeLimit]
				};
                var scheduleResponse = submitMessageTransaction.Schedule().Execute(payerClient);
                var scheduleId = scheduleResponse.GetReceipt(payerClient).ScheduleId;

                // The scheduled transaction should execute immediately since we have all required signatures
                Assert.NotNull(scheduleId);
                payerClient.Dispose();

                var accountBalance = new AccountBalanceQuery
                {
					AccountId = payerAccountId
				}
                .Execute(testEnv.Client);

                Assert.True(accountBalance.Hbars.ToTinybars() < hbar / 2);

                // Cleanup
                new TopicDeleteTransaction
                {
					TopicId = topicId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void ShouldNotChargeHbarsWithLowerLimitUsingScheduledTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbar = 100000000; // 1 HBAR in tinybars
                var customFixedFee = new CustomFixedFee
                {
					FeeCollectorAccountId = testEnv.Client.OperatorAccountId,
					Amount = hbar / 2,
				};

                // Create a revenue generating topic with Hbar custom fee
                var topicResponse = new TopicCreateTransaction()
                {
					AdminKey = testEnv.OperatorKey,
					FeeScheduleKey = testEnv.OperatorKey,
					CustomFees = [customFixedFee],
				}
                .Execute(testEnv.Client);
                var topicId = topicResponse.GetReceipt(testEnv.Client).TopicId);

                // Create payer account
                var payerKey = PrivateKey.GenerateED25519();
                var payerResponse = new AccountCreateTransaction
                {
					Key = payerKey,
					InitialBalance = Hbar.FromTinybars(hbar),
				}
                .Execute(testEnv.Client);
                var payerAccountId = payerResponse.GetReceipt(testEnv.Client).AccountId;

                // Set custom fee limit with lower amount than the custom fee
                var customFeeLimit = new CustomFeeLimit
                {
                    PayerId = payerAccountId,
                    CustomFees = [ new CustomFixedFee
                    {
                        Amount = hbar / 2 - 1
                    }]
                };

                // Submit a message to the revenue generating topic with custom fee limit using scheduled transaction
                // Create a new client with the payer account as operator
                var payerClient = Client.ForNetwork(testEnv.Client.Network);
                payerClient.MirrorNetwork = testEnv.Client.MirrorNetwork;
                payerClient.OperatorSet(payerAccountId, payerKey);
                new TopicMessageSubmitTransaction()
                {
					Message = "Hello",
					TopicId = topicId,
					CustomFeeLimits = [customFeeLimit],
				}
                .Schedule()
                .Execute(payerClient)
                .GetReceipt(payerClient);
                var accountBalance = new AccountBalanceQuery
                {
					AccountId = payerAccountId
				}
                .Execute(testEnv.Client);
                Assert.True(accountBalance.hbars.ToTinybars() > hbar / 2);
                
                payerClient.Dispose();

                // Cleanup
                new TopicDeleteTransaction
                {
					TopicId = topicId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void ShouldNotChargeTokensWithLowerLimitUsingScheduledTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                // Create a fungible token
                var tokenResponse = new TokenCreateTransaction
                {
					TokenName = "Test Token",
					TokenSymbol = "TT",
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,
				}
                .Execute(testEnv.Client);

                var tokenId = tokenResponse.GetReceipt(testEnv.Client).TokenId;
                var customFixedFee = new CustomFixedFee
                {
					Amount = 2,
					DenominatingTokenId = tokenId,
					FeeCollectorAccountId = testEnv.Client.OperatorAccountId
				};

                // Create a revenue generating topic
                var topicResponse = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    FeeScheduleKey = testEnv.OperatorKey,
                    CustomFees = [customFixedFee]
                }
                .Execute(testEnv.Client);

                var topicId = topicResponse.GetReceipt(testEnv.Client).TopicId;

                // Create payer account with unlimited token associations
                var payerKey = PrivateKey.GenerateED25519();
                var payerResponse = new AccountCreateTransaction
                {
					Key = payerKey,
					InitialBalance = Hbar.FromTinybars(100000000),
					MaxAutomaticTokenAssociations = -1,
				}
                .Execute(testEnv.Client);

                var payerAccountId = payerResponse.GetReceipt(testEnv.Client).AccountId;

                // Send tokens to payer
                new TransferTransaction()
                    .AddTokenTransfer(tokenId, testEnv.Client.OperatorAccountId, -2)
                    .AddTokenTransfer(tokenId, payerAccountId, 2)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // Set custom fee limit with lower amount than the custom fee
                var customFeeLimit = new CustomFeeLimit()
                {
					PayerId = payerAccountId,
					CustomFees = [ new CustomFixedFee
					{
						Amount = 1,
						DenominatingTokenId = tokenId
					}])
			    };

                // Submit a message to the revenue generating topic with custom fee limit using scheduled transaction
                // Create a new client with the payer account as operator
                testEnv.Client.OperatorSet(payerAccountId, payerKey);

                new TopicMessageSubmitTransaction
                {
					Message = "Hello!",
					TopicId = topicId,
					CustomFeeLimits = [customFeeLimit],
				}
                .Schedule()
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                var accountBalance = new AccountBalanceQuery
                {
					AccountId = payerAccountId
				}
                .Execute(testEnv.Client);

                Assert.Equal(accountBalance.Tokens[tokenId], 2);
            }
        }
    }
}