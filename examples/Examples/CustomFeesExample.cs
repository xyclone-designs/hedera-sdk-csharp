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
    /// How to set and receive custom fees.
    /// </summary>
    public class CustomFeesExample
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
            Console.WriteLine("Custom Fees Example Start!");
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
            /// <summary>
            /// Step 1:
            /// Create three accounts: Alice, Bob, and Charlie.
            ///
            /// Alice will be the treasury for our example token.
            /// Fees only apply in transactions not involving the treasury, so we need two other accounts.
            /// </summary>
            Console.WriteLine("Creating Alice's, Bob's and Charlie's accounts...");
            Hbar initialAccountBalance = Hbar.From(1);
            PrivateKey alicePrivateKey = PrivateKey.GenerateED25519();
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
            AccountId aliceAccountId = new AccountCreateTransaction().SetInitialBalance(initialAccountBalance).SetKeyWithoutAlias(alicePublicKey).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client).AccountId;
            aliceAccountId;
            PrivateKey bobPrivateKey = PrivateKey.GenerateED25519();
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            AccountId bobAccountId = new AccountCreateTransaction().SetInitialBalance(initialAccountBalance).SetKeyWithoutAlias(bobPublicKey).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client).AccountId;
            bobAccountId;
            PrivateKey charliePrivateKey = PrivateKey.GenerateED25519();
            PublicKey charliePublicKey = charliePrivateKey.GetPublicKey();
            AccountId charlieAccountId = new AccountCreateTransaction().SetInitialBalance(initialAccountBalance).SetKeyWithoutAlias(charliePublicKey).FreezeWith(client).Sign(charliePrivateKey).Execute(client).GetReceipt(client).AccountId;
            charlieAccountId;
            Console.WriteLine("Alice's account ID: " + aliceAccountId);
            Console.WriteLine("Bob's account ID: " + bobAccountId);
            Console.WriteLine("Charlie's account ID: " + charlieAccountId);
            /// <summary>
            /// Step 2:
            /// Create a custom fee list of 1 fixed fee.
            ///
            /// A custom fee list can be a list of up to 10 custom fees,
            /// where each fee is a fixed fee or a fractional fee.
            /// This fixed fee will mean that every time Bob transfers any number of tokens to Charlie,
            /// Alice will collect 1 Hbar from each account involved in the transaction who is SENDING
            /// the Token (in this case, Bob).
            ///
            /// In this example the fee is in Hbar, but you can charge a fixed fee in a token if you'd like.
            /// E.g., you can make it so that each time an account transfers Foo tokens,
            /// they must pay a fee in Bar tokens to the fee collecting account.
            /// To charge a fixed fee in tokens, instead of calling setHbarAmount(), call
            /// setDenominatingTokenId(tokenForFee) and setAmount(tokenFeeAmount).
            /// </summary>
            CustomFixedFee customHbarFee = new CustomFixedFee().SetHbarAmount(Hbar.From(1)).SetFeeCollectorAccountId(aliceAccountId);
            IList<CustomFee> hbarFeeList = [customHbarFee];
            /// <summary>
            /// Step 3:
            /// Create a fungible token.
            ///
            /// Setting the feeScheduleKey to Alice's key will enable Alice to change the custom
            /// fees list on this token later using the TokenFeeScheduleUpdateTransaction.
            /// We will create an initial supply of 100 of these tokens.
            /// </summary>
            Console.WriteLine("Creating new Fungible Token using the Hedera Token Service...");
            TokenId fungibleTokenId = new TokenCreateTransaction().SetTokenName("Custom Fees Example Fungible Token").SetTokenSymbol("CFEFT").SetAdminKey(alicePublicKey).SetSupplyKey(alicePublicKey).SetFeeScheduleKey(alicePublicKey).SetWipeKey(alicePublicKey).SetTreasuryAccountId(aliceAccountId).SetCustomFees(hbarFeeList).SetInitialSupply(100).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client).TokenId;
            fungibleTokenId;
            TokenInfo fungibleTokenInfo = new TokenInfoQuery { TokenId = fungibleTokenId }.Execute(client);
            Console.WriteLine("Created new fungible token with ID: " + fungibleTokenId + " and custom fees: " + fungibleTokenInfo.customFees);
            /// <summary>
            /// Step 4:
            /// Associate the token with Bob and Charlie before they can transfer and receive it.
            /// </summary>
            Console.WriteLine("Associate created fungible token with Bob's and Charlie's accounts...");
            new TokenAssociateTransaction().SetAccountId(bobAccountId).SetTokenIds([fungibleTokenId]).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            new TokenAssociateTransaction().SetAccountId(charlieAccountId).SetTokenIds([fungibleTokenId]).FreezeWith(client).Sign(charliePrivateKey).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 5:
            /// Transfer all 100 tokens to Bob.
            /// </summary>
            Console.WriteLine("Transferring all 100 tokens from Alice to Bob...");
            new TransferTransaction().AddTokenTransfer(fungibleTokenId, bobAccountId, 100).AddTokenTransfer(fungibleTokenId, aliceAccountId, -100).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 6:
            /// Check Alice's Hbar balance.
            /// </summary>
            Hbar aliceAccountBalanceHbars_BeforeCollectingFees = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).Hbars;
            if (aliceAccountBalanceHbars_BeforeCollectingFees.Equals(initialAccountBalance))
            {
                Console.WriteLine("Alice's Hbar balance before: " + aliceAccountBalanceHbars_BeforeCollectingFees);
            }
            else
            {
                throw new Exception("Alice's account initial balance was not set correctly! (Fail)");
            }

            /// <summary>
            /// Step 7:
            /// Transfer 20 tokens from Bob to Charlie.
            /// </summary>
            Console.WriteLine("Transferring 20 tokens from Bob to Charlie...");
            TransactionRecord transferTxRecord = new TransferTransaction().AddTokenTransfer(fungibleTokenId, bobAccountId, -20).AddTokenTransfer(fungibleTokenId, charlieAccountId, 20).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetRecord(client);
            /// <summary>
            /// Step 8:
            /// Check Alice's Hbar balance.
            ///
            /// It should increase, because of the fee taken from the transfer in the previous step.
            /// </summary>
            Hbar aliceAccountBalanceHbars_AfterCollectingFees = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).Hbars;
            if (aliceAccountBalanceHbars_AfterCollectingFees.Equals(Hbar.From(2)))
            {
                Console.WriteLine("Alice's Hbar balance after Bob transferred 20 tokens to Charlie: " + aliceAccountBalanceHbars_AfterCollectingFees);
            }
            else
            {
                throw new Exception("Custom fee was not set correctly! (Fail)");
            }

            Console.WriteLine("Assessed fees: " + transferTxRecord.assessedCustomFees);
            /// <summary>
            /// Step 9:
            /// Use the TokenUpdateFeeScheduleTransaction with Alice's key to change the custom fees on our token.
            ///
            /// TokenUpdateFeeScheduleTransaction will replace the list of fees that apply to the token with
            /// an entirely new list. Let's charge a 10% fractional fee. This means that when Bob attempts to transfer
            /// 20 tokens to Charlie, 10% of the tokens he attempts to transfer (2 in this case) will be transferred to
            /// Alice instead.
            ///
            /// Fractional fees default to FeeAssessmentMethod.INCLUSIVE, which is the behavior described above.
            /// If you set the assessment method to EXCLUSIVE, then when Bob attempts to transfer 20 tokens to Charlie,
            /// Charlie will receive all 20 tokens, and Bob will be charged an additional 10% fee which
            /// will be transferred to Alice.
            /// </summary>
            CustomFractionalFee customFractionalFee = new CustomFractionalFee().SetNumerator(1).SetDenominator(10).SetMin(1).SetMax(10).SetFeeCollectorAccountId(aliceAccountId);
            IList<CustomFee> fractionalFeeList = [customFractionalFee];
            Console.WriteLine("Updating the custom fees for a fungible token...");
            new TokenFeeScheduleUpdateTransaction().SetTokenId(fungibleTokenId).SetCustomFees(fractionalFeeList).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            TokenInfo tokenInfo2 = new TokenInfoQuery { TokenId = fungibleTokenId }.Execute(client);
            Console.WriteLine("Updated custom fees: " + tokenInfo2.customFees);
            /// <summary>
            /// Step 10:
            /// Check Alice's token balance.
            /// </summary>
            Dictionary<TokenId, long> aliceAccountBalanceTokens_BeforeCollectingFees = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).tokens;
            if (aliceAccountBalanceTokens_BeforeCollectingFees[fungibleTokenId] == 0)
            {
                Console.WriteLine("Alice's token balance before Bob transfers 20 tokens to Charlie: " + aliceAccountBalanceTokens_BeforeCollectingFees[fungibleTokenId]);
            }
            else
            {
                throw new Exception("Alice's account initial token balance is not zero! (Fail)");
            }

            /// <summary>
            /// Step 11:
            /// Transfer 20 tokens from Bob to Charlie.
            /// </summary>
            Console.WriteLine("Transferring 20 tokens from Bob to Charlie...");
            TransactionRecord transferTxRecord_2 = new TransferTransaction().AddTokenTransfer(fungibleTokenId, bobAccountId, -20).AddTokenTransfer(fungibleTokenId, charlieAccountId, 20).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetRecord(client);
            /// <summary>
            /// Step 12:
            /// Check Alice's token balance. It should increase, because of the fee taken from the
            /// transfer in the previous step.
            /// </summary>
            Dictionary<TokenId, long> aliceAccountBalanceTokens_AfterCollectingFees = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).tokens;
            if (aliceAccountBalanceTokens_AfterCollectingFees[fungibleTokenId] == 2)
            {
                Console.WriteLine("Alice's token balance after Bob transfers 20 tokens to Charlie: " + aliceAccountBalanceTokens_AfterCollectingFees[fungibleTokenId]);
            }
            else
            {
                throw new Exception("Custom fractional fee was not set correctly! (Fail)");
            }

            Console.WriteLine("Token transfers: " + transferTxRecord_2.tokenTransfers);
            Console.WriteLine("Assessed fees: " + transferTxRecord_2.assessedCustomFees);
            /// <summary>
            /// Clean up:
            /// Delete created accounts and tokens.
            /// </summary>

            // Move token to operator account.
            new TokenAssociateTransaction().SetAccountId(client.GetOperatorAccountId()).SetTokenIds([fungibleTokenId]).FreezeWith(client).Sign(OPERATOR_KEY).Execute(client).GetReceipt(client);
            new TokenUpdateTransaction().SetTokenId(fungibleTokenId).SetAdminKey(OPERATOR_KEY).SetSupplyKey(OPERATOR_KEY).SetFeeScheduleKey(OPERATOR_KEY).SetWipeKey(OPERATOR_KEY).SetTreasuryAccountId(client.GetOperatorAccountId()).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);

            // Wipe token on created accounts.
            Dictionary<TokenId, long> charlieTokensBeforeWipe = new AccountBalanceQuery { AccountId = charlieAccountId }.Execute(client).tokens;
            new TokenWipeTransaction().SetTokenId(fungibleTokenId).SetAmount(charlieTokensBeforeWipe[fungibleTokenId]).SetAccountId(charlieAccountId).FreezeWith(client).Sign(OPERATOR_KEY).Execute(client).GetReceipt(client);
            Dictionary<TokenId, long> bobsTokens = new AccountBalanceQuery { AccountId = bobAccountId }.Execute(client).tokens;
            new TokenWipeTransaction().SetTokenId(fungibleTokenId).SetAmount(bobsTokens[fungibleTokenId]).SetAccountId(bobAccountId).FreezeWith(client).Sign(OPERATOR_KEY).Execute(client).GetReceipt(client);
            Dictionary<TokenId, long> aliceTokensBeforeWipe = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).tokens;
            new TokenWipeTransaction().SetTokenId(fungibleTokenId).SetAmount(aliceTokensBeforeWipe[fungibleTokenId]).SetAccountId(aliceAccountId).FreezeWith(client).Sign(OPERATOR_KEY).Execute(client).GetReceipt(client);

            // Delete created accounts.
            new AccountDeleteTransaction().SetAccountId(charlieAccountId).SetTransferAccountId(client.GetOperatorAccountId()).FreezeWith(client).Sign(charliePrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction().SetAccountId(bobAccountId).SetTransferAccountId(client.GetOperatorAccountId()).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction().SetAccountId(aliceAccountId).SetTransferAccountId(client.GetOperatorAccountId()).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);

            // Delete created token.
            new TokenDeleteTransaction().SetTokenId(fungibleTokenId).FreezeWith(client).Sign(OPERATOR_KEY).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Custom Fees Example Complete!");
        }
    }
}