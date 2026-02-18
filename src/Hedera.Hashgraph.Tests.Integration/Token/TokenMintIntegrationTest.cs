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
    class TokenMintIntegrationTest
    {
        public virtual void CanMintTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var receipt = new TokenMintTransaction().SetAmount(10).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.totalSupply, 1000000 + 10);
            }
        }

        public virtual void CannotMintMoreThanMaxSupply()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetSupplyType(TokenSupplyType.FINITE).SetMaxSupply(5)TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenMintTransaction().SetTokenId(tokenId).SetAmount(6).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.TOKEN_MAX_SUPPLY_REACHED.ToString());
            }
        }

        public virtual void CannotMintTokensWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenMintTransaction().SetAmount(10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        public virtual void CanMintTokensWhenAmountIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var receipt = new TokenMintTransaction().SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }

        public virtual void CannotMintTokensWhenSupplyKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,.SetSupplyKey(key)FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenMintTransaction().SetTokenId(tokenId).SetAmount(10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CanMintNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenType(TokenType.NonFungibleUnique)TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var receipt = new TokenMintTransaction().SetMetadata(NftMetadataGenerator.Generate((byte)10)).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.serials.Count, 10);
            }
        }

        public virtual void CannotMintNftsIfMetadataTooBig()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenType(TokenType.NonFungibleUnique)TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenMintTransaction().SetMetadata(NftMetadataGenerator.GenerateOneLarge()).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.METADATA_TOO_LONG.ToString());
            }
        }
    }
}