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
        public virtual void CanAirdropAssociatedTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver with unlimited auto associations and receiverSig = false
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, -1);

                // airdrop the tokens
                new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the receiver holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.Client);
                Assert.Equal(amount, receiverAccountBalance.tokens[tokenID]);
                Assert.Equal(2, receiverAccountBalance.tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
                Assert.Equal(fungibleInitialBalance - amount, operatorBalance.tokens[tokenID]);
                Assert.Equal(mitedNfts - 2, operatorBalance.tokens[nftID]);
            }
        }

        public virtual void CanAirdropNonAssociatedTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver with 0 auto associations and receiverSig = false
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var txn = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client);
                txn.SetValidateStatus(true).GetReceipt(testEnv.Client);
                var record = txn.GetRecord(testEnv.Client);

                // verify in the transaction record the pending airdrops
                Assert.NotNull(record.pendingAirdropRecords);
                Assert.False(record.pendingAirdropRecords.IsEmpty());

                // verify the receiver does not hold the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.Client);
                Assert.Null(receiverAccountBalance.tokens[tokenID]);
                Assert.Null(receiverAccountBalance.tokens[nftID]);

                // verify the operator does hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
                Assert.Equal(fungibleInitialBalance, operatorBalance.tokens[tokenID]);
                Assert.Equal(mitedNfts, operatorBalance.tokens[nftID]);
            }
        }

        public virtual void CanAirdropToAlias()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // airdrop the tokens to an alias
                PrivateKey privateKey = PrivateKey.GenerateED25519();
                PublicKey publicKey = privateKey.GetPublicKey();
                AccountId aliasAccountId = publicKey.ToAccountId(0, 0);

                // should lazy-create and transfer the tokens
                new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, aliasAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, aliasAccountId).AddTokenTransfer(tokenID, aliasAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the receiver holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(aliasAccountId).Execute(testEnv.Client);
                Assert.Equal(amount, receiverAccountBalance.tokens[tokenID]);
                Assert.Equal(2, receiverAccountBalance.tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
                Assert.Equal(fungibleInitialBalance - amount, operatorBalance.tokens[tokenID]);
                Assert.Equal(mitedNfts - 2, operatorBalance.tokens[nftID]);
            }
        }

        public virtual void CanAirdropWithCustomFee()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create receiver unlimited auto associations and receiverSig = false
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, -1);

                // create fungible token with custom fee another token
                var customFeeTokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // make the custom fee to be paid by the sender and the fee collector to be the operator account
                CustomFixedFee fee = new CustomFixedFee().SetFeeCollectorAccountId(testEnv.OperatorId).SetDenominatingTokenId(customFeeTokenID).SetAmount(1).SetAllCollectorsAreExempt(true);
                var tokenID = new TokenCreateTransaction().SetTokenName("Test Fungible Token").SetTokenSymbol("TFT").SetTokenMemo("I was created for integration tests")Decimals = 3,.SetInitialSupply(fungibleInitialBalance).SetMaxSupply(fungibleInitialBalance)TreasuryAccountId = testEnv.OperatorId,.SetSupplyType(TokenSupplyType.FINITE)AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,.SetMetadataKey(testEnv.OperatorKey).SetPauseKey(testEnv.OperatorKey).SetCustomFees(Collections.SingletonList(fee)).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                // create sender account with unlimited associations and send some tokens to it
                var senderKey = PrivateKey.GenerateED25519();
                var senderAccountID = EntityHelper.CreateAccount(testEnv, senderKey, -1);

                // associate the token to the sender
                new TokenAssociateTransaction().SetAccountId(senderAccountID).SetTokenIds(Collections.SingletonList(customFeeTokenID)).FreezeWith(testEnv.Client).Sign(senderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // send tokens to the sender
                new TransferTransaction().AddTokenTransfer(customFeeTokenID, testEnv.OperatorId, -amount).AddTokenTransfer(customFeeTokenID, senderAccountID, amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).AddTokenTransfer(tokenID, senderAccountID, amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // airdrop the tokens from the sender to the receiver
                new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, senderAccountID, -amount).FreezeWith(testEnv.Client).Sign(senderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the custom fee has been paid by the sender to the collector
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.Client);
                Assert.Equal(amount, receiverAccountBalance.tokens[tokenID]);
                var senderAccountBalance = new AccountBalanceQuery().SetAccountId(senderAccountID).Execute(testEnv.Client);
                Assert.Equal(0, senderAccountBalance.tokens[tokenID]);
                Assert.Equal(amount - 1, senderAccountBalance.tokens[customFeeTokenID]);
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
                Assert.Equal(fungibleInitialBalance - amount + 1, operatorBalance.tokens[customFeeTokenID]);
                Assert.Equal(fungibleInitialBalance - amount, operatorBalance.tokens[tokenID]);
            }
        }

        public virtual void CanAirdropTokensWithReceiverSigRequiredFungible()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with unlimited auto associations and receiverSig = true
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverAccountKey).SetInitialBalance(new Hbar(1)).SetReceiverSignatureRequired(true).SetMaxAutomaticTokenAssociations(-1).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // airdrop the tokens
                new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanAirdropTokensWithReceiverSigRequiredNFT()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create nft
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver with unlimited auto associations and receiverSig = true
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverAccountKey).SetInitialBalance(new Hbar(1)).SetReceiverSignatureRequired(true).SetMaxAutomaticTokenAssociations(-1).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // airdrop the tokens
                new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotAirdropTokensWithAllowanceAndWithoutBalanceFungible()
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
                new TransferTransaction().AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).AddTokenTransfer(tokenID, senderAccountID, amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction().ApproveTokenAllowance(tokenID, senderAccountID, spenderAccountID, amount).FreezeWith(testEnv.Client).Sign(senderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // airdrop the tokens from the sender to the spender via approval
                // fails with NOT_SUPPORTED
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenAirdropTransaction().AddTokenTransfer(tokenID, spenderAccountID, amount).AddApprovedTokenTransfer(tokenID, spenderAccountID, -amount).SetTransactionId(TransactionId.Generate(spenderAccountID)).FreezeWith(testEnv.Client).Sign(spenderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.NOT_SUPPORTED.ToString());
            }
        }

        public virtual void CannotAirdropTokensWithAllowanceAndWithoutBalanceNFT()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create nft
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create spender and approve to it some tokens
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountID = EntityHelper.CreateAccount(testEnv, spenderKey, -1);

                // create sender
                var senderKey = PrivateKey.GenerateED25519();
                var senderAccountID = EntityHelper.CreateAccount(testEnv, senderKey, -1);

                // transfer ft to sender
                new TransferTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, senderAccountID).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, senderAccountID).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction().ApproveTokenNftAllowance(nftID.Nft(nftSerials[0]), senderAccountID, spenderAccountID).ApproveTokenNftAllowance(nftID.Nft(nftSerials[1]), senderAccountID, spenderAccountID).FreezeWith(testEnv.Client).Sign(senderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // airdrop the tokens from the sender to the spender via approval
                // fails with NOT_SUPPORTED
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenAirdropTransaction().AddApprovedNftTransfer(nftID.Nft(nftSerials[0]), senderAccountID, spenderAccountID).AddApprovedNftTransfer(nftID.Nft(nftSerials[1]), senderAccountID, spenderAccountID).SetTransactionId(TransactionId.Generate(spenderAccountID)).FreezeWith(testEnv.Client).Sign(spenderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.NOT_SUPPORTED.ToString());
            }
        }

        public virtual void CannotAirdropTokensWithInvalidBody()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // airdrop with no tokenID or NftID
                // fails with EMPTY_TOKEN_TRANSFER_BODY
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenAirdropTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.EMPTY_TOKEN_TRANSFER_BODY.ToString());

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // airdrop with invalid transfers
                // fails with INVALID_TRANSACTION_BODY
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenAirdropTransaction().AddTokenTransfer(tokenID, testEnv.OperatorId, 100).AddTokenTransfer(tokenID, testEnv.OperatorId, 100).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_TRANSACTION_BODY.ToString());
            }
        }
    }
}