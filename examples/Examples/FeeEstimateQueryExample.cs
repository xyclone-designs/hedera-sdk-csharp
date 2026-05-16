// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class FeeEstimateQueryExample
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
            Console.WriteLine("Fee Estimate Example Start!");
            Client client = CreateAndConfigureClient();
            AccountId recipientId = AccountId.FromString("0.0.3");
            TransferTransaction tx = CreateTransferTransaction(client, recipientId);
            FeeEstimateResponse stateEstimate = EstimateWithStateMode(client, tx);
            FeeEstimateResponse intrinsicEstimate = EstimateWithIntrinsicMode(client, tx);
            CompareEstimates(stateEstimate, intrinsicEstimate);
            DemonstrateTokenCreationEstimate(client);
            client.Dispose();
            Console.WriteLine("\nExample complete!");
        }

        private static Client CreateAndConfigureClient()
        {
            Client client = ClientHelper.ForName(HEDERA_NETWORK);
            if ("localhost".Equals(HEDERA_NETWORK))
            {
                client.MirrorNetwork_.Network = ["127.0.0.1:8084"];
            }

            client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
            //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL)));
            return client;
        }

        private static TransferTransaction CreateTransferTransaction(Client client, AccountId recipientId)
        {
            Console.WriteLine("\n=== Creating Transfer Transaction ===");
            Hbar transferAmount = Hbar.From(1);
            TransferTransaction tx = new TransferTransaction { TransactionMemo = "Fee estimate example" }
                .AddHbarTransfer(OPERATOR_ID, transferAmount.Negated())
                .AddHbarTransfer(recipientId, transferAmount)
                .FreezeWith(client);
            tx.SignWithOperator(client);
            Console.WriteLine("Transaction created: Transfer " + transferAmount + " from " + OPERATOR_ID + " to " + recipientId);
            return tx;
        }

        private static FeeEstimateResponse EstimateWithStateMode(Client client, TransferTransaction tx)
        {
            Console.WriteLine("\n=== Estimating Fees with STATE Mode ===");
            FeeEstimateResponse stateEstimate = new FeeEstimateQuery
            {
                Mode = FeeEstimateMode.State,
                Transaction = tx

            }.Execute(client);
            PrintNetworkFee(stateEstimate);
            PrintNodeFee(stateEstimate);
            PrintServiceFee(stateEstimate);
            PrintTotalFee(stateEstimate);
            Console.WriteLine("\nHigh Volume Multiplier: " + stateEstimate.HighVolumeMultiplier); 
            return stateEstimate;
        }

        private static void PrintNetworkFee(FeeEstimateResponse estimate)
        {
            Console.WriteLine("\nNetwork Fee:");
            Console.WriteLine("  Multiplier: " + estimate.NetworkFee.Multiplier);
            Console.WriteLine("  Subtotal: " + estimate.NetworkFee.Subtotal + " tinycents");
        }

        private static void PrintNodeFee(FeeEstimateResponse estimate)
        {
            Console.WriteLine("\nNode Fee:");
            Console.WriteLine("  Base: " + estimate.NodeFee.Base + " tinycents");
            long nodeTotal = estimate.NodeFee.Base;
            foreach (FeeExtra extra in estimate.NodeFee.Extras)
            {
                Console.WriteLine("  Extra - " + extra.Name + ": " + extra.Subtotal + " tinycents");
                nodeTotal += extra.Subtotal;
            }

            Console.WriteLine("  Node Total: " + nodeTotal + " tinycents");
        }

        private static void PrintServiceFee(FeeEstimateResponse estimate)
        {
            Console.WriteLine("\nService Fee:");
            Console.WriteLine("  Base: " + estimate.ServiceFee.Base + " tinycents");
            long serviceTotal = estimate.ServiceFee.Base;
            foreach (FeeExtra extra in estimate.ServiceFee.Extras)
            {
                Console.WriteLine("  Extra - " + extra.Name + ": " + extra.Subtotal + " tinycents");
                serviceTotal += extra.Subtotal;
            }

            Console.WriteLine("  Service Total: " + serviceTotal + " tinycents");
        }

        private static void PrintTotalFee(FeeEstimateResponse estimate)
        {
            Console.WriteLine("\nTotal Estimated Fee: " + estimate.Total + " tinycents");
            Console.WriteLine("Total Estimated Fee: " + Hbar.FromTinybars(estimate.Total / 100));
        }

        private static FeeEstimateResponse EstimateWithIntrinsicMode(Client client, TransferTransaction tx)
        {
            Console.WriteLine("\n=== Estimating Fees with INTRINSIC Mode ===");
            FeeEstimateResponse intrinsicEstimate = new FeeEstimateQuery
            {
                Mode = FeeEstimateMode.Intrinsic,
                Transaction = tx.ToProtobuf()

            }.Execute(client);
            Console.WriteLine("Network Fee Subtotal: " + intrinsicEstimate.NetworkFee.Subtotal + " tinycents");
            Console.WriteLine("Node Fee Base: " + intrinsicEstimate.NodeFee.Base + " tinycents");
            Console.WriteLine("Service Fee Base: " + intrinsicEstimate.ServiceFee.Base + " tinycents");
            Console.WriteLine("Total Estimated Fee: " + intrinsicEstimate.Total + " tinycents");
            Console.WriteLine("Total Estimated Fee: " + Hbar.FromTinybars(intrinsicEstimate.Total / 100));
            return intrinsicEstimate;
        }

        private static void CompareEstimates(FeeEstimateResponse stateEstimate, FeeEstimateResponse intrinsicEstimate)
        {
            Console.WriteLine("\n=== Comparison ===");
            Console.WriteLine("STATE mode total:  " + stateEstimate.Total + " tinycents");
            Console.WriteLine("INTRINSIC mode total: " + intrinsicEstimate.Total + " tinycents");
            long difference = Math.Abs(stateEstimate.Total - intrinsicEstimate.Total);
            Console.WriteLine("Difference: " + difference + " tinycents");
        }

        private static void DemonstrateTokenCreationEstimate(Client client)
        {
            Console.WriteLine("\n=== Estimating Token Creation Fees ===");
            TokenCreateTransaction tokenTx = new TokenCreateTransaction
            {
                TokenName = "Example Token",
                TokenSymbol = "EXT",
                Decimals = 3,
                InitialSupply = 1000000,
                TreasuryAccountId = OPERATOR_ID,
                AdminKey = OPERATOR_KEY,
            }
            .FreezeWith(client)
            .SignWithOperator(client);
            FeeEstimateResponse tokenEstimate = new FeeEstimateQuery
            {
                Mode = FeeEstimateMode.State,
                Transaction = tokenTx

            }.Execute(client);
            Console.WriteLine("Token Creation Estimated Fee:  " + tokenEstimate.Total + " tinycents");
            Console.WriteLine("Token Creation Estimated Fee: " + Hbar.FromTinybars(tokenEstimate.Total / 100));
        }
    }
}