// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenAssociateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId accountId = AccountId.FromString("1.2.3");
        private static readonly List<TokenId> tokenIds = [TokenId.FromString("4.5.6"), TokenId.FromString("7.8.9"), TokenId.FromString("10.11.12")];
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenAssociateTransaction();
            var tx2 = Transaction.FromBytes<TokenAssociateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenAssociateTransaction SpawnTestTransaction()
        {
            return new TokenAssociateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				AccountId = AccountId.FromString("0.0.222"),
				TokenIds = [TokenId.FromString("0.0.666")],
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenAssociateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                TokenAssociate = new Proto.TokenAssociateTransactionBody()
            };
            var tx = Transaction.FromScheduledTransaction<TokenAssociateTransaction>(transactionBody);
            Assert.IsType<TokenAssociateTransaction>(tx);
        }

        public virtual void ConstructTokenDeleteTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenAssociateTransactionBody
            {
				Account = accountId.ToProtobuf(),

            };
            transactionBody.Tokens.AddRange(tokenIds.Select(_ => _.ToProtobuf()));

            var txBody = new Proto.TransactionBody
            {
				TokenAssociate = transactionBody
			};
            var tokenAssociateTransaction = new TokenAssociateTransaction(txBody);
            Assert.Equal(tokenAssociateTransaction.AccountId, accountId);

            Assert.Equal(tokenAssociateTransaction.TokenIds.Count, tokenIds.Count);
        }

        public virtual void GetSetAccountId()
        {
            var transaction = new TokenAssociateTransaction
            {
				AccountId = accountId,
			};
            Assert.Equal(transaction.AccountId, accountId);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => transaction.AccountId = accountId);
        }

        public virtual void GetSetTokenIds()
        {
            var transaction = new TokenAssociateTransaction
            {
				TokenIds = tokenIds
			};
            Assert.Equal(transaction.TokenIds, tokenIds);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => transaction.TokenIds = tokenIds);
        }
    }
}