// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using VerifyXunit;
using Hedera.Hashgraph.SDK.Core;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    /// <include file="test-token-pause-transaction.ts.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Token.TokenPauseTransactionTest"]' />
    public class TokenPauseTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public virtual TokenPauseTransaction SpawnTestTransaction()
        {
            return new TokenPauseTransaction
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
        /// <include file="test-token-pause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenPauseTransactionTest.ShouldSerialize"]' />
        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        /// <include file="test-token-pause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenPauseTransactionTest.ShouldBytesNft"]' />
        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenPauseTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-token-pause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenPauseTransactionTest.ShouldBytesNoSetters"]' />
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenPauseTransaction();
            var tx2 = Transaction.FromBytes<TokenPauseTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-token-pause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenPauseTransactionTest.FromScheduledTransaction"]' />
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
				TokenPause = new Proto.Services.TokenPauseTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenPauseTransaction>(transactionBody);

            Assert.IsType<TokenPauseTransaction>(tx);
        }
        [Fact]
        /// <include file="test-token-pause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenPauseTransactionTest.ConstructTokenPauseTransactionFromTransactionBodyProtobuf"]' />
        public virtual void ConstructTokenPauseTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.Services.TokenPauseTransactionBody
            {
				Token = testTokenId.ToProtobuf()
			};
            var tx = new Proto.Services.TransactionBody
            {
				TokenPause = transactionBody
			};
            var tokenPauseTransaction = new TokenPauseTransaction(tx);

            Assert.Equal(tokenPauseTransaction.TokenId, testTokenId);
        }
        [Fact]
        /// <include file="test-token-pause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenPauseTransactionTest.GetSetTokenId"]' />
        public virtual void GetSetTokenId()
        {
            var tokenPauseTransaction = new TokenPauseTransaction
            {
				TokenId = testTokenId
			};
            Assert.Equal(tokenPauseTransaction.TokenId, testTokenId);
        }
        [Fact]
        /// <include file="test-token-pause-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenPauseTransactionTest.GetSetTokenIdFrozen"]' />
        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
    }
}
