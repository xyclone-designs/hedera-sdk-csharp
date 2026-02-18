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
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenWipeIntegrationTest
    {
        public virtual void CanWipeAccountsBalance()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.OperatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).SetAmount(10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanWipeAccountsNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenType(TokenType.NonFungibleUnique)TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var serialsToTransfer = mintReceipt.serials.SubList(0, 4);
                var transfer = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
                {
                    transfer.AddNftTransfer(tokenId.Nft(serial), testEnv.OperatorId, accountId);
                }

                transfer.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).SetSerials(serialsToTransfer).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotWipeAccountsNftsIfNotOwned()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenType(TokenType.NonFungibleUnique)TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var serialsToTransfer = mintReceipt.serials.SubList(0, 4);

                // don't transfer them
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).SetSerials(serialsToTransfer).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.ACCOUNT_DOES_NOT_OWN_WIPED_NFT.ToString());
            }
        }

        public virtual void CannotWipeAccountsBalanceWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.OperatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenWipeTransaction().SetTokenId(tokenId).SetAmount(10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
            }
        }

        public virtual void CannotWipeAccountsBalanceWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.OperatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenWipeTransaction().SetAccountId(accountId).SetAmount(10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        public virtual void CanWipeAccountsBalanceWhenAmountIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds([tokenId]).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.OperatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var receipt = new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }
    }
}