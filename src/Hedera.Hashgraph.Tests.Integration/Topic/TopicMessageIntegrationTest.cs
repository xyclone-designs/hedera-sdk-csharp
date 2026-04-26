// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Consensus;

using System;
using System.Text;
using System.Threading;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicMessageIntegrationTest
    {
        [Fact]
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
                Assert.Equal(info.SequenceNumber, (ulong)0);
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
					StartTime = DateTimeOffset.UnixEpoch,

				}.Subscribe(testEnv.Client, (message) =>
                {
                    receivedMessage[0] = Encoding.UTF8.GetString(message.Contents).Equals("Hello, from HCS!");
                });
                Thread.Sleep(3000);
                new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					Message = ByteString.CopyFromUtf8("Hello, from HCS!"),
				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                while (!receivedMessage[0])
                {
					if (DateTimeOffset.UtcNow - start > TimeSpan.FromSeconds(60))
						throw new Exception("TopicMessage was not received in 60 seconds or less");

					Thread.Sleep(5000);
                }

                new TopicDeleteTransaction
                {
					TopicId = topicId

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
        [Fact]
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
                Assert.Equal(info.SequenceNumber, (ulong)0);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                var receivedMessage = new bool[]
                {
                    false
                };
                var start = DateTimeOffset.UtcNow;
                var handle = new TopicMessageQuery
                {
					TopicId = topicId,
					StartTime = DateTimeOffset.UnixEpoch,

				}.Subscribe(testEnv.Client, (message) =>
                {
                    receivedMessage[0] = Encoding.UTF8.GetString(message.Contents).Equals(Contents.BIG_CONTENTS);
                });
                new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					Message = ByteString.CopyFromUtf8(Contents.BIG_CONTENTS),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                while (!receivedMessage[0])
                {
                    if ((start - DateTimeOffset.UtcNow).CompareTo(TimeSpan.FromSeconds(60)) > 0)
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
        [Fact]
        public virtual void UnsubscribingDoesNotLogRetryWarnings()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					TopicMemo = "[e2e::TopicCreateTransaction]"
				
                }.Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var receivedMessage = false;
                var retryWarningLogged = false;
                var errorHandlerInvoked = false;
                var handle = new TopicMessageQuery
                {
                    TopicId = topicId,
                    StartTime = DateTime.UnixEpoch,
					ErrorHandler = (exception, topicMessage) => Volatile.Write(ref errorHandlerInvoked, true),
					RetryHandler = (exception) =>
                    {
                        Volatile.Write(ref retryWarningLogged, true);
                        return false;
					},

                }.Subscribe(testEnv.Client, message => Volatile.Write(ref receivedMessage, true));

                handle.Unsubscribe();
                
                Thread.Sleep(3000);
                
                Assert.False(Volatile.Read(ref retryWarningLogged));
                Assert.False(Volatile.Read(ref receivedMessage));
                Assert.False(Volatile.Read(ref errorHandlerInvoked));
                
                new TopicDeleteTransaction
                {
					TopicId = topicId
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
    }
}