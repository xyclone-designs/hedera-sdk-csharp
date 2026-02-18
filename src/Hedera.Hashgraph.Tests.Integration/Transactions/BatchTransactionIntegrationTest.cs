// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Assertj.Core.Api.AssertionsForClassTypes;
using Com.Hedera.Hashgraph;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class BatchTransactionIntegrationTest
    {
        public virtual void CanCreateBatchTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateECDSA();
                var tx = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key)
                    .SetInitialBalance(new Hbar(1)).Batchify(testEnv.Client, testEnv.OperatorKey);
                var batchTransaction = new BatchTransaction().AddInnerTransaction(tx);
                batchTransaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountIdInnerTransaction = batchTransaction.GetInnerTransactionIds()[0].accountId;
                var execute = new AccountInfoQuery()
                    .SetAccountId(accountIdInnerTransaction).Execute(testEnv.Client);
                Assert.Equal(accountIdInnerTransaction, execute.accountId);
            }
        }

        public virtual void CanExecuteFromToBytes()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateECDSA();
                var tx = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key)
                    .SetInitialBalance(new Hbar(1)).Batchify(testEnv.Client, testEnv.OperatorKey);
                var batchTransaction = new BatchTransaction().AddInnerTransaction(tx);
                var batchTransactionBytes = batchTransaction.ToBytes();
                var batchTransactionFromBytes = BatchTransaction.FromBytes(batchTransactionBytes);
                batchTransactionFromBytes.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountIdInnerTransaction = batchTransaction.GetInnerTransactionIds()[0].accountId;
                var execute = new AccountInfoQuery()
                    .SetAccountId(accountIdInnerTransaction).Execute(testEnv.Client);
                Assert.Equal(accountIdInnerTransaction, execute.accountId);
            }
        }

        public virtual void CanExecuteLargeBatchTransactionUpToMaximumRequestSize()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                BatchTransaction batchTransaction = new BatchTransaction();

                // 50 is the maximum limit for internal transaction inside a BatchTransaction
                for (int i = 0; i < 25; i++)
                {
                    var key = PrivateKey.GenerateECDSA();
                    var tx = new AccountCreateTransaction()
                        .SetKeyWithoutAlias(key)
                        .SetInitialBalance(new Hbar(1)).Batchify(testEnv.Client, testEnv.OperatorKey);
                    batchTransaction.AddInnerTransaction(tx);
                }

                batchTransaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                foreach (var innerTransactionID in batchTransaction.GetInnerTransactionIds())
                {
                    var receipt = new TransactionReceiptQuery()
                        .SetTransactionId(innerTransactionID).Execute(testEnv.Client);
                    Assert.Equal(receipt.status, ResponseStatus.Success);
                }
            }
        }

        public virtual void BatchTransactionWithoutInnerTransactionsShouldThrowAnError()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                Assert.Throws(typeof(PrecheckStatusException), () => new BatchTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client)).WithMessageContaining(Status.BATCH_LIST_EMPTY.ToString());
            }
        }

        public virtual void BatchTransactionWithBlacklistedInnerTransactionShouldThrowAnError()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var freezeTransaction = new FreezeTransaction()
                    .SetFileId(FileId.FromString("4.5.6"))
                    .SetFileHash(Hex.Decode("1723904587120938954702349857"))
                    .SetStartTime(DateTimeOffset.UtcNow)
                    .SetFreezeType(FreezeType.FREEZE_ONLY).Batchify(testEnv.Client, testEnv.OperatorKey);
                Assert.Throws<ArgumentException>(() => new BatchTransaction().AddInnerTransaction(freezeTransaction)).WithMessageContaining("Transaction type FreezeTransaction is not allowed in a batch transaction");
                var key = PrivateKey.GenerateECDSA();
                var tx = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key)
                    .SetInitialBalance(new Hbar(1)).Batchify(testEnv.Client, testEnv.OperatorKey);
                var batchTransaction = new BatchTransaction().AddInnerTransaction(tx).Batchify(testEnv.Client, testEnv.OperatorKey);
                Assert.Throws<ArgumentException>(() => new BatchTransaction().AddInnerTransaction(batchTransaction)).WithMessageContaining("Transaction type BatchTransaction is not allowed in a batch transaction");
            }
        }

        public virtual void BatchTransactionWithInvalidBatchKeyInsideInnerTransactionShouldThrowAnError()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                BatchTransaction batchTransaction = new BatchTransaction();
                var key = PrivateKey.GenerateECDSA();
                var invalidKey = PrivateKey.GenerateECDSA();
                var tx = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key)
                    .SetInitialBalance(new Hbar(1)).Batchify(testEnv.Client, invalidKey.GetPublicKey());
                Assert.Throws(typeof(ReceiptStatusException), () => batchTransaction.AddInnerTransaction(tx).Execute(testEnv.Client).GetReceipt(testEnv.Client)).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void ChunkedInnerTransactionsShouldBeExecutedSuccessfully()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction()
                    AdminKey = testEnv.OperatorKey,
                    .SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					MaxChunks = 15,
					Message = Contents.BIG_CONTENTS,
				}
                .Batchify(testEnv.Client, testEnv.OperatorKey);
                new BatchTransaction()
                    .AddInnerTransaction(topicMessageSubmitTransaction)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                var info = new TopicInfoQuery
                {
					TopicId = topicId
				
                }.Execute(testEnv.Client);
                Assert.Equal(info.SequenceNumber, 1);
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
                var account1 = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key1)
                    .SetInitialBalance(new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(account1);
                var batchedTransfer1 = new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(100))
                    .AddHbarTransfer(account1, Hbar.FromTinybars(100).Negated())
                    .SetTransactionId(TransactionId.Generate(account1))
                    .SetBatchKey(batchKey1).FreezeWith(testEnv.Client).Sign(key1);
                var key2 = PrivateKey.GenerateECDSA();
                var account2 = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key2)
                    .SetInitialBalance(new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(account2);
                var batchedTransfer2 = new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(100))
                    .AddHbarTransfer(account2, Hbar.FromTinybars(100).Negated())
                    .SetTransactionId(TransactionId.Generate(account2))
                    .SetBatchKey(batchKey2).FreezeWith(testEnv.Client).Sign(key2);
                var key3 = PrivateKey.GenerateECDSA();
                var account3 = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key3)
                    .SetInitialBalance(new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(account3);
                var batchedTransfer3 = new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(100))
                    .AddHbarTransfer(account3, Hbar.FromTinybars(100).Negated())
                    .SetTransactionId(TransactionId.Generate(account3))
                    .SetBatchKey(batchKey3).FreezeWith(testEnv.Client).Sign(key3);
                var receipt = new BatchTransaction()
                    .AddInnerTransaction(batchedTransfer1)
                    .AddInnerTransaction(batchedTransfer2)
                    .AddInnerTransaction(batchedTransfer3).FreezeWith(testEnv.Client).Sign(batchKey1).Sign(batchKey2).Sign(batchKey3).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }

        public virtual void SuccessfulInnerTransactionsShouldIncurFeesEvenThoughOneFailed()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialBalance = new AccountInfoQuery()
                    .SetAccountId(testEnv.OperatorId).Execute(testEnv.Client).balance;
                var key1 = PrivateKey.GenerateECDSA();
                var tx1 = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key1)
                    .SetInitialBalance(new Hbar(1)).Batchify(testEnv.Client, testEnv.OperatorKey);
                var key2 = PrivateKey.GenerateECDSA();
                var tx2 = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key2)
                    .SetInitialBalance(new Hbar(1)).Batchify(testEnv.Client, testEnv.OperatorKey);
                var key3 = PrivateKey.GenerateECDSA();
                var tx3 = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key3)
                    .SetReceiverSignatureRequired(true)
                    .SetInitialBalance(new Hbar(1)).Batchify(testEnv.Client, testEnv.OperatorKey);
                Assert.Throws(typeof(ReceiptStatusException), () => new BatchTransaction().AddInnerTransaction(tx1).AddInnerTransaction(tx2).AddInnerTransaction(tx3).Execute(testEnv.Client).GetReceipt(testEnv.Client)).WithMessageContaining(Status.INNER_TRANSACTION_FAILED.ToString());
                var finalBalance = new AccountInfoQuery()
                    .SetAccountId(testEnv.OperatorId).Execute(testEnv.Client).balance;
                AssertThat(finalBalance.GetValue().IntValue()).IsLessThan(initialBalance.GetValue().IntValue());
            }
        }

        public virtual void TransactionShouldFailWhenBatchified()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                Assert.Throws<ArgumentException>(() => new TopicCreateTransaction()
                AdminKey = testEnv.OperatorKey,
                .SetTopicMemo("[e2e::TopicCreateTransaction]").Batchify(testEnv.Client, key).Execute(testEnv.Client).GetReceipt(testEnv.Client)).WithMessageContaining("Cannot execute batchified transaction outside of BatchTransaction");
            }
        }
    }
}