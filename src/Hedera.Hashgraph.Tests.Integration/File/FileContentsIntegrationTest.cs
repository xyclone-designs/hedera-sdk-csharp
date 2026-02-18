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
    public class FileContentsIntegrationTest
    {
        public virtual void CanQueryFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                var contents = new FileContentsQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]");
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanQueryEmptyFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                var contents = new FileContentsQuery().SetFileId(fileId).Execute(testEnv.Client);
                Assert.Equal(contents.Count, 0);
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotQueryFileContentsWhenFileIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new FileContentsQuery().Execute(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_FILE_ID.ToString());
            }
        }

        public virtual void GetCostBigMaxQueryFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                var contentsQuery = new FileContentsQuery().SetFileId(fileId).SetMaxQueryPayment(new Hbar(1000));
                var contents = contentsQuery.Execute(testEnv.Client);
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]");
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void GetCostSmallMaxQueryFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                var contentsQuery = new FileContentsQuery().SetFileId(fileId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                Assert.Throws(typeof(MaxQueryPaymentExceededException), () =>
                {
                    contentsQuery.Execute(testEnv.Client);
                });
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void GetCostInsufficientTxFeeQueryFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                var contentsQuery = new FileContentsQuery().SetFileId(fileId).SetMaxQueryPayment(new Hbar(100));
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    contentsQuery.SetQueryPayment(Hbar.FromTinybars(1)).Execute(testEnv.Client);
                }).Satisfies((error) => Assert.Equal(error.status.ToString(), "INSUFFICIENT_TX_FEE"));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
    }
}