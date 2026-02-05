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
    class TokenAirdropClaimIntegrationTest
    {
        private readonly int amount = 100;
        virtual void CanClaimTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // verify the txn record
                AssertEquals(3, record.pendingAirdropRecords.Count);
                AssertEquals(100, record.pendingAirdropRecords[0].GetPendingAirdropAmount());
                AssertEquals(tokenID, record.pendingAirdropRecords[0].GetPendingAirdropId().GetTokenId());
                Assert.Null(record.pendingAirdropRecords[0].GetPendingAirdropId().GetNftId());
                AssertEquals(0, record.pendingAirdropRecords[1].GetPendingAirdropAmount());
                AssertEquals(nftID.Nft(1), record.pendingAirdropRecords[1].GetPendingAirdropId().GetNftId());
                Assert.Null(record.pendingAirdropRecords[1].GetPendingAirdropId().GetTokenId());
                AssertEquals(0, record.pendingAirdropRecords[2].GetPendingAirdropAmount());
                AssertEquals(nftID.Nft(2), record.pendingAirdropRecords[2].GetPendingAirdropId().GetNftId());
                Assert.Null(record.pendingAirdropRecords[2].GetPendingAirdropId().GetTokenId());

                // claim the tokens with the receiver
                record = new TokenClaimAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[1].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[2].GetPendingAirdropId()).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetRecord(testEnv.client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                AssertEquals(0, record.pendingAirdropRecords.Count);

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

        virtual void CanClaimTokensToMultipleReceivers()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // create receiver1 with 0 auto associations
                var receiver1AccountKey = PrivateKey.GenerateED25519();
                var receiver1AccountId = EntityHelper.CreateAccount(testEnv, receiver1AccountKey, 0);

                // create receiver2 with 0 auto associations
                var receiver2AccountKey = PrivateKey.GenerateED25519();
                var receiver2AccountId = EntityHelper.CreateAccount(testEnv, receiver2AccountKey, 0);

                // airdrop the tokens to both
                var record = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.operatorId, receiver1AccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.operatorId, receiver1AccountId).AddTokenTransfer(tokenID, receiver1AccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).AddNftTransfer(nftID.Nft(nftSerials[2]), testEnv.operatorId, receiver2AccountId).AddNftTransfer(nftID.Nft(nftSerials[3]), testEnv.operatorId, receiver2AccountId).AddTokenTransfer(tokenID, receiver2AccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // verify the txn record
                AssertEquals(6, record.pendingAirdropRecords.Count);

                // claim the tokens signing with receiver1 and receiver2
                var pendingAirdropIDs = record.pendingAirdropRecords.Stream().Map(PendingAirdropRecord.GetPendingAirdropId()).ToList();
                record = new TokenClaimAirdropTransaction().SetPendingAirdropIds(pendingAirdropIDs).FreezeWith(testEnv.client).Sign(receiver1AccountKey).Sign(receiver2AccountKey).Execute(testEnv.client).GetRecord(testEnv.client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                AssertEquals(0, record.pendingAirdropRecords.Count);

                // verify receiver1 holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiver1AccountId).Execute(testEnv.client);
                AssertEquals(amount, receiverAccountBalance.tokens[tokenID]);
                AssertEquals(2, receiverAccountBalance.tokens[nftID]);

                // verify receiver2 holds the tokens via query
                var receiver2AccountBalance = new AccountBalanceQuery().SetAccountId(receiver1AccountId).Execute(testEnv.client);
                AssertEquals(amount, receiver2AccountBalance.tokens[tokenID]);
                AssertEquals(2, receiver2AccountBalance.tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                AssertEquals(fungibleInitialBalance - amount * 2, operatorBalance.tokens[tokenID]);
                AssertEquals(mitedNfts - 4, operatorBalance.tokens[nftID]);
            }
        }

        virtual void CanClaimTokensFromMultipleAirdropTxns()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = mintReceipt.serials;

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop some of the tokens to the receiver
                var record1 = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetRecord(testEnv.client);

                // airdrop some of the tokens to the receiver
                var record2 = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.operatorId, receiverAccountId).Execute(testEnv.client).GetRecord(testEnv.client);

                // airdrop some of the tokens to the receiver
                var record3 = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // get the PendingIds from the records
                var pendingAirdropIDs = new List<PendingAirdropId>();
                pendingAirdropIDs.Add(record1.pendingAirdropRecords[0].GetPendingAirdropId());
                pendingAirdropIDs.Add(record2.pendingAirdropRecords[0].GetPendingAirdropId());
                pendingAirdropIDs.Add(record3.pendingAirdropRecords[0].GetPendingAirdropId());

                // claim the all the tokens with the receiver
                var record = new TokenClaimAirdropTransaction().SetPendingAirdropIds(pendingAirdropIDs).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetRecord(testEnv.client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                AssertEquals(0, record.pendingAirdropRecords.Count);

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

        virtual void CannotClaimTokensForNonExistingAirdrop()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // claim the tokens with the operator which does not have pending airdrops
                // fails with INVALID_SIGNATURE
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenClaimAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotClaimTokensForAlreadyClaimedAirdrop()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // claim the tokens with the receiver
                new TokenClaimAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetRecord(testEnv.client);

                // claim the tokens with the receiver again
                // fails with INVALID_PENDING_AIRDROP_ID
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenClaimAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.INVALID_PENDING_AIRDROP_ID.ToString());
            }
        }

        virtual void CannotClaimWithEmptyPendingAirdropsList()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // claim the tokens with the receiver without setting pendingAirdropIds
                // fails with EMPTY_PENDING_AIRDROP_ID_LIST
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenClaimAirdropTransaction().Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.EMPTY_PENDING_AIRDROP_ID_LIST.ToString());
            }
        }

        virtual void CannotClaimTokensWithDuplicateEntries()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // claim the tokens with duplicate pending airdrop token ids
                // fails with PENDING_AIRDROP_ID_REPEATED
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenClaimAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.PENDING_AIRDROP_ID_REPEATED.ToString());
            }
        }

        virtual void CannotClaimTokensWhenTokenIsPaused()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // pause the token
                new TokenPauseTransaction().SetTokenId(tokenID).Execute(testEnv.client).GetReceipt(testEnv.client);

                // claim the tokens with receiver
                // fails with TOKEN_IS_PAUSED
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenClaimAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_PAUSED.ToString());
            }
        }

        virtual void CannotClaimTokensWhenTokenIsDeleted()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // delete the token
                new TokenDeleteTransaction().SetTokenId(tokenID).Execute(testEnv.client).GetReceipt(testEnv.client);

                // claim the tokens with receiver
                // fails with TOKEN_IS_DELETED
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenClaimAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_WAS_DELETED.ToString());
            }
        }

        virtual void CannotClaimTokensWhenTokenIsFrozen()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.operatorId, -amount).Execute(testEnv.client).GetRecord(testEnv.client);

                // associate
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(tokenID)).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // freeze the token
                new TokenFreezeTransaction().SetAccountId(receiverAccountId).SetTokenId(tokenID).Execute(testEnv.client).GetReceipt(testEnv.client);

                // claim the tokens with receiver
                // fails with ACCOUNT_FROZEN_FOR_TOKEN
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenClaimAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).FreezeWith(testEnv.client).Sign(receiverAccountKey).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.ACCOUNT_FROZEN_FOR_TOKEN.ToString());
            }
        }
    }
}