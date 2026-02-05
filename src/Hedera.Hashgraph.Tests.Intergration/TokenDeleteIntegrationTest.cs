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
    class TokenDeleteIntegrationTest
    {
        virtual void CanDeleteToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                new TokenDeleteTransaction().SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanDeleteTokenWithOnlyAdminKeySet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).Execute(testEnv.client);
                Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
            }
        }

        virtual void CannotDeleteTokenWhenAdminKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(key).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenDeleteTransaction().SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                new TokenDeleteTransaction().SetTokenId(tokenId).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CannotDeleteTokenWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenDeleteTransaction().Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }
    }
}