// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Common.Base;
using Com.Google.Common.Primitives;
using Com.Google.Common.Util.Concurrent;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Com.Hedera.Hashgraph.Sdk.Proto.Mirror;
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

namespace Com.Hedera.Hashgraph.Sdk
{
    class TopicMessageQueryTest
    {
        private static readonly Instant START_TIME = Instant.Now();
        private Client client;
        private readonly AtomicBoolean complete = new AtomicBoolean(false);
        private readonly IList<Throwable> errors = new List();
        private readonly IList<TopicMessage> received = new List();
        private readonly ConsensusServiceStub consensusServiceStub = new ConsensusServiceStub();
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

        virtual void Setup()
        {
            client = Client.ForNetwork(Collections.EmptyMap());
            client.SetMirrorNetwork(List.Of("in-process:test"));
            server = InProcessServerBuilder.ForName("test").AddService(consensusServiceStub).DirectExecutor().Build().Start();
            topicMessageQuery = new TopicMessageQuery();
            topicMessageQuery.SetCompletionHandler(() => complete.Set(true));
            topicMessageQuery.SetEndTime(START_TIME.PlusSeconds(100));
            topicMessageQuery.SetErrorHandler((t, r) => errors.Add(t));
            topicMessageQuery.SetMaxBackoff(Duration.OfMillis(500));
            topicMessageQuery.SetStartTime(START_TIME);
            topicMessageQuery.SetTopicId(TopicId.FromString("0.0.1000"));
        }

        virtual void Teardown()
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

        virtual void SetCompletionHandlerNull()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetCompletionHandler(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("completionHandler must not be null");
        }

        virtual void SetEndTimeNull()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetEndTime(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("endTime must not be null");
        }

        virtual void SetErrorHandlerNull()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetErrorHandler(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("errorHandler must not be null");
        }

