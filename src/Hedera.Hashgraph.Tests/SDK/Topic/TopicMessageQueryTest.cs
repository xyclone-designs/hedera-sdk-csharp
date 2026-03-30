// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK;

using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using Grpc.Core;
using System.Threading;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    class TopicMessageQueryTest
    {
        private static readonly DateTimeOffset START_TIME = DateTimeOffset.UtcNow;
        private Client client;
        private bool complete = false;
        private readonly List<Exception> errors = [];
        private readonly List<TopicMessage> received = [];
        private readonly ConsensusServiceStub consensusServiceStub = new ();
        private Server server;
        private TopicMessageQuery topicMessageQuery;
        static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void Setup()
        {
            client = Client.ForNetwork([]);
            client.MirrorNetwork_.Network = ["in-process:test"];

            server = InProcessServerBuilder.ForName("test").AddService(consensusServiceStub).DirectExecutor().Start();

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
            server?.ShutdownAsync();
        }

        public virtual void Subscribe()
        {
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(Response(1));
            consensusServiceStub.responses.Append(Response(2));
            SubscribeToMirror(received.Add());
            Assert.Empty(errors);
            Assert.That(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
        }

        public virtual void SubscribeChunked()
        {
            Proto.ConsensusTopicResponse response1 = Response(1, 2);
            Proto.ConsensusTopicResponse response2 = Response(2, 2);
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(response1);
            consensusServiceStub.responses.Append(response2);
            SubscribeToMirror(received.Add());
            var message = Combine(response1.Message.ToByteArray(), response2.Message.ToByteArray());
            Assert.Empty(errors);
            Assert.Contains(received).HasSize(1).First()
                .Returns(ToDateTime(response2.GetConsensusTimestamp()), (t) => t.consensusTimestamp)
                .Returns(response2.GetChunkInfo().GetInitialTransactionID(), (t) => t.transactionId).ToProtobuf())
            .Returns(message, (t) => t.contents)
                .Returns(response2.GetRunningHash().ToByteArray(), (t) => t.runningHash)
                .Returns(response2.GetSequenceNumber(), (t) => t.sequenceNumber).Extracting((t) => t.chunks).AsInstanceOf(InstanceOfAssertFactories.ARRAY).HasSize(2).Extracting((c) => ((TopicMessageChunk)c).sequenceNumber, 1, 2);
        }

        public virtual void SubscribeNoResponse()
        {
            consensusServiceStub.requests.Append(Request());
            SubscribeToMirror(received.Add());
            Assert.Empty(errors);
            Assert.Empty(received);
        }

        public virtual void ErrorDuringOnNext()
        {
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(Response(1));
            SubscribeToMirror((t) =>
            {
                throw new Exception();
            });
            Assert.IsType<Exception>(errors).HasSize(1).First();
            Assert.Empty(received);
        }

        public virtual void RetryRecovers(ResponseStatus.Code code, string description)
        {
            Proto.ConsensusTopicResponse response = Response(1);
            DateTimeOffset nextTimestamp = ToDateTime(response.GetConsensusTimestamp()).PlusNanos(1);
			Proto.ConsensusTopicQuery request = Request();
            consensusServiceStub.requests.Append(request);
            consensusServiceStub.requests.Append(request.SetConsensusStartTime(ToTimestamp(nextTimestamp)));
            consensusServiceStub.responses.Append(response);
            consensusServiceStub.responses.Append(code.ToStatus().WithDescription(description).AsRuntimeException());
            consensusServiceStub.responses.Append(Response(2));
            SubscribeToMirror(received.Add());
            Assert.That(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
            Assert.Empty(errors);
        }

        public virtual void NoRetry(ResponseStatus.Code code, string description)
        {
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(code.ToStatus().WithDescription(description).AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assert.Empty(received);
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus().GetCode(), code);
        }

        public virtual void CustomRetry()
        {
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(ResponseStatus.INVALID_ARGUMENT.AsRuntimeException());
            consensusServiceStub.responses.Append(Response(1));
            topicMessageQuery
                .SetRetryHandler((t) => true);
            SubscribeToMirror(received.Add());
            Assert.That(received).HasSize(1).Extracting((t) => t.sequenceNumber).ContainsExactly(1);
            Assert.Empty(errors);
        }

        public virtual void RetryWithLimit()
        {
            Proto.ConsensusTopicResponse response = Response(1);
            DateTimeOffset nextTimestamp = ToDateTime(response.GetConsensusTimestamp()).PlusNanos(1);
            Proto.ConsensusTopicQuery request = Request();
            topicMessageQuery
                .SetLimit(2);
            consensusServiceStub.requests.Append(request
                .SetLimit(2));
            consensusServiceStub.requests.Append(request
                .SetConsensusStartTime(ToTimestamp(nextTimestamp))
                .SetLimit(1));
            consensusServiceStub.responses.Append(response);
            consensusServiceStub.responses.Append(ResponseStatus.RESOURCE_EXHAUSTED.AsRuntimeException());
            consensusServiceStub.Responses.Add(Response(2));
            SubscribeToMirror(received.Add());
            Assert.That(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
            Assert.Empty(errors);
        }

        public virtual void RetriesExhausted()
        {
            topicMessageQuery
                .SetMaxAttempts(1);
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(ResponseStatus.RESOURCE_EXHAUSTED.AsRuntimeException());
            consensusServiceStub.responses.Append(ResponseStatus.RESOURCE_EXHAUSTED.AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assert.Empty(received);
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus(), Status.RESOURCE_EXHAUSTED);
        }

        public virtual void ErrorWhenCallIsCancelled()
        {
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(ResponseStatus.CANCELLED.AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus(), Status.CANCELLED);
            Assert.Empty(received);
        }

        public virtual void UnsubscribeDoesNotInvokeErrorOrRetry()
        {
            consensusServiceStub.requests.Append(Request());
            SubscriptionHandle handle = topicMessageQuery.Subscribe(client, received.Add());
            handle.Unsubscribe();
            Uninterruptibles.SleepUninterruptibly(100, TimeUnit.MILLISECONDS);
            Assert.Empty(errors);
            Assert.Empty(received);
        }

        public virtual void ServerCancelledRetriesWhenCustomRetryAllows()
        {
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(ResponseStatus.CANCELLED.AsRuntimeException());
            consensusServiceStub.responses.Append(Response(1));
            topicMessageQuery
                .SetRetryHandler((t) =>
            {
                if (t is StatusRuntimeException)
                {
                    return sre.GetStatus().GetCode() == Status.Code.CANCELLED;
                }

                return false;
            });
            SubscribeToMirror(received.Add());
            Assert.That(received).HasSize(1).Extracting((t) => t.sequenceNumber).ContainsExactly(1);
            Assert.Empty(errors);
        }

        public virtual void UnsubscribeThenResubscribeResetsClientCancelFlagAllowsRetryOnCancelled()
        {
            consensusServiceStub.requests.Append(Request());
            SubscriptionHandle firstHandle = topicMessageQuery.Subscribe(client, received.Add());
            firstHandle.Unsubscribe();
            Uninterruptibles.SleepUninterruptibly(100, TimeUnit.MILLISECONDS);
            Assert.Empty(errors);
            Assert.Empty(received);
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.requests.Append(Request());
            consensusServiceStub.responses.Append(ResponseStatus.CANCELLED.AsRuntimeException());
            consensusServiceStub.responses.Append(Response(1));
            topicMessageQuery
                .SetRetryHandler((t) =>
            {
                if (t is StatusRuntimeException)
                {
                    return sre.GetStatus().GetCode() == Status.Code.CANCELLED;
                }

                return false;
            });
            SubscriptionHandle secondHandle = topicMessageQuery.Subscribe(client, received.Add());
            Stopwatch stopwatch = Stopwatch.CreateStarted();
            while (received.Count < 1 && stopwatch.Elapsed(TimeUnit.SECONDS) < 3)
            {
                Uninterruptibles.SleepUninterruptibly(50, TimeUnit.MILLISECONDS);
            }

            Assert.Empty(errors);
            Assert.That(received).HasSize(1).Extracting((t) => t.sequenceNumber).ContainsExactly(1);
            secondHandle.Unsubscribe();
        }

        private void SubscribeToMirror(Consumer<TopicMessage> onNext)
        {
            SubscriptionHandle subscriptionHandle = topicMessageQuery.Subscribe(client, onNext);
            Stopwatch stopwatch = Stopwatch.CreateStarted();
            while (!complete.Get() && errors.IsEmpty() && stopwatch.Elapsed(TimeUnit.SECONDS) < 3)
            {
                Uninterruptibles.SleepUninterruptibly(100, TimeUnit.MILLISECONDS);
            }

            subscriptionHandle.Unsubscribe();
        }

        private static Proto.ConsensusTopicQuery Request()
        {
            return Proto.ConsensusTopicQuery.NewBuilder()
                .SetConsensusEndTime(ToTimestamp(START_TIME.AddSeconds(100)))
                .SetConsensusStartTime(ToTimestamp(START_TIME))
                .SetTopicID(TopicID.NewBuilder()
                .SetTopicNum(1000));
        }

        private static Proto.ConsensusTopicResponse Response(long sequenceNumber)
        {
            return Response(sequenceNumber, 0);
        }

        private static Proto.ConsensusTopicResponse Response(long sequenceNumber, int total)
        {
            Proto.ConsensusTopicResponse consensusTopicResponseBuilder = Proto.ConsensusTopicResponse.NewBuilder();
            if (total > 0)
            {
                var chunkInfo = ConsensusMessageChunkInfo.NewBuilder()
                    .SetInitialTransactionID(TransactionID.NewBuilder()
                    .SetAccountID(AccountID.NewBuilder()
                    .SetAccountNum(3))
                    .SetTransactionValidStart(ToTimestamp(START_TIME)))
                    .SetNumber((int)sequenceNumber)
                    .SetTotal(total);
                consensusTopicResponseBuilder
                    .SetChunkInfo(chunkInfo);
            }

            var message = ByteString.CopyFrom(Longs.ToByteArray(sequenceNumber));

            return consensusTopicResponseBuilder
                .SetConsensusTimestamp(ToTimestamp(START_TIME.AddSeconds(sequenceNumber)))
                .SetSequenceNumber(sequenceNumber)
                .SetMessage(message)
                .SetRunningHash(message)
                .SetRunningHashVersion(2);
        }

        private static DateTimeOffset ToDateTime(Timestamp timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Seconds, timestamp.Nanos);
        }

        private static Timestamp ToTimestamp(DateTimeOffset instant)
        {
            return new Timestamp
            {
                Seconds = instant.ToUnixTimeSeconds(),
                Nanos = instant.Nanosecond,
            };
        }

        private class ConsensusServiceStub : Proto.ConsensusService.ConsensusServiceBase
        {
            public readonly Queue<Proto.ConsensusTopicQuery> requests = new ();
            public readonly Queue<object> responses = new ();

            public override Task subscribeTopic(Proto.ConsensusTopicQuery request, IServerStreamWriter<Proto.ConsensusTopicResponse> streamObserver, ServerCallContext context)
            {
                var request = requests.Poll();
                Assert.NotNull(request);
                Assert.Equal(consensusTopicQuery, request);
                while (!responses.IsEmpty())
                {
                    var response = responses.Poll();
                    Assert.NotNull(response);
                    if (response is Exception)
                    {
                        streamObserver.OnError((Exception)response);
                        return;
                    }

                    streamObserver.WriteAsync((Proto.ConsensusTopicResponse)response);
                }

                return base.subscribeTopic(request, responseStream, context);
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