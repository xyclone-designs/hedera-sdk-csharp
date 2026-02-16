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

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TopicCreateIntegrationTest
    {
        public virtual void CanCreateTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                new TopicCreateTransaction().SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client).GetReceipt(testEnv.client);
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanCreateTopicWithNoFieldsSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().Execute(testEnv.client);
                AssertThat(response.GetReceipt(testEnv.client).topicId).IsNotNull();
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
                var customFixedFees = List.Of(new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(denominatingTokenId1).SetAmount(amount1), new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(denominatingTokenId2).SetAmount(amount2));

                // Create revenue-generating topic
                var response = new TopicCreateTransaction().SetFeeScheduleKey(testEnv.operatorKey).SetSubmitKey(testEnv.operatorKey).SetAdminKey(testEnv.operatorKey).SetFeeExemptKeys(feeExemptKeys).SetCustomFees(customFixedFees).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);

                // Get Topic Info
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.feeScheduleKey, testEnv.operatorKey);

                // Validate fee exempt keys
                for (int i = 0; i < feeExemptKeys.Count; i++)
                {
                    var key = (PrivateKey)feeExemptKeys[i];
                    PublicKey publicKey = key.GetPublicKey();
                    Assert.Equal(info.feeExemptKeys[i], publicKey);
                }


                // Validate custom fees
                for (int i = 0; i < customFixedFees.Count; i++)
                {
                    Assert.Equal(info.customFees[i].GetAmount(), customFixedFees[i].GetAmount());
                    Assert.Equal(info.customFees[i].GetDenominatingTokenId(), customFixedFees[i].GetDenominatingTokenId());
                }


                // Update the revenue-generating topic
                IList<Key> newFeeExemptKeys = List.Of(PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA());
                var newFeeScheduleKey = PrivateKey.GenerateECDSA();
                var newAmount1 = 3;
                var newDenominatingTokenId1 = CreateToken(testEnv);
                var newAmount2 = 4;
                var newDenominatingTokenId2 = CreateToken(testEnv);
                var newCustomFixedFees = new List(List.Of(new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetAmount(newAmount1).SetDenominatingTokenId(newDenominatingTokenId1), new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetAmount(newAmount2).SetDenominatingTokenId(newDenominatingTokenId2)));
                var updateResponse = new TopicUpdateTransaction().SetTopicId(topicId).SetFeeExemptKeys(newFeeExemptKeys).SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey()).SetCustomFees(newCustomFixedFees).Execute(testEnv.client);
                updateResponse.GetReceipt(testEnv.client);
                var updatedInfo = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
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
                Executable duplicatesExecutable = () => new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetFeeExemptKeys(feeExemptKeyListWithDuplicates).Execute(testEnv.client);

                // Expect failure due to duplicated fee exempt keys
                Assert.Throws<PrecheckStatusException>(duplicatesExecutable, ResponseCodeEnum.FEE_EXEMPT_KEY_LIST_CONTAINS_DUPLICATED_KEYS.Name());
                var invalidKey = PublicKey.FromString("000000000000000000000000000000000000000000000000000000000000000000");
                Executable invalidKeyExecutable = () => new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetFeeExemptKeys(new List(List.Of(invalidKey))).Execute(testEnv.client).GetReceipt(testEnv.client);

                // Expect failure due to invalid fee exempt key
                Assert.Throws<ReceiptStatusException>(invalidKeyExecutable, ResponseCodeEnum.INVALID_KEY_IN_FEE_EXEMPT_KEY_LIST.Name());

                // Create 11 keys (exceeding the limit of 10)
                IList<Key> feeExemptKeyListExceedingLimit = List.Of(PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA());
                Executable exceedKeyListLimitExecutable = () => new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetFeeExemptKeys(feeExemptKeyListExceedingLimit).Execute(testEnv.client).GetReceipt(testEnv.client);

                // Expect failure due to exceeding fee exempt key list limit
                Assert.Throws<ReceiptStatusException>(exceedKeyListLimitExecutable, ResponseCodeEnum.MAX_ENTRIES_FOR_FEE_EXEMPT_KEY_LIST_EXCEEDED.Name());
            }
        }

        public virtual void FailsToUpdateFeeScheduleKeyWithoutPermissions()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var newFeeScheduleKey = PrivateKey.GenerateECDSA();
                Executable updateExecutable = () => new TopicUpdateTransaction().SetTopicId(topicId).SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Throws<ReceiptStatusException>(updateExecutable, ResponseCodeEnum.FEE_SCHEDULE_KEY_CANNOT_BE_UPDATED.Name());
            }
        }

        public virtual void FailsToUpdateCustomFeesWithoutFeeScheduleKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var denominatingTokenId1 = CreateToken(testEnv);
                var denominatingTokenId2 = CreateToken(testEnv);
                var customFees = List.Of(new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(denominatingTokenId1).SetAmount(1), new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(denominatingTokenId2).SetAmount(2));
                Executable updateExecutable = () => new TopicUpdateTransaction().SetTopicId(topicId).SetCustomFees(customFees).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Throws<ReceiptStatusException>(updateExecutable, ResponseCodeEnum.FEE_SCHEDULE_KEY_NOT_SET.Name());
            }
        }

        public virtual void ChargesHbarFeesWithLimitsApplied()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbarAmount = 100000000;
                var privateKey = PrivateKey.GenerateECDSA();
                var customFixedFee = new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetAmount(hbarAmount / 2);
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetFeeScheduleKey(testEnv.operatorKey).AddCustomFee(customFixedFee).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var accountId = CreateAccount(testEnv, new Hbar(1), privateKey);
                ClientSetOperator(testEnv, accountId, privateKey);
                new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMessage("Hedera HBAR Fee Test").Execute(testEnv.client).GetReceipt(testEnv.client);
                ClientSetOperator(testEnv, accountId);
                var balance = new AccountBalanceQuery().SetAccountId(accountId).Execute(testEnv.client).hbars;
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
                var customFixedFee = new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetAmount(hbarAmount / 2);
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetFeeScheduleKey(testEnv.operatorKey).SetFeeExemptKeys(List.Of(feeExemptKey1.GetPublicKey(), feeExemptKey2.GetPublicKey())).AddCustomFee(customFixedFee).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var payerAccountId = CreateAccount(testEnv, new Hbar(1), feeExemptKey1);
                ClientSetOperator(testEnv, payerAccountId, feeExemptKey1);
                new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMessage("Hedera Fee Exemption Test").Execute(testEnv.client).GetReceipt(testEnv.client);
                ClientSetOperator(testEnv, payerAccountId);
                var balance = new AccountBalanceQuery().SetAccountId(payerAccountId).Execute(testEnv.client).hbars;
                AssertThat(balance.ToTinybars()).IsGreaterThan(hbarAmount / 2);
            }
        }

        public virtual void CreateTopicTransactionShouldAssignAutomaticallyAutoRenewAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var topicId = new TopicCreateTransaction().Execute(testEnv.client).GetReceipt(testEnv.client).topicId;
                var autoRenewAccountId = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client).autoRenewAccountId;
                AssertThat(autoRenewAccountId).IsNotNull();
            }
        }

        public virtual void CreateTopicTransactionWithTransactionIdShouldAssignAutoRenewAccountIdToTransactionIdAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(publicKey).SetInitialBalance(Hbar.From(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var topicId = new TopicCreateTransaction().SetTransactionId(TransactionId.Generate(accountId)).FreezeWith(testEnv.client).Sign(privateKey).Execute(testEnv.client).GetReceipt(testEnv.client).topicId;
                var autoRenewAccountId = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client).autoRenewAccountId;
                Assert.Equal(autoRenewAccountId, accountId);
            }
        }

        private AccountId CreateAccount(IntegrationTestEnv testEnv, Hbar initialBalance, PrivateKey key)
        {
            return new AccountCreateTransaction().SetInitialBalance(initialBalance).SetKeyWithoutAlias(key).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
        }

        private void ClientSetOperator(IntegrationTestEnv testEnv, AccountId accountId)
        {
            testEnv.client.SetOperator(accountId, PrivateKey.GenerateECDSA());
        }

        private void ClientSetOperator(IntegrationTestEnv testEnv, AccountId accountId, PrivateKey key)
        {
            testEnv.client.SetOperator(accountId, key);
        }

        private TokenId CreateToken(IntegrationTestEnv testEnv)
        {
            var tokenCreateResponse = new TokenCreateTransaction().SetTokenName("Test Token").SetTokenSymbol("TT").SetTreasuryAccountId(testEnv.operatorId).SetInitialSupply(1000000).SetDecimals(2).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).Execute(testEnv.client);
            return Objects.RequireNonNull(tokenCreateResponse.GetReceipt(testEnv.client).tokenId);
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
                var customFixedFees = List.Of(new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(denominatingTokenId1).SetAmount(amount1), new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(denominatingTokenId2).SetAmount(amount2));

                // Create revenue-generating topic
                var response = new TopicCreateTransaction().SetFeeScheduleKey(testEnv.operatorKey).SetSubmitKey(testEnv.operatorKey).SetAdminKey(testEnv.operatorKey).SetFeeExemptKeys(feeExemptKeys).SetCustomFees(customFixedFees).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);

                // Get Topic Info
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.feeScheduleKey, testEnv.operatorKey);

                // Validate fee exempt keys
                for (int i = 0; i < feeExemptKeys.Count; i++)
                {
                    var key = (PrivateKey)feeExemptKeys[i];
                    PublicKey publicKey = key.GetPublicKey();
                    Assert.Equal(info.feeExemptKeys[i], publicKey);
                }


                // Validate custom fees
                for (int i = 0; i < customFixedFees.Count; i++)
                {
                    Assert.Equal(info.customFees[i].GetAmount(), customFixedFees[i].GetAmount());
                    Assert.Equal(info.customFees[i].GetDenominatingTokenId(), customFixedFees[i].GetDenominatingTokenId());
                }

                var newFeeScheduleKey = PrivateKey.GenerateECDSA();
                new TopicUpdateTransaction().SetTopicId(topicId).ClearFeeExemptKeys().ClearFeeScheduleKey().ClearCustomFees().FreezeWith(testEnv.client).Sign(newFeeScheduleKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var cleared = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Empty(cleared.feeExemptKeys);
                AssertThat(cleared.feeScheduleKey).IsNull();
                Assert.Empty(cleared.customFees);
            }
        }

        public virtual void CanUpdateTopicWithoutSpecifyingAnythingTopicShouldHaveTheSameValues()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                IList<Key> feeExemptKeys = new List(List.Of(PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA()));
                var denominatingTokenId1 = CreateToken(testEnv);
                var amount1 = 1;
                var denominatingTokenId2 = CreateToken(testEnv);
                var amount2 = 2;
                var customFixedFees = List.Of(new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(denominatingTokenId1).SetAmount(amount1), new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(denominatingTokenId2).SetAmount(amount2));

                // Create revenue-generating topic
                var response = new TopicCreateTransaction().SetFeeScheduleKey(testEnv.operatorKey).SetSubmitKey(testEnv.operatorKey).SetAdminKey(testEnv.operatorKey).SetFeeExemptKeys(feeExemptKeys).SetCustomFees(customFixedFees).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);

                // Get Topic Info
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.feeScheduleKey, testEnv.operatorKey);

                // Validate fee exempt keys
                for (int i = 0; i < feeExemptKeys.Count; i++)
                {
                    var key = (PrivateKey)feeExemptKeys[i];
                    PublicKey publicKey = key.GetPublicKey();
                    Assert.Equal(info.feeExemptKeys[i], publicKey);
                }


                // Validate custom fees
                for (int i = 0; i < customFixedFees.Count; i++)
                {
                    Assert.Equal(info.customFees[i].GetAmount(), customFixedFees[i].GetAmount());
                    Assert.Equal(info.customFees[i].GetDenominatingTokenId(), customFixedFees[i].GetDenominatingTokenId());
                }

                new TopicUpdateTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
                var sameTopic = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(sameTopic.feeExemptKeys, info.feeExemptKeys);
                Assert.Equal(sameTopic.feeScheduleKey, info.feeScheduleKey);
                Assert.Equal(sameTopic.customFees[0].GetAmount(), info.customFees[0].GetAmount());
            }
        }
    }
}