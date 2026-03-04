// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    class BatchTransactionTest
    {
        private static readonly PrivateKey privateKeyED25519 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PrivateKey privateKeyECDSA = PrivateKey.FromStringECDSA("7f109a9e3b0d8ecfba9cc23a3614433ce0fa7ddcc80f2a8f10b222179a5a80d6");
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private static readonly List<Transaction<T>> INNER_TRANSACTIONS = [SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate()];
        private static AccountCreateTransaction SpawnTestTransactionAccountCreate()
        {
            return new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ReceiverSigRequired = true,
				AutoRenewPeriod = TimeSpan.FromHours(10),
				StakedAccountId = AccountId.FromString("0.0.3"),
				Alias = EvmAddress.FromString("0x5c562e90feaf0eebd33ea75d21024f249d451417"),
				MaxAutomaticTokenAssociations = 100,
				MaxTransactionFee = Hbar.FromTinybars(100000),
				BatchKey = privateKeyECDSA,
				Key = privateKeyED25519,
				InitialBalance = Hbar.FromTinybars(450),
				AccountMemo = "some memo",
			}
            .SetKeyWithAlias(privateKeyECDSA)
            .SetKeyWithAlias(privateKeyED25519, privateKeyECDSA)
            .Freeze()
            .Sign(privateKeyED25519);
        }

        private BatchTransaction SpawnTestTransaction()
        {
            var batchKey = PrivateKey.GenerateECDSA();
            
            return new BatchTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				InnerTransactions = INNER_TRANSACTIONS,
			}
            .Freeze()
            .Sign(batchKey);
        }

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

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<BatchTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new BatchTransaction();
            var tx2 = Transaction.FromBytes<BatchTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void GetInnerTransactionsShouldReturnCorrectTransactions()
        {
            var batchTransaction = SpawnTestTransaction();

			Assert.Equal(batchTransaction.InnerTransactions.Count, 3);
			Assert.Equal(batchTransaction.InnerTransactions[0], INNER_TRANSACTIONS[0]);
			Assert.Equal(batchTransaction.InnerTransactions[1], INNER_TRANSACTIONS[1]);
			Assert.Equal(batchTransaction.InnerTransactions[2], INNER_TRANSACTIONS[2]);
		}

        public virtual void SetInnerTransactionsShouldUpdateTransactions()
        {
            var batchTransaction = new BatchTransaction();
            IList<Transaction> newInnerTransactions = [ SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate() ];
            batchTransaction.InnerTransactions.ClearAndSet(newInnerTransactions);
            
			Assert.Equal(batchTransaction.InnerTransactions.Count, 2);
			Assert.Equal(batchTransaction.InnerTransactions[0], newInnerTransactions[0]);
			Assert.Equal(batchTransaction.InnerTransactions[1], newInnerTransactions[1]);
		}

        public virtual void InnerTransactionsAddShouldAppendTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var newTransaction = SpawnTestTransactionAccountCreate();
            batchTransaction.InnerTransactions.Add(newTransaction);

            Assert.Equal(batchTransaction.InnerTransactions.Count, 1);
            Assert.Equal(batchTransaction.InnerTransactions[0], newTransaction);
        }

        public virtual void GetInnerTransactionIdsShouldReturnCorrectIds()
        {
            var batchTransaction = SpawnTestTransaction();
            var expectedTransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart);
            var transactionIds = batchTransaction.InnerTransactions.Select(_ => _.TransactionId);
            
            Assert.Equal(transactionIds.Count(), 3);
            Assert.All(transactionIds, (id) => Equals(id, expectedTransactionId));
        }

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

        public virtual void ShouldRejectFreezeTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var freezeTransaction = new FreezeTransaction
            {
				StartTime = DateTimeOffset.UtcNow.ToTimestamp(),
				FreezeType = FreezeType.FreezeOnly,
				NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),

			}.Freeze();

			InvalidOperationException exception = Assert.Throws<ArgumentException>(() => batchTransaction.InnerTransactions.Add(freezeTransaction));
            Assert.Contains(exception.Message, "FreezeTransaction is not allowed in a batch transaction");
		}

        public virtual void ShouldRejectBatchTransaction()
        {
            var batchTransaction = new BatchTransaction
            {
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")]

			}.Freeze();

			InvalidOperationException exception = Assert.Throws<ArgumentException>(() => batchTransaction.InnerTransactions.Add(innerBatchTransaction));
            Assert.Contains(exception.Message, "BatchTransaction is not allowed in a batch transaction");
		}

        public virtual void ShouldRejectBlacklistedTransactionInList()
        {
            var batchTransaction = new BatchTransaction();
            var validTransaction = SpawnTestTransactionAccountCreate();
            var freezeTransaction = new FreezeTransaction
            {
				StartTime = DateTimeOffset.UtcNow.ToTimestamp(),
				FreezeType = FreezeType.FreezeOnly,
				NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
			
            InvalidOperationException exception = Assert.Throws<ArgumentException>(() => batchTransaction.InnerTransactions.AddRange(validTransaction, freezeTransaction));
            Assert.Contains(exception.Message, "FreezeTransaction is not allowed in a batch transaction");
		}

        public virtual void ShouldRejectUnfrozenTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var unfrozenTransaction = new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			};

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(unfrozenTransaction));
			Assert.Contains(exception.Message, "Inner transaction should be frozen");
		}

        public virtual void ShouldRejectTransactionAfterFreeze()
        {
            var batchTransaction = new BatchTransaction
            {
				NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(SpawnTestTransactionAccountCreate()));
            Assert.Contains(exception.Message, "transaction is immutable");
		}

        public virtual void ShouldRejectTransactionListAfterFreeze()
        {
            var batchTransaction = new BatchTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.AddRange(INNER_TRANSACTIONS));
			Assert.Contains(exception.Message, "transaction is immutable");
		}

        public virtual void ShouldPreserveTransactionOrder()
        {
            var batchTransaction = new BatchTransaction();
            var transaction1 = SpawnTestTransactionAccountCreate();
            var transaction2 = SpawnTestTransactionAccountCreate();
            var transaction3 = SpawnTestTransactionAccountCreate();
            IList<Transaction> transactions = [transaction1, transaction2, transaction3];
            batchTransaction.InnerTransactions.ClearAndSet(transactions);
            AssertThat(batchTransaction.GetInnerTransactions()).ContainsExactly(transaction1, transaction2, transaction3);
        }

        public virtual void ShouldCreateDefensiveCopyOfTransactionList()
        {
            var batchTransaction = new BatchTransaction();
            var mutableList = new List<Transaction<T>>(INNER_TRANSACTIONS);
            batchTransaction.InnerTransactions.ClearAndSet(mutableList);
            mutableList.Clear();

            Assert.Equal(batchTransaction.GetInnerTransactions()).IsNotNull().HasSize(3, INNER_TRANSACTIONS);
        }

        public virtual void ShouldRejectTransactionWithoutBatchKey()
        {
            var batchTransaction = new BatchTransaction();
            var transactionWithoutBatchKey = new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(transactionWithoutBatchKey));
			Assert.Contains(exception.Message, "Batch key needs to be set");
		}

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
			Assert.Contains(exception.Message, "Batch key needs to be set");
		}

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
            Assert.Contains(exception.Message, "Inner transaction should be frozen");

			// Test frozen transaction with no batch key
			var frozenTransactionWithoutBatchKey = unfrozenTransactionWithoutBatchKey.Freeze();
            InvalidOperationException exception1 = Assert.Throws<InvalidOperationException>(() => batchTransaction.InnerTransactions.Add(frozenTransactionWithoutBatchKey));
            Assert.Contains(exception1.Message, "Batch key needs to be set");

			// Test blacklisted transaction with batch key
			var blacklistedTransaction = new FreezeTransaction
            {
				StartTime = DateTimeOffset.UtcNow.ToTimestamp(),
				FreezeType = FreezeType.FreezeOnly,
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				BatchKey = privateKeyECDSA
			
            }.Freeze();

            InvalidOperationException exception2 = Assert.Throws<ArgumentException>(() => batchTransaction.InnerTransactions.Add(blacklistedTransaction));
            Assert.Contains(exception2.Message, "FreezeTransaction is not allowed in a batch transaction");
		}

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

            Assert.Contains(batchTransaction.InnerTransactions).IsNotNull().HasSize(1, validTransaction);
        }

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