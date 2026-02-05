// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Proto;
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

namespace Com.Hedera.Hashgraph.Sdk
{
    public class RegenerateTransactionIdsTest
    {
        virtual void RegeneratesTransactionIdsWhenTransactionExpiredIsReturned()
        {
            var transactionIds = new HashSet<TransactionId>();
            AtomicInteger count = new AtomicInteger(0);
            var responses = List.Of(TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.TRANSACTION_EXPIRED).Build(), TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.TRANSACTION_EXPIRED).Build(), TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.TRANSACTION_EXPIRED).Build(), TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.OK).Build());
            var call = (Function<object, object>)(o) =>
            {
                try
                {
                    var transaction = (Transaction)o;
                    var signedTransaction = SignedTransaction.ParseFrom(transaction.GetSignedTransactionBytes());
                    var transactionBody = TransactionBody.ParseFrom(signedTransaction.GetBodyBytes());
                    var transactionId = TransactionId.FromProtobuf(transactionBody.GetTransactionID());
                    if (transactionIds.Contains(transactionId))
                    {
                        return Status.Code.ABORTED.ToStatus().AsRuntimeException();
                    }

                    transactionIds.Add(transactionId);
                    return responses[count.GetAndIncrement()];
                }
                catch (Throwable e)
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