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
    public class TokenAutomaticAssociationIntegrationTest
    {
        virtual void CanTransferFungibleTokensToAccountsWithLimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, 0);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, 0);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 1;
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                var accountInfoBeforeTokenAssociation = new AccountInfoQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(accountInfoBeforeTokenAssociation.maxAutomaticTokenAssociations, 1);
                Assert.Equal(accountInfoBeforeTokenAssociation.tokenRelationships.Count, 0);
                var transferRecord = new TransferTransaction().AddTokenTransfer(tokenId1, testEnv.operatorId, -1).AddTokenTransfer(tokenId1, receiverAccountId, 1).Execute(testEnv.client).GetRecord(testEnv.client);
                Assert.Equal(transferRecord.automaticTokenAssociations.Count, 1);
                Assert.Equal(transferRecord.automaticTokenAssociations[0].accountId, receiverAccountId);
                Assert.Equal(transferRecord.automaticTokenAssociations[0].tokenId, tokenId1);
                var accountInfoAfterTokenAssociation = new AccountInfoQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(accountInfoAfterTokenAssociation.tokenRelationships.Count, 1);
                AssertThat(accountInfoAfterTokenAssociation.tokenRelationships[tokenId1].automaticAssociation).IsTrue();
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new TransferTransaction().AddTokenTransfer(tokenId2, testEnv.operatorId, -1).AddTokenTransfer(tokenId2, receiverAccountId, 1).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("NO_REMAINING_AUTOMATIC_ASSOCIATIONS");
                new AccountUpdateTransaction().SetAccountId(receiverAccountId).SetMaxAutomaticTokenAssociations(2).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var accountInfoAfterMaxAssocUpdate = new AccountInfoQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(accountInfoAfterMaxAssocUpdate.maxAutomaticTokenAssociations, 2);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanTransferNftsToAccountsWithLimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateNft(testEnv);
                var tokenId2 = EntityHelper.CreateNft(testEnv);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 1;
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                var mintReceiptToken1 = new TokenMintTransaction().SetTokenId(tokenId1).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var mintReceiptToken2 = new TokenMintTransaction().SetTokenId(tokenId2).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var accountInfoBeforeTokenAssociation = new AccountInfoQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(accountInfoBeforeTokenAssociation.maxAutomaticTokenAssociations, 1);
                Assert.Equal(accountInfoBeforeTokenAssociation.tokenRelationships.Count, 0);
                var serialsToTransfer = new List(mintReceiptToken2.serials);
                var nftTransferTransaction = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
                {
                    nftTransferTransaction.AddNftTransfer(tokenId1.Nft(serial), testEnv.operatorId, receiverAccountId);
                }

                var transferRecord = nftTransferTransaction.Execute(testEnv.client).GetRecord(testEnv.client);
                Assert.Equal(transferRecord.automaticTokenAssociations.Count, 1);
                Assert.Equal(transferRecord.automaticTokenAssociations[0].accountId, receiverAccountId);
                Assert.Equal(transferRecord.automaticTokenAssociations[0].tokenId, tokenId1);
                var accountInfoAfterTokenAssociation = new AccountInfoQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(accountInfoAfterTokenAssociation.tokenRelationships.Count, 1);
                AssertThat(accountInfoAfterTokenAssociation.tokenRelationships[tokenId1].automaticAssociation).IsTrue();
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    var serial = mintReceiptToken2.serials[0];
                    new TransferTransaction().AddNftTransfer(tokenId2.Nft(serial), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("NO_REMAINING_AUTOMATIC_ASSOCIATIONS");
                new AccountUpdateTransaction().SetAccountId(receiverAccountId).SetMaxAutomaticTokenAssociations(2).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var accountInfoAfterMaxAssocUpdate = new AccountInfoQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(accountInfoAfterMaxAssocUpdate.maxAutomaticTokenAssociations, 2);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanSetUnlimitedMaxAutoAssociationsForAccount()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = -1;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                new AccountUpdateTransaction().SetAccountId(accountId).SetMaxAutomaticTokenAssociations(accountMaxAutomaticTokenAssociations).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var accountInfoBeforeTokenAssociation = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(accountInfoBeforeTokenAssociation.maxAutomaticTokenAssociations, -1);
            }
        }

        virtual void CanTransferFungibleTokensToAccountsWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var accountKey = PrivateKey.GenerateED25519();
                var accountId1 = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                var accountId2 = EntityHelper.CreateAccount(testEnv, accountKey, 100);
                new AccountUpdateTransaction().SetAccountId(accountId2).SetMaxAutomaticTokenAssociations(-1).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer to both receivers some token1 tokens
                new TransferTransaction().AddTokenTransfer(tokenId1, testEnv.operatorId, -1000).AddTokenTransfer(tokenId1, accountId1, 1000).AddTokenTransfer(tokenId1, testEnv.operatorId, -1000).AddTokenTransfer(tokenId1, accountId2, 1000).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer to both receivers some token2 tokens
                new TransferTransaction().AddTokenTransfer(tokenId2, testEnv.operatorId, -1000).AddTokenTransfer(tokenId2, accountId1, 1000).AddTokenTransfer(tokenId2, testEnv.operatorId, -1000).AddTokenTransfer(tokenId2, accountId2, 1000).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the balance of the receivers is 1000
                var accountId1Balance = new AccountBalanceQuery().SetAccountId(accountId1).Execute(testEnv.client);
                Assert.Equal(accountId1Balance.tokens[tokenId1], 1000);
                Assert.Equal(accountId1Balance.tokens[tokenId2], 1000);
                var accountId2Balance = new AccountBalanceQuery().SetAccountId(accountId2).Execute(testEnv.client);
                Assert.Equal(accountId2Balance.tokens[tokenId1], 1000);
                Assert.Equal(accountId2Balance.tokens[tokenId2], 1000);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanTransferFungibleTokensWithDecimalsToAccountsWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenDecimals = 10;
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, tokenDecimals);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, tokenDecimals);
                var accountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                new TransferTransaction().AddTokenTransferWithDecimals(tokenId1, testEnv.operatorId, -1000, tokenDecimals).AddTokenTransferWithDecimals(tokenId1, receiverAccountId, 1000, tokenDecimals).AddTokenTransferWithDecimals(tokenId2, testEnv.operatorId, -1000, tokenDecimals).AddTokenTransferWithDecimals(tokenId2, receiverAccountId, 1000, tokenDecimals).Execute(testEnv.client).GetReceipt(testEnv.client);
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(receiverAccountBalance.tokens[tokenId1], 1000);
                Assert.Equal(receiverAccountBalance.tokens[tokenId2], 1000);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanTransferFungibleTokensOnBehalfOfOwnerToAccountWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                var spenderAccountKey = PrivateKey.GenerateED25519();
                var spenderAccountId = EntityHelper.CreateAccount(testEnv, spenderAccountKey, -1);
                new AccountAllowanceApproveTransaction().ApproveTokenAllowance(tokenId1, testEnv.operatorId, spenderAccountId, 2000).ApproveTokenAllowance(tokenId2, testEnv.operatorId, spenderAccountId, 2000).Execute(testEnv.client).GetReceipt(testEnv.client);
                var record = new TransferTransaction().AddApprovedTokenTransfer(tokenId1, testEnv.operatorId, -1000).AddTokenTransfer(tokenId1, accountId, 1000).AddApprovedTokenTransfer(tokenId2, testEnv.operatorId, -1000).AddTokenTransfer(tokenId2, accountId, 1000).SetTransactionId(TransactionId.Generate(spenderAccountId)).FreezeWith(testEnv.client).Sign(spenderAccountKey).Execute(testEnv.client).GetRecord(testEnv.client);
                var accountBalance = new AccountBalanceQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(accountBalance.tokens[tokenId1], 1000);
                Assert.Equal(accountBalance.tokens[tokenId2], 1000);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanTransferNftsToAccountsWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateNft(testEnv);
                var tokenId2 = EntityHelper.CreateNft(testEnv);
                var accountKey = PrivateKey.GenerateED25519();
                var accountId1 = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                var accountId2 = EntityHelper.CreateAccount(testEnv, accountKey, 100);
                var mintReceiptToken1 = new TokenMintTransaction().SetTokenId(tokenId1).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var mintReceiptToken2 = new TokenMintTransaction().SetTokenId(tokenId2).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceiptToken2.serials;
                new AccountUpdateTransaction().SetAccountId(accountId2).SetMaxAutomaticTokenAssociations(-1).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer nft1 to both receivers, 2 for each
                new TransferTransaction().AddNftTransfer(tokenId1.Nft(nftSerials[0]), testEnv.operatorId, accountId1).AddNftTransfer(tokenId1.Nft(nftSerials[1]), testEnv.operatorId, accountId1).AddNftTransfer(tokenId1.Nft(nftSerials[2]), testEnv.operatorId, accountId2).AddNftTransfer(tokenId1.Nft(nftSerials[3]), testEnv.operatorId, accountId2).Execute(testEnv.client).GetReceipt(testEnv.client);

                // transfer nft2 to both receivers, 2 for each
                new TransferTransaction().AddNftTransfer(tokenId2.Nft(nftSerials[0]), testEnv.operatorId, accountId1).AddNftTransfer(tokenId2.Nft(nftSerials[1]), testEnv.operatorId, accountId1).AddNftTransfer(tokenId2.Nft(nftSerials[2]), testEnv.operatorId, accountId2).AddNftTransfer(tokenId2.Nft(nftSerials[3]), testEnv.operatorId, accountId2).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the balance of the receivers is 2
                var accountId1Balance = new AccountBalanceQuery().SetAccountId(accountId1).Execute(testEnv.client);
                Assert.Equal(accountId1Balance.tokens[tokenId1], 2);
                Assert.Equal(accountId1Balance.tokens[tokenId2], 2);
                var accountId2Balance = new AccountBalanceQuery().SetAccountId(accountId2).Execute(testEnv.client);
                Assert.Equal(accountId2Balance.tokens[tokenId1], 2);
                Assert.Equal(accountId2Balance.tokens[tokenId2], 2);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanTransferNftsOnBehalfOfOwnerToAccountWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateNft(testEnv);
                var tokenId2 = EntityHelper.CreateNft(testEnv);
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                var spenderAccountKey = PrivateKey.GenerateED25519();
                var spenderAccountId = EntityHelper.CreateAccount(testEnv, spenderAccountKey, -1);
                var mintReceiptToken1 = new TokenMintTransaction().SetTokenId(tokenId1).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var mintReceiptToken2 = new TokenMintTransaction().SetTokenId(tokenId2).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceiptToken2.serials;
                new AccountAllowanceApproveTransaction().ApproveTokenNftAllowanceAllSerials(tokenId1, testEnv.operatorId, spenderAccountId).ApproveTokenNftAllowanceAllSerials(tokenId2, testEnv.operatorId, spenderAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddApprovedNftTransfer(tokenId1.Nft(nftSerials[0]), testEnv.operatorId, accountId).AddApprovedNftTransfer(tokenId1.Nft(nftSerials[1]), testEnv.operatorId, accountId).AddApprovedNftTransfer(tokenId2.Nft(nftSerials[0]), testEnv.operatorId, accountId).AddApprovedNftTransfer(tokenId2.Nft(nftSerials[1]), testEnv.operatorId, accountId).SetTransactionId(TransactionId.Generate(spenderAccountId)).FreezeWith(testEnv.client).Sign(spenderAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var accountBalance = new AccountBalanceQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(accountBalance.tokens[tokenId1], 2);
                Assert.Equal(accountBalance.tokens[tokenId2], 2);
                new TokenDeleteTransaction().SetTokenId(tokenId1).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenDeleteTransaction().SetTokenId(tokenId2).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CannotSetInvalidMaxAutoAssociationsValues()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetMaxAutomaticTokenAssociations(-2).Execute(testEnv.client);
                }).WithMessageContaining("INVALID_MAX_AUTO_ASSOCIATIONS");
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetMaxAutomaticTokenAssociations(-1000).Execute(testEnv.client);
                }).WithMessageContaining("INVALID_MAX_AUTO_ASSOCIATIONS");
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, 100);
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new AccountUpdateTransaction().SetAccountId(accountId).SetMaxAutomaticTokenAssociations(-2).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("INVALID_MAX_AUTO_ASSOCIATIONS");
                AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() =>
                {
                    new AccountUpdateTransaction().SetAccountId(accountId).SetMaxAutomaticTokenAssociations(-1000).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining("INVALID_MAX_AUTO_ASSOCIATIONS");
            }
        }
    }
}