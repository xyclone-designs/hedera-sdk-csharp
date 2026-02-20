// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Topic;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicDeleteIntegrationTest
    {
        public virtual void CanDeleteTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    TopicMemo = "[e2e::TopicCreateTransaction]"
                
                }.Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                new TopicDeleteTransaction
                {
                    TopicId = topicId
                
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotDeleteImmutableTopic()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.UNAUTHORIZED.ToString(), exception.Message);
            }
        }
    }
}