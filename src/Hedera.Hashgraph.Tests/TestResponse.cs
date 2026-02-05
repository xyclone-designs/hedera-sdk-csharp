// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Grpc;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TestResponse
    {
        public readonly TransactionResponse transactionResponse;
        public readonly Response queryResponse;
        public readonly StatusRuntimeException errorResponse;
        private TestResponse(TransactionResponse transactionResponse, Response queryResponse, StatusRuntimeException errorResponse)
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

        public static TestResponse Query(Response queryResponse)
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
            return Receipt(Status.SUCCESS);
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