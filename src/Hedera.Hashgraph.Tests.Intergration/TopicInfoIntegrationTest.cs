// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicInfoIntegrationTest
    {
        virtual void CanQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void GetCostQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var infoQuery = new TopicInfoQuery().SetTopicId(topicId);
                var cost = infoQuery.GetCost(testEnv.client);
                AssertThat(cost).IsNotNull();
                var info = infoQuery.Execute(testEnv.client);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void GetCostBigMaxQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var infoQuery = new TopicInfoQuery().SetTopicId(topicId).SetMaxQueryPayment(new Hbar(1000));
                var cost = infoQuery.GetCost(testEnv.client);
                AssertThat(cost).IsNotNull();
                var info = infoQuery.Execute(testEnv.client);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void GetCostSmallMaxQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var infoQuery = new TopicInfoQuery().SetTopicId(topicId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                AssertThatExceptionOfType(typeof(MaxQueryPaymentExceededException)).IsThrownBy(() =>
                {
                    infoQuery.Execute(testEnv.client);
                });
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void GetCostInsufficientTxFeeQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var infoQuery = new TopicInfoQuery().SetTopicId(topicId);
                var cost = infoQuery.GetCost(testEnv.client);
                AssertThat(cost).IsNotNull();
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    infoQuery.SetQueryPayment(Hbar.FromTinybars(1)).Execute(testEnv.client);
                }).Satisfies((error) => Assert.Equal(error.status.ToString(), "INSUFFICIENT_TX_FEE"));
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}