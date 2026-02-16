// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api;
using Com.Hedera.Hashgraph;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class ScheduleTransactionIntegrationTest
    {
        public virtual void ShouldChargeHbarsWithLimitUsingScheduledTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbar = 100000000; // 1 HBAR in tinybars
                var customFixedFee = new CustomFixedFee().SetFeeCollectorAccountId(testEnv.client.GetOperatorAccountId()).SetAmount(hbar / 2);

                // Create a revenue generating topic
                var topicResponse = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetFeeScheduleKey(testEnv.operatorKey).SetCustomFees(List.Of(customFixedFee)).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(topicResponse.GetReceipt(testEnv.client).topicId);

                // Create payer account
                var payerKey = PrivateKey.GenerateED25519();
                var payerResponse = new AccountCreateTransaction().SetKey(payerKey).SetInitialBalance(Hbar.FromTinybars(hbar)).Execute(testEnv.client);
                var payerAccountId = Objects.RequireNonNull(payerResponse.GetReceipt(testEnv.client).accountId);
                var customFeeLimit = new CustomFeeLimit().SetPayerId(payerAccountId).SetCustomFees(List.Of(customFixedFee));

                // Submit a message to the revenue generating topic with custom fee limit using scheduled transaction
                // Create a new client with the payer account as operator
                var payerClient = Client.ForNetwork(testEnv.client.GetNetwork());
                payerClient.SetMirrorNetwork(testEnv.client.GetMirrorNetwork());
                payerClient.SetOperator(payerAccountId, payerKey);
                var submitMessageTransaction = new TopicMessageSubmitTransaction().SetMessage("hello!").SetTopicId(topicId).SetCustomFeeLimits(List.Of(customFeeLimit));
                var scheduleResponse = submitMessageTransaction.Schedule().Execute(payerClient);
                var scheduleId = Objects.RequireNonNull(scheduleResponse.GetReceipt(payerClient).scheduleId);

                // The scheduled transaction should execute immediately since we have all required signatures
                AssertThat(scheduleId).IsNotNull();
                payerClient.Dispose();
                var accountBalance = new AccountBalanceQuery().SetAccountId(payerAccountId).Execute(testEnv.client);
                AssertThat(accountBalance.hbars.ToTinybars()).IsLessThan(hbar / 2);

                // Cleanup
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void ShouldNotChargeHbarsWithLowerLimitUsingScheduledTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var hbar = 100000000; // 1 HBAR in tinybars
                var customFixedFee = new CustomFixedFee().SetFeeCollectorAccountId(testEnv.client.GetOperatorAccountId()).SetAmount(hbar / 2);

                // Create a revenue generating topic with Hbar custom fee
                var topicResponse = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetFeeScheduleKey(testEnv.operatorKey).SetCustomFees(List.Of(customFixedFee)).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(topicResponse.GetReceipt(testEnv.client).topicId);

                // Create payer account
                var payerKey = PrivateKey.GenerateED25519();
                var payerResponse = new AccountCreateTransaction().SetKey(payerKey).SetInitialBalance(Hbar.FromTinybars(hbar)).Execute(testEnv.client);
                var payerAccountId = Objects.RequireNonNull(payerResponse.GetReceipt(testEnv.client).accountId);

                // Set custom fee limit with lower amount than the custom fee
                var customFeeLimit = new CustomFeeLimit().SetPayerId(payerAccountId).SetCustomFees(List.Of(new CustomFixedFee().SetAmount(hbar / 2 - 1)));

                // Submit a message to the revenue generating topic with custom fee limit using scheduled transaction
                // Create a new client with the payer account as operator
                var payerClient = Client.ForNetwork(testEnv.client.GetNetwork());
                payerClient.SetMirrorNetwork(testEnv.client.GetMirrorNetwork());
                payerClient.SetOperator(payerAccountId, payerKey);
                new TopicMessageSubmitTransaction().SetMessage("Hello").SetTopicId(topicId).SetCustomFeeLimits(List.Of(customFeeLimit)).Schedule().Execute(payerClient).GetReceipt(payerClient);
                var accountBalance = new AccountBalanceQuery().SetAccountId(payerAccountId).Execute(testEnv.client);
                AssertThat(accountBalance.hbars.ToTinybars()).IsGreaterThan(hbar / 2);
                payerClient.Dispose();

                // Cleanup
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void ShouldNotChargeTokensWithLowerLimitUsingScheduledTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create a fungible token
                var tokenResponse = new TokenCreateTransaction().SetTokenName("Test Token").SetTokenSymbol("TT").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(tokenResponse.GetReceipt(testEnv.client).tokenId);
                var customFixedFee = new CustomFixedFee().SetAmount(2).SetDenominatingTokenId(tokenId).SetFeeCollectorAccountId(testEnv.client.GetOperatorAccountId());

                // Create a revenue generating topic
                var topicResponse = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetFeeScheduleKey(testEnv.operatorKey).SetCustomFees(List.Of(customFixedFee)).Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(topicResponse.GetReceipt(testEnv.client).topicId);

                // Create payer account with unlimited token associations
                var payerKey = PrivateKey.GenerateED25519();
                var payerResponse = new AccountCreateTransaction().SetKey(payerKey).SetInitialBalance(Hbar.FromTinybars(100000000)).SetMaxAutomaticTokenAssociations(-1).Execute(testEnv.client);
                var payerAccountId = Objects.RequireNonNull(payerResponse.GetReceipt(testEnv.client).accountId);

                // Send tokens to payer
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.client.GetOperatorAccountId(), -2).AddTokenTransfer(tokenId, payerAccountId, 2).Execute(testEnv.client).GetReceipt(testEnv.client);

                // Set custom fee limit with lower amount than the custom fee
                var customFeeLimit = new CustomFeeLimit().SetPayerId(payerAccountId).SetCustomFees(List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(tokenId)));

                // Submit a message to the revenue generating topic with custom fee limit using scheduled transaction
                // Create a new client with the payer account as operator
                testEnv.client.SetOperator(payerAccountId, payerKey);
                new TopicMessageSubmitTransaction().SetMessage("Hello!").SetTopicId(topicId).SetCustomFeeLimits(List.Of(customFeeLimit)).Schedule().Execute(testEnv.client).GetReceipt(testEnv.client);
                var accountBalance = new AccountBalanceQuery().SetAccountId(payerAccountId).Execute(testEnv.client);
                Assert.Equal(accountBalance.tokens[tokenId], 2);
            }
        }
    }
}