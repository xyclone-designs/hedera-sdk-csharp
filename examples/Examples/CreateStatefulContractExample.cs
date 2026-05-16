// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;

using System;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to create a stateful smart contract and call its function.
    /// </summary>
    public class CreateStatefulContractExample
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
            Console.WriteLine("Create Stateful Contract Example Start!");
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
                // Default max fee for all transactions executed by this client.
                _client.DefaultMaxTransactionFee = Hbar.From(100);
                _client.DefaultMaxQueryPayment = Hbar.From(10);
            });

            var operatorPublicKey = OPERATOR_KEY.GetPublicKey();
            /// <summary>
            /// Step 1:
            /// Create a file with smart contract bytecode.
            /// </summary>
            Console.WriteLine("Creating new bytecode file...");
            string contractBytecodeHex = ContractHelper.GetBytecodeHex("contracts/stateful/stateful.json");
            TransactionResponse fileCreateTxResponse = new FileCreateTransaction 
            { 
                Keys = [operatorPublicKey], 
                Contents = Encoding.UTF8.GetBytes(contractBytecodeHex)
            
            }.Execute(client);
            TransactionReceipt fileCreateTxReceipt = fileCreateTxResponse.GetReceipt(client);
            FileId newFileId = fileCreateTxReceipt.FileId;

            Console.WriteLine("Created new bytecode file with ID: " + newFileId);
            /// <summary>
            /// Step 2:
            /// Create a smart contract.
            /// </summary>
            TransactionResponse contractCreateTxResponse = new ContractCreateTransaction
            {
                Gas = 350000,
                BytecodeFileId = newFileId,
                AdminKey = operatorPublicKey,
                ConstructorParameters = new ContractFunctionParameters().AddString("Hello from Hedera!").ToBytes(null),

            }.Execute(client);
            TransactionReceipt contractCreateTxReceipt = contractCreateTxResponse.GetReceipt(client);
            ContractId newContractId = contractCreateTxReceipt.ContractId;

            Console.WriteLine("Created new contract with ID: " + newContractId);
            /// <summary>
            /// Step 3:
            /// Call smart contract function.
            /// </summary>
            Console.WriteLine("Calling contract function \"get_message\"...");
            ContractFunctionResult contractCallResult_BeforeSetMessage = new ContractCallQuery
            {
                ContractId = newContractId,
                Gas = 300000,
                MaxQueryPayment = Hbar.From(1),

            }.SetFunction("get_message").Execute(client);

            if (contractCallResult_BeforeSetMessage.ErrorMessage != null)
            {
                throw new Exception("Error calling contract function \"get_message\": " + contractCallResult_BeforeSetMessage.ErrorMessage);
            }

            string contractCallResult_BeforeSetMessage_String = contractCallResult_BeforeSetMessage.GetString(0);
            Console.WriteLine("Contract call result (\"get_message\" function returned): " + contractCallResult_BeforeSetMessage_String);
            Console.WriteLine("Calling contract function \"set_message\"...");
            TransactionResponse contractExecuteTxResponse = new ContractExecuteTransaction
            {
                ContractId = newContractId,
                Gas = 300000,
            }
            .SetFunction("set_message", new ContractFunctionParameters().AddString("Hello from hedera again!"))
            .Execute(client);

            // If this doesn't throw then we know the contract executed successfully.
            contractExecuteTxResponse.GetReceipt(client);
            /// <summary>
            /// Step 3:
            /// Call smart contract function.
            /// </summary>
            Console.WriteLine("Calling contract function \"get_message\"...");
            ContractFunctionResult contractCallResult_AfterSetMessage = new ContractCallQuery
            {
                Gas = 300000,
                ContractId = newContractId,
                MaxQueryPayment = Hbar.From(1),

            }.SetFunction("get_message").Execute(client);
            if (contractCallResult_AfterSetMessage.ErrorMessage != null)
            {
                throw new Exception("Error calling contract function \"get_message\": " + contractCallResult_AfterSetMessage.ErrorMessage);
            }

            string contractCallResult_AfterSetMessage_String = contractCallResult_AfterSetMessage.GetString(0);
            Console.WriteLine("Contract call result (\"get_message\" function returned): " + contractCallResult_AfterSetMessage_String);
            /// <summary>
            /// Clean up:
            /// Delete created contract.
            /// </summary>
            new ContractDeleteTransaction
            {
                ContractId = newContractId,
                TransferAccountId = contractCreateTxResponse.TransactionId.AccountId,
                MaxTransactionFee = Hbar.From(1),
            }
            .Execute(client)
            .GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Create Stateful Contract Example Complete!");
        }
    }
}