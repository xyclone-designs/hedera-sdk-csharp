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
    class TokenWipeIntegrationTest
    {
        public virtual void CanWipeAccountsBalance()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.operatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).SetAmount(10).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanWipeAccountsNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                var serialsToTransfer = mintReceipt.serials.SubList(0, 4);
                var transfer = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
                {
                    transfer.AddNftTransfer(tokenId.Nft(serial), testEnv.operatorId, accountId);
                }

                transfer.Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).SetSerials(serialsToTransfer).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotWipeAccountsNftsIfNotOwned()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                var serialsToTransfer = mintReceipt.serials.SubList(0, 4);

                // don't transfer them
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).SetSerials(serialsToTransfer).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.ACCOUNT_DOES_NOT_OWN_WIPED_NFT.ToString());
            }
        }

        public virtual void CannotWipeAccountsBalanceWhenAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.operatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenWipeTransaction().SetTokenId(tokenId).SetAmount(10).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
            }
        }

        public virtual void CannotWipeAccountsBalanceWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.operatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenWipeTransaction().SetAccountId(accountId).SetAmount(10).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        public virtual void CanWipeAccountsBalanceWhenAmountIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddTokenTransfer(tokenId, testEnv.operatorId, -10).AddTokenTransfer(tokenId, accountId, 10).Execute(testEnv.client).GetReceipt(testEnv.client);
                var receipt = new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }
    }
}