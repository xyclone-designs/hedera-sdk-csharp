// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TopicCreateIntegrationTest
    {
        public virtual void CanCreateTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                new TopicCreateTransaction
                {
					TopicMemo = "[e2e::TopicCreateTransaction]"
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					TopicMemo = "[e2e::TopicCreateTransaction]",
				}
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                new TopicDeleteTransaction
                {
					TopicId = topicId,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
        public virtual void CanCreateTopicWithNoFieldsSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction()
                .Execute(testEnv.Client);
                Assert.NotNull(response.GetReceipt(testEnv.Client).TopicId);
            }
        }
        public virtual void CreatesAndUpdatesRevenueGeneratingTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                IList<Key> feeExemptKeys = [PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA()];
                var denominatingTokenId1 = CreateToken(testEnv);
                var amount1 = 1;
                var denominatingTokenId2 = CreateToken(testEnv);
                var amount2 = 2;
                var customFixedFees = new List<CustomFixedFee>
                {
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = denominatingTokenId1,
						Amount = amount1,
					},
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = denominatingTokenId2,
						Amount = amount2,
					}
				};

                // Create revenue-generating topic
                var response = new TopicCreateTransaction
                {
					FeeScheduleKey = testEnv.OperatorKey,
					SubmitKey = testEnv.OperatorKey,
					AdminKey = testEnv.OperatorKey,
					FeeExemptKeys = feeExemptKeys,
					CustomFees = customFixedFees,
				}
                .Execute(testEnv.Client);

                var topicId = response.GetReceipt(testEnv.Client).TopicId;

                // Get Topic Info
                var info = new TopicInfoQuery
                {
					TopicId = topicId,
				}
                .Execute(testEnv.Client);

                Assert.Equal(info.FeeScheduleKey, testEnv.OperatorKey);

                // Validate fee exempt keys
                for (int i = 0; i < feeExemptKeys.Count; i++)
                {
                    var key = (PrivateKey)feeExemptKeys[i];
                    PublicKey publicKey = key.GetPublicKey();
                    Assert.Equal(info.FeeExemptKeys[i], publicKey);
                }

                // Validate custom fees
                for (int i = 0; i < customFixedFees.Count; i++)
                {
                    Assert.Equal(info.CustomFees[i].Amount, customFixedFees[i].Amount);
                    Assert.Equal(info.CustomFees[i].DenominatingTokenId, customFixedFees[i].DenominatingTokenId);
                }


                // Update the revenue-generating topic
                IList<Key> newFeeExemptKeys = [PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA()];
                var newFeeScheduleKey = PrivateKey.GenerateECDSA();
                var newAmount1 = 3;
                var newDenominatingTokenId1 = CreateToken(testEnv);
                var newAmount2 = 4;
                var newDenominatingTokenId2 = CreateToken(testEnv);
                var newCustomFixedFees = new List<CustomFixedFee>
                {
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = newDenominatingTokenId1,
						Amount = newAmount1,
					},
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = newDenominatingTokenId2,
						Amount = newAmount2,
					}
				};

				var updateResponse = new TopicUpdateTransaction
                {
					TopicId = topicId,
					FeeExemptKeys = newFeeExemptKeys,
					FeeScheduleKey = newFeeScheduleKey.GetPublicKey(),
					CustomFees = newCustomFixedFees,
				
                }.Execute(testEnv.Client);
                
                updateResponse.GetReceipt(testEnv.Client);
                
                var updatedInfo = new TopicInfoQuery
                {
					TopicId = topicId,
				
                }.Execute(testEnv.Client);
                
                Assert.Equal(updatedInfo.FeeScheduleKey, newFeeScheduleKey.GetPublicKey());

                for (int i = 0; i < newFeeExemptKeys.Count; i++)
                {
                    var key = (PrivateKey)newFeeExemptKeys[i];
                    PublicKey publicKey = key.GetPublicKey();
                    Assert.Equal(updatedInfo.FeeExemptKeys[i], publicKey);
                }

                for (int i = 0; i < newCustomFixedFees.Count; i++)
                {
                    Assert.Equal(updatedInfo.CustomFees[i].Amount, newCustomFixedFees[i].Amount);
                    Assert.Equal(updatedInfo.CustomFees[i].DenominatingTokenId, newCustomFixedFees[i].DenominatingTokenId);
                }
            }
        }
        public virtual void FailsToCreateRevenueGeneratingTopicWithInvalidFeeExemptKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var feeExemptKey = PrivateKey.GenerateECDSA();

                IList<Key> feeExemptKeyListWithDuplicates = [feeExemptKey, feeExemptKey];

				Action duplicatesExecutable = () => new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					FeeExemptKeys = feeExemptKeyListWithDuplicates,
				
                }.Execute(testEnv.Client);

                // Expect failure due to duplicated fee exempt keys
                Assert.Throws<PrecheckStatusException>(duplicatesExecutable, Proto.ResponseCodeEnum.FeeExemptKeyListContainsDuplicatedKeys.Name());

                var invalidKey = PublicKey.FromString("000000000000000000000000000000000000000000000000000000000000000000");

				Action invalidKeyExecutable = () => new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					FeeExemptKeys = [invalidKey],

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // Expect failure due to invalid fee exempt key
                Assert.Throws<ReceiptStatusException>(invalidKeyExecutable, Proto.ResponseCodeEnum.InvalidKeyInFeeExemptKeyList.Name());

                // Create 11 keys (exceeding the limit of 10)
                IList<Key> feeExemptKeyListExceedingLimit = [.. Enumerable.Range(0, 11).Select(_ => PrivateKey.GenerateECDSA())];

                Action exceedKeyListLimitExecutable = () => new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					FeeExemptKeys = feeExemptKeyListExceedingLimit
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // Expect failure due to exceeding fee exempt key list limit
                Assert.Throws<ReceiptStatusException>(exceedKeyListLimitExecutable, Proto.ResponseCodeEnum.MaxEntriesForFeeExemptKeyListExceeded.Name());
            }
        }
        public virtual void FailsToUpdateFeeScheduleKeyWithoutPermissions()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
				
                }.Execute(testEnv.Client);
                
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var newFeeScheduleKey = PrivateKey.GenerateECDSA();

				Action updateExecutable = () => new TopicUpdateTransaction
                {
					TopicId = topicId,
                    FeeScheduleKey = newFeeScheduleKey.GetPublicKey()
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                Assert.Throws<ReceiptStatusException>(updateExecutable, Proto.ResponseCodeEnum.FeeScheduleKeyCannotBeUpdated.Name());
            }
        }
        public virtual void FailsToUpdateCustomFeesWithoutFeeScheduleKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,

				}.Execute(testEnv.Client);

                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var denominatingTokenId1 = CreateToken(testEnv);
                var denominatingTokenId2 = CreateToken(testEnv);
				var customFees = new List<CustomFixedFee>
                {
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = denominatingTokenId1,
						Amount = 1,
					},
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = denominatingTokenId2,
						Amount = 2,
					}
				};

				Action updateExecutable = () => new TopicUpdateTransaction
                {
					TopicId = topicId,
					CustomFees = customFees,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                Assert.Throws<ReceiptStatusException>(updateExecutable, Proto.ResponseCodeEnum.FeeScheduleKeyNotSet.Name());
            }
        }
        public virtual void ChargesHbarFeesWithLimitsApplied()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbarAmount = 100000000;
                var privateKey = PrivateKey.GenerateECDSA();
                var customFixedFee = new CustomFixedFee
                {
                    FeeCollectorAccountId = testEnv.OperatorId,
                    Amount = hbarAmount / 2,
                };
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					FeeScheduleKey = testEnv.OperatorKey,
                    CustomFees = [customFixedFee]
				}
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var accountId = CreateAccount(testEnv, new Hbar(1), privateKey);
                ClientSetOperator(testEnv, accountId, privateKey);
                new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					Message = "Hedera HBAR Fee Test"

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                ClientSetOperator(testEnv, accountId);

                var balance = new AccountBalanceQuery
                {
					AccountId = accountId,
				
                }.Execute(testEnv.Client).Hbars;

                Assert.True(balance.ToTinybars() < hbarAmount / 2);
            }
        }
        public virtual void ExemptsFeeExemptKeysFromHbarFees()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbarAmount = 100000000;
                var feeExemptKey1 = PrivateKey.GenerateECDSA();
                var feeExemptKey2 = PrivateKey.GenerateECDSA();
                var customFixedFee = new CustomFixedFee
                {
					FeeCollectorAccountId = testEnv.OperatorId,
					Amount = hbarAmount / 2,
				};
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					FeeScheduleKey = testEnv.OperatorKey,
					FeeExemptKeys = [feeExemptKey1.GetPublicKey(), feeExemptKey2.GetPublicKey()],
					CustomFees = [customFixedFee]
				}
                .Execute(testEnv.Client);

                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var payerAccountId = CreateAccount(testEnv, new Hbar(1), feeExemptKey1);
                
                ClientSetOperator(testEnv, payerAccountId, feeExemptKey1);

                new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					Message = "Hedera Fee Exemption Test",
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                ClientSetOperator(testEnv, payerAccountId);
                var balance = new AccountBalanceQuery
                {
					AccountId = payerAccountId
				
                }.Execute(testEnv.Client).Hbars;

                Assert.True(balance.ToTinybars() > hbarAmount / 2);
            }
        }
        public virtual void CreateTopicTransactionShouldAssignAutomaticallyAutoRenewAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var topicId = new TopicCreateTransaction()
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client).TopicId;

                var autoRenewAccountId = new TopicInfoQuery
                {
					TopicId = topicId,
				
                }.Execute(testEnv.Client).AutoRenewAccountId;

                Assert.NotNull(autoRenewAccountId);
            }
        }
        public virtual void CreateTopicTransactionWithTransactionIdShouldAssignAutoRenewAccountIdToTransactionIdAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var accountId = new AccountCreateTransaction
                {
					Key = publicKey,
					InitialBalance = Hbar.From(10),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                var topicId = new TopicCreateTransaction
                {
					TransactionId = TransactionId.Generate(accountId)
				}
                .FreezeWith(testEnv.Client)
                .Sign(privateKey)
                .Execute(testEnv.Client).GetReceipt(testEnv.Client).TopicId;

                var autoRenewAccountId = new TopicInfoQuery
                {
					TopicId = topicId,
				
                }.Execute(testEnv.Client).AutoRenewAccountId;

                Assert.Equal(autoRenewAccountId, accountId);
            }
        }

		public virtual void CanClearCustomFeesListAndFeeExemptKeysList()
		{
			using (var testEnv = new IntegrationTestEnv(1))
			{
				IList<Key> feeExemptKeys = [PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA()];
				var denominatingTokenId1 = CreateToken(testEnv);
				var amount1 = 1;
				var denominatingTokenId2 = CreateToken(testEnv);
				var amount2 = 2;
				var customFixedFees = new List<CustomFixedFee>
				{
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = denominatingTokenId1,
						Amount = amount1,
					},
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = denominatingTokenId2,
						Amount = amount2,
					}
				};

				// Create revenue-generating topic
				var response = new TopicCreateTransaction
				{
					FeeScheduleKey = testEnv.OperatorKey,
					SubmitKey = testEnv.OperatorKey,
					AdminKey = testEnv.OperatorKey,
					FeeExemptKeys = feeExemptKeys,
					CustomFees = customFixedFees

				}.Execute(testEnv.Client);
				var topicId = response.GetReceipt(testEnv.Client).TopicId;

				// Get Topic Info
				var info = new TopicInfoQuery
				{
					TopicId = topicId,

				}.Execute(testEnv.Client);
				Assert.Equal(info.FeeScheduleKey, testEnv.OperatorKey);

				// Validate fee exempt keys
				for (int i = 0; i < feeExemptKeys.Count; i++)
				{
					var key = (PrivateKey)feeExemptKeys[i];
					PublicKey publicKey = key.GetPublicKey();
					Assert.Equal(info.FeeExemptKeys[i], publicKey);
				}

				// Validate custom fees
				for (int i = 0; i < customFixedFees.Count; i++)
				{
					Assert.Equal(info.CustomFees[i].Amount, customFixedFees[i].Amount);
					Assert.Equal(info.CustomFees[i].DenominatingTokenId, customFixedFees[i].DenominatingTokenId);
				}

				var newFeeScheduleKey = PrivateKey.GenerateECDSA();
				new TopicUpdateTransaction
				{
					TopicId = topicId,
					FeeExemptKeys = null,
					FeeScheduleKey = null,
					CustomFees = null,
				}
				.FreezeWith(testEnv.Client)
				.Sign(newFeeScheduleKey)
				.Execute(testEnv.Client).GetReceipt(testEnv.Client);

				var cleared = new TopicInfoQuery
				{
					TopicId = topicId
				}
				.Execute(testEnv.Client);

				Assert.Empty(cleared.FeeExemptKeys);
				Assert.Null(cleared.FeeScheduleKey);
				Assert.Empty(cleared.CustomFees);
			}
		}
		public virtual void CanUpdateTopicWithoutSpecifyingAnythingTopicShouldHaveTheSameValues()
		{
			using (var testEnv = new IntegrationTestEnv(1))
			{
				IList<Key> feeExemptKeys = [PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA()];
				var denominatingTokenId1 = CreateToken(testEnv);
				var amount1 = 1;
				var denominatingTokenId2 = CreateToken(testEnv);
				var amount2 = 2;
				var customFixedFees = new List<CustomFixedFee>
				{
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = denominatingTokenId1,
						Amount = amount1
					},
					new CustomFixedFee
					{
						FeeCollectorAccountId = testEnv.OperatorId,
						DenominatingTokenId = denominatingTokenId2,
						Amount = amount2
					}
				};

				// Create revenue-generating topic
				var response = new TopicCreateTransaction
				{
					FeeScheduleKey = testEnv.OperatorKey,
					SubmitKey = testEnv.OperatorKey,
					AdminKey = testEnv.OperatorKey,
					FeeExemptKeys = feeExemptKeys,
					CustomFees = customFixedFees,
				}
				.Execute(testEnv.Client);

				var topicId = response.GetReceipt(testEnv.Client).TopicId;

				// Get Topic Info
				var info = new TopicInfoQuery
				{
					TopicId = topicId
				}
				.Execute(testEnv.Client);

				Assert.Equal(info.FeeScheduleKey, testEnv.OperatorKey);

				// Validate fee exempt keys
				for (int i = 0; i < feeExemptKeys.Count; i++)
				{
					var key = (PrivateKey)feeExemptKeys[i];
					PublicKey publicKey = key.GetPublicKey();
					Assert.Equal(info.FeeExemptKeys[i], publicKey);
				}

				// Validate custom fees
				for (int i = 0; i < customFixedFees.Count; i++)
				{
					Assert.Equal(info.CustomFees[i].Amount, customFixedFees[i].Amount);
					Assert.Equal(info.CustomFees[i].DenominatingTokenId, customFixedFees[i].DenominatingTokenId);
				}

				new TopicUpdateTransaction
				{
					TopicId = topicId
				}
				.Execute(testEnv.Client).GetReceipt(testEnv.Client);

				var sameTopic = new TopicInfoQuery
				{
					TopicId = topicId
				}
				.Execute(testEnv.Client);

				Assert.Equal(sameTopic.FeeExemptKeys, info.FeeExemptKeys);
				Assert.Equal(sameTopic.FeeScheduleKey, info.FeeScheduleKey);
				Assert.Equal(sameTopic.CustomFees[0].Amount, info.CustomFees[0].Amount);
			}
		}

		private TokenId CreateToken(IntegrationTestEnv testEnv)
		{
			var tokenCreateResponse = new TokenCreateTransaction
			{
				TokenName = "Test Token",
				TokenSymbol = "TT",
				TreasuryAccountId = testEnv.OperatorId,
				InitialSupply = 1000000,
				Decimals = 2,
				AdminKey = testEnv.OperatorKey,
				SupplyKey = testEnv.OperatorKey,

			}.Execute(testEnv.Client);

			return tokenCreateResponse.GetReceipt(testEnv.Client).TokenId;
		}
		private AccountId CreateAccount(IntegrationTestEnv testEnv, Hbar initialBalance, PrivateKey key)
        {
            return new AccountCreateTransaction
            {
				InitialBalance = initialBalance,
				Key = key,
			
            }.Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
        }

        private void ClientSetOperator(IntegrationTestEnv testEnv, AccountId accountId)
        {
            testEnv.Client.OperatorSet(accountId, PrivateKey.GenerateECDSA());
        }
        private void ClientSetOperator(IntegrationTestEnv testEnv, AccountId accountId, PrivateKey key)
        {
            testEnv.Client.OperatorSet(accountId, key);
        }
    }
}