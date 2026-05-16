// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to exempt token creators all of their token’s fee collectors from a custom fee (HIP-573).
    /// </summary>
    public class ExemptCustomFeesExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Environment.GetEnvironmentVariable("OPERATOR_KEY"));
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "testnet";
        private static readonly string SDK_LOG_LEVEL = Environment.GetEnvironmentVariable("SDK_LOG_LEVEL") ?? "SILENT";
        public static void Main(string[] args)
        {
            Console.WriteLine("Exempt Custom Fees Example Start!");
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
            var operatorPublicKey = OPERATOR_KEY.GetPublicKey();
            /// <summary>
            /// Step 1:
            /// Create three accounts: Alice, Bob, and Charlie.
            /// </summary>
            Console.WriteLine("Creating new accounts...");
            Hbar initialBalance = Hbar.From(1);
            PrivateKey alicePrivateKey = PrivateKey.GenerateED25519();
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
            AccountId aliceAccountId = new AccountCreateTransaction { InitialBalance = initialBalance }.SetKeyWithoutAlias(alicePublicKey).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client).AccountId;
            
            PrivateKey bobPrivateKey = PrivateKey.GenerateED25519();
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            AccountId bobAccountId = new AccountCreateTransaction { InitialBalance = initialBalance }.SetKeyWithoutAlias(bobPublicKey).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client).AccountId;

            PrivateKey charilePrivateKey = PrivateKey.GenerateED25519();
            PublicKey charilePublicKey = charilePrivateKey.GetPublicKey();
            AccountId charlieAccountId = new AccountCreateTransaction { InitialBalance = initialBalance }.SetKeyWithoutAlias(charilePublicKey).FreezeWith(client).Sign(charilePrivateKey).Execute(client).GetReceipt(client).AccountId;

            /// <summary>
            /// Step 2:
            /// Create a fungible token that has three fractional fees:
            /// - aliceFee sends 1/100 of the transferred value to Alice's account;
            /// - bobFee sends 2/100 of the transferred value to Bob's account;
            /// - charlieFee sends 3/100 of the transferred value to Charlie's account.
            /// </summary>
            ///

            CustomFractionalFee aliceFee = new () { FeeCollectorAccountId = aliceAccountId, Numerator = 1, Denominator = 100, AllCollectorsAreExempt = true };
            CustomFractionalFee bobFee = new () { FeeCollectorAccountId = bobAccountId, Numerator = 2, Denominator = 100, AllCollectorsAreExempt = true };
            CustomFractionalFee charlieFee = new () { FeeCollectorAccountId = charlieAccountId, Numerator = 3, Denominator = 100, AllCollectorsAreExempt = true };
            Console.WriteLine("Creating new Fungible Token using the Hedera Token Service...");
            TokenId fungibleTokenId = new TokenCreateTransaction
            {
                TokenName = "HIP-573 Fungible Token",
                TokenSymbol = "HIP573FT",
                TokenType = TokenType.FungibleCommon,
                TreasuryAccountId = OPERATOR_ID,
                AutoRenewAccountId = OPERATOR_ID,
                AdminKey = operatorPublicKey,
                FreezeKey = operatorPublicKey,
                WipeKey = operatorPublicKey,
                InitialSupply = 100000000,
                Decimals = 2,
                CustomFees = [aliceFee, bobFee, charlieFee],
            }
            .FreezeWith(client)
            .Sign(alicePrivateKey)
            .Sign(bobPrivateKey)
            .Sign(charilePrivateKey)
            .Execute(client)
            .GetReceipt(client).TokenId;

            Console.WriteLine("Created new fungible token with ID: " + fungibleTokenId);
            /// <summary>
            /// Step 3:
            /// Transfer tokens:
            /// - 10_000 units of the Fungible Token from the operator's to Bob's account;
            /// - 10_000 units of the Fungible Token from Bob's to Alice's account.
            /// </summary>
            Console.WriteLine("Transferring 10_000 units of the Fungible Token from the operator's to Bob's account...");
            new TransferTransaction().AddTokenTransfer(fungibleTokenId, OPERATOR_ID, -10000).AddTokenTransfer(fungibleTokenId, bobAccountId, 10000).FreezeWith(client).Sign(OPERATOR_KEY).Execute(client);
            Console.WriteLine("Transferring 10_000 units of the Fungible Token from Bob's to Alice's account...");
            TransactionResponse transferTxResponse = new TransferTransaction().AddTokenTransfer(fungibleTokenId, bobAccountId, -10000).AddTokenTransfer(fungibleTokenId, aliceAccountId, 10000).FreezeWith(client).Sign(bobPrivateKey).Execute(client);
            /// <summary>
            /// Step 4:
            /// Get the transaction fee for that transfer transaction.
            /// </summary>
            Hbar transactionFee = transferTxResponse.GetRecord(client).TransactionFee;
            Console.WriteLine("Transaction fee for the transfer above: " + transactionFee);
            /// <summary>
            /// Step 5:
            /// Show that the fee collector accounts in the custom fee list
            /// of the token that was created was not charged a custom fee in the transfer.
            /// </summary>
            ulong aliceAccountBalanceAfter = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).Tokens[fungibleTokenId];
            ulong bobAccountBalanceAfter = new AccountBalanceQuery { AccountId = bobAccountId }.Execute(client).Tokens[fungibleTokenId];
            ulong charlieAccountBalanceAfter = new AccountBalanceQuery { AccountId = charlieAccountId }.Execute(client).Tokens[fungibleTokenId];
            Console.WriteLine("Alice's balance after transferring the fungible token: " + aliceAccountBalanceAfter);
            Console.WriteLine("Bob's account balance after transferring the fungible token: " + bobAccountBalanceAfter);
            Console.WriteLine("Charlie's account balance after transferring the fungible token: " + charlieAccountBalanceAfter);
            /// <summary>
            /// Clean up:
            /// Delete created accounts and token.
            /// </summary>
            Dictionary<TokenId, ulong> alicesTokens = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).Tokens;
            new TokenWipeTransaction
            {
                TokenId = fungibleTokenId,
                Amount = alicesTokens[fungibleTokenId],
                AccountId = aliceAccountId,

            }.FreezeWith(client).Sign(OPERATOR_KEY).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction
            {
                AccountId = aliceAccountId,
                TransferAccountId = OPERATOR_ID 
            
            }.FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction
            {
                AccountId = bobAccountId,
                TransferAccountId = OPERATOR_ID 
            
            }.FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction
            {
                AccountId = charlieAccountId,
                TransferAccountId = OPERATOR_ID 
            
            }.FreezeWith(client).Sign(charilePrivateKey).Execute(client).GetReceipt(client);
            new TokenDeleteTransaction { TokenId = fungibleTokenId }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Exempt Custom Fees Example Complete!");
        }
    }
}