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
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotDeleteImmutableFile()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNull();
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.UNAUTHORIZED.ToString());
            }
        }
    }
}