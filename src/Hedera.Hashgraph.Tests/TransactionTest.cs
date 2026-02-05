// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Transaction;
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PrivateKey mockPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly IList<AccountId> testNodeAccountIds = Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"));
        private static readonly AccountId testAccountId = AccountId.FromString("0.0.5006");
        private static readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        private static readonly TransactionId testTransactionID = TransactionId.WithValidStart(testAccountId, validStart);
        private Client client;
        // Additional test setup for new V2 methods
        private FileId fileID;
        private AccountId nodeAccountID1;
        private AccountId nodeAccountID2;
        private IList<AccountId> nodeAccountIDs;
        private byte[] mockSignature;
        virtual void SetUp()
        {
            client = Client.ForTestnet();
            client.SetOperator(testAccountId, mockPrivateKey);
            fileID = new FileId(3);
            nodeAccountID1 = AccountId.FromString("0.0.3");
            nodeAccountID2 = AccountId.FromString("0.0.4");
            nodeAccountIDs = Arrays.AsList(nodeAccountID1, nodeAccountID2);
            mockSignature = new byte[]
            {
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9
            };
        }

        virtual void TransactionFromBytesWorksWithProtobufTransactionBytes()
        {
            var bytes = Hex.Decode("1acc010a640a2046fe5013b6f6fc796c3e65ec10d2a10d03c07188fc3de13d46caad6b8ec4dfb81a4045f1186be5746c9783f68cb71d6a71becd3ffb024906b855ac1fa3a2601273d41b58446e5d6a0aaf421c229885f9e70417353fab2ce6e9d8e7b162e9944e19020a640a20f102e75ff7dc3d72c9b7075bb246fcc54e714c59714814011e8f4b922d2a6f0a1a40f2e5f061349ab03fa21075020c75cf876d80498ae4bac767f35941b8e3c393b0e0a886ede328e44c1df7028ea1474722f2dcd493812d04db339480909076a10122500a180a0c08a1cc98830610c092d09e0312080800100018e4881d120608001000180418b293072202087872240a220a0f0a080800100018e4881d10ff83af5f0a0f0a080800100018eb881d108084af5f");
            var transaction = (TransferTransaction)FromBytes(bytes);
            AssertThat(transaction.GetHbarTransfers()).ContainsEntry(new AccountId(0, 0, 476260), new Hbar(1).Negated());
            AssertThat(transaction.GetHbarTransfers()).ContainsEntry(new AccountId(0, 0, 476267), new Hbar(1));
        }

        virtual void TokenAssociateTransactionFromTransactionBodyBytes()
        {
            var tokenAssociateTransactionBodyProto = TokenAssociateTransactionBody.NewBuilder().Build();
            var transactionBodyProto = TransactionBody.NewBuilder().SetTokenAssociate(tokenAssociateTransactionBodyProto).Build();
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(transactionBodyProto);
            var tokenAssociateTransactionFromBytes = Transaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }

        virtual void TokenAssociateTransactionFromSignedTransactionBytes()
        {
            var tokenAssociateTransactionBodyProto = TokenAssociateTransactionBody.NewBuilder().Build();
            var transactionBodyProto = TransactionBody.NewBuilder().SetTokenAssociate(tokenAssociateTransactionBodyProto).Build();
            var signedTransactionProto = SignedTransaction.NewBuilder().SetBodyBytes(transactionBodyProto.ToByteString()).Build();
            var signedTransactionBodyProto = TransactionBody.ParseFrom(signedTransactionProto.GetBodyBytes());
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(signedTransactionBodyProto);
            var tokenAssociateTransactionFromBytes = Transaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }

        virtual void TokenAssociateTransactionFromTransactionBytes()
        {
            var tokenAssociateTransactionBodyProto = TokenAssociateTransactionBody.NewBuilder().Build();
            var transactionBodyProto = TransactionBody.NewBuilder().SetTokenAssociate(tokenAssociateTransactionBodyProto).Build();
            var signedTransactionProto = SignedTransaction.NewBuilder().SetBodyBytes(transactionBodyProto.ToByteString()).Build();
            var signedTransactionBodyProto = TransactionBody.ParseFrom(signedTransactionProto.GetBodyBytes());
            var transactionSignedProto = com.hedera.hashgraph.sdk.proto.Transaction.NewBuilder().SetSignedTransactionBytes(signedTransactionBodyProto.ToByteString()).Build();
            var transactionSignedBodyProto = TransactionBody.ParseFrom(transactionSignedProto.GetSignedTransactionBytes());
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(transactionSignedBodyProto);
            var tokenAssociateTransactionFromBytes = Transaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }

        private TokenAssociateTransaction SpawnTestTransaction(TransactionBody txBody)
        {
            return new TokenAssociateTransaction(txBody).SetNodeAccountIds(testNodeAccountIds).SetTransactionId(TransactionId.WithValidStart(testAccountId, validStart)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void SameSizeForIdenticalTransactions()
        {
            var accountCreateTransaction = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var accountCreateTransaction2 = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            Assert.Equal(accountCreateTransaction.GetTransactionSize(), accountCreateTransaction2.GetTransactionSize());
        }

        virtual void SignedTransactionShouldHaveLargerSize()
        {
            var accountCreateTransaction = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze().Sign(PrivateKey.GenerateECDSA());
            var accountCreateTransaction2 = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            AssertThat(accountCreateTransaction.GetTransactionSize()).IsGreaterThan(accountCreateTransaction2.GetTransactionSize());
        }

        virtual void TransactionWithLargerContentShouldHaveLargerTransactionBody()
        {
            var fileCreateTransactionSmallContent = new FileCreateTransaction().SetContents("smallBody").SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var fileCreateTransactionLargeContent = new FileCreateTransaction().SetContents("largeLargeBody").SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            AssertThat(fileCreateTransactionSmallContent.GetTransactionBodySize()).IsLessThan(fileCreateTransactionLargeContent.GetTransactionBodySize());
        }

        virtual void TransactionWithoutOptionalFieldsShouldHaveSmallerTransactionBody()
        {
            var noOptionalFieldsTransaction = new AccountCreateTransaction().SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var fullOptionalFieldsTransaction = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).SetMaxTransactionFee(new Hbar(1)).SetTransactionValidDuration(Duration.OfHours(1)).Freeze();
            AssertThat(noOptionalFieldsTransaction.GetTransactionBodySize()).IsLessThan(fullOptionalFieldsTransaction.GetTransactionBodySize());
        }

        virtual void MultiChunkTransactionShouldReturnArrayOfBodySizes()
        {
            var chunkSize = 1024;
            byte[] content = new byte[chunkSize * 3];
            Arrays.Fill(content, (byte)'a');
            var fileAppentTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetChunkSize(chunkSize).SetContents(content).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var objects = fileAppentTx.BodySizeAllChunks();
            AssertThat(objects).IsNotNull();
            AssertThat(objects).HasSize(3);
        }

        virtual void SingleChunkTransactionShouldReturnArrayOfOneSize()
        {

            // Small enough for one chunk
            byte[] smallContent = new byte[500];
            Arrays.Fill(smallContent, (byte)'a');
            var fileAppendTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetContents(smallContent).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var bodySizes = fileAppendTx.BodySizeAllChunks();
            AssertThat(bodySizes).IsNotNull();
            AssertThat(bodySizes).HasSize(1);
        }

        virtual void TransactionWithNoContentShouldReturnSingleBodyChunk()
        {
            var fileAppendTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetTransactionId(new TransactionId(testAccountId, validStart)).SetContents(" ").SetNodeAccountIds(testNodeAccountIds).Freeze();
            var bodySizes = fileAppendTx.BodySizeAllChunks();
            AssertThat(bodySizes).IsNotNull();
            AssertThat(bodySizes).HasSize(1); // Contains one empty chunk
        }

        virtual void ChunkedFileAppendTransactionShouldReturnProperSizes()
        {
            byte[] largeContent = new byte[2048];
            Arrays.Fill(largeContent, (byte)'a');
            var largeFileAppendTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetContents(largeContent).SetChunkSize(1024).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            long largeSize = largeFileAppendTx.GetTransactionSize();
            byte[] smallContent = new byte[512];
            Arrays.Fill(smallContent, (byte)'a');
            var smallFileAppendTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetContents(smallContent).SetTransactionId(new TransactionId(testAccountId, validStart)).SetNodeAccountIds(testNodeAccountIds).Freeze();
            long smallSize = smallFileAppendTx.GetTransactionSize();

            // Since large content is 2KB and chunk size is 1KB, this should create 2 chunks
            // Size should be greater than single chunk size
            AssertThat(largeSize).IsGreaterThan(1024);

            // The larger chunked transaction should be bigger than the small single-chunk transaction
            AssertThat(largeSize).IsGreaterThan(smallSize);
        }

        virtual void TestAddSignatureV2SingleNodeSingleChunk()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents("test content".GetBytes()).SetNodeAccountIds(Arrays.AsList(nodeAccountID1)).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            AssertThat(signatures).HasSize(1);
            AssertThat(signatures).ContainsKey(nodeAccountID1);
            Map<PublicKey, byte[]> nodeSignatures = signatures[nodeAccountID1];
            foreach (Map.Entry<PublicKey, byte[]> entry in nodeSignatures.EntrySet())
            {
                Assert.Equal(entry.GetValue(), mockSignature);
            }
        }

        virtual void TestAddSignatureV2MultipleNodesSingleChunk()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents("test content".GetBytes()).SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID2);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            AssertThat(signatures).HasSize(2);
            AssertThat(signatures).ContainsKey(nodeAccountID1);
            AssertThat(signatures).ContainsKey(nodeAccountID2);
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                Map<PublicKey, byte[]> nodeSignatures = signatures[nodeID];
                foreach (Map.Entry<PublicKey, byte[]> entry in nodeSignatures.EntrySet())
                {
                    Assert.Equal(entry.GetValue(), mockSignature);
                }
            }
        }

        virtual void TestAddSignatureV2MultipleNodesMultipleChunks()
        {
            byte[] content = new byte[2048];
            for (int i = 0; i < content.Length; i++)
            {
                content[i] = (byte)(i % 256);
            }

            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents(content).SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID2);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            AssertThat(signatures).HasSize(2);
            AssertThat(signatures).ContainsKey(nodeAccountID1);
            AssertThat(signatures).ContainsKey(nodeAccountID2);
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                Map<PublicKey, byte[]> nodeSigs = signatures[nodeID];
                AssertThat(nodeSigs).HasSize(1);
                foreach (Map.Entry<PublicKey, byte[]> entry in nodeSigs.EntrySet())
                {
                    AssertThat(entry.GetKey()).IsNotNull();
                    Assert.Equal(entry.GetKey().ToString(), mockPrivateKey.GetPublicKey().ToString());
                    Assert.Equal(entry.GetValue(), mockSignature);
                }
            }
        }

        virtual void TestAddSignatureV2WrongNodeID()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents("test content".GetBytes()).SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            AccountId invalidNodeID = AccountId.FromString("0.0.999");
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, invalidNodeID);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            AssertThat(signatures).DoesNotContainKey(invalidNodeID);
        }

        virtual void TestAddSignatureV2WrongTransactionID()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents("test content".GetBytes()).SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            TransactionId invalidTxID = TransactionId.WithValidStart(AccountId.FromString("0.0.999"), Instant.Now());
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, invalidTxID, nodeAccountID1);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            if (signatures.ContainsKey(nodeAccountID1))
            {
                AssertThat(signatures[nodeAccountID1]).DoesNotContainKey(mockPrivateKey.GetPublicKey());
            }
        }

        virtual void TestAddSignatureV2SameSignatureTwice()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents("test content".GetBytes()).SetNodeAccountIds(Arrays.AsList(nodeAccountID1)).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            AssertThat(signatures).HasSize(1);
            AssertThat(signatures).ContainsKey(nodeAccountID1);
            Map<PublicKey, byte[]> nodeSigs = signatures[nodeAccountID1];
            AssertThat(nodeSigs).HasSize(1);
            foreach (Map.Entry<PublicKey, byte[]> entry in nodeSigs.EntrySet())
            {
                Assert.Equal(entry.GetValue(), mockSignature);
            }
        }

        virtual void TestAddSignatureV2WithEmptyInnerSignedTransactions()
        {
            var tx = new FileAppendTransaction();
            PrivateKey key = PrivateKey.GenerateED25519();
            byte[] mockSig = new[]
            {
                0,
                1,
                2,
                3,
                4
            };
            AccountId nodeID = AccountId.FromString("0.0.3");
            TransactionId testTxID = TransactionId.WithValidStart(AccountId.FromString("0.0.5"), Instant.Now());
            var result = tx.AddSignature(key.GetPublicKey(), mockSig, testTxID, nodeID);
            AssertThat(result).IsSameAs(tx);
        }

        virtual void TestGetSignableNodeBodyBytesListUnfrozen()
        {
            var tx = new TransferTransaction();
            AssertThrows(typeof(Exception), () =>
            {
                tx.GetSignableNodeBodyBytesList();
            });
        }

        virtual void TestGetSignableNodeBodyBytesListBasic()
        {
            var tx = new TransferTransaction().SetNodeAccountIds(Arrays.AsList(nodeAccountID1)).SetTransactionId(testTransactionID).AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1)).FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            Assert.NotEmpty(list);
            AssertThat(list).HasSize(1); // Should have one entry for our single node
            Assert.Equal(list[0].GetNodeID(), nodeAccountID1);
            Assert.Equal(list[0].GetTransactionID(), testTransactionID);
            Assert.NotEmpty(list[0].GetBody());
        }

        virtual void TestGetSignableNodeBodyBytesListContents()
        {
            var tx = new TransferTransaction().SetNodeAccountIds(Arrays.AsList(nodeAccountID1)).SetTransactionId(testTransactionID).AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1)).FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            TransactionBody body = TransactionBody.ParseFrom(list[0].GetBody());
            AssertThat(body.GetCryptoTransfer()).IsNotNull();
            Assert.Equal(AccountId.FromProtobuf(body.GetNodeAccountID()).ToString(), nodeAccountID1.ToString());
            Assert.Equal(TransactionId.FromProtobuf(body.GetTransactionID()).ToString(), testTransactionID.ToString());
        }

        virtual void TestGetSignableNodeBodyBytesListMultipleNodeIDs()
        {
            var tx = new TransferTransaction().SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1)).FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            AssertThat(list).HasSize(2); // Should have two entries, one per node
            for (int i = 0; i < nodeAccountIDs.Count; i++)
            {
                AccountId nodeID = nodeAccountIDs[i];
                Assert.Equal(list[i].GetNodeID(), nodeID);
                Assert.Equal(list[i].GetTransactionID(), testTransactionID);
                Assert.NotEmpty(list[i].GetBody());

                // Verify body contents
                TransactionBody body = TransactionBody.ParseFrom(list[i].GetBody());
                AssertThat(body.GetCryptoTransfer()).IsNotNull();
                Assert.Equal(AccountId.FromProtobuf(body.GetNodeAccountID()).ToString(), nodeID.ToString());
            }
        }

        virtual void TestGetSignableNodeBodyBytesListFileAppendMultipleChunks()
        {
            byte[] content = new byte[4096];
            for (int i = 0; i < content.Length; i++)
            {
                content[i] = (byte)(i % 256);
            }

            var tx = new FileAppendTransaction().SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetFileId(new FileId(5)).SetContents(content).SetChunkSize(2048).FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            AssertThat(list).HasSize(4); // Should have 4 entries: 2 nodes * 2 chunks

            // Map to track transaction IDs per node
            Dictionary<string, Dictionary<string, bool>> txIDsByNode = new HashMap();
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                txIDsByNode.Put(nodeID.ToString(), new HashMap());
            }

            for (int i = 0; i < list.Count; i++)
            {
                AssertThat(nodeAccountIDs).Contains(list[i].GetNodeID());
                AssertThat(list[i].GetTransactionID()).IsNotNull();
                Assert.NotEmpty(list[i].GetBody());
                string nodeIDStr = list[i].GetNodeID().ToString();
                string txIDStr = list[i].GetTransactionID().ToString();

                // Each transaction ID should appear exactly once per node
                AssertThat(txIDsByNode[nodeIDStr].ContainsKey(txIDStr)).As("Duplicate transaction ID found for the same node").IsFalse();
                txIDsByNode[nodeIDStr].Put(txIDStr, true);
                TransactionBody body = TransactionBody.ParseFrom(list[i].GetBody());
                AssertThat(body.GetFileAppend()).IsNotNull();
                Assert.Equal(AccountId.FromProtobuf(body.GetNodeAccountID()).ToString(), list[i].GetNodeID().ToString());
            }


            // Verify each node has the same number of unique transaction IDs
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                AssertThat(txIDsByNode[nodeID.ToString()]).As("Each node should have exactly 2 unique transaction IDs").HasSize(2);
            }


            // Verify that all nodes have the same set of transaction IDs
            Dictionary<string, bool> firstNodeTxIDs = txIDsByNode[nodeAccountID1.ToString()];
            for (int i = 1; i < nodeAccountIDs.Count; i++)
            {
                Dictionary<string, bool> nodeTxIDs = txIDsByNode[nodeAccountIDs[i].ToString()];
                foreach (string txID in firstNodeTxIDs.KeySet())
                {
                    AssertThat(nodeTxIDs.ContainsKey(txID)).As("All nodes should have the same set of transaction IDs").IsTrue();
                }
            }
        }
    }
}