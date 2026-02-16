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
    public class FileUpdateIntegrationTest
    {
        public virtual void CanUpdateFile()
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
                new FileUpdateTransaction().SetFileId(fileId).SetContents("[e2e::FileUpdateTransaction]").Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNotNull();
                AssertThat(info.keys.GetThreshold()).IsNull();
                Assert.Equal(info.keys, KeyList.Of(testEnv.operatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotUpdateImmutableFile()
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
                    new FileUpdateTransaction().SetFileId(fileId).SetContents("[e2e::FileUpdateTransaction]").Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.UNAUTHORIZED.ToString());
            }
        }

        public virtual void CannotUpdateFileWhenFileIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new FileUpdateTransaction().SetContents("[e2e::FileUpdateTransaction]").Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_FILE_ID.ToString());
            }
        }

        public virtual void CanUpdateFeeScheduleFile()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                testEnv.client.SetOperator(new AccountId(0, 0, 2), PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137"));
                var fileId = new FileId(0, 0, 111);
                var receipt = new FileUpdateTransaction().SetFileId(fileId).SetContents("[e2e::FileUpdateTransaction]").Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.FEE_SCHEDULE_FILE_PART_UPLOADED);
            }
        }
    }
}