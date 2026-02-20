// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.File;

using Google.Protobuf.WellKnownTypes;

using System;
using System.Text;
using System.Threading;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class FileAppendIntegrationTest
    {
        public virtual void CanAppendToFile()
        {

            // There are potential bugs in FileAppendTransaction which require more than one node to trigger.
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")
				
                }.Execute(testEnv.Client);

                var fileId = response.GetReceipt(testEnv.Client).FileId;
                var info = new FileInfoQuery
                {
					FileId = fileId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 28);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);

                new FileAppendTransaction
                {
					FileId = fileId,
					Contents = Encoding.UTF8.GetBytes("[e2e::FileAppendTransaction]")
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                info = new FileInfoQuery { FileId = fileId, }.Execute(testEnv.Client);

                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 56);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);
                
                new FileDeleteTransaction
                {
					FileId = fileId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanAppendLargeContentsToFile()
        {

            // There are potential bugs in FileAppendTransaction which require more than one node to trigger.
            using (var testEnv = new IntegrationTestEnv(2))
            {

                // Skip if using local node.
                // Note: this check should be removed once the local node is supporting multiple nodes.
                testEnv.AssumeNotLocalNode();
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
                new FileAppendTransaction
                {
					FileId = fileId,
                    Contents = Encoding.UTF8.GetBytes(Contents.BIG_CONTENTS)
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var contents = new FileContentsQuery { FileId = fileId, }.Execute(testEnv.Client);

                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]" + Contents.BIG_CONTENTS);
                
                info = new FileInfoQuery
                {
					FileId = fileId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 13522);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);

                new FileDeleteTransaction
                {
					FileId = fileId,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanAppendLargeContentsToFileDespiteExpiration()
        {

            // There are potential bugs in FileAppendTransaction which require more than one node to trigger.
            using (var testEnv = new IntegrationTestEnv(2))
            {

                // Skip if using local node.
                // Note: this check should be removed once the local node is supporting multiple nodes.
                testEnv.AssumeNotLocalNode();
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")
				
                }.Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId;
                
                Thread.Sleep(5000);
                
                var info = new FileInfoQuery
                {
					FileId = fileId,
				
                }.Execute(testEnv.Client);
                
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 28);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);
                
                var appendTx = new FileAppendTransaction
                {
					FileId = fileId,
                    Contents = Encoding.UTF8.GetBytes(Contents.BIG_CONTENTS),
					TransactionValidDuration = Duration.FromTimeSpan(TimeSpan.FromSeconds(25))

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var contents = new FileContentsQuery { FileId = fileId, }.Execute(testEnv.Client);
                
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]" + Contents.BIG_CONTENTS);
                
                info = new FileInfoQuery
                {
					FileId = fileId,
				
                }.Execute(testEnv.Client);
                
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 13522);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Null(info.Keys.Threshold);
                Assert.Equal(info.Keys, [testEnv.OperatorKey]);

                new FileDeleteTransaction { FileId = fileId, }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanFileAppendSignForMultipleNodes()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var newKey = PrivateKey.GenerateED25519();
                var createTransaction = new FileCreateTransaction
                {
					Keys = newKey.GetPublicKey(),
					NodeAccountIds = testEnv.Client.Network_.Network_Read.Keys,
					Contents = "Hello",
					TransactionMemo = "java sdk e2e tests",

				}.FreezeWith(testEnv.Client).Sign(newKey);
                var createResponse = createTransaction.Execute(testEnv.Client);
                var createReceipt = createResponse.GetReceipt(testEnv.Client);
                var fileId = createReceipt.FileId;
                Assert.NotNull(fileId);

                // Create file append transaction with large content that will be chunked
                var appendTransaction = new FileAppendTransaction
                {
					FileId = fileId,
                    Contents = Encoding.UTF8.GetBytes(Contents.BIG_CONTENTS)

				}.FreezeWith(testEnv.Client);

                var signableBodyList = appendTransaction.GetSignableNodeBodyBytesList();
                Assert.NotEmpty(signableBodyList);

                foreach (var signableBody in signableBodyList)
                {
                    // External signing simulation (like HSM)
                    byte[] signature = newKey.Sign(signableBody.Body);

                    // Add signature back to transaction using addSignature
                    appendTransaction = appendTransaction.AddSignature(newKey.GetPublicKey(), signature, signableBody.TransactionID, signableBody.NodeID);
                }

                // Step 6: Execute the file append transaction
                var appendResponse = appendTransaction.Execute(testEnv.Client);
                var appendReceipt = appendResponse.GetReceipt(testEnv.Client);
                
                Assert.Equal(appendReceipt.Status, ResponseStatus.Success);
                
                var contents = new FileContentsQuery
                {
					FileId = fileId,
					NodeAccountIds = [appendResponse.NodeId]
				
                }.Execute(testEnv.Client);

                var expectedContent = "Hello" + Contents.BIG_CONTENTS;
                
                Assert.Equal(contents.ToStringUtf8(), expectedContent);
                Assert.Equal(contents.Length, expectedContent.Length);
                
                var info = new FileInfoQuery { FileId = fileId, }.Execute(testEnv.Client);
                
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, expectedContent.Length);
                Assert.False(info.IsDeleted);
                Assert.NotNull(info.Keys);
                Assert.Equal(info.Keys, [newKey.GetPublicKey()]);

                // Cleanup - delete the file
                new FileDeleteTransaction
                {
					FileId = fileId,
				
                }.FreezeWith(testEnv.Client).Sign(newKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
    }
}