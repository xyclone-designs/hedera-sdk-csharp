// SPDX-License-Identifier: Apache-2.0
using Grpc.Core;
using Io.Grpc;
using Io.Grpc.Inprocess;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    abstract partial class BaseNode<N, KeyT> 
    {
        /// <summary>
        /// Metadata interceptor for the client.
        /// This interceptor adds the user agent header to the request.
        /// </summary>
        private class MetadataInterceptor : ClientInterceptor
        {
            private readonly Metadata metadata;
            public MetadataInterceptor()
            {
                metadata = new Metadata();
                Metadata.Key<String> authKey = Metadata.Key.Of("x-user-agent", Metadata.ASCII_STRING_MARSHALLER);
                metadata.Put(authKey, GetUserAgent());
            }

            public virtual ClientCall<ReqT, RespT> InterceptCall<ReqT, RespT>(MethodDescriptor<ReqT, RespT> method, CallOptions callOptions, Channel next)
            {
                ClientCall<ReqT, RespT> call = next.NewCall(method, callOptions);
                return new AnonymousSimpleForwardingClientCall(call);
            }

            private sealed class AnonymousSimpleForwardingClientCall : SimpleForwardingClientCall
            {
                public AnonymousSimpleForwardingClientCall(MetadataInterceptor parent)
                {
                    parent = parent;
                }

                private readonly MetadataInterceptor parent;
                public void Start(Listener<RespT> responseListener, Metadata headers)
                {
                    headers.Merge(metadata);
                    base.Start(responseListener, headers);
                }
            }

            /// <summary>
            /// Extract the user agent. This information is used to gather usage metrics.
            /// If the version is not available, the user agent will be set to "hiero-sdk-csharp/DEV".
            /// </summary>
            private string GetUserAgent()
            {
                var thePackage = this.GetType().GetPackage();
                var implementationVersion = thePackage != null ? thePackage.GetImplementationVersion() : null;
                return "hiero-sdk-csharp/" + ((implementationVersion != null) ? (implementationVersion) : "DEV");
            }
        }
    }
}