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
	/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="T:AddressBookQuery"]/*' />
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

		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="P:AddressBookQuery.FileId"]/*' />
		public virtual FileId? FileId { get; set; }
		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="T:AddressBookQuery_2"]/*' />
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

		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="P:AddressBookQuery.Limit"]/*' />
		public virtual int? Limit { get; set; }
		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="P:AddressBookQuery.MaxAttempts"]/*' />
		public virtual int MaxAttempts { get; set; } = 10;

		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="M:AddressBookQuery.Execute(Client)"]/*' />
		public virtual NodeAddressBook Execute(Client client)
		{
			return Execute(client, client.RequestTimeout);
		}
		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="M:AddressBookQuery.Execute(Client,System.TimeSpan)"]/*' />
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
		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="M:AddressBookQuery.ExecuteAsync(Client)"]/*' />
		public virtual Task<NodeAddressBook> ExecuteAsync(Client client)
		{
			return ExecuteAsync(client, client.RequestTimeout);
		}
		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="M:AddressBookQuery.ExecuteAsync(Client,System.TimeSpan)"]/*' />
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

		/// <include file="AddressBookQuery.cs.xml" path='docs/member[@name="M:AddressBookQuery.BuildProtoQuery"]/*' />
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