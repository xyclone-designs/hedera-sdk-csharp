// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text;

using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Exceptions;

using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class LargeTransactionSystemAccountIntegrationTest
    {
        private static readonly int LARGE_CONTENT_SIZE_BYTES = 100 * 1024;
        private static readonly int SIZE_THRESHOLD_BYTES = 6 * 1024;
        private static readonly int EXTENDED_SIZE_LIMIT_BYTES = 130 * 1024;

        [Fact]
        public virtual void PrivilegedSystemAccountCanCreateLargeFile()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {
                var largeContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Array.Fill(largeContents, (byte)1);
                var transaction = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = largeContents,
					TransactionMemo = "HIP-1300 create large file",
					MaxTransactionFee = new Hbar(20)
				};
                transaction.FreezeWith(testEnv.Client);
                
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);
                
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.NotNull(receipt.FileId);
                
                new FileDeleteTransaction { FileId = receipt.FileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        [Fact]
        public virtual void PrivilegedSystemAccountCanUpdateLargeFile()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {
                var initialContents = Encoding.UTF8.GetBytes("test");
                var fileId = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = initialContents,
					TransactionMemo = "HIP-1300 initial file"
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).FileId;
                
                var updatedContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Array.Fill(updatedContents, (byte)2);
                var transaction = new FileUpdateTransaction
                {
					FileId = fileId,
					Contents = ByteString.CopyFrom(updatedContents),
					TransactionMemo = "HIP-1300 update large file",
					MaxTransactionFee = new Hbar(20)
				};

                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);
                transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                new FileDeleteTransaction { FileId = fileId, }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        [Fact]
        public virtual void PrivilegedSystemAccountCanAppendLargeFile()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {
                // 1.  Create a small file
                var fileId = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("start")

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).FileId;
                
                // 2.  Append large content - need to set max chunks for content over default limit
                byte[] largeContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Array.Fill(largeContents, (byte)3);

                var transaction = new FileAppendTransaction
                {
					FileId = fileId,
					Contents = ByteString.CopyFrom(largeContents),
					MaxChunks = 100,
					MaxTransactionFee = new Hbar(20),
					TransactionMemo = "HIP-1300 append large file"
				};
                
                transaction.FreezeWith(testEnv.Client);
                Assert.True(transaction.ToBytes().Length < EXTENDED_SIZE_LIMIT_BYTES);
                Assert.True(transaction.ToBytes().Length > SIZE_THRESHOLD_BYTES);
                transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                new FileDeleteTransaction { FileId = fileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        [Fact]
        public virtual void NonPrivilegedAccountCannotCreateLargeFile()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount(new Hbar(50)))
            {
                // useThrowawayAccount creates a non-privileged account for testing
                // This ensures the test runs even if OPERATOR_ID is 0.0.2 or 0.0.50
                var largeContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Array.Fill(largeContents, (byte)1);
                var transaction = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = largeContents,
					TransactionMemo = "Should fail - too large for non-privileged",
					MaxTransactionFee = new Hbar(20)
				};
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    transaction.FreezeWith(testEnv.Client);
                    transaction.Execute(testEnv.Client);
                });

                Assert.Equal(exception.Status, ResponseStatus.TransactionOversize);
            }
        }

        [Fact]
        public virtual void NonPrivilegedAccountCannotCreateAccountWithLargeKeyList()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount(new Hbar(50)))
            {
                // useThrowawayAccount creates a non-privileged account for testing
                // This ensures the test runs even if OPERATOR_ID is 0.0.2 or 0.0.50
                // Generate 180 key pairs to ensure transaction size exceeds 6KB
                var NumberOfKeys = 180;
                var publicKeys = new PublicKey[NumberOfKeys];
                for (int i = 0; i < NumberOfKeys; i++)
                {
                    var key = PrivateKey.GenerateED25519();
                    publicKeys[i] = key.GetPublicKey();
                }


                // Create a KeyList with all public keys
                var keyList = new KeyList();
                foreach (var publicKey in publicKeys)
                {
                    keyList.Add(publicKey);
                }

                var transaction = new AccountCreateTransaction
                {
					Key = keyList,
					InitialBalance = new Hbar(1),
					TransactionMemo = "Should fail - too large for non-privileged",
					MaxTransactionFee = new Hbar(20),
				};

                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    transaction.FreezeWith(testEnv.Client);
                    transaction.Execute(testEnv.Client);
                });
                Assert.Equal(exception.Status, ResponseStatus.TransactionExpired);
            }
        }

        [Fact]
        public virtual void PrivilegedAccountAtNear130KBLimit()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {

                // Create content that will result in transaction just under 130KB
                var contentSize = 128 * 1024; // 128KB content
                var largeContents = new byte[contentSize];
                Array.Fill(largeContents, (byte)1);

                var transaction = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = largeContents,
					TransactionMemo = "HIP-1300 near limit test",
					MaxTransactionFee = new Hbar(20)
				};
                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();

                // Should be less than 130KB total
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);

                // But definitely over 6KB
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.NotNull(receipt.FileId);
                
                new FileDeleteTransaction { FileId = receipt.FileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        [Fact]
        public virtual void NonPrivilegedAccountCanCreateSmallFile()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount(new Hbar(50)))
            {

                // useThrowawayAccount creates a non-privileged account for testing
                // This ensures the test runs even if OPERATOR_ID is 0.0.2 or 0.0.50
                // Small content that stays well under 6KB
                var smallContents = new byte[2 * 1024]; // 2KB
                Array.Fill(smallContents, (byte)1);

                var transaction = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = smallContents,
					TransactionMemo = "Small file test",
					MaxTransactionFee = new Hbar(5)
				};
                transaction.FreezeWith(testEnv.Client);
                
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length < SIZE_THRESHOLD_BYTES);
                
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.NotNull(receipt.FileId);
                
                new FileDeleteTransaction { FileId = receipt.FileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        [Fact]
        public virtual void TreasuryAccountCanCreateLargeFile()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {
                // This test specifically validates 0.0.2 if that's the operator
                Assert.True(testEnv.OperatorId.Num == 2, "Test requires treasury account 0.0.2");
                var largeContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                
                Array.Fill(largeContents, (byte)1);
                var transaction = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = largeContents,
					TransactionMemo = "HIP-1300 test with 0.0.2",
					MaxTransactionFee = new Hbar(20)
				};
                transaction.FreezeWith(testEnv.Client);
                
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);
                
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                Assert.NotNull(receipt.FileId);
                
                new FileDeleteTransaction { FileId = receipt.FileId }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        [Fact]
        public virtual void PrivilegedSystemAccountCanCreateAccountWithLargeKeyList()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {

                // Generate 180 key pairs to ensure transaction size exceeds 6KB
                // With 100 keys we got ~3709 bytes, so 180 keys should give us ~6676 bytes
                var NumberOfKeys = 180;
                var privateKeys = new PrivateKey[NumberOfKeys];
                var publicKeys = new PublicKey[NumberOfKeys];
                for (int i = 0; i < NumberOfKeys; i++)
                {
                    var key = PrivateKey.GenerateED25519();
                    privateKeys[i] = key;
                    publicKeys[i] = key.GetPublicKey();
                }


                // Create a KeyList with all public keys
                var keyList = new KeyList();
                foreach (var publicKey in publicKeys)
                {
                    keyList.Add(publicKey);
                }

                var transaction = new AccountCreateTransaction
                {
					Key = keyList,
					InitialBalance = new Hbar(1),
					TransactionMemo = "HIP-1300 create account with large KeyList",
					MaxTransactionFee = new Hbar(20)
				};
                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountId = receipt.AccountId;
                Assert.NotNull(accountId);
            }
        }

        private IntegrationTestEnv CreateSystemAccountTestEnv()
        {
            var testEnv = new IntegrationTestEnv(1);
            if (!IsPrivilegedSystemAccount(testEnv.OperatorId))
            {
                var systemAccountId = Environment.GetEnvironmentVariable("SYSTEM_ACCOUNT_ID");
                var systemAccountKey = Environment.GetEnvironmentVariable("SYSTEM_ACCOUNT_KEY");

                if (string.IsNullOrWhiteSpace(systemAccountId) is not false && string.IsNullOrWhiteSpace(systemAccountKey) is not false)
                {
                    var accountId = AccountId.FromString(systemAccountId);
                    var privateKey = PrivateKey.FromString(systemAccountKey);

                    testEnv.Client.OperatorSet(accountId, privateKey);
                    testEnv.OperatorId = accountId;
                    testEnv.OperatorKey = privateKey.GetPublicKey();
                }
            }

            Assert.True(IsPrivilegedSystemAccount(testEnv.OperatorId), "System account credentials (0.0.2 or 0.0.50) are required for large transaction tests");
            return testEnv;
        }

        private bool IsPrivilegedSystemAccount(AccountId accountId)
        {
            return accountId != null && accountId.Shard == 0 && accountId.Realm == 0 && (accountId.Num == 2 || accountId.Num == 50);
        }
    }
}