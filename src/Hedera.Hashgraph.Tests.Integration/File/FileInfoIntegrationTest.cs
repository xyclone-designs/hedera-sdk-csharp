// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;

using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class FileInfoIntegrationTest
    {
        public virtual void CanQueryFileInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]"),
				
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

                new FileDeleteTransaction
                {
					FileId = fileId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanQueryFileInfoWithNoAdminKeyOrContents()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId;
                var info = new FileInfoQuery
                {
					FileId = fileId,

				}.Execute(testEnv.Client);
                Assert.Equal(info.FileId, fileId);
                Assert.Equal(info.Size, 0);
                Assert.False(info.IsDeleted);
                Assert.Null(info.Keys);
            }
        }

        public virtual void GetCostBigMaxQueryFileInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]"),
				
                }.Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId;
                var infoQuery = new FileInfoQuery
                {
					FileId = fileId,
					MaxQueryPayment = new Hbar(1000),
				};

				infoQuery.QueryPayment = infoQuery.GetCost(testEnv.Client);
                var info = infoQuery.Execute(testEnv.Client);
                new FileDeleteTransaction
                {
					FileId = fileId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void GetCostSmallMaxQueryFileInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]"),
				
                }.Execute(testEnv.Client);

                var fileId = response.GetReceipt(testEnv.Client).FileId;
                var infoQuery = new FileInfoQuery
                {
					FileId = fileId,
					MaxQueryPayment = Hbar.FromTinybars(1),
				};

                MaxQueryPaymentExceededException exception = Assert.Throws<MaxQueryPaymentExceededException>(() =>
                {
                    infoQuery.Execute(testEnv.Client);
                });

                new FileDeleteTransaction
				{
					FileId = fileId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void GetCostInsufficientTxFeeQueryFileInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]"),
				
                }.Execute(testEnv.Client);

                var fileId = response.GetReceipt(testEnv.Client).FileId;
                var infoQuery = new FileInfoQuery
                {
					FileId = fileId,
					MaxQueryPayment = Hbar.FromTinybars(1)
				};

                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    infoQuery.QueryPayment = Hbar.FromTinybars(1);
                    infoQuery.Execute(testEnv.Client);
                });

                Assert.Equal(exception.Status.ToString(), "INSUFFICIENT_TX_FEE");

                new FileDeleteTransaction
                {
					FileId = fileId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
    }
}