// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Keys;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountInfoIntegrationTest
    {
        public virtual void CanQueryAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery
                {
                    AccountId = testEnv.OperatorId
                
                }.Execute(testEnv.Client);

                Assert.Equal(info.AccountId, testEnv.OperatorId);
                Assert.False(info.IsDeleted);
                Assert.Equal(info.Key, testEnv.OperatorKey);
                Assert.True(info.Balance.ToTinybars() > 0);
                Assert.Null(info.ProxyAccountId);
                Assert.Equal(info.ProxyReceived, Hbar.ZERO);
            }
        }
        public virtual void GetCostAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery
                {
                    AccountId = testEnv.OperatorId,
					MaxQueryPayment = new Hbar(1)
				};
                var cost = info.GetCost(testEnv.Client);
                info.QueryPayment = cost;
                var accInfo = info.Execute(testEnv.Client);
                
                Assert.Equal(accInfo.AccountId, testEnv.OperatorId);
            }
        }
        public virtual void GetCostBigMaxAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery
                {
                    AccountId = testEnv.OperatorId,
					MaxQueryPayment = Hbar.MAX
				};
                var cost = info.GetCost(testEnv.Client);
                info.QueryPayment = cost;
                var accInfo = info.Execute(testEnv.Client);
                
                Assert.Equal(accInfo.AccountId, testEnv.OperatorId);
            }
        }
        public virtual void GetCostSmallMaxAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery
                {
                    AccountId = testEnv.OperatorId,
					MaxQueryPayment = Hbar.FromTinybars(1)
				};
                var cost = info.GetCost(testEnv.Client);

                Exception exception = Assert.Throws<Exception>(() =>
                {
                    info.Execute(testEnv.Client);
                });

                Assert.Equal(exception.Message, "com.hedera.hashgraph.sdk.MaxQueryPaymentExceededException: cost for AccountInfoQuery, of " + cost.ToString() + ", without explicit payment is greater than the maximum allowed payment of 1 tℏ");
            }
        }
        public virtual void GetCostInsufficientTxFeeAccountInfoForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var info = new AccountInfoQuery()
                {
					AccountId = testEnv.OperatorId,
					MaxQueryPayment = Hbar.FromTinybars(10000),
				};

				PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    info.QueryPayment = Hbar.FromTinybars(1);
                    info.Execute(testEnv.Client);
                });

                Assert.Equal("INSUFFICIENT_TX_FEE", exception.Status.ToString());
			}
        }
        public virtual void AccountInfoFlowVerifyFunctions()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var newKey = PrivateKey.GenerateED25519();
                var newPublicKey = newKey.GetPublicKey();

				AccountCreateTransaction signedTx = new AccountCreateTransaction
                {
					InitialBalance = Hbar.FromTinybars(1000),
					Key = newPublicKey,
				}
                .FreezeWith(testEnv.Client)
                .SignWithOperator(testEnv.Client);

				AccountCreateTransaction unsignedTx = new AccountCreateTransaction
                {
                    InitialBalance = Hbar.FromTinybars(1000),
					Key = newPublicKey,
				}
                .FreezeWith(testEnv.Client);

                Assert.True(AccountInfoFlow.VerifyTransactionSignature(testEnv.Client, testEnv.OperatorId, signedTx));
                Assert.False(AccountInfoFlow.VerifyTransactionSignature(testEnv.Client, testEnv.OperatorId, unsignedTx));
            }
        }
    }
}