// SPDX-License-Identifier: Apache-2.0
using System.Text;

using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
	public class FileContentsIntegrationTest
	{
		public virtual void CanQueryFileContents()
		{
			using (var testEnv = new IntegrationTestEnv(1))
			{
				var response = new FileCreateTransaction
				{
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")

				}.Execute(testEnv.Client);

				var fileId = response.GetReceipt(testEnv.Client).FileId;
				var contents = new FileContentsQuery { FileId = fileId, }.Execute(testEnv.Client);

				Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]");

				new FileDeleteTransaction { FileId = fileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
			}
		}

		public virtual void CanQueryEmptyFileContents()
		{
			using (var testEnv = new IntegrationTestEnv(1))
			{
				var response = new FileCreateTransaction
				{
					Keys = [testEnv.OperatorKey],

				}.Execute(testEnv.Client);
				var fileId = response.GetReceipt(testEnv.Client).FileId;
				var contents = new FileContentsQuery { FileId = fileId, }.Execute(testEnv.Client);

				Assert.Equal(contents.Length, 0);

				new FileDeleteTransaction { FileId = fileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
			}
		}

		public virtual void CannotQueryFileContentsWhenFileIDIsNotSet()
		{
			using (var testEnv = new IntegrationTestEnv(1))
			{
				PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
				{
					new FileContentsQuery().Execute(testEnv.Client);
				});

				Assert.Contains(ResponseStatus.InvalidFileId.ToString(), exception.Message);
			}
		}

		public virtual void GetCostBigMaxQueryFileContents()
		{
			using (var testEnv = new IntegrationTestEnv(1))
			{
				var response = new FileCreateTransaction
				{
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")

				}.Execute(testEnv.Client);

				var fileId = response.GetReceipt(testEnv.Client).FileId;
				var contentsQuery = new FileContentsQuery
				{
					FileId = fileId,
					MaxQueryPayment = new Hbar(1000)
				};
				var contents = contentsQuery.Execute(testEnv.Client);

				Assert.Equal(contents.ToStringUtf8(), "[e2e::FileCreateTransaction]");

				new FileDeleteTransaction { FileId = fileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
			}
		}

		public virtual void GetCostSmallMaxQueryFileContents()
		{
			using (var testEnv = new IntegrationTestEnv(1))
			{
				var response = new FileCreateTransaction
				{
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")

				}.Execute(testEnv.Client);

				var fileId = response.GetReceipt(testEnv.Client).FileId;
				var contentsQuery = new FileContentsQuery
				{
					FileId = fileId,
					MaxQueryPayment = Hbar.FromTinybars(1)
				};

				MaxQueryPaymentExceededException exception = Assert.Throws<MaxQueryPaymentExceededException>(() =>
				{
					contentsQuery.Execute(testEnv.Client);
				});

				new FileDeleteTransaction { FileId = fileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
			}
		}

		public virtual void GetCostInsufficientTxFeeQueryFileContents()
		{
			using (var testEnv = new IntegrationTestEnv(1))
			{
				var response = new FileCreateTransaction
				{
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("[e2e::FileCreateTransaction]")

				}.Execute(testEnv.Client);

				var fileId = response.GetReceipt(testEnv.Client).FileId;
				var contentsQuery = new FileContentsQuery
				{
					FileId = fileId,
					MaxQueryPayment = new Hbar(100)
				};
				PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
				{
					contentsQuery.QueryPayment = Hbar.FromTinybars(1);
					contentsQuery.Execute(testEnv.Client);
				});

				Assert.Equal(exception.Status.ToString(), "INSUFFICIENT_TX_FEE");

				new FileDeleteTransaction { FileId = fileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
			}
		}
	}
}