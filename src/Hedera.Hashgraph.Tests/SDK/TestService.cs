// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.Tests.SDK
{
    public interface ITestService
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
        void RespondToTransaction(Proto.Transaction request, StreamObserver<Proto.TransactionResponse> streamObserver, TestResponse response)
        {
            GetBuffer().transactionRequestsReceived.Add(request);
            var exceptionString = "TestService tried to respond to transaction with query response";
            Respond(streamObserver, response.transactionResponse, response.errorResponse, exceptionString);
        }

        void RespondToQuery(Proto.Query request, StreamObserver<Proto.Response> streamObserver, TestResponse response)
        {
            GetBuffer().queryRequestsReceived.Add(request);
            var exceptionString = "TestService tried to respond to query with transaction response";
            Respond(streamObserver, response.queryResponse, response.errorResponse, exceptionString);
        }

        void RespondToTransactionFromQueue(Proto.Transaction request, IStreamObserver<Proto.TransactionResponse> streamObserver)
        {
            RespondToTransaction(request, streamObserver, GetBuffer().responsesToSend.Dequeue());
        }

        void RespondToQueryFromQueue(Proto.Query request, StreamObserver<Proto.Response> streamObserver)
        {
            RespondToQuery(request, streamObserver, GetBuffer().responsesToSend.Dequeue());
        }

        class Buffer
        {
            public readonly List<ITransaction> transactionRequestsReceived = [];
            public readonly List<Proto.Query> queryRequestsReceived = [];
            public readonly Queue<TestResponse> responsesToSend = new ();
            public virtual Buffer EnqueueResponse(TestResponse response)
            {
                responsesToSend.Enqueue(response);
                return this;
            }
        }
    }
}