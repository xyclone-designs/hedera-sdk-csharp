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
    class TokenBurnIntegrationTest
    {
        public virtual void CanBurnTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var receipt = new TokenBurnTransaction().SetAmount(10).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.totalSupply, 1000000 - 10);
            }
        }

        public virtual void CannotBurnTokensWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenBurnTransaction().SetAmount(10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        public virtual void CanBurnTokensWhenAmountIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var receipt = new TokenBurnTransaction().SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }

        public virtual void CannotBurnTokensWhenSupplyKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,.SetSupplyKey(PrivateKey.Generate())FreezeDefault = false,.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenBurnTransaction().SetTokenId(tokenId).SetAmount(10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CanBurnNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenType(TokenType.NonFungibleUnique)TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenId = createReceipt.tokenId);
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenBurnTransaction().SetSerials(mintReceipt.serials.SubList(0, 4)).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotBurnNftsWhenNftIsNotOwned()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenType(TokenType.NonFungibleUnique)TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenId = createReceipt.tokenId);
                var serials = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).Serials;
                var key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddNftTransfer(tokenId.Nft(serials[0]), testEnv.OperatorId, accountId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenBurnTransaction().SetSerials(serials).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.TREASURY_MUST_OWN_BURNED_NFT.ToString());
            }
        }
    }
}