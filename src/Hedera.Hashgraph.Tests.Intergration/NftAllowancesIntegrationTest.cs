// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Nio.Charset;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class NftAllowancesIntegrationTest
    {
        virtual void CannotTransferWithoutAllowanceApproval()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(spenderKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var receiverKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverKey).SetMaxAutomaticTokenAssociations(10).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                TokenId nftTokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                new TokenAssociateTransaction().SetAccountId(spenderAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client);
                var serials = new TokenMintTransaction().SetTokenId(nftTokenId).AddMetadata("asd".GetBytes(StandardCharsets.UTF_8)).Execute(testEnv.client).GetReceipt(testEnv.client).serials;
                var nft1 = new NftId(nftTokenId, serials[0]);
                var onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new TransferTransaction().AddApprovedNftTransfer(nft1, testEnv.operatorId, receiverAccountId).SetTransactionId(onBehalfOfTransactionId).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.SPENDER_DOES_NOT_HAVE_ALLOWANCE.ToString());
            }
        }

        virtual void CannotTransferAfterAllowanceRemove()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(spenderKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var receiverKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                TokenId nftTokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                new TokenAssociateTransaction().SetAccountId(spenderAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.client).Sign(receiverKey).Execute(testEnv.client);
                var serials = new TokenMintTransaction().SetTokenId(nftTokenId).AddMetadata("asd1".GetBytes(StandardCharsets.UTF_8)).AddMetadata("asd2".GetBytes(StandardCharsets.UTF_8)).Execute(testEnv.client).GetReceipt(testEnv.client).serials;
                var nft1 = new NftId(nftTokenId, serials[0]);
                var nft2 = new NftId(nftTokenId, serials[1]);
                new AccountAllowanceApproveTransaction().ApproveTokenNftAllowance(nft1, testEnv.operatorId, spenderAccountId).ApproveTokenNftAllowance(nft2, testEnv.operatorId, spenderAccountId).Execute(testEnv.client);
                new AccountAllowanceDeleteTransaction().DeleteAllTokenNftAllowances(nft2, testEnv.operatorId).Execute(testEnv.client);
                var onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);
                new TransferTransaction().AddApprovedNftTransfer(nft1, testEnv.operatorId, receiverAccountId).SetTransactionId(onBehalfOfTransactionId).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var info = new TokenNftInfoQuery().SetNftId(nft1).Execute(testEnv.client);
                Assert.Equal(info[0].accountId, receiverAccountId);
                var onBehalfOfTransactionId2 = TransactionId.Generate(spenderAccountId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new TransferTransaction().AddApprovedNftTransfer(nft2, testEnv.operatorId, receiverAccountId).SetTransactionId(onBehalfOfTransactionId2).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.SPENDER_DOES_NOT_HAVE_ALLOWANCE.ToString());
            }
        }

        virtual void CannotRemoveSingleSerialWhenAllowanceIsGivenForAll()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(spenderKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var receiverKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                TokenId nftTokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                new TokenAssociateTransaction().SetAccountId(spenderAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.client).Sign(receiverKey).Execute(testEnv.client);
                var serials = new TokenMintTransaction().SetTokenId(nftTokenId).AddMetadata("asd1".GetBytes(StandardCharsets.UTF_8)).AddMetadata("asd2".GetBytes(StandardCharsets.UTF_8)).Execute(testEnv.client).GetReceipt(testEnv.client).serials;
                var nft1 = new NftId(nftTokenId, serials[0]);
                var nft2 = new NftId(nftTokenId, serials[1]);
                new AccountAllowanceApproveTransaction().ApproveTokenNftAllowanceAllSerials(nftTokenId, testEnv.operatorId, spenderAccountId).Execute(testEnv.client);
                var onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);
                new TransferTransaction().AddApprovedNftTransfer(nft1, testEnv.operatorId, receiverAccountId).SetTransactionId(onBehalfOfTransactionId).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // hopefully in the future this should end up with a precheck error provided from services
                new AccountAllowanceDeleteTransaction().DeleteAllTokenNftAllowances(nft2, testEnv.operatorId).Execute(testEnv.client);
                var onBehalfOfTransactionId2 = TransactionId.Generate(spenderAccountId);
                new TransferTransaction().AddApprovedNftTransfer(nft2, testEnv.operatorId, receiverAccountId).SetTransactionId(onBehalfOfTransactionId2).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var infoNft1 = new TokenNftInfoQuery().SetNftId(nft1).Execute(testEnv.client);
                var infoNft2 = new TokenNftInfoQuery().SetNftId(nft2).Execute(testEnv.client);
                Assert.Equal(infoNft1[0].accountId, receiverAccountId);
                Assert.Equal(infoNft2[0].accountId, receiverAccountId);
            }
        }

        virtual void AccountGivenAllowanceForAllShouldBeAbleToGiveAllowanceForSingle()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var delegatingSpenderKey = PrivateKey.GenerateED25519();
                var delegatingSpenderAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(delegatingSpenderKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(spenderKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var receiverKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                TokenId nftTokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                new TokenAssociateTransaction().SetAccountId(delegatingSpenderAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.client).Sign(receiverKey).Execute(testEnv.client);
                var serials = new TokenMintTransaction().SetTokenId(nftTokenId).AddMetadata("asd1".GetBytes(StandardCharsets.UTF_8)).AddMetadata("asd2".GetBytes(StandardCharsets.UTF_8)).Execute(testEnv.client).GetReceipt(testEnv.client).serials;
                var nft1 = new NftId(nftTokenId, serials[0]);
                var nft2 = new NftId(nftTokenId, serials[1]);
                new AccountAllowanceApproveTransaction().ApproveTokenNftAllowanceAllSerials(nftTokenId, testEnv.operatorId, delegatingSpenderAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new AccountAllowanceApproveTransaction().ApproveTokenNftAllowance(nft1, testEnv.operatorId, spenderAccountId, delegatingSpenderAccountId).FreezeWith(testEnv.client).Sign(delegatingSpenderKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);
                new TransferTransaction().AddApprovedNftTransfer(nft1, testEnv.operatorId, receiverAccountId).SetTransactionId(onBehalfOfTransactionId).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var onBehalfOfTransactionId2 = TransactionId.Generate(spenderAccountId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new TransferTransaction().AddApprovedNftTransfer(nft2, testEnv.operatorId, receiverAccountId).SetTransactionId(onBehalfOfTransactionId2).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.SPENDER_DOES_NOT_HAVE_ALLOWANCE.ToString());
                var infoNft1 = new TokenNftInfoQuery().SetNftId(nft1).Execute(testEnv.client);
                var infoNft2 = new TokenNftInfoQuery().SetNftId(nft2).Execute(testEnv.client);
                Assert.Equal(infoNft1[0].accountId, receiverAccountId);
                Assert.Equal(infoNft2[0].accountId, testEnv.operatorId);
            }
        }
    }
}