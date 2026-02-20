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
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // sender cancels the tokens
                record = new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[1].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[2].GetPendingAirdropId()).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.pendingAirdropRecords.Count);

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
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // associate
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds([tokenID]).FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // freeze the token
                new TokenFreezeTransaction().SetAccountId(receiverAccountId).SetTokenId(tokenID).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // cancel
                new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.Client).GetRecord(testEnv.Client);
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
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // pause the token
                new TokenPauseTransaction().SetTokenId(tokenID).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // cancel
                new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.Client).GetRecord(testEnv.Client);
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
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // delete the token
                new TokenDeleteTransaction().SetTokenId(tokenID).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // cancel
                new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.Client).GetRecord(testEnv.Client);
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
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver1 with 0 auto associations
                var receiver1AccountKey = PrivateKey.GenerateED25519();
                var receiver1AccountId = EntityHelper.CreateAccount(testEnv, receiver1AccountKey, 0);

                // create receiver2 with 0 auto associations
                var receiver2AccountKey = PrivateKey.GenerateED25519();
                var receiver2AccountId = EntityHelper.CreateAccount(testEnv, receiver2AccountKey, 0);

                // airdrop the tokens to both
                var record = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiver1AccountId).AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiver1AccountId).AddTokenTransfer(tokenID, receiver1AccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).AddNftTransfer(nftID.Nft(nftSerials[2]), testEnv.OperatorId, receiver2AccountId).AddNftTransfer(nftID.Nft(nftSerials[3]), testEnv.OperatorId, receiver2AccountId).AddTokenTransfer(tokenID, receiver2AccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify the txn record
                Assert.Equal(6, record.pendingAirdropRecords.Count);

                // cancel the tokens signing with receiver1 and receiver2
                var pendingAirdropIDs = record.pendingAirdropRecords.Stream().Map(PendingAirdropRecord.GetPendingAirdropId()).ToList();
                record = new TokenCancelAirdropTransaction().SetPendingAirdropIds(pendingAirdropIDs).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.pendingAirdropRecords.Count);

                // verify receiver1 does not hold the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery().SetAccountId(receiver1AccountId).Execute(testEnv.Client);
                Assert.Null(receiverAccountBalance.tokens[tokenID]);
                Assert.Null(receiverAccountBalance.tokens[nftID]);

                // verify receiver2 does not hold the tokens via query
                var receiver2AccountBalance = new AccountBalanceQuery().SetAccountId(receiver1AccountId).Execute(testEnv.Client);
                Assert.Null(receiver2AccountBalance.tokens[tokenID]);
                Assert.Null(receiver2AccountBalance.tokens[nftID]);

                // verify the operator does hold the tokens
                var operatorBalance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
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
                var mintReceipt = new TokenMintTransaction().SetTokenId(nftID).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop some of the tokens to the receiver
                var record1 = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // airdrop some of the tokens to the receiver
                var record2 = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // airdrop some of the tokens to the receiver
                var record3 = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // get the PendingIds from the records
                var pendingAirdropIDs = new List<PendingAirdropId>();
                pendingAirdropIDs.Add(record1.pendingAirdropRecords[0].GetPendingAirdropId());
                pendingAirdropIDs.Add(record2.pendingAirdropRecords[0].GetPendingAirdropId());
                pendingAirdropIDs.Add(record3.pendingAirdropRecords[0].GetPendingAirdropId());

                // cancel the all the tokens with the receiver
                var record = new TokenCancelAirdropTransaction().SetPendingAirdropIds(pendingAirdropIDs).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.pendingAirdropRecords.Count);

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
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // create receiver with 0 auto associations
                var randomAccountKey = PrivateKey.GenerateED25519();
                var randomAccount = EntityHelper.CreateAccount(testEnv, randomAccountKey, 0);

                // cancel the tokens with the random account which has not created pending airdrops
                // fails with INVALID_SIGNATURE
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenCancelAirdropTransaction().SetTransactionId(TransactionId.Generate(randomAccount)).AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.Client).GetRecord(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
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
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // cancel the tokens with the receiver
                new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // cancel the tokens with the receiver again
                // fails with INVALID_PENDING_AIRDROP_ID
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.Client).GetRecord(testEnv.Client);
                }); Assert.Contains(ResponseStatus.INVALID_PENDING_AIRDROP_ID.ToString(), exception.Message);
            }
        }

        public virtual void CanonCancelWithEmptyPendingAirdropsList()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // cancel the tokens with the receiver without setting pendingAirdropIds
                // fails with EMPTY_PENDING_AIRDROP_ID_LIST
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenCancelAirdropTransaction().Execute(testEnv.Client).GetRecord(testEnv.Client);
                }); Assert.Contains(ResponseStatus.EMPTY_PENDING_AIRDROP_ID_LIST.ToString(), exception.Message);
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
                var record = new TokenAirdropTransaction().AddTokenTransfer(tokenID, receiverAccountId, amount).AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // cancel the tokens with duplicate pending airdrop token ids
                // fails with PENDING_AIRDROP_ID_REPEATED
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenCancelAirdropTransaction().AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).AddPendingAirdrop(record.pendingAirdropRecords[0].GetPendingAirdropId()).Execute(testEnv.Client).GetRecord(testEnv.Client);
                }); Assert.Contains(ResponseStatus.PENDING_AIRDROP_ID_REPEATED.ToString(), exception.Message);
            }
        }
    }
}