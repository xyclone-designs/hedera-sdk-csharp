// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK;

using VerifyXunit;
using Hedera.Hashgraph.SDK.Core;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest"]' />
    public class BatchTransactionTest
    {
        private static readonly PrivateKey privateKeyED25519 = KeyTestDataFactory.ED25519_TEST_KEY;
        private static readonly PrivateKey privateKeyECDSA = KeyTestDataFactory.ECDSA_TEST_KEY;
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeSeconds(1554158542);
        private static readonly List<ITransaction> INNER_TRANSACTIONS = [SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate()];

        private static AccountCreateTransaction SpawnTestTransactionAccountCreate()
        {
            return TransactionTestFactory.SpawnAccountCreateTransaction(privateKeyED25519, privateKeyECDSA);
        }

        private BatchTransaction SpawnTestTransaction()
        {
            var batchKey = KeyTestDataFactory.CreateECDSAKey();

            return new BatchTransaction
            {
				NodeAccountIds = TransactionTestFactory.CreateDefaultNodeAccountIds(),
				TransactionId = TransactionTestFactory.CreateDefaultTransactionId(),
				InnerTransactions = [.. INNER_TRANSACTIONS],
			}
            .Freeze()
            .Sign(batchKey);
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldBytes"]' />
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<BatchTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldBytesNoSetters"]' />
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new BatchTransaction();
            var tx2 = Transaction.FromBytes<BatchTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.GetInnerTransactionsShouldReturnCorrectTransactions"]' />
        public virtual void GetInnerTransactionsShouldReturnCorrectTransactions()
        {
            var batchTransaction = SpawnTestTransaction();

			Assert.Equal(batchTransaction.InnerTransactions.Count, 3);
			Assert.Equal(batchTransaction.InnerTransactions[0], INNER_TRANSACTIONS[0]);
			Assert.Equal(batchTransaction.InnerTransactions[1], INNER_TRANSACTIONS[1]);
			Assert.Equal(batchTransaction.InnerTransactions[2], INNER_TRANSACTIONS[2]);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.SetInnerTransactionsShouldUpdateTransactions"]' />
        public virtual void SetInnerTransactionsShouldUpdateTransactions()
        {
            var batchTransaction = new BatchTransaction();
            IList<ITransaction> newInnerTransactions = [ SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate() ];
            batchTransaction.InnerTransactions.ClearAndSet(newInnerTransactions);
            
			Assert.Equal(batchTransaction.InnerTransactions.Count, 2);
			Assert.Equal(batchTransaction.InnerTransactions[0], newInnerTransactions[0]);
			Assert.Equal(batchTransaction.InnerTransactions[1], newInnerTransactions[1]);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.InnerTransactionsAddShouldAppendTransaction"]' />
        public virtual void InnerTransactionsAddShouldAppendTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var newTransaction = SpawnTestTransactionAccountCreate();
            batchTransaction.InnerTransactions.Add(newTransaction);

            Assert.Equal(batchTransaction.InnerTransactions.Count, 1);
            Assert.Equal(batchTransaction.InnerTransactions[0], newTransaction);
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.GetInnerTransactionIdsShouldReturnCorrectIds"]' />
        public virtual void GetInnerTransactionIdsShouldReturnCorrectIds()
        {
            var batchTransaction = SpawnTestTransaction();
            var expectedTransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart);
            var transactionIds = batchTransaction.InnerTransactions.Select(_ => _.TransactionId);
            
            Assert.Equal(transactionIds.Count(), 3);
            Assert.All(transactionIds, (id) => Equals(id, expectedTransactionId));
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldAllowChainedSetters"]' />
        public virtual void ShouldAllowChainedSetters()
        {
            var batchTransaction = new BatchTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                InnerTransactions = [SpawnTestTransactionAccountCreate()],

			} .Freeze();
            
            Assert.Single(batchTransaction.InnerTransactions);
            Assert.Single(batchTransaction.NodeAccountIds);
            Assert.NotNull(batchTransaction.TransactionId);
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldRejectFreezeTransaction"]' />
        public virtual void ShouldRejectFreezeTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var freezeTransaction = new FreezeTransaction
            {
				StartTime = DateTimeOffset.UtcNow,
				FreezeType = FreezeType.FreezeOnly,
				NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),

			}.Freeze();

			InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                batchTransaction.InnerTransactions.Add(freezeTransaction);
            });
            Assert.Contains("Transaction type FreezeTransaction is not allowed in a batch transaction", exception.Message);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldRejectBatchTransaction"]' />
        public virtual void ShouldRejectBatchTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var innerBatchTransaction = new BatchTransaction
            {
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")]

			}.Freeze();

			InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(innerBatchTransaction));
            Assert.Contains("Transaction type BatchTransaction is not allowed in a batch transaction", exception.Message);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldRejectBlacklistedTransactionInList"]' />
        public virtual void ShouldRejectBlacklistedTransactionInList()
        {
            var batchTransaction = new BatchTransaction();
            var validTransaction = SpawnTestTransactionAccountCreate();
            var freezeTransaction = new FreezeTransaction
            {
				StartTime = DateTimeOffset.UtcNow,
				FreezeType = FreezeType.FreezeOnly,
				NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
			
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.AddRange(validTransaction, freezeTransaction));
            Assert.Contains("Transaction type FreezeTransaction is not allowed in a batch transaction", exception.Message);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldRejectUnfrozenTransaction"]' />
        public virtual void ShouldRejectUnfrozenTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var unfrozenTransaction = new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			};

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(unfrozenTransaction));
			Assert.Contains("Inner transaction should be frozen", exception.Message);
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldRejectTransactionAfterFreeze"]' />
        public virtual void ShouldRejectTransactionAfterFreeze()
        {
            var batchTransaction = new BatchTransaction
            {
				NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(SpawnTestTransactionAccountCreate()));
            Assert.Contains("transaction is immutable", exception.Message);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldRejectTransactionListAfterFreeze"]' />
        public virtual void ShouldRejectTransactionListAfterFreeze()
        {
            var batchTransaction = new BatchTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.AddRange(INNER_TRANSACTIONS));
			Assert.Contains("transaction is immutable", exception.Message);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldPreserveTransactionOrder"]' />
        public virtual void ShouldPreserveTransactionOrder()
        {
            var batchTransaction = new BatchTransaction();
            var transaction1 = SpawnTestTransactionAccountCreate();
            var transaction2 = SpawnTestTransactionAccountCreate();
            var transaction3 = SpawnTestTransactionAccountCreate();
            
            IList<ITransaction> transactions = [transaction1, transaction2, transaction3];
            batchTransaction.InnerTransactions.ClearAndSet(transactions);
            
            Assert.Equal(batchTransaction.InnerTransactions, [transaction1, transaction2, transaction3]);
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldCreateDefensiveCopyOfTransactionList"]' />
        public virtual void ShouldCreateDefensiveCopyOfTransactionList()
        {
            var batchTransaction = new BatchTransaction();
            List<ITransaction> mutableList = [.. INNER_TRANSACTIONS];
            batchTransaction.InnerTransactions.ClearAndSet(mutableList);
            mutableList.Clear();

            Assert.Equal(batchTransaction.InnerTransactions, INNER_TRANSACTIONS);
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldRejectTransactionWithoutBatchKey"]' />
        public virtual void ShouldRejectTransactionWithoutBatchKey()
        {
            var batchTransaction = new BatchTransaction();
            var transactionWithoutBatchKey = new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(transactionWithoutBatchKey));
			Assert.Contains("Batch key needs to be set", exception.Message);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldValidateAllTransactionsInList"]' />
        public virtual void ShouldValidateAllTransactionsInList()
        {
            var batchTransaction = new BatchTransaction();
            var validTransaction = SpawnTestTransactionAccountCreate();
            var transactionWithoutBatchKey = new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),

			}.Freeze();
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.ClearAndSet(validTransaction, transactionWithoutBatchKey));
			Assert.Contains("Batch key needs to be set", exception.Message);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldValidateMultipleConditions"]' />
        public virtual void ShouldValidateMultipleConditions()
        {
            var batchTransaction = new BatchTransaction();

            // Test unfrozen transaction with no batch key
            var unfrozenTransactionWithoutBatchKey = new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			};
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(unfrozenTransactionWithoutBatchKey));
            Assert.Contains("Inner transaction should be frozen", exception.Message);

			// Test frozen transaction with no batch key
			var frozenTransactionWithoutBatchKey = unfrozenTransactionWithoutBatchKey.Freeze();
            InvalidOperationException exception1 = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(frozenTransactionWithoutBatchKey));
            Assert.Contains("Batch key needs to be set", exception1.Message);

			// Test blacklisted transaction with batch key
			var blacklistedTransaction = new FreezeTransaction
            {
				StartTime = DateTimeOffset.UtcNow,
				FreezeType = FreezeType.FreezeOnly,
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				BatchKey = privateKeyECDSA
			
            }.Freeze();

            InvalidOperationException exception2 = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(blacklistedTransaction));
            Assert.Contains("FreezeTransaction is not allowed in a batch transaction", exception2.Message);
		}
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldAcceptValidTransaction"]' />
        public virtual void ShouldAcceptValidTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var validTransaction = new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				BatchKey = privateKeyECDSA,

			}.Freeze();
            
            batchTransaction.InnerTransactions.Add(validTransaction);

            Assert.Equal(batchTransaction.InnerTransactions, [validTransaction]);
        }
        [Fact]
        /// <include file="test-transactions-batch.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.BatchTransactionTest.ShouldValidateTransactionStateInOrder"]' />
        public virtual void ShouldValidateTransactionStateInOrder()
        {
            var batchTransaction = new BatchTransaction();
            var transaction = new AccountCreateTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)
            };

            // First check should be for frozen state
            InvalidOperationException exception1 = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(transaction));
			Assert.Contains(exception1.Message, "Inner transaction should be frozen");

			// After freezing, next check should be for batch key
			var frozenTransaction = transaction.Freeze();
            InvalidOperationException exception2 = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(frozenTransaction));
            Assert.Contains(exception2.Message, "Batch key needs to be set");
        }
    }
}
