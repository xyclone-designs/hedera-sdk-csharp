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
        virtual void CanQueryFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var contents = new FileContentsQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]");
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanQueryEmptyFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var contents = new FileContentsQuery().SetFileId(fileId).Execute(testEnv.client);
                Assert.Equal(contents.Count, 0);
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CannotQueryFileContentsWhenFileIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new FileContentsQuery().Execute(testEnv.client);
                }).WithMessageContaining(Status.INVALID_FILE_ID.ToString());
            }
        }

        virtual void GetCostBigMaxQueryFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var contentsQuery = new FileContentsQuery().SetFileId(fileId).SetMaxQueryPayment(new Hbar(1000));
                var contents = contentsQuery.Execute(testEnv.client);
                Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]");
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void GetCostSmallMaxQueryFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var contentsQuery = new FileContentsQuery().SetFileId(fileId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                AssertThatExceptionOfType(typeof(MaxQueryPaymentExceededException)).IsThrownBy(() =>
                {
                    contentsQuery.Execute(testEnv.client);
                });
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void GetCostInsufficientTxFeeQueryFileContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents("[e2e::FileCreateTransaction]").Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                var contentsQuery = new FileContentsQuery().SetFileId(fileId).SetMaxQueryPayment(new Hbar(100));
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    contentsQuery.SetQueryPayment(Hbar.FromTinybars(1)).Execute(testEnv.client);
                }).Satisfies((error) => Assert.Equal(error.status.ToString(), "INSUFFICIENT_TX_FEE"));
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}