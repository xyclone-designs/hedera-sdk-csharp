// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class AccountAllowanceExample
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
            Console.WriteLine("Account Allowance Example Start!");
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
            /// Generate ED25519 key pairs.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pairs...");
            PrivateKey alicePrivateKey = PrivateKey.GenerateED25519();
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
            PrivateKey bobPrivateKey = PrivateKey.GenerateED25519();
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            PrivateKey charliePrivateKey = PrivateKey.GenerateED25519();
            PublicKey charliePublicKey = charliePrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create accounts for this example.
            /// </summary>
            Console.WriteLine("Creating Alice's, Bob's and Charlie's accounts...");
            AccountId aliceId = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(5)
            }
            .SetKeyWithoutAlias(alicePublicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            AccountId bobId = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(5)
            }
            .SetKeyWithoutAlias(bobPublicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            AccountId charlieId = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(5)
            }
            .SetKeyWithoutAlias(charliePublicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            Console.WriteLine("Alice's account ID: " + aliceId);
            Console.WriteLine("Bob's account ID: " + bobId);
            Console.WriteLine("Charlie's account ID: " + charlieId);
            Console.WriteLine("Alice's balance: " + new AccountBalanceQuery { AccountId = aliceId }.Execute(client).Hbars);
            Console.WriteLine("Bob's balance: " + new AccountBalanceQuery { AccountId = bobId }.Execute(client).Hbars);
            Console.WriteLine("Charlie's balance: " + new AccountBalanceQuery { AccountId = charlieId }.Execute(client).Hbars);
            /// <summary>
            /// Step 3:
            /// Approve an allowance of 2 Hbar with owner Alice and spender Bob.
            /// </summary>
            Console.WriteLine("Approving an allowance of 2 Hbar with owner Alice and spender Bob...");
            new AccountAllowanceApproveTransaction().ApproveHbarAllowance(aliceId, bobId, Hbar.From(2)).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            Console.WriteLine("Alice's balance: " + new AccountBalanceQuery { AccountId = aliceId }.Execute(client).Hbars);
            Console.WriteLine("Bob's balance: " + new AccountBalanceQuery { AccountId = bobId }.Execute(client).Hbars);
            Console.WriteLine("Charlie's balance: " + new AccountBalanceQuery { AccountId = charlieId }.Execute(client).Hbars);
            /// <summary>
            /// Step 4:
            /// Demonstrate allowance -- transfer 1 Hbar from Alice to Charlie, but the transaction is signed only by Bob
            /// (Bob is dipping into his allowance from Alice).
            /// </summary>
            Console.WriteLine("Transferring 1 Hbar from Alice to Charlie, " + "but the transaction is signed only by Bob (Bob is dipping into his allowance from Alice)...");
            new TransferTransaction 
            { 
                TransactionId = TransactionId.Generate(bobId) 
            }
                .AddApprovedHbarTransfer(aliceId, Hbar.From(1).Negated())
                .AddHbarTransfer(charlieId, Hbar.From(1))
                .FreezeWith(client)
                .Sign(bobPrivateKey)
                .Execute(client)
                .GetReceipt(client);
            Console.WriteLine("Transfer succeeded. Bob should now have 1 Hbar left in his allowance.");
            Console.WriteLine("Alice's balance: " + new AccountBalanceQuery { AccountId = aliceId }.Execute(client).Hbars);
            Console.WriteLine("Bob's balance: " + new AccountBalanceQuery { AccountId = bobId }.Execute(client).Hbars);
            Console.WriteLine("Charlie's balance: " + new AccountBalanceQuery { AccountId = charlieId }.Execute(client).Hbars);
            /// <summary>
            /// Step 5:
            /// Demonstrate the absence of an allowance -- attempt to transfer 2 Hbar from Alice to Charlie using Bob's allowance.
            ///
            /// This should fail, because there is only 1 Hbar left in Bob's allowance.
            /// </summary>
            try
            {
                Console.WriteLine("Attempting to transfer 2 Hbar from Alice to Charlie using Bob's allowance... " + "(this should fail, because there is only 1 Hbar left in Bob's allowance).");
                new TransferTransaction 
                { 
                    TransactionId = TransactionId.Generate(bobId) 
                }
                    .AddApprovedHbarTransfer(aliceId, Hbar.From(2).Negated())
                    .AddHbarTransfer(charlieId, Hbar.From(2))
                    .FreezeWith(client)
                    .Sign(bobPrivateKey)
                    .Execute(client)
                    .GetReceipt(client);
                throw new Exception("This transfer shouldn't have succeeded!");
            }
            catch (Exception error)
            {
                Console.WriteLine("This transfer failed as expected: " + error.Message);
            }

            /// <summary>
            /// Step 6:
            /// Demonstrate update of an allowance -- adjust Bob's allowance to 3 Hbar.
            /// </summary>
            Console.WriteLine("Adjusting Bob's allowance to 3 Hbar...");
            new AccountAllowanceApproveTransaction().ApproveHbarAllowance(aliceId, bobId, Hbar.From(3)).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 7:
            /// Demonstrate allowance -- transfer 2 Hbar from Alice to Charlie using Bob's allowance again.
            /// </summary>
            Console.WriteLine("Attempting to transfer 2 Hbar from Alice to Charlie using Bob's allowance again... " + "(this time it should succeed).");
            new TransferTransaction 
            { 
                TransactionId = TransactionId.Generate(bobId) 
            }
                .AddApprovedHbarTransfer(aliceId, Hbar.From(2).Negated())
                .AddHbarTransfer(charlieId, Hbar.From(2))
                .FreezeWith(client)
                .Sign(bobPrivateKey)
                .Execute(client)
                .GetReceipt(client);
            Console.WriteLine("Transfer succeeded.");
            Console.WriteLine("Alice's balance: " + new AccountBalanceQuery { AccountId = aliceId }.Execute(client).Hbars);
            Console.WriteLine("Bob's balance: " + new AccountBalanceQuery { AccountId = bobId }.Execute(client).Hbars);
            Console.WriteLine("Charlie's balance: " + new AccountBalanceQuery { AccountId = charlieId }.Execute(client).Hbars);
            /// <summary>
            /// Clean up:
            /// Delete allowance and created accounts.
            /// </summary>
            new AccountAllowanceApproveTransaction().ApproveHbarAllowance(aliceId, bobId, Hbar.ZERO).FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction 
            {
                AccountId = aliceId, 
                TransferAccountId = OPERATOR_ID 
            
            }.FreezeWith(client).Sign(alicePrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction 
            {
                AccountId = bobId, 
                TransferAccountId = OPERATOR_ID 
            
            }.FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            new AccountDeleteTransaction 
            {
                AccountId = charlieId, 
                TransferAccountId = OPERATOR_ID 
            
            }.FreezeWith(client).Sign(charliePrivateKey).Execute(client).GetReceipt(client);
            
            client.Dispose();

            Console.WriteLine("Account Allowance Example Complete!");
        }
    }
}