// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Java.Nio.Charset;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class PrivateKeyTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        virtual void SignTransactionWorks()
        {
            byte[] bytes = new AccountCreateTransaction().SetNodeAccountIds(Collections.SingletonList(AccountId.FromString("0.0.5005"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetKeyWithoutAlias(unusedPrivateKey).SetInitialBalance(Hbar.FromTinybars(450)).SetProxyAccountId(AccountId.FromString("0.0.1001")).SetReceiverSignatureRequired(true).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().ToBytes();
            AccountCreateTransaction transaction = (AccountCreateTransaction)Transaction.FromBytes(bytes);
            unusedPrivateKey.SignTransaction(transaction);
        }

        virtual void Ecdsa()
        {
            var message = "hello world".GetBytes(StandardCharsets.UTF_8);
            var key = PrivateKey.FromStringECDSA("8776c6b831a1b61ac10dac0304a2843de4716f54b1919bb91a2685d0fe3f3048");
            var signature = key.Sign(message);
            Assert.Equal(Hex.ToHexString(signature), "f3a13a555f1f8cd6532716b8f388bd4e9d8ed0b252743e923114c0c6cbfe414cf791c8e859afd3c12009ecf2cb20dacf01636d80823bcdbd9ec1ce59afe008f0");
        }

        virtual void Supports0xPrefix()
        {
            PrivateKey.FromString("0x8776c6b831a1b61ac10dac0304a2843de4716f54b1919bb91a2685d0fe3f3048");
        }
    }
}