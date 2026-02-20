// SPDX-License-Identifier: Apache-2.0
using System.Text;

using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Exceptions;
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class FileUpdateIntegrationTest
    {
        public virtual void CanUpdateFile()
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
                
                new FileUpdateTransaction
                {
					FileId = fileId,
					Contents = ByteString.CopyFromUtf8("[e2e::FileUpdateTransaction]"),

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                info = new FileInfoQuery
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

        public virtual void CannotUpdateImmutableFile()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction
                {
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
                Assert.Null(info.Keys);

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new FileUpdateTransaction
                    {
						FileId = fileId,
						Contents = ByteString.CopyFromUtf8("[e2e::FileUpdateTransaction]"),
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.Unauthorized.ToString(), exception.Message);
            }
        }

        public virtual void CannotUpdateFileWhenFileIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new FileUpdateTransaction
                    {
						Contents = ByteString.CopyFromUtf8("[e2e::FileUpdateTransaction]"),
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidFileId.ToString(), exception.Message);
            }
        }

        public virtual void CanUpdateFeeScheduleFile()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                testEnv.Client.OperatorSet(new AccountId(0, 0, 2), PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137"));
                var fileId = new FileId(0, 0, 111);
                var receipt = new FileUpdateTransaction
                {
					FileId = fileId,
					Contents = ByteString.CopyFromUtf8("[e2e::FileUpdateTransaction]"),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                Assert.Equal(receipt.Status, ResponseStatus.FeeScheduleFilePartUploaded);
            }
        }
    }
}