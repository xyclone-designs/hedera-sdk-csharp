// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class TransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PrivateKey mockPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly List<AccountId> testNodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")];
        private static readonly AccountId testAccountId = AccountId.FromString("0.0.5006");
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private static readonly TransactionId testTransactionID = TransactionId.WithValidStart(testAccountId, validStart);
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
            nodeAccountIDs = [nodeAccountID1, nodeAccountID2];
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
        [Fact]
        public virtual void TransactionFromBytesWorksWithProtobufTransactionBytes()
        {
            var bytes = Hex.Decode("1acc010a640a2046fe5013b6f6fc796c3e65ec10d2a10d03c07188fc3de13d46caad6b8ec4dfb81a4045f1186be5746c9783f68cb71d6a71becd3ffb024906b855ac1fa3a2601273d41b58446e5d6a0aaf421c229885f9e70417353fab2ce6e9d8e7b162e9944e19020a640a20f102e75ff7dc3d72c9b7075bb246fcc54e714c59714814011e8f4b922d2a6f0a1a40f2e5f061349ab03fa21075020c75cf876d80498ae4bac767f35941b8e3c393b0e0a886ede328e44c1df7028ea1474722f2dcd493812d04db339480909076a10122500a180a0c08a1cc98830610c092d09e0312080800100018e4881d120608001000180418b293072202087872240a220a0f0a080800100018e4881d10ff83af5f0a0f0a080800100018eb881d108084af5f");
            var transaction = Transaction.FromBytes<TransferTransaction>(bytes);
            Assert.True(transaction.GetHbarTransfers().Contains(KeyValuePair.Create(new AccountId(0, 0, 476260), new Hbar(1).Negated())));
            Assert.True(transaction.GetHbarTransfers().Contains(KeyValuePair.Create(new AccountId(0, 0, 476267), new Hbar(1))));
        }
        [Fact]
        public virtual void TokenAssociateTransactionFromTransactionBodyBytes()
        {
            var tokenAssociateTransactionBodyProto = new Proto.Services.TokenAssociateTransactionBody { };
            var transactionBodyProto = new Proto.Services.TransactionBody { TokenAssociate = tokenAssociateTransactionBodyProto };
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(transactionBodyProto);
            var tokenAssociateTransactionFromBytes = ITransaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }
        [Fact]
        public virtual void TokenAssociateTransactionFromSignedTransactionBytes()
        {
            var tokenAssociateTransactionBodyProto = new Proto.Services.TokenAssociateTransactionBody { };
            var transactionBodyProto = new Proto.Services.TransactionBody { TokenAssociate = tokenAssociateTransactionBodyProto };
            var signedTransactionProto = new Proto.Services.SignedTransaction { BodyBytes = transactionBodyProto.ToByteString() };
            var signedTransactionBodyProto = Proto.Services.TransactionBody.Parser.ParseFrom(signedTransactionProto.BodyBytes);
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(signedTransactionBodyProto);
            var tokenAssociateTransactionFromBytes = ITransaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }
        [Fact]
        public virtual void TokenAssociateTransactionFromTransactionBytes()
        {
            var tokenAssociateTransactionBodyProto = new Proto.Services.TokenAssociateTransactionBody
            { };
            var transactionBodyProto = new Proto.Services.TransactionBody
            {
                TokenAssociate = tokenAssociateTransactionBodyProto
            };
            var signedTransactionProto = new Proto.Services.SignedTransaction
            {
                BodyBytes = transactionBodyProto.ToByteString()
            };
            var signedTransactionBodyProto = Proto.Services.TransactionBody.Parser.ParseFrom(signedTransactionProto.BodyBytes);
            var transactionSignedProto = new Proto.Services.Transaction
            {
                SignedTransactionBytes = signedTransactionBodyProto.ToByteString()
            };
            var transactionSignedBodyProto = Proto.Services.TransactionBody.Parser.ParseFrom(transactionSignedProto.SignedTransactionBytes);
            TokenAssociateTransaction tokenAssociateTransaction = SpawnTestTransaction(transactionSignedBodyProto);
            var tokenAssociateTransactionFromBytes = ITransaction.FromBytes(tokenAssociateTransaction.ToBytes());
            Assert.IsType<TokenAssociateTransaction>(tokenAssociateTransactionFromBytes);
        }

        private TokenAssociateTransaction SpawnTestTransaction(Proto.Services.TransactionBody txBody)
        {
            return new TokenAssociateTransaction(txBody)
            {
                NodeAccountIds = [..testNodeAccountIds],
                TransactionId = TransactionId.WithValidStart(testAccountId, validStart)

            }.Freeze().Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void SameSizeForIdenticalTransactions()
        {
            var accountCreateTransaction = new AccountCreateTransaction
            {
				InitialBalance = new Hbar(2),
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            var accountCreateTransaction2 = new AccountCreateTransaction
            {
				InitialBalance = new Hbar(2),
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            Assert.Equal(accountCreateTransaction.GetTransactionSize(), accountCreateTransaction2.GetTransactionSize());
        }
        [Fact]
        public virtual void SignedTransactionShouldHaveLargerSize()
        {
            var accountCreateTransaction = new AccountCreateTransaction
            {
				InitialBalance = new Hbar(2),
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze().Sign(PrivateKey.GenerateECDSA());
            var accountCreateTransaction2 = new AccountCreateTransaction
            {
				InitialBalance = new Hbar(2),
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            Assert.True(accountCreateTransaction.GetTransactionSize() > accountCreateTransaction2.GetTransactionSize());
        }
        [Fact]
        public virtual void TransactionWithLargerContentShouldHaveLargerTransactionBody()
        {
            var fileCreateTransactionSmallContent = new FileCreateTransaction
            {
				Contents_String = "smallBody",
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            var fileCreateTransactionLargeContent = new FileCreateTransaction
            {
				Contents_String = "largeLargeBody",
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            Assert.True(fileCreateTransactionSmallContent.GetTransactionBodySize() < fileCreateTransactionLargeContent.GetTransactionBodySize());
        }
        [Fact]
        public virtual void TransactionWithoutOptionalFieldsShouldHaveSmallerTransactionBody()
        {
            var noOptionalFieldsTransaction = new AccountCreateTransaction
            {
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			}.Freeze();
            var fullOptionalFieldsTransaction = new AccountCreateTransaction
            {
				InitialBalance = new Hbar(2),
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
				MaxTransactionFee = new Hbar(1),
				TransactionValidDuration = TimeSpan.FromHours(1),
			
            }.Freeze();
            Assert.True(noOptionalFieldsTransaction.GetTransactionBodySize() < fullOptionalFieldsTransaction.GetTransactionBodySize());
        }
        [Fact]
        public virtual void MultiChunkTransactionShouldReturnArrayOfBodySizes()
        {
            var chunkSize = 1024;
            byte[] content = new byte[chunkSize * 3];
            Array.Fill(content, (byte)'a');
            var fileAppentTx = new FileAppendTransaction
            {
				FileId = new FileId(1),
				ChunkSize = chunkSize,
				Contents = ByteString.CopyFrom(content),
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
 
            var objects = fileAppentTx.BodySizeAllChunks();
            Assert.NotNull(objects);
            Assert.Equal(2, objects.Count);
        }
        [Fact]
        public virtual void SingleChunkTransactionShouldReturnArrayOfOneSize()
        {

            // Small enough for one chunk
            byte[] smallContent = new byte[500];
            Array.Fill(smallContent, (byte)'a');
            var fileAppendTx = new FileAppendTransaction
            {
				FileId = new FileId(1),
				Contents = ByteString.CopyFrom(smallContent),
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            var bodySizes = fileAppendTx.BodySizeAllChunks();
            Assert.NotNull(bodySizes);
            Assert.Single(bodySizes);
        }
        [Fact]
        public virtual void TransactionWithNoContentShouldReturnSingleBodyChunk()
        {
            var fileAppendTx = new FileAppendTransaction
            {
				FileId = new FileId(1),
				TransactionId = new TransactionId(testAccountId, validStart),
				Contents_String = " ",
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            var bodySizes = fileAppendTx.BodySizeAllChunks();
            Assert.NotNull(bodySizes);
            Assert.Single(bodySizes); // Contains one empty chunk
        }
        [Fact]
        public virtual void ChunkedFileAppendTransactionShouldReturnProperSizes()
        {
            byte[] largeContent = new byte[2048];
            Array.Fill(largeContent, (byte)'a');
            var largeFileAppendTx = new FileAppendTransaction
            {
				FileId = new FileId(1),
				Contents = ByteString.CopyFrom(largeContent),
				ChunkSize = 1024,
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            long largeSize = largeFileAppendTx.GetTransactionSize();
            byte[] smallContent = new byte[512];
            Array.Fill(smallContent, (byte)'a');
            var smallFileAppendTx = new FileAppendTransaction
            {
				FileId = new FileId(1),
				Contents = ByteString.CopyFrom(smallContent),
				TransactionId = new TransactionId(testAccountId, validStart),
				NodeAccountIds = [..testNodeAccountIds],
			
            }.Freeze();
            long smallSize = smallFileAppendTx.GetTransactionSize();

            // Since large content is 2KB and chunk size is 1KB, this should create 2 chunks
            // Size should be greater than single chunk size
            Assert.True(largeSize > 1024);

            // The larger chunked transaction should be bigger than the small single-chunk transaction
            Assert.True(largeSize > smallSize);
        }
        [Fact]
        public virtual void TestAddSignatureV2SingleNodeSingleChunk()
        {
            var transaction = new FileAppendTransaction
            {
                FileId = fileID,
                Contents_Bytes = Encoding.UTF8.GetBytes("test content"),
                NodeAccountIds = [nodeAccountID1],
                TransactionId = testTransactionID,
                ChunkSize = 2048,

            }.FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            Dictionary<AccountId, Dictionary<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            Assert.Single(signatures);
            Assert.True(signatures.ContainsKey(nodeAccountID1));
            Dictionary<PublicKey, byte[]> nodeSignatures = signatures[nodeAccountID1];
            foreach (KeyValuePair<PublicKey, byte[]> entry in nodeSignatures)
            {
                Assert.Equal(entry.Value, mockSignature);
            }
        }
        [Fact]
        public virtual void TestAddSignatureV2MultipleNodesSingleChunk()
        {
            var transaction = new FileAppendTransaction
            {
                FileId = fileID,
                Contents_Bytes = Encoding.UTF8.GetBytes("test content"),
                NodeAccountIds = [..nodeAccountIDs],
                TransactionId = testTransactionID,
                ChunkSize = 2048,

            }.FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID2);
            Dictionary<AccountId, Dictionary<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            Assert.Equal(2, signatures.Count);
            Assert.True(signatures.ContainsKey(nodeAccountID1));
            Assert.True(signatures.ContainsKey(nodeAccountID2));
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                Dictionary<PublicKey, byte[]> nodeSignatures = signatures[nodeID];
                foreach (KeyValuePair<PublicKey, byte[]> entry in nodeSignatures)
                {
                    Assert.Equal(entry.Value, mockSignature);
                }
            }
        }
        [Fact]
        public virtual void TestAddSignatureV2MultipleNodesMultipleChunks()
        {
            byte[] content = new byte[2048];
            for (int i = 0; i < content.Length; i++)
            {
                content[i] = (byte)(i % 256);
            }

            var transaction = new FileAppendTransaction
            {
                FileId = fileID,
                Contents_Bytes = content,
                NodeAccountIds = [..nodeAccountIDs],
                TransactionId = testTransactionID,
                ChunkSize = 2048,

            }.FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID2);
            Dictionary<AccountId, Dictionary<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            Assert.Equal(2, signatures.Count);
            Assert.True(signatures.ContainsKey(nodeAccountID1));
            Assert.True(signatures.ContainsKey(nodeAccountID2));
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                Dictionary<PublicKey, byte[]> nodeSigs = signatures[nodeID];
                Assert.Single(nodeSigs);
                foreach (KeyValuePair<PublicKey, byte[]> entry in nodeSigs)
                {
                    Assert.NotNull(entry.Key);
                    Assert.Equal(entry.Key.ToString(), mockPrivateKey.GetPublicKey().ToString());
                    Assert.Equal(entry.Value, mockSignature);
                }
            }
        }
        [Fact]
        public virtual void TestAddSignatureV2WrongNodeID()
        {
            var transaction = new FileAppendTransaction
            {
                FileId = fileID,
                Contents_Bytes = Encoding.UTF8.GetBytes("test content"),
                NodeAccountIds = [..nodeAccountIDs],
                TransactionId = testTransactionID,
                ChunkSize = 2048,

            }.FreezeWith(client);
            AccountId invalidNodeID = AccountId.FromString("0.0.999");
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, invalidNodeID);
            Dictionary<AccountId, Dictionary<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            Assert.False(signatures.ContainsKey(invalidNodeID));
        }
        [Fact]
        public virtual void TestAddSignatureV2WrongTransactionID()
        {
            var transaction = new FileAppendTransaction
            {
                FileId = fileID,
                Contents_Bytes = Encoding.UTF8.GetBytes("test content"),
                NodeAccountIds = [..nodeAccountIDs],
                TransactionId = testTransactionID,
                ChunkSize = 2048,
            
            }.FreezeWith(client);
            TransactionId invalidTxID = TransactionId.WithValidStart(AccountId.FromString("0.0.999"), DateTimeOffset.UtcNow);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, invalidTxID, nodeAccountID1);
            Dictionary<AccountId, Dictionary<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            if (signatures.ContainsKey(nodeAccountID1))
            {
                Assert.False(signatures[nodeAccountID1].ContainsKey(mockPrivateKey.GetPublicKey()));
            }
        }
        [Fact]
        public virtual void TestAddSignatureV2SameSignatureTwice()
        {
            var transaction = new FileAppendTransaction
            {
                FileId = fileID,
                Contents_Bytes = Encoding.UTF8.GetBytes("test content"),
                NodeAccountIds = [nodeAccountID1],
                TransactionId = testTransactionID,
                ChunkSize = 2048,

            }.FreezeWith(client);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            transaction = transaction.AddSignature(mockPrivateKey.GetPublicKey(), mockSignature, testTransactionID, nodeAccountID1);
            Dictionary<AccountId, Dictionary<PublicKey, byte[]>> signatures = transaction.GetSignatures();
            Assert.Single(signatures);
            Assert.True(signatures.ContainsKey(nodeAccountID1));
            Dictionary<PublicKey, byte[]> nodeSigs = signatures[nodeAccountID1];
            Assert.Single(nodeSigs);
            foreach (KeyValuePair<PublicKey, byte[]> entry in nodeSigs)
            {
                Assert.Equal(entry.Value, mockSignature);
            }
        }
        [Fact]
        public virtual void TestAddSignatureV2WithEmptyInnerSignedTransactions()
        {
            var tx = new FileAppendTransaction();
            PrivateKey key = PrivateKey.GenerateED25519();
            byte[] mockSig = new byte[]
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
        [Fact]
        public virtual void TestGetSignableNodeBodyBytesListUnfrozen()
        {
            var tx = new TransferTransaction();
            Exception exception = Assert.Throws<Exception>(() =>
            {
                tx.GetSignableNodeBodyBytesList();
            });
        }
        [Fact]
        public virtual void TestGetSignableNodeBodyBytesListBasic()
        {
            var tx = new TransferTransaction
            {
                NodeAccountIds = [nodeAccountID1],
                TransactionId = testTransactionID,
            }
            .AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1))
            .AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1))
            .FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            Assert.NotEmpty(list);
            Assert.Single(list); // Should have one entry for our single node
            Assert.Equal(list[0].NodeID, nodeAccountID1);
            Assert.Equal(list[0].TransactionID, testTransactionID);
            Assert.NotEmpty(list[0].Body);
        }
        [Fact]
        public virtual void TestGetSignableNodeBodyBytesListContents()
        {
            var tx = new TransferTransaction
            {
                NodeAccountIds = [nodeAccountID1],
                TransactionId = testTransactionID,
            }
            .AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1))
            .AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1))
            .FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            Proto.Services.TransactionBody body = Proto.Services.TransactionBody.Parser.ParseFrom(list[0].Body);
            Assert.NotNull(body.CryptoTransfer);
            Assert.Equal(AccountId.FromProtobuf(body.NodeAccountId).ToString(), nodeAccountID1.ToString());
            Assert.Equal(TransactionId.FromProtobuf(body.TransactionId).ToString(), testTransactionID.ToString());
        }
        [Fact]
        public virtual void TestGetSignableNodeBodyBytesListMultipleNodeIDs()
        {
            var tx = new TransferTransaction
            {
                NodeAccountIds = [.. nodeAccountIDs],
                TransactionId = testTransactionID,
            }
            .AddHbarTransfer(AccountId.FromString("0.0.2"), Hbar.From(-1))
            .AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.From(1))
            .FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            Assert.Equal(2, tx.GetHbarTransfers().Count); // Should have two entries, one per node
            for (int i = 0; i < nodeAccountIDs.Count; i++)
            {
                AccountId nodeID = nodeAccountIDs[i];
                Assert.Equal(list[i].NodeID, nodeID);
                Assert.Equal(list[i].TransactionID, testTransactionID);
                Assert.NotEmpty(list[i].Body);

                // Verify body contents
                Proto.Services.TransactionBody body = Proto.Services.TransactionBody.Parser.ParseFrom(list[i].Body);
                Assert.NotNull(body.CryptoTransfer);
                Assert.Equal(AccountId.FromProtobuf(body.NodeAccountId).ToString(), nodeID.ToString());
            }
        }
        [Fact]
        public virtual void TestGetSignableNodeBodyBytesListFileAppendMultipleChunks()
        {
            byte[] content = new byte[4096];
            for (int i = 0; i < content.Length; i++)
            {
                content[i] = (byte)(i % 256);
            }

            var tx = new FileAppendTransaction
            {
                NodeAccountIds = [..nodeAccountIDs],
                TransactionId = testTransactionID,
                FileId = new FileId(5),
                Contents_Bytes = content,
                ChunkSize = 2048,

            }.FreezeWith(client);
            List<Transaction.SignableNodeTransactionBodyBytes> list = tx.GetSignableNodeBodyBytesList();
            Assert.Equal(2, list.Count); // Should have 4 entries: 2 nodes * 2 chunks

            // Map to track transaction IDs per node
            Dictionary<string, Dictionary<string, bool>> txIDsByNode = [];
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                txIDsByNode.Add(nodeID.ToString(), []);
            }

            for (int i = 0; i < list.Count; i++)
            {
                Assert.True(nodeAccountIDs.Contains(list[i].NodeID));
                Assert.NotNull(list[i].TransactionID);
                Assert.NotEmpty(list[i].Body);
                string nodeIDStr = list[i].NodeID.ToString();
                string txIDStr = list[i].TransactionID.ToString();

                // Each transaction ID should appear exactly once per node
                Assert.False(txIDsByNode[nodeIDStr].ContainsKey(txIDStr), "Duplicate transaction ID found for the same node");
                txIDsByNode[nodeIDStr].Add(txIDStr, true);
                Proto.Services.TransactionBody body = Proto.Services.TransactionBody.Parser.ParseFrom(list[i].Body);
                Assert.NotNull(body.FileAppend);
                Assert.Equal(AccountId.FromProtobuf(body.NodeAccountId).ToString(), list[i].NodeID.ToString());
            }


            // Verify each node has the same number of unique transaction IDs
            foreach (AccountId nodeID in nodeAccountIDs)
            {
                Assert.Equal(2, tx.TransactionIds.Count);
            }


            // Verify that all nodes have the same set of transaction IDs
            Dictionary<string, bool> firstNodeTxIDs = txIDsByNode[nodeAccountID1.ToString()];
            for (int i = 1; i < nodeAccountIDs.Count; i++)
            {
                Dictionary<string, bool> nodeTxIDs = txIDsByNode[nodeAccountIDs[i].ToString()];
                foreach (string txID in firstNodeTxIDs.Keys)
                {
                    Assert.True(nodeTxIDs.ContainsKey(txID), "All nodes should have the same set of transaction IDs");
                }
            }
        }
    }
}