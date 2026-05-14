// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class TransferTokensExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Dotenv.Load()["OPERATOR_ID"]);
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Dotenv.Load()["OPERATOR_KEY"]);
        private static readonly string HEDERA_NETWORK = Dotenv.Load().Get("HEDERA_NETWORK", "testnet");
        private static readonly string SDK_LOG_LEVEL = Dotenv.Load().Get("SDK_LOG_LEVEL", "SILENT");
        public static void Main(string[] args)
        {
            Console.WriteLine("Transfer Tokens Example Start!");
            /// <summary>
            /// Step 0:
            /// Create and configure the SDK Client.
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK, _client =>
            {
                // All generated transactions will be paid by this account and signed by this key.
                _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
                // Attach logger to the SDK Client.
                //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL));
            });
            PublicKey operatorPublicKey = OPERATOR_KEY.GetPublicKey();
            /// <summary>
            /// Step 1:
            /// Generate ED25519 key pairs.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pairs for accounts...");
            PrivateKey alicePrivateKey = PrivateKey.GenerateED25519();
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
            PrivateKey bobPrivateKey = PrivateKey.GenerateED25519();
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create two new accounts.
            /// </summary>
            Console.WriteLine("Creating accounts...");
            Hbar initialBalance = Hbar.From(1);
            TransactionResponse aliceAccountCreateTxResponse = new AccountCreateTransaction { KeyWithoutAlias = alicePublicKey, InitialBalance = initialBalance }.Execute(client);

            // This will wait for the receipt to become available.
            TransactionReceipt aliceAccountCreateTxReceipt = aliceAccountCreateTxResponse.GetReceipt(client);
            AccountId aliceAccountId = aliceAccountCreateTxReceipt.AccountId;
            aliceAccountId;
            Console.WriteLine("Created Alice's account with ID: " + aliceAccountId);
            TransactionResponse bobAccountCreateTxResponse = new AccountCreateTransaction { KeyWithoutAlias = bobPublicKey, InitialBalance = initialBalance }.Execute(client);

            // This will wait for the receipt to become available.
            TransactionReceipt bobAccountCreateTxReceipt = bobAccountCreateTxResponse.GetReceipt(client);
            AccountId bobAccountId = bobAccountCreateTxReceipt.AccountId;
            bobAccountId;
            Console.WriteLine("Created Bob's account with ID: " + bobAccountId);
            /// <summary>
            /// Step 3:
            /// Create a Fungible Token.
            /// </summary>
            Console.WriteLine("Creating Fungible Token...");
            TransactionResponse tokenCreateTxResponse = new TokenCreateTransaction().SetNodeAccountIds([bobAccountCreateTxResponse.nodeId]).SetTokenName("Example Fungible Token for Transfer demo").SetTokenSymbol("EFT").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(OPERATOR_ID).SetAdminKey(operatorPublicKey).SetFreezeKey(operatorPublicKey).SetWipeKey(operatorPublicKey).SetKycKey(operatorPublicKey).SetSupplyKey(operatorPublicKey).SetFreezeDefault(false).Execute(client);
            TokenId tokenId = tokenCreateTxResponse.GetReceipt(client).TokenId;
            tokenId;
            Console.WriteLine("Created Fungible Token with ID: " + tokenId);
            /// <summary>
            /// Step 4:
            /// Associate the token with created accounts.
            /// </summary>
            Console.WriteLine("Associating the token with created accounts...");
            new TokenAssociateTransaction().SetNodeAccountIds([tokenCreateTxResponse.nodeId]).SetAccountId(aliceAccountId).SetTokenIds([tokenId]).FreezeWith(client).Sign(OPERATOR_KEY).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            Console.WriteLine("Associated account " + aliceAccountId + " with token " + tokenId);
            new TokenAssociateTransaction().SetNodeAccountIds([tokenCreateTxResponse.nodeId]).SetAccountId(bobAccountId).SetTokenIds([tokenId]).FreezeWith(client).Sign(OPERATOR_KEY).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            Console.WriteLine("Associated account " + bobAccountId + " with token " + tokenId);
            /// <summary>
            /// Step 5:
            /// Grant token KYC for created accounts.
            /// </summary>
            Console.WriteLine("Granting token KYC for created accounts...");
            new TokenGrantKycTransaction { NodeAccountIds = [tokenCreateTxResponse.nodeId], AccountId = aliceAccountId, TokenId = tokenId }.Execute(client).GetReceipt(client);
            Console.WriteLine("Granted KYC for account " + aliceAccountId + " on token " + tokenId);
            new TokenGrantKycTransaction { NodeAccountIds = [tokenCreateTxResponse.nodeId], AccountId = bobAccountId, TokenId = tokenId }.Execute(client).GetReceipt(client);
            Console.WriteLine("Granted KYC for account " + bobAccountId + " on token " + tokenId);
            /// <summary>
            /// Step 6:
            /// Transfer tokens from the operator (treasury) to Alice's account.
            /// </summary>
            Console.WriteLine("Transferring tokens from operator's (treasury) account to the `accountId1`...");
            new TransferTransaction().SetNodeAccountIds([tokenCreateTxResponse.nodeId]).AddTokenTransfer(tokenId, OPERATOR_ID, -10).AddTokenTransfer(tokenId, aliceAccountId, 10).Execute(client).GetReceipt(client);
            Console.WriteLine("Sent 10 tokens from account " + OPERATOR_ID + " to account " + aliceAccountId + " on token " + tokenId);
            /// <summary>
            /// Step 6:
            /// Transfer 10 tokens from the Alice to Bob.
            /// </summary>
            Console.WriteLine("Transferring tokens from the `accountId1` to the `accountId2`...");
            new TransferTransaction().SetNodeAccountIds([tokenCreateTxResponse.nodeId]).AddTokenTransfer(tokenId, aliceAccountId, -10).AddTokenTransfer(tokenId, bobAccountId, 10).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            Console.WriteLine("Sent 10 tokens from account " + aliceAccountId + " to account " + bobAccountId + " on token " + tokenId);
            /// <summary>
            /// Step 6:
            /// Transfer 10 tokens from Bob to Alice.
            /// </summary>
            Console.WriteLine("Transferring tokens from the `accountId2` to the `accountId1`...");
            new TransferTransaction().SetNodeAccountIds([tokenCreateTxResponse.nodeId]).AddTokenTransfer(tokenId, bobAccountId, -10).AddTokenTransfer(tokenId, aliceAccountId, 10).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            Console.WriteLine("Sent 10 tokens from account " + bobAccountId + " to account " + aliceAccountId + " on token " + tokenId);
            /// <summary>
            /// Clean up:
            /// Delete created accounts and tokens.
            /// </summary>
            new TokenWipeTransaction().SetNodeAccountIds([tokenCreateTxResponse.nodeId]).SetTokenId(tokenId).SetAccountId(aliceAccountId).SetAmount(10).Execute(client).GetReceipt(client);
            new TokenDeleteTransaction { NodeAccountIds = [tokenCreateTxResponse.nodeId], TokenId = tokenId }.Execute(client).GetReceipt(client);
            new AccountDeleteTransaction().SetAccountId(aliceAccountId).SetTransferAccountId(OPERATOR_ID).FreezeWith(client).Sign(OPERATOR_KEY).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction().SetAccountId(bobAccountId).SetTransferAccountId(OPERATOR_ID).FreezeWith(client).Sign(OPERATOR_KEY).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Example complete!");
        }
    }
}