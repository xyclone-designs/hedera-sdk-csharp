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
        public virtual void CanExecuteTokenPauseTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var testTokenAmount = 10;
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",InitialSupply = 1000000,Decimals = 3,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,.SetPauseKey(testEnv.OperatorKey)FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddTokenTransfer(tokenId, accountId, testTokenAmount).AddTokenTransfer(tokenId, testEnv.OperatorId, -testTokenAmount).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenPauseTransaction().SetTokenId(tokenId).FreezeWith(testEnv.Client).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                AssertThrows(typeof(ReceiptStatusException), () =>
                {
                    new TransferTransaction().AddTokenTransfer(tokenId, accountId, testTokenAmount).AddTokenTransfer(tokenId, testEnv.OperatorId, -testTokenAmount).FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
            }
        }

        public virtual void CannotPauseWithNoTokenId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                AssertThrows(typeof(PrecheckStatusException), () =>
                {
                    new TokenPauseTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
            }
        }
    }
}