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
        virtual void CanAppendToFile()
        {

            // There are potential bugs in FileAppendTransaction which require more than one node to trigger.
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                new FileAppendTransaction().SetFileId(fileId).SetContents("[e2e::FileAppendTransaction]").Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 56);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanAppendLargeContentsToFile()
        {

            // There are potential bugs in FileAppendTransaction which require more than one node to trigger.
            using (var testEnv = new IntegrationTestEnv(2))
            {

                // Skip if using local node.
                // Note: this check should be removed once the local node is supporting multiple nodes.
                testEnv.AssumeNotLocalNode();
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
                new FileAppendTransaction().SetFileId(fileId).SetContents(Contents.BIG_CONTENTS).Execute(testEnv.client).GetReceipt(testEnv.client);
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

        virtual void CanAppendLargeContentsToFileDespiteExpiration()
        {

            // There are potential bugs in FileAppendTransaction which require more than one node to trigger.
            using (var testEnv = new IntegrationTestEnv(2))
            {

                // Skip if using local node.
                // Note: this check should be removed once the local node is supporting multiple nodes.
                testEnv.AssumeNotLocalNode();
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
                var appendTx = new FileAppendTransaction().SetFileId(fileId).SetContents(Contents.BIG_CONTENTS).SetTransactionValidDuration(Duration.OfSeconds(25)).Execute(testEnv.client).GetReceipt(testEnv.client);
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

        virtual void CanFileAppendSignForMultipleNodes()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var newKey = PrivateKey.GenerateED25519();
                var createTransaction = new FileCreateTransaction().SetKeys(newKey.GetPublicKey()).SetNodeAccountIds(testEnv.client.GetNetwork().Values().Stream().ToList()).SetContents("Hello").SetTransactionMemo("java sdk e2e tests").FreezeWith(testEnv.client).Sign(newKey);
                var createResponse = createTransaction.Execute(testEnv.client);
                var createReceipt = createResponse.GetReceipt(testEnv.client);
                var fileId = Objects.RequireNonNull(createReceipt.fileId);
                AssertThat(fileId).IsNotNull();

                // Create file append transaction with large content that will be chunked
                var appendTransaction = new FileAppendTransaction().SetFileId(fileId).SetContents(Contents.BIG_CONTENTS).FreezeWith(testEnv.client);
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
                var appendResponse = appendTransaction.Execute(testEnv.client);
                var appendReceipt = appendResponse.GetReceipt(testEnv.client);
                Assert.Equal(appendReceipt.status, Status.SUCCESS);
                var contents = new FileContentsQuery().SetFileId(fileId).SetNodeAccountIds(Arrays.AsList(appendResponse.nodeId)).Execute(testEnv.client);
                var expectedContent = "Hello" + Contents.BIG_CONTENTS;
                Assert.Equal(contents.ToStringUtf8(), expectedContent);
                Assert.Equal(contents.Count, expectedContent.Length);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, expectedContent.Length);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                Assert.Equal(info.keys, KeyList.Of(newKey.GetPublicKey()));

                // Cleanup - delete the file
                new FileDeleteTransaction().SetFileId(fileId).FreezeWith(testEnv.client).Sign(newKey).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}