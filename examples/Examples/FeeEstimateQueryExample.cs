// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
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
                client.SetMirrorNetwork(List.Of("127.0.0.1:8084"));
            }

            _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
            //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL)));
            return client;
        }

        private static TransferTransaction CreateTransferTransaction(Client client, AccountId recipientId)
        {
            Console.WriteLine("\n=== Creating Transfer Transaction ===");
            Hbar transferAmount = Hbar.From(1);
            TransferTransaction tx = new TransferTransaction().AddHbarTransfer(OPERATOR_ID, transferAmount.Negated()).AddHbarTransfer(recipientId, transferAmount).SetTransactionMemo("Fee estimate example").FreezeWith(client);
            tx.SignWithOperator(client);
            Console.WriteLine("Transaction created: Transfer " + transferAmount + " from " + OPERATOR_ID + " to " + recipientId);
            return tx;
        }

        private static FeeEstimateResponse EstimateWithStateMode(Client client, TransferTransaction tx)
        {
            Console.WriteLine("\n=== Estimating Fees with STATE Mode ===");
            FeeEstimateResponse stateEstimate = new FeeEstimateQuery().SetMode(FeeEstimateMode.STATE).SetTransaction(tx).Execute(client);
            PrintNetworkFee(stateEstimate);
            PrintNodeFee(stateEstimate);
            PrintServiceFee(stateEstimate);
            PrintTotalFee(stateEstimate);
            Console.WriteLine("\nHigh Volume Multiplier: " + stateEstimate.GetHighVolumeMultiplier());
            return stateEstimate;
        }

        private static void PrintNetworkFee(FeeEstimateResponse estimate)
        {
            Console.WriteLine("\nNetwork Fee:");
            Console.WriteLine("  Multiplier: " + estimate.GetNetwork().GetMultiplier());
            Console.WriteLine("  Subtotal: " + estimate.GetNetwork().GetSubtotal() + " tinycents");
        }

        private static void PrintNodeFee(FeeEstimateResponse estimate)
        {
            Console.WriteLine("\nNode Fee:");
            Console.WriteLine("  Base: " + estimate.GetNode().GetBase() + " tinycents");
            long nodeTotal = estimate.GetNode().GetBase();
            foreach (FeeExtra extra in estimate.GetNode().GetExtras())
            {
                Console.WriteLine("  Extra - " + extra.GetName() + ": " + extra.GetSubtotal() + " tinycents");
                nodeTotal += extra.GetSubtotal();
            }

            Console.WriteLine("  Node Total: " + nodeTotal + " tinycents");
        }

        private static void PrintServiceFee(FeeEstimateResponse estimate)
        {
            Console.WriteLine("\nService Fee:");
            Console.WriteLine("  Base: " + estimate.GetService().GetBase() + " tinycents");
            long serviceTotal = estimate.GetService().GetBase();
            foreach (FeeExtra extra in estimate.GetService().GetExtras())
            {
                Console.WriteLine("  Extra - " + extra.GetName() + ": " + extra.GetSubtotal() + " tinycents");
                serviceTotal += extra.GetSubtotal();
            }

            Console.WriteLine("  Service Total: " + serviceTotal + " tinycents");
        }

        private static void PrintTotalFee(FeeEstimateResponse estimate)
        {
            Console.WriteLine("\nTotal Estimated Fee: " + estimate.GetTotal() + " tinycents");
            Console.WriteLine("Total Estimated Fee: " + Hbar.FromTinybars(estimate.GetTotal() / 100));
        }

        private static FeeEstimateResponse EstimateWithIntrinsicMode(Client client, TransferTransaction tx)
        {
            Console.WriteLine("\n=== Estimating Fees with INTRINSIC Mode ===");
            FeeEstimateResponse intrinsicEstimate = new FeeEstimateQuery().SetMode(FeeEstimateMode.INTRINSIC).SetTransaction(tx).Execute(client);
            Console.WriteLine("Network Fee Subtotal: " + intrinsicEstimate.GetNetwork().GetSubtotal() + " tinycents");
            Console.WriteLine("Node Fee Base: " + intrinsicEstimate.GetNode().GetBase() + " tinycents");
            Console.WriteLine("Service Fee Base: " + intrinsicEstimate.GetService().GetBase() + " tinycents");
            Console.WriteLine("Total Estimated Fee: " + intrinsicEstimate.GetTotal() + " tinycents");
            Console.WriteLine("Total Estimated Fee: " + Hbar.FromTinybars(intrinsicEstimate.GetTotal() / 100));
            return intrinsicEstimate;
        }

        private static void CompareEstimates(FeeEstimateResponse stateEstimate, FeeEstimateResponse intrinsicEstimate)
        {
            Console.WriteLine("\n=== Comparison ===");
            Console.WriteLine("STATE mode total:  " + stateEstimate.GetTotal() + " tinycents");
            Console.WriteLine("INTRINSIC mode total: " + intrinsicEstimate.GetTotal() + " tinycents");
            long difference = Math.Abs(stateEstimate.GetTotal() - intrinsicEstimate.GetTotal());
            Console.WriteLine("Difference: " + difference + " tinycents");
        }

        private static void DemonstrateTokenCreationEstimate(Client client)
        {
            Console.WriteLine("\n=== Estimating Token Creation Fees ===");
            TokenCreateTransaction tokenTx = new TokenCreateTransaction().SetTokenName("Example Token").SetTokenSymbol("EXT").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(OPERATOR_ID).SetAdminKey(OPERATOR_KEY).FreezeWith(client).SignWithOperator(client);
            FeeEstimateResponse tokenEstimate = new FeeEstimateQuery().SetMode(FeeEstimateMode.STATE).SetTransaction(tokenTx).Execute(client);
            Console.WriteLine("Token Creation Estimated Fee:  " + tokenEstimate.GetTotal() + " tinycents");
            Console.WriteLine("Token Creation Estimated Fee: " + Hbar.FromTinybars(tokenEstimate.GetTotal() / 100));
        }
    }
}