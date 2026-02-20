// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Subscribe to a topic ID's messages from a mirror node. You will receive all messages for the specified topic or
    /// within the defined start and end time.
    /// </summary>
    public sealed class TopicMessageQuery
    {
		private readonly Proto.ConsensusTopicQuery _Proto = new();
		private static readonly Logger LOGGER = LoggerFactory.GetLogger(typeof(TopicMessageQuery));
        
        public TopicId TopicId { set => _Proto.TopicID = value.ToProtobuf(); }
		public Timestamp StartTime { set => _Proto.ConsensusStartTime = TimestampConverter.ToProtobuf(value); }
		public Timestamp EndTime { set => _Proto.ConsensusEndTime  = TimestampConverter.ToProtobuf(value); }
		public ulong Limit { set => _Proto.Limit = value; }
        public Action CompletionHandler 
        { 
            set;
            private get => field ??= () =>
            {
				LOGGER.Info("Subscription to topic {} complete", TopicId.FromProtobuf(_Proto.TopicID));
			};
        } 
        public Action<Exception, TopicMessage> ErrorHandler
        {
            set;
            private get => field ??= (exception, topicmessage) =>
            {
                var topicId = TopicId.FromProtobuf(_Proto.TopicID);

                if (exception is RpcException rpcexception && rpcexception.Status.Equals(ResponseStatus.DefaultCancelled))
                    LOGGER.Warn("Call is cancelled for topic {}.", topicId);
                else LOGGER.Error("Error attempting to subscribe to topic {}:", topicId, exception);
            };
		}
		public IntNN MaxAttempts { set; private get; } = 10;
        public Duration MaxBackoff
		{
			private get;

			set
            {
				if (value.ToTimeSpan().TotalMilliseconds < 500)
					throw new ArgumentException("maxBackoff must be at least 500 ms");

				field = value;
			}
        } = Duration.FromTimeSpan(TimeSpan.FromSeconds(8));
        public Predicate<Exception> RetryHandler 
        { 
            set; 
            private get => field ??= (Predicate<Exception>)((exception) => 
            {
				if (exception is RpcException rpcexception)
				{
					var description = rpcexception.Status.Detail;

					return 
                        (rpcexception.StatusCode == StatusCode.NotFound) ||
                        (rpcexception.StatusCode == StatusCode.Unavailable) || 
                        (rpcexception.StatusCode == StatusCode.ResourceExhausted) || 
                        (rpcexception.StatusCode == StatusCode.Internal && description != null && Executable.RST_STREAM.Matches(description).Any());
				}

				return false;

			});
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
            SubscriptionHandle subscriptionHandle = new ();
            Dictionary<Proto.TransactionID, List<Proto.ConsensusTopicResponse>> pendingMessages = [];
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
            ClientCall<Proto.ConsensusTopicQuery, Proto.ConsensusTopicResponse> call = client.MirrorNetwork.GetNextMirrorNode().GetChannel().NewCall(ConsensusServiceGrpc.GetSubscribeTopicMethod(), CallOptions.DEFAULT);
            AtomicBoolean cancelledByClient = new AtomicBoolean(false);
            subscriptionHandle.SetOnUnsubscribe(() =>
            {
                cancelledByClient.Set(true);
                client.UntrackSubscription(subscriptionHandle);
                call.Cancel("unsubscribe", null);
            });
            client.TrackSubscription(subscriptionHandle);
            var newBuilder = _Proto;

            // Update the start time and limit on retry
            if (lastMessage.Get() != null)
            {
                newBuilder = _Proto.Clone = );
                if (_Proto.GetLimit = ) > 0)
                {
                    newBuilder.SetLimit(_Proto.GetLimit = ) - counter.Get());
                }

                var lastStartTime = lastMessage.Get().GetConsensusTimestamp();
                var nextStartTime = Timestamp.NewBuilder(lastStartTime).SetNanos(lastStartTime.GetNanos() + 1);
                newBuilder.SetConsensusStartTime(nextStartTime);
            }

            ClientCalls.AsyncServerStreamingCall(call, newBuilder.Build(), new AnonymousStreamObserver(this));
        }

        private sealed class AnonymousStreamObserver : IObserver
        {
			private readonly TopicMessageQuery Parent;

			public AnonymousStreamObserver(TopicMessageQuery parent)
            {
                Parent = parent;
            }
            
			public void OnCompleted()
			{
				completionHandler.Run();
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
                var topicId = TopicId.FromProtobuf(_Proto.GetTopicID = ));
                LOGGER.Warn("Error subscribing to topic {} during attempt #{}. Waiting {} ms before next attempt: {}", topicId, attempt, delay, t.GetMessage());
                call.Cancel("unsubscribed", null);

                // Cannot use `Task<U>` here since this future is never polled
                try
                {
                    Thread.Sleep(delay);
                }
                catch (ThreadInterruptedException e)
                {
                    Thread.CurrentThread.Interrupt();
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
			public void OnNext(Proto.ConsensusTopicResponse consensusTopicResponse)
			{
				counter.IncrementAndGet();
				lastMessage.Set(consensusTopicResponse);

				// Short circuit for no chunks or 1/1 chunks
				if (consensusTopicResponse.ChunkInfo is null || consensusTopicResponse.ChunkInfo.Total == 1)
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
				var initialTransactionID = consensusTopicResponse.ChunkInfo.InitialTransactionID;

				// Can't use `Dictionary.putIfAbsent()` since that method is not available on Android
				if (!pendingMessages.ContainsKey(initialTransactionID))
				{
					pendingMessages.Add(initialTransactionID, new());
				}

				List<Proto.ConsensusTopicResponse> chunks = pendingMessages[initialTransactionID];

				// not possible as we do [putIfAbsent]
				// add our response to the pending chunk list
				chunks.Add(consensusTopicResponse);

				// if we now have enough chunks, emit
				if (chunks.Count == consensusTopicResponse.ChunkInfo.Total)
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
		}
    }
}