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
        public virtual void CanDeleteToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                new TokenDeleteTransaction().SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanDeleteTokenWithOnlyAdminKeySet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,.Execute(testEnv.Client);
                response.GetReceipt(testEnv.Client).TokenId;
            }
        }

        public virtual void CannotDeleteTokenWhenAdminKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(key).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenDeleteTransaction().SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                new TokenDeleteTransaction().SetTokenId(tokenId).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotDeleteTokenWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenDeleteTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }
    }
}