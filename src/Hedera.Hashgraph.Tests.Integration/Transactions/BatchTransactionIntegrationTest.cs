// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;

using Org.BouncyCastle.Utilities.Encoders;

using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class BatchTransactionIntegrationTest
    {
        public virtual void CanCreateBatchTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateECDSA();
                var tx = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(1),
				
                }.Batchify(testEnv.Client, testEnv.OperatorKey);
				var batchTransaction = new BatchTransaction { InnerTransactions = [tx] };
				batchTransaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountIdInnerTransaction = batchTransaction.InnerTransactions.Read.Select(_ => _.TransactionId).ElementAt(0).AccountId;
                var execute = new AccountInfoQuery
                {
					AccountId = accountIdInnerTransaction

				}.Execute(testEnv.Client);
                Assert.Equal(accountIdInnerTransaction, execute.AccountId);
            }
        }

        public virtual void CanExecuteFromToBytes()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateECDSA();
                var tx = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(1),
				
                }.Batchify(testEnv.Client, testEnv.OperatorKey);
                var batchTransaction = new BatchTransaction { InnerTransactions = [ tx ] };
                var batchTransactionBytes = batchTransaction.ToBytes();
                var batchTransactionFromBytes = Transaction.FromBytes<BatchTransaction>(batchTransactionBytes);
                batchTransactionFromBytes.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountIdInnerTransaction = batchTransaction.InnerTransactions.ElementAt(0).TransactionId.AccountId;
                var execute = new AccountInfoQuery
                {
					AccountId = accountIdInnerTransaction

				}.Execute(testEnv.Client);

                Assert.Equal(accountIdInnerTransaction, execute.AccountId);
            }
        }

        public virtual void CanExecuteLargeBatchTransactionUpToMaximumRequestSize()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                BatchTransaction batchTransaction = new ();

                // 50 is the maximum limit for internal transaction inside a BatchTransaction
                for (int i = 0; i < 25; i++)
                {
                    var key = PrivateKey.GenerateECDSA();
                    var tx = new AccountCreateTransaction
                    {
						Key = key,
						InitialBalance = new Hbar(1),
					
                    }.Batchify(testEnv.Client, testEnv.OperatorKey);

                    batchTransaction.InnerTransactions.Add(tx);
                }

                batchTransaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                foreach (var innerTransactionID in batchTransaction.InnerTransactions.Read.Select(_ => _.TransactionId))
                {
                    var receipt = new TransactionReceiptQuery
                    {
						TransactionId = innerTransactionID
					
                    }.Execute(testEnv.Client);
                    
                    Assert.Equal(ResponseStatus.Success, receipt.Status);
                }
            }
        }

        public virtual void BatchTransactionWithoutInnerTransactionsShouldThrowAnError()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() => new BatchTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client));
                
                Assert.Contains(exception.Message, ResponseStatus.BatchListEmpty.ToString());
            }
        }

        public virtual void BatchTransactionWithBlacklistedInnerTransactionShouldThrowAnError()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var freezeTransaction = new FreezeTransaction
                {
					FileId = FileId.FromString("4.5.6"),
					FileHash = Hex.Decode("1723904587120938954702349857"),
					StartTime = DateTimeOffset.UtcNow,
					FreezeType = FreezeType.FreezeOnly
				
                }.Batchify(testEnv.Client, testEnv.OperatorKey);
                
                ArgumentException exception1 = Assert.Throws<ArgumentException>(() => new BatchTransaction().InnerTransactions.Add(freezeTransaction));
                Assert.Contains(exception1.Message, "Transaction type FreezeTransaction is not allowed in a batch transaction");
                
                var key = PrivateKey.GenerateECDSA();
                var tx = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(1),
				
                }.Batchify(testEnv.Client, testEnv.OperatorKey);
                
                var batchTransaction = new BatchTransaction()
                {
                    InnerTransactions = [tx]
                
                }.Batchify(testEnv.Client, testEnv.OperatorKey);

                ArgumentException exception2 = Assert.Throws<ArgumentException>(() => new BatchTransaction().InnerTransactions.Add(batchTransaction));

                Assert.Contains(exception2.Message, "Transaction type BatchTransaction is not allowed in a batch transaction");
            }
        }

        public virtual void BatchTransactionWithInvalidBatchKeyInsideInnerTransactionShouldThrowAnError()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                BatchTransaction batchTransaction = new ();

                var key = PrivateKey.GenerateECDSA();
                var invalidKey = PrivateKey.GenerateECDSA();
                var tx = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(1),
				
                }.Batchify(testEnv.Client, invalidKey.GetPublicKey());

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    batchTransaction.InnerTransactions.Add(tx);
                    batchTransaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                
                Assert.Contains(exception.Message, ResponseStatus.InvalidSignature.ToString());
            }
        }

        public virtual void ChunkedInnerTransactionsShouldBeExecutedSuccessfully()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					TopicMemo = "[e2e::TopicCreateTransaction]"
				
                }.Execute(testEnv.Client);

                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					MaxChunks = 15,
					Message = ByteString.CopyFromUtf8(Contents.BIG_CONTENTS),
				}
                .Batchify(testEnv.Client, testEnv.OperatorKey);
                
                new BatchTransaction
                {
                    InnerTransactions = [topicMessageSubmitTransaction]
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var info = new TopicInfoQuery
                {
					TopicId = topicId
				
                }.Execute(testEnv.Client);

                Assert.Equal(info.SequenceNumber, (ulong)1);
            }
        }

        public virtual void CanExecuteWithDifferentBatchKeys()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var batchKey1 = PrivateKey.GenerateED25519();
                var batchKey2 = PrivateKey.GenerateED25519();
                var batchKey3 = PrivateKey.GenerateED25519();
                var key1 = PrivateKey.GenerateECDSA();
                var account1 = new AccountCreateTransaction
                {
					Key = key1,
					InitialBalance = new Hbar(1),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                
                Assert.NotNull(account1);
                
                var batchedTransfer1 = new TransferTransaction
                {
					TransactionId = TransactionId.Generate(account1),
					BatchKey = batchKey1
				}
                .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(100))
                .AddHbarTransfer(account1, Hbar.FromTinybars(100).Negated())
                .FreezeWith(testEnv.Client).Sign(key1);
                
                var key2 = PrivateKey.GenerateECDSA();
                var account2 = new AccountCreateTransaction
                {
					Key = key2,
					InitialBalance = new Hbar(1),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                
                Assert.NotNull(account2);

                var batchedTransfer2 = new TransferTransaction
                {
					TransactionId = TransactionId.Generate(account2),
					BatchKey = batchKey2
				}
                .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(100))
                .AddHbarTransfer(account2, Hbar.FromTinybars(100).Negated())
                .FreezeWith(testEnv.Client).Sign(key2);

                var key3 = PrivateKey.GenerateECDSA();
                var account3 = new AccountCreateTransaction
                {
					Key = key3,
					InitialBalance = new Hbar(1),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                Assert.NotNull(account3);

                var batchedTransfer3 = new TransferTransaction
                {
					TransactionId = TransactionId.Generate(account3),
					BatchKey = batchKey2
				}
                .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(100))
                .AddHbarTransfer(account3, Hbar.FromTinybars(100).Negated())
                .FreezeWith(testEnv.Client).Sign(key3);
                
                var receipt = new BatchTransaction
                {
                    InnerTransactions = 
                    [
						batchedTransfer1,
						batchedTransfer2,
						batchedTransfer3,
					]
                }
                .FreezeWith(testEnv.Client)
                .Sign(batchKey1)
                .Sign(batchKey2)
                .Sign(batchKey3)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                Assert.Equal(receipt.Status, ResponseStatus.Success);
            }
        }

        public virtual void SuccessfulInnerTransactionsShouldIncurFeesEvenThoughOneFailed()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialBalance = new AccountInfoQuery
                {
					AccountId = testEnv.OperatorId,
				
                }.Execute(testEnv.Client).Balance;
                
                var key1 = PrivateKey.GenerateECDSA();
                var tx1 = new AccountCreateTransaction
                {
					Key = key1,
					InitialBalance = new Hbar(1),
				
                }.Batchify(testEnv.Client, testEnv.OperatorKey);
                
                var key2 = PrivateKey.GenerateECDSA();
                var tx2 = new AccountCreateTransaction
                {
					Key = key2,
					InitialBalance = new Hbar(1),
				
                }.Batchify(testEnv.Client, testEnv.OperatorKey);

                var key3 = PrivateKey.GenerateECDSA();
                var tx3 = new AccountCreateTransaction
                {
					Key = key3,
					ReceiverSigRequired = true,
					InitialBalance = new Hbar(1),
				
                }.Batchify(testEnv.Client, testEnv.OperatorKey);

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new BatchTransaction
                    {
                        InnerTransactions =
                        [
							tx1,
							tx2,
							tx3,
						]
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                });
                
                Assert.Contains(exception.Message, ResponseStatus.InnerTransactionFailed.ToString());
                
                var finalBalance = new AccountInfoQuery{ AccountId = testEnv.OperatorId, }.Execute(testEnv.Client).Balance;
                
                Assert.True(finalBalance.GetValue().LongValue() < initialBalance.GetValue().LongValue());
            }
        }

        public virtual void TransactionShouldFailWhenBatchified()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                {
                    new TopicCreateTransaction
                    {
                        AdminKey = testEnv.OperatorKey,
                        TopicMemo = "[e2e::TopicCreateTransaction]"

                    }.Batchify(testEnv.Client, key).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                });
                
                Assert.Contains(exception.Message, "Cannot execute batchified transaction outside of BatchTransaction");
            }
        }
    }
}