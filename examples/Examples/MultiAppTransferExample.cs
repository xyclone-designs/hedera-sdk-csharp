// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Linq;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to transfer Hbar to an account with the receiver signature enabled.
    /// </summary>
    public class MultiAppTransferExample
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
            Console.WriteLine("MultiApp Transfer Example Start!");
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

            // The exchange should possess this key, we're only generating it for demonstration purposes.
            PrivateKey exchangePrivateKey = PrivateKey.GenerateED25519();
            PublicKey exchangePublicKey = exchangePrivateKey.GetPublicKey();

            // This is the only key we should actually possess.
            PrivateKey userPrivateKey = PrivateKey.GenerateED25519();
            PublicKey userPublicKey = userPrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create exchange and receiver accounts.
            /// </summary>
            Console.WriteLine("Creating exchange and receiver accounts...");

            // The exchange creates an account for the user to transfer funds to.
            AccountId exchangeAccountId = new AccountCreateTransaction { ReceiverSigRequired = true }
            .SetKeyWithoutAlias(exchangePublicKey)
            .FreezeWith(client)
            .Sign(exchangePrivateKey)
            .Execute(client)
            .GetReceipt(client).AccountId;

            // For the purpose of this example we create an account for the user with a balance of 5 Hbar.
            AccountId userAccountId = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(2),
            }
            .SetKeyWithoutAlias(userPublicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            Hbar senderBalanceBefore = new AccountBalanceQuery { AccountId = userAccountId }.Execute(client).Hbars;
            Hbar exchangeBalanceBefore = new AccountBalanceQuery { AccountId = exchangeAccountId }.Execute(client).Hbars;
            Console.WriteLine("User account (" + userAccountId + ") balance: " + senderBalanceBefore);
            Console.WriteLine("Exchange account (" + exchangeAccountId + ") balance: " + exchangeBalanceBefore);
            /// <summary>
            /// Step 3:
            /// Make a transfer from the user account to the exchange account, this requires signing by both parties.
            /// </summary>
            TransferTransaction transferTx = new TransferTransaction
            {
                TransactionMemo = "https://some-exchange.com/user1/account1"
            }
                .AddHbarTransfer(userAccountId, Hbar.From(1).Negated())
                .AddHbarTransfer(exchangeAccountId, Hbar.From(1))
            .FreezeWith(client)
            .Sign(userPrivateKey);

            // The exchange must sign the transaction in order for it to be accepted by the network
            // (assume this is some REST call to the exchange API server).
            byte[] signedTransferTxBytes = ITransaction.FromBytes(transferTx.ToBytes()).Sign(exchangePrivateKey).ToBytes();

            // Parse the transaction bytes returned from the exchange.
            TransferTransaction signedTransferTx = Transaction.FromBytes<TransferTransaction>(signedTransferTxBytes);

            // Get the amount we are about to transfer (we built this with +2, -2).
            Hbar transferAmount = signedTransferTx.GetHbarTransfers().Values.First();
            Console.WriteLine("Transferring " + transferAmount + " from the user account to the exchange account...");

            // We now execute the signed transaction and wait for it to be accepted.
            TransactionResponse transactionTxResponse = signedTransferTx.Execute(client);

            // (Important!) Wait for consensus by querying for the receipt.
            transactionTxResponse.GetReceipt(client);
            /// <summary>
            /// Step 4:
            /// Query user and exchange account balance to validate the transfer was successfully complete.
            /// </summary>
            Hbar senderBalanceAfter = new AccountBalanceQuery { AccountId = userAccountId }.Execute(client).Hbars;
            Hbar exchangeBalanceAfter = new AccountBalanceQuery { AccountId = exchangeAccountId }.Execute(client).Hbars;
            Console.WriteLine("User account (" + userAccountId + ") balance: " + senderBalanceAfter);
            Console.WriteLine("Exchange account (" + exchangeAccountId + ") balance: " + exchangeBalanceAfter);
            /// <summary>
            /// Clean up:
            /// Delete created accounts.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = exchangeAccountId,
                TransferAccountId = OPERATOR_ID
            }
            .FreezeWith(client)
            .Sign(exchangePrivateKey)
            .Execute(client)
            .GetReceipt(client);
            new AccountDeleteTransaction
            {
                AccountId = userAccountId,
                TransferAccountId = OPERATOR_ID
            }
            .FreezeWith(client)
            .Sign(userPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            client.Dispose();
            Console.WriteLine("MultiApp Transfer Example Complete!");
        }
    }
}