// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Grpc.Core;

using Hedera.Hashgraph.SDK.Cryptocurrency;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
    public interface IExecutable 
    {
        TimeSpan GrpcDeadline { get; set; }
        TimeSpan MaxBackoff { get; set; }
        TimeSpan MinBackoff { get; set; }
        int MaxAttempts { get; set; }
        int MaxRetry { get; set; }
        ListGuarded<AccountId> NodeAccountIds { get; set; }

        void OnExecute(Client client);
        Task OnExecuteAsync(Client client);
    }
    public interface IExecutable<TProtoRequest, TProtoResponse> : IExecutable where TProtoRequest : class, IMessage where TProtoResponse : class, IMessage 
	{
        Func<TProtoRequest, TProtoRequest> RequestListener { get; set; }
        Func<TProtoResponse, TProtoResponse> ResponseListener { get; set; }

        Method<TProtoRequest, TProtoResponse> GetMethod();
        MethodDescriptor GetMethodDescriptor();
    }
}
