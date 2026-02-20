// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountCreateIntegrationTest
    {
        public virtual void CanCreateAccountWithOnlyInitialBalanceAndKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction()
                {
					Key = key,
					InitialBalance = new Hbar(1),
				}
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
        public virtual void CanCreateAccountWithNoInitialBalance()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction { Key = key, }.Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);
                Assert.Equal(info.AccountId, accountId);
                Assert.False(info.IsDeleted);
                Assert.Equal(info.Key.ToString(), key.GetPublicKey().ToString());
                Assert.Equal(info.Balance, new Hbar(0));
                Assert.Equal(info.AutoRenewPeriod, Duration.FromTimeSpan(TimeSpan.FromDays(90)));
                Assert.Null(info.ProxyAccountId);
                Assert.Equal(info.ProxyReceived, Hbar.ZERO);
            }
        }
        public virtual void CanNotCreateAccountWithNoKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new AccountCreateTransaction
                    {
                        InitialBalance = new Hbar(1)
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                });
                
                Assert.Contains(ResponseStatus.KeyRequired.ToString(), exception.Message);
            }
        }
        public virtual void CanCreateWithAliasKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var aliasId = key.ToAccountId(0, 0);
                new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, new Hbar(10).Negated())
                    .AddHbarTransfer(aliasId, new Hbar(10))
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                var info = new AccountInfoQuery
                { 
                    AccountId = aliasId
                
                }.Execute(testEnv.Client);

                Assert.Equal(key.GetPublicKey(), info.AliasKey);
            }
        }
        public virtual void ManagesExpiration()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction()
                {
					TransactionId = TransactionId.WithValidStart(testEnv.OperatorId, Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddSeconds(-40))),
					TransactionValidDuration = Duration.FromTimeSpan(TimeSpan.FromSeconds(30)),
					Key = key,
				}
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client);

                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);

                Assert.Equal(info.AccountId, accountId);
                Assert.False(info.IsDeleted);
                Assert.Equal(info.Key.ToString(), key.GetPublicKey().ToString());
                Assert.Equal(info.Balance, new Hbar(0));
                Assert.Equal(info.AutoRenewPeriod, Duration.FromTimeSpan(TimeSpan.FromDays(90)));
                Assert.Null(info.ProxyAccountId);
                Assert.Equal(info.ProxyReceived, Hbar.ZERO);
            }
        }
        public virtual void CreateAccountWithAliasFromAdminKey()
        {

            // Tests the third row of this table
            // https://github.com/hashgraph/hedera-improvement-proposal/blob/d39f740021d7da592524cffeaf1d749803798e9a/HIP/hip-583.md#signatures
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var evmAddress = adminKey.GetPublicKey().ToEvmAddress();

                // Create the admin account
                new AccountCreateTransaction
                {
					Key = adminKey,
				}
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client);

                var accountId = new AccountCreateTransaction()
                    .SetKeyWithAlias(adminKey)
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                Assert.NotNull(accountId);

                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);

                Assert.NotNull(info.AccountId);
                Assert.Equal(info.ContractAccountId.ToString(), evmAddress.ToString());
                Assert.Equal(info.Key, adminKey.GetPublicKey());
            }
        }
        public virtual void CreateAccountWithAliasFromAdminKeyWithReceiverSigRequired()
        {

            // Tests the fourth row of this table
            // https://github.com/hashgraph/hedera-improvement-proposal/blob/d39f740021d7da592524cffeaf1d749803798e9a/HIP/hip-583.md#signatures
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var evmAddress = adminKey.GetPublicKey().ToEvmAddress();

                // Create the admin account
                new AccountCreateTransaction
                {
					Key = adminKey,

				}.FreezeWith(testEnv.Client).Execute(testEnv.Client);
                var accountId = new AccountCreateTransaction
                {
                    ReceiverSigRequired = true
                }
                .SetKeyWithAlias(adminKey)
                .FreezeWith(testEnv.Client)
                .Sign(adminKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;
                
                Assert.NotNull(accountId);

                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);

                Assert.NotNull(info.AccountId);
                Assert.Equal(info.ContractAccountId.ToString(), evmAddress.ToString());
                Assert.Equal(info.Key, adminKey.GetPublicKey());
            }
        }
        public virtual void CannotCreateAccountWithAliasFromAdminKeyWithReceiverSigRequiredAndNoSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var evmAddress = adminKey.GetPublicKey().ToEvmAddress();

                // Create the admin account
                new AccountCreateTransaction
                {
					Key = adminKey,
				}
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client);

				ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new AccountCreateTransaction
                    {
                        Alias = evmAddress,
                        ReceiverSigRequired = true
                    }
                    .SetKeyWithAlias(adminKey)
                    .FreezeWith(testEnv.Client)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
				});

				Assert.Contains(ResponseStatus.KeyRequired.ToString(), exception.Message);
			}
        }
        public virtual void CreateAccountWithAlias()
        {

            // Tests the fifth row of this table
            // https://github.com/hashgraph/hedera-improvement-proposal/blob/d39f740021d7da592524cffeaf1d749803798e9a/HIP/hip-583.md#signatures
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateED25519();

                // Create the admin account
                new AccountCreateTransaction
                {
					Key = adminKey,

				}.FreezeWith(testEnv.Client).Execute(testEnv.Client);
                var key = PrivateKey.GenerateECDSA();
                var evmAddress = key.GetPublicKey().ToEvmAddress();
                var accountId = new AccountCreateTransaction
                {
					Alias = evmAddress,
					Key = adminKey,
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                Assert.NotNull(accountId);
                
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);

                Assert.NotNull(info.AccountId);
                Assert.Equal(info.ContractAccountId.ToString(), evmAddress.ToString());
                Assert.Equal(info.Key, adminKey.GetPublicKey());
            }
        }
        public virtual void CannotCreateAccountWithAliasWithoutSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateED25519();

                // Create the admin account
                new AccountCreateTransaction
                {
					Key = adminKey,

				}.FreezeWith(testEnv.Client).Execute(testEnv.Client);
                var key = PrivateKey.GenerateECDSA();
                BadKeyException exception = Assert.Throws<BadKeyException>(() =>
                {
                    new AccountCreateTransaction
                    {
                        ReceiverSigRequired = true
                    }
                    .SetKeyWithAlias(key, adminKey)
                    .FreezeWith(testEnv.Client)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

				});

				Assert.Contains("Private key is not ECDSA", exception.Message);
			}
        }
        public virtual void CreateAccountWithAliasWithReceiverSigRequired()
        {

            // Tests the sixth row of this table
            // https://github.com/hashgraph/hedera-improvement-proposal/blob/d39f740021d7da592524cffeaf1d749803798e9a/HIP/hip-583.md#signatures
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateED25519();

                // Create the admin account
                new AccountCreateTransaction
                {
                    Key = adminKey,

                }.FreezeWith(testEnv.Client).Execute(testEnv.Client);
                var key = PrivateKey.GenerateECDSA();
                var evmAddress = key.GetPublicKey().ToEvmAddress();
                var accountId = new AccountCreateTransaction
                {
                    ReceiverSigRequired = true,
					Alias = evmAddress,
                    Key = adminKey,
				}                    
                .FreezeWith(testEnv.Client).Sign(key).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);
                Assert.NotNull(info.AccountId);
                Assert.Equal(info.ContractAccountId.ToString(), evmAddress.ToString());
                Assert.Equal(info.Key, adminKey.GetPublicKey());
            }
        }
        public virtual void CannotCreateAccountWithAliasWithReceiverSigRequiredWithoutSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();

                // Create the admin account
                new AccountCreateTransaction
                {
                    Key = adminKey,

                }.FreezeWith(testEnv.Client).Execute(testEnv.Client);
                var key = PrivateKey.GenerateECDSA();
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new AccountCreateTransaction
                    {
                        ReceiverSigRequired = true
                    }
                    .SetKeyWithAlias(adminKey)
                    .FreezeWith(testEnv.Client)
                    .Sign(key)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                });

				Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
			}
        }
        public virtual void CannotCreateAccountWithAliasWithoutBothKeySignatures()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateED25519();

                // Create the admin account
                new AccountCreateTransaction
        {
                    Key = adminKey,

                }.FreezeWith(testEnv.Client).Execute(testEnv.Client);
                var key = PrivateKey.GenerateECDSA();
                BadKeyException exception = Assert.Throws<BadKeyException>(() =>
                {
                    new AccountCreateTransaction
                    {
                        ReceiverSigRequired = true
                    }
                    .SetKeyWithAlias(key, adminKey)
                    .FreezeWith(testEnv.Client)
                    .Sign(adminKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
				});
                
				Assert.Contains("Private key is not ECDSA", exception.Message);
			}
        }
        public virtual void CreateAccountUsingSetKeyWithAliasAccountShouldHaveSameKeyAndSameKeysAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var ecdsaKey = PrivateKey.GenerateECDSA();
                var evmAddress = ecdsaKey.GetPublicKey().ToEvmAddress();
                var accountId = new AccountCreateTransaction
                {
					ReceiverSigRequired = true
				}
                    .SetKeyWithAlias(ecdsaKey).FreezeWith(testEnv.Client).Sign(ecdsaKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);
                Assert.NotNull(info.AccountId);
                Assert.Equal(info.Key, ecdsaKey.GetPublicKey());
                Assert.Equal(info.ContractAccountId.ToString(), evmAddress.ToString());
            }
        }
        public virtual void CreateAccountUsingSetKeyWithAliasAccountShouldHaveKeyAsKeyAndECDSAKEyAsAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var ecdsaKey = PrivateKey.GenerateECDSA();
                var key = PrivateKey.GenerateED25519();
                var evmAddress = ecdsaKey.GetPublicKey().ToEvmAddress();
                var accountId = new AccountCreateTransaction
                {
                    ReceiverSigRequired = true
                }
                    .SetKeyWithAlias(key, ecdsaKey).FreezeWith(testEnv.Client).Sign(key).Sign(ecdsaKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);
                Assert.NotNull(info.AccountId);
                Assert.Equal(info.Key, key.GetPublicKey());
                Assert.Equal(info.ContractAccountId.ToString(), evmAddress.ToString());
            }
        }
        public virtual void CreateAccountUsingSetKeyWithoutAliasAccountShouldHaveKeyAsKeyAndNoAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateECDSA();
                var accountId = new AccountCreateTransaction
                {
                    ReceiverSigRequired = true,
                    Key = key,

                }.FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);
                Assert.NotNull(info.AccountId);
                Assert.Equal(info.Key, key.GetPublicKey());
                Assert.True(IsLongZeroAddress(Hex.Decode(info.ContractAccountId)));
            }
        }
        public virtual void CreateAccountUsingSetKeyWithAliasWithED25519KeyShouldThrowAnException()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
                    ReceiverSigRequired = true,
                    Key = key,

                }.FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
				BadKeyException exception = Assert.Throws<BadKeyException>(() =>
                {
                    new AccountCreateTransaction
                    {
                        ReceiverSigRequired = true
                    }
                    .SetKeyWithAlias(key)
                    .FreezeWith(testEnv.Client)
                    .Sign(key)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
				});
                
				Assert.Contains("Private key is not ECDSA", exception.Message);
			}
        }
        public virtual void CreateAccountUsingSetKeyWithAliasWithPublicKeyShouldHavePublicKeyAndDerivedAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var accountId = new AccountCreateTransaction()
                    .SetKeyWithAlias(publicKey).FreezeWith(testEnv.Client).Sign(privateKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);
                Assert.NotNull(info.AccountId);
                Assert.Equal(info.Key, publicKey);
                Assert.Equal(info.ContractAccountId.ToString(), evmAddress.ToString());
            }
        }
        public virtual void CreateAccountUsingSetKeyWithAliasWithED25519KeyAndPublicECDSAKeyShouldHaveED25519KeyAndDerivedAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var accountKey = PrivateKey.GenerateED25519();
                var aliasPrivateKey = PrivateKey.GenerateECDSA();
                var aliasPublicKey = aliasPrivateKey.GetPublicKey();
                var evmAddress = aliasPublicKey.ToEvmAddress();
                var accountId = new AccountCreateTransaction()
                    .SetKeyWithAlias(accountKey, aliasPublicKey).FreezeWith(testEnv.Client).Sign(accountKey).Sign(aliasPrivateKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                var info = new AccountInfoQuery
                { 
                    AccountId = accountId
                
                }.Execute(testEnv.Client);
                Assert.NotNull(info.AccountId);
                Assert.Equal(info.Key, accountKey.GetPublicKey());
                Assert.Equal(info.ContractAccountId.ToString(), evmAddress.ToString());
            }
        }

        private bool IsLongZeroAddress(byte[] address)
        {
            for (int i = 0; i < 12; i++)
            {
                if (address[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}