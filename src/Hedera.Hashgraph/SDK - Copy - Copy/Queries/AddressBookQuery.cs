// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Networking;
using Io.Grpc;
using Io.Grpc.Stub;

using Org.Slf4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Query the mirror node for the address book.
    /// </summary>
    public class AddressBookQuery
    {
        private static readonly Logger LOGGER = LoggerFactory.GetLogger(typeof(AddressBookQuery));
		private static bool ShouldRetry(Exception exception)
		{
			if (exception is RpcException rpcexception)
			{
				var code = rpcexception.Status;
				var description = rpcexception.Status.Description;
				return (code == Status.UNAVAILABLE) || (code == io.grpc.Status.Code.RESOURCE_EXHAUSTED) || (code == ResponseStatus.Code.INTERNAL && description != null && Executable.RST_STREAM.Matcher(description).Matches());
			}

			return false;
		}


		/// <summary>
		/// Assign the file id of address book to retrieve.
		/// </summary>
		public virtual FileId? FileId { get; set; }
		/// <summary>
		/// Assign the maximum backoff duration.
		/// </summary>
		public virtual Duration MaxBackoff
        {
            get;
            set
            {
				if (value.ToMillis() < 500)
				{
					throw new ArgumentException("maxBackoff must be at least 500 ms");
				}

				field = value;
			}
        } = Duration.FromTimeSpan(TimeSpan.FromSeconds(8));
		/// <summary>
		/// Assign the number of node addresses to retrieve or all nodes set to 0.
		/// </summary>
		/// <param name="limit">number of node addresses to get</param>
		/// <returns>{@code this}</returns>
		public virtual int? Limit { get; set; }
        /// <summary>
        /// Assign the maximum number of attempts.
        /// </summary>
        public virtual int MaxAttempts { get; set; } = 10;

        /// <summary>
        /// Execute the query with preset timeout.
        /// </summary>
        /// <param name="client">the client object</param>
        /// <returns>the node address book</returns>
        public virtual NodeAddressBook Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
        }
        /// <summary>
        /// Execute the query with user supplied timeout.
        /// </summary>
        /// <param name="client">the client object</param>
        /// <param name="timeout">the user supplied timeout</param>
        /// <returns>the node address book</returns>
        public virtual NodeAddressBook Execute(Client client, Duration timeout)
        {
            var deadline = Deadline.After(timeout.ToMillis(), TimeUnit.MILLISECONDS);

            for (int attempt = 1; true; attempt++)
            {
                try
                {
                    var addressProtoIter = ClientCalls.BlockingServerStreamingCall(BuildCall(client, deadline), BuildQuery());

                    IList<NodeAddress> addresses = [];
                    while (addressProtoIter.HasNext())
                    {
                        addresses.Add(NodeAddress.FromProtobuf(addressProtoIter.Next()));
                    }

                    return new NodeAddressBook
                    {
						NodeAddresses = addresses
					};
                }
                catch (Exception error)
                {
                    if (!ShouldRetry(error) || attempt >= MaxAttempts)
                    {
                        LOGGER.Error("Error attempting to get address book at FileId {}", FileId, error);
                        throw;
                    }

                    WarnAndDelay(attempt, error);
                }
            }
        }
        /// <summary>
        /// Execute the query with preset timeout asynchronously.
        /// </summary>
        /// <param name="client">the client object</param>
        /// <returns>the node address book</returns>
        public virtual Task<NodeAddressBook> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.RequestTimeout);
        }
        /// <summary>
        /// Execute the query with user supplied timeout.
        /// </summary>
        /// <param name="client">the client object</param>
        /// <param name="timeout">the user supplied timeout</param>
        /// <returns>the node address book</returns>
        public virtual Task<NodeAddressBook> ExecuteAsync(Client client, Duration timeout)
        {
            var deadline = Deadline.After(timeout.ToMillis(), TimeUnit.MILLISECONDS);

            Task<NodeAddressBook> returnFuture = Task.FromResult<NodeAddressBook>(default);
            
            ExecuteAsync(client, deadline, returnFuture, 1);

            return returnFuture;
        }
        /// <summary>
        /// Execute the query.
        /// </summary>
        /// <param name="client">the client object</param>
        /// <param name="deadline">the user supplied timeout</param>
        /// <param name="returnFuture">returned promise callback</param>
        /// <param name="attempt">maximum number of attempts</param>
        public virtual void ExecuteAsync(Client client, Deadline deadline, Task<NodeAddressBook> returnFuture, int attempt)
        {
            IList<NodeAddress> addresses = [];
            ClientCalls.AsyncServerStreamingCall(BuildCall(client, deadline), BuildQuery(), new AnonymousStreamObserver(this));
        }

        private sealed class AnonymousStreamObserver : StreamObserver
        {
            public AnonymousStreamObserver(AddressBookQuery parent)
            {
                parent = parent;
            }

            private readonly AddressBookQuery parent;

			public void OnCompleted()
			{
				returnFuture.Complete(new NodeAddressBook { NodeAddresses = Addresses });
			}
			public void OnError(Exception error)
			{
				if (Attempt >= MaxAttempts || !ShouldRetry(error))
				{
					LOGGER.Error("Error attempting to get address book at FileId {}", FileId, error);
					returnFuture.CompleteExceptionally(error);
					return;
				}

				WarnAndDelay(Attempt, error);
				Addresses.Clear();
				ExecuteAsync(Client, Deadline, ReturnFuture, Attempt + 1);
			}
			public void OnNext(Proto.NodeAddress addressProto)
            {
                Addresses.Add(NodeAddress.FromProtobuf(addressProto));
            }
        }

        /// <summary>
        /// Build the address book query.
        /// </summary>
        /// <returns>{@link Proto.mirror.AddressBookQuery buildQuery }</returns>
        public virtual AddressBookQuery BuildQuery()
        {
            var builder = new AddressBookQuery();

            if (FileId != null)
                builder.FileId = FileId.ToProtobuf();

            if (Limit != null)
                builder.Limit = Limit;

            return builder;
        }

        private ClientCall<Proto.AddressBookQuery, Proto.NodeAddress> BuildCall(Client client, Deadline deadline)
        {
            try
            {
                return client.mirrorNetwork.GetNextMirrorNode().GetChannel().NewCall(NetworkServiceGrpc.GetGetNodesMethod(), CallOptions.DEFAULT.WithDeadline(deadline));
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception(string.Empty, e);
            }
        }

        private void WarnAndDelay(int attempt, Exception error)
        {
            var delay = Math.Min(500 * (long)Math.Pow(2, attempt), maxBackoff.ToMillis());
            LOGGER.Warn("Error fetching address book at FileId {} during attempt #{}. Waiting {} ms before next attempt: {}", fileId, attempt, delay, error.Message);
            try
            {
                Thread.Sleep(delay);
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
            }
        }
    }
}