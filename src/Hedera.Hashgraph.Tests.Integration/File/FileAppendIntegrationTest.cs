// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class FileAppendIntegrationTest
    {
        public virtual void CanAppendToFile()
        {

            // There are potential bugs in FileAppendTransaction which require more than one node to trigger.
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Null(info.keys.GetThreshold());
                Assert.Equal(info.keys, KeyList.Of(testEnv.OperatorKey));
                new FileAppendTransaction().SetFileId(fileId).SetContents("[e2e::FileAppendTransaction]").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 56);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Null(info.keys.GetThreshold());
                Assert.Equal(info.keys, KeyList.Of(testEnv.OperatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
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
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                Thread.Sleep(5000);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Null(info.keys.GetThreshold());
                Assert.Equal(info.keys, KeyList.Of(testEnv.OperatorKey));
                new FileAppendTransaction().SetFileId(fileId).SetContents(Contents.BIG_CONTENTS).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var contents = new FileContentsQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]" + Contents.BIG_CONTENTS);
                info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 13522);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Null(info.keys.GetThreshold());
                Assert.Equal(info.keys, KeyList.Of(testEnv.OperatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
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
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                Thread.Sleep(5000);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Null(info.keys.GetThreshold());
                Assert.Equal(info.keys, KeyList.Of(testEnv.OperatorKey));
                var appendTx = new FileAppendTransaction().SetFileId(fileId).SetContents(Contents.BIG_CONTENTS).SetTransactionValidDuration(Duration.OfSeconds(25)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var contents = new FileContentsQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]" + Contents.BIG_CONTENTS);
                info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 13522);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Null(info.keys.GetThreshold());
                Assert.Equal(info.keys, KeyList.Of(testEnv.OperatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanFileAppendSignForMultipleNodes()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var newKey = PrivateKey.GenerateED25519();
                var createTransaction = new FileCreateTransaction().SetKeys(newKey.GetPublicKey()).SetNodeAccountIds(testEnv.Client.Network.Values().Stream().ToList()).SetContents("Hello").SetTransactionMemo("java sdk e2e tests").FreezeWith(testEnv.Client).Sign(newKey);
                var createResponse = createTransaction.Execute(testEnv.Client);
                var createReceipt = createResponse.GetReceipt(testEnv.Client);
                var fileId = createReceipt.fileId);
                Assert.NotNull(fileId);

                // Create file append transaction with large content that will be chunked
                var appendTransaction = new FileAppendTransaction().SetFileId(fileId).SetContents(Contents.BIG_CONTENTS).FreezeWith(testEnv.Client);
                var signableBodyList = appendTransaction.GetSignableNodeBodyBytesList();
                Assert.NotEmpty(signableBodyList);
                foreach (var signableBody in signableBodyList)
                {

                    // External signing simulation (like HSM)
                    byte[] signature = newKey.Sign(signableBody.GetBody());

                    // Add signature back to transaction using addSignature
                    appendTransaction = appendTransaction.AddSignature(newKey.GetPublicKey(), signature, signableBody.GetTransactionID(), signableBody.GetNodeID());
                }


                // Step 6: Execute the file append transaction
                var appendResponse = appendTransaction.Execute(testEnv.Client);
                var appendReceipt = appendResponse.GetReceipt(testEnv.Client);
                Assert.Equal(appendReceipt.status, ResponseStatus.Success);
                var contents = new FileContentsQuery().SetFileId(fileId).SetNodeAccountIds(Arrays.AsList(appendResponse.nodeId)).Execute(testEnv.Client);
                var expectedContent = "Hello" + Contents.BIG_CONTENTS;
                Assert.Equal(contents.ToStringUtf8(), expectedContent);
                Assert.Equal(contents.Count, expectedContent.Length);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, expectedContent.Length);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Equal(info.keys, KeyList.Of(newKey.GetPublicKey()));

                // Cleanup - delete the file
                new FileDeleteTransaction().SetFileId(fileId).FreezeWith(testEnv.Client).Sign(newKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
    }
}