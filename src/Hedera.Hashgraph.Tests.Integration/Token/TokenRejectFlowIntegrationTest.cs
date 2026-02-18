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
        public virtual void CanExecuteTokenRejectFlowForFungibleToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // manually associate ft
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(ftTokenId)).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // execute the token reject flow
                new TokenRejectFlow().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
                Assert.Equal(treasuryAccountBalance.tokens[ftTokenId], 1000000);

                // verify the allowance - should be 0, because TokenRejectFlow dissociates
                Assert.Throws(typeof(Exception), () =>
                {
                    new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenRejectFlowForFungibleTokenAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // manually associate ft
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(ftTokenId)).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // execute the token reject flow
                new TokenRejectFlow().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.Client).Sign(receiverAccountKey).ExecuteAsync(testEnv.Client).Get().GetReceipt(testEnv.Client);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
                Assert.Equal(treasuryAccountBalance.tokens[ftTokenId], 1000000);

                // verify the tokens are not associated with the receiver
                Assert.Throws(typeof(Exception), () =>
                {
                    new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenRejectFlowForNft()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);
                var mintReceiptToken = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceiptToken.Serials;

                // manually associate bft
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(nftTokenId)).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // execute the token reject flow
                new TokenRejectFlow().SetOwnerId(receiverAccountId).SetNftIds(List.Of(nftTokenId.Nft(nftSerials[0]), nftTokenId.Nft(nftSerials[1]))).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the token is transferred back to the treasury
                var nftTokenIdNftInfo = new TokenNftInfoQuery().SetNftId(nftTokenId.Nft(nftSerials[1])).Execute(testEnv.Client);
                Assert.Equal(nftTokenIdNftInfo[0].accountId, testEnv.OperatorId);

                // verify the tokens are not associated with the receiver
                Assert.Throws(typeof(Exception), () =>
                {
                    new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT");
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenRejectFlowForNftWhenRejectingOnlyPartOfOwnedNFTs()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var nftTokenId1 = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);
                var mintReceiptToken = new TokenMintTransaction().SetTokenId(nftTokenId1).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceiptToken.Serials;

                // manually associate bft
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(nftTokenId1)).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId1.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).AddNftTransfer(nftTokenId1.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // execute the token reject flow
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectFlow().SetOwnerId(receiverAccountId).AddNftId(nftTokenId1.Nft(nftSerials[1])).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining("ACCOUNT_STILL_OWNS_NFTS");
            }
        }
    }
}