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
    class AccountIdPopulationIntegrationTest
    {
        virtual void CanPopulateAccountIdNumSync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var evmAddressAccount = AccountId.FromEvmAddress(evmAddress, 0, 0);
                var tx = new TransferTransaction().AddHbarTransfer(evmAddressAccount, new Hbar(1)).AddHbarTransfer(testEnv.operatorId, new Hbar(-1)).Execute(testEnv.client);
                var receipt = new TransactionReceiptQuery().SetTransactionId(tx.transactionId).SetIncludeChildren(true).Execute(testEnv.client);
                var newAccountId = receipt.children[0].accountId;
                var idMirror = AccountId.FromEvmAddress(evmAddress, 0, 0);
                Thread.Sleep(5000);
                var accountId = idMirror.PopulateAccountNum(testEnv.client);
                Assert.Equal(newAccountId.num, accountId.num);
            }
        }

        virtual void CanPopulateAccountIdNumAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var evmAddressAccount = AccountId.FromEvmAddress(evmAddress, 0, 0);
                var tx = new TransferTransaction().AddHbarTransfer(evmAddressAccount, new Hbar(1)).AddHbarTransfer(testEnv.operatorId, new Hbar(-1)).Execute(testEnv.client);
                var receipt = new TransactionReceiptQuery().SetTransactionId(tx.transactionId).SetIncludeChildren(true).Execute(testEnv.client).ValidateStatus(true);
                var newAccountId = receipt.children[0].accountId;
                var idMirror = AccountId.FromEvmAddress(evmAddress, 0, 0);
                Thread.Sleep(5000);
                var accountId = idMirror.PopulateAccountNumAsync(testEnv.client).Get();
                Assert.Equal(newAccountId.num, accountId.num);
            }
        }

        virtual void CanPopulateAccountIdEvmAddressSync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var evmAddressAccount = AccountId.FromEvmAddress(evmAddress, 0, 0);
                var tx = new TransferTransaction().AddHbarTransfer(evmAddressAccount, new Hbar(1)).AddHbarTransfer(testEnv.operatorId, new Hbar(-1)).Execute(testEnv.client);
                var receipt = new TransactionReceiptQuery().SetTransactionId(tx.transactionId).SetIncludeChildren(true).Execute(testEnv.client);
                var newAccountId = receipt.children[0].accountId;
                Thread.Sleep(5000);
                var accountId = newAccountId.PopulateAccountEvmAddress(testEnv.client);
                Assert.Equal(evmAddressAccount.evmAddress, accountId.evmAddress);
            }
        }

        virtual void CanPopulateAccountIdEvmAddressAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var evmAddressAccount = AccountId.FromEvmAddress(evmAddress, 0, 0);
                var tx = new TransferTransaction().AddHbarTransfer(evmAddressAccount, new Hbar(1)).AddHbarTransfer(testEnv.operatorId, new Hbar(-1)).Execute(testEnv.client);
                var receipt = new TransactionReceiptQuery().SetTransactionId(tx.transactionId).SetIncludeChildren(true).Execute(testEnv.client);
                var newAccountId = receipt.children[0].accountId;
                Thread.Sleep(5000);
                var accountId = newAccountId.PopulateAccountEvmAddressAsync(testEnv.client).Get();
                Assert.Equal(evmAddressAccount.evmAddress, accountId.evmAddress);
            }
        }
    }
}