// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenUnpauseTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual TokenUnpauseTransaction SpawnTestTransaction()
        {
            return new TokenUnpauseTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				TokenId = testTokenId,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenUnpauseTransaction();
            var tx2 = Transaction.FromBytes<TokenUnpauseTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenCreateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenUnpause = new Proto.TokenUnpauseTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenUnpauseTransaction>(transactionBody);
            
            Assert.IsType<TokenUnpauseTransaction>(tx);
        }

        public virtual void ConstructTokenUnpauseTransactionFromTransactionBodyProtobuf()
        {
            var tx = new Proto.TransactionBody
            {
				TokenUnpause = new Proto.TokenUnpauseTransactionBody
				{
					Token = testTokenId.ToProtobuf()
				}
			};
            var tokenUnpauseTransaction = new TokenUnpauseTransaction(tx);

            Assert.Equal(tokenUnpauseTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenId()
        {
            var tokenUnpauseTransaction = new TokenUnpauseTransaction
            {
				TokenId = testTokenId
			};
            Assert.Equal(tokenUnpauseTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
    }
}