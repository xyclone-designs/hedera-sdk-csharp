// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TransactionIntegrationTest
    {
        virtual void TransactionHashInTransactionRecordIsEqualToTheDerivedTransactionHash()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction().SetKeyWithoutAlias(key).FreezeWith(testEnv.client).SignWithOperator(testEnv.client);
                var expectedHash = transaction.GetTransactionHashPerNode();
                var response = transaction.Execute(testEnv.client);
                var record = response.GetRecord(testEnv.client);
                AssertThat(expectedHash[response.nodeId]).ContainsExactly(record.transactionHash.ToByteArray());
                var accountId = record.receipt.accountId;
                AssertThat(accountId).IsNotNull();
                var transactionId = transaction.GetTransactionId();
                AssertThat(transactionId.GetReceipt(testEnv.client)).IsNotNull();
                AssertThat(transactionId.GetReceiptAsync(testEnv.client).Get()).IsNotNull();
                AssertThat(transactionId.GetRecord(testEnv.client)).IsNotNull();
                AssertThat(transactionId.GetRecordAsync(testEnv.client).Get()).IsNotNull();
            }
        }

        virtual void CanSerializeDeserializeCompareFields()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var accountCreateTransaction = new AccountCreateTransaction().SetKeyWithoutAlias(publicKey).SetInitialBalance(new Hbar(1));
                var expectedNodeAccountIds = accountCreateTransaction.GetNodeAccountIds();
                var expectedBalance = new Hbar(1);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = (AccountCreateTransaction)Transaction.FromBytes(transactionBytesSerialized);
                Assert.Equal(expectedNodeAccountIds, accountCreateTransactionDeserialized.GetNodeAccountIds());
                Assert.Equal(expectedBalance, accountCreateTransactionDeserialized.GetInitialBalance());
                AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(accountCreateTransactionDeserialized.GetTransactionId());
            }
        }

        virtual void CanSerializeWithNodeAccountIdsDeserializeCompareFields()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var nodeAccountIds = testEnv.client.GetNetwork().Values().Stream().ToList();
                var accountCreateTransaction = new AccountCreateTransaction().SetNodeAccountIds(nodeAccountIds).SetKeyWithoutAlias(publicKey).SetInitialBalance(new Hbar(1));
                var expectedNodeAccountIds = accountCreateTransaction.GetNodeAccountIds();
                var expectedBalance = new Hbar(1);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = (AccountCreateTransaction)Transaction.FromBytes(transactionBytesSerialized);
                Assert.Equal(expectedNodeAccountIds.Count, accountCreateTransactionDeserialized.GetNodeAccountIds().Count);
                Assert.Equal(expectedBalance, accountCreateTransactionDeserialized.GetInitialBalance());
                AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(accountCreateTransactionDeserialized.GetTransactionId());
            }
        }

        virtual void CanSerializeDeserializeAndExecuteIncompleteTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var accountCreateTransaction = new AccountCreateTransaction().SetKeyWithoutAlias(publicKey).SetInitialBalance(new Hbar(1));
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = (AccountCreateTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var txReceipt = accountCreateTransactionDeserialized.Execute(testEnv.client).GetReceipt(testEnv.client);
                new AccountDeleteTransaction().SetAccountId(txReceipt.accountId).SetTransferAccountId(testEnv.client.GetOperatorAccountId()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client);
            }
        }

        virtual void CanSerializeDeserializeAndExecuteIncompleteTransactionWithNodeAccountIds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var nodeAccountIds = testEnv.client.GetNetwork().Values().Stream().ToList();
                var accountCreateTransaction = new AccountCreateTransaction().SetNodeAccountIds(nodeAccountIds).SetKeyWithoutAlias(publicKey).SetInitialBalance(new Hbar(1));
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = (AccountCreateTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var txReceipt = accountCreateTransactionDeserialized.Execute(testEnv.client).GetReceipt(testEnv.client);
                new AccountDeleteTransaction().SetAccountId(txReceipt.accountId).SetTransferAccountId(testEnv.client.GetOperatorAccountId()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client);
            }
        }

        virtual void CanSerializeDeserializeEditExecuteCompareFields()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var accountCreateTransaction = new AccountCreateTransaction().SetKeyWithoutAlias(publicKey);
                var expectedBalance = new Hbar(1);
                var nodeAccountIds = testEnv.client.GetNetwork().Values().Stream().ToList();
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = (AccountCreateTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var txReceipt = accountCreateTransactionDeserialized.SetInitialBalance(new Hbar(1)).SetNodeAccountIds(nodeAccountIds).SetTransactionId(TransactionId.Generate(testEnv.client.GetOperatorAccountId())).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(expectedBalance, accountCreateTransactionDeserialized.GetInitialBalance());
                new AccountDeleteTransaction().SetAccountId(txReceipt.accountId).SetTransferAccountId(testEnv.client.GetOperatorAccountId()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client);
            }
        }

        virtual void CanSerializeDeserializeEditExecuteCompareFieldsIncompleteTransactionWithNodeAccountIds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var nodeAccountIds = testEnv.client.GetNetwork().Values().Stream().ToList();
                var accountCreateTransaction = new AccountCreateTransaction().SetNodeAccountIds(nodeAccountIds).SetKeyWithoutAlias(publicKey);
                var expectedBalance = new Hbar(1);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = (AccountCreateTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var txReceipt = accountCreateTransactionDeserialized.SetInitialBalance(new Hbar(1)).SetTransactionId(TransactionId.Generate(testEnv.client.GetOperatorAccountId())).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(expectedBalance, accountCreateTransactionDeserialized.GetInitialBalance());
                new AccountDeleteTransaction().SetAccountId(txReceipt.accountId).SetTransferAccountId(testEnv.client.GetOperatorAccountId()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client);
            }
        }

        virtual void CanFreezeSignSerializeDeserializeReserializeAndExecute()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var publicKey = adminKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var initialBalance = new Hbar(1);
                var autoRenewPeriod = java.time.Duration.OfSeconds(2592000);
                var memo = "test account memo";
                var maxAutomaticTokenAssociations = 4;
                var accountCreateTransaction = new AccountCreateTransaction().SetKeyWithoutAlias(publicKey).SetInitialBalance(initialBalance).SetReceiverSignatureRequired(true).SetAutoRenewPeriod(autoRenewPeriod).SetAccountMemo(memo).SetMaxAutomaticTokenAssociations(maxAutomaticTokenAssociations).SetDeclineStakingReward(true).SetAlias(evmAddress).FreezeWith(testEnv.client).Sign(adminKey);
                var transactionBytesSerialized = accountCreateTransaction.ToBytes();
                AccountCreateTransaction accountCreateTransactionDeserialized = (AccountCreateTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var transactionBytesReserialized = accountCreateTransactionDeserialized.ToBytes();
                Assert.Equal(transactionBytesSerialized, transactionBytesReserialized);
                AccountCreateTransaction accountCreateTransactionReserialized = (AccountCreateTransaction)Transaction.FromBytes(transactionBytesReserialized);
                var txResponse = accountCreateTransactionReserialized.Execute(testEnv.client);
                var accountId = txResponse.GetReceipt(testEnv.client).accountId;
                new AccountDeleteTransaction().SetAccountId(accountId).SetTransferAccountId(testEnv.client.GetOperatorAccountId()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client);
            }
        }

        virtual void CanFreezeSerializeDeserializeAddSignatureAndExecute()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction().SetKeyWithoutAlias(key).FreezeWith(testEnv.client).SignWithOperator(testEnv.client);
                var expectedHash = transaction.GetTransactionHashPerNode();
                var response = transaction.Execute(testEnv.client);
                var record = response.GetRecord(testEnv.client);
                AssertThat(expectedHash[response.nodeId]).ContainsExactly(record.transactionHash.ToByteArray());
                var accountId = record.receipt.accountId;
                AssertThat(accountId).IsNotNull();
                var deleteTransaction = new AccountDeleteTransaction().SetAccountId(accountId).SetTransferAccountId(testEnv.operatorId).FreezeWith(testEnv.client);
                var updateBytes = deleteTransaction.ToBytes();
                var sig1 = key.SignTransaction(deleteTransaction);
                var deleteTransaction2 = Transaction.FromBytes(updateBytes);
                deleteTransaction2.AddSignature(key.GetPublicKey(), sig1).Execute(testEnv.client);
            }
        }

        virtual void CanFreezeSignSerializeDeserializeAndCompareFileAppendChunkedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateED25519();
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                Thread.Sleep(5000);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                var fileAppendTransaction = new FileAppendTransaction().SetFileId(fileId).SetContents(Contents.BIG_CONTENTS).FreezeWith(testEnv.client).Sign(privateKey);
                var transactionBytesSerialized = fileAppendTransaction.ToBytes();
                FileAppendTransaction fileAppendTransactionDeserialized = (FileAppendTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var transactionBytesReserialized = fileAppendTransactionDeserialized.ToBytes();
                Assert.Equal(transactionBytesSerialized, transactionBytesReserialized);
            }
        }

        virtual void CanSerializeDeserializeExecuteFileAppendChunkedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                Thread.Sleep(5000);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                var fileAppendTransaction = new FileAppendTransaction().SetFileId(fileId).SetContents(Contents.BIG_CONTENTS);
                var transactionBytesSerialized = fileAppendTransaction.ToBytes();
                FileAppendTransaction fileAppendTransactionDeserialized = (FileAppendTransaction)Transaction.FromBytes(transactionBytesSerialized);
                fileAppendTransactionDeserialized.Execute(testEnv.client).GetReceipt(testEnv.client);
                var contents = new FileContentsQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]" + Contents.BIG_CONTENTS);
                info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 13522);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanSerializeDeserializeExecuteIncompleteFileAppendChunkedTransactionWithNodeAccountIds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var nodeAccountIds = testEnv.client.GetNetwork().Values().Stream().ToList();
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                Thread.Sleep(5000);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                var fileAppendTransaction = new FileAppendTransaction().SetNodeAccountIds(nodeAccountIds).SetFileId(fileId).SetContents(Contents.BIG_CONTENTS);
                var transactionBytesSerialized = fileAppendTransaction.ToBytes();
                FileAppendTransaction fileAppendTransactionDeserialized = (FileAppendTransaction)Transaction.FromBytes(transactionBytesSerialized);
                fileAppendTransactionDeserialized.SetTransactionId(TransactionId.Generate(testEnv.client.GetOperatorAccountId())).Execute(testEnv.client).GetReceipt(testEnv.client);
                var contents = new FileContentsQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]" + Contents.BIG_CONTENTS);
                info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 13522);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanFreezeSignSerializeDeserializeAndCompareTopicMessageSubmitChunkedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateED25519();
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                Thread.Sleep(5000);
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 0);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMaxChunks(15).SetMessage(Contents.BIG_CONTENTS).FreezeWith(testEnv.client).Sign(privateKey);
                var transactionBytesSerialized = topicMessageSubmitTransaction.ToBytes();
                TopicMessageSubmitTransaction fileAppendTransactionDeserialized = (TopicMessageSubmitTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var transactionBytesReserialized = fileAppendTransactionDeserialized.ToBytes();
                Assert.Equal(transactionBytesSerialized, transactionBytesReserialized);
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanSerializeDeserializeExecuteIncompleteTopicMessageSubmitChunkedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                Thread.Sleep(5000);
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 0);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMaxChunks(15).SetMessage(Contents.BIG_CONTENTS);
                var transactionBytesSerialized = topicMessageSubmitTransaction.ToBytes();
                TopicMessageSubmitTransaction topicMessageSubmitTransactionDeserialized = (TopicMessageSubmitTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var responses = topicMessageSubmitTransactionDeserialized.ExecuteAll(testEnv.client);
                foreach (var resp in responses)
                {
                    resp.GetReceipt(testEnv.client);
                }

                info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 14);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanSerializeDeserializeExecuteIncompleteTopicMessageSubmitChunkedTransactionWithNodeAccountIds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var nodeAccountIds = testEnv.client.GetNetwork().Values().Stream().ToList();
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                Thread.Sleep(5000);
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 0);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction().SetNodeAccountIds(nodeAccountIds).SetTopicId(topicId).SetMaxChunks(15).SetMessage(Contents.BIG_CONTENTS);
                var transactionBytesSerialized = topicMessageSubmitTransaction.ToBytes();
                TopicMessageSubmitTransaction topicMessageSubmitTransactionDeserialized = (TopicMessageSubmitTransaction)Transaction.FromBytes(transactionBytesSerialized);
                var responses = topicMessageSubmitTransactionDeserialized.ExecuteAll(testEnv.client);
                foreach (var resp in responses)
                {
                    resp.GetReceipt(testEnv.client);
                }

                info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 14);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        // TODO: this test has a bunch of things hard-coded into it, which is kinda
        // dumb, but it's a good idea for a test.
        // Any way to fix it and bring it back?
        virtual void TransactionFromToBytes2()
        {
            AssertThatNoException().IsThrownBy(() =>
            {
                var id = TransactionId.Generate(new AccountId(0, 0, 542348));
                var transactionBodyBuilder = TransactionBody.NewBuilder();
                transactionBodyBuilder.SetTransactionID(TransactionID.NewBuilder().SetTransactionValidStart(Timestamp.NewBuilder().SetNanos(id.validStart.GetNano()).SetSeconds(id.validStart.GetEpochSecond()).Build()).SetAccountID(AccountID.NewBuilder().SetAccountNum(542348).SetRealmNum(0).SetShardNum(0).Build()).Build()).SetNodeAccountID(AccountID.NewBuilder().SetAccountNum(3).SetRealmNum(0).SetShardNum(0).Build()).SetTransactionFee(200000000).SetTransactionValidDuration(Duration.NewBuilder().SetSeconds(120).Build()).SetGenerateRecord(false).SetMemo("").SetCryptoTransfer(CryptoTransferTransactionBody.NewBuilder().SetTransfers(TransferList.NewBuilder().AddAccountAmounts(AccountAmount.NewBuilder().SetAccountID(AccountID.NewBuilder().SetAccountNum(47439).SetRealmNum(0).SetShardNum(0).Build()).SetAmount(10).Build()).AddAccountAmounts(AccountAmount.NewBuilder().SetAccountID(AccountID.NewBuilder().SetAccountNum(542348).SetRealmNum(0).SetShardNum(0).Build()).SetAmount(-10).Build()).Build()).Build());
                var bodyBytes = transactionBodyBuilder.Build().ToByteString();
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
                var signedBuilder = SignedTransaction.NewBuilder();
                signedBuilder.SetBodyBytes(bodyBytes).SetSigMap(SignatureMap.NewBuilder().AddSigPair(SignaturePair.NewBuilder().SetEd25519(ByteString.CopyFrom(signature1)).SetPubKeyPrefix(ByteString.CopyFrom(publicKey1.ToBytes())).Build()).AddSigPair(SignaturePair.NewBuilder().SetEd25519(ByteString.CopyFrom(signature2)).SetPubKeyPrefix(ByteString.CopyFrom(publicKey2.ToBytes())).Build()).AddSigPair(SignaturePair.NewBuilder().SetEd25519(ByteString.CopyFrom(signature3)).SetPubKeyPrefix(ByteString.CopyFrom(publicKey3.ToBytes())).Build()).AddSigPair(SignaturePair.NewBuilder().SetEd25519(ByteString.CopyFrom(signature4)).SetPubKeyPrefix(ByteString.CopyFrom(publicKey4.ToBytes())).Build()).AddSigPair(SignaturePair.NewBuilder().SetEd25519(ByteString.CopyFrom(signature5)).SetPubKeyPrefix(ByteString.CopyFrom(publicKey5.ToBytes())).Build()));
                var byts = signedBuilder.Build().ToByteString();
                byts = TransactionList.NewBuilder().AddTransactionList(com.hedera.hashgraph.sdk.proto.Transaction.NewBuilder().SetSignedTransactionBytes(byts).Build()).Build().ToByteString();
                var tx = (TransferTransaction)Transaction.FromBytes(byts.ToByteArray());
                using (var testEnv = new IntegrationTestEnv(1))
                {
                    Assert.Equal(tx.GetHbarTransfers()[new AccountId(0, 0, 542348)].ToTinybars(), -10);
                    Assert.Equal(tx.GetHbarTransfers()[new AccountId(0, 0, 47439)].ToTinybars(), 10);
                    AssertThat(tx.GetNodeAccountIds()).IsNotNull();
                    Assert.Equal(tx.GetNodeAccountIds().Count, 1);
                    Assert.Equal(tx.GetNodeAccountIds()[0], new AccountId(0, 0, 3));
                    var signatures = tx.GetSignatures();
                    Assert.Equal(Arrays.ToString(signatures[new AccountId(0, 0, 3)][publicKey1]), Arrays.ToString(signature1));
                    Assert.Equal(Arrays.ToString(signatures[new AccountId(0, 0, 3)][publicKey2]), Arrays.ToString(signature2));
                    Assert.Equal(Arrays.ToString(signatures[new AccountId(0, 0, 3)][publicKey3]), Arrays.ToString(signature3));
                    Assert.Equal(Arrays.ToString(signatures[new AccountId(0, 0, 3)][publicKey4]), Arrays.ToString(signature4));
                    Assert.Equal(Arrays.ToString(signatures[new AccountId(0, 0, 3)][publicKey5]), Arrays.ToString(signature5));
                    var resp = tx.Execute(testEnv.client);
                    resp.GetReceipt(testEnv.client);
                }
            });
        }

        virtual void CanAddSignatureToTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Step 1: Create a new key for the account
                var newKey = PrivateKey.GenerateED25519();

                // Step 2: Create account with the new key
                var createResponse = new AccountCreateTransaction().SetKeyWithoutAlias(newKey.GetPublicKey()).SetNodeAccountIds(testEnv.client.GetNetwork().Values().Stream().ToList()).Execute(testEnv.client);
                var createReceipt = createResponse.GetReceipt(testEnv.client);
                var accountId = Objects.RequireNonNull(createReceipt.accountId);
                var nodeId = createResponse.nodeId;

                // Step 3: Create account delete transaction and freeze it
                var deleteTransaction = new AccountDeleteTransaction().SetNodeAccountIds(Arrays.AsList(nodeId)).SetAccountId(accountId).SetTransferAccountId(testEnv.client.GetOperatorAccountId()).FreezeWith(testEnv.client);

                // Step 4: Get signable body bytes list
                var signableBodyList = deleteTransaction.GetSignableNodeBodyBytesList();
                Assert.NotEmpty(signableBodyList);

                // Step 5: Sign each signable body externally and add signatures back
                foreach (var signableBody in signableBodyList)
                {
                    byte[] signature = newKey.Sign(signableBody.GetBody());
                    deleteTransaction = deleteTransaction.AddSignature(newKey.GetPublicKey(), signature, signableBody.GetTransactionID(), signableBody.GetNodeID());
                }

                var deleteResponse = deleteTransaction.Execute(testEnv.client);
                var deleteReceipt = deleteResponse.GetReceipt(testEnv.client);
                Assert.Equal(deleteReceipt.status, Status.SUCCESS);
            }
        }
    }
}