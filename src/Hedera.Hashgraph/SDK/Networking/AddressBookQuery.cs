// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;

using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Logging;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <summary>
	/// Query the mirror node for the address book.
	/// </summary>
	public class AddressBookQuery
	{
		private static readonly Logger LOGGER = LoggerFactory.GetLogger(typeof(AddressBookQuery));

		private static bool ShouldRetry(Exception exception)
		{
			if (exception is RpcException rpcException)
			{
				var code = rpcException.StatusCode;
				var description = rpcException.Status.Detail;

				return code == StatusCode.Unavailable
					|| code == StatusCode.ResourceExhausted
					|| (code == StatusCode.Internal
						&& description != null
						&& Executable.RST_STREAM.IsMatch(description));
			}

			return false;
		}

		/// <summary>
		/// Assign the file id of address book to retrieve.
		/// </summary>
		public virtual FileId? FileId { get; set; }
		/// <summary>
		/// Assign the maximum backoff duration. Must be at least 500 ms.
		/// </summary>
		public virtual TimeSpan MaxBackoff
		{
			get;
			set
			{
				if (value.TotalMilliseconds < 500)
				{
					throw new ArgumentException("maxBackoff must be at least 500 ms");
				}

				field = value;
			}

		} = TimeSpan.FromSeconds(8);

		/// <summary>
		/// Assign the number of node addresses to retrieve, or 0 for all nodes.
		/// </summary>
		public virtual int? Limit { get; set; }
		/// <summary>
		/// Assign the maximum number of attempts.
		/// </summary>
		public virtual int MaxAttempts { get; set; } = 10;

		/// <summary>
		/// Execute the query with the client's preset timeout.
		/// </summary>
		public virtual NodeAddressBook Execute(Client client)
		{
			return Execute(client, client.RequestTimeout);
		}
		/// <summary>
		/// Execute the query with a user-supplied timeout.
		/// </summary>
		public virtual NodeAddressBook Execute(Client client, TimeSpan timeout)
		{
			var deadline = DateTime.UtcNow.Add(timeout);

			for (int attempt = 1; true; attempt++)
			{
				try
				{
					var call = BuildCall(client, deadline);
					var addresses = new List<NodeAddress>();

					// Blocking read of the streaming response.
					var streamingCall = call.ResponseStream;
					while (streamingCall.MoveNext(CancellationToken.None).GetAwaiter().GetResult())
					{
						addresses.Add(NodeAddress.FromProtobuf(streamingCall.Current));
					}

					return new NodeAddressBook { NodeAddresses = addresses };
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
		/// Execute the query asynchronously with the client's preset timeout.
		/// </summary>
		public virtual Task<NodeAddressBook> ExecuteAsync(Client client)
		{
			return ExecuteAsync(client, client.RequestTimeout);
		}
		/// <summary>
		/// Execute the query asynchronously with a user-supplied timeout.
		/// </summary>
		public virtual Task<NodeAddressBook> ExecuteAsync(Client client, TimeSpan timeout)
		{
			var deadline = DateTime.UtcNow.Add(timeout);

			var tcs = new TaskCompletionSource<NodeAddressBook>(
				TaskCreationOptions.RunContinuationsAsynchronously);

			_ = ExecuteAsync(client, deadline, tcs, attempt: 1);

			return tcs.Task;
		}
		private async Task ExecuteAsync(Client client, DateTime deadline, TaskCompletionSource<NodeAddressBook> tcs, int attempt)
		{
			var addresses = new List<NodeAddress>();

			try
			{
				var call = BuildCall(client, deadline);
				var streamingCall = call.ResponseStream;

				while (await streamingCall.MoveNext(CancellationToken.None).ConfigureAwait(false))
				{
					addresses.Add(NodeAddress.FromProtobuf(streamingCall.Current));
				}

				tcs.TrySetResult(new NodeAddressBook { NodeAddresses = addresses });
			}
			catch (Exception error)
			{
				if (attempt >= MaxAttempts || !ShouldRetry(error))
				{
					LOGGER.Error("Error attempting to get address book at FileId {}", FileId, error);
					tcs.TrySetException(error);
					return;
				}

				WarnAndDelayAsync(attempt, error);
				addresses.Clear();

				await ExecuteAsync(client, deadline, tcs, attempt + 1).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Build the protobuf address book query message.
		/// </summary>
		public virtual Proto.AddressBookQuery BuildProtoQuery()
		{
			var builder = new Proto.AddressBookQuery();

			if (FileId != null)
				builder.FileId = FileId.ToProtobuf();

			if (Limit != null)
				builder.Limit = Limit.Value;

			return builder;
		}

		private AsyncServerStreamingCall<Proto.NodeAddress> BuildCall(Client client, DateTime deadline)
		{
			string methodname = nameof(Proto.NetworkService.NetworkServiceClient.getNodes);
			MethodDescriptor methoddescriptor = Proto.NetworkService.Descriptor.FindMethodByName(methodname);

			IMessage input = (IMessage)Activator.CreateInstance(methoddescriptor.InputType.ClrType)!;
			IMessage output = (IMessage)Activator.CreateInstance(methoddescriptor.OutputType.ClrType)!;

			Method<Proto.AddressBookQuery, Proto.NodeAddress> method = new (
				type: MethodType.Unary,
				name: methoddescriptor.Name,
				serviceName: methoddescriptor.Service.FullName,
				requestMarshaller: Marshallers.Create(r => r.ToByteArray(), data => Proto.AddressBookQuery.Parser.ParseFrom(data)),
				responseMarshaller: Marshallers.Create(r => r.ToByteArray(), data => Proto.NodeAddress.Parser.ParseFrom(data)));

			try
			{
				var channel = client.MirrorNetwork_.GetNextMirrorNode().Channel;
				var callOptions = new CallOptions(deadline: deadline);
				var invoker = channel.CreateCallInvoker();

				return invoker.AsyncServerStreamingCall(
					method: method,
					host: null,
					callOptions,
					BuildProtoQuery());
			}
			catch (OperationCanceledException e)
			{
				throw new Exception("Call was cancelled.", e);
			}
		}

		private void WarnAndDelay(int attempt, Exception error)
		{
			var delay = (int)Math.Min(500L * (long)Math.Pow(2, attempt), (long)MaxBackoff.TotalMilliseconds);
			LOGGER.Warn(
				"Error fetching address book at FileId {0} during attempt #{1}. Waiting {2} ms before next attempt: {3}",
				FileId, attempt, delay, error.Message);

			try
			{
				Thread.Sleep(delay);
			}
			catch (ThreadInterruptedException)
			{
				// Re-interrupt the current thread to preserve interrupt status.
				Thread.CurrentThread.Interrupt();
			}
		}

		private async void WarnAndDelayAsync(int attempt, Exception error)
		{
			var delay = (int)Math.Min(500L * (long)Math.Pow(2, attempt), (long)MaxBackoff.TotalMilliseconds);
			LOGGER.Warn(
				"Error fetching address book at FileId {0} during attempt #{1}. Waiting {2} ms before next attempt: {3}",
				FileId, attempt, delay, error.Message);

			await Task.Delay(delay);
		}
	}
}