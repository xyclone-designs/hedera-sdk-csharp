// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenBurnIntegrationTest
    {
        public virtual void CanBurnTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
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
				
                }.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var receipt = new TokenBurnTransaction
                {
					Amount = 10,
					TokenId = tokenId,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal<ulong>(receipt.TotalSupply, 1000000 - 10);
            }
        }

        public virtual void CannotBurnTokensWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenBurnTransaction
                    {
						Amount = 10

					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidTokenId.ToString(), exception.Message);
            }
        }

        public virtual void CanBurnTokensWhenAmountIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
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

				}.Execute(testEnv.Client);
                
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var receipt = new TokenBurnTransaction
                {
					TokenId = tokenId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                Assert.Equal(receipt.Status, ResponseStatus.Success);
            }
        }

        public virtual void CannotBurnTokensWhenSupplyKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
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
					SupplyKey = PrivateKey.Generate(),
					FreezeDefault = false,

				}.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenBurnTransaction
                    {
						TokenId = tokenId,
						Amount = 10

					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }

        public virtual void CanBurnNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction
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

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var tokenId = createReceipt.TokenId;
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = tokenId,
					Metadata = NftMetadataGenerator.Generate((byte)10)

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                new TokenBurnTransaction
                {
					Serials = mintReceipt.Serials[0..4),
					TokenId = tokenId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotBurnNftsWhenNftIsNotOwned()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique, 
                    TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var tokenId = createReceipt.tokenId);
                var serials = new TokenMintTransaction
                {
					TokenId = tokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)1)

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).Serials;
                var key = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(1),

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                
                new TokenAssociateTransaction
                {
					AccountId = accountId,
					TokenIds = [tokenId],
				}
                    .FreezeWith(testEnv.Client)
                    .SignWithOperator(testEnv.Client)
                    .Sign(key)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                new TransferTransaction()
                    .AddNftTransfer(tokenId.Nft(serials[0]), testEnv.OperatorId, accountId)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenBurnTransaction
                    {
						Serials = serials,
						TokenId = tokenId,

					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TreasuryMustOwnBurnedNft.ToString(), exception.Message);
            }
        }
    }
}