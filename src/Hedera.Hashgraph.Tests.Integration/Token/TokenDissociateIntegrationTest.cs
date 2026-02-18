// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenDissociateIntegrationTest
    {
        public virtual void CanAssociateAccountWithToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenDissociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenDissociateTransactionEvenWhenTokenIDsAreNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                new TokenDissociateTransaction().SetAccountId(accountId).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotDissociateAccountWithTokensWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenDissociateTransaction().FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
            }
        }

        public virtual void CannotDissociateAccountWhenAccountDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenDissociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CannotDissociateAccountFromTokenWhenAccountWasNotAssociatedWith()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenDissociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.TOKEN_NOT_ASSOCIATED_TO_ACCOUNT.ToString());
            }
        }
    }
}