// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;

using System;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountDeleteIntegrationTest
    {
        public virtual void CanDeleteAccount()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)
				}
                .SetKeyWithoutAlias(key)
                .Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var info = new AccountInfoQuery
                {
					AccountId = accountId

				}.Execute(testEnv.Client);

                Assert.Equal(info.AccountId, accountId);
                Assert.False(info.IsDeleted);
                Assert.Equal(info.Key.ToString(), key.GetPublicKey().ToString());
                Assert.Equal(info.Balance, new Hbar(1));
                Assert.Equal(info.AutoRenewPeriod, Duration.FromTimeSpan(TimeSpan.FromDays(90)));
                Assert.Null(info.ProxyAccountId);
                Assert.Equal(info.ProxyReceived, Hbar.ZERO);
            }
        }

        public virtual void CannotCreateAccountWithNoKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                Assert.Throws<PrecheckStatusException>(() =>
                {
                    new AccountDeleteTransaction
                    {
						TransferAccountId = testEnv.OperatorId

					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.ACCOUNT_ID_DOES_NOT_EXIST.ToString());
            }
        }

        public virtual void CannotDeleteAccountThatHasNotSignedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)

				}.SetKeyWithoutAlias(key).Execute(testEnv.Client);

                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                
                Assert.Throws<ReceiptStatusException>(() =>
                {
                    new AccountDeleteTransaction
                    {
						AccountId = accountId,
						TransferAccountId = testEnv.OperatorId,
					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }
    }
}