// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class ReceiptQueryIntegrationTest
    {
        public virtual void CanGetTransactionReceipt()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.Client);
                var receipt = new TransactionReceiptQuery { TransactionId = response.TransactionId }.Execute(testEnv.Client);
            }
        }

        public virtual void CanGetTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.Client);
                new TransactionReceiptQuery { TransactionId = response.TransactionId }.Execute(testEnv.Client);
                new TransactionRecordQuery { TransactionId = response.TransactionId }.Execute(testEnv.Client);
            }
        }

        public virtual void GetCostTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.Client);
                new TransactionReceiptQuery { TransactionId = response.TransactionId }.Execute(testEnv.Client);
                var recordQuery = new TransactionRecordQuery { TransactionId = response.TransactionId };
                recordQuery.GetCost(testEnv.Client);
                recordQuery.Execute(testEnv.Client);
            }
        }

        public virtual void GetCostBigMaxTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.Client);
                new TransactionReceiptQuery { TransactionId = response.TransactionId }.Execute(testEnv.Client);
                var recordQuery = new TransactionRecordQuery { TransactionId = response.TransactionId, MaxQueryPayment = new Hbar(1000) };
                recordQuery.GetCost(testEnv.Client);
                recordQuery.Execute(testEnv.Client);
            }
        }

        public virtual void GetCostSmallMaxTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.Client);
                var receipt = new TransactionReceiptQuery { TransactionId = response.TransactionId }.Execute(testEnv.Client);
                var recordQuery = new TransactionRecordQuery { TransactionId = response.TransactionId, MaxQueryPayment = Hbar.FromTinybars(1) };
                var cost = recordQuery.GetCost(testEnv.Client);
                Assert.Throws(typeof(Exception), () =>
                {
                    recordQuery.Execute(testEnv.Client);

                }).WithMessage("cost for TransactionRecordQuery, of " + cost.ToString() + ", without explicit payment is greater than the maximum allowed payment of 1 tℏ");
            }
        }

        public virtual void GetCostInsufficientTxFeeTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.Client);
                var receipt = new TransactionReceiptQuery { TransactionId = response.TransactionId }.Execute(testEnv.Client);
                var recordQuery = new TransactionRecordQuery { TransactionId = response.TransactionId };
                Assert.Throws<PrecheckStatusException>(() =>
                {
                    recordQuery.QueryPayment = Hbar.FromTinybars(1);
					recordQuery.Execute(testEnv.Client);

                }).Satisfies((error) => Assert.Equal(error.status.ToString(), "INSUFFICIENT_TX_FEE"));
            }
        }
    }
}