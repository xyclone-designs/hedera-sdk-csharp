// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenNftTransferIntegrationTest
    {
        public virtual void CanTransferNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                TransactionResponse response = new AccountCreateTransaction
                {
					Key = key,
					InitialBalance = new Hbar(1),

				}.Execute(testEnv.Client);

                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                response = new TokenCreateTransaction
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

				}.Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                Assert.NotNull(tokenId);
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = tokenId,
					Metadata = NftMetadataGenerator.Generate((byte)10),

				}.Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

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

                new TokenGrantKycTransaction
                {
					AccountId = accountId,
					TokenId = tokenId,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                var serialsToTransfer = new List<long>(mintReceipt.Serials[0 .. 4)];
                var transfer = new TransferTransaction();

                foreach (var serial in serialsToTransfer)
                {
                    transfer.AddNftTransfer(tokenId.Nft(serial), testEnv.OperatorId, accountId);
                }

                transfer.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                new TokenWipeTransaction
                {
					TokenId = tokenId,
					AccountId = accountId,
					Serials = serialsToTransfer
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotTransferUnownedNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                TransactionResponse response = new AccountCreateTransaction()
                    Key = key,
                    InitialBalance = new Hbar(1),
                    .Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                Assert.NotNull(accountId);
                response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    FreezeKey = testEnv.OperatorKey,
                    WipeKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                    FreezeDefault = false
                
                }.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                Assert.NotNull(tokenId);
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = tokenId,
					Metadata = NftMetadataGenerator.Generate((byte)10),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new TokenAssociateTransaction
                {
					TokenIds = [tokenId],
					AccountId = accountId
				}
                .FreezeWith(testEnv.Client)
                .SignWithOperator(testEnv.Client)
                .Sign(key)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var serialsToTransfer = new List<long>(mintReceipt.Serials[0 .. 4)];
                var transfer = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
                {
                    // Try to transfer in wrong direction
                    transfer.AddNftTransfer(tokenId.Nft(serial), accountId, testEnv.OperatorId);
                }

                transfer.FreezeWith(testEnv.Client).Sign(key);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    transfer.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.SENDER_DOES_NOT_OWN_NFT_SERIAL_NO.ToString(), exception.Message);
            }
        }
    }
}