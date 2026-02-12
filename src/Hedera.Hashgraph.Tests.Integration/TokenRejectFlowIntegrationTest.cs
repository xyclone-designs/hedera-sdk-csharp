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
    public class TokenRejectFlowIntegrationTest
    {
        virtual void CanExecuteTokenRejectFlowForFungibleToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // manually associate ft
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(ftTokenId)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // execute the token reject flow
                new TokenRejectFlow().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(treasuryAccountBalance.tokens[ftTokenId], 1000000);

                // verify the allowance - should be 0, because TokenRejectFlow dissociates
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanExecuteTokenRejectFlowForFungibleTokenAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // manually associate ft
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(ftTokenId)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // execute the token reject flow
                new TokenRejectFlow().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.client).Sign(receiverAccountKey).ExecuteAsync(testEnv.client).Get().GetReceipt(testEnv.client);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(treasuryAccountBalance.tokens[ftTokenId], 1000000);

                // verify the tokens are not associated with the receiver
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanExecuteTokenRejectFlowForNft()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);
                var mintReceiptToken = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceiptToken.serials;

                // manually associate bft
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(nftTokenId)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // execute the token reject flow
                new TokenRejectFlow().SetOwnerId(receiverAccountId).SetNftIds(List.Of(nftTokenId.Nft(nftSerials[0]), nftTokenId.Nft(nftSerials[1]))).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the token is transferred back to the treasury
                var nftTokenIdNftInfo = new TokenNftInfoQuery().SetNftId(nftTokenId.Nft(nftSerials[1])).Execute(testEnv.client);
                Assert.Equal(nftTokenIdNftInfo[0].accountId, testEnv.operatorId);

                // verify the tokens are not associated with the receiver
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT");
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanExecuteTokenRejectFlowForNftWhenRejectingOnlyPartOfOwnedNFTs()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var nftTokenId1 = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);
                var mintReceiptToken = new TokenMintTransaction().SetTokenId(nftTokenId1).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceiptToken.serials;

                // manually associate bft
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(nftTokenId1)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId1.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId1.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // execute the token reject flow
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new TokenRejectFlow().SetOwnerId(receiverAccountId).AddNftId(nftTokenId1.Nft(nftSerials[1])).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("ACCOUNT_STILL_OWNS_NFTS");
            }
        }
    }
}