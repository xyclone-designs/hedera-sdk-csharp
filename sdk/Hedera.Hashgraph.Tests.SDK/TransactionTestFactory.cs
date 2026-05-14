using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Ethereum;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Tests.SDK
{
    public static class TransactionTestFactory
    {
        public static readonly DateTimeOffset DEFAULT_VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public static AccountId DefaultNodeAccountId => AccountId.FromString(TestData.DEFAULT_ENTITY_ID);
        public static AccountId SecondaryNodeAccountId => AccountId.FromString(TestData.SECONDARY_ENTITY_ID);
        public static AccountId DefaultStakedAccountId => AccountId.FromString(TestData.STANDARD_ENTITY_ID);

        public static TransactionId CreateDefaultTransactionId(PrivateKey privateKey = null)
        {
            return TransactionId.WithValidStart(SecondaryNodeAccountId, DEFAULT_VALID_START);
        }

        public static ListGuarded<AccountId> CreateDefaultNodeAccountIds()
        {
            return new ListGuarded<AccountId> { DefaultNodeAccountId, SecondaryNodeAccountId };
        }

        public static T ApplyDefaultTransactionProperties<T>(T transaction, PrivateKey signingKey) where T : ITransaction
        {
            transaction.NodeAccountIds = CreateDefaultNodeAccountIds();
            transaction.TransactionId = CreateDefaultTransactionId();
            return transaction;
        }

        public static AccountCreateTransaction SpawnAccountCreateTransaction(
            PrivateKey edKey = null,
            PrivateKey ecdsaKey = null,
            bool freeze = true,
            bool sign = true)
        {
            edKey ??= PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            ecdsaKey ??= PrivateKey.FromStringECDSA("7f109a9e3b0d8ecfba9cc23a3614433ce0fa7ddcc80f2a8f10b222179a5a80d6");

            var transaction = new AccountCreateTransaction
            {
                NodeAccountIds = CreateDefaultNodeAccountIds(),
                TransactionId = CreateDefaultTransactionId(),
                ReceiverSigRequired = true,
                AutoRenewPeriod = TimeSpan.FromHours(10),
                StakedAccountId = DefaultStakedAccountId,
                Alias = EvmAddress.FromString("0x5c562e90feaf0eebd33ea75d21024f249d451417"),
                MaxAutomaticTokenAssociations = 100,
                MaxTransactionFee = Hbar.FromTinybars(TestData.HBAR_100000),
                BatchKey = ecdsaKey,
                Key = edKey,
                InitialBalance = Hbar.FromTinybars(TestData.HBAR_450),
                AccountMemo = "some memo",
            }
            .SetKeyWithAlias(ecdsaKey)
            .SetKeyWithAlias(edKey, ecdsaKey);

            if (freeze)
            {
                transaction = transaction.Freeze() as AccountCreateTransaction;
            }

            if (sign && freeze)
            {
                transaction = transaction.Sign(edKey) as AccountCreateTransaction;
            }

            return transaction;
        }
    }
}
