// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenWipeIntegrationTest
    {
        public virtual void CanWipeAccountsBalance()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)
				}
                Key = key,
				.Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					KycKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId]

				}.FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction
                {
					AccountId = accountId,
					TokenId = tokenId

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.OperatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenWipeTransaction
                {
					TokenId = tokenId,
					AccountId = accountId,
					Amount = 10
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanWipeAccountsNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)
				}
				Key = key,
                .Execute(testEnv.Client);

                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique, 
                    TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					KycKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = tokenId,
					Metadata = NftMetadataGenerator.Generate((byte)10)

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId]

				}.FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction
                {
					AccountId = accountId,
					TokenId = tokenId

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var serialsToTransfer = mintReceipt.Serials[0..4];
                var transfer = new TransferTransaction();

                foreach (var serial in serialsToTransfer)
					transfer.AddNftTransfer(tokenId.Nft(serial), testEnv.OperatorId, accountId);

				transfer.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                new TokenWipeTransaction
                {
					TokenId = tokenId,
					AccountId = accountId,
					Serials = serialsToTransfer

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotWipeAccountsNftsIfNotOwned()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)
				}
				Key = key,
                .Execute(testEnv.Client);
                
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique, 
                    TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					KycKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = tokenId,
					Metadata = NftMetadataGenerator.Generate((byte)10)

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId]

				}.FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction
                {
					AccountId = accountId,
					TokenId = tokenId

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var serialsToTransfer = mintReceipt.Serials[0 .. 4];

				// don't transfer them
				ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenWipeTransaction
                    {
						TokenId = tokenId,
						AccountId = accountId,
						Serials = serialsToTransfer

					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); 
                
                Assert.Contains(ResponseStatus.AccountDoesNotOwnWipedNft.ToString(), exception.Message);
            }
        }

        public virtual void CannotWipeAccountsBalanceWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)
				}
				Key = key,
                .Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					KycKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId]

				}.FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                new TokenGrantKycTransaction
                {
					AccountId = accountId,
					TokenId = tokenId

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                new TransferTransaction()
                    .AddTokenTransfer(tokenId, testEnv.OperatorId, -10)
                    .AddTokenTransfer(tokenId, accountId, 10)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenWipeTransaction
                    {
						TokenId = tokenId,
						Amount = 10

					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); 
                Assert.Contains(ResponseStatus.InvalidAccountId.ToString(), exception.Message);
            }
        }

        public virtual void CannotWipeAccountsBalanceWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)
				}
                Key = key,
				.Execute(testEnv.Client);

                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					KycKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId]
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenGrantKycTransaction
                {
					AccountId = accountId,
					TokenId = tokenId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TransferTransaction()
                    .AddTokenTransfer(tokenId, testEnv.OperatorId, -10)
                    .AddTokenTransfer(tokenId, accountId, 10)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenWipeTransaction
                    {
						AccountId = accountId,
						Amount = 10
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.InvalidTokenId.ToString(), exception.Message);
            }
        }

        public virtual void CanWipeAccountsBalanceWhenAmountIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)
				}
				Key = key,
                .Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;

                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					KycKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId]
				}
                .FreezeWith(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                new TokenGrantKycTransaction
                {
					AccountId = accountId,
					TokenId = tokenId
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                new TransferTransaction()
                    .AddTokenTransfer(tokenId, testEnv.OperatorId, -10)
                    .AddTokenTransfer(tokenId, accountId, 10)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                var receipt = new TokenWipeTransaction
                {
					TokenId = tokenId,
					AccountId = accountId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                Assert.Equal(ResponseStatus.Success, receipt.Status);
            }
        }
    }
}