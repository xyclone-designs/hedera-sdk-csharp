// SPDX-License-Identifier: Apache-2.0
using Proto;
using Io.Grpc;
using Io.Grpc.Stub;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    public interface TestService
    {
        private static void Respond<ResponseTypeT>(StreamObserver<ResponseTypeT> streamObserver, ResponseTypeT normalResponse, StatusRuntimeException errorResponse, string exceptionString)
        {
            if (normalResponse != null)
            {
                streamObserver.OnNext(normalResponse);
                streamObserver.OnCompleted();
            }
            else if (errorResponse != null)
            {
                streamObserver.OnError(errorResponse);
            }
            else
            {
                throw new InvalidOperationException(exceptionString);
            }
        }

        Buffer GetBuffer();
        void RespondToTransaction(Transaction request, StreamObserver<TransactionResponse> streamObserver, TestResponse response)
        {
            GetBuffer().transactionRequestsReceived.Add(request);
            var exceptionString = "TestService tried to respond to transaction with query response";
            Respond(streamObserver, response.transactionResponse, response.errorResponse, exceptionString);
        }

        void RespondToQuery(Query request, StreamObserver<Response> streamObserver, TestResponse response)
        {
            GetBuffer().queryRequestsReceived.Add(request);
            var exceptionString = "TestService tried to respond to query with transaction response";
            Respond(streamObserver, response.queryResponse, response.errorResponse, exceptionString);
        }

        void RespondToTransactionFromQueue(Transaction request, StreamObserver<TransactionResponse> streamObserver)
        {
            RespondToTransaction(request, streamObserver, GetBuffer().responsesToSend.Remove());
        }

        void RespondToQueryFromQueue(Query request, StreamObserver<Response> streamObserver)
        {
            RespondToQuery(request, streamObserver, GetBuffer().responsesToSend.Remove());
        }

        class Buffer
        {
            public readonly List<Transaction<T>> transactionRequestsReceived = new List();
            public readonly List<Query> queryRequestsReceived = new List();
            public readonly Queue<TestResponse> responsesToSend = new ArrayDeque();
            public virtual Buffer EnqueueResponse(TestResponse response)
            {
                responsesToSend.Add(response);
                return this;
            }
        }
    }
}