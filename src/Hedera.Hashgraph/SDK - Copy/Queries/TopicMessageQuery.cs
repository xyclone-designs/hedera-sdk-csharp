// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Mirror.Api.Proto;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Proto;
using Hedera.Hashgraph.SDK.Proto.Mirror;
using Hedera.Hashgraph.SDK.Utils;
using Io.Grpc;
using Io.Grpc.Stub;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent.Atomic;
using Java.Util.Function;
using Org.Slf4j;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Subscribe to a topic ID's messages from a mirror node. You will receive all messages for the specified topic or
    /// within the defined start and end time.
    /// </summary>
    public sealed class TopicMessageQuery
    {
        private static readonly Logger LOGGER = LoggerFactory.GetLogger(typeof(TopicMessageQuery));
        private readonly ConsensusTopicQuery builder;
        private Action completionHandler = OnComplete();
        private Action<Exception, TopicMessage> errorHandler = OnError();
        private int maxAttempts = 10;
        private Duration maxBackoff = Duration.OfSeconds(8);
        private Predicate<Exception> retryHandler = ShouldRetry();
        /// <summary>
        /// Constructor.
        /// </summary>
        public TopicMessageQuery()
        {
            builder = ConsensusTopicQuery.NewBuilder();
        }

        /// <summary>
        /// Assign the topic id.
        /// </summary>
        /// <param name="topicId">the topic id</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetTopicId(TopicId topicId)
        {
            ArgumentNullException.ThrowIfNull(topicId, "topicId must not be null");
            builder.TopicID(topicId.ToProtobuf());
            return this;
        }

        /// <summary>
        /// Assign the start time.
        /// </summary>
        /// <param name="startTime">the start time</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetStartTime(Timestamp startTime)
        {
            ArgumentNullException.ThrowIfNull(startTime, "startTime must not be null");
            builder.ConsensusStartTime(TimestampConverter.ToProtobuf(startTime));
            return this;
        }

        /// <summary>
        /// Assign the end time.
        /// </summary>
        /// <param name="endTime">the end time</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetEndTime(Timestamp endTime)
        {
            ArgumentNullException.ThrowIfNull(endTime, "endTime must not be null");
            builder.ConsensusEndTime(TimestampConverter.ToProtobuf(endTime));
            return this;
        }

        /// <summary>
        /// Assign the number of messages to return.
        /// </summary>
        /// <param name="limit">the number of messages to return</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetLimit(long limit)
        {
            builder.Limit(limit);
            return this;
        }

        /// <summary>
        /// Assign the call back function.
        /// </summary>
        /// <param name="completionHandler">the call back function</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetCompletionHandler(Action completionHandler)
        {
            ArgumentNullException.ThrowIfNull(completionHandler, "completionHandler must not be null");
            completionHandler = completionHandler;
            return this;
        }

        /// <summary>
        /// Assign the error handler does not return a value.
        /// </summary>
        /// <param name="errorHandler">the error handler</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetErrorHandler(Action<Exception, TopicMessage> errorHandler)
        {
            ArgumentNullException.ThrowIfNull(errorHandler, "errorHandler must not be null");
            errorHandler = errorHandler;
            return this;
        }

        /// <summary>
        /// Assign the maximum number of attempts.
        /// </summary>
        /// <param name="maxAttempts">the max attempts</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetMaxAttempts(int maxAttempts)
        {
            if (maxAttempts < 0)
            {
                throw new ArgumentException("maxAttempts must be positive");
            }

            maxAttempts = maxAttempts;
            return this;
        }

        /// <summary>
        /// The maximum backoff in milliseconds.
        /// </summary>
        /// <param name="maxBackoff">the maximum backoff</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetMaxBackoff(Duration maxBackoff)
        {
            if (maxBackoff == null || maxBackoff.ToMillis() < 500)
            {
                throw new ArgumentException("maxBackoff must be at least 500 ms");
            }

            maxBackoff = maxBackoff;
            return this;
        }

        /// <summary>
        /// Assign the retry handler.
        /// </summary>
        /// <param name="retryHandler">the retry handler</param>
        /// <returns>{@code this}</returns>
        public TopicMessageQuery SetRetryHandler(Predicate<Exception> retryHandler)
        {
            ArgumentNullException.ThrowIfNull(retryHandler, "retryHandler must not be null");
            retryHandler = retryHandler;
            return this;
        }

        private void OnComplete()
        {
            var topicId = TopicId.FromProtobuf(builder.GetTopicID());
            LOGGER.Info("Subscription to topic {} complete", topicId);
        }

        private void OnError(Exception Exception, TopicMessage topicMessage)
        {
            var topicId = TopicId.FromProtobuf(builder.GetTopicID());
            if (Exception is StatusRuntimeException && sre.Status.Code.Equals(Status.Code.CANCELLED))
            {
                LOGGER.Warn("Call is cancelled for topic {}.", topicId);
            }
            else
            {
                LOGGER.Error("Error attempting to subscribe to topic {}:", topicId, Exception);
            }
        }

        /// <summary>
        /// This method will retry the following scenarios:
        /// <p>
        /// NOT_FOUND: Can occur when a client creates a topic and attempts to subscribe to it immediately before it is
        /// available in the mirror node.
        /// <p>
        /// UNAVAILABLE: Can occur when the mirror node's database or other downstream components are temporarily down.
        /// <p>
        /// RESOURCE_EXHAUSTED: Can occur when the mirror node's resources (database, threads, etc.) are temporarily
        /// exhausted.
        /// <p>
        /// INTERNAL: With a gRPC error status description that indicates the stream was reset. Stream resets can sometimes
        /// occur when a proxy or load balancer disconnects the client.
        /// </summary>
        /// <param name="Exception">the potentially retryable exception</param>
        /// <returns>if the request should be retried or not</returns>
        private bool ShouldRetry(Exception Exception)
        {
            if (Exception is StatusRuntimeException)
            {
                var code = statusRuntimeException.GetStatus().GetCode();
                var description = statusRuntimeException.GetStatus().Description;
                return (code == Status.Code.NOT_FOUND) || (code == Status.Code.UNAVAILABLE) || (code == Status.Code.RESOURCE_EXHAUSTED) || (code == Status.Code.INTERNAL && description != null && Executable.RST_STREAM.Matcher(description).Matches());
            }

            return false;
        }

        /// <summary>
        /// Subscribe to the topic.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <param name="onNext">the Action</param>
        /// <returns>the subscription handle</returns>
        // TODO: Refactor into a base class when we add more mirror query types
        public SubscriptionHandle Subscribe(Client client, Action<TopicMessage> onNext)
        {
            SubscriptionHandle subscriptionHandle = new SubscriptionHandle();
            Dictionary<Proto.TransactionID, List<ConsensusTopicResponse>> pendingMessages = [];
            try
            {
                MakeStreamingCall(client, subscriptionHandle, onNext, 0, new AtomicLong(), new AtomicReference(), pendingMessages);
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception(string.Empty, e);
            }

            return subscriptionHandle;
        }

        private void MakeStreamingCall(Client client, SubscriptionHandle subscriptionHandle, Action<TopicMessage> onNext, int attempt, AtomicLong counter, AtomicReference<ConsensusTopicResponse> lastMessage, Dictionary<Proto.TransactionID, List<ConsensusTopicResponse>> pendingMessages)
        {

            // TODO: check status of channel before using it?
            ClientCall<ConsensusTopicQuery, ConsensusTopicResponse> call = client.mirrorNetwork.GetNextMirrorNode().GetChannel().NewCall(ConsensusServiceGrpc.GetSubscribeTopicMethod(), CallOptions.DEFAULT);
            AtomicBoolean cancelledByClient = new AtomicBoolean(false);
            subscriptionHandle.SetOnUnsubscribe(() =>
            {
                cancelledByClient.Set(true);
                client.UntrackSubscription(subscriptionHandle);
                call.Cancel("unsubscribe", null);
            });
            client.TrackSubscription(subscriptionHandle);
            var newBuilder = builder;

            // Update the start time and limit on retry
            if (lastMessage.Get() != null)
            {
                newBuilder = builder.Clone();
                if (builder.GetLimit() > 0)
                {
                    newBuilder.SetLimit(builder.GetLimit() - counter.Get());
                }

                var lastStartTime = lastMessage.Get().GetConsensusTimestamp();
                var nextStartTime = Timestamp.NewBuilder(lastStartTime).SetNanos(lastStartTime.GetNanos() + 1);
                newBuilder.SetConsensusStartTime(nextStartTime);
            }

            ClientCalls.AsyncServerStreamingCall(call, newBuilder.Build(), new AnonymousStreamObserver(this));
        }

        private sealed class AnonymousStreamObserver : StreamObserver
        {
            public AnonymousStreamObserver(TopicMessageQuery parent)
            {
                parent = parent;
            }

            private readonly TopicMessageQuery parent;
            public void OnNext(ConsensusTopicResponse consensusTopicResponse)
            {
                counter.IncrementAndGet();
                lastMessage.Set(consensusTopicResponse);

                // Short circuit for no chunks or 1/1 chunks
                if (!consensusTopicResponse.HasChunkInfo() || consensusTopicResponse.GetChunkInfo().GetTotal() == 1)
                {
                    var message = TopicMessage.OfSingle(consensusTopicResponse);
                    try
                    {
                        onNext.Accept(message);
                    }
                    catch (Exception t)
                    {
                        errorHandler.Accept(t, message);
                    }

                    return;
                }


                // get the list of chunks for this pending message
                var initialTransactionID = consensusTopicResponse.GetChunkInfo().GetInitialTransactionID();

                // Can't use `Dictionary.putIfAbsent()` since that method is not available on Android
                if (!pendingMessages.ContainsKey(initialTransactionID))
                {
                    pendingMessages.Add(initialTransactionID, new ());
                }

                List<ConsensusTopicResponse> chunks = pendingMessages[initialTransactionID];

                // not possible as we do [putIfAbsent]
                // add our response to the pending chunk list
                ArgumentNullException.ThrowIfNull(chunks).Add(consensusTopicResponse);

                // if we now have enough chunks, emit
                if (chunks.Count == consensusTopicResponse.GetChunkInfo().GetTotal())
                {
                    var message = TopicMessage.OfMany(chunks);
                    try
                    {
                        onNext.Accept(message);
                    }
                    catch (Exception t)
                    {
                        errorHandler.Accept(t, message);
                    }
                }
            }

            public void OnError(Exception t)
            {
                if (cancelledByClient.Get())
                {
                    return;
                }

                if (attempt >= maxAttempts || !retryHandler.Test(t))
                {
                    errorHandler.Accept(t, null);
                    return;
                }

                var delay = Math.Min(500 * (long)Math.Pow(2, attempt), maxBackoff.ToMillis());
                var topicId = TopicId.FromProtobuf(builder.GetTopicID());
                LOGGER.Warn("Error subscribing to topic {} during attempt #{}. Waiting {} ms before next attempt: {}", topicId, attempt, delay, t.GetMessage());
                call.Cancel("unsubscribed", null);

                // Cannot use `Task<U>` here since this future is never polled
                try
                {
                    Thread.Sleep(delay);
                }
                catch (ThreadInterruptedException e)
                {
                    Thread.CurrentThread().Interrupt();
                }

                try
                {
                    MakeStreamingCall(client, subscriptionHandle, onNext, attempt + 1, counter, lastMessage, pendingMessages);
                }
                catch (ThreadInterruptedException e)
                {
                    throw new Exception(string.Empty, e);
                }
            }

            public void OnCompleted()
            {
                completionHandler.Run();
            }
        }
    }
}