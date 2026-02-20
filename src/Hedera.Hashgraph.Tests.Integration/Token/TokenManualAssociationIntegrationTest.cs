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
                var tokenDecimals = 3;
                var tokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var receiverAccountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                new TokenAssociateTransaction().SetAccountId(receiverAccountId)TokenIds = [tokenId],.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountInfo = new AccountInfoQuery().SetAccountId(receiverAccountId).Execute(testEnv.Client);
                Assert.Equal(accountInfo.tokenRelationships[tokenId].decimals, tokenDecimals);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.OperatorId, -10).AddTokenTransfer(tokenId, receiverAccountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var accountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.Client);
                Assert.Equal(accountBalance.tokens[tokenId], 10);
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
                var mintReceiptToken = new TokenMintTransaction()TokenId = tokenId,.SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenAssociateTransaction().SetAccountId(receiverAccountId)TokenIds = [tokenId],.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var serialsToTransfer = new List(mintReceiptToken.serials);
                var nftTransferTransaction = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
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
                var tokenDecimals = 3;
                var tokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var contractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                new TokenAssociateTransaction().SetAccountId(new AccountId(0, 0, contractId.num))TokenIds = [tokenId],.FreezeWith(testEnv.Client).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var contractInfo = new ContractInfoQuery().SetContractId(contractId).Execute(testEnv.Client);
                Assert.Equal(contractInfo.contractId, contractId);
                Assert.NotNull(contractInfo.accountId);
                Assert.Equal(contractInfo.accountId).ToString(), contractId().ToString());
                Assert.NotNull(contractInfo.adminKey);
                Assert.Equal(contractInfo.adminKey).ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(contractInfo.storage, 128);
                Assert.Equal(contractInfo.contractMemo, "[e2e::ContractMemo]");
                Assert.Equal(contractInfo.tokenRelationships[tokenId].decimals, tokenDecimals);
                new ContractDeleteTransaction().SetTransferAccountId(testEnv.OperatorId).SetContractId(contractId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanManuallyAssociateContractWithNft()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var tokenId = EntityHelper.CreateNft(testEnv);
                var contractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                new TokenAssociateTransaction().SetAccountId(new AccountId(0, 0, contractId.num))TokenIds = [tokenId],.FreezeWith(testEnv.Client).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var contractInfo = new ContractInfoQuery().SetContractId(contractId).Execute(testEnv.Client);
                Assert.Equal(contractInfo.contractId, contractId);
                Assert.NotNull(contractInfo.accountId);
                Assert.Equal(contractInfo.accountId).ToString(), contractId().ToString());
                Assert.NotNull(contractInfo.adminKey);
                Assert.Equal(contractInfo.adminKey).ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(contractInfo.storage, 128);
                Assert.Equal(contractInfo.contractMemo, "[e2e::ContractMemo]");
                new ContractDeleteTransaction().SetTransferAccountId(testEnv.OperatorId).SetContractId(contractId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanExecuteTokenAssociateTransactionEvenWhenTokenIDsAreNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                new TokenAssociateTransaction()AccountId = accountId,.FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
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
                var tokenDecimals = 3;
                var tokenId = EntityHelper.CreateFungibleToken(testEnv, tokenDecimals);
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenAssociateTransaction()AccountId = accountId,TokenIds = [tokenId],.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }
    }
}