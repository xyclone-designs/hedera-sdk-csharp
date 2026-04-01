// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TransactionIntegrationTest
    {
        public virtual async void TransactionHashInTransactionRecordIsEqualToTheDerivedTransactionHash()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction { Key = key, }.FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                var expectedHash = transaction.GetTransactionHashPerNode();
                var response = transaction.Execute(testEnv.Client);
                var record = response.GetRecord(testEnv.Client);
                Assert.Equal(expectedHash[response.NodeId], record.TransactionHash.ToByteArray());
                var accountId = record.Receipt.AccountId;
                Assert.NotNull(accountId);
                var transactionId = transaction.TransactionId;
                Assert.NotNull(transactionId.GetReceipt(testEnv.Client));
                Assert.NotNull(await transactionId.GetReceiptAsync(testEnv.Client));
                Assert.NotNull(transactionId.GetRecord(testEnv.Client));
                Assert.NotNull(await transactionId.GetRecordAsync(testEnv.Client));
            }
        }

        public virtual void CanSerializeDeserializeCompareFields()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var accountCreateTransaction = new AccountCreateTransaction()
                {
					Key = publicKey,
					InitialBalance = new Hbar(1),
				};
                var expectedNodeAccountIds = accountCreateTransaction.NodeAccountIds.Read;
                var expectedBalance = new Hbar(1);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                
                AccountCreateTransaction accountCreateTransactionDeserialized = Transaction.FromBytes<AccountCreateTransaction>(transactionBytesSerialized);
                
                Assert.Equal(expectedNodeAccountIds, accountCreateTransactionDeserialized.NodeAccountIds.Read);
                Assert.Equal(expectedBalance, accountCreateTransactionDeserialized.InitialBalance);
                Assert.Throws<InvalidOperationException>(() => _ = accountCreateTransactionDeserialized.TransactionId);
            }
        }

        public virtual void CanSerializeWithNodeAccountIdsDeserializeCompareFields()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var accountCreateTransaction = new AccountCreateTransaction
                {
					NodeAccountIds = [.. testEnv.Client.Network_.Network_Read.Keys],
					Key = publicKey,
					InitialBalance = new Hbar(1),
				};
                var expectedNodeAccountIds = accountCreateTransaction.NodeAccountIds.Read;
                var expectedBalance = new Hbar(1);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                
                AccountCreateTransaction accountCreateTransactionDeserialized = Transaction.FromBytes<AccountCreateTransaction>(transactionBytesSerialized);
                
                Assert.Equal(expectedNodeAccountIds.Count, accountCreateTransactionDeserialized.NodeAccountIds.Read.Count);
                Assert.Equal(expectedBalance, accountCreateTransactionDeserialized.InitialBalance);
                Assert.Throws<InvalidOperationException>(() => _ = accountCreateTransactionDeserialized.TransactionId);
            }
        }

        public virtual void CanSerializeDeserializeAndExecuteIncompleteTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var accountCreateTransaction = new AccountCreateTransaction
                {
					Key = publicKey,
					InitialBalance = new Hbar(1),
				};
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = Transaction.FromBytes<AccountCreateTransaction>(transactionBytesSerialized);
                
                var txReceipt = accountCreateTransactionDeserialized.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                new AccountDeleteTransaction
                {
					AccountId = txReceipt.AccountId,
					TransferAccountId = testEnv.Client.OperatorAccountId,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client);
            }
        }

        public virtual void CanSerializeDeserializeAndExecuteIncompleteTransactionWithNodeAccountIds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();

                var accountCreateTransaction = new AccountCreateTransaction
                {
					NodeAccountIds = [.. testEnv.Client.Network_.Network_Read.Keys],
					Key = publicKey,
					InitialBalance = new Hbar(1),
				};
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = Transaction.FromBytes<AccountCreateTransaction>(transactionBytesSerialized);
                
                var txReceipt = accountCreateTransactionDeserialized.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                new AccountDeleteTransaction
                {
					AccountId = txReceipt.AccountId,
					TransferAccountId = testEnv.Client.OperatorAccountId,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client);
            }
        }

        public virtual void CanSerializeDeserializeEditExecuteCompareFields()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var accountCreateTransaction = new AccountCreateTransaction { Key = publicKey, };
                var expectedBalance = new Hbar(1);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();

                AccountCreateTransaction accountCreateTransactionDeserialized = Transaction.FromBytes<AccountCreateTransaction>(transactionBytesSerialized);
				accountCreateTransactionDeserialized.InitialBalance = new Hbar(1);
				accountCreateTransactionDeserialized.TransactionId = TransactionId.Generate(testEnv.Client.OperatorAccountId);

				var txReceipt = accountCreateTransactionDeserialized.Execute(testEnv.Client).GetReceipt(testEnv.Client);

				Assert.Equal(expectedBalance, accountCreateTransactionDeserialized.InitialBalance);

                new AccountDeleteTransaction
                {
					AccountId = txReceipt.AccountId,
					TransferAccountId = testEnv.Client.OperatorAccountId,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client);
            }
        }

        public virtual void CanSerializeDeserializeEditExecuteCompareFieldsIncompleteTransactionWithNodeAccountIds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var accountCreateTransaction = new AccountCreateTransaction
                {
					NodeAccountIds = [.. testEnv.Client.Network_.Network_Read.Keys],
					Key = publicKey,
				};
                var expectedBalance = new Hbar(1);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                
                AccountCreateTransaction accountCreateTransactionDeserialized = Transaction.FromBytes<AccountCreateTransaction>(transactionBytesSerialized);
                accountCreateTransactionDeserialized.InitialBalance = new Hbar(1);
                accountCreateTransactionDeserialized.TransactionId = TransactionId.Generate(testEnv.Client.OperatorAccountId);
                
                var txReceipt = accountCreateTransactionDeserialized.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                Assert.Equal(expectedBalance, accountCreateTransactionDeserialized.InitialBalance);
                
                new AccountDeleteTransaction
                {
					AccountId = txReceipt.AccountId,
					TransferAccountId = testEnv.Client.OperatorAccountId,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client);
            }
        }

        public virtual void CanFreezeSignSerializeDeserializeReserializeAndExecute()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var initialBalance = new Hbar(1);
                var autoRenewPeriod = TimeSpan.FromSeconds(2592000);
                var memo = "test account memo";
                var maxAutomaticTokenAssociations = 4;
                var accountCreateTransaction = new AccountCreateTransaction
                {
					Key = publicKey,InitialBalance = initialBalance, 
                    ReceiverSigRequired = true,
					AutoRenewPeriod = autoRenewPeriod,
					AccountMemo = memo,
					MaxAutomaticTokenAssociations = maxAutomaticTokenAssociations,
					DeclineStakingReward = true,
					Alias = evmAddress
				
                }.FreezeWith(testEnv.Client).Sign(adminKey);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                
                AccountCreateTransaction accountCreateTransactionDeserialized = Transaction.FromBytes<AccountCreateTransaction>(transactionBytesSerialized);
                var transactionBytesReserialized = accountCreateTransactionDeserialized.ToBytes();
                Assert.Equal(transactionBytesSerialized, transactionBytesReserialized);
                
                AccountCreateTransaction accountCreateTransactionReserialized = Transaction.FromBytes<AccountCreateTransaction>(transactionBytesReserialized);
                
                var txResponse = accountCreateTransactionReserialized.Execute(testEnv.Client);
                var accountId = txResponse.GetReceipt(testEnv.Client).AccountId;
                
                new AccountDeleteTransaction
                {
					AccountId = accountId,
					TransferAccountId = testEnv.Client.OperatorAccountId,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client);
            }
        }

        public virtual void CanFreezeSerializeDeserializeAddSignatureAndExecute()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction
                {
					Key = key,
				
                }.FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);

                var expectedHash = transaction.GetTransactionHashPerNode();
                var response = transaction.Execute(testEnv.Client);
                var record = response.GetRecord(testEnv.Client);
                Assert.Equal(expectedHash[response.NodeId], record.TransactionHash.ToByteArray());
                var accountId = record.Receipt.AccountId;
                
                Assert.NotNull(accountId);
                
                var deleteTransaction = new AccountDeleteTransaction
                {
					AccountId = accountId,
					TransferAccountId = testEnv.OperatorId
				
                }.FreezeWith(testEnv.Client);
                var updateBytes = deleteTransaction.ToBytes();
                var sig1 = key.SignTransaction(deleteTransaction);
                var deleteTransaction2 = Transaction.FromBytes<AccountDeleteTransaction>(updateBytes);
                deleteTransaction2.AddSignature(key.GetPublicKey(), sig1).Execute(testEnv.Client);
            }
        }

        public virtual void CanFreezeSignSerializeDeserializeAndCompareFileAppendChunkedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateED25519();
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")
				
                }.Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId;
                
                Thread.Sleep(5000);

                var info = new FileInfoQuery { FileId = fileId, }.Execute(testEnv.Client);
                
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 28);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);

                var fileAppendTransaction = new FileAppendTransaction
                {
					FileId = fileId,
					Contents = ByteString.CopyFromUtf8(Contents.BIG_CONTENTS)

				}.FreezeWith(testEnv.Client).Sign(privateKey);
                var transactionBytesSerialized = fileAppendTransaction.ToBytes();
                
                FileAppendTransaction fileAppendTransactionDeserialized = Transaction.FromBytes<FileAppendTransaction>(transactionBytesSerialized);
                
                var transactionBytesReserialized = fileAppendTransactionDeserialized.ToBytes();
                
                Assert.Equal(transactionBytesSerialized, transactionBytesReserialized);
            }
        }

        public virtual void CanSerializeDeserializeExecuteFileAppendChunkedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")
				
                }.Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId;
                
                Thread.Sleep(5000);

                var info = new FileInfoQuery { FileId = fileId, }.Execute(testEnv.Client);
                
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 28);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);

                var fileAppendTransaction = new FileAppendTransaction
                {
					FileId = fileId,
                    Contents = ByteString.CopyFromUtf8(Contents.BIG_CONTENTS)
				};
                var transactionBytesSerialized = fileAppendTransaction.ToBytes();
                
                FileAppendTransaction fileAppendTransactionDeserialized = Transaction.FromBytes<FileAppendTransaction>(transactionBytesSerialized);
                fileAppendTransactionDeserialized.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var contents = new FileContentsQuery { FileId = fileId, }.Execute(testEnv.Client);
                
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]" + Contents.BIG_CONTENTS);

                info = new FileInfoQuery { FileId = fileId, }.Execute(testEnv.Client);
                
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 13522);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);

                new FileDeleteTransaction { FileId = fileId, }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanSerializeDeserializeExecuteIncompleteFileAppendChunkedTransactionWithNodeAccountIds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction
                {
                    Keys = [testEnv.OperatorKey],
                    Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")

                }.Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId;

                Thread.Sleep(5000);

                var info = new FileInfoQuery { FileId = fileId, }.Execute(testEnv.Client);
                
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 28);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);
                
                var fileAppendTransaction = new FileAppendTransaction
                {
                    NodeAccountIds = [.. testEnv.Client.Network_.Network_Read.Keys],
                    FileId = fileId,
                    Contents = ByteString.CopyFromUtf8(Contents.BIG_CONTENTS)
                };
                var transactionBytesSerialized = fileAppendTransaction.ToBytes();
                
                FileAppendTransaction fileAppendTransactionDeserialized = Transaction.FromBytes<FileAppendTransaction>(transactionBytesSerialized);
                fileAppendTransactionDeserialized.TransactionId = TransactionId.Generate(testEnv.Client.OperatorAccountId);
                fileAppendTransactionDeserialized.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var contents = new FileContentsQuery { FileId = fileId, }.Execute(testEnv.Client);
                
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]" + Contents.BIG_CONTENTS);
                
                info = new FileInfoQuery { FileId = fileId }.Execute(testEnv.Client);
                
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 13522);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);
                
                new FileDeleteTransaction { FileId = fileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanFreezeSignSerializeDeserializeAndCompareTopicMessageSubmitChunkedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateED25519();
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					TopicMemo = "[e2e::TopicCreateTransaction]",
				
                }.Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                
                Thread.Sleep(5000);

                var info = new TopicInfoQuery { TopicId = topicId }.Execute(testEnv.Client);

                Assert.Equal(info.TopicId, topicId);
                Assert.Equal("[e2e::TopicCreateTransaction]", info.TopicMemo);
                Assert.Equal((ulong)0, info.SequenceNumber);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					MaxChunks = 15,
					Message = ByteString.CopyFromUtf8(Contents.BIG_CONTENTS)

				}.FreezeWith(testEnv.Client).Sign(privateKey);
                var transactionBytesSerialized = topicMessageSubmitTransaction.ToBytes();
                TopicMessageSubmitTransaction fileAppendTransactionDeserialized = Transaction.FromBytes<TopicMessageSubmitTransaction>(transactionBytesSerialized);
                var transactionBytesReserialized = fileAppendTransactionDeserialized.ToBytes();
                Assert.Equal(transactionBytesSerialized, transactionBytesReserialized);
                
                new TopicDeleteTransaction { TopicId = topicId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanSerializeDeserializeExecuteIncompleteTopicMessageSubmitChunkedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					TopicMemo = "[e2e::TopicCreateTransaction]",
				
                }.Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                
                Thread.Sleep(5000);

                var info = new TopicInfoQuery { TopicId = topicId }.Execute(testEnv.Client);

                Assert.Equal(info.TopicId, topicId);
                Assert.Equal("[e2e::TopicCreateTransaction]", info.TopicMemo);
                Assert.Equal((ulong)0, info.SequenceNumber);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                
                var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction
                {
					TopicId = topicId,
					MaxChunks = 15,
					Message = ByteString.CopyFromUtf8(Contents.BIG_CONTENTS)
				};
                var transactionBytesSerialized = topicMessageSubmitTransaction.ToBytes();
                
                TopicMessageSubmitTransaction topicMessageSubmitTransactionDeserialized = Transaction.FromBytes<TopicMessageSubmitTransaction>(transactionBytesSerialized);
                
                var responses = topicMessageSubmitTransactionDeserialized.ExecuteAll(testEnv.Client);
                foreach (var resp in responses)
                {
                    resp.GetReceipt(testEnv.Client);
                }

                info = new TopicInfoQuery { TopicId = topicId }.Execute(testEnv.Client);

                Assert.Equal(info.TopicId, topicId);
                Assert.Equal("[e2e::TopicCreateTransaction]", info.TopicMemo);
                Assert.Equal((ulong)14, info.SequenceNumber);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                
                new TopicDeleteTransaction { TopicId = topicId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanSerializeDeserializeExecuteIncompleteTopicMessageSubmitChunkedTransactionWithNodeAccountIds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
					AdminKey = testEnv.OperatorKey,
					TopicMemo = "[e2e::TopicCreateTransaction]",
				
                }.Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                
                Thread.Sleep(5000);

                var info = new TopicInfoQuery { TopicId = topicId }.Execute(testEnv.Client);

                Assert.Equal(info.TopicId, topicId);
                Assert.Equal("[e2e::TopicCreateTransaction]", info.TopicMemo);
                Assert.Equal((ulong)0, info.SequenceNumber);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction
                {
                    TopicId = topicId,
                    MaxChunks = 15,
					NodeAccountIds = [.. testEnv.Client.Network_.Network_Read.Keys],
					Message = ByteString.CopyFromUtf8(Contents.BIG_CONTENTS)
				};
                var transactionBytesSerialized = topicMessageSubmitTransaction.ToBytes();
                TopicMessageSubmitTransaction topicMessageSubmitTransactionDeserialized = Transaction.FromBytes<TopicMessageSubmitTransaction>(transactionBytesSerialized);
                var responses = topicMessageSubmitTransactionDeserialized.ExecuteAll(testEnv.Client);
                foreach (var resp in responses)
                {
                    resp.GetReceipt(testEnv.Client);
                }

                info = new TopicInfoQuery { TopicId = topicId }.Execute(testEnv.Client);

                Assert.Equal(info.TopicId, topicId);
                Assert.Equal("[e2e::TopicCreateTransaction]", info.TopicMemo);
                Assert.Equal((ulong)14, info.SequenceNumber);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                
                new TopicDeleteTransaction { TopicId = topicId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        // TODO: this test has a bunch of things hard-coded into it, which is kinda
        // dumb, but it's a good idea for a test.
        // Any way to fix it and bring it back?
        public virtual void TransactionFromToBytes2()
        {
			var id = TransactionId.Generate(new AccountId(0, 0, 542348));
			var transactionBodyBuilder = new Proto.TransactionBody
            {
				GenerateRecord = false,
				Memo = "",
				TransactionID = new Proto.TransactionID
                {
                    AccountID = new Proto.AccountID
                    {
						AccountNum = 542348,
						RealmNum = 0,
						ShardNum = 0
					},
                    TransactionValidStart = new Proto.Timestamp
                    {
						Nanos = id.ValidStart.Nanosecond,
						Seconds = id.ValidStart.ToUnixTimeSeconds(),
					},
				},
				TransactionFee = 200000000,
				TransactionValidDuration = new Proto.Duration { Seconds = 120 },
                CryptoTransfer = new Proto.CryptoTransferTransactionBody
                {
                    Transfers = new Proto.TransferList(),
				},
			};
            transactionBodyBuilder.CryptoTransfer.Transfers.AccountAmounts.Add(new Proto.AccountAmount { Amount = 10, AccountID = new Proto.AccountID { AccountNum = 47439, RealmNum = 0, ShardNum = 0 } });
            transactionBodyBuilder.CryptoTransfer.Transfers.AccountAmounts.Add(new Proto.AccountAmount { Amount = -10, AccountID = new Proto.AccountID { AccountNum = 542348, RealmNum = 0, ShardNum = 0 } });

			var bodyBytes = transactionBodyBuilder.ToByteString();
			var key1 = PrivateKey.FromString("302e020100300506032b6570042204203e7fda6dde63c3cdb3cb5ecf5264324c5faad7c9847b6db093c088838b35a110");
			var key2 = PrivateKey.FromString("302e020100300506032b65700422042032d3d5a32e9d06776976b39c09a31fbda4a4a0208223da761c26a2ae560c1755");
			var key3 = PrivateKey.FromString("302e020100300506032b657004220420195a919056d1d698f632c228dbf248bbbc3955adf8a80347032076832b8299f9");
			var key4 = PrivateKey.FromString("302e020100300506032b657004220420b9962f17f94ffce73a23649718a11638cac4b47095a7a6520e88c7563865be62");
			var key5 = PrivateKey.FromString("302e020100300506032b657004220420fef68591819080cd9d48b0cbaa10f65f919752abb50ffb3e7411ac66ab22692e");
			var publicKey1 = key1.GetPublicKey();
			var publicKey2 = key2.GetPublicKey();
			var publicKey3 = key3.GetPublicKey();
			var publicKey4 = key4.GetPublicKey();
			var publicKey5 = key5.GetPublicKey();
			var signature1 = key1.Sign(bodyBytes.ToByteArray());
			var signature2 = key2.Sign(bodyBytes.ToByteArray());
			var signature3 = key3.Sign(bodyBytes.ToByteArray());
			var signature4 = key4.Sign(bodyBytes.ToByteArray());
			var signature5 = key5.Sign(bodyBytes.ToByteArray());
			var signedBuilder = new Proto.SignedTransaction()
            {
                BodyBytes = bodyBytes,
                SigMap = new Proto.SignatureMap { }
            };
            signedBuilder.SigMap.SigPair.Add(new Proto.SignaturePair { Ed25519 = ByteString.CopyFrom(signature1), PubKeyPrefix = ByteString.CopyFrom(publicKey1.ToBytes()) });
            signedBuilder.SigMap.SigPair.Add(new Proto.SignaturePair { Ed25519 = ByteString.CopyFrom(signature2), PubKeyPrefix = ByteString.CopyFrom(publicKey2.ToBytes()) });
            signedBuilder.SigMap.SigPair.Add(new Proto.SignaturePair { Ed25519 = ByteString.CopyFrom(signature3), PubKeyPrefix = ByteString.CopyFrom(publicKey3.ToBytes()) });
            signedBuilder.SigMap.SigPair.Add(new Proto.SignaturePair { Ed25519 = ByteString.CopyFrom(signature4), PubKeyPrefix = ByteString.CopyFrom(publicKey4.ToBytes()) });
            signedBuilder.SigMap.SigPair.Add(new Proto.SignaturePair { Ed25519 = ByteString.CopyFrom(signature5), PubKeyPrefix = ByteString.CopyFrom(publicKey5.ToBytes()) });

			var byts = signedBuilder.ToByteString();
            var list = new Proto.TransactionList(); list.TransactionList_.Add(new Proto.Transaction { SignedTransactionBytes = byts });
			byts = list.ToByteString();
			var tx = Transaction.FromBytes<TransferTransaction>(byts.ToByteArray());

			using (var testEnv = new IntegrationTestEnv(1))
			{
				Assert.Equal(tx.GetHbarTransfers()[new AccountId(0, 0, 542348)].ToTinybars(), -10);
				Assert.Equal(tx.GetHbarTransfers()[new AccountId(0, 0, 47439)].ToTinybars(), 10);

				Assert.NotNull(tx.NodeAccountIds.Read);
				Assert.Equal(tx.NodeAccountIds.Read.Count, 1);
				Assert.Equal(tx.NodeAccountIds.Read[0], new AccountId(0, 0, 3));
				
                Dictionary<AccountId, Dictionary<PublicKey, byte[]>> signatures = tx.GetSignatures();

				Assert.Equal(signatures[new AccountId(0, 0, 3)][publicKey1], signature1);
				Assert.Equal(signatures[new AccountId(0, 0, 3)][publicKey2], signature2);
				Assert.Equal(signatures[new AccountId(0, 0, 3)][publicKey3], signature3);
				Assert.Equal(signatures[new AccountId(0, 0, 3)][publicKey4], signature4);
				Assert.Equal(signatures[new AccountId(0, 0, 3)][publicKey5], signature5);
				
                var resp = tx.Execute(testEnv.Client);
				resp.GetReceipt(testEnv.Client);
			}
		}

        public virtual void CanAddSignatureToTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Step 1: Create a new key for the account
                var newKey = PrivateKey.GenerateED25519();

                // Step 2: Create account with the new key
                var createResponse = new AccountCreateTransaction
                {
					Key = newKey.GetPublicKey(),
					NodeAccountIds = [.. testEnv.Client.Network_.Network_Read.Keys]
				
                }.Execute(testEnv.Client);

                var createReceipt = createResponse.GetReceipt(testEnv.Client);
                var accountId = createReceipt.AccountId;
                var nodeId = createResponse.NodeId;

                // Step 3: Create account delete transaction and freeze it
                var deleteTransaction = new AccountDeleteTransaction
                {
					NodeAccountIds = [nodeId],
					AccountId = accountId,
					TransferAccountId = testEnv.Client.OperatorAccountId,
				
                }.FreezeWith(testEnv.Client);

                // Step 4: Get signable body bytes list
                var signableBodyList = deleteTransaction.GetSignableNodeBodyBytesList();
                Assert.NotEmpty(signableBodyList);

                // Step 5: Sign each signable body externally and add signatures back
                foreach (var signableBody in signableBodyList)
                {
                    byte[] signature = newKey.Sign(signableBody.Body);
                    deleteTransaction = deleteTransaction.AddSignature(newKey.GetPublicKey(), signature, signableBody.TransactionID, signableBody.NodeID);
                }

                var deleteResponse = deleteTransaction.Execute(testEnv.Client);
                var deleteReceipt = deleteResponse.GetReceipt(testEnv.Client);

                Assert.Equal(ResponseStatus.Success, deleteReceipt.Status);
            }
        }
    }
}