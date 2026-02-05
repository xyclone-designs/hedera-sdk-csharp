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
        virtual void CanBurnTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var receipt = new TokenBurnTransaction().SetAmount(10).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.totalSupply, 1000000 - 10);
            }
        }

        virtual void CannotBurnTokensWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenBurnTransaction().SetAmount(10).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        virtual void CanBurnTokensWhenAmountIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var receipt = new TokenBurnTransaction().SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }

        virtual void CannotBurnTokensWhenSupplyKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(PrivateKey.Generate()).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenBurnTransaction().SetTokenId(tokenId).SetAmount(10).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CanBurnNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenId = Objects.RequireNonNull(createReceipt.tokenId);
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenBurnTransaction().SetSerials(mintReceipt.serials.SubList(0, 4)).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CannotBurnNftsWhenNftIsNotOwned()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenId = Objects.RequireNonNull(createReceipt.tokenId);
                var serials = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)1)).Execute(testEnv.client).GetReceipt(testEnv.client).serials;
                var key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).SignWithOperator(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddNftTransfer(tokenId.Nft(serials[0]), testEnv.operatorId, accountId).Execute(testEnv.client).GetReceipt(testEnv.client);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenBurnTransaction().SetSerials(serials).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TREASURY_MUST_OWN_BURNED_NFT.ToString());
            }
        }
    }
}