// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Topic;
using Java.Nio.Charset;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent.Atomic;
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicMessageIntegrationTest
    {
        public virtual void CanReceiveATopicMessage()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    TopicMemo = "[e2e::TopicCreateTransaction]"

                }.Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var info = new TopicInfoQuery
                {
					TopicId = topicId
				}.Execute(testEnv.Client);
                Assert.Equal(info.TopicId, topicId);
                Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.SequenceNumber, 0);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                Thread.Sleep(3000);
                var receivedMessage = new bool[]
                {
                    false
                };
                var start = DateTimeOffset.UtcNow;
                var handle = new TopicMessageQuery
                {
					TopicId = topicId,
					StartTime = Instant.EPOCH,

				}.Subscribe(testEnv.Client, (message) =>
                {
                    receivedMessage[0] = new string (message.contents, StandardCharsets.UTF_8).Equals("Hello, from HCS!");
                });
                Thread.Sleep(3000);
                new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					Message = "Hello, from HCS!",
				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                while (!receivedMessage[0])
                {
                    if (Duration.Between(start, DateTimeOffset.UtcNow).CompareTo(Duration.OfSeconds(60)) > 0)
                    {
                        throw new Exception("TopicMessage was not received in 60 seconds or less");
                    }

                    Thread.Sleep(5000);
                }

                new TopicDeleteTransaction
                {
					TopicId = topicId
				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanReceiveALargeTopicMessage()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Skip if using local node.
                // Note: this check should be removed once the local node is supporting multiple nodes.
                testEnv.AssumeNotLocalNode();
                var response = new TopicCreateTransaction()
                {
					AdminKey = testEnv.OperatorKey,
					TopicMemo = "[e2e::TopicCreateTransaction]",

				}.Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                Thread.Sleep(5000);
                var info = new TopicInfoQuery()
                {
					TopicId = topicId
				}.Execute(testEnv.Client);
                Assert.Equal(info.TopicId, topicId);
                Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.SequenceNumber, 0);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                var receivedMessage = new bool[]
                {
                    false
                };
                var start = DateTimeOffset.UtcNow;
                var handle = new TopicMessageQuery
                {
					TopicId = topicId,
					StartTime = Instant.EPOCH,
				}.Subscribe(testEnv.Client, (message) =>
                {
                    receivedMessage[0] = new string (message.contents, StandardCharsets.UTF_8).Equals(Contents.BIG_CONTENTS);
                });
                new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					Message = Contents.BIG_CONTENTS,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                while (!receivedMessage[0])
                {
                    if (Duration.Between(start, DateTimeOffset.UtcNow).CompareTo(Duration.OfSeconds(60)) > 0)
                    {
                        throw new Exception("TopicMessage was not received in 60 seconds or less");
                    }

                    Thread.Sleep(1000);
                }

                new TopicDeleteTransaction
                {
					TopicId = topicId

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void UnsubscribingDoesNotLogRetryWarnings()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction()
                    AdminKey = testEnv.OperatorKey)
                    TopicMemo = "[e2e::TopicCreateTransaction]").Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId);
                var receivedMessage = new AtomicBoolean(false);
                var retryWarningLogged = new AtomicBoolean(false);
                var errorHandlerInvoked = new AtomicBoolean(false);
                var retryHandler = new AnonymousPredicate(this);
                var handle = new TopicMessageQuery()
                    TopicId = topicId)
                    StartTime = Instant.EPOCH)
                    RetryHandler = retryHandler)
                    ErrorHandler = (throwable, topicMessage) => errorHandlerInvoked
                    ( = rue)).Subscribe(testEnv.Client, (message) =>
                {
                    receivedMessage
                    ( = rue);
                });
                handle.Unsubscribe();
                Thread.Sleep(3000);
                Assert.False(retryWarningLogged.Get());
                Assert.False(receivedMessage.Get());
                Assert.False(errorHandlerInvoked.Get());
                new TopicDeleteTransaction()
                    TopicId = topicId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        private sealed class AnonymousPredicate : Predicate
        {
            public AnonymousPredicate(TopicMessageIntegrationTest parent)
            {
                this.parent = parent;
            }

            private readonly TopicMessageIntegrationTest parent;
            public bool Test(Throwable throwable)
            {
                retryWarningLogged
                    ( = rue);
                return false; // Don't actually retry
            }
        }
    }
}