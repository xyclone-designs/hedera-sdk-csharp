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
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Null(info.keys.GetThreshold());
                Assert.Equal(info.keys, KeyList.Of(testEnv.OperatorKey));
                new FileUpdateTransaction().SetFileId(fileId).SetContents("[e2e::FileUpdateTransaction]").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 28);
                Assert.False(info.isDeleted);
                Assert.NotNull(info.keys);
                Assert.Null(info.keys.GetThreshold());
                Assert.Equal(info.keys, KeyList.Of(testEnv.OperatorKey));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotUpdateImmutableFile()
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
                    new FileUpdateTransaction().SetFileId(fileId).SetContents("[e2e::FileUpdateTransaction]").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.UNAUTHORIZED.ToString());
            }
        }

        public virtual void CannotUpdateFileWhenFileIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new FileUpdateTransaction().SetContents("[e2e::FileUpdateTransaction]").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_FILE_ID.ToString());
            }
        }

        public virtual void CanUpdateFeeScheduleFile()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                testEnv.Client.OperatorSet(new AccountId(0, 0, 2), PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137"));
                var fileId = new FileId(0, 0, 111);
                var receipt = new FileUpdateTransaction().SetFileId(fileId).SetContents("[e2e::FileUpdateTransaction]").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, Status.FEE_SCHEDULE_FILE_PART_UPLOADED);
            }
        }
    }
}