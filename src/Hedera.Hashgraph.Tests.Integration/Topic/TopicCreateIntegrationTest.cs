// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Hedera.Hashgraph;
using Proto;
using Java.Util;
using Org.Junit.Jupiter.Api;
using Org.Junit.Jupiter.Api.Function;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Fees;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TopicCreateIntegrationTest
    {
        public virtual void CanCreateTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                new TopicCreateTransaction()
                    .SetTopicMemo("[e2e::TopicCreateTransaction]")
                .Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var response = new TopicCreateTransaction()
                    AdminKey = testEnv.OperatorKey,
                    .SetTopicMemo("[e2e::TopicCreateTransaction]")
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                new TopicDeleteTransaction()
                    .SetTopicId(topicId)
                .Execute(testEnv.Client).GetReceipt(testEnv.Client);
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
                IList<Key> feeExemptKeys = new List(List.Of(PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA()));
                var denominatingTokenId1 = CreateToken(testEnv);
                var amount1 = 1;
                var denominatingTokenId2 = CreateToken(testEnv);
                var amount2 = 2;
                var customFixedFees = List.Of(new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetDenominatingTokenId(denominatingTokenId1)
                    .SetAmount(amount1), new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetDenominatingTokenId(denominatingTokenId2)
                    .SetAmount(amount2));

                // Create revenue-generating topic
                var response = new TopicCreateTransaction()
                    .SetFeeScheduleKey(testEnv.OperatorKey)
                    .SetSubmitKey(testEnv.OperatorKey)
                    AdminKey = testEnv.OperatorKey,
                    .SetFeeExemptKeys(feeExemptKeys)
                    .SetCustomFees(customFixedFees)
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;

                // Get Topic Info
                var info = new TopicInfoQuery()
                    .SetTopicId(topicId)
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
                    Assert.Equal(info.CustomFees[i].GetAmount(), customFixedFees[i].GetAmount());
                    Assert.Equal(info.CustomFees[i].GetDenominatingTokenId(), customFixedFees[i].GetDenominatingTokenId());
                }


                // Update the revenue-generating topic
                IList<Key> newFeeExemptKeys = List.Of(PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA());
                var newFeeScheduleKey = PrivateKey.GenerateECDSA();
                var newAmount1 = 3;
                var newDenominatingTokenId1 = CreateToken(testEnv);
                var newAmount2 = 4;
                var newDenominatingTokenId2 = CreateToken(testEnv);
                var newCustomFixedFees = new List(List.Of(new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetAmount(newAmount1)
                    .SetDenominatingTokenId(newDenominatingTokenId1), new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetAmount(newAmount2)
                    .SetDenominatingTokenId(newDenominatingTokenId2)));
                var updateResponse = new TopicUpdateTransaction()
                    .SetTopicId(topicId)
                    .SetFeeExemptKeys(newFeeExemptKeys)
                    .SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey())
                    .SetCustomFees(newCustomFixedFees)
                .Execute(testEnv.Client);
                updateResponse.GetReceipt(testEnv.Client);
                var updatedInfo = new TopicInfoQuery()
                    .SetTopicId(topicId)
                .Execute(testEnv.Client);
                Assert.Equal(updatedInfo.feeScheduleKey, newFeeScheduleKey.GetPublicKey());
                for (int i = 0; i < newFeeExemptKeys.Count; i++)
                {
                    var key = (PrivateKey)newFeeExemptKeys[i];
                    PublicKey publicKey = key.GetPublicKey();
                    Assert.Equal(updatedInfo.feeExemptKeys[i], publicKey);
                }

                for (int i = 0; i < newCustomFixedFees.Count; i++)
                {
                    Assert.Equal(updatedInfo.customFees[i].GetAmount(), newCustomFixedFees[i].GetAmount());
                    Assert.Equal(updatedInfo.customFees[i].GetDenominatingTokenId(), newCustomFixedFees[i].GetDenominatingTokenId());
                }
            }
        }

        public virtual void FailsToCreateRevenueGeneratingTopicWithInvalidFeeExemptKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var feeExemptKey = PrivateKey.GenerateECDSA();
                IList<Key> feeExemptKeyListWithDuplicates = List.Of(feeExemptKey, feeExemptKey);
                Executable duplicatesExecutable = () => new TopicCreateTransaction()
                AdminKey = testEnv.OperatorKey,
                .SetFeeExemptKeys(feeExemptKeyListWithDuplicates)
            .Execute(testEnv.Client);

                // Expect failure due to duplicated fee exempt keys
                Assert.Throws<PrecheckStatusException>(duplicatesExecutable, ResponseCodeEnum.FEE_EXEMPT_KEY_LIST_CONTAINS_DUPLICATED_KEYS.Name());
                var invalidKey = PublicKey.FromString("000000000000000000000000000000000000000000000000000000000000000000");
                Executable invalidKeyExecutable = () => new TopicCreateTransaction()
                AdminKey = testEnv.OperatorKey,
                .SetFeeExemptKeys(new List(List.Of(invalidKey)))
            .Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // Expect failure due to invalid fee exempt key
                Assert.Throws<ReceiptStatusException>(invalidKeyExecutable, ResponseCodeEnum.INVALID_KEY_IN_FEE_EXEMPT_KEY_LIST.Name());

                // Create 11 keys (exceeding the limit of 10)
                IList<Key> feeExemptKeyListExceedingLimit = List.Of(PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA());
                Executable exceedKeyListLimitExecutable = () => new TopicCreateTransaction()
                AdminKey = testEnv.OperatorKey,
                .SetFeeExemptKeys(feeExemptKeyListExceedingLimit)
            .Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // Expect failure due to exceeding fee exempt key list limit
                Assert.Throws<ReceiptStatusException>(exceedKeyListLimitExecutable, ResponseCodeEnum.MAX_ENTRIES_FOR_FEE_EXEMPT_KEY_LIST_EXCEEDED.Name());
            }
        }

        public virtual void FailsToUpdateFeeScheduleKeyWithoutPermissions()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction()
                    AdminKey = testEnv.OperatorKey,
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var newFeeScheduleKey = PrivateKey.GenerateECDSA();
                Executable updateExecutable = () => new TopicUpdateTransaction()
                .SetTopicId(topicId)
                .SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey())
            .Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Throws<ReceiptStatusException>(updateExecutable, ResponseCodeEnum.FEE_SCHEDULE_KEY_CANNOT_BE_UPDATED.Name());
            }
        }

        public virtual void FailsToUpdateCustomFeesWithoutFeeScheduleKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction()
                    AdminKey = testEnv.OperatorKey,
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var denominatingTokenId1 = CreateToken(testEnv);
                var denominatingTokenId2 = CreateToken(testEnv);
                var customFees = List.Of(new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetDenominatingTokenId(denominatingTokenId1)
                    .SetAmount(1), new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetDenominatingTokenId(denominatingTokenId2)
                    .SetAmount(2));
                Executable updateExecutable = () => new TopicUpdateTransaction()
                .SetTopicId(topicId)
                .SetCustomFees(customFees)
            .Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Throws<ReceiptStatusException>(updateExecutable, ResponseCodeEnum.FEE_SCHEDULE_KEY_NOT_SET.Name());
            }
        }

        public virtual void ChargesHbarFeesWithLimitsApplied()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbarAmount = 100000000;
                var privateKey = PrivateKey.GenerateECDSA();
                var customFixedFee = new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetAmount(hbarAmount / 2);
                var response = new TopicCreateTransaction()
                    AdminKey = testEnv.OperatorKey,
                    .SetFeeScheduleKey(testEnv.OperatorKey).AddCustomFee(customFixedFee)
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var accountId = CreateAccount(testEnv, new Hbar(1), privateKey);
                ClientSetOperator(testEnv, accountId, privateKey);
                new TopicMessageSubmitTransaction()
                    .SetTopicId(topicId)
                    .SetMessage("Hedera HBAR Fee Test")
                .Execute(testEnv.Client).GetReceipt(testEnv.Client);
                ClientSetOperator(testEnv, accountId);
                var balance = new AccountBalanceQuery()
                    .SetAccountId(accountId)
                .Execute(testEnv.Client).hbars;
                AssertThat(balance.ToTinybars()).IsLessThan(hbarAmount / 2);
            }
        }

        public virtual void ExemptsFeeExemptKeysFromHbarFees()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbarAmount = 100000000;
                var feeExemptKey1 = PrivateKey.GenerateECDSA();
                var feeExemptKey2 = PrivateKey.GenerateECDSA();
                var customFixedFee = new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetAmount(hbarAmount / 2);
                var response = new TopicCreateTransaction()
                    AdminKey = testEnv.OperatorKey,
                    .SetFeeScheduleKey(testEnv.OperatorKey)
                    .SetFeeExemptKeys(List.Of(feeExemptKey1.GetPublicKey(), feeExemptKey2.GetPublicKey())).AddCustomFee(customFixedFee)
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var payerAccountId = CreateAccount(testEnv, new Hbar(1), feeExemptKey1);
                ClientSetOperator(testEnv, payerAccountId, feeExemptKey1);
                new TopicMessageSubmitTransaction()
                    .SetTopicId(topicId)
                    .SetMessage("Hedera Fee Exemption Test")
                .Execute(testEnv.Client).GetReceipt(testEnv.Client);
                ClientSetOperator(testEnv, payerAccountId);
                var balance = new AccountBalanceQuery()
                    .SetAccountId(payerAccountId)
                .Execute(testEnv.Client).hbars;
                AssertThat(balance.ToTinybars()).IsGreaterThan(hbarAmount / 2);
            }
        }

        public virtual void CreateTopicTransactionShouldAssignAutomaticallyAutoRenewAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var topicId = new TopicCreateTransaction()
                .Execute(testEnv.Client).GetReceipt(testEnv.Client).topicId;
                var autoRenewAccountId = new TopicInfoQuery()
                    .SetTopicId(topicId)
                .Execute(testEnv.Client).autoRenewAccountId;
                Assert.NotNull(autoRenewAccountId);
            }
        }

        public virtual void CreateTopicTransactionWithTransactionIdShouldAssignAutoRenewAccountIdToTransactionIdAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var accountId = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(publicKey)
                    .SetInitialBalance(Hbar.From(10))
                .Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                var topicId = new TopicCreateTransaction()
                    .SetTransactionId(TransactionId.Generate(accountId)).FreezeWith(testEnv.Client).Sign(privateKey)
                .Execute(testEnv.Client).GetReceipt(testEnv.Client).topicId;
                var autoRenewAccountId = new TopicInfoQuery()
                    .SetTopicId(topicId)
                .Execute(testEnv.Client).autoRenewAccountId;
                Assert.Equal(autoRenewAccountId, accountId);
            }
        }

        private AccountId CreateAccount(IntegrationTestEnv testEnv, Hbar initialBalance, PrivateKey key)
        {
            return new AccountCreateTransaction()
                .SetInitialBalance(initialBalance)
                .SetKeyWithoutAlias(key)
            .Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
        }

        private void ClientSetOperator(IntegrationTestEnv testEnv, AccountId accountId)
        {
            testEnv.Client
                .OperatorSet(accountId, PrivateKey.GenerateECDSA());
        }

        private void ClientSetOperator(IntegrationTestEnv testEnv, AccountId accountId, PrivateKey key)
        {
            testEnv.Client
                .OperatorSet(accountId, key);
        }

        private TokenId CreateToken(IntegrationTestEnv testEnv)
        {
            var tokenCreateResponse = new TokenCreateTransaction()
                .SetTokenName("Test Token")
                .SetTokenSymbol("TT")
                TreasuryAccountId = testEnv.OperatorId,
                InitialSupply = 1000000,
                .SetDecimals(2)
                AdminKey = testEnv.OperatorKey,
                SupplyKey = testEnv.OperatorKey,
            .Execute(testEnv.Client);
            return tokenCreateResponse.GetReceipt(testEnv.Client).TokenId;
        }

        public virtual void CanClearCustomFeesListAndFeeExemptKeysList()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                IList<Key> feeExemptKeys = new List(List.Of(PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA()));
                var denominatingTokenId1 = CreateToken(testEnv);
                var amount1 = 1;
                var denominatingTokenId2 = CreateToken(testEnv);
                var amount2 = 2;
                var customFixedFees = List.Of(new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetDenominatingTokenId(denominatingTokenId1)
                    .SetAmount(amount1), new CustomFixedFee()
                    .SetFeeCollectorAccountId(testEnv.OperatorId)
                    .SetDenominatingTokenId(denominatingTokenId2)
                    .SetAmount(amount2));

                // Create revenue-generating topic
                var response = new TopicCreateTransaction()
                    .SetFeeScheduleKey(testEnv.OperatorKey)
                    .SetSubmitKey(testEnv.OperatorKey)
                    AdminKey = testEnv.OperatorKey,
                    .SetFeeExemptKeys(feeExemptKeys)
                    .SetCustomFees(customFixedFees)
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;

                // Get Topic Info
                var info = new TopicInfoQuery()
                    .SetTopicId(topicId)
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
                    Assert.Equal(info.CustomFees[i].GetAmount(), customFixedFees[i].GetAmount());
                    Assert.Equal(info.CustomFees[i].GetDenominatingTokenId(), customFixedFees[i].GetDenominatingTokenId());
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
                IList<Key> feeExemptKeys = [ PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA() ];
                var denominatingTokenId1 = CreateToken(testEnv);
                var amount1 = 1;
                var denominatingTokenId2 = CreateToken(testEnv);
                var amount2 = 2;
                var customFixedFees = 
                [
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
				];

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
                    Assert.Equal(info.CustomFees[i].GetAmount(), customFixedFees[i].GetAmount());
                    Assert.Equal(info.CustomFees[i].GetDenominatingTokenId(), customFixedFees[i].GetDenominatingTokenId());
                }

                new TopicUpdateTransaction()
                    .SetTopicId(topicId)
                .Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var sameTopic = new TopicInfoQuery()
                    .SetTopicId(topicId)
                .Execute(testEnv.Client);
                Assert.Equal(sameTopic.FeeExemptKeys, info.FeeExemptKeys);
                Assert.Equal(sameTopic.FeeScheduleKey, info.FeeScheduleKey);
                Assert.Equal(sameTopic.CustomFees[0].GetAmount(), info.CustomFees[0].GetAmount());
            }
        }
    }
}