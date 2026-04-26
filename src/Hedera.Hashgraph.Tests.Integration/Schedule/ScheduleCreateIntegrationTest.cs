// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Threading;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class ScheduleCreateIntegrationTest
    {
        private readonly int oneDayInSecs = 86400;
        
        [Fact]
        public virtual void CanCreateSchedule()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(10)
				}.Schedule();
                var response = new ScheduleCreateTransaction
                {
					ScheduledTransactionBody = transaction.ScheduledTransactionBody,
					AdminKey = testEnv.OperatorKey,
					PayerAccountId = testEnv.OperatorId

				}.Execute(testEnv.Client);

                var scheduleId = response.GetReceipt(testEnv.Client).ScheduleId;
                var info = new ScheduleInfoQuery
                {
					ScheduleId = scheduleId

				}.Execute(testEnv.Client);

                Assert.NotNull(info.ExecutedAt);
            }
        }
        [Fact]
        public virtual void CanGetTransactionSchedule()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction
                {
                    Key = key,
                    InitialBalance = new Hbar(10)
                }.Schedule();
                var response = new ScheduleCreateTransaction
                {
                    ScheduledTransactionBody = transaction.ScheduledTransactionBody,
                    AdminKey = testEnv.OperatorKey,
                    PayerAccountId = testEnv.OperatorId
                }
                .Execute(testEnv.Client);

                var scheduleId = response.GetReceipt(testEnv.Client).ScheduleId;
                var info = new ScheduleInfoQuery
                {
					ScheduleId = scheduleId

				}.Execute(testEnv.Client);

                Assert.NotNull(info.ExecutedAt);
                Assert.NotNull(info.GetScheduledTransaction());
            }
        }
        [Fact]
        public virtual void CanCreateWithSchedule()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(10)
				};
                var tx = transaction.Schedule();
                tx.AdminKey = testEnv.OperatorKey;
				var response = tx.Execute(testEnv.Client);
                var scheduleId = response.GetReceipt(testEnv.Client).ScheduleId;
                var info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);
                Assert.NotNull(info.ExecutedAt);
                Assert.NotNull(info.GetScheduledTransaction());
            }
        }
        [Fact]
        public virtual void CanSignSchedule2()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                PrivateKey key2 = PrivateKey.GenerateED25519();
                PrivateKey key3 = PrivateKey.GenerateED25519();
                KeyList keyList = new ();
                keyList.Add(key1.GetPublicKey());
                keyList.Add(key2.GetPublicKey());
                keyList.Add(key3.GetPublicKey());

                // Creat the account with the `KeyList`
                TransactionResponse response = new AccountCreateTransaction
                {
					Key = keyList,
					InitialBalance = new Hbar(10),

				}.Execute(testEnv.Client);

                // This will wait for the receipt to become available
                TransactionReceipt receipt = response.GetReceipt(testEnv.Client);
                AccountId accountId = receipt.AccountId;

                // Generate a `TransactionId`. This id is used to query the inner scheduled transaction
                // after we expect it to have been executed
                TransactionId transactionId = TransactionId.Generate(testEnv.OperatorId);

                // Create a transfer transaction with 2/3 signatures.
                TransferTransaction transfer = new TransferTransaction
                {
					TransactionId = transactionId
				}
                    .AddHbarTransfer(accountId, new Hbar(1).Negated())
                    .AddHbarTransfer(testEnv.OperatorId, new Hbar(1));

                // Schedule the transaction
                ScheduleCreateTransaction scheduled = transfer.Schedule();
                receipt = scheduled
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // Get the schedule ID from the receipt
                ScheduleId scheduleId = receipt.ScheduleId;

                // Get the schedule info to see if `signatories` is populated with 2/3 signatures
                ScheduleInfo info = new ScheduleInfoQuery
                {
					ScheduleId = scheduleId

				}.Execute(testEnv.Client);
                Assert.Null(info.ExecutedAt);

                // Finally send this last signature to Hedera. This last signature _should_ mean the transaction executes
                // since all 3 signatures have been provided.
                new ScheduleSignTransaction
                {
					ScheduleId = scheduleId

				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Sign(key2)
                .Sign(key3)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                info = new ScheduleInfoQuery
                {
					ScheduleId = scheduleId

				}.Execute(testEnv.Client);

                Assert.NotNull(info.ExecutedAt);
                Assert.Null(scheduleId.Checksum);

				Assert.NotEqual(0, scheduleId.GetHashCode());
                Assert.Equal(0, scheduleId.CompareTo(ScheduleId.FromBytes(scheduleId.ToBytes())));

                new AccountDeleteTransaction
                {
					AccountId = accountId,
					TransferAccountId = testEnv.OperatorId,
				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Sign(key2)
                .Sign(key3)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }
        [Fact]
        public virtual void CanScheduleTokenTransfer()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
                    ReceiverSigRequired = true,
                    Key = key,
                    InitialBalance = new Hbar(10),
                }
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client)
                .AccountId;

                var tokenId = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    InitialSupply = 100,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId],
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var scheduleId = new TransferTransaction()
                    .AddTokenTransfer(tokenId, testEnv.OperatorId, -10)
                    .AddTokenTransfer(tokenId, accountId, 10)
                    .Schedule()
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client).ScheduleId;

                var balanceQuery1 = new AccountBalanceQuery
                {
					AccountId = accountId
				
                }.Execute(testEnv.Client);
                Assert.Equal(balanceQuery1.Tokens[tokenId], (ulong)0);
                new ScheduleSignTransaction
                {
					ScheduleId = scheduleId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var balanceQuery2 = new AccountBalanceQuery
                {
					AccountId = accountId
				
                }.Execute(testEnv.Client);
                Assert.Equal(balanceQuery2.Tokens[tokenId], (ulong)10);
            }
        }
        [Fact]
        public virtual void CannotScheduleTwoTransactions()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
                    InitialBalance = new Hbar(10),
					Key = key,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;
                var transferTx = new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, new Hbar(-10))
                    .AddHbarTransfer(accountId, new Hbar(10));
                var scheduleId1 = transferTx
                    .Schedule()
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client).ScheduleId;
                var info1 = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId1
                }
                .Execute(testEnv.Client);
                
                Assert.NotNull(info1.ExecutedAt);
                var transferTxFromInfo = info1.GetScheduledTransaction<TransferTransaction>();
                var scheduleCreateTx1 = transferTx.Schedule();
                var scheduleCreateTx2 = transferTxFromInfo.Schedule();

                Assert.Equal(scheduleCreateTx2.ToString(), scheduleCreateTx1.ToString());
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    transferTxFromInfo
                        .Schedule()
                        .Execute(testEnv.Client)
                        .GetReceipt(testEnv.Client);

                }); Assert.Contains("IDENTICAL_SCHEDULE_ALREADY_CREATED", exception.Message);
            }
        }
        [Fact]
        public virtual void CanScheduleTopicMessage()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                // Generate 3 random keys
                var key1 = PrivateKey.GenerateED25519();

                // This is the submit key
                var key2 = PrivateKey.GenerateED25519();
                var key3 = PrivateKey.GenerateED25519();
                var keyList = new KeyList();
                keyList.Add(key1.GetPublicKey());
                keyList.Add(key2.GetPublicKey());
                keyList.Add(key3.GetPublicKey());
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(100),
					Key = keyList,
				}
                .Execute(testEnv.Client);
                Assert.NotNull(response.GetReceipt(testEnv.Client).AccountId);
                var topicId = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					AutoRenewAccountId = testEnv.OperatorId,
					TopicMemo = "HCS Topic_",
					SubmitKey = key2.GetPublicKey(),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TopicId;
                var transaction = new TopicMessageSubmitTransaction
                {
                    TopicId = topicId,
                    Message = ByteString.CopyFromUtf8("scheduled hcs message")
                };

                // create schedule
                var scheduledTx = transaction.Schedule();
                scheduledTx.AdminKey = testEnv.OperatorKey;
                scheduledTx.PayerAccountId = testEnv.OperatorId;
				scheduledTx.ScheduleMemo = "mirror scheduled E2E signature on create and sign_" + DateTimeOffset.UtcNow;
                var scheduled = scheduledTx.FreezeWith(testEnv.Client);
                var scheduleId = scheduled
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ScheduleId;

                // verify schedule has been created and has 1 of 2 signatures
                var info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);
                Assert.NotNull(info);
                Assert.Equal(info.ScheduleId, scheduleId);
                
                var infoTransaction = (TopicMessageSubmitTransaction)info.GetScheduledTransaction();

                Assert.Equal(transaction.TopicId, infoTransaction.TopicId);
                Assert.Equal(transaction.NodeAccountIds, infoTransaction.NodeAccountIds);
                var scheduleSign = new ScheduleSignTransaction
                {
					ScheduleId = scheduleId

				}.FreezeWith(testEnv.Client);
                scheduleSign.Sign(key2)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
					ScheduleId = scheduleId

				}.Execute(testEnv.Client);
                Assert.NotNull(info.ExecutedAt);
            }
        }
        [Fact]
        public virtual void CanSignSchedule()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
					Key = key.GetPublicKey(),
					InitialBalance = new Hbar(10)
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction { }
                    .AddHbarTransfer(accountId, new Hbar(1).Negated())
                    .AddHbarTransfer(testEnv.OperatorId, new Hbar(1));

                // Schedule the transaction
                var scheduleId = transfer.Schedule(_ =>
                {
                    _.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(oneDayInSecs);
					_.ScheduleMemo = "HIP-423 Integration Test";
				})
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ScheduleId;
                ScheduleInfo info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is not yet executed
                Assert.Null(info.ExecutedAt);

                // Schedule sign
                new ScheduleSignTransaction
                {
					ScheduleId = scheduleId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is executed
                Assert.NotNull(info.ExecutedAt);
                Assert.Null(scheduleId.Checksum);
                Assert.NotEqual(0, scheduleId.GetHashCode());
                Assert.Equal(0, scheduleId.CompareTo(ScheduleId.FromBytes(scheduleId.ToBytes())));
            }
        }
        [Fact]
        public virtual void CannotScheduleTransactionOneYearIntoTheFuture()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
					Key = key.GetPublicKey(),
					InitialBalance = new Hbar(10),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.OperatorId, new Hbar(1));

                // Schedule the transaction
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    transfer.Schedule(_ =>
                    {
                        _.ExpirationTime = DateTimeOffset.UtcNow.AddDays(365);
                        _.ScheduleMemo = "HIP-423 Integration Test";
                    })
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                });
                
                Assert.Contains(exception.Message, ResponseStatus.ScheduleExpirationTimeTooFarInFuture.ToString());
            }
        }
        [Fact]
        public virtual void CannotScheduleTransactionInThePast()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
					Key = key.GetPublicKey(),
					InitialBalance = new Hbar(10),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.OperatorId, new Hbar(1));

                // Schedule the transaction
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    transfer.Schedule(_ =>
                    {
                        _.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(-10);
                        _.ScheduleMemo = "HIP-423 Integration Test";
                    })
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                });

				Assert.Contains(exception.Message, ResponseStatus.ScheduleExpirationTimeMustBeHigherThanConsensusTime.ToString());
            }
        }
        [Fact]
        public virtual void CanSignScheduleAndWaitForExpiry()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
					Key = key.GetPublicKey(),
					InitialBalance = new Hbar(10),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.OperatorId, new Hbar(1));

				// Schedule the transaction
				var scheduleId = transfer.Schedule(_ =>
				{
					_.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(oneDayInSecs);
					_.ScheduleMemo = "HIP-423 Integration Test";
				})
				.Execute(testEnv.Client)
				.GetReceipt(testEnv.Client).ScheduleId;

				ScheduleInfo info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is not yet executed
                Assert.Null(info.ExecutedAt);

                // Schedule sign
                new ScheduleSignTransaction
                {
					ScheduleId = scheduleId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
					ScheduleId = scheduleId
				}.Execute(testEnv.Client);

                // Verify the transaction is still not executed
                Assert.Null(info.ExecutedAt);
                Assert.Null(scheduleId.Checksum);
                Assert.NotEqual(0, scheduleId.GetHashCode());
                Assert.Equal(0, scheduleId.CompareTo(ScheduleId.FromBytes(scheduleId.ToBytes())));
            }
        }
        [Fact]
        public virtual void CanSignWithMultiSigAndUpdateSigningRequirements()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                PrivateKey key2 = PrivateKey.GenerateED25519();
                PrivateKey key3 = PrivateKey.GenerateED25519();
                PrivateKey key4 = PrivateKey.GenerateED25519();
                KeyList keyList = KeyList.Of(2, key1.GetPublicKey(), key2.GetPublicKey(), key3.GetPublicKey());
                var accountId = new AccountCreateTransaction
                {
					Key = keyList,
					InitialBalance = new Hbar(10),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.OperatorId, new Hbar(1));

                // Schedule the transaction
                var scheduleId = transfer.Schedule(_ =>
                {
                    _.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(oneDayInSecs);
                    _.ScheduleMemo = "HIP-423 Integration Test";
                })
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ScheduleId;
                ScheduleInfo info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is not executed
                Assert.Null(info.ExecutedAt);

                // Sign with one key
                new ScheduleSignTransaction
                {
					ScheduleId = scheduleId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is still not executed
                Assert.Null(info.ExecutedAt);

                // Update the signing requirements
                new AccountUpdateTransaction
                {
					AccountId = accountId,
					Key = key4.GetPublicKey(),
				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Sign(key2)
                .Sign(key4)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is still not executed
                Assert.Null(info.ExecutedAt);

                // Sign with the updated key
                new ScheduleSignTransaction
                {
					ScheduleId = scheduleId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key4)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is executed
                Assert.NotNull(info.ExecutedAt);
            }
        }
        [Fact]
        public virtual void CanSignWithMultiSig()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                PrivateKey key2 = PrivateKey.GenerateED25519();
                PrivateKey key3 = PrivateKey.GenerateED25519();
                KeyList keyList = new () { Threshold = 2 };
                keyList.Add(key1.GetPublicKey());
                keyList.Add(key2.GetPublicKey());
                keyList.Add(key3.GetPublicKey());
                var accountId = new AccountCreateTransaction
                {
					Key = keyList,
					InitialBalance = new Hbar(10)
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.OperatorId, new Hbar(1));

                // Schedule the transaction
                var schedule = transfer.Schedule();
				schedule.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(oneDayInSecs);
				schedule.ScheduleMemo = "HIP-423 Integration Test";
				var scheduleId = schedule.Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ScheduleId;

                ScheduleInfo info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is not executed
                Assert.Null(info.ExecutedAt);

                // Sign with one key
                new ScheduleSignTransaction
                {
					ScheduleId = scheduleId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is still not executed
                Assert.Null(info.ExecutedAt);

                // Update the signing requirements
                new AccountUpdateTransaction
                {
					AccountId = accountId,
					Key = key1.GetPublicKey(),
				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Sign(key2)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is still not executed
                Assert.Null(info.ExecutedAt);

                // Sign with one more key
                new ScheduleSignTransaction
                {
					ScheduleId = scheduleId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key2)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId
                }
                .Execute(testEnv.Client);

                // Verify the transaction is executed
                Assert.NotNull(info.ExecutedAt);
            }
        }
        [Fact]
        public virtual void CanExecuteWithShortExpirationTime()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
					Key = key1,
					InitialBalance = new Hbar(10),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.OperatorId, new Hbar(1));

                // Schedule the transaction
                var scheduleId = transfer.Schedule();
				scheduleId.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(10);
                scheduleId.WaitForExpiry = true;
                scheduleId.ScheduleMemo = "HIP-423 Integration Test";
                ScheduleInfo info = new ScheduleInfoQuery
                {
                    ScheduleId = scheduleId.Execute(testEnv.Client).GetReceipt(testEnv.Client).ScheduleId
				}
                .Execute(testEnv.Client);

                // Verify the transaction is not executed
                Assert.Null(info.ExecutedAt);

                // Sign
                new ScheduleSignTransaction
                {
					ScheduleId = info.ScheduleId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                info = new ScheduleInfoQuery
                {
                    ScheduleId = info.ScheduleId
				}
                .Execute(testEnv.Client);

                // Verify the transaction is still not executed
                Assert.Null(info.ExecutedAt);
                var accountBalanceBefore = new AccountBalanceQuery
                {
					AccountId = accountId
				}.Execute(testEnv.Client);
                Thread.Sleep(10000);
                var accountBalanceAfter = new AccountBalanceQuery
                { 
                    AccountId = accountId

                }.Execute(testEnv.Client);

                // Verify the transaction executed after 10 seconds
                Assert.Equal(accountBalanceBefore.Hbars.CompareTo(accountBalanceAfter.Hbars), 1);
            }
        }
    }
}