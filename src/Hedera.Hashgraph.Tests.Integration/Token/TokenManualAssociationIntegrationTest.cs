// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenManualAssociationIntegrationTest
    {
        public virtual void CanManuallyAssociateAccountWithFungibleToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                uint tokenDecimals = 3;
                var tokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                
                new TokenAssociateTransaction
                {
					AccountId = receiverAccountId,
					TokenIds = [tokenId],
				
                }.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                var accountInfo = new AccountInfoQuery
                {
					AccountId = receiverAccountId
				
                }.Execute(testEnv.Client);
                
                Assert.Equal(accountInfo.TokenRelationships[tokenId].Decimals, tokenDecimals);
                
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.OperatorId, -10).AddTokenTransfer(tokenId, receiverAccountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                var accountBalance = new AccountBalanceQuery
                {
					AccountId = receiverAccountId
				
                }.Execute(testEnv.Client);

                Assert.Equal(accountBalance.Tokens[tokenId], (ulong)10);
            }
        }

        public virtual void CanManuallyAssociateAccountWithNft()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = EntityHelper.CreateNft(testEnv);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                var mintReceiptToken = new TokenMintTransaction
                {
					TokenId = tokenId,
                    Metadata = NftMetadataGenerator.Generate((byte)10)
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                new TokenAssociateTransaction
                {
					AccountId = receiverAccountId,
					TokenIds = [tokenId],
				
                }.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var nftTransferTransaction = new TransferTransaction();

                foreach (var serial in mintReceiptToken.Serials)
                {
                    nftTransferTransaction.AddNftTransfer(tokenId.Nft(serial), testEnv.OperatorId, receiverAccountId);
                }

                nftTransferTransaction.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanManuallyAssociateContractWithFungibleToken()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                uint tokenDecimals = 3;
                var tokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var contractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                new TokenAssociateTransaction
                {
					AccountId = new AccountId(0, 0, contractId.Num),
					TokenIds = [tokenId],
				
                }.FreezeWith(testEnv.Client).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var contractInfo = new ContractInfoQuery
                {
					ContractId = contractId

				}.Execute(testEnv.Client);

                Assert.Equal(contractInfo.ContractId, contractId);
                Assert.NotNull(contractInfo.AccountId);
                Assert.Equal(contractInfo.AccountId.ToString(), contractId.ToString());
                Assert.NotNull(contractInfo.AdminKey);
                Assert.Equal(contractInfo.AdminKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(contractInfo.Storage, 128);
                Assert.Equal(contractInfo.ContractMemo, "[e2e::ContractMemo]");
                Assert.Equal(contractInfo.TokenRelationships[tokenId].Decimals, tokenDecimals);
                
                new ContractDeleteTransaction
                {
					TransferAccountId = testEnv.OperatorId,
					ContractId = contractId,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanManuallyAssociateContractWithNft()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var tokenId = EntityHelper.CreateNft(testEnv);
                var contractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                new TokenAssociateTransaction
                {
					AccountId = new AccountId(0, 0, contractId.Num),
					TokenIds = [tokenId],
				
                }.FreezeWith(testEnv.Client).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                var contractInfo = new ContractInfoQuery
                {
					ContractId = contractId
				
                }.Execute(testEnv.Client);

                Assert.Equal(contractInfo.ContractId, contractId);
                Assert.NotNull(contractInfo.AccountId);
                Assert.Equal(contractInfo.AccountId.ToString(), contractId.ToString());
                Assert.NotNull(contractInfo.AdminKey);
                Assert.Equal(contractInfo.AdminKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(contractInfo.Storage, 128);
                Assert.Equal(contractInfo.ContractMemo, "[e2e::ContractMemo]");
                
                new ContractDeleteTransaction
                {
					TransferAccountId = testEnv.OperatorId,
					ContractId = contractId,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenAssociateTransactionEvenWhenTokenIDsAreNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                new TokenAssociateTransaction
                {
					AccountId = accountId,
				
                }.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotAssociateAccountWithTokensWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenAssociateTransaction().FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidAccountId.ToString(), exception.Message);
            }
        }

        public virtual void CannotAssociateAccountWhenAccountDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                uint tokenDecimals = 3;
                var tokenId = EntityHelper.CreateFungibleToken(testEnv, tokenDecimals);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenAssociateTransaction
                    {
						AccountId = accountId,
						TokenIds = [tokenId],
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }
    }
}