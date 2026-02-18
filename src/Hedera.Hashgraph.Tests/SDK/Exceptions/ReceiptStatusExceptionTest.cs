// SPDX-License-Identifier: Apache-2.0
using System;
using Hedera.Hashgraph.SDK.Transactions;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.Tests.SDK.Exceptions
{
    class ReceiptStatusExceptionTest
    {
        public virtual void ShouldHaveMessage()
        {
            var validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
            var txId = new TransactionId(new AccountId(0, 0, 100), Timestamp.FromDateTimeOffset(validStart));
            var txReceipt = TransactionReceipt.FromProtobuf(new Proto.TransactionReceipt { Status = Proto.ResponseCodeEnum.InsufficientTxFee });
            var e = new ReceiptStatusException(txId, txReceipt);
            Assert.Equal(e.Message, "receipt for transaction 0.0.100@1554158542.000000000 raised status INSUFFICIENT_TX_FEE");
        }
    }
}