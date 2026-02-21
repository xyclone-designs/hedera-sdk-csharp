// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Topic;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicUpdateIntegrationTest
    {
        public virtual void CanUpdateTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction()
                {
					AdminKey = testEnv.OperatorKey,
					AutoRenewAccountId = testEnv.OperatorId,
					TopicMemo = "[e2e::TopicCreateTransaction]",
				}
                .Execute(testEnv.Client);

                var topicId = response.GetReceipt(testEnv.Client).TopicId);
                new TopicUpdateTransaction
                { 
                    TopicMemo = "hello",
					TopicId = topicId,
                    AutoRenewAccountId = null,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var topicInfo = new TopicInfoQuery
                {
					TopicId = topicId
				}.Execute(testEnv.Client);

                Assert.Equal(topicInfo.TopicMemo, "hello");
                Assert.Null(topicInfo.AutoRenewAccountId);

                new TopicDeleteTransaction
                {
					TopicId = topicId
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
    }
}