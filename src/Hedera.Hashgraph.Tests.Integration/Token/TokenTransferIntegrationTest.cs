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
    class TokenTransferIntegrationTest
    {
        public virtual void TokenTransferTest()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                TransactionResponse response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = response.GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = response.GetReceipt(testEnv.client).tokenId;
                AssertThat(tokenId).IsNotNull();
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).SignWithOperator(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.operatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void InsufficientBalanceForFee()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                PrivateKey key1 = PrivateKey.GenerateED25519();
                PrivateKey key2 = PrivateKey.GenerateED25519();
                var accountId1 = new AccountCreateTransaction().SetKeyWithoutAlias(key1).SetInitialBalance(new Hbar(2)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var accountId2 = new AccountCreateTransaction().SetKeyWithoutAlias(key2).SetInitialBalance(new Hbar(2)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var tokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetInitialSupply(1).SetCustomFees(Collections.SingletonList(new CustomFixedFee().SetAmount(5000000000).SetFeeCollectorAccountId(testEnv.operatorId))).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFeeScheduleKey(testEnv.operatorKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                new TokenAssociateTransaction().SetAccountId(accountId1).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(accountId2).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key2).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.operatorId, -1).AddTokenTransfer(tokenId, accountId1, 1).FreezeWith(testEnv.client).Sign(key1).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TransferTransaction().AddTokenTransfer(tokenId, accountId1, -1).AddTokenTransfer(tokenId, accountId2, 1).FreezeWith(testEnv.client).Sign(key1).Sign(key2).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).Satisfies((error) => AssertThat(error.GetMessage()).ContainsAnyOf(Status.INSUFFICIENT_SENDER_ACCOUNT_BALANCE_FOR_CUSTOM_FEE.ToString(), Status.INSUFFICIENT_PAYER_BALANCE_FOR_CUSTOM_FEE.ToString()));
            }
        }
    }
}