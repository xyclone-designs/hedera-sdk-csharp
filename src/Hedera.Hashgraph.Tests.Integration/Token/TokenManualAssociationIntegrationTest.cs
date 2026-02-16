// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var accountInfo = new AccountInfoQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
                Assert.Equal(accountInfo.tokenRelationships[tokenId].decimals, tokenDecimals);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.operatorId, -10).AddTokenTransfer(tokenId, receiverAccountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);
                var accountBalance = new AccountBalanceQuery().SetAccountId(receiverAccountId).Execute(testEnv.client);
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
                var mintReceiptToken = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(receiverAccountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var serialsToTransfer = new List(mintReceiptToken.serials);
                var nftTransferTransaction = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
                {
                    nftTransferTransaction.AddNftTransfer(tokenId.Nft(serial), testEnv.operatorId, receiverAccountId);
                }

                nftTransferTransaction.Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanManuallyAssociateContractWithFungibleToken()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var tokenDecimals = 3;
                var tokenId = EntityHelper.CreateFungibleToken(testEnv, 3);
                var contractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                new TokenAssociateTransaction().SetAccountId(new AccountId(0, 0, contractId.num)).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client);
                var contractInfo = new ContractInfoQuery().SetContractId(contractId).Execute(testEnv.client);
                Assert.Equal(contractInfo.contractId, contractId);
                AssertThat(contractInfo.accountId).IsNotNull();
                Assert.Equal(Objects.RequireNonNull(contractInfo.accountId).ToString(), Objects.RequireNonNull(contractId).ToString());
                AssertThat(contractInfo.adminKey).IsNotNull();
                Assert.Equal(Objects.RequireNonNull(contractInfo.adminKey).ToString(), Objects.RequireNonNull(testEnv.operatorKey).ToString());
                Assert.Equal(contractInfo.storage, 128);
                Assert.Equal(contractInfo.contractMemo, "[e2e::ContractMemo]");
                Assert.Equal(contractInfo.tokenRelationships[tokenId].decimals, tokenDecimals);
                new ContractDeleteTransaction().SetTransferAccountId(testEnv.operatorId).SetContractId(contractId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanManuallyAssociateContractWithNft()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var tokenId = EntityHelper.CreateNft(testEnv);
                var contractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                new TokenAssociateTransaction().SetAccountId(new AccountId(0, 0, contractId.num)).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Execute(testEnv.client).GetReceipt(testEnv.client);
                var contractInfo = new ContractInfoQuery().SetContractId(contractId).Execute(testEnv.client);
                Assert.Equal(contractInfo.contractId, contractId);
                AssertThat(contractInfo.accountId).IsNotNull();
                Assert.Equal(Objects.RequireNonNull(contractInfo.accountId).ToString(), Objects.RequireNonNull(contractId).ToString());
                AssertThat(contractInfo.adminKey).IsNotNull();
                Assert.Equal(Objects.RequireNonNull(contractInfo.adminKey).ToString(), Objects.RequireNonNull(testEnv.operatorKey).ToString());
                Assert.Equal(contractInfo.storage, 128);
                Assert.Equal(contractInfo.contractMemo, "[e2e::ContractMemo]");
                new ContractDeleteTransaction().SetTransferAccountId(testEnv.operatorId).SetContractId(contractId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanExecuteTokenAssociateTransactionEvenWhenTokenIDsAreNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                new TokenAssociateTransaction().SetAccountId(accountId).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotAssociateAccountWithTokensWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var accountKey = PrivateKey.GenerateED25519();
                var accountMaxAutomaticTokenAssociations = 0;
                var accountId = EntityHelper.CreateAccount(testEnv, accountKey, accountMaxAutomaticTokenAssociations);
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenAssociateTransaction().FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
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
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }
    }
}