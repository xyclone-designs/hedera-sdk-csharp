// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class ReceiptQueryIntegrationTest
    {
        public virtual void CanGetTransactionReceipt()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                var receipt = new TransactionReceiptQuery().SetTransactionId(response.transactionId).Execute(testEnv.client);
            }
        }

        public virtual void CanGetTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                new TransactionReceiptQuery().SetTransactionId(response.transactionId).Execute(testEnv.client);
                new TransactionRecordQuery().SetTransactionId(response.transactionId).Execute(testEnv.client);
            }
        }

        public virtual void GetCostTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                new TransactionReceiptQuery().SetTransactionId(response.transactionId).Execute(testEnv.client);
                var recordQuery = new TransactionRecordQuery().SetTransactionId(response.transactionId);
                recordQuery.GetCost(testEnv.client);
                recordQuery.Execute(testEnv.client);
            }
        }

        public virtual void GetCostBigMaxTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                new TransactionReceiptQuery().SetTransactionId(response.transactionId).Execute(testEnv.client);
                var recordQuery = new TransactionRecordQuery().SetTransactionId(response.transactionId).SetMaxQueryPayment(new Hbar(1000));
                recordQuery.GetCost(testEnv.client);
                recordQuery.Execute(testEnv.client);
            }
        }

        public virtual void GetCostSmallMaxTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                var receipt = new TransactionReceiptQuery().SetTransactionId(response.transactionId).Execute(testEnv.client);
                var recordQuery = new TransactionRecordQuery().SetTransactionId(response.transactionId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                var cost = recordQuery.GetCost(testEnv.client);
                Assert.Throws(typeof(Exception), () =>
                {
                    recordQuery.Execute(testEnv.client);
                }).WithMessage("cost for TransactionRecordQuery, of " + cost.ToString() + ", without explicit payment is greater than the maximum allowed payment of 1 tℏ");
            }
        }

        public virtual void GetCostInsufficientTxFeeTransactionRecord()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                var receipt = new TransactionReceiptQuery().SetTransactionId(response.transactionId).Execute(testEnv.client);
                var recordQuery = new TransactionRecordQuery().SetTransactionId(response.transactionId);
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    recordQuery.SetQueryPayment(Hbar.FromTinybars(1)).Execute(testEnv.client);
                }).Satisfies((error) => Assert.Equal(error.status.ToString(), "INSUFFICIENT_TX_FEE"));
            }
        }
    }
}