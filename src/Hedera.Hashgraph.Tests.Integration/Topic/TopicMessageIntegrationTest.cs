// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph;
using Java.Nio.Charset;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent.Atomic;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicMessageIntegrationTest
    {
        public virtual void CanReceiveATopicMessage()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 0);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                Thread.Sleep(3000);
                var receivedMessage = new bool[]
                {
                    false
                };
                var start = DateTimeOffset.UtcNow;
                var handle = new TopicMessageQuery().SetTopicId(topicId).SetStartTime(Instant.EPOCH).Subscribe(testEnv.client, (message) =>
                {
                    receivedMessage[0] = new string (message.contents, StandardCharsets.UTF_8).Equals("Hello, from HCS!");
                });
                Thread.Sleep(3000);
                new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMessage("Hello, from HCS!").Execute(testEnv.client).GetReceipt(testEnv.client);
                while (!receivedMessage[0])
                {
                    if (Duration.Between(start, DateTimeOffset.UtcNow).CompareTo(Duration.OfSeconds(60)) > 0)
                    {
                        throw new Exception("TopicMessage was not received in 60 seconds or less");
                    }

                    Thread.Sleep(5000);
                }

                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanReceiveALargeTopicMessage()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Skip if using local node.
                // Note: this check should be removed once the local node is supporting multiple nodes.
                testEnv.AssumeNotLocalNode();
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                Thread.Sleep(5000);
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 0);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                var receivedMessage = new bool[]
                {
                    false
                };
                var start = DateTimeOffset.UtcNow;
                var handle = new TopicMessageQuery().SetTopicId(topicId).SetStartTime(Instant.EPOCH).Subscribe(testEnv.client, (message) =>
                {
                    receivedMessage[0] = new string (message.contents, StandardCharsets.UTF_8).Equals(Contents.BIG_CONTENTS);
                });
                new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMessage(Contents.BIG_CONTENTS).Execute(testEnv.client).GetReceipt(testEnv.client);
                while (!receivedMessage[0])
                {
                    if (Duration.Between(start, DateTimeOffset.UtcNow).CompareTo(Duration.OfSeconds(60)) > 0)
                    {
                        throw new Exception("TopicMessage was not received in 60 seconds or less");
                    }

                    Thread.Sleep(1000);
                }

                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void UnsubscribingDoesNotLogRetryWarnings()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var receivedMessage = new AtomicBoolean(false);
                var retryWarningLogged = new AtomicBoolean(false);
                var errorHandlerInvoked = new AtomicBoolean(false);
                var retryHandler = new AnonymousPredicate(this);
                var handle = new TopicMessageQuery().SetTopicId(topicId).SetStartTime(Instant.EPOCH).SetRetryHandler(retryHandler).SetErrorHandler((throwable, topicMessage) => errorHandlerInvoked.Set(true)).Subscribe(testEnv.client, (message) =>
                {
                    receivedMessage.Set(true);
                });
                handle.Unsubscribe();
                Thread.Sleep(3000);
                AssertThat(retryWarningLogged.Get()).IsFalse();
                AssertThat(receivedMessage.Get()).IsFalse();
                AssertThat(errorHandlerInvoked.Get()).IsFalse();
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
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
                retryWarningLogged.Set(true);
                return false; // Don't actually retry
            }
        }
    }
}