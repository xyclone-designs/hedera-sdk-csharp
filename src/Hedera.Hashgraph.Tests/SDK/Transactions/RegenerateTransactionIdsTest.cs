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
            List<Proto.Services.TransactionResponse>  responses = 
            [
				new Proto.Services.TransactionResponse { NodeTransactionPrecheckCode = Proto.Services.ResponseCodeEnum.TransactionExpired },
				new Proto.Services.TransactionResponse { NodeTransactionPrecheckCode = Proto.Services.ResponseCodeEnum.TransactionExpired }, 
                new Proto.Services.TransactionResponse { NodeTransactionPrecheckCode = Proto.Services.ResponseCodeEnum.TransactionExpired }, 
                new Proto.Services.TransactionResponse { NodeTransactionPrecheckCode = Proto.Services.ResponseCodeEnum.Ok }
            ];

			//Func<object, object> call = (o) =>
   //         {
   //             try
   //             {
   //                 var transaction = (ITransaction)o;
   //                 var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(transaction.SignedTransactionBytes);
   //                 var transactionBody = Proto.Services.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);
   //                 var transactionId = TransactionId.FromProtobuf(transactionBody.TransactionID);
   //                 if (transactionIds.Contains(transactionId))
   //                 {
   //                     return new RuntimeWrappedException(StatusCode.Aborted);
   //                 }

   //                 transactionIds.Add(transactionId);

   //                 return responses[Interlocked.Increment(ref count)];
   //             }
   //             catch (Exception e)
   //             {
   //                 return e;
   //             }
   //         };

   //         using (var mocker = Mocker.WithResponses([call, call, call, call]))
   //         {
   //             new FileCreateTransaction().Execute(mocker.client);
   //         }
        }
    }
}