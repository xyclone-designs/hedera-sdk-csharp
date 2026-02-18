// SPDX-License-Identifier: Apache-2.0
using Proto;
using Io.Grpc;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.HBar;
using Grpc.Core;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class TestResponse
    {
        public readonly Proto.TransactionResponse transactionResponse;
        public readonly Proto.Response queryResponse;
        public readonly StatusRuntimeException errorResponse;
        private TestResponse(Proto.TransactionResponse transactionResponse, Proto.Response queryResponse, StatusRuntimeException errorResponse)
        {
            this.transactionResponse = transactionResponse;
            this.queryResponse = queryResponse;
            this.errorResponse = errorResponse;
        }

        public static TestResponse Transaction(Status status, Hbar cost)
        {
            return new TestResponse(BuildTransactionResponse(status, cost), null, null);
        }

        public static TestResponse Transaction(Status status)
        {
            return Transaction(status, new Hbar(1));
        }

        public static TestResponse TransactionOk(Hbar cost)
        {
            return Transaction(Status.OK, cost);
        }

        public static TestResponse TransactionOk()
        {
            return TransactionOk(new Hbar(1));
        }

        public static TestResponse Query(Proto.Response queryResponse)
        {
            return new TestResponse(null, queryResponse, null);
        }

        public static TestResponse Receipt(Status status)
        {
            var response = Response.NewBuilder().SetTransactionGetReceipt(TransactionGetReceiptResponse.NewBuilder().SetReceipt(TransactionReceipt.NewBuilder().SetStatus(status.code).Build()).Build()).Build();
            return new TestResponse(null, response, null);
        }

        public static TestResponse SuccessfulReceipt()
        {
            return Receipt(ResponseStatus.Success);
        }

        public static TestResponse Error(StatusRuntimeException exception)
        {
            return new TestResponse(null, null, exception);
        }

        public static TransactionResponse BuildTransactionResponse(Status status, Hbar cost)
        {
            return TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(status.code).SetCost(cost.ToTinybars()).Build();
        }
    }
}