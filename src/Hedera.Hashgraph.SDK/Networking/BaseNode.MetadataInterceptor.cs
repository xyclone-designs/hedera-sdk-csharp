// SPDX-License-Identifier: Apache-2.0
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Hedera.Hashgraph.SDK
{
    abstract partial class BaseNode<N, KeyT> 
    {
		/// <include file="BaseNode.MetadataInterceptor.cs.xml" path='docs/member[@name="T:MetadataInterceptor"]/*' />
		internal sealed class MetadataInterceptor : Interceptor
		{
			private readonly Metadata metadata;

			public MetadataInterceptor()
			{
				metadata = new Metadata
				{
					{
						"x-user-agent",
						GetUserAgent()
					}
				};
			}

			public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
				TRequest request,
				ClientInterceptorContext<TRequest, TResponse> context,
				AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
			{
				Metadata headers =  [..context.Options.Headers ?? [], .. metadata];
				var newOptions = context.Options.WithHeaders(headers);
				var newContext =
					new ClientInterceptorContext<TRequest, TResponse>(
						context.Method,
						context.Host,
						newOptions);

				return continuation(request, newContext);
			}

			private static string GetUserAgent()
			{
				var version =
					typeof(BaseNode<,>).Assembly
						.GetName()
						.Version?
						.ToString();

				return "hiero-sdk-csharp/" +
					   (version ?? "DEV");
			}
		}
	}
}