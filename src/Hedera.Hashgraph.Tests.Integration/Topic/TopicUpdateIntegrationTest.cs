// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Consensus;

namespace Hedera.Hashgraph.Tests.Integration
{
    /// <include file="TopicUpdateIntegrationTest.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.Integration.TopicUpdateIntegrationTest"]' />
    public class TopicUpdateIntegrationTest
    {
        [Fact]
        /// <include file="TopicUpdateIntegrationTest.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.Integration.TopicUpdateIntegrationTest.CanUpdateTopic"]' />
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

                var topicId = response.GetReceipt(testEnv.Client).TopicId;
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
