// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenDeleteTransaction();
            var tx2 = Transaction.FromBytes<TokenDeleteTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenDeleteTransaction SpawnTestTransaction()
        {
            return new TokenDeleteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				TokenId = TokenId.FromString("1.2.3"),
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenDeleteTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenDeletion = new Proto.TokenDeleteTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenDeleteTransaction>(tx);
        }

        public virtual void ConstructTokenDeleteTransaction()
        {
            var transaction = new TokenDeleteTransaction();
            
            Assert.Null(transaction.TokenId);
        }

        public virtual void ConstructTokenDeleteTransactionFromTransactionBodyProtobuf()
        {
            var tokenId = TokenId.FromString("1.2.3");
            var transactionBody = new Proto.TokenDeleteTransactionBody
            {
                Token = tokenId.ToProtobuf()
            };
            var txBody = new Proto.TransactionBody
            {
                TokenDeletion = transactionBody
            };
            var tokenDeleteTransaction = new TokenDeleteTransaction(txBody);
            
            Assert.Equal(tokenDeleteTransaction.TokenId, tokenId);
        }

        public virtual void GetSetTokenId()
        {
            var tokenId = TokenId.FromString("1.2.3");
            var transaction = new TokenDeleteTransaction
            {
				TokenId = tokenId,
			};
            Assert.Equal(transaction.TokenId, tokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tokenId = TokenId.FromString("1.2.3");
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = tokenId);
        }
    }
}