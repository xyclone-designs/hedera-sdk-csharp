// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Topic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    public class TopicMessageQueryTest
    {
        private static readonly DateTimeOffset START_TIME = DateTimeOffset.UtcNow;
        private Client client;
        private bool complete = false;
        private readonly List<Exception> errors = new();
        private readonly List<TopicMessage> received = new();
        private readonly ConsensusServiceStub consensusServiceStub = new();
        private Server server;
        private TopicMessageQuery topicMessageQuery;

        public virtual void Setup()
        {
            client = Client.ForNetwork(new Dictionary<string, AccountId>());
            client.MirrorNetwork_.Network = new[] { "in-process:test" };

            server = new Server
            {
                Services = { Proto.ConsensusService.BindService(consensusServiceStub) },
                Ports = { new ServerPort("localhost", 1000, ServerCredentials.Insecure) }
            };
            server.Start();

            topicMessageQuery = new TopicMessageQuery
            {
                CompletionHandler = () => Volatile.Write(ref complete, true),
                EndTime = START_TIME.AddSeconds(100),
                ErrorHandler = (t, r) => errors.Add(t),
                MaxBackoff = TimeSpan.FromMilliseconds(500),
                StartTime = START_TIME,
                TopicId = TopicId.FromString("0.0.1000")
            };
        }

        public virtual void Teardown()
        {
            consensusServiceStub.Verify();

            client?.Dispose();
            server?.ShutdownAsync().Wait();
        }
        [Fact]
        public virtual void Subscribe()
        {
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(Response(1));
            consensusServiceStub.responses.Enqueue(Response(2));

            SubscribeToMirror(received.Add);

            Assert.Empty(errors);
            Assert.Equal(2, received.Count);
            Assert.Equal((ulong)1, received[0].SequenceNumber);
            Assert.Equal((ulong)2, received[1].SequenceNumber);
        }
        [Fact]
        public virtual void SubscribeChunked()
        {
            Proto.ConsensusTopicResponse response1 = Response(1, 2);
            Proto.ConsensusTopicResponse response2 = Response(2, 2);

            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(response1);
            consensusServiceStub.responses.Enqueue(response2);

            SubscribeToMirror(received.Add);

            var message = Combine(response1.Message.ToByteArray(), response2.Message.ToByteArray());

            Assert.Empty(errors);
            Assert.Single(received);
            var first = received[0];
            Assert.Equal(response2.ConsensusTimestamp.ToDateTimeOffset(), first.ConsensusTimestamp);
            Assert.Equal(response2.ChunkInfo.InitialTransactionID, first.TransactionId.ToProtobuf());
            Assert.Equal(message, first.Contents);
            Assert.Equal(response2.RunningHash.ToByteArray(), first.RunningHash);
            Assert.Equal(response2.SequenceNumber, first.SequenceNumber);
            Assert.Equal(2, first.Chunks.Length);
            Assert.Equal((ulong)1, first.Chunks[0].SequenceNumber);
            Assert.Equal((ulong)2, first.Chunks[1].SequenceNumber);
        }
        [Fact]
        public virtual void SubscribeNoResponse()
        {
            consensusServiceStub.requests.Enqueue(Request());

            SubscribeToMirror(received.Add);

            Assert.Empty(errors);
            Assert.Empty(received);
        }
        [Fact]
        public virtual void ErrorDuringOnNext()
        {
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(Response(1));

            SubscribeToMirror((t) =>
            {
                throw new Exception();
            });

            Assert.Single(errors);
            Assert.IsType<Exception>(errors[0]);
            Assert.Empty(received);
        }
        [Theory]
        [InlineData(StatusCode.OK, "")]
        public virtual void RetryRecovers(StatusCode code, string description)
        {
            Proto.ConsensusTopicResponse response = Response(1);
            DateTimeOffset nextTimestamp = response.ConsensusTimestamp.ToDateTimeOffset().AddTicks(1);
            Proto.ConsensusTopicQuery request = Request();

            var retryRequest = Request();
            retryRequest.ConsensusStartTime = nextTimestamp.ToProtoTimestamp();

            consensusServiceStub.requests.Enqueue(request);
            consensusServiceStub.requests.Enqueue(retryRequest);
            consensusServiceStub.responses.Enqueue(response);
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(code, description)));
            consensusServiceStub.responses.Enqueue(Response(2));

            SubscribeToMirror(received.Add);

            Assert.Equal(2, received.Count);
            Assert.Equal((ulong)1, received[0].SequenceNumber);
            Assert.Equal((ulong)2, received[1].SequenceNumber);
            Assert.Empty(errors);
        }
        [Theory]
        [InlineData(StatusCode.OK, "")]
        public virtual void NoRetry(StatusCode code, string description)
        {
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(code, description)));

            SubscribeToMirror(received.Add);

            Assert.Empty(received);
            Assert.Single(errors);
            var rpcEx = Assert.IsType<RpcException>(errors[0]);
            Assert.Equal(code, rpcEx.StatusCode);
        }
        [Fact]
        public virtual void CustomRetry()
        {
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(StatusCode.InvalidArgument, string.Empty)));
            consensusServiceStub.responses.Enqueue(Response(1));
            topicMessageQuery.RetryHandler = (t) => true;

            SubscribeToMirror(received.Add);

            Assert.Single(received);
            Assert.Equal((ulong)1, received[0].SequenceNumber);
            Assert.Empty(errors);
        }
        [Fact]
        public virtual void RetryWithLimit()
        {
            Proto.ConsensusTopicResponse response = Response(1);
            DateTimeOffset nextTimestamp = response.ConsensusTimestamp.ToDateTimeOffset().AddTicks(1);
            Proto.ConsensusTopicQuery request = Request();

            topicMessageQuery.Limit = 2;

            var requestWithLimit = Request();
            requestWithLimit.Limit = 2;

            var retryRequest = Request();
            retryRequest.ConsensusStartTime = nextTimestamp.ToProtoTimestamp();
            retryRequest.Limit = 1;

            consensusServiceStub.requests.Enqueue(requestWithLimit);
            consensusServiceStub.requests.Enqueue(retryRequest);
            consensusServiceStub.responses.Enqueue(response);
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(StatusCode.ResourceExhausted, string.Empty)));
            consensusServiceStub.responses.Enqueue(Response(2));

            SubscribeToMirror(received.Add);

            Assert.Equal(2, received.Count);
            Assert.Equal((ulong)1, received[0].SequenceNumber);
            Assert.Equal((ulong)2, received[1].SequenceNumber);
            Assert.Empty(errors);
        }
        [Fact]
        public virtual void RetriesExhausted()
        {
            topicMessageQuery.MaxAttempts = 1;
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(StatusCode.ResourceExhausted, string.Empty)));
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(StatusCode.ResourceExhausted, string.Empty)));

            SubscribeToMirror(received.Add);

            Assert.Empty(received);
            Assert.Single(errors);
            var rpcEx = Assert.IsType<RpcException>(errors[0]);
            Assert.Equal(StatusCode.ResourceExhausted, rpcEx.StatusCode);
        }
        [Fact]
        public virtual void ErrorWhenCallIsCancelled()
        {
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(StatusCode.Cancelled, string.Empty)));

            SubscribeToMirror(received.Add);

            Assert.Single(errors);
            var rpcEx = Assert.IsType<RpcException>(errors[0]);
            Assert.Equal(StatusCode.Cancelled, rpcEx.StatusCode);
            Assert.Empty(received);
        }
        [Fact]
        public virtual void UnsubscribeDoesNotInvokeErrorOrRetry()
        {
            consensusServiceStub.requests.Enqueue(Request());
            SubscriptionHandle handle = topicMessageQuery.Subscribe(client, received.Add);
            handle.Unsubscribe();
            Thread.Sleep(100);
            Assert.Empty(errors);
            Assert.Empty(received);
        }
        [Fact]
        public virtual void ServerCancelledRetriesWhenCustomRetryAllows()
        {
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(StatusCode.Cancelled, string.Empty)));
            consensusServiceStub.responses.Enqueue(Response(1));
            topicMessageQuery.RetryHandler = (t) =>
            {
                if (t is RpcException sre)
                {
                    return sre.StatusCode == StatusCode.Cancelled;
                }

                return false;
            };

            SubscribeToMirror(received.Add);

            Assert.Single(received);
            Assert.Equal((ulong)1, received[0].SequenceNumber);
            Assert.Empty(errors);
        }
        [Fact]
        public virtual void UnsubscribeThenResubscribeResetsClientCancelFlagAllowsRetryOnCancelled()
        {
            consensusServiceStub.requests.Enqueue(Request());
            SubscriptionHandle firstHandle = topicMessageQuery.Subscribe(client, received.Add);
            firstHandle.Unsubscribe();
            Thread.Sleep(100);
            Assert.Empty(errors);
            Assert.Empty(received);

            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.requests.Enqueue(Request());
            consensusServiceStub.responses.Enqueue(new RpcException(new Status(StatusCode.Cancelled, string.Empty)));
            consensusServiceStub.responses.Enqueue(Response(1));
            topicMessageQuery.RetryHandler = (t) =>
            {
                if (t is RpcException sre)
                {
                    return sre.StatusCode == StatusCode.Cancelled;
                }

                return false;
            };

            SubscriptionHandle secondHandle = topicMessageQuery.Subscribe(client, received.Add);
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (received.Count < 1 && stopwatch.Elapsed.TotalSeconds < 3)
            {
                Thread.Sleep(50);
            }

            Assert.Empty(errors);
            Assert.Single(received);
            Assert.Equal((ulong)1, received[0].SequenceNumber);
            secondHandle.Unsubscribe();
        }
        private void SubscribeToMirror(Action<TopicMessage> onNext)
        {
            SubscriptionHandle subscriptionHandle = topicMessageQuery.Subscribe(client, onNext);
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (!Volatile.Read(ref complete) && errors.Count == 0 && stopwatch.Elapsed.TotalSeconds < 3)
            {
                Thread.Sleep(100);
            }

            subscriptionHandle.Unsubscribe();
        }

        private static Proto.ConsensusTopicQuery Request()
        {
            return new Proto.ConsensusTopicQuery
            {
                ConsensusEndTime = START_TIME.AddSeconds(100).ToProtoTimestamp(),
                ConsensusStartTime = START_TIME.ToProtoTimestamp(),
                TopicID = new Proto.TopicID { TopicNum = 1000 }
            };
        }

        private static Timestamp ToTimestamp(DateTimeOffset dateTimeOffset)
        {
            return Timestamp.FromDateTimeOffset(dateTimeOffset);
        }

        private static Proto.ConsensusTopicResponse Response(long sequenceNumber)
        {
            return Response(sequenceNumber, 0);
        }

        private static Proto.ConsensusTopicResponse Response(long sequenceNumber, int total)
        {
            var response = new Proto.ConsensusTopicResponse();

            if (total > 0)
            {
                response.ChunkInfo = new Proto.ConsensusMessageChunkInfo
                {
                    Number = (int)sequenceNumber,
                    Total = total,
                    InitialTransactionID = new Proto.Services.TransactionID
                    {
                        AccountID = new Proto.AccountID { AccountNum = 3 },
                        TransactionValidStart = START_TIME.ToProtoTimestamp(),
                    },
                };
            }

            var message = ByteString.CopyFrom(new byte[] { (byte)sequenceNumber });

            response.ConsensusTimestamp = START_TIME.AddSeconds(sequenceNumber).ToProtoTimestamp();
            response.SequenceNumber = (ulong)sequenceNumber;
            response.Message = message;
            response.RunningHash = message;
            response.RunningHashVersion = 2;

            return response;
        }
        private class ConsensusServiceStub : Proto.ConsensusService.ConsensusServiceBase
        {
            public readonly Queue<Proto.ConsensusTopicQuery> requests = new();
            public readonly Queue<object> responses = new();

            public override async Task subscribeTopic(Proto.ConsensusTopicQuery consensusTopicQuery, IServerStreamWriter<Proto.ConsensusTopicResponse> streamWriter, ServerCallContext context)
            {
                var request = requests.Count > 0 ? requests.Dequeue() : null;
                Assert.NotNull(request);
                Assert.Equal(consensusTopicQuery, request);

                while (responses.Count > 0)
                {
                    var response = responses.Dequeue();
                    Assert.NotNull(response);

                    if (response is RpcException rpcEx)
                    {
                        throw rpcEx;
                    }

                    await streamWriter.WriteAsync((Proto.ConsensusTopicResponse)response);
                }
            }

            public virtual void Verify()
            {
                Assert.Empty(requests);
                Assert.Empty(responses);
            }
        }

        private byte[] Combine(byte[] array1, byte[] array2)
        {
            byte[] joinedArray = new byte[array1.Length + array2.Length];
            Array.Copy(array1, 0, joinedArray, 0, array1.Length);
            Array.Copy(array2, 0, joinedArray, array1.Length, array2.Length);
            return joinedArray;
        }
    }
}