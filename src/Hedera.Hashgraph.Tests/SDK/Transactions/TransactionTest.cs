// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class TransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PrivateKey mockPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly List<AccountId> testNodeAccountIds = Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"));
        private static readonly AccountId testAccountId = AccountId.FromString("0.0.5006");
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private static readonly TransactionId testTransactionID = TransactionId.WithValidStart(testAccountId, Timestamp.FromDateTimeOffset(validStart));
        private Client client;
        // Additional test setup for new V2 methods
        private FileId fileID;
        private AccountId nodeAccountID1;
        private AccountId nodeAccountID2;
        private List<AccountId> nodeAccountIDs;
        private byte[] mockSignature;
        public virtual void SetUp()
        {
            client = Client.ForTestnet();
            client.OperatorSet(testAccountId, mockPrivateKey);
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

        public virtual void TransactionFromBytesWorksWithProtobufTransactionBytes()
        {
            var bytes = Hex.Decode("1acc010a640a2046fe5013b6f6fc796c3e65ec10d2a10d03c07188fc3de13d46caad6b8ec4dfb81a4045f1186be5746c9783f68cb71d6a71becd3ffb024906b855ac1fa3a2601273d41b58446e5d6a0aaf421c229885f9e70417353fab2ce6e9d8e7b162e9944e19020a640a20f102e75ff7dc3d72c9b7075bb246fcc54e714c59714814011e8f4b922d2a6f0a1a40f2e5f061349ab03fa21075020c75cf876d80498ae4bac767f35941b8e3c393b0e0a886ede328e44c1df7028ea1474722f2dcd493812d04db339480909076a10122500a180a0c08a1cc98830610c092d09e0312080800100018e4881d120608001000180418b293072202087872240a220a0f0a080800100018e4881d10ff83af5f0a0f0a080800100018eb881d108084af5f");
            var transaction = (TransferTransaction)FromBytes(bytes);
            AssertThat(transaction.GetHbarTransfers()).ContainsEntry(new AccountId(0, 0, 476260), new Hbar(1).Negated());
            AssertThat(transaction.GetHbarTransfers()).ContainsEntry(new AccountId(0, 0, 476267), new Hbar(1));
        }

        public virtual void TokenAssociateTransactionFromTransactionBodyBytes()
        {
            var tokenAssociateTransactionBodyProto = TokenAssociateTransactionBody.NewBuilder().Build();
            var transactionBodyProto = TransactionBody.NewBuilder().SetTokenAssociate(tokenAssociateTransactionBodyProto).Build();
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(transactionBodyProto);
            var tokenAssociateTransactionFromBytes = Transaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }

        public virtual void TokenAssociateTransactionFromSignedTransactionBytes()
        {
            var tokenAssociateTransactionBodyProto = TokenAssociateTransactionBody.NewBuilder().Build();
            var transactionBodyProto = TransactionBody.NewBuilder().SetTokenAssociate(tokenAssociateTransactionBodyProto).Build();
            var signedTransactionProto = SignedTransaction.NewBuilder().SetBodyBytes(transactionBodyProto.ToByteString()).Build();
            var signedTransactionBodyProto = TransactionBody.ParseFrom(signedTransactionProto.GetBodyBytes());
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(signedTransactionBodyProto);
            var tokenAssociateTransactionFromBytes = Transaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }

        public virtual void TokenAssociateTransactionFromTransactionBytes()
        {
            var tokenAssociateTransactionBodyProto = TokenAssociateTransactionBody.NewBuilder().Build();
            var transactionBodyProto = TransactionBody.NewBuilder().SetTokenAssociate(tokenAssociateTransactionBodyProto).Build();
            var signedTransactionProto = SignedTransaction.NewBuilder().SetBodyBytes(transactionBodyProto.ToByteString()).Build();
            var signedTransactionBodyProto = TransactionBody.ParseFrom(signedTransactionProto.GetBodyBytes());
            var transactionSignedProto = Proto.Transaction.NewBuilder().SetSignedTransactionBytes(signedTransactionBodyProto.ToByteString()).Build();
            var transactionSignedBodyProto = TransactionBody.ParseFrom(transactionSignedProto.GetSignedTransactionBytes());
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(transactionSignedBodyProto);
            var tokenAssociateTransactionFromBytes = Transaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }

        private TokenAssociateTransaction SpawnTestTransaction(TransactionBody txBody)
        {
            return new TokenAssociateTransaction(txBody).SetNodeAccountIds(testNodeAccountIds).SetTransactionId(TransactionId.WithValidStart(testAccountId, Timestamp.FromDateTimeOffset(validStart))).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void SameSizeForIdenticalTransactions()
        {
            var accountCreateTransaction = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var accountCreateTransaction2 = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            Assert.Equal(accountCreateTransaction.GetTransactionSize(), accountCreateTransaction2.GetTransactionSize());
        }

        public virtual void SignedTransactionShouldHaveLargerSize()
        {
            var accountCreateTransaction = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze().Sign(PrivateKey.GenerateECDSA());
            var accountCreateTransaction2 = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            Assert.True(accountCreateTransaction.GetTransactionSize() > accountCreateTransaction2.GetTransactionSize());
        }

        public virtual void TransactionWithLargerContentShouldHaveLargerTransactionBody()
        {
            var fileCreateTransactionSmallContent = new FileCreateTransaction().SetContents("smallBody").SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var fileCreateTransactionLargeContent = new FileCreateTransaction().SetContents("largeLargeBody").SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            Assert.True(fileCreateTransactionSmallContent.GetTransactionBodySize() < fileCreateTransactionLargeContent.GetTransactionBodySize());
        }

        public virtual void TransactionWithoutOptionalFieldsShouldHaveSmallerTransactionBody()
        {
            var noOptionalFieldsTransaction = new AccountCreateTransaction().SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var fullOptionalFieldsTransaction = new AccountCreateTransaction().SetInitialBalance(new Hbar(2)).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).SetMaxTransactionFee(new Hbar(1)).SetTransactionValidDuration(Duration.OfHours(1)).Freeze();
            Assert.True(noOptionalFieldsTransaction.GetTransactionBodySize() < fullOptionalFieldsTransaction.GetTransactionBodySize());
        }

        public virtual void MultiChunkTransactionShouldReturnArrayOfBodySizes()
        {
            var chunkSize = 1024;
            byte[] content = new byte[chunkSize * 3];
            Arrays.Fill(content, (byte)'a');
            var fileAppentTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetChunkSize(chunkSize).SetContents(content).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var objects = fileAppentTx.BodySizeAllChunks();
            Assert.NotNull(objects);
            Assert.Equal(2, tx.GetHbarTransfers().Count);
        }

        public virtual void SingleChunkTransactionShouldReturnArrayOfOneSize()
        {

            // Small enough for one chunk
            byte[] smallContent = new byte[500];
            Arrays.Fill(smallContent, (byte)'a');
            var fileAppendTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetContents(smallContent).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            var bodySizes = fileAppendTx.BodySizeAllChunks();
            Assert.NotNull(bodySizes);
            Assert.Single(bodySizes);
        }

        public virtual void TransactionWithNoContentShouldReturnSingleBodyChunk()
        {
            var fileAppendTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetContents(" ").SetNodeAccountIds(testNodeAccountIds).Freeze();
            var bodySizes = fileAppendTx.BodySizeAllChunks();
            Assert.NotNull(bodySizes);
            Assert.Single(bodySizes); // Contains one empty chunk
        }

        public virtual void ChunkedFileAppendTransactionShouldReturnProperSizes()
        {
            byte[] largeContent = new byte[2048];
            Arrays.Fill(largeContent, (byte)'a');
            var largeFileAppendTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetContents(largeContent).SetChunkSize(1024).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            long largeSize = largeFileAppendTx.GetTransactionSize();
            byte[] smallContent = new byte[512];
            Arrays.Fill(smallContent, (byte)'a');
            var smallFileAppendTx = new FileAppendTransaction().SetFileId(new FileId(1)).SetContents(smallContent).SetTransactionId(new TransactionId(testAccountId, Timestamp.FromDateTimeOffset(validStart))).SetNodeAccountIds(testNodeAccountIds).Freeze();
            long smallSize = smallFileAppendTx.GetTransactionSize();

            // Since large content is 2KB and chunk size is 1KB, this should create 2 chunks
            // Size should be greater than single chunk size
            Assert.True(largeSize > 1024);

            // The larger chunked transaction should be bigger than the small single-chunk transaction
            Assert.True(largeSize > smallSize);
        }

        public virtual void TestAddSignatureV2SingleNodeSingleChunk()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents(Encoding.UTF8.GetBytes("test content")).SetNodeAccountIds(Arrays.AsList(nodeAccountID1)).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            Assert.Single(signatures);
            AssertThat(signatures).ContainsKey(nodeAccountID1);
            Map<PublicKey, byte[]> nodeSignatures = signatures[nodeAccountID1];
            foreach (Map.Entry<PublicKey, byte[]> entry in nodeSignatures.EntrySet())
            {
                Assert.Equal(entry.Value, mockSignature);
            }
        }

        public virtual void TestAddSignatureV2MultipleNodesSingleChunk()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents(Encoding.UTF8.GetBytes("test content")).SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID2);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            AssertThat(signatures).ContainsKey(nodeAccountID1);
            AssertThat(signatures).ContainsKey(nodeAccountID2);
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                Map<PublicKey, byte[]> nodeSignatures = signatures[nodeID];
                foreach (Map.Entry<PublicKey, byte[]> entry in nodeSignatures.EntrySet())
                {
                    Assert.Equal(entry.Value, mockSignature);
                }
            }
        }

        public virtual void TestAddSignatureV2MultipleNodesMultipleChunks()
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
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            AssertThat(signatures).ContainsKey(nodeAccountID1);
            AssertThat(signatures).ContainsKey(nodeAccountID2);
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                Map<PublicKey, byte[]> nodeSigs = signatures[nodeID];
                Assert.Single(nodeSigs);
                foreach (Map.Entry<PublicKey, byte[]> entry in nodeSigs.EntrySet())
                {
                    Assert.NotNull(entry.Key);
                    Assert.Equal(entry.Key.ToString(), mockPrivateKey.GetPublicKey().ToString());
                    Assert.Equal(entry.Value, mockSignature);
                }
            }
        }

        public virtual void TestAddSignatureV2WrongNodeID()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents(Encoding.UTF8.GetBytes("test content")).SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            AccountId invalidNodeID = AccountId.FromString("0.0.999");
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, invalidNodeID);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            AssertThat(signatures).DoesNotContainKey(invalidNodeID);
        }

        public virtual void TestAddSignatureV2WrongTransactionID()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents(Encoding.UTF8.GetBytes("test content")).SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            TransactionId invalidTxID = TransactionId.WithValidStart(AccountId.FromString("0.0.999"), DateTimeOffset.UtcNow);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, invalidTxID, nodeAccountID1);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            if (signatures.ContainsKey(nodeAccountID1))
            {
                AssertThat(signatures[nodeAccountID1]).DoesNotContainKey(mockPrivateKey.GetPublicKey());
            }
        }

        public virtual void TestAddSignatureV2SameSignatureTwice()
        {
            var transaction = new FileAppendTransaction().SetFileId(fileID).SetContents(Encoding.UTF8.GetBytes("test content")).SetNodeAccountIds(Arrays.AsList(nodeAccountID1)).SetTransactionId(testTransactionID).SetChunkSize(2048).FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            Map<AccountId, Map<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            Assert.Single(signatures);
            AssertThat(signatures).ContainsKey(nodeAccountID1);
            Map<PublicKey, byte[]> nodeSigs = signatures[nodeAccountID1];
            Assert.Single(nodeSigs);
            foreach (Map.Entry<PublicKey, byte[]> entry in nodeSigs.EntrySet())
            {
                Assert.Equal(entry.Value, mockSignature);
            }
        }

        public virtual void TestAddSignatureV2WithEmptyInnerSignedTransactions()
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
            TransactionId testTxID = TransactionId.WithValidStart(AccountId.FromString("0.0.5"), DateTimeOffset.UtcNow);
            var result = tx.AddSignature(key.GetPublicKey(), mockSig, testTxID, nodeID);
            Assert.Equal(result, tx);
        }

        public virtual void TestGetSignableNodeBodyBytesListUnfrozen()
        {
            var tx = new TransferTransaction();
            Exception exception = Assert.Throws<Exception>(() =>
            {
                tx.GetSignableNodeBodyBytesList();
            });
        }

        public virtual void TestGetSignableNodeBodyBytesListBasic()
        {
            var tx = new TransferTransaction().SetNodeAccountIds(Arrays.AsList(nodeAccountID1)).SetTransactionId(testTransactionID).AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1)).FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            Assert.NotEmpty(list);
            Assert.Single(list); // Should have one entry for our single node
            Assert.Equal(list[0].GetNodeID(), nodeAccountID1);
            Assert.Equal(list[0].GetTransactionID(), testTransactionID);
            Assert.NotEmpty(list[0].GetBody());
        }

        public virtual void TestGetSignableNodeBodyBytesListContents()
        {
            var tx = new TransferTransaction().SetNodeAccountIds(Arrays.AsList(nodeAccountID1)).SetTransactionId(testTransactionID).AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1)).FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            TransactionBody body = TransactionBody.ParseFrom(list[0].GetBody());
            Assert.NotNull(body.GetCryptoTransfer());
            Assert.Equal(AccountId.FromProtobuf(body.GetNodeAccountID()).ToString(), nodeAccountID1.ToString());
            Assert.Equal(TransactionId.FromProtobuf(body.GetTransactionID()).ToString(), testTransactionID.ToString());
        }

        public virtual void TestGetSignableNodeBodyBytesListMultipleNodeIDs()
        {
            var tx = new TransferTransaction().SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1)).FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            Assert.Equal(2, tx.GetHbarTransfers().Count); // Should have two entries, one per node
            for (int i = 0; i < nodeAccountIDs.Count; i++)
            {
                AccountId nodeID = nodeAccountIDs[i];
                Assert.Equal(list[i].GetNodeID(), nodeID);
                Assert.Equal(list[i].GetTransactionID(), testTransactionID);
                Assert.NotEmpty(list[i].GetBody());

                // Verify body contents
                TransactionBody body = TransactionBody.ParseFrom(list[i].GetBody());
                Assert.NotNull(body.GetCryptoTransfer());
                Assert.Equal(AccountId.FromProtobuf(body.GetNodeAccountID()).ToString(), nodeID.ToString());
            }
        }

        public virtual void TestGetSignableNodeBodyBytesListFileAppendMultipleChunks()
        {
            byte[] content = new byte[4096];
            for (int i = 0; i < content.Length; i++)
            {
                content[i] = (byte)(i % 256);
            }

            var tx = new FileAppendTransaction().SetNodeAccountIds(nodeAccountIDs).SetTransactionId(testTransactionID).SetFileId(new FileId(5)).SetContents(content).SetChunkSize(2048).FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            Assert.Equal(2, tx.GetHbarTransfers().Count); // Should have 4 entries: 2 nodes * 2 chunks

            // Map to track transaction IDs per node
            Dictionary<string, Dictionary<string, bool>> txIDsByNode = new HashMap();
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                txIDsByNode.Put(nodeID.ToString(), new HashMap());
            }

            for (int i = 0; i < list.Count; i++)
            {
                Assert.Contains(nodeAccountIDs, list[i].GetNodeID());
                Assert.NotNull(list[i].GetTransactionID());
                Assert.NotEmpty(list[i].GetBody());
                string nodeIDStr = list[i].GetNodeID().ToString();
                string txIDStr = list[i].GetTransactionID().ToString();

                // Each transaction ID should appear exactly once per node
                Assert.False(txIDsByNode[nodeIDStr].ContainsKey(txIDStr)).As("Duplicate transaction ID found for the same node");
                txIDsByNode[nodeIDStr].Put(txIDStr, true);
                TransactionBody body = TransactionBody.ParseFrom(list[i].GetBody());
                Assert.NotNull(body.GetFileAppend());
                Assert.Equal(AccountId.FromProtobuf(body.GetNodeAccountID()).ToString(), list[i].GetNodeID().ToString());
            }


            // Verify each node has the same number of unique transaction IDs
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                Assert.Equal(2, tx.GetHbarTransfers().Count);
            }


            // Verify that all nodes have the same set of transaction IDs
            Dictionary<string, bool> firstNodeTxIDs = txIDsByNode[nodeAccountID1.ToString()];
            for (int i = 1; i < nodeAccountIDs.Count; i++)
            {
                Dictionary<string, bool> nodeTxIDs = txIDsByNode[nodeAccountIDs[i].ToString()];
                foreach (string txID in firstNodeTxIDs.KeySet())
                {
                    Assert.True(nodeTxIDs.ContainsKey(txID)).As("All nodes should have the same set of transaction IDs");
                }
            }
        }
    }
}