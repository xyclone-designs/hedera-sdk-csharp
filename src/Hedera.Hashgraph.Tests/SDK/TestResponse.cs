// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.HBar;

using Grpc.Core;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class TestResponse
    {
        public readonly Proto.TransactionResponse transactionResponse;
        public readonly Proto.Response queryResponse;
        public readonly RpcException errorResponse;
        private TestResponse(Proto.TransactionResponse transactionResponse, Proto.Response queryResponse, RpcException errorResponse)
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
            return Transaction(Status.DefaultSuccess, cost);
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
            var response = new Proto.Response
            {
                TransactionGetReceipt = new Proto.TransactionGetReceiptResponse
                {
                    Receipt = new Proto.TransactionReceipt { Status = (Proto.ResponseCodeEnum)status.StatusCode }
                }
            };
            return new TestResponse(null, response, null);
        }

        public static TestResponse SuccessfulReceipt()
        {
            return Receipt(Status.DefaultSuccess);
        }

        public static TestResponse Error(RpcException exception)
        {
            return new TestResponse(null, null, exception);
        }

        public static Proto.TransactionResponse BuildTransactionResponse(Status status, Hbar cost)
        {
            return new Proto.TransactionResponse
            {
                NodeTransactionPrecheckCode = (Proto.ResponseCodeEnum)status.StatusCode,
                Cost = (ulong)cost.ToTinybars()
            };
        }
    }
}