        virtual void SetMaxAttemptsNegative()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetMaxAttempts(-1)).IsInstanceOf(typeof(ArgumentException)).HasMessage("maxAttempts must be positive");
        }

        virtual void SetMaxBackoffNull()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetMaxBackoff(null)).IsInstanceOf(typeof(ArgumentException)).HasMessage("maxBackoff must be at least 500 ms");
        }

        virtual void SetMaxBackoffLow()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetMaxBackoff(Duration.OfMillis(499))).IsInstanceOf(typeof(ArgumentException)).HasMessage("maxBackoff must be at least 500 ms");
        }

        virtual void SetRetryHandlerNull()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetRetryHandler(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("retryHandler must not be null");
        }

        virtual void SetStartTimeNull()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetStartTime(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("startTime must not be null");
        }

        virtual void SetTopicIdNull()
        {
            AssertThatThrownBy(() => topicMessageQuery.SetTopicId(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("topicId must not be null");
        }

        virtual void Subscribe()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(Response(1));
            consensusServiceStub.responses.Add(Response(2));
            SubscribeToMirror(received.Add());
            Assert.Empty(errors);
            Assertions.AssertThat(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
        }

        virtual void SubscribeChunked()
        {
            ConsensusTopicResponse response1 = Response(1, 2);
            ConsensusTopicResponse response2 = Response(2, 2);
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(response1);
            consensusServiceStub.responses.Add(response2);
            SubscribeToMirror(received.Add());
            var message = Combine(response1.GetMessage().ToByteArray(), response2.GetMessage().ToByteArray());
            Assert.Empty(errors);
            Assertions.AssertThat(received).HasSize(1).First().Returns(ToInstant(response2.GetConsensusTimestamp()), (t) => t.consensusTimestamp).Returns(response2.GetChunkInfo().GetInitialTransactionID(), (t) => Objects.RequireNonNull(t.transactionId).ToProtobuf()).Returns(message, (t) => t.contents).Returns(response2.GetRunningHash().ToByteArray(), (t) => t.runningHash).Returns(response2.GetSequenceNumber(), (t) => t.sequenceNumber).Extracting((t) => t.chunks).AsInstanceOf(InstanceOfAssertFactories.ARRAY).HasSize(2).Extracting((c) => ((TopicMessageChunk)c).sequenceNumber).Contains(1, 2);
        }

        virtual void SubscribeNoResponse()
        {
            consensusServiceStub.requests.Add(Request().Build());
            SubscribeToMirror(received.Add());
            Assert.Empty(errors);
            Assertions.Assert.Empty(received);
        }

        virtual void ErrorDuringOnNext()
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

        virtual void RetryRecovers(Status.Code code, string description)
        {
            ConsensusTopicResponse response = Response(1);
            Instant nextTimestamp = ToInstant(response.GetConsensusTimestamp()).PlusNanos(1);
            ConsensusTopicQuery.Builder request = Request();
            consensusServiceStub.requests.Add(request.Build());
            consensusServiceStub.requests.Add(request.SetConsensusStartTime(ToTimestamp(nextTimestamp)).Build());
            consensusServiceStub.responses.Add(response);
            consensusServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            consensusServiceStub.responses.Add(Response(2));
            SubscribeToMirror(received.Add());
            Assertions.AssertThat(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
            Assert.Empty(errors);
        }

        virtual void NoRetry(Status.Code code, string description)
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assertions.Assert.Empty(received);
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus().GetCode(), code);
        }

        virtual void CustomRetry()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(Status.INVALID_ARGUMENT.AsRuntimeException());
            consensusServiceStub.responses.Add(Response(1));
            topicMessageQuery.SetRetryHandler((t) => true);
            SubscribeToMirror(received.Add());
            Assertions.AssertThat(received).HasSize(1).Extracting((t) => t.sequenceNumber).ContainsExactly(1);
            Assert.Empty(errors);
        }

        virtual void RetryWithLimit()
        {
            ConsensusTopicResponse response = Response(1);
            Instant nextTimestamp = ToInstant(response.GetConsensusTimestamp()).PlusNanos(1);
            ConsensusTopicQuery.Builder request = Request();
            topicMessageQuery.SetLimit(2);
            consensusServiceStub.requests.Add(request.SetLimit(2).Build());
            consensusServiceStub.requests.Add(request.SetConsensusStartTime(ToTimestamp(nextTimestamp)).SetLimit(1).Build());
            consensusServiceStub.responses.Add(response);
            consensusServiceStub.responses.Add(Status.RESOURCE_EXHAUSTED.AsRuntimeException());
            consensusServiceStub.responses.Add(Response(2));
            SubscribeToMirror(received.Add());
            Assertions.AssertThat(received).HasSize(2).Extracting((t) => t.sequenceNumber).ContainsExactly(1, 2);
            Assert.Empty(errors);
        }

        virtual void RetriesExhausted()
        {
            topicMessageQuery.SetMaxAttempts(1);
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(Status.RESOURCE_EXHAUSTED.AsRuntimeException());
            consensusServiceStub.responses.Add(Status.RESOURCE_EXHAUSTED.AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assertions.Assert.Empty(received);
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus(), Status.RESOURCE_EXHAUSTED);
        }

        virtual void ErrorWhenCallIsCancelled()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(Status.CANCELLED.AsRuntimeException());
            SubscribeToMirror(received.Add());
            Assert.Equal(errors).HasSize(1).First().IsInstanceOf(typeof(StatusRuntimeException)).Extracting((t) => ((StatusRuntimeException)t).GetStatus(), Status.CANCELLED);
            Assertions.Assert.Empty(received);
        }

        virtual void UnsubscribeDoesNotInvokeErrorOrRetry()
        {
            consensusServiceStub.requests.Add(Request().Build());
            SubscriptionHandle handle = topicMessageQuery.Subscribe(client, received.Add());
            handle.Unsubscribe();
            Uninterruptibles.SleepUninterruptibly(100, TimeUnit.MILLISECONDS);
            Assert.Empty(errors);
            Assertions.Assert.Empty(received);
        }

        virtual void ServerCancelledRetriesWhenCustomRetryAllows()
        {
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(Status.CANCELLED.AsRuntimeException());
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

        virtual void UnsubscribeThenResubscribeResetsClientCancelFlagAllowsRetryOnCancelled()
        {
            consensusServiceStub.requests.Add(Request().Build());
            SubscriptionHandle firstHandle = topicMessageQuery.Subscribe(client, received.Add());
            firstHandle.Unsubscribe();
            Uninterruptibles.SleepUninterruptibly(100, TimeUnit.MILLISECONDS);
            Assert.Empty(errors);
            Assertions.Assert.Empty(received);
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.requests.Add(Request().Build());
            consensusServiceStub.responses.Add(Status.CANCELLED.AsRuntimeException());
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

        private static ConsensusTopicQuery.Builder Request()
        {
            return ConsensusTopicQuery.NewBuilder().SetConsensusEndTime(ToTimestamp(START_TIME.PlusSeconds(100))).SetConsensusStartTime(ToTimestamp(START_TIME)).SetTopicID(TopicID.NewBuilder().SetTopicNum(1000).Build());
        }

        private static ConsensusTopicResponse Response(long sequenceNumber)
        {
            return Response(sequenceNumber, 0);
        }

        private static ConsensusTopicResponse Response(long sequenceNumber, int total)
        {
            ConsensusTopicResponse.Builder consensusTopicResponseBuilder = ConsensusTopicResponse.NewBuilder();
            if (total > 0)
            {
                var chunkInfo = ConsensusMessageChunkInfo.NewBuilder().SetInitialTransactionID(TransactionID.NewBuilder().SetAccountID(AccountID.NewBuilder().SetAccountNum(3).Build()).SetTransactionValidStart(ToTimestamp(START_TIME)).Build()).SetNumber((int)sequenceNumber).SetTotal(total).Build();
                consensusTopicResponseBuilder.SetChunkInfo(chunkInfo);
            }

            var message = ByteString.CopyFrom(Longs.ToByteArray(sequenceNumber));
            return consensusTopicResponseBuilder.SetConsensusTimestamp(ToTimestamp(START_TIME.PlusSeconds(sequenceNumber))).SetSequenceNumber(sequenceNumber).SetMessage(message).SetRunningHash(message).SetRunningHashVersion(2).Build();
        }

        private static Instant ToInstant(Timestamp timestamp)
        {
            return Instant.OfEpochSecond(timestamp.GetSeconds(), timestamp.GetNanos());
        }

        private static Timestamp ToTimestamp(Instant instant)
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
                AssertThat(request).IsNotNull();
                Assert.Equal(consensusTopicQuery, request);
                while (!responses.IsEmpty())
                {
                    var response = responses.Poll();
                    AssertThat(response).IsNotNull();
                    if (response is Throwable)
                    {
                        streamObserver.OnError((Throwable)response);
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