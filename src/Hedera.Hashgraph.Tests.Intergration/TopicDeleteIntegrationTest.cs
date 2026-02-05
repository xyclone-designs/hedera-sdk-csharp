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
    public class TopicDeleteIntegrationTest
    {
        virtual void CanDeleteTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CannotDeleteImmutableTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.UNAUTHORIZED.ToString());
            }
        }
    }
}