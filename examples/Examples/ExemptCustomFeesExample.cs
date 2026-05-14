// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

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
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Dotenv.Load()["OPERATOR_ID"]);
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Dotenv.Load()["OPERATOR_KEY"]);
        private static readonly string HEDERA_NETWORK = Dotenv.Load().Get("HEDERA_NETWORK", "testnet");
        private static readonly string SDK_LOG_LEVEL = Dotenv.Load().Get("SDK_LOG_LEVEL", "SILENT");
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
            AccountId aliceAccountId = new AccountCreateTransaction().SetInitialBalance(initialBalance).SetKeyWithoutAlias(alicePublicKey).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client).AccountId;
            aliceAccountId;
            PrivateKey bobPrivateKey = PrivateKey.GenerateED25519();
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            AccountId bobAccountId = new AccountCreateTransaction().SetInitialBalance(initialBalance).SetKeyWithoutAlias(bobPublicKey).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client).AccountId;
            bobAccountId;
            PrivateKey charilePrivateKey = PrivateKey.GenerateED25519();
            PublicKey charilePublicKey = charilePrivateKey.GetPublicKey();
            AccountId charlieAccountId = new AccountCreateTransaction().SetInitialBalance(initialBalance).SetKeyWithoutAlias(charilePublicKey).FreezeWith(client).Sign(charilePrivateKey).Execute(client).GetReceipt(client).AccountId;
            charlieAccountId;
            /// <summary>
            /// Step 2:
            /// Create a fungible token that has three fractional fees:
            /// - aliceFee sends 1/100 of the transferred value to Alice's account;
            /// - bobFee sends 2/100 of the transferred value to Bob's account;
            /// - charlieFee sends 3/100 of the transferred value to Charlie's account.
            /// </summary>
            CustomFractionalFee aliceFee = new CustomFractionalFee().SetFeeCollectorAccountId(aliceAccountId).SetNumerator(1).SetDenominator(100).SetAllCollectorsAreExempt(true);
            CustomFractionalFee bobFee = new CustomFractionalFee().SetFeeCollectorAccountId(bobAccountId).SetNumerator(2).SetDenominator(100).SetAllCollectorsAreExempt(true);
            CustomFractionalFee charlieFee = new CustomFractionalFee().SetFeeCollectorAccountId(charlieAccountId).SetNumerator(3).SetDenominator(100).SetAllCollectorsAreExempt(true);
            Console.WriteLine("Creating new Fungible Token using the Hedera Token Service...");
            TokenId fungibleTokenId = new TokenCreateTransaction().SetTokenName("HIP-573 Fungible Token").SetTokenSymbol("HIP573FT").SetTokenType(TokenType.FUNGIBLE_COMMON).SetTreasuryAccountId(OPERATOR_ID).SetAutoRenewAccountId(OPERATOR_ID).SetAdminKey(operatorPublicKey).SetFreezeKey(operatorPublicKey).SetWipeKey(operatorPublicKey).SetInitialSupply(100000000).SetDecimals(2).SetCustomFees(List.Of(aliceFee, bobFee, charlieFee)).FreezeWith(client).Sign(alicePrivateKey).Sign(bobPrivateKey).Sign(charilePrivateKey).Execute(client).GetReceipt(client).TokenId;
            fungibleTokenId;
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
            Hbar transactionFee = transferTxResponse.GetRecord(client).transactionFee;
            Console.WriteLine("Transaction fee for the transfer above: " + transactionFee);
            /// <summary>
            /// Step 5:
            /// Show that the fee collector accounts in the custom fee list
            /// of the token that was created was not charged a custom fee in the transfer.
            /// </summary>
            long aliceAccountBalanceAfter = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).tokens[fungibleTokenId];
            long bobAccountBalanceAfter = new AccountBalanceQuery { AccountId = bobAccountId }.Execute(client).tokens[fungibleTokenId];
            long charlieAccountBalanceAfter = new AccountBalanceQuery { AccountId = charlieAccountId }.Execute(client).tokens[fungibleTokenId];
            Console.WriteLine("Alice's balance after transferring the fungible token: " + aliceAccountBalanceAfter);
            Console.WriteLine("Bob's account balance after transferring the fungible token: " + bobAccountBalanceAfter);
            Console.WriteLine("Charlie's account balance after transferring the fungible token: " + charlieAccountBalanceAfter);
            /// <summary>
            /// Clean up:
            /// Delete created accounts and token.
            /// </summary>
            Dictionary<TokenId, long> alicesTokens = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).tokens;
            new TokenWipeTransaction().SetTokenId(fungibleTokenId).SetAmount(alicesTokens[fungibleTokenId]).SetAccountId(aliceAccountId).FreezeWith(client).Sign(OPERATOR_KEY).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction().SetAccountId(aliceAccountId).SetTransferAccountId(OPERATOR_ID).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction().SetAccountId(bobAccountId).SetTransferAccountId(OPERATOR_ID).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction().SetAccountId(charlieAccountId).SetTransferAccountId(OPERATOR_ID).FreezeWith(client).Sign(charilePrivateKey).Execute(client).GetReceipt(client);
            new TokenDeleteTransaction { TokenId = fungibleTokenId }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Exempt Custom Fees Example Complete!");
        }
    }
}