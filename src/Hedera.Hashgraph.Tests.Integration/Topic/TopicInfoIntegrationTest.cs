// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Topic;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicInfoIntegrationTest
    {
        public virtual void CanQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    TopicMemo = "[e2e::TopicCreateTransaction]"
                }
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var info = new TopicInfoQuery
                {
                    TopicId = topicId

                }.Execute(testEnv.Client);
                Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                new TopicDeleteTransaction
                {
                    TopicId = topicId

                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void GetCostQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    TopicMemo = "[e2e::TopicCreateTransaction]"
                }
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var infoQuery = new TopicInfoQuery
                {
                    TopicId = topicId
                };
                var cost = infoQuery.GetCost(testEnv.Client);
                Assert.NotNull(cost);
                var info = infoQuery.Execute(testEnv.Client);
                Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                new TopicDeleteTransaction
                {
                    TopicId = topicId

                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void GetCostBigMaxQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    TopicMemo = "[e2e::TopicCreateTransaction]"
                }
                .Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var infoQuery = new TopicInfoQuery
                {
                    TopicId = topicId,
					MaxQueryPayment = new Hbar(1000),
				};
                var cost = infoQuery.GetCost(testEnv.Client);
                Assert.NotNull(cost);
                var info = infoQuery.Execute(testEnv.Client);
                Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                new TopicDeleteTransaction
                {
					TopicId = topicId

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void GetCostSmallMaxQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    TopicMemo = "[e2e::TopicCreateTransaction]"
                }
                .Execute(testEnv.Client);
                
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var infoQuery = new TopicInfoQuery
				{
					TopicId = topicId,
					MaxQueryPayment = new Hbar(1000),
				};
                
                MaxQueryPaymentExceededException exception = Assert.Throws<MaxQueryPaymentExceededException>(() =>
                {
                    infoQuery.Execute(testEnv.Client);
                });

                new TopicDeleteTransaction
                {
                    TopicId = topicId

                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void GetCostInsufficientTxFeeQueryTopicInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    TopicMemo = "[e2e::TopicCreateTransaction]"
                }
                .Execute(testEnv.Client);

                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var infoQuery = new TopicInfoQuery
                {
                    TopicId = topicId
                };
                var cost = infoQuery.GetCost(testEnv.Client);

                Assert.NotNull(cost);

                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    infoQuery.QueryPayment = Hbar.FromTinybars(1);
                    infoQuery.Execute(testEnv.Client);

                });
                
                Assert.Equal(exception.Status.ToString(), "INSUFFICIENT_TX_FEE");


                new TopicDeleteTransaction
                {
                    TopicId = topicId

                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
    }
}