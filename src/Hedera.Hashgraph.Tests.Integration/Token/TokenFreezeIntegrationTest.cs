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
    class TokenFreezeIntegrationTest
    {
        public virtual void CanFreezeAccountWithToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenFreezeTransaction().SetAccountId(accountId).SetTokenId(tokenId).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotFreezeAccountOnTokenWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenFreezeTransaction().SetAccountId(accountId).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        public virtual void CannotFreezeAccountOnTokenWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenFreezeTransaction().SetTokenId(tokenId).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
            }
        }

        public virtual void CannotFreezeAccountOnTokenWhenAccountWasNotAssociatedWith()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenFreezeTransaction().SetAccountId(accountId).SetTokenId(tokenId).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.TOKEN_NOT_ASSOCIATED_TO_ACCOUNT.ToString());
            }
        }
    }
}