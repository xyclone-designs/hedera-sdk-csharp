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
    class AccountInfoIntegrationTest
    {
        public virtual void CanQueryAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(info.accountId, testEnv.operatorId);
                AssertThat(info.isDeleted).IsFalse();
                Assert.Equal(info.key, testEnv.operatorKey);
                AssertThat(info.balance.ToTinybars()).IsGreaterThan(0);
                AssertThat(info.proxyAccountId).IsNull();
                Assert.Equal(info.proxyReceived, Hbar.ZERO);
            }
        }

        public virtual void GetCostAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery().SetAccountId(testEnv.operatorId).SetMaxQueryPayment(new Hbar(1));
                var cost = info.GetCost(testEnv.client);
                var accInfo = info.SetQueryPayment(cost).Execute(testEnv.client);
                Assert.Equal(accInfo.accountId, testEnv.operatorId);
            }
        }

        public virtual void GetCostBigMaxAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery().SetAccountId(testEnv.operatorId).SetMaxQueryPayment(Hbar.MAX);
                var cost = info.GetCost(testEnv.client);
                var accInfo = info.SetQueryPayment(cost).Execute(testEnv.client);
                Assert.Equal(accInfo.accountId, testEnv.operatorId);
            }
        }

        public virtual void GetCostSmallMaxAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery().SetAccountId(testEnv.operatorId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                var cost = info.GetCost(testEnv.client);
                Assert.Throws(typeof(Exception), () =>
                {
                    info.Execute(testEnv.client);
                }).WithMessage("com.hedera.hashgraph.sdk.MaxQueryPaymentExceededException: cost for AccountInfoQuery, of " + cost.ToString() + ", without explicit payment is greater than the maximum allowed payment of 1 tℏ");
            }
        }

        public virtual void GetCostInsufficientTxFeeAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery().SetAccountId(testEnv.operatorId).SetMaxQueryPayment(Hbar.FromTinybars(10000));
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    info.SetQueryPayment(Hbar.FromTinybars(1)).Execute(testEnv.client);
                }).Satisfies((error) => Assert.Equal(error.status.ToString(), "INSUFFICIENT_TX_FEE"));
            }
        }

        public virtual void AccountInfoFlowVerifyFunctions()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var newKey = PrivateKey.GenerateED25519();
                var newPublicKey = newKey.GetPublicKey();
                Transaction<TWildcardTodo> signedTx = new AccountCreateTransaction().SetKeyWithoutAlias(newPublicKey).SetInitialBalance(Hbar.FromTinybars(1000)).FreezeWith(testEnv.client).SignWithOperator(testEnv.client);
                Transaction<TWildcardTodo> unsignedTx = new AccountCreateTransaction().SetKeyWithoutAlias(newPublicKey).SetInitialBalance(Hbar.FromTinybars(1000)).FreezeWith(testEnv.client);
                AssertThat(AccountInfoFlow.VerifyTransactionSignature(testEnv.client, testEnv.operatorId, signedTx)).IsTrue();
                AssertThat(AccountInfoFlow.VerifyTransactionSignature(testEnv.client, testEnv.operatorId, unsignedTx)).IsFalse();
            }
        }
    }
}