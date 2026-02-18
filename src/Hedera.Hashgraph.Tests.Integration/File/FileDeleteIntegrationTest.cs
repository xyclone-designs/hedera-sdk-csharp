// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class FileDeleteIntegrationTest
    {
        public virtual void CanDeleteFile()
        {
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
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotDeleteImmutableFile()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                Assert.False(info.isDeleted);
                Assert.Null(info.keys);
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.UNAUTHORIZED.ToString());
            }
        }
    }
}