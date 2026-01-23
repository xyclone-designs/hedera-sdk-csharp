// SPDX-License-Identifier: Apache-2.0
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Execute an Ethereum transaction on Hedera
    /// </summary>
    [Obsolete("Obsolete")]
    public class EthereumFlow
    {
        /// <summary>
        /// 128,000 bytes - jumbo transaction limit
        /// Indicates when we should splice out the call data from an ethereum transaction data
        /// </summary>
        static int MAX_ETHEREUM_DATA_SIZE = 128000;
        private EthereumTransactionData ethereumData;
        private FileId callDataFileId;
        private Hbar maxGasAllowance;
        /// <summary>
        /// Constructor
        /// </summary>
        public EthereumFlow()
        {
        }

        private static FileId CreateFile(byte[] callData, Client client, Duration timeoutPerTransaction, Transaction<TWildcardTodo> ethereumTransaction)
        {
            try
            {

                // Hex encode the call data
                byte[] callDataHex = Hex.Encode(callData);
                var transaction = new FileCreateTransaction().SetKeys(Objects.RequireNonNull(client.GetOperatorPublicKey())).SetContents(Array.CopyOfRange(callDataHex, 0, Math.Min(FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length))).Execute(client, timeoutPerTransaction);
                var fileId = transaction.GetReceipt(client, timeoutPerTransaction).fileId;
                var nodeId = transaction.nodeId;
                if (callDataHex.Length > FileAppendTransaction.DEFAULT_CHUNK_SIZE)
                {
                    new FileAppendTransaction().SetFileId(fileId).SetMaxChunks(1000).SetNodeAccountIds(Collections.SingletonList(nodeId)).SetContents(Array.CopyOfRange(callDataHex, FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length)).Execute(client, timeoutPerTransaction).GetReceipt(client);
                }

                ethereumTransaction.SetNodeAccountIds(Collections.SingletonList(nodeId));
                return fileId;
            }
            catch (ReceiptStatusException e)
            {
                throw new Exception(e);
            }
        }

        private static CompletableFuture<FileId> CreateFileAsync(byte[] callData, Client client, Duration timeoutPerTransaction, Transaction<TWildcardTodo> ethereumTransaction)
        {

            // Hex encode the call data
            byte[] callDataHex = Hex.Encode(callData);
            return new FileCreateTransaction().SetKeys(Objects.RequireNonNull(client.GetOperatorPublicKey())).SetContents(Array.CopyOfRange(callDataHex, 0, Math.Min(FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length))).ExecuteAsync(client, timeoutPerTransaction).ThenCompose((response) =>
            {
                var nodeId = response.nodeId;
                ethereumTransaction.SetNodeAccountIds(Collections.SingletonList(nodeId));
                return response.GetReceiptAsync(client, timeoutPerTransaction).ThenCompose((receipt) =>
                {
                    if (callDataHex.Length > FileAppendTransaction.DEFAULT_CHUNK_SIZE)
                    {
                        return new FileAppendTransaction().SetFileId(receipt.fileId).SetNodeAccountIds(Collections.SingletonList(nodeId)).SetMaxChunks(1000).SetContents(Array.CopyOfRange(callDataHex, FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length)).ExecuteAsync(client, timeoutPerTransaction).ThenCompose((appendResponse) => appendResponse.GetReceiptAsync(client, timeoutPerTransaction)).ThenApply((r) => receipt.fileId);
                    }
                    else
                    {
                        return CompletableFuture.CompletedFuture(receipt.fileId);
                    }
                });
            });
        }

        /// <summary>
        /// Gets the data of the Ethereum transaction
        /// </summary>
        /// <returns>the data of the Ethereum transaction</returns>
        public virtual EthereumTransactionData GetEthereumData()
        {
            return ethereumData;
        }

        /// <summary>
        /// Sets the raw Ethereum transaction (RLP encoded type 0, 1, and 2). Complete
        /// unless the callDataFileId is set.
        /// </summary>
        /// <param name="ethereumData">raw ethereum transaction bytes</param>
        /// <returns>{@code this}</returns>
        public virtual EthereumFlow SetEthereumData(byte[] ethereumData)
        {
            ethereumData = EthereumTransactionData.FromBytes(ethereumData);
            return this;
        }

        /// <summary>
        /// Gets the maximum amount that the payer of the hedera transaction
        /// is willing to pay to complete the transaction.
        /// </summary>
        /// <returns>the max gas allowance</returns>
        public virtual Hbar GetMaxGasAllowance()
        {
            return maxGasAllowance;
        }

        /// <summary>
        /// Sets the maximum amount that the payer of the hedera transaction
        /// is willing to pay to complete the transaction.
        /// <br>
        /// Ordinarily the account with the ECDSA alias corresponding to the public
        /// key that is extracted from the ethereum_data signature is responsible for
        /// fees that result from the execution of the transaction. If that amount of
        /// authorized fees is not sufficient then the payer of the transaction can be
        /// charged, up to but not exceeding this amount. If the ethereum_data
        /// transaction authorized an amount that was insufficient then the payer will
        /// only be charged the amount needed to make up the difference. If the gas
        /// price in the transaction was set to zero then the payer will be assessed
        /// the entire fee.
        /// </summary>
        /// <param name="maxGasAllowance">the maximum gas allowance</param>
        /// <returns>{@code this}</returns>
        public virtual EthereumFlow SetMaxGasAllowance(Hbar maxGasAllowance)
        {
            maxGasAllowance = maxGasAllowance;
            return this;
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>the response</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public virtual TransactionResponse Execute(Client client)
        {
            return Execute(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <returns>the response</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public virtual TransactionResponse Execute(Client client, Duration timeoutPerTransaction)
        {
            if (ethereumData == null)
            {
                throw new InvalidOperationException("Cannot execute a ethereum flow when ethereum data was not provided");
            }

            var ethereumTransaction = new EthereumTransaction();
            var ethereumDataBytes = ethereumData.ToBytes();
            if (maxGasAllowance != null)
            {
                ethereumTransaction.SetMaxGasAllowanceHbar(maxGasAllowance);
            }

            if (ethereumDataBytes.Length <= MAX_ETHEREUM_DATA_SIZE)
            {
                ethereumTransaction.SetEthereumData(ethereumDataBytes);
            }
            else
            {
                var callDataFileId = CreateFile(ethereumData.callData, client, timeoutPerTransaction, ethereumTransaction);
                ethereumData.callData = new byte[]
                {
                };
                ethereumTransaction.SetEthereumData(ethereumData.ToBytes()).SetCallDataFileId(callDataFileId);
            }

            return ethereumTransaction.Execute(client, timeoutPerTransaction);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
        /// because it uses features introduced in API level 31 (Android 12).</p>*
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>the response</returns>
        public virtual CompletableFuture<TransactionResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
        /// because it uses features introduced in API level 31 (Android 12).</p>*
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <returns>the response</returns>
        public virtual CompletableFuture<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerTransaction)
        {
            if (ethereumData == null)
            {
                return CompletableFuture.FailedFuture(new InvalidOperationException("Cannot execute a ethereum flow when ethereum data was not provided"));
            }

            var ethereumTransaction = new EthereumTransaction();
            var ethereumDataBytes = ethereumData.ToBytes();
            if (maxGasAllowance != null)
            {
                ethereumTransaction.SetMaxGasAllowanceHbar(maxGasAllowance);
            }

            if (ethereumDataBytes.Length <= MAX_ETHEREUM_DATA_SIZE)
            {
                return ethereumTransaction.SetEthereumData(ethereumDataBytes).ExecuteAsync(client);
            }
            else
            {
                return CreateFileAsync(ethereumData.callData, client, timeoutPerTransaction, ethereumTransaction).ThenCompose((callDataFileId) => ethereumTransaction.SetEthereumData(ethereumData.ToBytes()).SetCallDataFileId(callDataFileId).ExecuteAsync(client, timeoutPerTransaction));
            }
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="callback">a BiConsumer which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, BiConsumer<TransactionResponse, Throwable> callback)
        {
            ConsumerHelper.BiConsumer(ExecuteAsync(client), callback);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="callback">a BiConsumer which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, BiConsumer<TransactionResponse, Throwable> callback)
        {
            ConsumerHelper.BiConsumer(ExecuteAsync(client, timeoutPerTransaction), callback);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="onSuccess">a Consumer which consumes the result on success.</param>
        /// <param name="onFailure">a Consumer which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Consumer<TransactionResponse> onSuccess, Consumer<Throwable> onFailure)
        {
            ConsumerHelper.TwoConsumers(ExecuteAsync(client), onSuccess, onFailure);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Consumer which consumes the result on success.</param>
        /// <param name="onFailure">a Consumer which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, Consumer<TransactionResponse> onSuccess, Consumer<Throwable> onFailure)
        {
            ConsumerHelper.TwoConsumers(ExecuteAsync(client, timeoutPerTransaction), onSuccess, onFailure);
        }
    }
}