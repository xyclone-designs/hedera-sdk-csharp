// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <include file="TopicMessageQuery.cs.xml" path='docs/member[@name="T:TopicMessageQuery"]/*' />
    public sealed class TopicMessageQuery
    {
		private readonly Proto.ConsensusTopicQuery _Proto = new();
		private static readonly Logger LOGGER = LoggerFactory.GetLogger(typeof(TopicMessageQuery));
        private long Counter = 0;
        private bool CancelledByClient = false;
        
        public TopicId TopicId { set => _Proto.TopicID = value.ToProtobuf(); }
		public DateTimeOffset StartTime { set => _Proto.ConsensusStartTime = value.ToProtoTimestamp(); }
		public DateTimeOffset EndTime { set => _Proto.ConsensusEndTime  = value.ToProtoTimestamp(); }
		public ulong Limit { set => _Proto.Limit = value; }
        public Action CompletionHandler 
        { 
            set;
            private get => field ??= () =>
            {
				LOGGER.Info("Subscription to topic {} complete", TopicId.FromProtobuf(_Proto.TopicID));
			};
        } 
        public Action<Exception, TopicMessage?> ErrorHandler
        {
            set;
            private get => field ??= ((exception, topicmessage) =>
            {
                var topicId = TopicId.FromProtobuf(_Proto.TopicID);

                if (exception is RpcException rpcexception && rpcexception.Status.Equals(Status.DefaultCancelled))
                    LOGGER.Warn("Call is cancelled for topic {}.", topicId);
                else LOGGER.Error("Error attempting to subscribe to topic {}:", topicId, exception);
            });
		}
		public IntNN MaxAttempts { set; private get; } = 10;
        public TimeSpan MaxBackoff
		{
			private get;

			set
            {
				if (value.TotalMilliseconds < 500)
					throw new ArgumentException("maxBackoff must be at least 500 ms");

				field = value;
			}
        } = TimeSpan.FromSeconds(8);
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

        /// <include file="TopicMessageQuery.cs.xml" path='docs/member[@name="T:TopicMessageQuery.when"]/*' />
        // TODO: Refactor into a base class when we add more mirror query types
        public SubscriptionHandle Subscribe(Client client, Action<TopicMessage> onNext)
        {
            SubscriptionHandle subscriptionHandle = new ();
            Dictionary<Proto.TransactionID, List<Proto.ConsensusTopicResponse>> pendingMessages = [];
            try
            {
                MakeStreamingCall(client, subscriptionHandle, onNext, 0, null, new AtomicClass<Proto.ConsensusTopicResponse>(default), pendingMessages);
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception(string.Empty, e);
            }

            return subscriptionHandle;
        }

        private void MakeStreamingCall(
            Client client,
            SubscriptionHandle subscriptionHandle,
            Action<TopicMessage> onNext,
            int attempt,
            long? counter,
            AtomicClass<Proto.ConsensusTopicResponse> lastMessage,
            Dictionary<Proto.TransactionID, List<Proto.ConsensusTopicResponse>> pendingMessages)
        {
            Counter = counter ?? 0;

			var callInvoker = client.MirrorNetwork_
                .GetNextMirrorNode()
                .Channel
                .CreateCallInvoker();

			string methodname = nameof(Proto.ConsensusService.ConsensusServiceClient.subscribeTopic);

            MethodDescriptor methoddescriptor = Proto.CryptoService.Descriptor.FindMethodByName(methodname); 

			IMessage input = (IMessage)Activator.CreateInstance(methoddescriptor.InputType.ClrType)!;
			IMessage output = (IMessage)Activator.CreateInstance(methoddescriptor.OutputType.ClrType)!;

			var method = new Method<Proto.ConsensusTopicQuery, Proto.ConsensusTopicResponse>(
				type: MethodType.Unary,
				name: methoddescriptor.Name,
				serviceName: methoddescriptor.Service.FullName,
				requestMarshaller: Marshallers.Create(r => r.ToByteArray(), data => Proto.ConsensusTopicQuery.Parser.ParseFrom(data)),
				responseMarshaller: Marshallers.Create(r => r.ToByteArray(), data => Proto.ConsensusTopicResponse.Parser.ParseFrom(data)));

            CancellationTokenSource cts = new();

            subscriptionHandle.SetOnUnsubscribe(() =>
            {
				Volatile.Write(ref CancelledByClient, true);
                client.UntrackSubscription(subscriptionHandle);
                cts.Cancel();
            });

            client.TrackSubscription(subscriptionHandle);

            var newBuilder = _Proto;

            // Update start time & limit on retry
            if (lastMessage.Get() != null)
            {
                newBuilder = _Proto.Clone();

                if (_Proto.Limit > 0)
                {
                    newBuilder.Limit = _Proto.Limit - (ulong)Interlocked.Read(ref Counter);
                }

                var lastStartTime = lastMessage.Get().ConsensusTimestamp;

                newBuilder.ConsensusStartTime = new Proto.Timestamp
                {
                    Seconds = lastStartTime.Seconds,
                    Nanos = lastStartTime.Nanos + 1
                };
            }

            var call = callInvoker.AsyncServerStreamingCall(method, null, new CallOptions(cancellationToken: cts.Token), newBuilder);

            _ = Task.Run(async () =>
            {
                try
                {
                    while (await call.ResponseStream.MoveNext(cts.Token))
                    {
                        Interlocked.Increment(ref Counter);
                        lastMessage.Set(call.ResponseStream.Current);

                        // No chunk or single chunk
                        if (call.ResponseStream.Current.ChunkInfo.Total == 1)
                        {
                            var message = TopicMessage.OfSingle(call.ResponseStream.Current);

                            try
                            {
                                onNext(message);
                            }
                            catch (Exception ex)
                            {
                                ErrorHandler?.Invoke(ex, message);
                            }

                            continue;
                        }

                        var initialTransactionId = call.ResponseStream.Current.ChunkInfo.InitialTransactionID;

                        if (!pendingMessages.ContainsKey(initialTransactionId))
                        {
                            pendingMessages[initialTransactionId] = new List<Proto.ConsensusTopicResponse>();
                        }

                        var chunks = pendingMessages[initialTransactionId];
                        chunks.Add(call.ResponseStream.Current);

                        if (chunks.Count == call.ResponseStream.Current.ChunkInfo.Total)
                        {
                            var message = TopicMessage.OfMany(chunks);

                            try
                            {
                                onNext(message);
                            }
                            catch (Exception ex)
                            {
                                ErrorHandler?.Invoke(ex, message);
                            }
                        }
                    }

                    CompletionHandler?.Invoke();
                }
                catch (Exception ex)
                {
                    if (Volatile.Read(ref CancelledByClient))
                    {
                        Volatile.Write(ref CancelledByClient, false);
                        return;
					}

                    if (attempt >= MaxAttempts || !RetryHandler(ex))
                    {
                        ErrorHandler?.Invoke(ex, null);
                        return;
                    }

                    var delay = Math.Min(500L * (long)Math.Pow(2, attempt), (long)MaxBackoff.TotalMilliseconds);

                    var topicId = TopicId.FromProtobuf(_Proto.TopicID);

                    LOGGER.Warn(
                        $"Error subscribing to topic {topicId} during attempt #{attempt}. " +
                        $"Waiting {delay} ms before next attempt: {ex.Message}");

                    cts.Cancel();

                    try
                    {
                        await Task.Delay((int)delay);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }

                    MakeStreamingCall(client, subscriptionHandle, onNext, attempt + 1, counter, lastMessage, pendingMessages);
                }
            });
        }
    }
}