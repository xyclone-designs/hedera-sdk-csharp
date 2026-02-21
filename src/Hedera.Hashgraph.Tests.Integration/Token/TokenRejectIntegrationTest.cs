// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.HBar;

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
                new TransferTransaction()
                    .AddTokenTransfer(tokenId1, testEnv.OperatorId, -10)
                    .AddTokenTransfer(tokenId1, receiverAccountId, 10)
                    .AddTokenTransfer(tokenId2, testEnv.OperatorId, -10)                    
                    .AddTokenTransfer(tokenId2, receiverAccountId, 10)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token
                new TokenRejectTransaction
                {
					OwnerId = receiverAccountId,
					TokenIds = [tokenId1, tokenId2],
				}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // verify the balance of the receiver is 0
                var receiverAccountBalance = new AccountBalanceQuery
                { 
                    AccountId = receiverAccountId 

                }.Execute(testEnv.Client);

                Assert.Equal<ulong>(0, receiverAccountBalance.Tokens[tokenId1]);
                Assert.Equal<ulong>(0, receiverAccountBalance.Tokens[tokenId2]);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery
                { 
                    AccountId = testEnv.OperatorId 

                }.Execute(testEnv.Client);

                Assert.Equal<ulong>(1000000, treasuryAccountBalance.Tokens[tokenId1]);
                Assert.Equal<ulong>(1000000, treasuryAccountBalance.Tokens[tokenId2]);
                
                new TokenDeleteTransaction
                {
                    TokenId = tokenId1
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                new TokenDeleteTransaction
                {
                    TokenId = tokenId2
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
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
                var mintReceiptToken1 = new TokenMintTransaction
                {
                    TokenId = tokenId1,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var mintReceiptToken2 = new TokenMintTransaction
                {
                    TokenId = tokenId2,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftSerials = mintReceiptToken2.Serials;

                // transfer nfts to the receiver
                new TransferTransaction()
                    .AddNftTransfer(tokenId1.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(tokenId1.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(tokenId2.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(tokenId2.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject one of the nfts
                new TokenRejectTransaction()
                { 
                    OwnerId = receiverAccountId,
                    NftIds = [tokenId1.Nft(nftSerials[1]), tokenId2.Nft(nftSerials[1])]
                }
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // verify the balance is decremented by 1
                var receiverAccountBalance = new AccountBalanceQuery
                {
					AccountId = receiverAccountId
				
                }.Execute(testEnv.Client);
                Assert.Equal<ulong>(1, receiverAccountBalance.Tokens[tokenId1]);
                Assert.Equal<ulong>(1, receiverAccountBalance.Tokens[tokenId2]);

                // verify the token is transferred back to the treasury
                var tokenId1NftInfo = new TokenNftInfoQuery
                {
                    NftId = tokenId1.Nft(nftSerials[1])
                }
                .Execute(testEnv.Client);

                Assert.Equal(tokenId1NftInfo[0].AccountId, testEnv.OperatorId);

                var tokenId2NftInfo = new TokenNftInfoQuery
                {
					NftId = tokenId2.Nft(nftSerials[1])
				}
                .Execute(testEnv.Client);

                Assert.Equal(tokenId2NftInfo[0].AccountId, testEnv.OperatorId);
                
                new TokenDeleteTransaction
                {
                    TokenId = tokenId1
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                new TokenDeleteTransaction
                {
                    TokenId = tokenId2
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
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
                var mintReceiptNftToken1 = new TokenMintTransaction
                {
                    TokenId = nftTokenId1,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var mintReceiptNftToken2 = new TokenMintTransaction
                {
                    TokenId = nftTokenId2,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftSerials = mintReceiptNftToken2.Serials;

                // transfer fts to the receiver
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId1, testEnv.OperatorId, -10)
                    .AddTokenTransfer(ftTokenId1, receiverAccountId, 10)
                    .AddTokenTransfer(ftTokenId2, testEnv.OperatorId, -10)                    
                    .AddTokenTransfer(ftTokenId2, receiverAccountId, 10)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // transfer nfts to the receiver
                new TransferTransaction()
                    .AddNftTransfer(nftTokenId1.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId1.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId2.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId2.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token
                new TokenRejectTransaction
				{
                    OwnerId = receiverAccountId,
                    TokenIds = [ftTokenId1, ftTokenId2],
                    NftIds = [nftTokenId1.Nft(nftSerials[1]), nftTokenId2.Nft(nftSerials[1])]
				}
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // verify the balance of the receiver is 0
                var receiverAccountBalance = new AccountBalanceQuery
                {
					AccountId = receiverAccountId

				}.Execute(testEnv.Client);
                Assert.Equal<ulong>(0, receiverAccountBalance.Tokens[ftTokenId1]);
                Assert.Equal<ulong>(0, receiverAccountBalance.Tokens[ftTokenId2]);
                Assert.Equal<ulong>(1, receiverAccountBalance.Tokens[nftTokenId1]);
                Assert.Equal<ulong>(1, receiverAccountBalance.Tokens[nftTokenId2]);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId

				}.Execute(testEnv.Client);
                Assert.Equal<ulong>(1000000, treasuryAccountBalance.Tokens[ftTokenId1]);
                Assert.Equal<ulong>(1000000, treasuryAccountBalance.Tokens[ftTokenId2]);
                var tokenId1NftInfo = new TokenNftInfoQuery
                {
                    NftId = nftTokenId1.Nft(nftSerials[1])

                }.Execute(testEnv.Client);
                Assert.Equal(tokenId1NftInfo[0].AccountId, testEnv.OperatorId);
                var tokenId2NftInfo = new TokenNftInfoQuery
                {
					NftId = nftTokenId2.Nft(nftSerials[1])

				}.Execute(testEnv.Client);
                Assert.Equal(tokenId2NftInfo[0].AccountId, testEnv.OperatorId);
                
                new TokenDeleteTransaction
                {
                    TokenId = ftTokenId1
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                new TokenDeleteTransaction
                {
                    TokenId = ftTokenId2
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                new TokenDeleteTransaction
                {
                    TokenId = nftTokenId1
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                new TokenDeleteTransaction
                {
                    TokenId = nftTokenId2
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }
        public virtual void CanExecuteTokenRejectTransactionForFtAndNftWhenTreasuryReceiverSigRequiredIsEnabled()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);
                var treasuryAccountKey = PrivateKey.GenerateED25519();
                var treasuryAccountId = new AccountCreateTransaction
                {
                    Key = treasuryAccountKey,
                    InitialBalance = new Hbar(0),
                    ReceiverSigRequired = true,
                    MaxAutomaticTokenAssociations = 100,
                }
                .FreezeWith(testEnv.Client)
                .Sign(treasuryAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;
                var ftTokenId = new TokenCreateTransaction
                {
                    TokenName = "Test Fungible Token",
                    TokenSymbol = "TFT",
                    TokenMemo = "I was created for integration tests",
                    Decimals = 18,
                    InitialSupply = 1000000,
                    MaxSupply = 1000000,
                    TreasuryAccountId = treasuryAccountId,
                    TokenSupplyType = TokenSupplyType.Finite,
                    AdminKey = testEnv.OperatorKey,
                    FreezeKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                    MetadataKey = testEnv.OperatorKey,
                }
                .FreezeWith(testEnv.Client)
                .Sign(treasuryAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                // transfer fts to the receiver
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId, treasuryAccountId, -10)
                    .AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                .FreezeWith(testEnv.Client)
                .Sign(treasuryAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token
                new TokenRejectTransaction
                {
					OwnerId = receiverAccountId
				}
                .AddTokenId(ftTokenId)
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // verify the balance of the receiver is 0
                var receiverAccountBalanceFt = new AccountBalanceQuery
                {
                    AccountId = receiverAccountId

                }.Execute(testEnv.Client);

                Assert.Equal<ulong>(0, receiverAccountBalanceFt.Tokens[ftTokenId]);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery
                {
                    AccountId = treasuryAccountId

                }.Execute(testEnv.Client);

                Assert.Equal<ulong>(1000000, treasuryAccountBalance.Tokens[ftTokenId]);

                // same test for nft
                var nftTokenId = new TokenCreateTransaction
                {
                    TokenName = "Test NFT",
                    TokenSymbol = "TNFT",
                    TokenType = TokenType.NonFungibleUnique,
                    TreasuryAccountId = treasuryAccountId,
                    TokenSupplyType = TokenSupplyType.Finite,
                    MaxSupply = 10, 
                    AdminKey = testEnv.OperatorKey,
                    FreezeKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                    MetadataKey = testEnv.OperatorKey, 
                    WipeKey = testEnv.OperatorKey
                }
                .FreezeWith(testEnv.Client)
                .Sign(treasuryAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                var mintReceiptNftToken = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var nftSerials = mintReceiptNftToken.Serials;

                // transfer nfts to the receiver
                new TransferTransaction()
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[0]), treasuryAccountId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[1]), treasuryAccountId, receiverAccountId)
                .FreezeWith(testEnv.Client)
                .Sign(treasuryAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token
                new TokenRejectTransaction
                {
					OwnerId = receiverAccountId
				}
                .AddNftId(nftTokenId.Nft(nftSerials[1]))
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // verify the balance is decremented by 1
                var receiverAccountBalanceNft = new AccountBalanceQuery
                {
					AccountId = receiverAccountId

				}.Execute(testEnv.Client);

                Assert.Equal<ulong>(1, receiverAccountBalanceNft.Tokens[nftTokenId]);

                // verify the token is transferred back to the treasury
                var nftTokenIdInfo = new TokenNftInfoQuery
                {
					NftId = nftTokenId.Nft(nftSerials[1])
				}
                .Execute(testEnv.Client);

                Assert.Equal(nftTokenIdInfo[0].AccountId, treasuryAccountId);
                
                new TokenDeleteTransaction
                {
                    TokenId = ftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenDeleteTransaction
                {
                    TokenId = nftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
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
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10)
                    .AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // freeze ft
                new TokenFreezeTransaction
                {
					TokenId = ftTokenId,
					AccountId = receiverAccountId,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // reject the token - should fail with ACCOUNT_FROZEN_FOR_TOKEN
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId,
						TokenIds = [ftTokenId],
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); Assert.Contains("ACCOUNT_FROZEN_FOR_TOKEN", exception.Message);

                // same test for nft
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // transfer nfts to the receiver
                new TransferTransaction()
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // freeze nft
                new TokenFreezeTransaction
                {
					TokenId = nftTokenId,
					AccountId = receiverAccountId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // reject the token - should fail with ACCOUNT_FROZEN_FOR_TOKEN
                Exception exception2 = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId
					}
                    .AddNftId(nftTokenId.Nft(nftSerials[1]))
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("ACCOUNT_FROZEN_FOR_TOKEN", exception2.Message);
                
                new TokenDeleteTransaction
                {
                    TokenId = ftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenDeleteTransaction
                {
                    TokenId = nftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
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
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10)    
                    .AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // pause ft
                new TokenPauseTransaction
                {
                    TokenId = ftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token - should fail with TOKEN_IS_PAUSED
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId,
						TokenIds = [ftTokenId],
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); Assert.Contains("TOKEN_IS_PAUSED", exception.Message);

                // same test for nft
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // transfer nfts to the receiver
                new TransferTransaction()
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // pause nft
                new TokenPauseTransaction
                {
                    TokenId = nftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token - should fail with TOKEN_IS_PAUSED
                Exception exception2 = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId
					}
                    .AddNftId(nftTokenId.Nft(nftSerials[1]))
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("TOKEN_IS_PAUSED", exception2.Message);
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
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10)    
                    .AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenAllowance(ftTokenId, receiverAccountId, spenderAccountId, 10)
                    .FreezeWith(testEnv.Client).Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // verify the spender has allowance
                new TransferTransaction
                {
					TransactionId = TransactionId.Generate(spenderAccountId)
				}
                    .AddApprovedTokenTransfer(ftTokenId, receiverAccountId, -5)
                    .AddTokenTransfer(ftTokenId, spenderAccountId, 5)
                    .FreezeWith(testEnv.Client)
                    .Sign(spenderAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // reject the token
                new TokenRejectTransaction
                {
					OwnerId = receiverAccountId,
                    TokenIds = [ftTokenId]
				}
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // verify the allowance - should be 0 , because the receiver is no longer the owner
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TransferTransaction
                    {
						TransactionId = TransactionId.Generate(spenderAccountId)
					}
                    .AddApprovedTokenTransfer(ftTokenId, receiverAccountId, -5)
                    .AddTokenTransfer(ftTokenId, spenderAccountId, 5)
                    .FreezeWith(testEnv.Client)
                    .Sign(spenderAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); Assert.Contains("SPENDER_DOES_NOT_HAVE_ALLOWANCE", exception.Message);

                // same test for nft
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var nftSerials = mintReceipt.Serials;

                // transfer nfts to the receiver
                new TransferTransaction()
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[2]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[3]), testEnv.OperatorId, receiverAccountId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenNftAllowance(nftTokenId.Nft(nftSerials[0]), receiverAccountId, spenderAccountId)
                    .ApproveTokenNftAllowance(nftTokenId.Nft(nftSerials[1]), receiverAccountId, spenderAccountId)
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // verify the spender has allowance
                new TransferTransaction
                {
					TransactionId = TransactionId.Generate(spenderAccountId)
				}
                    .AddApprovedNftTransfer(nftTokenId.Nft(nftSerials[0]), receiverAccountId, spenderAccountId)
                    .FreezeWith(testEnv.Client)
                    .Sign(spenderAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // reject the token
                new TokenRejectTransaction
                {
					OwnerId = receiverAccountId,
					NftIds = [nftTokenId.Nft(nftSerials[1])],
				}
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // verify the allowance - should be 0 , because the receiver is no longer the owner
                Exception exception2 = Assert.Throws<Exception>(() =>
                {
                    new TransferTransaction
                    {
						TransactionId = TransactionId.Generate(spenderAccountId)
					}
                    .AddApprovedNftTransfer(nftTokenId.Nft(nftSerials[1]), receiverAccountId, spenderAccountId)
                    .AddApprovedNftTransfer(nftTokenId.Nft(nftSerials[2]), receiverAccountId, spenderAccountId)
                    .FreezeWith(testEnv.Client)
                    .Sign(spenderAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("SPENDER_DOES_NOT_HAVE_ALLOWANCE", exception2.Message);
                
                new TokenDeleteTransaction
                {
                    TokenId = ftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenDeleteTransaction
                {
                    TokenId = nftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotRejectNftWhenUsingAddOrSetTokenId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 100);
                var mintReceiptNftToken = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var nftSerials = mintReceiptNftToken.Serials;

                // transfer nfts to the receiver
                new TransferTransaction()
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[2]), testEnv.OperatorId, receiverAccountId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the whole collection (addTokenId) - should fail
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId
					}
                    .AddTokenId(nftTokenId)
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("ACCOUNT_AMOUNT_TRANSFERS_ONLY_ALLOWED_FOR_FUNGIBLE_COMMON", exception.Message);

                // reject the whole collection (setTokenIds) - should fail
                Exception exception2 = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId,
						TokenIds = [nftTokenId]
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("ACCOUNT_AMOUNT_TRANSFERS_ONLY_ALLOWED_FOR_FUNGIBLE_COMMON", exception2.Message);

                new TokenDeleteTransaction
                {
                    TokenId = nftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
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
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10)    
                    .AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token with duplicate token id - should fail with TOKEN_REFERENCE_REPEATED
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId,
                        TokenIds = [ftTokenId, ftTokenId],
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("TOKEN_REFERENCE_REPEATED", exception.Message);

                // same test for nft
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // transfer nfts to the receiver
                new TransferTransaction()
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);


                // reject the nft with duplicate nft id - should fail with TOKEN_REFERENCE_REPEATED
                Exception exception2 = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId,
						NftIds = [nftTokenId.Nft(nftSerials[0]), 
                            nftTokenId.Nft(nftSerials[0])],
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("TOKEN_REFERENCE_REPEATED", exception2.Message);
                
                new TokenDeleteTransaction
                {
                    TokenId = ftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenDeleteTransaction
                {
					TokenId = nftTokenId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
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
                new TokenAssociateTransaction
                {
                    AccountId = receiverAccountId,
                    TokenIds = [ ftTokenId ],
                }
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token - should fail with INSUFFICIENT_TOKEN_BALANCE
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId
					}
                    .AddTokenId(ftTokenId)
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("INSUFFICIENT_TOKEN_BALANCE", exception.Message);

                // same test for nft
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // skip the transfer
                // associate the receiver
                new TokenAssociateTransaction
                {
                    AccountId = receiverAccountId,
                    TokenIds = [ nftTokenId ],
                }
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the nft - should fail with INVALID_OWNER_ID
                Exception exception2 = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId
					}
                    .AddNftId(nftTokenId.Nft(nftSerials[0]))
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("INVALID_OWNER_ID", exception2.Message);
                
                new TokenDeleteTransaction
                {
					TokenId = ftTokenId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenDeleteTransaction
                {
                    TokenId = nftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }
        public virtual void CannotRejectTokenWhenTreasuryRejects()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);

                // skip the transfer
                // reject the token with the treasury - should fail with ACCOUNT_IS_TREASURY
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction()
                    {
                        OwnerId = testEnv.OperatorId
                    }
                    .AddTokenId(ftTokenId)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("ACCOUNT_IS_TREASURY", exception.Message);

                // same test for nft
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var nftSerials = mintReceipt.Serials;

                // skip the transfer
                // reject the nft with the treasury - should fail with ACCOUNT_IS_TREASURY
                Exception exception2 = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction()
                    {
                        OwnerId = testEnv.OperatorId
                    }
                    .AddNftId(nftTokenId.Nft(nftSerials[0]))
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("ACCOUNT_IS_TREASURY", exception2.Message);
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
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10)    
                    .AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token with different key - should fail with INVALID_SIGNATURE
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
                        OwnerId = receiverAccountId
                    }
					.AddTokenId(ftTokenId)
                    .FreezeWith(testEnv.Client)
                    .Sign(randomKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("INVALID_SIGNATURE", exception.Message);
                
                new TokenDeleteTransaction
                {
                    TokenId = ftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

            }
        }
        public virtual void CannotRejectTokenWhenTokenOrNFTIdIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // reject the token with invalid token - should fail with EMPTY_TOKEN_REFERENCE_LIST
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = testEnv.OperatorId
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("EMPTY_TOKEN_REFERENCE_LIST", exception.Message);
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
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var nftSerials = mintReceipt.Serials;

                // transfer the tokens to the receiver
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10)
                    .AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[2]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[3]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[4]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[5]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[6]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[7]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[8]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftTokenId.Nft(nftSerials[9]), testEnv.OperatorId, receiverAccountId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // reject the token with 11 token references - should fail with TOKEN_REFERENCE_LIST_SIZE_LIMIT_EXCEEDED
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectTransaction
                    {
						OwnerId = receiverAccountId,
						NftIds = 
                        [
							nftTokenId.Nft(nftSerials[0]), 
                            nftTokenId.Nft(nftSerials[1]), 
                            nftTokenId.Nft(nftSerials[2]), 
                            nftTokenId.Nft(nftSerials[3]), 
                            nftTokenId.Nft(nftSerials[4]), 
                            nftTokenId.Nft(nftSerials[5]), 
                            nftTokenId.Nft(nftSerials[6]), 
                            nftTokenId.Nft(nftSerials[7]), 
                            nftTokenId.Nft(nftSerials[8]), 
                            nftTokenId.Nft(nftSerials[9])
						]
                    }
                    .AddTokenId(ftTokenId)
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains("TOKEN_REFERENCE_LIST_SIZE_LIMIT_EXCEEDED", exception.Message);
                
                new TokenDeleteTransaction
                {
                    TokenId = ftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenDeleteTransaction
                {
                    TokenId = nftTokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }
    }
}