// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;

using System;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountUpdateIntegrationTest
    {
        public virtual void CanUpdateAccountWithNewKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key1 = PrivateKey.GenerateED25519();
                var key2 = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					Key = key1

				}.Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var info = new AccountInfoQuery
                {
					AccountId = accountId

				}.Execute(testEnv.Client);
                Assert.Equal(info.AccountId, accountId);
                Assert.False(info.IsDeleted);
                Assert.Equal(info.Key.ToString(), key1.GetPublicKey().ToString());
                Assert.Equal(info.Balance, new Hbar(0));
                Assert.Equal(info.AutoRenewPeriod, Duration.FromTimeSpan(TimeSpan.FromDays(90)));
                Assert.Null(info.ProxyAccountId);
                Assert.Equal(info.ProxyReceived, Hbar.ZERO);
                new AccountUpdateTransaction
                {
					AccountId = accountId,
					Key = key2.GetPublicKey(),
				
                }.FreezeWith(testEnv.Client).Sign(key1).Sign(key2).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                info = new AccountInfoQuery
                {
					AccountId = accountId

				}.Execute(testEnv.Client);
                Assert.Equal(info.AccountId, accountId);
                Assert.False(info.IsDeleted);
                Assert.Equal(info.Key.ToString(), key2.GetPublicKey().ToString());
                Assert.Equal(info.Balance, new Hbar(0));
				Assert.Equal(info.AutoRenewPeriod, Duration.FromTimeSpan(TimeSpan.FromDays(90)));
				Assert.Null(info.ProxyAccountId);
                Assert.Equal(info.ProxyReceived, Hbar.ZERO);
            }
        }

        public virtual void CannotUpdateAccountWhenAccountIdIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                Assert.Throws<PrecheckStatusException>(() =>
                {
                    new AccountUpdateTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                }).WithMessageContaining(Status.ACCOUNT_ID_DOES_NOT_EXIST.ToString());
            }
        }
    }
}