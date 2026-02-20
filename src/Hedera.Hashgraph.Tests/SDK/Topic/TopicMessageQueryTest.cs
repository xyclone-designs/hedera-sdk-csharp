// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Common.Base;
using Com.Google.Common.Primitives;
using Com.Google.Common.Util.Concurrent;
using Com.Google.Protobuf;
using Proto;
using Proto.Mirror;
using Io.Github.JsonSnapshot;
using Io.Grpc;
using Io.Grpc.Inprocess;
using Io.Grpc.Stub;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Concurrent.Atomic;
using Java.Util.Function;
using Org.Assertj.Core.Api;
using Org.Junit.Jupiter.Api;
using Org.Junit.Jupiter.Params;
using Org.Junit.Jupiter.Params.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK;
using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    class TopicMessageQueryTest
    {
        private static readonly DateTimeOffset START_TIME = DateTimeOffset.UtcNow;
        private Client client;
        private readonly AtomicBoolean complete = new AtomicBoolean(false);
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
            client.MirrorNetwork_ = ["in-process:test"];

            server = InProcessServerBuilder.ForName("test").AddService(consensusServiceStub).DirectExecutor().Build().Start();

            topicMessageQuery = new TopicMessageQuery
            {
                CompletionHandler = () => complete.Set(true),
                EndTime = Timestamp.FromDateTimeOffset(START_TIME.AddSeconds(100)),
                ErrorHandler = (t, r) => errors.Add(t),
                MaxBackoff = Duration.FromTimeSpan(TimeSpan.FromMilliseconds(500)),
                StartTime = Timestamp.FromDateTimeOffset(START_TIME),
                TopicId = TopicId.FromString("0.0.1000")
            };
        }

        public virtual void Teardown()
        {
            consensusServiceStub.Verify();
            if (client != null)
            {
                client.Dispose();
            }

            if (server != null)
            {
                server.Shutdown();
                server.AwaitTermination();
            }
        }

        public virtual void Subscribe()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(Response(1));
            consensusServiceStub.responses.Add(Response(2));
            SubscribeToMirror(received.Add());
            Assert.Empty(errors);
            Assertions.AssertThat(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
        }

        public virtual void SubscribeChunked()
        {
            Proto.ConsensusTopicResponse response1 = Response(1, 2);
            Proto.ConsensusTopicResponse response2 = Response(2, 2);
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(response1);
            consensusServiceStub.responses.Add(response2);
            SubscribeToMirror(received.Add());
            var message = Combine(response1.GetMessage().ToByteArray(), response2.GetMessage().ToByteArray());
            Assert.Empty(errors);
            Assertions.Assert.Contains(received).HasSize(1).First().Returns(ToInstant(response2.GetConsensusTimestamp()), (t) => t.consensusTimestamp).Returns(response2.GetChunkInfo().GetInitialTransactionID(), (t) => t.transactionId).ToProtobuf()).Returns(message, (t) => t.contents).Returns(response2.GetRunningHash().ToByteArray(), (t) => t.runningHash).Returns(response2.GetSequenceNumber(), (t) => t.sequenceNumber).Extracting((t) => t.chunks).AsInstanceOf(InstanceOfAssertFactories.ARRAY).HasSize(2).Extracting((c) => ((TopicMessageChunk)c).sequenceNumber, 1, 2);
        }

        public virtual void SubscribeNoResponse()
        {
            consensusServiceStub.requests.Add(Request().Build());
            SubscribeToMirror(received.Add());
            Assert.Empty(errors);
            Assertions.Assert.Empty(received);
        }

        public virtual void ErrorDuringOnNext()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(Response(1));
            SubscribeToMirror((t) =>
            {
                throw new Exception();
            });
            Assert.IsType<Exception>(errors).HasSize(1).First();
            Assertions.Assert.Empty(received);
        }

        public virtual void RetryRecovers(ResponseStatus.Code code, string description)
        {
            Proto.ConsensusTopicResponse response = Response(1);
            DateTimeOffset nextTimestamp = ToInstant(response.GetConsensusTimestamp()).PlusNanos(1);
			Proto.ConsensusTopicQuery request = Request();
            consensusServiceStub.requests.Add(request.Build());
            consensusServiceStub.requests.Add(request.SetConsensusStartTime(ToTimestamp(nextTimestamp)).Build());
            consensusServiceStub.responses.Add(response);
            consensusServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            consensusServiceStub.responses.Add(Response(2));
            SubscribeToMirror(received.Add());
            Assertions.AssertThat(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
            Assert.Empty(errors);
        }

        public virtual void NoRetry(ResponseStatus.Code code, string description)
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assertions.Assert.Empty(received);
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus().GetCode(), code);
        }

        public virtual void CustomRetry()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(ResponseStatus.INVALID_ARGUMENT.AsRuntimeException());
            consensusServiceStub.responses.Add(Response(1));
            topicMessageQuery.SetRetryHandler((t) => true);
            SubscribeToMirror(received.Add());
            Assertions.AssertThat(received).HasSize(1).Extracting((t) => t.sequenceNumber).ContainsExactly(1);
            Assert.Empty(errors);
        }

        public virtual void RetryWithLimit()
        {
            Proto.ConsensusTopicResponse response = Response(1);
            DateTimeOffset nextTimestamp = ToInstant(response.GetConsensusTimestamp()).PlusNanos(1);
            Proto.ConsensusTopicQuery request = Request();
            topicMessageQuery.SetLimit(2);
            consensusServiceStub.requests.Add(request.SetLimit(2).Build());
            consensusServiceStub.requests.Add(request.SetConsensusStartTime(ToTimestamp(nextTimestamp)).SetLimit(1).Build());
            consensusServiceStub.responses.Add(response);
            consensusServiceStub.responses.Add(ResponseStatus.RESOURCE_EXHAUSTED.AsRuntimeException());
            consensusServiceStub.Responses.Add(Response(2));
            SubscribeToMirror(received.Add());
            Assertions.AssertThat(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
            Assert.Empty(errors);
        }

        public virtual void RetriesExhausted()
        {
            topicMessageQuery.SetMaxAttempts(1);
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(ResponseStatus.RESOURCE_EXHAUSTED.AsRuntimeException());
            consensusServiceStub.responses.Add(ResponseStatus.RESOURCE_EXHAUSTED.AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assertions.Assert.Empty(received);
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus(), Status.RESOURCE_EXHAUSTED);
        }

        public virtual void ErrorWhenCallIsCancelled()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(ResponseStatus.CANCELLED.AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus(), Status.CANCELLED);
            Assertions.Assert.Empty(received);
        }

        public virtual void UnsubscribeDoesNotInvokeErrorOrRetry()
        {
            consensusServiceStub.requests.Add(Request().Build());
            SubscriptionHandle handle = topicMessageQuery.Subscribe(client, received.Add());
            handle.Unsubscribe();
            Uninterruptibles.SleepUninterruptibly(100, TimeUnit.MILLISECONDS);
            Assert.Empty(errors);
            Assertions.Assert.Empty(received);
        }

        public virtual void ServerCancelledRetriesWhenCustomRetryAllows()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(ResponseStatus.CANCELLED.AsRuntimeException());
            consensusServiceStub.responses.Add(Response(1));
            topicMessageQuery.SetRetryHandler((t) =>
            {
                if (t is StatusRuntimeException)
                {
                    return sre.GetStatus().GetCode() == Status.Code.CANCELLED;
                }

                return false;
            });
            SubscribeToMirror(received.Add());
            Assertions.AssertThat(received).HasSize(1).Extracting((t) => t.sequenceNumber).ContainsExactly(1);
            Assert.Empty(errors);
        }

        public virtual void UnsubscribeThenResubscribeResetsClientCancelFlagAllowsRetryOnCancelled()
        {
            consensusServiceStub.requests.Add(Request().Build());
            SubscriptionHandle firstHandle = topicMessageQuery.Subscribe(client, received.Add());
            firstHandle.Unsubscribe();
            Uninterruptibles.SleepUninterruptibly(100, TimeUnit.MILLISECONDS);
            Assert.Empty(errors);
            Assertions.Assert.Empty(received);
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(ResponseStatus.CANCELLED.AsRuntimeException());
            consensusServiceStub.responses.Add(Response(1));
            topicMessageQuery.SetRetryHandler((t) =>
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
            Assertions.AssertThat(received).HasSize(1).Extracting((t) => t.sequenceNumber).ContainsExactly(1);
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
            return Proto.ConsensusTopicQuery.NewBuilder().SetConsensusEndTime(ToTimestamp(START_TIME.PlusSeconds(100))).SetConsensusStartTime(ToTimestamp(START_TIME)).SetTopicID(TopicID.NewBuilder().SetTopicNum(1000).Build());
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
                var chunkInfo = ConsensusMessageChunkInfo.NewBuilder().SetInitialTransactionID(TransactionID.NewBuilder().SetAccountID(AccountID.NewBuilder().SetAccountNum(3).Build()).SetTransactionValidStart(ToTimestamp(START_TIME)).Build()).SetNumber((int)sequenceNumber).SetTotal(total).Build();
                consensusTopicResponseBuilder.SetChunkInfo(chunkInfo);
            }

            var message = ByteString.CopyFrom(Longs.ToByteArray(sequenceNumber));
            return consensusTopicResponseBuilder.SetConsensusTimestamp(ToTimestamp(START_TIME.PlusSeconds(sequenceNumber))).SetSequenceNumber(sequenceNumber).SetMessage(message).SetRunningHash(message).SetRunningHashVersion(2).Build();
        }

        private static DateTimeOffset ToInstant(Timestamp timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp.GetSeconds(), timestamp.GetNanos());
        }

        private static Timestamp ToTimestamp(DateTimeOffset instant)
        {
            return Timestamp.NewBuilder().SetSeconds(instant.GetEpochSecond()).SetNanos(instant.GetNano()).Build();
        }

        private class ConsensusServiceStub : ConsensusServiceImplBase
        {
            private readonly Queue<ConsensusTopicQuery> requests = new ArrayDeque();
            private readonly Queue<object> responses = new ArrayDeque();
            public override void SubscribeTopic(ConsensusTopicQuery consensusTopicQuery, StreamObserver<ConsensusTopicResponse> streamObserver)
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

                    streamObserver.OnNext((ConsensusTopicResponse)response);
                }

                streamObserver.OnCompleted();
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