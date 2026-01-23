// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Proto.Mirror;
using Io.Grpc;
using Io.Grpc.Stub;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Javax.Annotation;
using Org.Slf4j;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Query the mirror node for the address book.
    /// </summary>
    public class AddressBookQuery
    {
        private static readonly Logger LOGGER = LoggerFactory.GetLogger(typeof(AddressBookQuery));
        private FileId fileId = null;
        private int limit = null;
        private int maxAttempts = 10;
        private Duration maxBackoff = Duration.OfSeconds(8);
        /// <summary>
        /// Constructor.
        /// </summary>
        public AddressBookQuery()
        {
        }

        private static bool ShouldRetry(Throwable throwable)
        {
            if (throwable is StatusRuntimeException)
            {
                var code = statusRuntimeException.GetStatus().GetCode();
                var description = statusRuntimeException.GetStatus().GetDescription();
                return (code == io.grpc.Status.Code.UNAVAILABLE) || (code == io.grpc.Status.Code.RESOURCE_EXHAUSTED) || (code == Status.Code.INTERNAL && description != null && Executable.RST_STREAM.Matcher(description).Matches());
            }

            return false;
        }

        /// <summary>
        /// Extract the file id.
        /// </summary>
        /// <returns>the file id that was assigned</returns>
        public virtual FileId GetFileId()
        {
            return fileId;
        }

        /// <summary>
        /// Assign the file id of address book to retrieve.
        /// </summary>
        /// <param name="fileId">the file id of the address book</param>
        /// <returns>{@code this}</returns>
        public virtual AddressBookQuery SetFileId(FileId fileId)
        {
            fileId = fileId;
            return this;
        }

        /// <summary>
        /// Extract the limit number.
        /// </summary>
        /// <returns>the limit number that was assigned</returns>
        public virtual int GetLimit()
        {
            return limit;
        }

        /// <summary>
        /// Assign the number of node addresses to retrieve or all nodes set to 0.
        /// </summary>
        /// <param name="limit">number of node addresses to get</param>
        /// <returns>{@code this}</returns>
        public virtual AddressBookQuery SetLimit(int limit)
        {
            limit = limit;
            return this;
        }

        /// <summary>
        /// Extract the maximum number of attempts.
        /// </summary>
        /// <returns>the maximum number of attempts</returns>
        public virtual int GetMaxAttempts()
        {
            return maxAttempts;
        }

        /// <summary>
        /// Assign the maximum number of attempts.
        /// </summary>
        /// <param name="maxAttempts">the maximum number of attempts</param>
        /// <returns>{@code this}</returns>
        public virtual AddressBookQuery SetMaxAttempts(int maxAttempts)
        {
            maxAttempts = maxAttempts;
            return this;
        }

        /// <summary>
        /// Assign the maximum backoff duration.
        /// </summary>
        /// <param name="maxBackoff">the maximum backoff duration</param>
        /// <returns>{@code this}</returns>
        public virtual AddressBookQuery SetMaxBackoff(Duration maxBackoff)
        {
            Objects.RequireNonNull(maxBackoff);
            if (maxBackoff.ToMillis() < 500)
            {
                throw new ArgumentException("maxBackoff must be at least 500 ms");
            }

            maxBackoff = maxBackoff;
            return this;
        }

        /// <summary>
        /// Execute the query with preset timeout.
        /// </summary>
        /// <param name="client">the client object</param>
        /// <returns>the node address book</returns>
        public virtual NodeAddressBook Execute(Client client)
        {
            return Execute(client, client.GetRequestTimeout());
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
                    IList<NodeAddress> addresses = new ();
                    while (addressProtoIter.HasNext())
                    {
                        addresses.Add(NodeAddress.FromProtobuf(addressProtoIter.Next()));
                    }

                    return new NodeAddressBook().SetNodeAddresses(addresses);
                }
                catch (Throwable error)
                {
                    if (!ShouldRetry(error) || attempt >= maxAttempts)
                    {
                        LOGGER.Error("Error attempting to get address book at FileId {}", fileId, error);
                        throw error;
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
        public virtual CompletableFuture<NodeAddressBook> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Execute the query with user supplied timeout.
        /// </summary>
        /// <param name="client">the client object</param>
        /// <param name="timeout">the user supplied timeout</param>
        /// <returns>the node address book</returns>
        public virtual CompletableFuture<NodeAddressBook> ExecuteAsync(Client client, Duration timeout)
        {
            var deadline = Deadline.After(timeout.ToMillis(), TimeUnit.MILLISECONDS);
            CompletableFuture<NodeAddressBook> returnFuture = new CompletableFuture();
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
        virtual void ExecuteAsync(Client client, Deadline deadline, CompletableFuture<NodeAddressBook> returnFuture, int attempt)
        {
            IList<NodeAddress> addresses = new ();
            ClientCalls.AsyncServerStreamingCall(BuildCall(client, deadline), BuildQuery(), new AnonymousStreamObserver(this));
        }

        private sealed class AnonymousStreamObserver : StreamObserver
        {
            public AnonymousStreamObserver(AddressBookQuery parent)
            {
                parent = parent;
            }

            private readonly AddressBookQuery parent;
            public void OnNext(Proto.NodeAddress addressProto)
            {
                addresses.Add(NodeAddress.FromProtobuf(addressProto));
            }

            public void OnError(Throwable error)
            {
                if (attempt >= maxAttempts || !ShouldRetry(error))
                {
                    LOGGER.Error("Error attempting to get address book at FileId {}", fileId, error);
                    returnFuture.CompleteExceptionally(error);
                    return;
                }

                WarnAndDelay(attempt, error);
                addresses.Clear();
                ExecuteAsync(client, deadline, returnFuture, attempt + 1);
            }

            public void OnCompleted()
            {
                returnFuture.Complete(new NodeAddressBook().SetNodeAddresses(addresses));
            }
        }

        /// <summary>
        /// Build the address book query.
        /// </summary>
        /// <returns>{@link Proto.mirror.AddressBookQuery buildQuery }</returns>
        virtual Proto.mirror.AddressBookQuery BuildQuery()
        {
            var builder = Proto.mirror.AddressBookQuery.NewBuilder();
            if (fileId != null)
            {
                builder.SetFileId(fileId.ToProtobuf());
            }

            if (limit != null)
            {
                builder.SetLimit(limit);
            }

            return proto;
        }

        private ClientCall<Proto.mirror.AddressBookQuery, Proto.NodeAddress> BuildCall(Client client, Deadline deadline)
        {
            try
            {
                return client.mirrorNetwork.GetNextMirrorNode().GetChannel().NewCall(NetworkServiceGrpc.GetGetNodesMethod(), CallOptions.DEFAULT.WithDeadline(deadline));
            }
            catch (InterruptedException e)
            {
                throw new Exception(e);
            }
        }

        private void WarnAndDelay(int attempt, Throwable error)
        {
            var delay = Math.Min(500 * (long)Math.Pow(2, attempt), maxBackoff.ToMillis());
            LOGGER.Warn("Error fetching address book at FileId {} during attempt #{}. Waiting {} ms before next attempt: {}", fileId, attempt, delay, error.GetMessage());
            try
            {
                Thread.Sleep(delay);
            }
            catch (InterruptedException e)
            {
                Thread.CurrentThread().Interrupt();
            }
        }
    }
}