// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;

using System;
using System.Collections.Generic;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    public class ContractNoncesExample
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
            Console.WriteLine("Contract Nonces (HIP-729) Example Start!");
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
            Console.WriteLine("Creating new contract...");
            /// <summary>
            /// Step 1:
            /// Create a file with smart contract bytecode.
            /// </summary>
            string contractBytecodeHex = ContractHelper.GetBytecodeHex("contracts/parent_deploys_child/parent_deploys_child.json");
            TransactionResponse bytecodeFileCreateTxResponse = new FileCreateTransaction
            {
                Keys = [operatorPublicKey],
                Contents = Encoding.UTF8.GetBytes(contractBytecodeHex),
                MaxTransactionFee = Hbar.From(2),

            }.Execute(client);
            TransactionReceipt bytecodeFileCreateTxReceipt = bytecodeFileCreateTxResponse.GetReceipt(client);
            FileId bytecodeFileId = bytecodeFileCreateTxReceipt.FileId;

            /// <summary>
            /// Step 2:
            /// Create a smart contract.
            /// </summary>
            TransactionResponse contractCreateTxResponse = new ContractCreateTransaction
            {
                AdminKey = operatorPublicKey,
                Gas = 100000,
                BytecodeFileId = bytecodeFileId,
                ContractMemo = "HIP-729 Contract",

            }.Execute(client);
            TransactionReceipt contractCreateTxReceipt = contractCreateTxResponse.GetReceipt(client);
            ContractId contractId = contractCreateTxReceipt.ContractId;

            Console.WriteLine("Created new contract with ID: " + contractId);
            /// <summary>
            /// Step 3:
            /// Get a record from a contract create transaction to check contracts nonces.
            /// We expect to see `nonce=2` as we deploy a contract that creates another contract in its constructor.
            /// </summary>
            IList<ContractNonceInfo> contractNonces = contractCreateTxResponse.GetRecord(client).ContractFunctionResult.ContractNonces;
            Console.WriteLine("Contract nonces: " + contractNonces);
            /// <summary>
            /// Clean up:
            /// Delete created contract.
            /// </summary>
            new ContractDeleteTransaction
            {
                ContractId = contractId,
                TransferAccountId = contractCreateTxReceipt.TransactionId.AccountId,
                MaxTransactionFee = Hbar.From(1),
            }
            .Execute(client)
            .GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Contract Nonces (HIP-729) Example Complete!");
        }
    }
}