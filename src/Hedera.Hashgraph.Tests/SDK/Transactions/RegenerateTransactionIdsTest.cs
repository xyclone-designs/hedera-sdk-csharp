// SPDX-License-Identifier: Apache-2.0
using Proto;
using Io.Grpc;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Concurrent.Atomic;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class RegenerateTransactionIdsTest
    {
        public virtual void RegeneratesTransactionIdsWhenTransactionExpiredIsReturned()
        {
			HashSet<TransactionId> transactionIds = [];
            AtomicInteger count = new AtomicInteger(0);
            List<Proto.TransactionResponse>  responses = 
            [
				new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.TransactionExpired },
				new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.TransactionExpired }, 
                new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.TransactionExpired }, 
                new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.Ok }
            ];
            var call = (Function<object, object>)(o) =>
            {
                try
                {
                    var transaction = (Transaction)o;
                    var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(transaction.GetSignedTransactionBytes());
                    var transactionBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.GetBodyBytes());
                    var transactionId = TransactionId.FromProtobuf(transactionBody.GetTransactionID());
                    if (transactionIds.Contains(transactionId))
                    {
                        return Status.Code.ABORTED.ToStatus().AsRuntimeException();
                    }

                    transactionIds.Add(transactionId);
                    return responses[count.GetAndIncrement()];
                }
                catch (Exception e)
                {
                    return new Exception(e);
                }
            };
            IList<object> responses1 = List.Of(call, call, call, call);
            using (var mocker = Mocker.WithResponses(List.Of(responses1)))
            {
                new FileCreateTransaction().Execute(mocker.client);
            }
        }
    }
}