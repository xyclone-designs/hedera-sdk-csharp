// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class ZeroTokenOperationsExample
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
            Console.WriteLine("Zero Token Operations Example Start!");
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
                _client.DefaultMaxTransactionFee = Hbar.From(10);
            });
            
            /// <summary>
            /// Step 1:
            /// Generate an ED25519 key pair.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pair...");
            PrivateKey alicePrivateKey = PrivateKey.GenerateED25519();
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
           
            /// <summary>
            /// Step 2:
            /// Create a new account for the contract to interact with in some of its steps.
            /// </summary>
            Console.WriteLine("Creating Alice account...");
            AccountCreateTransaction accountCreateTx = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(1)
            }
            .SetKeyWithoutAlias(alicePublicKey)
            .FreezeWith(client);
            accountCreateTx = accountCreateTx.SignWithOperator(client);
            TransactionResponse accountCreateTxResponse = accountCreateTx.Execute(client);
            AccountId aliceAccountId = accountCreateTxResponse.GetReceipt(client).AccountId;

            Console.WriteLine("Created Alice's account with ID: " + aliceAccountId);
            
            /// <summary>
            /// Step 3:
            /// Instantiate ContractHelper.
            /// </summary>
            Console.WriteLine("Instantiating `ContractHelper`...");
            ContractHelper contractHelper = new (
                "contracts/precompile/ZeroTokenOperations.json",
                new ContractFunctionParameters()
                    .AddAddress(OPERATOR_ID.ToEvmAddress())
                    .AddAddress(aliceAccountId.ToEvmAddress()), 
                client);
            
            /// <summary>
            /// Step 4:
            /// Configure steps in ContractHelper.
            /// </summary>
            Console.WriteLine("Configuring steps in `ContractHelper`...");
            contractHelper
                .SetPayableAmountForStep(0, Hbar.From(20))
                .AddSignerForStep(1, alicePrivateKey);

            /// <summary>
            /// Step 5:
            /// Execute steps in ContractHelper.
            /// - step 0 creates a fungible token;
            /// - step 1 Associate with account;
            /// - step 2 transfer the token by passing a zero value;
            /// - step 3 mint the token by passing a zero value;
            /// - step 4 burn the token by passing a zero value;
            /// - step 5 wipe the token by passing a zero value.
            /// </summary>
            Console.WriteLine("Executing steps in `ContractHelper`.");

            // Update the signer to have contractId KeyList (this is by security requirement)
            new AccountUpdateTransaction
            {
                AccountId = OPERATOR_ID,
                Key = KeyList.Of(1, OPERATOR_KEY.GetPublicKey(), contractHelper.ContractId),
            }
            .Execute(client)
            .GetReceipt(client);

            // Update the Alice account to have contractId KeyList (this is by security requirement)
            new AccountUpdateTransaction
            {
                AccountId = aliceAccountId,
                Key = KeyList.Of(1, alicePublicKey, contractHelper.ContractId)
            }
            .FreezeWith(client)
            .Sign(alicePrivateKey)
            .Execute(client)
            .GetReceipt(client);

            // Configure steps in ContractHelper
            contractHelper
                .SetPayableAmountForStep(0, Hbar.From(40))
                .AddSignerForStep(1, alicePrivateKey);

            // step 0 creates a fungible token
            // step 1 Associate with account
            // step 2 transfer the token by passing a zero value
            // step 3 mint the token by passing a zero value
            // step 4 burn the token by passing a zero value
            // step 5 wipe the token by passing a zero value
            contractHelper.ExecuteSteps(0, 5, client);
            /// <summary>
            /// Step 6:
            /// Create and execute a transfer transaction with a zero value.
            /// </summary>
            Console.WriteLine("Creating a Fungible Token...");
            TokenCreateTransaction tokenCreateTx = new TokenCreateTransaction
            {
                TokenName = "Zero Token Ops Fungible Token",
                TokenSymbol = "ZTOFT",
                TreasuryAccountId = OPERATOR_ID,
                InitialSupply = 10000,
                Decimals = 2,
                AutoRenewAccountId = OPERATOR_ID,

            }.FreezeWith(client); tokenCreateTx = tokenCreateTx.SignWithOperator(client);
            TransactionResponse tokenCreateTxResponse = tokenCreateTx.Execute(client);
            TokenId fungibleTokenId = tokenCreateTxResponse.GetReceipt(client).TokenId;
            Console.WriteLine("Created Fungible Token with ID: " + fungibleTokenId);

            // Associate Token with Account.
            // Accounts on hedera have to opt in to receive any types of token that aren't Hbar.
            Console.WriteLine("Associate Token with Alice's account...");
            TokenAssociateTransaction tokenAssociateTx = new TokenAssociateTransaction
            {
                AccountId = aliceAccountId,
                TokenIds = [fungibleTokenId]
            
            }.FreezeWith(client);
            TokenAssociateTransaction tokenAssociateTxSigned = tokenAssociateTx.Sign(alicePrivateKey);
            TransactionResponse tokenAssociateTxResponse = tokenAssociateTxSigned.Execute(client);
            TransactionReceipt tokenAssociateTxReceipt = tokenAssociateTxResponse.GetReceipt(client);
            Console.WriteLine("Alice association transaction was complete with status: " + tokenAssociateTxReceipt.Status);

            // Transfer token.
            Console.WriteLine("Transferring zero tokens from operator's account to Alice's account...");
            TransferTransaction transferTx = new TransferTransaction()
                .AddTokenTransfer(fungibleTokenId, OPERATOR_ID, 0)
                .AddTokenTransfer(fungibleTokenId, aliceAccountId, 0)
                .FreezeWith(client);
            TransferTransaction transferTxSigned = transferTx.SignWithOperator(client);
            TransactionResponse transferTxResponse = transferTxSigned.Execute(client);

            // Verify the transaction reached consensus.
            TransactionRecord transferTxRecord = transferTxResponse.GetRecord(client);
            Console.WriteLine("step 6 completed, and returned valid result. TransactionId: " + transferTxRecord.TransactionId);
            Console.WriteLine("All steps completed with valid results.");
            
            /// <summary>
            /// Clean up:
            /// Delete created account and contract.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = aliceAccountId,
                TransferAccountId = OPERATOR_ID, 
            }
            .FreezeWith(client)
            .Sign(alicePrivateKey)
            .Execute(client)
            .GetReceipt(client);

            client.Dispose();

            Console.WriteLine("Zero Token Operations Example Complete!");
        }
    }
}