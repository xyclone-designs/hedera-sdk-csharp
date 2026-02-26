// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.File;
using System.Runtime.CompilerServices;

using Grpc.Core;
using System.Threading;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class RegenerateTransactionIdsTest
    {
        public virtual void RegeneratesTransactionIdsWhenTransactionExpiredIsReturned()
        {
			int count = 0;
			HashSet<TransactionId> transactionIds = [];
            List<Proto.TransactionResponse>  responses = 
            [
				new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.TransactionExpired },
				new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.TransactionExpired }, 
                new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.TransactionExpired }, 
                new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.Ok }
            ];

			Func<object, object> call = (o) =>
            {
                try
                {
                    var transaction = (Transaction<>)o;
                    var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(transaction.SignedTransactionBytes);
                    var transactionBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);
                    var transactionId = TransactionId.FromProtobuf(transactionBody.TransactionID);
                    if (transactionIds.Contains(transactionId))
                    {
                        return new RuntimeWrappedException(StatusCode.Aborted);
                    }

                    transactionIds.Add(transactionId);

                    return responses[Interlocked.Increment(ref count)];
                }
                catch (Exception e)
                {
                    return e;
                }
            };

            using (var mocker = Mocker.WithResponses([call, call, call, call]))
            {
                new FileCreateTransaction().Execute(mocker.client);
            }
        }
    }
}