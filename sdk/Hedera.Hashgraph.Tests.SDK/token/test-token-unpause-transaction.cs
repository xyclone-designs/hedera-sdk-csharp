// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using VerifyXunit;
using Hedera.Hashgraph.SDK.Core;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    /// <include file="test-token-unpause-transaction.ts.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Token.TokenUnpauseTransactionTest"]' />
    public class TokenUnpauseTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public virtual TokenUnpauseTransaction SpawnTestTransaction()
        {
            return new TokenUnpauseTransaction
            {
				NodeAccountIds = new (AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")),
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				TokenId = testTokenId,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        /// <include file="test-token-unpause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenUnpauseTransactionTest.ShouldBytesNoSetters"]' />
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
        [Fact]
        /// <include file="test-token-unpause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenUnpauseTransactionTest.ShouldBytesNft"]' />
        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenCreateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-token-unpause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenUnpauseTransactionTest.FromScheduledTransaction"]' />
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
				TokenUnpause = new Proto.Services.TokenUnpauseTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenUnpauseTransaction>(transactionBody);
            
            Assert.IsType<TokenUnpauseTransaction>(tx);
        }
        [Fact]
        /// <include file="test-token-unpause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenUnpauseTransactionTest.ConstructTokenUnpauseTransactionFromTransactionBodyProtobuf"]' />
        public virtual void ConstructTokenUnpauseTransactionFromTransactionBodyProtobuf()
        {
            var tx = new Proto.Services.TransactionBody
            {
				TokenUnpause = new Proto.Services.TokenUnpauseTransactionBody
				{
					Token = testTokenId.ToProtobuf()
				}
			};
            var tokenUnpauseTransaction = new TokenUnpauseTransaction(tx);

            Assert.Equal(tokenUnpauseTransaction.TokenId, testTokenId);
        }
        [Fact]
        /// <include file="test-token-unpause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenUnpauseTransactionTest.GetSetTokenId"]' />
        public virtual void GetSetTokenId()
        {
            var tokenUnpauseTransaction = new TokenUnpauseTransaction
            {
				TokenId = testTokenId
			};
            Assert.Equal(tokenUnpauseTransaction.TokenId, testTokenId);
        }
        [Fact]
        /// <include file="test-token-unpause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenUnpauseTransactionTest.GetSetTokenIdFrozen"]' />
        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
    }
}
