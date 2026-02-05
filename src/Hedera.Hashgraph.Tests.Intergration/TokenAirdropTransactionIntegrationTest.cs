// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Test.Integration.EntityHelper;
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
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
    class TokenAirdropTransactionIntegrationTest
    {
        private readonly int amount = 100;
        virtual void CanAirdropAssociatedTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // create receiver with unlimited auto associations and receiverSig = false
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, -1);

                // airdrop the tokens
                new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the receiver holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                AssertEquals(amount, receiverAccountBalance.tokens[tokenID]);
                AssertEquals(2, receiverAccountBalance.tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                AssertEquals(fungibleInitialBalance - amount, operatorBalance.tokens[tokenID]);
                AssertEquals(mitedNfts - 2, operatorBalance.tokens[nftID]);
            }
        }

        virtual void CanAirdropNonAssociatedTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // create receiver with 0 auto associations and receiverSig = false
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var txn = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client);
                txn.SetValidateStatus(true).GetReceipt(testEnv.client);
                var record = txn.GetRecord(testEnv.client);

                // verify in the transaction record the pending airdrops
                AssertThat(record.pendingAirdropRecords).IsNotNull();
                Assert.False(record.pendingAirdropRecords.IsEmpty());

                // verify the receiver does not hold the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Null(receiverAccountBalance.tokens[tokenID]);
                Assert.Null(receiverAccountBalance.tokens[nftID]);

                // verify the operator does hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                AssertEquals(fungibleInitialBalance, operatorBalance.tokens[tokenID]);
                AssertEquals(mitedNfts, operatorBalance.tokens[nftID]);
            }
        }

        virtual void CanAirdropToAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // airdrop the tokens to an alias
                PrivateKey privateKey = PrivateKey.GenerateED25519();
                PublicKey publicKey = privateKey.GetPublicKey();
                AccountId aliasAccountId = publicKey.ToAccountId(0, 0);

                // should lazy-create and transfer the tokens
                new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.operatorId, aliasAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.operatorId, aliasAccountId).AddTokenTransfer(tokenID, aliasAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the receiver holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(aliasAccountId).Execute(testEnv.client);
                AssertEquals(amount, receiverAccountBalance.tokens[tokenID]);
                AssertEquals(2, receiverAccountBalance.tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                AssertEquals(fungibleInitialBalance - amount, operatorBalance.tokens[tokenID]);
                AssertEquals(mitedNfts - 2, operatorBalance.tokens[nftID]);
            }
        }

        virtual void CanAirdropWithCustomFee()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create receiver unlimited auto associations and receiverSig = false
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, -1);

                // create fungible token with custom fee another token
                var customFeeTokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // make the custom fee to be paid by the sender and the fee collector to be the operator account
                CustomFixedFee fee = new CustomFixedFee().SetFeeCollectorAccountId(testEnv.operatorId).SetDenominatingTokenId(customFeeTokenID).SetAmount(1).SetAllCollectorsAreExempt(true);
                var tokenID = new TokenCreateTransaction().SetTokenName("Test Fungible Token").SetTokenSymbol("TFT").SetTokenMemo("I was created for integration tests").SetDecimals(3).SetInitialSupply(fungibleInitialBalance).SetMaxSupply(fungibleInitialBalance).SetTreasuryAccountId(testEnv.operatorId).SetSupplyType(TokenSupplyType.FINITE).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(testEnv.operatorKey).SetPauseKey(testEnv.operatorKey).SetCustomFees(Collections.SingletonList(fee)).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;

                // create sender account with unlimited associations and send some tokens to it
                var senderKey = PrivateKey.GenerateED25519();
                var senderAccountID = EntityHelper.CreateAccount(testEnv, senderKey, -1);

                // associate the token to the sender
                new TokenAssociateTransaction().SetAccountId(senderAccountID).SetTokenIds(Collections.SingletonList(customFeeTokenID)).FreezeWith(testEnv.client).Sign(senderKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // send tokens to the sender
                new TransferTransaction().AddTokenTransfer(customFeeTokenID, testEnv.operatorId, -amount).AddTokenTransfer(customFeeTokenID, senderAccountID, amount).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddTokenTransfer(tokenID, testEnv.operatorId, -amount).AddTokenTransfer(tokenID, senderAccountID, amount).Execute(testEnv.client).GetReceipt(testEnv.client);

                // airdrop the tokens from the sender to the receiver
                new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, senderAccountID, -amount).FreezeWith(testEnv.client).Sign(senderKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // verify the custom fee has been paid by the sender to the collector
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                AssertEquals(amount, receiverAccountBalance.tokens[tokenID]);
                var senderAccountBalance = new AccountBalanceQuery().SetAccountId(senderAccountID).Execute(testEnv.client);
                AssertEquals(0, senderAccountBalance.tokens[tokenID]);
                AssertEquals(amount - 1, senderAccountBalance.tokens[customFeeTokenID]);
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                AssertEquals(fungibleInitialBalance - amount + 1, operatorBalance.tokens[customFeeTokenID]);
                AssertEquals(fungibleInitialBalance - amount, operatorBalance.tokens[tokenID]);
            }
        }

        virtual void CanAirdropTokensWithReceiverSigRequiredFungible()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with unlimited auto associations and receiverSig = true
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverAccountKey).SetInitialBalance(new Hbar(1)).SetReceiverSignatureRequired(true).SetMaxAutomaticTokenAssociations(-1).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // airdrop the tokens
                new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanAirdropTokensWithReceiverSigRequiredNFT()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create nft
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // create receiver with unlimited auto associations and receiverSig = true
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverAccountKey).SetInitialBalance(new Hbar(1)).SetReceiverSignatureRequired(true).SetMaxAutomaticTokenAssociations(-1).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // airdrop the tokens
                new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CannotAirdropTokensWithAllowanceAndWithoutBalanceFungible()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create spender and approve to it some tokens
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountID = EntityHelper.CreateAccount(testEnv, spenderKey, -1);

                // create sender
                var senderKey = PrivateKey.GenerateED25519();
                var senderAccountID = EntityHelper.CreateAccount(testEnv, senderKey, -1);

                // transfer ft to sender
                new TransferTransaction().AddTokenTransfer(tokenID, testEnv.operatorId, -amount).AddTokenTransfer(tokenID, senderAccountID, amount).Execute(testEnv.client).GetReceipt(testEnv.client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction().ApproveTokenAllowance(tokenID, senderAccountID, spenderAccountID, amount).FreezeWith(testEnv.client).Sign(senderKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // airdrop the tokens from the sender to the spender via approval
                // fails with NOT_SUPPORTED
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenAirdropTransaction().AddTokenTransfer(tokenID, spenderAccountID, amount).AddApprovedTokenTransfer(tokenID, spenderAccountID, -amount).SetTransactionId(TransactionId.Generate(spenderAccountID)).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.NOT_SUPPORTED.ToString());
            }
        }

        virtual void CannotAirdropTokensWithAllowanceAndWithoutBalanceNFT()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create nft
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // create spender and approve to it some tokens
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountID = EntityHelper.CreateAccount(testEnv, spenderKey, -1);

                // create sender
                var senderKey = PrivateKey.GenerateED25519();
                var senderAccountID = EntityHelper.CreateAccount(testEnv, senderKey, -1);

                // transfer ft to sender
                new TransferTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.operatorId, senderAccountID).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.operatorId, senderAccountID).Execute(testEnv.client).GetReceipt(testEnv.client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction().ApproveTokenNftAllowance(nftID.Nft(nftSerials[0]), senderAccountID, spenderAccountID).ApproveTokenNftAllowance(nftID.Nft(nftSerials[1]), senderAccountID, spenderAccountID).FreezeWith(testEnv.client).Sign(senderKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // airdrop the tokens from the sender to the spender via approval
                // fails with NOT_SUPPORTED
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenAirdropTransaction().AddApprovedNftTransfer(nftID.Nft(nftSerials[0]), senderAccountID, spenderAccountID).AddApprovedNftTransfer(nftID.Nft(nftSerials[1]), senderAccountID, spenderAccountID).SetTransactionId(TransactionId.Generate(spenderAccountID)).FreezeWith(testEnv.client).Sign(spenderKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.NOT_SUPPORTED.ToString());
            }
        }

        virtual void CannotAirdropTokensWithInvalidBody()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // airdrop with no tokenID or NftID
                // fails with EMPTY_TOKEN_TRANSFER_BODY
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenAirdropTransaction().Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.EMPTY_TOKEN_TRANSFER_BODY.ToString());

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // airdrop with invalid transfers
                // fails with INVALID_TRANSACTION_BODY
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenAirdropTransaction().AddTokenTransfer(tokenID, testEnv.operatorId, 100).AddTokenTransfer(tokenID, testEnv.operatorId, 100).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_TRANSACTION_BODY.ToString());
            }
        }
    }
}