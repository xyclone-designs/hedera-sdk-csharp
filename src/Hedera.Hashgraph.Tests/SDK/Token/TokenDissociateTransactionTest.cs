// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenDissociateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId testAccountId = AccountId.FromString("6.9.0");
        private static readonly List<TokenId> testTokenIds = [TokenId.FromString("4.2.0"), TokenId.FromString("4.2.1"), TokenId.FromString("4.2.2") ];
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenDissociateTransaction();
            var tx2 = Transaction.FromBytes<TokenDissociateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenDissociateTransaction SpawnTestTransaction()
        {
            return new TokenDissociateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AccountId = testAccountId,
				TokenIds = testTokenIds,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenDissociateTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenDissociate = new Proto.TokenDissociateTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenDissociateTransaction>(transactionBody);

            Assert.IsType<TokenDissociateTransaction>(tx);
        }

        public virtual void ConstructTokenDissociateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenDissociateTransactionBody
            {
                Account = testAccountId.ToProtobuf(),

            };
            transactionBody.Tokens.AddRange(testTokenIds.Select(_ => _.ToProtobuf()));

            var tx = new Proto.TransactionBody
            {
				TokenDissociate = transactionBody
			};
            var tokenDissociateTransaction = new TokenDissociateTransaction(tx);
            Assert.Equal(tokenDissociateTransaction.AccountId, testAccountId);
            Assert.Equal(tokenDissociateTransaction.TokenIds.Count, testTokenIds.Count);
        }

        public virtual void GetSetAccountId()
        {
            var tokenDissociateTransaction = new TokenDissociateTransaction
            {
				AccountId = testAccountId
			};
            Assert.Equal(tokenDissociateTransaction.AccountId, testAccountId);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AccountId = testAccountId);
        }

        public virtual void GetSetTokenIds()
        {
            var tokenDissociateTransaction = new TokenDissociateTransaction
            {
				TokenIds = testTokenIds
			};
            Assert.Equal(tokenDissociateTransaction.TokenIds, testTokenIds);
        }

        public virtual void GetSetTokenIdsFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenIds = testTokenIds);
        }
    }
}