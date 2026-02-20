// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;

using Org.BouncyCastle.Utilities.Encoders;

using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.Tests.SDK.Keys
{
    public class PrivateKeyTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public virtual void SignTransactionWorks()
        {
            byte[] bytes = new AccountCreateTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				InitialBalance = Hbar.FromTinybars(450),
				ProxyAccountId = AccountId.FromString("0.0.1001"),
				ReceiverSigRequired = true,
				MaxTransactionFee = Hbar.FromTinybars(100000),
			}
            Key = unusedPrivateKey,
            .Freeze()
            .ToBytes();
            AccountCreateTransaction transaction = Transaction.FromBytes<AccountCreateTransaction>(bytes);
            
            unusedPrivateKey.SignTransaction(transaction);
        }

        public virtual void Ecdsa()
        {
            var message = Encoding.UTF8.GetBytes("hello world");
            var key = PrivateKey.FromStringECDSA("8776c6b831a1b61ac10dac0304a2843de4716f54b1919bb91a2685d0fe3f3048");
            var signature = key.Sign(message);
            Assert.Equal(Hex.ToHexString(signature), "f3a13a555f1f8cd6532716b8f388bd4e9d8ed0b252743e923114c0c6cbfe414cf791c8e859afd3c12009ecf2cb20dacf01636d80823bcdbd9ec1ce59afe008f0");
        }

        public virtual void Supports0xPrefix()
        {
            PrivateKey.FromString("0x8776c6b831a1b61ac10dac0304a2843de4716f54b1919bb91a2685d0fe3f3048");
        }
    }
}