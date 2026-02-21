// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Airdrops;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenAirdropClaimIntegrationTest
    {
        private readonly int amount = 100;
        public virtual void CanClaimTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = nftID,
					Metadata = NftMetadataGenerator.Generate((byte)10),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var nftSerials = mintReceipt.Serials;

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction()
                    .AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify the txn record
                Assert.Equal(3, record.PendingAirdropRecords.Count);
                Assert.Equal((ulong)100, record.PendingAirdropRecords[0].PendingAirdropAmount);
                Assert.Equal(tokenID, record.PendingAirdropRecords[0].PendingAirdropId.TokenId);
                Assert.Null(record.PendingAirdropRecords[0].PendingAirdropId.NftId);
                Assert.Equal((ulong)0, record.PendingAirdropRecords[1].PendingAirdropAmount);
                Assert.Equal(nftID.Nft(1), record.PendingAirdropRecords[1].PendingAirdropId.NftId);
                Assert.Null(record.PendingAirdropRecords[1].PendingAirdropId.TokenId);
                Assert.Equal((ulong)0, record.PendingAirdropRecords[2].PendingAirdropAmount);
                Assert.Equal(nftID.Nft(2), record.PendingAirdropRecords[2].PendingAirdropId.NftId);
                Assert.Null(record.PendingAirdropRecords[2].PendingAirdropId.TokenId);

                // claim the tokens with the receiver
                record = new TokenClaimAirdropTransaction
				{
					PendingAirdropIds = 
                    [
                        record.PendingAirdropRecords[0].PendingAirdropId,
					    record.PendingAirdropRecords[1].PendingAirdropId,
					    record.PendingAirdropRecords[2].PendingAirdropId,
                    ]
				}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.PendingAirdropRecords.Count);

                // verify the receiver holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery { AccountId = receiverAccountId, }.Execute(testEnv.Client);

                Assert.Equal((ulong)amount, receiverAccountBalance.Tokens[tokenID]);
                Assert.Equal((ulong)2, receiverAccountBalance.Tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(fungibleInitialBalance - amount, operatorBalance.Tokens[tokenID]);
                Assert.Equal(mitedNfts - 2, operatorBalance.Tokens[nftID]);
            }
        }

        public virtual void CanClaimTokensToMultipleReceivers()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = nftID,
					Metadata = NftMetadataGenerator.Generate((byte)10),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var nftSerials = mintReceipt.Serials;

                // create receiver1 with 0 auto associations
                var receiver1AccountKey = PrivateKey.GenerateED25519();
                var receiver1AccountId = EntityHelper.CreateAccount(testEnv, receiver1AccountKey, 0);

                // create receiver2 with 0 auto associations
                var receiver2AccountKey = PrivateKey.GenerateED25519();
                var receiver2AccountId = EntityHelper.CreateAccount(testEnv, receiver2AccountKey, 0);

                // airdrop the tokens to both
                var record = new TokenAirdropTransaction()
                    .AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiver1AccountId)
                    .AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiver1AccountId)
                    .AddTokenTransfer(tokenID, receiver1AccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount)
                    .AddNftTransfer(nftID.Nft(nftSerials[2]), testEnv.OperatorId, receiver2AccountId)
                    .AddNftTransfer(nftID.Nft(nftSerials[3]), testEnv.OperatorId, receiver2AccountId)
                    .AddTokenTransfer(tokenID, receiver2AccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify the txn record
                Assert.Equal(6, record.PendingAirdropRecords.Count);

                // claim the tokens signing with receiver1 and receiver2
                var pendingAirdropIDs = [.. record.PendingAirdropRecords.Select(_ => _.PendingAirdropId)];

                record = new TokenClaimAirdropTransaction
                {
					PendingAirdropIds = pendingAirdropIDs,
				
                }.FreezeWith(testEnv.Client).Sign(receiver1AccountKey).Sign(receiver2AccountKey).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.PendingAirdropRecords.Count);

                // verify receiver1 holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery
                {
					AccountId = receiver1AccountId,
				
                }.Execute(testEnv.Client);

                Assert.Equal((ulong)amount, receiverAccountBalance.Tokens[tokenID]);
                Assert.Equal((ulong)2, receiverAccountBalance.Tokens[nftID]);

                // verify receiver2 holds the tokens via query
                var receiver2AccountBalance = new AccountBalanceQuery
                {
					AccountId = receiver1AccountId,
				
                }.Execute(testEnv.Client);
                
                Assert.Equal((ulong)amount, receiver2AccountBalance.Tokens[tokenID]);
                Assert.Equal((ulong)2, receiver2AccountBalance.Tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(fungibleInitialBalance - amount * 2, operatorBalance.Tokens[tokenID]);
                Assert.Equal(mitedNfts - 4, operatorBalance.Tokens[nftID]);
            }
        }

        public virtual void CanClaimTokensFromMultipleAirdropTxns()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible and nf token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = nftID,
					Metadata = NftMetadataGenerator.Generate((byte)10),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var nftSerials = mintReceipt.Serials;

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop some of the tokens to the receiver
                var record1 = new TokenAirdropTransaction()
                    .AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // airdrop some of the tokens to the receiver
                var record2 = new TokenAirdropTransaction()
                    .AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // airdrop some of the tokens to the receiver
                var record3 = new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // get the PendingIds from the records
                var pendingAirdropIDs = new List<PendingAirdropId>
                {
                    record1.PendingAirdropRecords[0].PendingAirdropId,
                    record2.PendingAirdropRecords[0].PendingAirdropId,
                    record3.PendingAirdropRecords[0].PendingAirdropId,
                };

                // claim the all the tokens with the receiver
                var record = new TokenClaimAirdropTransaction
                {
					PendingAirdropIds = pendingAirdropIDs,

				}.FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // verify in the transaction record the pending airdrop ids for nft and ft - should no longer exist
                Assert.Equal(0, record.PendingAirdropRecords.Count);

                // verify the receiver holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery { AccountId = receiverAccountId, }.Execute(testEnv.Client);
                
                Assert.Equal((ulong)amount, receiverAccountBalance.Tokens[tokenID]);
                Assert.Equal((ulong)2, receiverAccountBalance.Tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery { AccountId = testEnv.OperatorId, }.Execute(testEnv.Client);

                Assert.Equal(fungibleInitialBalance - amount, operatorBalance.Tokens[tokenID]);
                Assert.Equal(mitedNfts - 2, operatorBalance.Tokens[nftID]);
            }
        }

        public virtual void CannotClaimTokensForNonExistingAirdrop()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount)
                    .Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);

                // claim the tokens with the operator which does not have pending airdrops
                // fails with INVALID_SIGNATURE
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenClaimAirdropTransaction()
					{
						PendingAirdropIds = [record.PendingAirdropRecords[0].PendingAirdropId]
					}
					.Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }

        public virtual void CannotClaimTokensForAlreadyClaimedAirdrop()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // claim the tokens with the receiver
                new TokenClaimAirdropTransaction()
				{
					PendingAirdropIds = [record.PendingAirdropRecords[0].PendingAirdropId]
				}
					.FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // claim the tokens with the receiver again
                // fails with INVALID_PENDING_AIRDROP_ID
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenClaimAirdropTransaction()

                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.InvalidPendingAirdropId.ToString(), exception.Message);
            }
        }

        public virtual void CannotClaimWithEmptyPendingAirdropsList()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // claim the tokens with the receiver without setting pendingAirdropIds
                // fails with EMPTY_PENDING_AIRDROP_ID_LIST
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenClaimAirdropTransaction()
                    .Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.EmptyPendingAirdropIdList.ToString(), exception.Message);
            }
        }

        public virtual void CannotClaimTokensWithDuplicateEntries()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // claim the tokens with duplicate pending airdrop token ids
                // fails with PENDING_AIRDROP_ID_REPEATED
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenClaimAirdropTransaction()
					{
						PendingAirdropIds = [ record.PendingAirdropRecords[0].PendingAirdropId, record.PendingAirdropRecords[0].PendingAirdropId]
					}
					.Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.PendingAirdropIdRepeated.ToString(), exception.Message);
            }
        }

        public virtual void CannotClaimTokensWhenTokenIsPaused()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // pause the token
                new TokenPauseTransaction { TokenId = tokenID }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // claim the tokens with receiver
                // fails with TOKEN_IS_PAUSED
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenClaimAirdropTransaction()
					{
						PendingAirdropIds = [record.PendingAirdropRecords[0].PendingAirdropId]
					}
					.FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.TokenIsPaused.ToString(), exception.Message);
            }
        }

        public virtual void CannotClaimTokensWhenTokenIsDeleted()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetRecord(testEnv.Client);

                // delete the token
                new TokenDeleteTransaction { TokenId = tokenID }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // claim the tokens with receiver
                // fails with TOKEN_IS_DELETED
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenClaimAirdropTransaction()
					{
						PendingAirdropIds = [record.PendingAirdropRecords[0].PendingAirdropId]
					}
					.FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.TokenWasDeleted.ToString(), exception.Message);
            }
        }

        public virtual void CannotClaimTokensWhenTokenIsFrozen()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // create receiver with 0 auto associations
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var record = new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount)
                    .Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);

                // associate
                new TokenAssociateTransaction
                {
					AccountId = receiverAccountId,
					TokenIds = [tokenID],

				}.FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // freeze the token
                new TokenFreezeTransaction
                {
					AccountId = receiverAccountId,
					TokenId = tokenID,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // claim the tokens with receiver
                // fails with ACCOUNT_FROZEN_FOR_TOKEN
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenClaimAirdropTransaction
                    {
                        PendingAirdropIds = [record.PendingAirdropRecords[0].PendingAirdropId]
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetRecord(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.AccountFrozenForToken.ToString(), exception.Message);
            }
        }
    }
}