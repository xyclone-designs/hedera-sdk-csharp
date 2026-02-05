// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api.Assertions;
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
    public class TokenPauseIntegrationTest
    {
        virtual void CanExecuteTokenPauseTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var testTokenAmount = 10;
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var tokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetInitialSupply(1000000).SetDecimals(3).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetPauseKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddTokenTransfer(tokenId, accountId, testTokenAmount).AddTokenTransfer(tokenId, testEnv.operatorId, -testTokenAmount).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenPauseTransaction().SetTokenId(tokenId).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client);
                AssertThrows(typeof(ReceiptStatusException), () =>
                {
                    new TransferTransaction().AddTokenTransfer(tokenId, accountId, testTokenAmount).AddTokenTransfer(tokenId, testEnv.operatorId, -testTokenAmount).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                });
            }
        }

        virtual void CannotPauseWithNoTokenId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                AssertThrows(typeof(PrecheckStatusException), () =>
                {
                    new TokenPauseTransaction().Execute(testEnv.client).GetReceipt(testEnv.client);
                });
            }
        }
    }
}