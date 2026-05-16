// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to transfer Hbar or tokens to a Hedera account using their public-address (HIP-583).
    /// </summary>
    public class TransferUsingEvmAddressExample
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
            Console.WriteLine("Transfer Using Evm Address Example Start!");
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
            /// Create an ECSDA private key.
            /// </summary>
            PrivateKey alicePrivateKey = PrivateKey.GenerateECDSA();
            /// <summary>
            /// Step 2:
            /// Extract the ECDSA public key.
            /// </summary>
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
            /// <summary>
            /// Step 3:
            /// Extract the Ethereum public address.
            /// </summary>
            EvmAddress aliceEvmAddress = alicePublicKey.ToEvmAddress();
            Console.WriteLine("EVM address of Alice's account: " + aliceEvmAddress);
            /// <summary>
            /// Step 4:
            /// Transfer tokens using the TransferTransaction to the Ethereum Account Address.
            /// - the from field should be a complete account that has a public address;
            /// - the to field should be to a public address (to create a new account).
            /// </summary>
            Console.WriteLine("Transferring Hbar to Alice's account...");
            TransferTransaction transferTx = new TransferTransaction().AddHbarTransfer(OPERATOR_ID, Hbar.From(1).Negated()).AddHbarTransfer(aliceEvmAddress, Hbar.From(1)).FreezeWith(client);
            TransferTransaction transferTxSigned = transferTx.Sign(OPERATOR_KEY);
            TransactionResponse transferTxResponse = transferTxSigned.Execute(client);
            /// <summary>
            /// Step 5:
            /// Get the child receipt or child record to return the Hedera Account ID for the new account that was created.
            /// </summary>
            TransactionReceipt transferTxReceipt = new TransactionReceiptQuery
            {
                TransactionId = transferTxResponse.TransactionId,
                IncludeChildren = true

            }.Execute(client);
            AccountId aliceAccountId = transferTxReceipt.Children[0].AccountId;
            Console.WriteLine("The \"normal\" account ID of the given alias: " + aliceAccountId);
            /// <summary>
            /// Step 6:
            /// Get the AccountInfo on the new account and show it is a hollow account by not having a public key.
            /// </summary>
            AccountInfo aliceAccountInfo_BeforeEnhancing = new AccountInfoQuery { AccountId = aliceAccountId }.Execute(client);
            Console.WriteLine("Alice's account info: " + aliceAccountInfo_BeforeEnhancing);
            /// <summary>
            /// Step 7:
            /// Use the hollow account as a transaction fee payer in a HAPI transaction.
            /// </summary>
            Console.WriteLine("Setting new account as client's operator...");
            client.OperatorSet(aliceAccountId, alicePrivateKey);
            PrivateKey bobPrivateKey = PrivateKey.GenerateED25519();
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            Console.WriteLine("Creating Bob's account...");
            AccountCreateTransaction accountCreateTx = new AccountCreateTransaction().SetKeyWithoutAlias(bobPublicKey).FreezeWith(client);
            /// <summary>
            /// Step 8:
            /// Sign the transaction with ECDSA private key.
            /// </summary>
            AccountCreateTransaction accountCreateTxSigned = accountCreateTx.Sign(alicePrivateKey);
            TransactionResponse accountCreateTxResponse = accountCreateTxSigned.Execute(client);
            TransactionReceipt accountCreateTxReceipt = accountCreateTxResponse.GetReceipt(client);
            var bobAccountId = accountCreateTxReceipt.AccountId;
            Console.WriteLine("Created Bob's account with ID: " + bobAccountId);
            /// <summary>
            /// Step 9:
            /// Get the AccountInfo of the account and show the account is now a complete account
            /// by returning the public key on the account.
            /// </summary>
            AccountInfo aliceAccountInfo_AfterEnhancing = new AccountInfoQuery { AccountId = aliceAccountId }.Execute(client);
            Console.WriteLine("The public key of the newly created (and now complete) account: " + aliceAccountInfo_AfterEnhancing.Key);
            /// <summary>
            /// Clean up:
            /// Delete created accounts.
            /// </summary>
            client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
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
            client.Dispose();
            Console.WriteLine("Transfer Using Evm Address Example Complete!");
        }
    }
}