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
    class TokenNftTransferIntegrationTest
    {
        virtual void CanTransferNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                TransactionResponse response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = response.GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = response.GetReceipt(testEnv.client).tokenId;
                AssertThat(tokenId).IsNotNull();
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).SignWithOperator(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenGrantKycTransaction().SetAccountId(accountId).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                var serialsToTransfer = new List<long>(mintReceipt.serials.SubList(0, 4));
                var transfer = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
                {
                    transfer.AddNftTransfer(tokenId.Nft(serial), testEnv.operatorId, accountId);
                }

                transfer.Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenWipeTransaction().SetTokenId(tokenId).SetAccountId(accountId).SetSerials(serialsToTransfer).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CannotTransferUnownedNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                TransactionResponse response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = response.GetReceipt(testEnv.client).accountId;
                AssertThat(accountId).IsNotNull();
                response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = response.GetReceipt(testEnv.client).tokenId;
                AssertThat(tokenId).IsNotNull();
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TokenAssociateTransaction().SetAccountId(accountId).SetTokenIds(Collections.SingletonList(tokenId)).FreezeWith(testEnv.client).SignWithOperator(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                var serialsToTransfer = new List<long>(mintReceipt.serials.SubList(0, 4));
                var transfer = new TransferTransaction();
                foreach (var serial in serialsToTransfer)
                {

                    // Try to transfer in wrong direction
                    transfer.AddNftTransfer(tokenId.Nft(serial), accountId, testEnv.operatorId);
                }

                transfer.FreezeWith(testEnv.client).Sign(key);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    transfer.Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.SENDER_DOES_NOT_OWN_NFT_SERIAL_NO.ToString());
            }
        }
    }
}