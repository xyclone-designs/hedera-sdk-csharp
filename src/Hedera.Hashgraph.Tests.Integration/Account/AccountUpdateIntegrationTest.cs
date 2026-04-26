// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class AccountUpdateIntegrationTest
    {
        [Fact]
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
                Assert.Equal(info.AutoRenewPeriod, TimeSpan.FromDays(90));
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
				Assert.Equal(info.AutoRenewPeriod, TimeSpan.FromDays(90));
				Assert.Null(info.ProxyAccountId);
                Assert.Equal(info.ProxyReceived, Hbar.ZERO);
            }
        }
        [Fact]
        public virtual void CannotUpdateAccountWhenAccountIdIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
				PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new AccountUpdateTransaction()
                        .Execute(testEnv.Client)
                        .GetReceipt(testEnv.Client);
                });

                Assert.Contains(exception.Message, ResponseStatus.AccountIdDoesNotExist.ToString());
            }
        }
    }
}