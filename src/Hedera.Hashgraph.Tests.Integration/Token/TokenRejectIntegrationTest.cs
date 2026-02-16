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
    public class TokenRejectIntegrationTest
    {
        public virtual void CanExecuteTokenRejectTransactionForFungibleToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(tokenId1, testEnv.operatorId, -10).AddTokenTransfer(tokenId1, receiverAccountId, 10).AddTokenTransfer(tokenId2, testEnv.operatorId, -10).AddTokenTransfer(tokenId2, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token
                new TokenRejectTransaction().SetOwnerId(receiverAccountId).SetTokenIds(List.Of(tokenId1, tokenId2)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the balance of the receiver is 0
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(receiverAccountBalance.tokens[tokenId1], 0);
                Assert.Equal(receiverAccountBalance.tokens[tokenId2], 0);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(treasuryAccountBalance.tokens[tokenId1], 1000000);
                Assert.Equal(treasuryAccountBalance.tokens[tokenId2], 1000000);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanExecuteTokenRejectTransactionForNft()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateNft(testEnv);
                var tokenId2 = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);
                var mintReceiptToken1 = new TokenMintTransaction().SetTokenId(tokenId1).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var mintReceiptToken2 = new TokenMintTransaction().SetTokenId(tokenId2).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceiptToken2.serials;

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(tokenId1.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(tokenId1.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).AddNftTransfer(tokenId2.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(tokenId2.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject one of the nfts
                new TokenRejectTransaction().SetOwnerId(receiverAccountId).SetNftIds(List.Of(tokenId1.Nft(nftSerials[1]), tokenId2.Nft(nftSerials[1]))).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the balance is decremented by 1
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(receiverAccountBalance.tokens[tokenId1], 1);
                Assert.Equal(receiverAccountBalance.tokens[tokenId2], 1);

                // verify the token is transferred back to the treasury
                var tokenId1NftInfo = new TokenNftInfoQuery().SetNftId(tokenId1.Nft(nftSerials[1])).Execute(testEnv.client);
                Assert.Equal(tokenId1NftInfo[0].accountId, testEnv.operatorId);
                var tokenId2NftInfo = new TokenNftInfoQuery().SetNftId(tokenId2.Nft(nftSerials[1])).Execute(testEnv.client);
                Assert.Equal(tokenId2NftInfo[0].accountId, testEnv.operatorId);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanExecuteTokenRejectTransactionForFtAndNftInOneTx()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId1 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var ftTokenId2 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftTokenId1 = EntityHelper.CreateNft(testEnv);
                var nftTokenId2 = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);
                var mintReceiptNftToken1 = new TokenMintTransaction().SetTokenId(nftTokenId1).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var mintReceiptNftToken2 = new TokenMintTransaction().SetTokenId(nftTokenId2).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceiptNftToken2.serials;

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId1, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId1, receiverAccountId, 10).AddTokenTransfer(ftTokenId2, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId2, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId1.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId1.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId2.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId2.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token
                new TokenRejectTransaction().SetOwnerId(receiverAccountId).SetTokenIds(List.Of(ftTokenId1, ftTokenId2)).SetNftIds(List.Of(nftTokenId1.Nft(nftSerials[1]), nftTokenId2.Nft(nftSerials[1]))).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the balance of the receiver is 0
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(receiverAccountBalance.tokens[ftTokenId1], 0);
                Assert.Equal(receiverAccountBalance.tokens[ftTokenId2], 0);
                Assert.Equal(receiverAccountBalance.tokens[nftTokenId1], 1);
                Assert.Equal(receiverAccountBalance.tokens[nftTokenId2], 1);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(treasuryAccountBalance.tokens[ftTokenId1], 1000000);
                Assert.Equal(treasuryAccountBalance.tokens[ftTokenId2], 1000000);
                var tokenId1NftInfo = new TokenNftInfoQuery().SetNftId(nftTokenId1.Nft(nftSerials[1])).Execute(testEnv.client);
                Assert.Equal(tokenId1NftInfo[0].accountId, testEnv.operatorId);
                var tokenId2NftInfo = new TokenNftInfoQuery().SetNftId(nftTokenId2.Nft(nftSerials[1])).Execute(testEnv.client);
                Assert.Equal(tokenId2NftInfo[0].accountId, testEnv.operatorId);
                new TokenDeleteTransaction().SetTokenId(ftTokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(ftTokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(nftTokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(nftTokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanExecuteTokenRejectTransactionForFtAndNftWhenTreasuryReceiverSigRequiredIsEnabled()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);
                var treasuryAccountKey = PrivateKey.GenerateED25519();
                var treasuryAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(treasuryAccountKey).SetInitialBalance(new Hbar(0)).SetReceiverSignatureRequired(true).SetMaxAutomaticTokenAssociations(100).FreezeWith(testEnv.client).Sign(treasuryAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var ftTokenId = new TokenCreateTransaction().SetTokenName("Test Fungible Token").SetTokenSymbol("TFT").SetTokenMemo("I was created for integration tests").SetDecimals(18).SetInitialSupply(1000000).SetMaxSupply(1000000).SetTreasuryAccountId(treasuryAccountId).SetSupplyType(TokenSupplyType.FINITE).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(testEnv.operatorKey).FreezeWith(testEnv.client).Sign(treasuryAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, treasuryAccountId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).FreezeWith(testEnv.client).Sign(treasuryAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token
                new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the balance of the receiver is 0
                var receiverAccountBalanceFt = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(receiverAccountBalanceFt.tokens[ftTokenId], 0);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery().SetAccountId(treasuryAccountId).Execute(testEnv.client);
                Assert.Equal(treasuryAccountBalance.tokens[ftTokenId], 1000000);

                // same test for nft
                var nftTokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(treasuryAccountId).SetSupplyType(TokenSupplyType.FINITE).SetMaxSupply(10).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).FreezeWith(testEnv.client).Sign(treasuryAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                var mintReceiptNftToken = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceiptNftToken.serials;

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), treasuryAccountId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), treasuryAccountId, receiverAccountId).FreezeWith(testEnv.client).Sign(treasuryAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token
                new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddNftId(nftTokenId.Nft(nftSerials[1])).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the balance is decremented by 1
                var receiverAccountBalanceNft = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(receiverAccountBalanceNft.tokens[nftTokenId], 1);

                // verify the token is transferred back to the treasury
                var nftTokenIdInfo = new TokenNftInfoQuery().SetNftId(nftTokenId.Nft(nftSerials[1])).Execute(testEnv.client);
                Assert.Equal(nftTokenIdInfo[0].accountId, treasuryAccountId);
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanExecuteTokenRejectTransactionForFtAndNftWhenTokenIsFrozen()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 18);
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // freeze ft
                new TokenFreezeTransaction().SetTokenId(ftTokenId).SetAccountId(receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token - should fail with ACCOUNT_FROZEN_FOR_TOKEN
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("ACCOUNT_FROZEN_FOR_TOKEN");

                // same test for nft
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // freeze nft
                new TokenFreezeTransaction().SetTokenId(nftTokenId).SetAccountId(receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token - should fail with ACCOUNT_FROZEN_FOR_TOKEN
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddNftId(nftTokenId.Nft(nftSerials[1])).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("ACCOUNT_FROZEN_FOR_TOKEN");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanExecuteTokenRejectTransactionForFtAndNftWhenTokenIsPaused()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 18);
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // pause ft
                new TokenPauseTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token - should fail with TOKEN_IS_PAUSED
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("TOKEN_IS_PAUSED");

                // same test for nft
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // pause nft
                new TokenPauseTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token - should fail with TOKEN_IS_PAUSED
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddNftId(nftTokenId.Nft(nftSerials[1])).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("TOKEN_IS_PAUSED");
            }
        }

        public virtual void CanRemoveAllowanceWhenExecutingTokenRejectForFtAndNft()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, -1);
                var spenderAccountKey = PrivateKey.GenerateED25519();
                var spenderAccountId = EntityHelper.CreateAccount(testEnv, spenderAccountKey, -1);

                // transfer ft to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction().ApproveTokenAllowance(ftTokenId, receiverAccountId, spenderAccountId, 10).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the spender has allowance
                new TransferTransaction().AddApprovedTokenTransfer(ftTokenId, receiverAccountId, -5).AddTokenTransfer(ftTokenId, spenderAccountId, 5).SetTransactionId(TransactionId.Generate(spenderAccountId)).FreezeWith(testEnv.client).Sign(spenderAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token
                new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the allowance - should be 0 , because the receiver is no longer the owner
                Assert.Throws(typeof(Exception), () =>
                {
                    new TransferTransaction().AddApprovedTokenTransfer(ftTokenId, receiverAccountId, -5).AddTokenTransfer(ftTokenId, spenderAccountId, 5).SetTransactionId(TransactionId.Generate(spenderAccountId)).FreezeWith(testEnv.client).Sign(spenderAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("SPENDER_DOES_NOT_HAVE_ALLOWANCE");

                // same test for nft
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[2]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[3]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction().ApproveTokenNftAllowance(nftTokenId.Nft(nftSerials[0]), receiverAccountId, spenderAccountId).ApproveTokenNftAllowance(nftTokenId.Nft(nftSerials[1]), receiverAccountId, spenderAccountId).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the spender has allowance
                new TransferTransaction().AddApprovedNftTransfer(nftTokenId.Nft(nftSerials[0]), receiverAccountId, spenderAccountId).SetTransactionId(TransactionId.Generate(spenderAccountId)).FreezeWith(testEnv.client).Sign(spenderAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token
                new TokenRejectTransaction().SetOwnerId(receiverAccountId).SetNftIds(List.Of(nftTokenId.Nft(nftSerials[1]), nftTokenId.Nft(nftSerials[2]))).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the allowance - should be 0 , because the receiver is no longer the owner
                Assert.Throws(typeof(Exception), () =>
                {
                    new TransferTransaction().AddApprovedNftTransfer(nftTokenId.Nft(nftSerials[1]), receiverAccountId, spenderAccountId).AddApprovedNftTransfer(nftTokenId.Nft(nftSerials[2]), receiverAccountId, spenderAccountId).SetTransactionId(TransactionId.Generate(spenderAccountId)).FreezeWith(testEnv.client).Sign(spenderAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("SPENDER_DOES_NOT_HAVE_ALLOWANCE");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotRejectNftWhenUsingAddOrSetTokenId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);
                var mintReceiptNftToken = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceiptNftToken.serials;

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[2]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the whole collection (addTokenId) - should fail
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(nftTokenId).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("ACCOUNT_AMOUNT_TRANSFERS_ONLY_ALLOWED_FOR_FUNGIBLE_COMMON");

                // reject the whole collection (setTokenIds) - should fail
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("ACCOUNT_AMOUNT_TRANSFERS_ONLY_ALLOWED_FOR_FUNGIBLE_COMMON");
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotRejectTokenWhenExecutingTokenRejectAndDuplicatingTokenReference()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token with duplicate token id - should fail with TOKEN_REFERENCE_REPEATED
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).SetTokenIds(List.Of(ftTokenId, ftTokenId)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("TOKEN_REFERENCE_REPEATED");

                // same test for nft
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the nft with duplicate nft id - should fail with TOKEN_REFERENCE_REPEATED
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).SetNftIds(List.Of(nftTokenId.Nft(nftSerials[0]), nftTokenId.Nft(nftSerials[0]))).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("TOKEN_REFERENCE_REPEATED");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotRejectTokenWhenOwnerHasEmptyBalance()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);

                // skip the transfer
                // associate the receiver
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(ftTokenId)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token - should fail with INSUFFICIENT_TOKEN_BALANCE
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("INSUFFICIENT_TOKEN_BALANCE");

                // same test for nft
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // skip the transfer
                // associate the receiver
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(nftTokenId)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the nft - should fail with INVALID_OWNER_ID
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddNftId(nftTokenId.Nft(nftSerials[0])).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("INVALID_OWNER_ID");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotRejectTokenWhenTreasuryRejects()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);

                // skip the transfer
                // reject the token with the treasury - should fail with ACCOUNT_IS_TREASURY
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(testEnv.operatorId).AddTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("ACCOUNT_IS_TREASURY");

                // same test for nft
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // skip the transfer
                // reject the nft with the treasury - should fail with ACCOUNT_IS_TREASURY
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(testEnv.operatorId).AddNftId(nftTokenId.Nft(nftSerials[0])).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("ACCOUNT_IS_TREASURY");
            }
        }

        public virtual void CannotRejectTokenWithInvalidSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var randomKey = PrivateKey.GenerateED25519();
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token with different key - should fail with INVALID_SIGNATURE
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).FreezeWith(testEnv.client).Sign(randomKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("INVALID_SIGNATURE");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotRejectTokenWhenTokenOrNFTIdIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // reject the token with invalid token - should fail with EMPTY_TOKEN_REFERENCE_LIST
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(testEnv.operatorId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("EMPTY_TOKEN_REFERENCE_LIST");
            }
        }

        public virtual void CannotRejectTokenWhenTokenReferenceListSizeExceeded()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 18);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, -1);
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftTokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // transfer the tokens to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.operatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10).AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[2]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[3]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[4]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[5]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[6]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[7]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[8]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[9]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // reject the token with 11 token references - should fail with TOKEN_REFERENCE_LIST_SIZE_LIMIT_EXCEEDED
                Assert.Throws(typeof(Exception), () =>
                {
                    new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(ftTokenId).SetNftIds(List.Of(nftTokenId.Nft(nftSerials[0]), nftTokenId.Nft(nftSerials[1]), nftTokenId.Nft(nftSerials[2]), nftTokenId.Nft(nftSerials[3]), nftTokenId.Nft(nftSerials[4]), nftTokenId.Nft(nftSerials[5]), nftTokenId.Nft(nftSerials[6]), nftTokenId.Nft(nftSerials[7]), nftTokenId.Nft(nftSerials[8]), nftTokenId.Nft(nftSerials[9]))).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("TOKEN_REFERENCE_LIST_SIZE_LIMIT_EXCEEDED");
                new TokenDeleteTransaction().SetTokenId(ftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(nftTokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}