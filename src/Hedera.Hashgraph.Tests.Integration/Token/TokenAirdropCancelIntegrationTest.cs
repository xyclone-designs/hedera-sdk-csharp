// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Test.Integration.EntityHelper;
using Org.Assertj.Core.Api.AssertionsForClassTypes;
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
    class TokenAirdropCancelIntegrationTest
    {
        private readonly int amount = 100;
        public virtual void CanCancelTokens()
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

                // sender cancels the tokens
                record = new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[1].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[2].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.pendingAirdropRecords.Count);

                // verify the receiver does not hold the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Null(receiverAccountBalance.tokens[tokenID]);
                Assert.Null(receiverAccountBalance.tokens[nftID]);

                // verify the operator does hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(fungibleInitialBalance, operatorBalance.tokens[tokenID]);
                Assert.Equal(mitedNfts, operatorBalance.tokens[nftID]);
            }
        }

        public virtual void CanCancelTokensWhenTokenIsFrozen()
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

                // cancel
                new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);
            }
        }

        public virtual void CanCancelTokensWhenTokenIsPaused()
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

                // cancel
                new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);
            }
        }

        public virtual void CanCancelTokensWhenTokenIsDeleted()
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

                // cancel
                new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);
            }
        }

        public virtual void CanCancelTokensToMultipleReceivers()
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
                Assert.Equal(6, record.pendingAirdropRecords.Count);

                // cancel the tokens signing with receiver1 and receiver2
                var pendingAirdropIDs = record.pendingAirdropRecords.Stream().Map(PendingAirdropRecord.GetPendingAirdropId()).ToList();
                record = new TokenCancelAirdropTransaction().SetPendingAirdropIds(pendingAirdropIDs).Execute(testEnv.client).GetRecord(testEnv.client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.pendingAirdropRecords.Count);

                // verify receiver1 does not hold the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiver1AccountId).Execute(testEnv.client);
                Assert.Null(receiverAccountBalance.tokens[tokenID]);
                Assert.Null(receiverAccountBalance.tokens[nftID]);

                // verify receiver2 does not hold the tokens via query
                var receiver2AccountBalance = new AccountBalanceQuery().SetAccountId(receiver1AccountId).Execute(testEnv.client);
                Assert.Null(receiver2AccountBalance.tokens[tokenID]);
                Assert.Null(receiver2AccountBalance.tokens[nftID]);

                // verify the operator does hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(fungibleInitialBalance, operatorBalance.tokens[tokenID]);
                Assert.Equal(mitedNfts, operatorBalance.tokens[nftID]);
            }
        }

        public virtual void CanCancelTokensFromMultipleAirdropTxns()
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

                // cancel the all the tokens with the receiver
                var record = new TokenCancelAirdropTransaction().SetPendingAirdropIds(pendingAirdropIDs).Execute(testEnv.client).GetRecord(testEnv.client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.pendingAirdropRecords.Count);

                // verify the receiver does not hold the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Null(receiverAccountBalance.tokens[tokenID]);
                Assert.Null(receiverAccountBalance.tokens[nftID]);

                // verify the operator does hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(fungibleInitialBalance, operatorBalance.tokens[tokenID]);
                Assert.Equal(mitedNfts, operatorBalance.tokens[nftID]);
            }
        }

        public virtual void CannotCancelTokensForNonExistingAirdrop()
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

                // create receiver with 0 auto associations
                var randomAccountKey = PrivateKey.GenerateED25519();
                var randomAccount = EntityHelper.CreateAccount(testEnv, randomAccountKey, 0);

                // cancel the tokens with the random account which has not created pending airdrops
                // fails with INVALID_SIGNATURE
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenCancelAirdropTransaction().SetTransactionId(TransactionId.Generate(randomAccount)).AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CanonCancelTokensForAlreadyCanceledAirdrop()
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

                // cancel the tokens with the receiver
                new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);

                // cancel the tokens with the receiver again
                // fails with INVALID_PENDING_AIRDROP_ID
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.INVALID_PENDING_AIRDROP_ID.ToString());
            }
        }

        public virtual void CanonCancelWithEmptyPendingAirdropsList()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // cancel the tokens with the receiver without setting pendingAirdropIds
                // fails with EMPTY_PENDING_AIRDROP_ID_LIST
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenCancelAirdropTransaction().Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.EMPTY_PENDING_AIRDROP_ID_LIST.ToString());
            }
        }

        public virtual void CannotCancelTokensWithDuplicateEntries()
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

                // cancel the tokens with duplicate pending airdrop token ids
                // fails with PENDING_AIRDROP_ID_REPEATED
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.client).GetRecord(testEnv.client);
                }).WithMessageContaining(Status.PENDING_AIRDROP_ID_REPEATED.ToString());
            }
        }
    }
}