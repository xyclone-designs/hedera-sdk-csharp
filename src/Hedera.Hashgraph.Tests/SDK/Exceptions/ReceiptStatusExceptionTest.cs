// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Proto;
using Java.Time;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Exceptions
{
    class ReceiptStatusExceptionTest
    {
        public virtual void ShouldHaveMessage()
        {
            var validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
            var txId = new TransactionId(new AccountId(0, 0, 100), Timestamp.FromDateTimeOffset(validStart));
            var txReceipt = TransactionReceipt.FromProtobuf(Proto.TransactionReceipt.NewBuilder().SetStatusValue(ResponseCodeEnum.INSUFFICIENT_TX_FEE_VALUE).Build());
            var e = new ReceiptStatusException(txId, txReceipt);
            Assert.Equal(e.GetMessage(), "receipt for transaction 0.0.100@1554158542.000000000 raised status INSUFFICIENT_TX_FEE");
        }
    }
}