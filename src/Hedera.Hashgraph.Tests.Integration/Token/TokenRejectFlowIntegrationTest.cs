// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TokenRejectFlowIntegrationTest
    {
        public virtual void CanExecuteTokenRejectFlowForFungibleToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // manually associate ft
                new TokenAssociateTransaction
                {
					AccountId = receiverAccountId,
					TokenIds = [ftTokenId]
				}
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // transfer fts to the receiver
                new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // execute the token reject flow
                new TokenRejectFlow
                {
					OwnerId = receiverAccountId,
					TokenId = ftTokenId,
				}
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId

				}.Execute(testEnv.Client);
                Assert.Equal(treasuryAccountBalance.Tokens[ftTokenId], 1000000);

                // verify the allowance - should be 0, because TokenRejectFlow dissociates
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); Assert.Contains("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT", exception.Message);
                new TokenDeleteTransaction(, TokenId = ftTokenId)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenRejectFlowForFungibleTokenAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var ftTokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // manually associate ft
                new TokenAssociateTransaction
                {
					AccountId = receiverAccountId,
					TokenIds = [ftTokenId]
				}
                .FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // transfer fts to the receiver
                new TransferTransaction()
                    .AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10)
                    .AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // execute the token reject flow
                new TokenRejectFlow
                {
					OwnerId = receiverAccountId,
					TokenIds = [ftTokenId]
				}
				.FreezeWith(testEnv.Client)
                .Sign(receiverAccountKey)
                .ExecuteAsync(testEnv.Client).Get()
                .GetReceipt(testEnv.Client);

                // verify the tokens are transferred back to the treasury
                var treasuryAccountBalance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId

				}.Execute(testEnv.Client);
                Assert.Equal<ulong>(1000000, treasuryAccountBalance.Tokens[ftTokenId]);

                // verify the tokens are not associated with the receiver
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TransferTransaction().AddTokenTransfer(ftTokenId, testEnv.OperatorId, -10).AddTokenTransfer(ftTokenId, receiverAccountId, 10)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); Assert.Contains("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT", exception.Message);
                new TokenDeleteTransaction
                {
					TokenId = ftTokenId
				}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenRejectFlowForNft()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var nftTokenId = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);
                var mintReceiptToken = new TokenMintTransaction
                {
					TokenId = nftTokenId,
					Metadata = NftMetadataGenerator.Generate((byte)10)
				}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                var nftSerials = mintReceiptToken.Serials;

                // manually associate bft
                new TokenAssociateTransaction
                {
					AccountId = receiverAccountId,
					TokenIds = [nftTokenId]
				}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // execute the token reject flow
                new TokenRejectFlow
                {
					OwnerId = receiverAccountId,
					NftIds = [nftTokenId.Nft(nftSerials[0]), nftTokenId.Nft(nftSerials[1])]
				}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // verify the token is transferred back to the treasury
                var nftTokenIdNftInfo = new TokenNftInfoQuery
                {
					NftId = nftTokenId.Nft(nftSerials[1])
				
                }.Execute(testEnv.Client);
                Assert.Equal(nftTokenIdNftInfo[0].AccountId, testEnv.OperatorId);

                // verify the tokens are not associated with the receiver
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TransferTransaction().AddNftTransfer(nftTokenId.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); Assert.Contains("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT", exception.Message);
                new TokenDeleteTransaction
                {
					TokenId = nftTokenId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenRejectFlowForNftWhenRejectingOnlyPartOfOwnedNFTs()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var nftTokenId1 = EntityHelper.CreateNft(testEnv);
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);
                var mintReceiptToken = new TokenMintTransaction
                {
					TokenId = nftTokenId1,
					Metadata = NftMetadataGenerator.Generate((byte)10)
				}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                var nftSerials = mintReceiptToken.Serials;

                // manually associate bft
                new TokenAssociateTransaction
                {
					AccountId = receiverAccountId,
					TokenIds = [nftTokenId1]
				}
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // transfer nfts to the receiver
                new TransferTransaction().AddNftTransfer(nftTokenId1.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId).AddNftTransfer(nftTokenId1.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                // execute the token reject flow
                Exception exception = Assert.Throws<Exception>(() =>
                {
                    new TokenRejectFlow
                    {
						OwnerId = receiverAccountId,
						NftIds = [ nftTokenId1.Nft(nftSerials[1]) ]
					}
                    .FreezeWith = testEnv.Client)
                    .Sign(receiverAccountKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains("ACCOUNT_STILL_OWNS_NFTS", exception.Message);
            }
        }
    }
}