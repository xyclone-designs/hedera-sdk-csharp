// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Hedera.Hashgraph;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountCreateIntegrationTest
    {
        virtual void CanCreateAccountWithOnlyInitialBalanceAndKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(info.accountId, accountId);
                AssertThat(info.isDeleted).IsFalse();
                Assert.Equal(info.key.ToString(), key.GetPublicKey().ToString());
                Assert.Equal(info.balance, new Hbar(1));
                Assert.Equal(info.autoRenewPeriod, Duration.OfDays(90));
                AssertThat(info.proxyAccountId).IsNull();
                Assert.Equal(info.proxyReceived, Hbar.ZERO);
            }
        }

        virtual void CanCreateAccountWithNoInitialBalance()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(info.accountId, accountId);
                AssertThat(info.isDeleted).IsFalse();
                Assert.Equal(info.key.ToString(), key.GetPublicKey().ToString());
                Assert.Equal(info.balance, new Hbar(0));
                Assert.Equal(info.autoRenewPeriod, Duration.OfDays(90));
                AssertThat(info.proxyAccountId).IsNull();
                Assert.Equal(info.proxyReceived, Hbar.ZERO);
            }
        }

        virtual void CanNotCreateAccountWithNoKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() => new AccountCreateTransaction().SetInitialBalance(new Hbar(1)).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.KEY_REQUIRED.ToString());
            }
        }

        virtual void CanCreateWithAliasKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var aliasId = key.ToAccountId(0, 0);
                new TransferTransaction().AddHbarTransfer(testEnv.operatorId, new Hbar(10).Negated()).AddHbarTransfer(aliasId, new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var info = new AccountInfoQuery().SetAccountId(aliasId).Execute(testEnv.client);
                Assert.Equal(key.GetPublicKey(), info.aliasKey);
            }
        }

        virtual void ManagesExpiration()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetTransactionId(new TransactionId(testEnv.operatorId, Instant.Now().MinusSeconds(40))).SetTransactionValidDuration(Duration.OfSeconds(30)).FreezeWith(testEnv.client).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(info.accountId, accountId);
                AssertThat(info.isDeleted).IsFalse();
                Assert.Equal(info.key.ToString(), key.GetPublicKey().ToString());
                Assert.Equal(info.balance, new Hbar(0));
                Assert.Equal(info.autoRenewPeriod, Duration.OfDays(90));
                AssertThat(info.proxyAccountId).IsNull();
                Assert.Equal(info.proxyReceived, Hbar.ZERO);
            }
        }

        virtual void CreateAccountWithAliasFromAdminKey()
        {

            // Tests the third row of this table
            // https://github.com/hashgraph/hedera-improvement-proposal/blob/d39f740021d7da592524cffeaf1d749803798e9a/HIP/hip-583.md#signatures
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var evmAddress = adminKey.GetPublicKey().ToEvmAddress();

                // Create the admin account
                new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client);
                var accountId = new AccountCreateTransaction().SetKeyWithAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                AssertThat(info.contractAccountId).HasToString(evmAddress.ToString());
                Assert.Equal(info.key, adminKey.GetPublicKey());
            }
        }

        virtual void CreateAccountWithAliasFromAdminKeyWithReceiverSigRequired()
        {

            // Tests the fourth row of this table
            // https://github.com/hashgraph/hedera-improvement-proposal/blob/d39f740021d7da592524cffeaf1d749803798e9a/HIP/hip-583.md#signatures
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var evmAddress = adminKey.GetPublicKey().ToEvmAddress();

                // Create the admin account
                new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client);
                var accountId = new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithAlias(adminKey).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                AssertThat(info.contractAccountId).HasToString(evmAddress.ToString());
                Assert.Equal(info.key, adminKey.GetPublicKey());
            }
        }

        virtual void CannotCreateAccountWithAliasFromAdminKeyWithReceiverSigRequiredAndNoSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();
                var evmAddress = adminKey.GetPublicKey().ToEvmAddress();

                // Create the admin account
                new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithAlias(adminKey).SetAlias(evmAddress).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CreateAccountWithAlias()
        {

            // Tests the fifth row of this table
            // https://github.com/hashgraph/hedera-improvement-proposal/blob/d39f740021d7da592524cffeaf1d749803798e9a/HIP/hip-583.md#signatures
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateED25519();

                // Create the admin account
                new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client);
                var key = PrivateKey.GenerateECDSA();
                var evmAddress = key.GetPublicKey().ToEvmAddress();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).SetAlias(evmAddress).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                AssertThat(info.contractAccountId).HasToString(evmAddress.ToString());
                Assert.Equal(info.key, adminKey.GetPublicKey());
            }
        }

        virtual void CannotCreateAccountWithAliasWithoutSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateED25519();

                // Create the admin account
                new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client);
                var key = PrivateKey.GenerateECDSA();
                AssertThatExceptionOfType(typeof(BadKeyException)).IsThrownBy(() => new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithAlias(key, adminKey).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining("Private key is not ECDSA");
            }
        }

        virtual void CreateAccountWithAliasWithReceiverSigRequired()
        {

            // Tests the sixth row of this table
            // https://github.com/hashgraph/hedera-improvement-proposal/blob/d39f740021d7da592524cffeaf1d749803798e9a/HIP/hip-583.md#signatures
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateED25519();

                // Create the admin account
                new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client);
                var key = PrivateKey.GenerateECDSA();
                var evmAddress = key.GetPublicKey().ToEvmAddress();
                var accountId = new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithoutAlias(adminKey).SetAlias(evmAddress).FreezeWith(testEnv.client).Sign(key).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                AssertThat(info.contractAccountId).HasToString(evmAddress.ToString());
                Assert.Equal(info.key, adminKey.GetPublicKey());
            }
        }

        virtual void CannotCreateAccountWithAliasWithReceiverSigRequiredWithoutSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateECDSA();

                // Create the admin account
                new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client);
                var key = PrivateKey.GenerateECDSA();
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithAlias(adminKey).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotCreateAccountWithAliasWithoutBothKeySignatures()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var adminKey = PrivateKey.GenerateED25519();

                // Create the admin account
                new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).FreezeWith(testEnv.client).Execute(testEnv.client);
                var key = PrivateKey.GenerateECDSA();
                AssertThatExceptionOfType(typeof(BadKeyException)).IsThrownBy(() => new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithAlias(key, adminKey).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining("Private key is not ECDSA");
            }
        }

        virtual void CreateAccountUsingSetKeyWithAliasAccountShouldHaveSameKeyAndSameKeysAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var ecdsaKey = PrivateKey.GenerateECDSA();
                var evmAddress = ecdsaKey.GetPublicKey().ToEvmAddress();
                var accountId = new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithAlias(ecdsaKey).FreezeWith(testEnv.client).Sign(ecdsaKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                Assert.Equal(info.key, ecdsaKey.GetPublicKey());
                AssertThat(info.contractAccountId).HasToString(evmAddress.ToString());
            }
        }

        virtual void CreateAccountUsingSetKeyWithAliasAccountShouldHaveKeyAsKeyAndECDSAKEyAsAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var ecdsaKey = PrivateKey.GenerateECDSA();
                var key = PrivateKey.GenerateED25519();
                var evmAddress = ecdsaKey.GetPublicKey().ToEvmAddress();
                var accountId = new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithAlias(key, ecdsaKey).FreezeWith(testEnv.client).Sign(key).Sign(ecdsaKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                Assert.Equal(info.key, key.GetPublicKey());
                AssertThat(info.contractAccountId).HasToString(evmAddress.ToString());
            }
        }

        virtual void CreateAccountUsingSetKeyWithoutAliasAccountShouldHaveKeyAsKeyAndNoAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateECDSA();
                var accountId = new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithoutAlias(key).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                Assert.Equal(info.key, key.GetPublicKey());
                AssertTrue(IsLongZeroAddress(Hex.Decode(info.contractAccountId)));
            }
        }

        virtual void CreateAccountUsingSetKeyWithAliasWithED25519KeyShouldThrowAnException()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithoutAlias(key).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                AssertThatExceptionOfType(typeof(BadKeyException)).IsThrownBy(() => new AccountCreateTransaction().SetReceiverSignatureRequired(true).SetKeyWithAlias(key).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining("Private key is not ECDSA");
            }
        }

        virtual void CreateAccountUsingSetKeyWithAliasWithPublicKeyShouldHavePublicKeyAndDerivedAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var accountId = new AccountCreateTransaction().SetKeyWithAlias(publicKey).FreezeWith(testEnv.client).Sign(privateKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                Assert.Equal(info.key, publicKey);
                AssertThat(info.contractAccountId).HasToString(evmAddress.ToString());
            }
        }

        virtual void CreateAccountUsingSetKeyWithAliasWithED25519KeyAndPublicECDSAKeyShouldHaveED25519KeyAndDerivedAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var accountKey = PrivateKey.GenerateED25519();
                var aliasPrivateKey = PrivateKey.GenerateECDSA();
                var aliasPublicKey = aliasPrivateKey.GetPublicKey();
                var evmAddress = aliasPublicKey.ToEvmAddress();
                var accountId = new AccountCreateTransaction().SetKeyWithAlias(accountKey, aliasPublicKey).FreezeWith(testEnv.client).Sign(accountKey).Sign(aliasPrivateKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                AssertThat(info.accountId).IsNotNull();
                Assert.Equal(info.key, accountKey.GetPublicKey());
                AssertThat(info.contractAccountId).HasToString(evmAddress.ToString());
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