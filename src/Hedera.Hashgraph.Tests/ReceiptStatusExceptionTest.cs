// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Java.Time;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class ReceiptStatusExceptionTest
    {
        virtual void ShouldHaveMessage()
        {
            var validStart = Instant.OfEpochSecond(1554158542);
            var txId = new TransactionId(new AccountId(0, 0, 100), validStart);
            var txReceipt = TransactionReceipt.FromProtobuf(com.hedera.hashgraph.sdk.proto.TransactionReceipt.NewBuilder().SetStatusValue(ResponseCodeEnum.INSUFFICIENT_TX_FEE_VALUE).Build());
            var e = new ReceiptStatusException(txId, txReceipt);
            Assert.Equal(e.GetMessage(), "receipt for transaction 0.0.100@1554158542.000000000 raised status INSUFFICIENT_TX_FEE");
        }
    }
}