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
    public class FileInfoIntegrationTest
    {
        public virtual void CanQueryFileInfo()
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

        public virtual void CanQueryFileInfoWithNoAdminKeyOrContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var info = new FileInfoQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(info.fileId, fileId);
                Assert.Equal(info.size, 0);
                AssertThat(info.isDeleted).IsFalse();
                AssertThat(info.keys).IsNull();
            }
        }

        public virtual void GetCostBigMaxQueryFileInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var infoQuery = new FileInfoQuery().SetFileId(fileId).SetMaxQueryPayment(new Hbar(1000));
                var cost = infoQuery.GetCost(testEnv.client);
                var info = infoQuery.SetQueryPayment(cost).Execute(testEnv.client);
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void GetCostSmallMaxQueryFileInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var infoQuery = new FileInfoQuery().SetFileId(fileId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                Assert.Throws(typeof(MaxQueryPaymentExceededException), () =>
                {
                    infoQuery.Execute(testEnv.client);
                });
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void GetCostInsufficientTxFeeQueryFileInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var infoQuery = new FileInfoQuery().SetFileId(fileId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    infoQuery.SetQueryPayment(Hbar.FromTinybars(1)).Execute(testEnv.client);
                }).Satisfies((error) => Assert.Equal(error.status.ToString(), "INSUFFICIENT_TX_FEE"));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}