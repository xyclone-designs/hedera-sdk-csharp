// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TokenAutomaticAssociationIntegrationTest
    {
        public virtual void CanTransferFungibleTokensToAccountsWithLimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, 0);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, 0);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 1;
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                var accountInfoBeforeTokenAssociation = new AccountInfoQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                Assert.Equal(accountInfoBeforeTokenAssociation.MaxAutomaticTokenAssociations, 1);
                Assert.Equal(accountInfoBeforeTokenAssociation.TokenRelationships.Count, 0);
                var transferRecord = new TransferTransaction()
                    .AddTokenTransfer(tokenId1, testEnv.OperatorId, -1)
                    .AddTokenTransfer(tokenId1, receiverAccountId, 1).Execute(testEnv.Client).GetRecord(testEnv.Client);
                Assert.Equal(transferRecord.AutomaticTokenAssociations.Count, 1);
                Assert.Equal(transferRecord.AutomaticTokenAssociations[0].AccountId, receiverAccountId);
                Assert.Equal(transferRecord.AutomaticTokenAssociations[0].TokenId, tokenId1);
                var accountInfoAfterTokenAssociation = new AccountInfoQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                Assert.Equal(accountInfoAfterTokenAssociation.TokenRelationships.Count, 1);
                Assert.True(accountInfoAfterTokenAssociation.TokenRelationships[tokenId1].AutomaticAssociation);
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TransferTransaction()
                    .AddTokenTransfer(tokenId2, testEnv.OperatorId, -1)
                    .AddTokenTransfer(tokenId2, receiverAccountId, 1).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains("NO_REMAINING_AUTOMATIC_ASSOCIATIONS", exception.Message);
                new AccountUpdateTransaction
                {
					AccountId = receiverAccountId,
					MaxAutomaticTokenAssociations = 2
				}
                .FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                var accountInfoAfterMaxAssocUpdate = new AccountInfoQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                
                Assert.Equal(accountInfoAfterMaxAssocUpdate.MaxAutomaticTokenAssociations, 2);

                new TokenDeleteTransaction { TokenId = tokenId1 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenDeleteTransaction { TokenId = tokenId2 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanTransferNftsToAccountsWithLimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateNft(testEnv);
                var tokenId2 = EntityHelper.CreateNft(testEnv);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 1;
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                var mintReceiptToken1 = new TokenMintTransaction { TokenId = tokenId1, Metadata = NftMetadataGenerator.Generate((byte)10) }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var mintReceiptToken2 = new TokenMintTransaction { TokenId = tokenId2, Metadata = NftMetadataGenerator.Generate((byte)10) }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountInfoBeforeTokenAssociation = new AccountInfoQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                
                Assert.Equal(accountInfoBeforeTokenAssociation.MaxAutomaticTokenAssociations, 1);
                Assert.Equal(accountInfoBeforeTokenAssociation.TokenRelationships.Count, 0);
                
                var serialsToTransfer = [.. mintReceiptToken2.Serials];
                var nftTransferTransaction = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
                {
                    nftTransferTransaction.AddNftTransfer(tokenId1.Nft(serial), testEnv.OperatorId, receiverAccountId);
                }

                var transferRecord = nftTransferTransaction.Execute(testEnv.Client).GetRecord(testEnv.Client);
                
                Assert.Equal(transferRecord.AutomaticTokenAssociations.Count, 1);
                Assert.Equal(transferRecord.AutomaticTokenAssociations[0].AccountId, receiverAccountId);
                Assert.Equal(transferRecord.AutomaticTokenAssociations[0].TokenId, tokenId1);
                
                var accountInfoAfterTokenAssociation = new AccountInfoQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                
                Assert.Equal(accountInfoAfterTokenAssociation.TokenRelationships.Count, 1);
                Assert.True(accountInfoAfterTokenAssociation.TokenRelationships[tokenId1].AutomaticAssociation);
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    var serial = mintReceiptToken2.Serials[0];
                    new TransferTransaction().AddNftTransfer(tokenId2.Nft(serial), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains("NO_REMAINING_AUTOMATIC_ASSOCIATIONS", exception.Message);
                
                new AccountUpdateTransaction
                {
					AccountId = receiverAccountId,
					MaxAutomaticTokenAssociations = 2,
				}
                .FreezeWith(testEnv.Client)
                .Sign(accountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                var accountInfoAfterMaxAssocUpdate = new AccountInfoQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                
                Assert.Equal(accountInfoAfterMaxAssocUpdate.MaxAutomaticTokenAssociations, 2);
                
                new TokenDeleteTransaction { TokenId = tokenId1 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenDeleteTransaction { TokenId = tokenId2 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanSetUnlimitedMaxAutoAssociationsForAccount()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = -1;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                new AccountUpdateTransaction
                {
					AccountId = accountId,
					MaxAutomaticTokenAssociations = accountMaxAutomaticTokenAssociations

				}.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                var accountInfoBeforeTokenAssociation = new AccountInfoQuery
                {
					AccountId = accountId,

				}.Execute(testEnv.Client);
                
                Assert.Equal(accountInfoBeforeTokenAssociation.MaxAutomaticTokenAssociations, -1);
            }
        }

        public virtual void CanTransferFungibleTokensToAccountsWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var accountKey = PrivateKey.GenerateED25519();
                var accountId1 = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                var accountId2 = EntityHelper.CreateAccount(testEnv, accountKey, 100);
                new AccountUpdateTransaction
                {
					AccountId = accountId2,
					MaxAutomaticTokenAssociations = -1,
				}
                .FreezeWith(testEnv.Client)
                .Sign(accountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // transfer to both receivers some token1 tokens
                new TransferTransaction()
                    .AddTokenTransfer(tokenId1, testEnv.OperatorId, -1000)
                    .AddTokenTransfer(tokenId1, accountId1, 1000)
                    .AddTokenTransfer(tokenId1, testEnv.OperatorId, -1000)
                    .AddTokenTransfer(tokenId1, accountId2, 1000).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // transfer to both receivers some token2 tokens
                new TransferTransaction()
                    .AddTokenTransfer(tokenId2, testEnv.OperatorId, -1000)
                    .AddTokenTransfer(tokenId2, accountId1, 1000)
                    .AddTokenTransfer(tokenId2, testEnv.OperatorId, -1000)
                    .AddTokenTransfer(tokenId2, accountId2, 1000).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the balance of the receivers is 1000
                var accountId1Balance = new AccountBalanceQuery { AccountId = accountId1 }.Execute(testEnv.Client);
                
                Assert.Equal<ulong>(accountId1Balance.Tokens[tokenId1], 1000);
                Assert.Equal<ulong>(accountId1Balance.Tokens[tokenId2], 1000);
                
                var accountId2Balance = new AccountBalanceQuery { AccountId = accountId2 }.Execute(testEnv.Client);
                
                Assert.Equal<ulong>(accountId2Balance.Tokens[tokenId1], 1000);
                Assert.Equal<ulong>(accountId2Balance.Tokens[tokenId2], 1000);

                new TokenDeleteTransaction { TokenId = tokenId1 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenDeleteTransaction { TokenId = tokenId2 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanTransferFungibleTokensWithDecimalsToAccountsWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenDecimals = 10;
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, tokenDecimals);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, tokenDecimals);
                var accountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                new TransferTransaction()
                    .AddTokenTransferWithDecimals(tokenId1, testEnv.OperatorId, -1000, tokenDecimals)
                    .AddTokenTransferWithDecimals(tokenId1, receiverAccountId, 1000, tokenDecimals)
                    .AddTokenTransferWithDecimals(tokenId2, testEnv.OperatorId, -1000, tokenDecimals)
                    .AddTokenTransferWithDecimals(tokenId2, receiverAccountId, 1000, tokenDecimals).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var receiverAccountBalance = new AccountBalanceQuery
                {
					AccountId = receiverAccountId
				
                }.Execute(testEnv.Client);
                
                Assert.Equal<ulong>(receiverAccountBalance.Tokens[tokenId1], 1000);
                Assert.Equal<ulong>(receiverAccountBalance.Tokens[tokenId2], 1000);

                new TokenDeleteTransaction { TokenId = tokenId1 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenDeleteTransaction { TokenId = tokenId2 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanTransferFungibleTokensOnBehalfOfOwnerToAccountWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var tokenId2 = EntityHelper.CreateFungibleToken(testEnv, 3);
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                var spenderAccountKey = PrivateKey.GenerateED25519();
                var spenderAccountId = EntityHelper.CreateAccount(testEnv, spenderAccountKey, -1);
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenAllowance(tokenId1, testEnv.OperatorId, spenderAccountId, 2000)
                    .ApproveTokenAllowance(tokenId2, testEnv.OperatorId, spenderAccountId, 2000)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                var record = new TransferTransaction
                {
					TransactionId = TransactionId.Generate(spenderAccountId)
				}
                .AddApprovedTokenTransfer(tokenId1, testEnv.OperatorId, -1000)
                .AddTokenTransfer(tokenId1, accountId, 1000)
                .AddApprovedTokenTransfer(tokenId2, testEnv.OperatorId, -1000)
                .AddTokenTransfer(tokenId2, accountId, 1000)
                .FreezeWith(testEnv.Client).Sign(spenderAccountKey)
                .Execute(testEnv.Client)
                .GetRecord(testEnv.Client);

                var accountBalance = new AccountBalanceQuery { AccountId = accountId, }.Execute(testEnv.Client);
                
                Assert.Equal<ulong>(accountBalance.Tokens[tokenId1], 1000);
                Assert.Equal<ulong>(accountBalance.Tokens[tokenId2], 1000);

                new TokenDeleteTransaction { TokenId = tokenId1 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenDeleteTransaction { TokenId = tokenId2 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanTransferNftsToAccountsWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateNft(testEnv);
                var tokenId2 = EntityHelper.CreateNft(testEnv);
                var accountKey = PrivateKey.GenerateED25519();
                var accountId1 = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                var accountId2 = EntityHelper.CreateAccount(testEnv, accountKey, 100);
                var mintReceiptToken1 = new TokenMintTransaction { TokenId = tokenId1, Metadata = NftMetadataGenerator.Generate((byte)10) }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var mintReceiptToken2 = new TokenMintTransaction { TokenId = tokenId2, Metadata = NftMetadataGenerator.Generate((byte)10) }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceiptToken2.Serials;
                new AccountUpdateTransaction
                {
					AccountId = accountId2,
					MaxAutomaticTokenAssociations = -1

				}.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // transfer nft1 to both receivers, 2 for each
                new TransferTransaction().AddNftTransfer(tokenId1.Nft(nftSerials[0]), testEnv.OperatorId, accountId1).AddNftTransfer(tokenId1.Nft(nftSerials[1]), testEnv.OperatorId, accountId1).AddNftTransfer(tokenId1.Nft(nftSerials[2]), testEnv.OperatorId, accountId2).AddNftTransfer(tokenId1.Nft(nftSerials[3]), testEnv.OperatorId, accountId2).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // transfer nft2 to both receivers, 2 for each
                new TransferTransaction().AddNftTransfer(tokenId2.Nft(nftSerials[0]), testEnv.OperatorId, accountId1).AddNftTransfer(tokenId2.Nft(nftSerials[1]), testEnv.OperatorId, accountId1).AddNftTransfer(tokenId2.Nft(nftSerials[2]), testEnv.OperatorId, accountId2).AddNftTransfer(tokenId2.Nft(nftSerials[3]), testEnv.OperatorId, accountId2).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the balance of the receivers is 2
                var accountId1Balance = new AccountBalanceQuery { AccountId = accountId1 }.Execute(testEnv.Client);
                
                Assert.Equal<ulong>(accountId1Balance.Tokens[tokenId1], 2);
                Assert.Equal<ulong>(accountId1Balance.Tokens[tokenId2], 2);
                
                var accountId2Balance = new AccountBalanceQuery { AccountId = accountId2 }.Execute(testEnv.Client);
                
                Assert.Equal<ulong>(accountId2Balance.Tokens[tokenId1], 2);
                Assert.Equal<ulong>(accountId2Balance.Tokens[tokenId2], 2);

                new TokenDeleteTransaction { TokenId = tokenId1 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenDeleteTransaction { TokenId = tokenId2 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanTransferNftsOnBehalfOfOwnerToAccountWithUnlimitedMaxAutoAssociations()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId1 = EntityHelper.CreateNft(testEnv);
                var tokenId2 = EntityHelper.CreateNft(testEnv);
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, -1);
                var spenderAccountKey = PrivateKey.GenerateED25519();
                var spenderAccountId = EntityHelper.CreateAccount(testEnv, spenderAccountKey, -1);
                var mintReceiptToken1 = new TokenMintTransaction { TokenId = tokenId1 }.SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var mintReceiptToken2 = new TokenMintTransaction { TokenId = tokenId2 }.SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceiptToken2.Serials;
                
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenNftAllowanceAllSerials(tokenId1, testEnv.OperatorId, spenderAccountId)
                    .ApproveTokenNftAllowanceAllSerials(tokenId2, testEnv.OperatorId, spenderAccountId)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                new TransferTransaction()
                    .AddApprovedNftTransfer(tokenId1.Nft(nftSerials[0]), testEnv.OperatorId, accountId)
                    .AddApprovedNftTransfer(tokenId1.Nft(nftSerials[1]), testEnv.OperatorId, accountId)
                    .AddApprovedNftTransfer(tokenId2.Nft(nftSerials[0]), testEnv.OperatorId, accountId)
                    .AddApprovedNftTransfer(tokenId2.Nft(nftSerials[1]), testEnv.OperatorId, accountId)
                    .SetTransactionId(TransactionId.Generate(spenderAccountId))
                    .FreezeWith(testEnv.Client)
                    .Sign(spenderAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                var accountBalance = new AccountBalanceQuery()AccountId = accountId,.Execute(testEnv.Client);
                
                Assert.Equal<ulong>(accountBalance.Tokens[tokenId1], 2);
                Assert.Equal<ulong>(accountBalance.Tokens[tokenId2], 2);

                new TokenDeleteTransaction { TokenId = tokenId1 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenDeleteTransaction { TokenId = tokenId2 }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotSetInvalidMaxAutoAssociationsValues()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new AccountCreateTransaction
                    {
						Key = accountKey,
						MaxAutomaticTokenAssociations = -2
					
                    }.Execute(testEnv.Client);
                }); 
                
                Assert.Contains("INVALID_MAX_AUTO_ASSOCIATIONS", exception.Message);
                
                Exception exception2 = Assert.Throws<Exception>(() =>
                {
                    new AccountCreateTransaction
                    {
						Key = accountKey,
						MaxAutomaticTokenAssociations = -1000
					
                    }.Execute(testEnv.Client);
                }); 
                
                Assert.Contains("INVALID_MAX_AUTO_ASSOCIATIONS", exception2.Message);
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, 100);
                
                Exception exception3 = Assert.Throws<Exception>(() =>
                {
                    new AccountUpdateTransaction
                    {
						AccountId = accountId,
						MaxAutomaticTokenAssociations = -2
					
                    }.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains("INVALID_MAX_AUTO_ASSOCIATIONS", exception3.Message);
                
                Exception exception4 = Assert.Throws<Exception>(() =>
                {
                    new AccountUpdateTransaction
                    {
						AccountId = accountId,
						MaxAutomaticTokenAssociations = -1000
					
                    }.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains("INVALID_MAX_AUTO_ASSOCIATIONS", exception4.Message);
            }
        }
    }
}