// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Hedera.Hashgraph;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class LargeTransactionSystemAccountIntegrationTest
    {
        private static readonly int LARGE_CONTENT_SIZE_BYTES = 100 * 1024;
        private static readonly int SIZE_THRESHOLD_BYTES = 6 * 1024;
        private static readonly int EXTENDED_SIZE_LIMIT_BYTES = 130 * 1024;
        public virtual void PrivilegedSystemAccountCanCreateLargeFile()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {
                var largeContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Arrays.Fill(largeContents, (byte)1);
                var transaction = new FileCreateTransaction()Keys = [testEnv.OperatorKey],.SetContents(largeContents).SetTransactionMemo("HIP-1300 create large file").SetMaxTransactionFee(new Hbar(20));
                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.NotNull(receipt.fileId);
                new FileDeleteTransaction().SetFileId(receipt.fileId)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void PrivilegedSystemAccountCanUpdateLargeFile()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {
                var initialContents = "test".GetBytes();
                var fileId = new FileCreateTransaction()Keys = [testEnv.OperatorKey],.SetContents(initialContents).SetTransactionMemo("HIP-1300 initial file").Execute(testEnv.Client).GetReceipt(testEnv.Client).fileId;
                fileId);
                var updatedContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Arrays.Fill(updatedContents, (byte)2);
                var transaction = new FileUpdateTransaction()FileId = fileId,.SetContents(updatedContents).SetTransactionMemo("HIP-1300 update large file").SetMaxTransactionFee(new Hbar(20));
                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);
                transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new FileDeleteTransaction()FileId = fileId,.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void PrivilegedSystemAccountCanAppendLargeFile()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {

                // 1.  Create a small file
                var fileId = new FileCreateTransaction()Keys = [testEnv.OperatorKey],.SetContents("start".GetBytes()).Execute(testEnv.Client).GetReceipt(testEnv.Client).fileId;
                fileId);

                // 2.  Append large content - need to set max chunks for content over default limit
                var largeContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Arrays.Fill(largeContents, (byte)3);
                var transaction = new FileAppendTransaction()FileId = fileId,.SetContents(largeContents).SetMaxChunks(100).SetMaxTransactionFee(new Hbar(20)).SetTransactionMemo("HIP-1300 append large file");
                transaction.FreezeWith(testEnv.Client);
                Assert.True(transaction.ToBytes().Length < EXTENDED_SIZE_LIMIT_BYTES);
                Assert.True(transaction.ToBytes().Length > SIZE_THRESHOLD_BYTES);
                transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new FileDeleteTransaction()FileId = fileId,.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void NonPrivilegedAccountCannotCreateLargeFile()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount(new Hbar(50)))
            {

                // useThrowawayAccount creates a non-privileged account for testing
                // This ensures the test runs even if OPERATOR_ID is 0.0.2 or 0.0.50
                var largeContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Arrays.Fill(largeContents, (byte)1);
                var transaction = new FileCreateTransaction()Keys = [testEnv.OperatorKey],.SetContents(largeContents).SetTransactionMemo("Should fail - too large for non-privileged").SetMaxTransactionFee(new Hbar(20));
                var exception = PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    transaction.FreezeWith(testEnv.Client);
                    transaction.Execute(testEnv.Client);
                });
                Assert.Equal(exception.status, Status.TRANSACTION_OVERSIZE);
            }
        }

        public virtual void NonPrivilegedAccountCannotCreateAccountWithLargeKeyList()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount(new Hbar(50)))
            {

                // useThrowawayAccount creates a non-privileged account for testing
                // This ensures the test runs even if OPERATOR_ID is 0.0.2 or 0.0.50
                // Generate 180 key pairs to ensure transaction size exceeds 6KB
                var numberOfKeys = 180;
                var publicKeys = new PublicKey[numberOfKeys];
                for (int i = 0; i < numberOfKeys; i++)
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

                var transaction = new AccountCreateTransaction()Key = keyList,InitialBalance = new Hbar(1),.SetTransactionMemo("Should fail - too large for non-privileged").SetMaxTransactionFee(new Hbar(20));
                var exception = PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    transaction.FreezeWith(testEnv.Client);
                    transaction.Execute(testEnv.Client);
                });
                Assert.Equal(exception.status, Status.TRANSACTION_OVERSIZE);
            }
        }

        public virtual void PrivilegedAccountAtNear130KBLimit()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {

                // Create content that will result in transaction just under 130KB
                var contentSize = 128 * 1024; // 128KB content
                var largeContents = new byte[contentSize];
                Arrays.Fill(largeContents, (byte)1);
                var transaction = new FileCreateTransaction()Keys = [testEnv.OperatorKey],.SetContents(largeContents).SetTransactionMemo("HIP-1300 near limit test").SetMaxTransactionFee(new Hbar(20));
                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();

                // Should be less than 130KB total
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);

                // But definitely over 6KB
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.NotNull(receipt.fileId);
                new FileDeleteTransaction().SetFileId(receipt.fileId)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void NonPrivilegedAccountCanCreateSmallFile()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount(new Hbar(50)))
            {

                // useThrowawayAccount creates a non-privileged account for testing
                // This ensures the test runs even if OPERATOR_ID is 0.0.2 or 0.0.50
                // Small content that stays well under 6KB
                var smallContents = new byte[2 * 1024]; // 2KB
                Arrays.Fill(smallContents, (byte)1);
                var transaction = new FileCreateTransaction()Keys = [testEnv.OperatorKey],.SetContents(smallContents).SetTransactionMemo("Small file test").SetMaxTransactionFee(new Hbar(5));
                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length < SIZE_THRESHOLD_BYTES);
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.NotNull(receipt.fileId);
                new FileDeleteTransaction().SetFileId(receipt.fileId)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void TreasuryAccountCanCreateLargeFile()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {

                // This test specifically validates 0.0.2 if that's the operator
                Assumptions.AssumeTrue(testEnv.OperatorId.num == 2, "Test requires treasury account 0.0.2");
                var largeContents = new byte[LARGE_CONTENT_SIZE_BYTES];
                Arrays.Fill(largeContents, (byte)1);
                var transaction = new FileCreateTransaction()Keys = [testEnv.OperatorKey],.SetContents(largeContents).SetTransactionMemo("HIP-1300 test with 0.0.2").SetMaxTransactionFee(new Hbar(20));
                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.NotNull(receipt.fileId);
                new FileDeleteTransaction().SetFileId(receipt.fileId)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void PrivilegedSystemAccountCanCreateAccountWithLargeKeyList()
        {
            using (var testEnv = CreateSystemAccountTestEnv())
            {

                // Generate 180 key pairs to ensure transaction size exceeds 6KB
                // With 100 keys we got ~3709 bytes, so 180 keys should give us ~6676 bytes
                var numberOfKeys = 180;
                var privateKeys = new PrivateKey[numberOfKeys];
                var publicKeys = new PublicKey[numberOfKeys];
                for (int i = 0; i < numberOfKeys; i++)
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

                var transaction = new AccountCreateTransaction()Key = keyList,InitialBalance = new Hbar(1),.SetTransactionMemo("HIP-1300 create account with large KeyList").SetMaxTransactionFee(new Hbar(20));
                transaction.FreezeWith(testEnv.Client);
                var serialized = transaction.ToBytes();
                Assert.True(serialized.Length > SIZE_THRESHOLD_BYTES);
                Assert.True(serialized.Length < EXTENDED_SIZE_LIMIT_BYTES);
                var receipt = transaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountId = receipt.accountId);
                Assert.NotNull(accountId);
            }
        }

        private IntegrationTestEnv CreateSystemAccountTestEnv()
        {
            var testEnv = new IntegrationTestEnv(1);
            if (!IsPrivilegedSystemAccount(testEnv.OperatorId))
            {
                var systemAccountId = System.GetProperty("SYSTEM_ACCOUNT_ID");
                var systemAccountKey = System.GetProperty("SYSTEM_ACCOUNT_KEY");
                if (systemAccountId != null && !systemAccountId.IsBlank() && systemAccountKey != null && !systemAccountKey.IsBlank())
                {
                    var accountId = AccountId.FromString(systemAccountId);
                    var privateKey = PrivateKey.FromString(systemAccountKey);
                    testEnv.Client.OperatorSet(accountId, privateKey);
                    testEnv.OperatorId = accountId;
                    testEnv.OperatorKey = privateKey.GetPublicKey();
                }
            }

            Assumptions.AssumeTrue(IsPrivilegedSystemAccount(testEnv.OperatorId), "System account credentials (0.0.2 or 0.0.50) are required for large transaction tests");
            return testEnv;
        }

        private bool IsPrivilegedSystemAccount(AccountId accountId)
        {
            return accountId != null && accountId.shard == 0 && accountId.realm == 0 && (accountId.num == 2 || accountId.num == 50);
        }
    }
}