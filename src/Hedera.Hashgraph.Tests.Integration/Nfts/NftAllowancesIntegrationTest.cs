// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Nio.Charset;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class NftAllowancesIntegrationTest
    {
        public virtual void CannotTransferWithoutAllowanceApproval()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountId = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(2)
				}
                .SetKeyWithoutAlias(spenderKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;
                var receiverKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction
                {
					MaxAutomaticTokenAssociations = 10
				}
                .SetKeyWithoutAlias(receiverKey)
                .Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                
                TokenId nftTokenId = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey
                }
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                
                new TokenAssociateTransaction
                {
					AccountId = spenderAccountId,
					TokenIds = [nftTokenId],
				}
                .FreezeWith(testEnv.Client)
                .Sign(spenderKey)
                .Execute(testEnv.Client);

                var serials = new TokenMintTransaction
                {
					TokenId = nftTokenId,
                    MetadataList = [Encoding.UTF8.GetBytes("asd")]
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).Serials;
                
                var nft1 = new NftId(nftTokenId, serials[0]);
                var onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);

                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    return new TransferTransaction
                    {
                        TransactionId = onBehalfOfTransactionId
                    }
                    .AddApprovedNftTransfer(nft1, testEnv.OperatorId, receiverAccountId)
                    .FreezeWith(testEnv.Client)
                    .Sign(spenderKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

				}).WithMessageContaining(Status.SPENDER_DOES_NOT_HAVE_ALLOWANCE.ToString());
            }
        }

        public virtual void CannotTransferAfterAllowanceRemove()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountId = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(2)

				}
                .SetKeyWithoutAlias(spenderKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                var receiverKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction()
                .SetKeyWithoutAlias(receiverKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                TokenId nftTokenId = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                }
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                new TokenAssociateTransaction
                {
					AccountId = spenderAccountId,
					TokenIds = [nftTokenId],
				}
                .FreezeWith(testEnv.Client)
                .Sign(spenderKey)
                .Execute(testEnv.Client);

                new TokenAssociateTransaction
                {
                    AccountId = receiverAccountId,
                    TokenIds = [ nftTokenId ],
                }
                .FreezeWith(testEnv.Client)
                .Sign(receiverKey)
                .Execute(testEnv.Client);

                var serials = new TokenMintTransaction
                {
					TokenId = nftTokenId
				}
                    .AddMetadata(Encoding.UTF8.GetBytes("asd1"))
                    .AddMetadata(Encoding.UTF8.GetBytes("asd2"))
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).Serials;

                var nft1 = new NftId(nftTokenId, serials[0]);
                var nft2 = new NftId(nftTokenId, serials[1]);
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenNftAllowance(nft1, testEnv.OperatorId, spenderAccountId)
                    .ApproveTokenNftAllowance(nft2, testEnv.OperatorId, spenderAccountId)
                    .Execute(testEnv.Client);
                new AccountAllowanceDeleteTransaction().DeleteAllTokenNftAllowances(nft2, testEnv.OperatorId).Execute(testEnv.Client);
                var onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);
                new TransferTransaction 
                { 
                    TransactionId = onBehalfOfTransactionId 
                }
                .AddApprovedNftTransfer(nft1, testEnv.OperatorId, receiverAccountId)
                .FreezeWith(testEnv.Client)
                .Sign(spenderKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var info = new TokenNftInfoQuery 
                { 
                    NftId = nft1 
                
                }.Execute(testEnv.Client);
                Assert.Equal(info[0].accountId, receiverAccountId);
                var onBehalfOfTransactionId2 = TransactionId.Generate(spenderAccountId);
                Assert.Throws(typeof(ReceiptStatusException), () => 
                {
                    return new TransferTransaction
                    {
                        TransactionId = onBehalfOfTransactionId2
                    }
                    .AddApprovedNftTransfer(nft2, testEnv.OperatorId, receiverAccountId)
                    .FreezeWith(testEnv.Client)
                    .Sign(spenderKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

				}).WithMessageContaining(Status.SPENDER_DOES_NOT_HAVE_ALLOWANCE.ToString());
            }
        }

        public virtual void CannotRemoveSingleSerialWhenAllowanceIsGivenForAll()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(spenderKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                var receiverKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                TokenId nftTokenId = new TokenCreateTransaction
                { 
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique, 
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                }
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction().SetAccountId(spenderAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.Client).Sign(spenderKey).Execute(testEnv.Client);
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(List.Of(nftTokenId)).FreezeWith(testEnv.Client).Sign(receiverKey).Execute(testEnv.Client);
                var serials = new TokenMintTransaction().SetTokenId(nftTokenId).AddMetadata(Encoding.UTF8.GetBytes("asd1")).AddMetadata(Encoding.UTF8.GetBytes("asd2")).Execute(testEnv.Client).GetReceipt(testEnv.Client).Serials;
                var nft1 = new NftId(nftTokenId, serials[0]);
                var nft2 = new NftId(nftTokenId, serials[1]);
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenNftAllowanceAllSerials(nftTokenId, testEnv.OperatorId, spenderAccountId)
                    .Execute(testEnv.Client);
                var onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);
                new TransferTransaction 
                { 
                    TransactionId = onBehalfOfTransactionId 
                }
                .AddApprovedNftTransfer(nft1, testEnv.OperatorId, receiverAccountId)
                .FreezeWith(testEnv.Client)
                .Sign(spenderKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);


                // hopefully in the future this should end up with a precheck error provided from services
                new AccountAllowanceDeleteTransaction().DeleteAllTokenNftAllowances(nft2, testEnv.OperatorId).Execute(testEnv.Client);
                var onBehalfOfTransactionId2 = TransactionId.Generate(spenderAccountId);
                new TransferTransaction 
                { 
                    TransactionId = onBehalfOfTransactionId2 
                }
                .AddApprovedNftTransfer(nft2, testEnv.OperatorId, receiverAccountId)
                .FreezeWith(testEnv.Client)
                .Sign(spenderKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var infoNft1 = new TokenNftInfoQuery 
                { 
                    NftId = nft1 
                
                }.Execute(testEnv.Client);
                var infoNft2 = new TokenNftInfoQuery 
                { 
                    NftId = nft2 
                
                }.Execute(testEnv.Client);

                Assert.Equal(infoNft1[0].accountId, receiverAccountId);
                Assert.Equal(infoNft2[0].accountId, receiverAccountId);
            }
        }

        public virtual void AccountGivenAllowanceForAllShouldBeAbleToGiveAllowanceForSingle()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var delegatingSpenderKey = PrivateKey.GenerateED25519();
                var delegatingSpenderAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(delegatingSpenderKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                var spenderKey = PrivateKey.GenerateED25519();
                var spenderAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(spenderKey).SetInitialBalance(new Hbar(2)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                var receiverKey = PrivateKey.GenerateED25519();
                var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                TokenId nftTokenId = new TokenCreateTransaction
                { 
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique, 
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                }
                .FreezeWith(testEnv.Client)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                new TokenAssociateTransaction()
                    .SetAccountId(delegatingSpenderAccountId)
                    .SetTokenIds([nftTokenId])
                    .FreezeWith(testEnv.Client)
                    .Sign(spenderKey)
                    .Execute(testEnv.Client);
                new TokenAssociateTransaction()
                    .SetAccountId(receiverAccountId)
                    .SetTokenIds([nftTokenId])
                    .FreezeWith(testEnv.Client)
                    .Sign(receiverKey)
                    .Execute(testEnv.Client);
                var serials = new TokenMintTransaction()
                    .SetTokenId(nftTokenId)
                    .AddMetadata(Encoding.UTF8.GetBytes("asd1"))
                    .AddMetadata(Encoding.UTF8.GetBytes("asd2"))
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client).Serials;
                var nft1 = new NftId(nftTokenId, serials[0]);
                var nft2 = new NftId(nftTokenId, serials[1]);
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenNftAllowanceAllSerials(nftTokenId, testEnv.OperatorId, delegatingSpenderAccountId)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                new AccountAllowanceApproveTransaction()
                    .ApproveTokenNftAllowance(nft1, testEnv.OperatorId, spenderAccountId, delegatingSpenderAccountId)
                    .FreezeWith(testEnv.Client)
                    .Sign(delegatingSpenderKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                var onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);

                new TransferTransaction 
                { 
                    TransactionId = onBehalfOfTransactionId 
                }
                .AddApprovedNftTransfer(nft1, testEnv.OperatorId, receiverAccountId)
                .FreezeWith(testEnv.Client)
                .Sign(spenderKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var onBehalfOfTransactionId2 = TransactionId.Generate(spenderAccountId);
                
                Assert.Throws(typeof(ReceiptStatusException), () =>
                { 
                    return new TransferTransaction
					{
						TransactionId = onBehalfOfTransactionId2
					}
				    .AddApprovedNftTransfer(nft2, testEnv.OperatorId, receiverAccountId)
				    .FreezeWith(testEnv.Client)
				    .Sign(spenderKey)
				    .Execute(testEnv.Client)
				    .GetReceipt(testEnv.Client)


				}).WithMessageContaining(Status.SPENDER_DOES_NOT_HAVE_ALLOWANCE.ToString());
                
                var infoNft1 = new TokenNftInfoQuery 
                { 
                    NftId = nft1 
                
                }.Execute(testEnv.Client);

                var infoNft2 = new TokenNftInfoQuery 
                { 
                    NftId = nft2 
                
                }.Execute(testEnv.Client);
                
                Assert.Equal(infoNft1[0].accountId, receiverAccountId);
                Assert.Equal(infoNft2[0].accountId, testEnv.OperatorId);
            }
        }
    }
}