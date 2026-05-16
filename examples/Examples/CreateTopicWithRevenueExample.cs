// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class CreateTopicWithRevenueExample
    {
        /// <summary>
        /// Operator's account ID used to sign and pay for transactions on Hedera.
        /// </summary>
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
        /// <summary>
        /// Operator's private key for signing transactions.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Environment.GetEnvironmentVariable("OPERATOR_KEY"));
        /// <summary>
        /// Hedera network (localhost, testnet, previewnet, or mainnet).
        /// </summary>
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "testnet";
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Hedera Custom Fees Example...");

            // Step 0: Initialize client and set the operator.
            try
            {
                using (Client client = ClientHelper.ForName(HEDERA_NETWORK, _client => _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY)))
                {
                    /// <summary>
                    /// Step 1: Create an account for Alice with an initial balance of 5 HBAR.
                    /// </summary>
                    Console.WriteLine("Creating Alice's account...");
                    PrivateKey aliceKey = PrivateKey.GenerateECDSA();
                    var aliceAccountId = new AccountCreateTransaction
                    {
                        MaxAutomaticTokenAssociations = 1,
                        InitialBalance = Hbar.From(2)
                    }
                    .SetKeyWithoutAlias(aliceKey)
                    .Execute(client)
                    .GetReceipt(client).AccountId;

                    Console.WriteLine("Alice's Account ID: " + aliceAccountId);

                    /// <summary>
                    /// Step 2: Create a topic with an HBAR custom fee.
                    /// </summary>
                    Console.WriteLine("Creating a topic with HBAR custom fee...");
                    var customFee = new CustomFixedFee
                    {
                        Amount = new Hbar(1).ToTinybars(),
                        FeeCollectorAccountId = OPERATOR_ID
                    };
                    var topicId = new TopicCreateTransaction 
                    { 
                        AdminKey = OPERATOR_KEY, 
                        FeeScheduleKey = OPERATOR_KEY, 
                        CustomFees = [customFee] 
                    
                    }.Execute(client).GetReceipt(client).TopicId;
                    Console.WriteLine("Created Topic ID: " + topicId);

                    /// <summary>
                    /// Step 3: Submit a message to the topic, paid by Alice, with a custom fee limit.
                    /// </summary>
                    Console.WriteLine("Submitting a message as Alice to the topic...");
                    var aliceBalanceBefore = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).Hbars;
                    var feeCollectorBalanceBefore = new AccountBalanceQuery { AccountId = OPERATOR_ID }.Execute(client).Hbars;
                    var customFeeLimit = new CustomFeeLimit
                    {
                        PayerId = aliceAccountId,
                        CustomFees = [new CustomFixedFee { Amount = Hbar.From(2).ToTinybars() }]
                    };
                    client.OperatorSet(aliceAccountId, aliceKey);
                    new TopicMessageSubmitTransaction
                    {
                        CustomFeeLimits = [customFeeLimit],
                        TopicId = topicId,
                        Message = ByteString.CopyFromUtf8("Hello, Hedera™ hashgraph!"),
                    }
                    .Execute(client)
                    .GetReceipt(client);
                    Console.WriteLine("Message submitted successfully.");

                    /// <summary>
                    /// Step 4: Verify Alice's and fee collector's balance after the transaction.
                    /// </summary>
                    client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
                    var aliceBalanceAfter = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).Hbars;
                    var feeCollectorBalanceAfter = new AccountBalanceQuery { AccountId = OPERATOR_ID }.Execute(client).Hbars;
                    Console.WriteLine("Alice's balance before: " + aliceBalanceBefore + ", after: " + aliceBalanceAfter);
                    Console.WriteLine("Fee collector's balance before: " + feeCollectorBalanceBefore + ", after: " + feeCollectorBalanceAfter);

                    /// <summary>
                    /// Step 5: Create a fungible token and transfer it to Alice.
                    /// </summary>
                    Console.WriteLine("Creating a token and transferring it to Alice...");
                    var tokenId = new TokenCreateTransaction
                    {
                        TokenName = "revenue-generating token",
                        TokenSymbol = "RGT",
                        TreasuryAccountId = client.OperatorAccountId,
                        Decimals = 8,
                        InitialSupply = 100,
                    }
                    .Execute(client)
                    .GetReceipt(client).TokenId;
                    new TransferTransaction().AddTokenTransfer(tokenId, client.OperatorAccountId, -1).AddTokenTransfer(tokenId, aliceAccountId, 1)
                    .Execute(client)
                    .GetReceipt(client);

                    /// <summary>
                    /// Step 6: Update the topic to charge a token-based fee.
                    /// </summary>
                    Console.WriteLine("Updating the topic to charge a token-based fee...");
                    var customFeeToken = new CustomFixedFee
                    {
                        Amount = 1,
                        FeeCollectorAccountId = OPERATOR_ID,
                        // TODO DenominatingTokenId = tokenId,
                    };
                    new TopicUpdateTransaction
                    {
                        TopicId = topicId,
                        CustomFees = { customFeeToken },
                    }                        
                    .Execute(client)
                    .GetReceipt(client);

                    /// <summary>
                    /// Step 7: Submit another message without specifying a custom fee limit.
                    /// </summary>
                    Console.WriteLine("Submitting another message without custom fee limit...");
                    client.OperatorSet(aliceAccountId, aliceKey);
                    new TopicMessageSubmitTransaction { TopicId = topicId, Message = ByteString.CopyFromUtf8("Another message!") }.Execute(client).GetReceipt(client);
                    client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);

                    /// <summary>
                    /// Step 8: Verify Alice's token balance and the fee collector's token balance after the transaction.
                    /// </summary>
                    var aliceTokenBalanceAfter = new AccountBalanceQuery { AccountId = aliceAccountId }.Execute(client).Tokens[tokenId];
                    var feeCollectorTokenBalanceAfter = new AccountBalanceQuery { AccountId = OPERATOR_ID }.Execute(client).Tokens[tokenId];
                    Console.WriteLine("Alice's token balance: " + aliceTokenBalanceAfter);
                    Console.WriteLine("Fee collector's token balance: " + feeCollectorTokenBalanceAfter);

                    /// <summary>
                    /// Step 9: Create Bob's account with 10 HBAR.
                    /// </summary>
                    Console.WriteLine("Creating Bob's account...");
                    Hbar initialBalance = new (10);
                    PrivateKey bobKey = PrivateKey.GenerateECDSA();
                    var bobAccountId = new AccountCreateTransaction { Key = bobKey, InitialBalance = initialBalance, MaxAutomaticTokenAssociations = 100 }.Execute(client).GetReceipt(client).AccountId;
                    Console.WriteLine("Bob's Account ID: " + bobAccountId);

                    /// <summary>
                    /// Step 10: Exempt Bob from paying topic fees.
                    /// </summary>
                    Console.WriteLine("Updating topic to add Bob as a fee-exempt key...");
                    new TopicUpdateTransaction { TopicId = topicId, FeeExemptKeys = { bobKey } }.Execute(client).GetReceipt(client);

                    /// <summary>
                    /// Step 11: Bob submits a message to the topic without paying the fee.
                    /// </summary>
                    client.OperatorSet(bobAccountId, bobKey);
                    new TopicMessageSubmitTransaction { TopicId = topicId, Message = ByteString.CopyFromUtf8("Hello from Bob!") }.Execute(client).GetReceipt(client);
                    Console.WriteLine("Message submitted successfully by Bob without being charged.");

                    /// <summary>
                    /// Step 12: Verify Bob's balance should be almost the same as the initial
                    /// </summary>
                    var bobBalanceAfter = new AccountBalanceQuery { AccountId = bobAccountId }.Execute(client).Hbars;
                    Console.WriteLine("Bob's initial balance: " + initialBalance + ", after: " + bobBalanceAfter);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
            finally
            {
                Console.WriteLine("Example execution completed.");
            }
        }
    }
}