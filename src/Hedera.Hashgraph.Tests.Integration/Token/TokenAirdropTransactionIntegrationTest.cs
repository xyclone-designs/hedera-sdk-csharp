// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

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
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = nftID,
                    Metadata = NftMetadataGenerator.Generate((byte)10)

                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver with unlimited auto associations and receiverSig = false
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, -1);

                // airdrop the tokens
                new TokenAirdropTransaction()
                    .AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the receiver holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                
                Assert.Equal((ulong)amount, receiverAccountBalance.Tokens[tokenID]);
                Assert.Equal((ulong)2, receiverAccountBalance.Tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery { AccountId = testEnv.OperatorId, }.Execute(testEnv.Client);
                
                Assert.Equal(fungibleInitialBalance - amount, operatorBalance.Tokens[tokenID]);
                Assert.Equal(mitedNfts - 2, operatorBalance.Tokens[nftID]);
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
                var mintReceipt = new TokenMintTransaction 
                {
					TokenId = nftID,
					Metadata = NftMetadataGenerator.Generate((byte)10)
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver with 0 auto associations and receiverSig = false
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, receiverAccountKey, 0);

                // airdrop the tokens
                var txn = new TokenAirdropTransaction
                {
					ValidateStatus = true
				}
                    .AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId)
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client);
                txn.GetReceipt(testEnv.Client);
                var record = txn.GetRecord(testEnv.Client);

                // verify in the transaction record the pending airdrops
                Assert.NotNull(record.PendingAirdropRecords);
                Assert.False(record.PendingAirdropRecords.Count == 0);

                // verify the receiver does not hold the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                Assert.Null(receiverAccountBalance.Tokens[tokenID]);
                Assert.Null(receiverAccountBalance.Tokens[nftID]);

                // verify the operator does hold the tokens
                var operatorBalance = new AccountBalanceQuery()AccountId = testEnv.OperatorId,.Execute(testEnv.Client);
                Assert.Equal(fungibleInitialBalance, operatorBalance.Tokens[tokenID]);
                Assert.Equal(mitedNfts, operatorBalance.Tokens[nftID]);
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
                var mintReceipt = new TokenMintTransaction 
                {
					TokenId = nftID,
					Metadata = NftMetadataGenerator.Generate((byte)10)

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // airdrop the tokens to an alias
                PrivateKey privateKey = PrivateKey.GenerateED25519();
                PublicKey publicKey = privateKey.GetPublicKey();
                AccountId aliasAccountId = publicKey.ToAccountId(0, 0);

                // should lazy-create and transfer the tokens
                new TokenAirdropTransaction()
                    .AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, aliasAccountId)
                    .AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, aliasAccountId)
                    .AddTokenTransfer(tokenID, aliasAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the receiver holds the tokens via query
                var receiverAccountBalance = new AccountBalanceQuery { AccountId = aliasAccountId }.Execute(testEnv.Client);
                Assert.Equal((ulong)amount, receiverAccountBalance.Tokens[tokenID]);
                Assert.Equal((ulong)2, receiverAccountBalance.Tokens[nftID]);

                // verify the operator does not hold the tokens
                var operatorBalance = new AccountBalanceQuery { AccountId = testEnv.OperatorId, }.Execute(testEnv.Client);
                Assert.Equal(fungibleInitialBalance - amount, operatorBalance.Tokens[tokenID]);
                Assert.Equal(mitedNfts - 2, operatorBalance.Tokens[nftID]);
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
                CustomFixedFee fee = new CustomFixedFee
                {
					FeeCollectorAccountId = testEnv.OperatorId,
					DenominatingTokenId = customFeeTokenID,
					Amount = 1,
					AllCollectorsAreExempt = true,
				};
                var tokenID = new TokenCreateTransaction
                {
					TokenName = "Test Fungible Token",
					TokenSymbol = "TFT",
					TokenMemo = "I was created for integration tests",
					Decimals = 3,
					InitialSupply = fungibleInitialBalance,
					MaxSupply = fungibleInitialBalance,
					TreasuryAccountId = testEnv.OperatorId,
					TokenSupplyType = TokenSupplyType.Finite,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					MetadataKey = testEnv.OperatorKey,
					PauseKey = testEnv.OperatorKey,
					CustomFees = [fee]
				}
                .Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                // create sender account with unlimited associations and send some tokens to it
                var senderKey = PrivateKey.GenerateED25519();
                var senderAccountID = EntityHelper.CreateAccount(testEnv, senderKey, -1);

                // associate the token to the sender
                new TokenAssociateTransaction
                {
					AccountId = senderAccountID,
					TokenIds = [customFeeTokenID]
				
                }.FreezeWith(testEnv.Client).Sign(senderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // send tokens to the sender
                new TransferTransaction()
                    .AddTokenTransfer(customFeeTokenID, testEnv.OperatorId, -amount)
                    .AddTokenTransfer(customFeeTokenID, senderAccountID, amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction()
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount)
                    .AddTokenTransfer(tokenID, senderAccountID, amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // airdrop the tokens from the sender to the receiver
                new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, senderAccountID, -amount).FreezeWith(testEnv.Client).Sign(senderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // verify the custom fee has been paid by the sender to the collector
                var receiverAccountBalance = new AccountBalanceQuery { AccountId = receiverAccountId }.Execute(testEnv.Client);
                
                Assert.Equal((ulong)amount, receiverAccountBalance.Tokens[tokenID]);
                
                var senderAccountBalance = new AccountBalanceQuery { AccountId = senderAccountID }.Execute(testEnv.Client);
                
                Assert.Equal((ulong)0, senderAccountBalance.Tokens[tokenID]);
                Assert.Equal((ulong)amount - 1, senderAccountBalance.Tokens[customFeeTokenID]);
                
                var operatorBalance = new AccountBalanceQuery { AccountId = testEnv.OperatorId, }.Execute(testEnv.Client);
                
                Assert.Equal(fungibleInitialBalance - amount + 1, operatorBalance.Tokens[customFeeTokenID]);
                Assert.Equal(fungibleInitialBalance - amount, operatorBalance.Tokens[tokenID]);
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
                var receiverAccountId = new AccountCreateTransaction
                {
					Key = receiverAccountKey,
					InitialBalance = new Hbar(1),
					ReceiverSigRequired = true,
					MaxAutomaticTokenAssociations = -1
				
                }.FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // airdrop the tokens
                new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, receiverAccountId, amount)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanAirdropTokensWithReceiverSigRequiredNFT()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create nft
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = nftID,
					Metadata = NftMetadataGenerator.Generate((byte)10)
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create receiver with unlimited auto associations and receiverSig = true
                var receiverAccountKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction
                {
					Key = receiverAccountKey,
					InitialBalance = new Hbar(1),
					ReceiverSigRequired = true,
					MaxAutomaticTokenAssociations = -1
				
                }.FreezeWith(testEnv.Client).Sign(receiverAccountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // airdrop the tokens
                new TokenAirdropTransaction()
                    .AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, receiverAccountId)
                    .AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, receiverAccountId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
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
                new TransferTransaction()
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, -amount)
                    .AddTokenTransfer(tokenID, senderAccountID, amount).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenAllowance(tokenID, senderAccountID, spenderAccountID, amount).FreezeWith(testEnv.Client).Sign(senderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // airdrop the tokens from the sender to the spender via approval
                // fails with NOT_SUPPORTED
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenAirdropTransaction
                    {
                        TransactionId = TransactionId.Generate(spenderAccountID) 
                    }
                    .AddTokenTransfer(tokenID, spenderAccountID, amount)
                    .AddApprovedTokenTransfer(tokenID, spenderAccountID, -amount)
                    .FreezeWith(testEnv.Client).Sign(spenderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.NotSupported.ToString(), exception.Message);
            }
        }

        public virtual void CannotAirdropTokensWithAllowanceAndWithoutBalanceNFT()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // create nft
                var nftID = EntityHelper.CreateNft(testEnv);

                // mint some NFTs
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = nftID,
					Metadata = NftMetadataGenerator.Generate((byte)10)
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = mintReceipt.Serials;

                // create spender and approve to it some tokens
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountID = EntityHelper.CreateAccount(testEnv, spenderKey, -1);

                // create sender
                var senderKey = PrivateKey.GenerateED25519();
                var senderAccountID = EntityHelper.CreateAccount(testEnv, senderKey, -1);

                // transfer ft to sender
                new TransferTransaction()
                    .AddNftTransfer(nftID.Nft(nftSerials[0]), testEnv.OperatorId, senderAccountID)
                    .AddNftTransfer(nftID.Nft(nftSerials[1]), testEnv.OperatorId, senderAccountID).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // approve allowance to the spender
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenNftAllowance(nftID.Nft(nftSerials[0]), senderAccountID, spenderAccountID)
                    .ApproveTokenNftAllowance(nftID.Nft(nftSerials[1]), senderAccountID, spenderAccountID).FreezeWith(testEnv.Client).Sign(senderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // airdrop the tokens from the sender to the spender via approval
                // fails with NOT_SUPPORTED
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenAirdropTransaction
                    {
						TransactionId = TransactionId.Generate(spenderAccountID)
					}
                    .AddApprovedNftTransfer(nftID.Nft(nftSerials[0]), senderAccountID, spenderAccountID)
                    .AddApprovedNftTransfer(nftID.Nft(nftSerials[1]), senderAccountID, spenderAccountID).FreezeWith(testEnv.Client).Sign(spenderKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.NotSupported.ToString(), exception.Message);
            }
        }

        public virtual void CannotAirdropTokensWithInvalidBody()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // airdrop with no tokenID or NftID
                // fails with EMPTY_TOKEN_TRANSFER_BODY
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenAirdropTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.EmptyTokenTransferBody.ToString(), exception.Message);

                // create fungible token
                var tokenID = EntityHelper.CreateFungibleToken(testEnv, 3);

                // airdrop with invalid transfers
                // fails with INVALID_TRANSACTION_BODY
                PrecheckStatusException exception2 = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenAirdropTransaction()
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, 100)
                    .AddTokenTransfer(tokenID, testEnv.OperatorId, 100).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.InvalidTransaction.ToString(), exception2.Message);
            }
        }
    }
}