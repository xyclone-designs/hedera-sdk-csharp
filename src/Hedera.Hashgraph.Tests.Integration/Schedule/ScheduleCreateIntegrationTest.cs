// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Nio.Charset;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class ScheduleCreateIntegrationTest
    {
        private readonly int oneDayInSecs = 86400;
        public virtual void CanCreateSchedule()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(10));
                var response = new ScheduleCreateTransaction().SetScheduledTransaction(transaction).SetAdminKey(testEnv.operatorKey).SetPayerAccountId(testEnv.operatorId).Execute(testEnv.client);
                var scheduleId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).scheduleId);
                var info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);
                AssertThat(info.executedAt).IsNotNull();
            }
        }

        public virtual void CanGetTransactionSchedule()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(10));
                var response = new ScheduleCreateTransaction().SetScheduledTransaction(transaction).SetAdminKey(testEnv.operatorKey).SetPayerAccountId(testEnv.operatorId).Execute(testEnv.client);
                var scheduleId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).scheduleId);
                var info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);
                AssertThat(info.executedAt).IsNotNull();
                AssertThat(info.GetScheduledTransaction()).IsNotNull();
            }
        }

        public virtual void CanCreateWithSchedule()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(10));
                var tx = transaction.Schedule();
                var response = tx.SetAdminKey(testEnv.operatorKey).SetPayerAccountId(testEnv.operatorId).Execute(testEnv.client);
                var scheduleId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).scheduleId);
                var info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);
                AssertThat(info.executedAt).IsNotNull();
                AssertThat(info.GetScheduledTransaction()).IsNotNull();
            }
        }

        public virtual void CanSignSchedule2()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                PrivateKey key2 = PrivateKey.GenerateED25519();
                PrivateKey key3 = PrivateKey.GenerateED25519();
                KeyList keyList = new KeyList();
                keyList.Add(key1.GetPublicKey());
                keyList.Add(key2.GetPublicKey());
                keyList.Add(key3.GetPublicKey());

                // Creat the account with the `KeyList`
                TransactionResponse response = new AccountCreateTransaction().SetKeyWithoutAlias(keyList).SetInitialBalance(new Hbar(10)).Execute(testEnv.client);

                // This will wait for the receipt to become available
                TransactionReceipt receipt = response.GetReceipt(testEnv.client);
                AccountId accountId = Objects.RequireNonNull(receipt.accountId);

                // Generate a `TransactionId`. This id is used to query the inner scheduled transaction
                // after we expect it to have been executed
                TransactionId transactionId = TransactionId.Generate(testEnv.operatorId);

                // Create a transfer transaction with 2/3 signatures.
                TransferTransaction transfer = new TransferTransaction().SetTransactionId(transactionId).AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.operatorId, new Hbar(1));

                // Schedule the transaction
                ScheduleCreateTransaction scheduled = transfer.Schedule();
                receipt = scheduled.Execute(testEnv.client).GetReceipt(testEnv.client);

                // Get the schedule ID from the receipt
                ScheduleId scheduleId = Objects.RequireNonNull(receipt.scheduleId);

                // Get the schedule info to see if `signatories` is populated with 2/3 signatures
                ScheduleInfo info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);
                AssertThat(info.executedAt).IsNull();

                // Finally send this last signature to Hedera. This last signature _should_ mean the transaction executes
                // since all 3 signatures have been provided.
                ScheduleSignTransaction signTransaction = new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client);
                signTransaction.Sign(key1).Sign(key2).Sign(key3).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);
                AssertThat(info.executedAt).IsNotNull();
                AssertThat(scheduleId.GetChecksum()).IsNull();
                AssertThat(scheduleId.GetHashCode()).IsNotZero();
                AssertThat(scheduleId.CompareTo(ScheduleId.FromBytes(scheduleId.ToBytes()))).IsZero();
                new AccountDeleteTransaction().SetAccountId(accountId).SetTransferAccountId(testEnv.operatorId).FreezeWith(testEnv.client).Sign(key1).Sign(key2).Sign(key3).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanScheduleTokenTransfer()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(10)).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                Objects.RequireNonNull(accountId);
                var tokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetInitialSupply(100).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                Objects.RequireNonNull(tokenId);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                var scheduleId = new TransferTransaction().AddTokenTransfer(tokenId, testEnv.operatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Schedule().Execute(testEnv.client).GetReceipt(testEnv.client).scheduleId;
                Objects.RequireNonNull(scheduleId);
                var balanceQuery1 = new AccountBalanceQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(balanceQuery1.tokens[tokenId], 0);
                new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                var balanceQuery2 = new AccountBalanceQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(balanceQuery2.tokens[tokenId], 10);
            }
        }

        public virtual void CannotScheduleTwoTransactions()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetInitialBalance(new Hbar(10)).SetKeyWithoutAlias(key).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var transferTx = new TransferTransaction().AddHbarTransfer(testEnv.operatorId, new Hbar(-10)).AddHbarTransfer(accountId, new Hbar(10));
                var scheduleId1 = transferTx.Schedule().Execute(testEnv.client).GetReceipt(testEnv.client).scheduleId;
                var info1 = new ScheduleInfoQuery().SetScheduleId(scheduleId1).Execute(testEnv.client);
                AssertThat(info1.executedAt).IsNotNull();
                var transferTxFromInfo = info1.GetScheduledTransaction();
                var scheduleCreateTx1 = transferTx.Schedule();
                var scheduleCreateTx2 = transferTxFromInfo.Schedule();
                Assert.Equal(scheduleCreateTx2.ToString(), scheduleCreateTx1.ToString());
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    transferTxFromInfo.Schedule().Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("IDENTICAL_SCHEDULE_ALREADY_CREATED");
            }
        }

        public virtual void CanScheduleTopicMessage()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Generate 3 random keys
                var key1 = PrivateKey.GenerateED25519();

                // This is the submit key
                var key2 = PrivateKey.GenerateED25519();
                var key3 = PrivateKey.GenerateED25519();
                var keyList = new KeyList();
                keyList.Add(key1.GetPublicKey());
                keyList.Add(key2.GetPublicKey());
                keyList.Add(key3.GetPublicKey());
                var response = new AccountCreateTransaction().SetInitialBalance(new Hbar(100)).SetKeyWithoutAlias(keyList).Execute(testEnv.client);
                AssertThat(response.GetReceipt(testEnv.client).accountId).IsNotNull();
                var topicId = Objects.RequireNonNull(new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetAutoRenewAccountId(testEnv.operatorId).SetTopicMemo("HCS Topic_").SetSubmitKey(key2.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).topicId);
                var transaction = new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMessage("scheduled hcs message".GetBytes(StandardCharsets.UTF_8));

                // create schedule
                var scheduledTx = transaction.Schedule().SetAdminKey(testEnv.operatorKey).SetPayerAccountId(testEnv.operatorId).SetScheduleMemo("mirror scheduled E2E signature on create and sign_" + DateTimeOffset.UtcNow);
                var scheduled = scheduledTx.FreezeWith(testEnv.client);
                var scheduleId = Objects.RequireNonNull(scheduled.Execute(testEnv.client).GetReceipt(testEnv.client).scheduleId);

                // verify schedule has been created and has 1 of 2 signatures
                var info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);
                AssertThat(info).IsNotNull();
                Assert.Equal(info.scheduleId, scheduleId);
                var infoTransaction = (TopicMessageSubmitTransaction)info.GetScheduledTransaction();
                Assert.Equal(transaction.GetTopicId(), infoTransaction.GetTopicId());
                Assert.Equal(transaction.GetNodeAccountIds(), infoTransaction.GetNodeAccountIds());
                var scheduleSign = new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client);
                scheduleSign.Sign(key2).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);
                AssertThat(info.executedAt).IsNotNull();
            }
        }

        public virtual void CanSignSchedule()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(key.GetPublicKey()).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.operatorId, new Hbar(1));

                // Schedule the transaction
                var scheduleId = transfer.Schedule().SetExpirationTime(DateTimeOffset.UtcNow.PlusSeconds(oneDayInSecs)).SetScheduleMemo("HIP-423 Integration Test").Execute(testEnv.client).GetReceipt(testEnv.client).scheduleId;
                ScheduleInfo info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is not yet executed
                AssertThat(info.executedAt).IsNull();

                // Schedule sign
                new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is executed
                AssertThat(info.executedAt).IsNotNull();
                AssertThat(scheduleId.GetChecksum()).IsNull();
                AssertThat(scheduleId.GetHashCode()).IsNotZero();
                AssertThat(scheduleId.CompareTo(ScheduleId.FromBytes(scheduleId.ToBytes()))).IsZero();
            }
        }

        public virtual void CannotScheduleTransactionOneYearIntoTheFuture()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(key.GetPublicKey()).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.operatorId, new Hbar(1));

                // Schedule the transaction
                Assert.Throws(typeof(ReceiptStatusException), () => transfer.Schedule().SetExpirationTime(DateTimeOffset.UtcNow.Plus(Duration.OfDays(365))).SetScheduleMemo("HIP-423 Integration Test").Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.SCHEDULE_EXPIRATION_TIME_TOO_FAR_IN_FUTURE.ToString());
            }
        }

        public virtual void CannotScheduleTransactionInThePast()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(key.GetPublicKey()).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.operatorId, new Hbar(1));

                // Schedule the transaction
                Assert.Throws(typeof(ReceiptStatusException), () => transfer.Schedule().SetExpirationTime(DateTimeOffset.UtcNow.MinusSeconds(10)).SetScheduleMemo("HIP-423 Integration Test").Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.SCHEDULE_EXPIRATION_TIME_MUST_BE_HIGHER_THAN_CONSENSUS_TIME.ToString());
            }
        }

        public virtual void CanSignScheduleAndWaitForExpiry()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(key.GetPublicKey()).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.operatorId, new Hbar(1));

                // Schedule the transaction
                var scheduleId = transfer.Schedule().SetExpirationTime(DateTimeOffset.UtcNow.PlusSeconds(oneDayInSecs)).SetWaitForExpiry(true).SetScheduleMemo("HIP-423 Integration Test").Execute(testEnv.client).GetReceipt(testEnv.client).scheduleId;
                ScheduleInfo info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is not yet executed
                AssertThat(info.executedAt).IsNull();

                // Schedule sign
                new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is still not executed
                AssertThat(info.executedAt).IsNull();
                AssertThat(scheduleId.GetChecksum()).IsNull();
                AssertThat(scheduleId.GetHashCode()).IsNotZero();
                AssertThat(scheduleId.CompareTo(ScheduleId.FromBytes(scheduleId.ToBytes()))).IsZero();
            }
        }

        public virtual void CanSignWithMultiSigAndUpdateSigningRequirements()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                PrivateKey key2 = PrivateKey.GenerateED25519();
                PrivateKey key3 = PrivateKey.GenerateED25519();
                PrivateKey key4 = PrivateKey.GenerateED25519();
                KeyList keyList = KeyList.WithThreshold(2);
                keyList.Add(key1.GetPublicKey());
                keyList.Add(key2.GetPublicKey());
                keyList.Add(key3.GetPublicKey());
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(keyList).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.operatorId, new Hbar(1));

                // Schedule the transaction
                var scheduleId = transfer.Schedule().SetExpirationTime(DateTimeOffset.UtcNow.PlusSeconds(oneDayInSecs)).SetScheduleMemo("HIP-423 Integration Test").Execute(testEnv.client).GetReceipt(testEnv.client).scheduleId;
                ScheduleInfo info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is not executed
                AssertThat(info.executedAt).IsNull();

                // Sign with one key
                new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client).Sign(key1).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is still not executed
                AssertThat(info.executedAt).IsNull();

                // Update the signing requirements
                new AccountUpdateTransaction().SetAccountId(accountId).SetKey(key4.GetPublicKey()).FreezeWith(testEnv.client).Sign(key1).Sign(key2).Sign(key4).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is still not executed
                AssertThat(info.executedAt).IsNull();

                // Sign with the updated key
                new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client).Sign(key4).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is executed
                AssertThat(info.executedAt).IsNotNull();
            }
        }

        public virtual void CanSignWithMultiSig()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                PrivateKey key2 = PrivateKey.GenerateED25519();
                PrivateKey key3 = PrivateKey.GenerateED25519();
                KeyList keyList = KeyList.WithThreshold(2);
                keyList.Add(key1.GetPublicKey());
                keyList.Add(key2.GetPublicKey());
                keyList.Add(key3.GetPublicKey());
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(keyList).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.operatorId, new Hbar(1));

                // Schedule the transaction
                var scheduleId = transfer.Schedule().SetExpirationTime(DateTimeOffset.UtcNow.PlusSeconds(oneDayInSecs)).SetScheduleMemo("HIP-423 Integration Test").Execute(testEnv.client).GetReceipt(testEnv.client).scheduleId;
                ScheduleInfo info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is not executed
                AssertThat(info.executedAt).IsNull();

                // Sign with one key
                new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client).Sign(key1).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is still not executed
                AssertThat(info.executedAt).IsNull();

                // Update the signing requirements
                new AccountUpdateTransaction().SetAccountId(accountId).SetKey(key1.GetPublicKey()).FreezeWith(testEnv.client).Sign(key1).Sign(key2).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is still not executed
                AssertThat(info.executedAt).IsNull();

                // Sign with one more key
                new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client).Sign(key2).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is executed
                AssertThat(info.executedAt).IsNotNull();
            }
        }

        public virtual void CanExecuteWithShortExpirationTime()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(key1).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Create the transaction
                TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(accountId, new Hbar(1).Negated()).AddHbarTransfer(testEnv.operatorId, new Hbar(1));

                // Schedule the transaction
                var scheduleId = transfer.Schedule().SetExpirationTime(DateTimeOffset.UtcNow.PlusSeconds(10)).SetWaitForExpiry(true).SetScheduleMemo("HIP-423 Integration Test").Execute(testEnv.client).GetReceipt(testEnv.client).scheduleId;
                ScheduleInfo info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is not executed
                AssertThat(info.executedAt).IsNull();

                // Sign
                new ScheduleSignTransaction().SetScheduleId(scheduleId).FreezeWith(testEnv.client).Sign(key1).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new ScheduleInfoQuery().SetScheduleId(scheduleId).Execute(testEnv.client);

                // Verify the transaction is still not executed
                AssertThat(info.executedAt).IsNull();
                var accountBalanceBefore = new AccountBalanceQuery().SetAccountId(accountId).Execute(testEnv.client);
                Thread.Sleep(10000);
                var accountBalanceAfter = new AccountBalanceQuery().SetAccountId(accountId).Execute(testEnv.client);

                // Verify the transaction executed after 10 seconds
                Assert.Equal(accountBalanceBefore.hbars.CompareTo(accountBalanceAfter.hbars), 1);
            }
        }
    }
}