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
    public class TopicUpdateIntegrationTest
    {
        virtual void CanUpdateTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetAutoRenewAccountId(testEnv.operatorId).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                new TopicUpdateTransaction().ClearAutoRenewAccountId().SetTopicMemo("hello").SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
                var topicInfo = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(topicInfo.topicMemo, "hello");
                AssertThat(topicInfo.autoRenewAccountId).IsNull();
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}