// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Transactions.Ethereum;
using Hedera.Hashgraph.SDK.Transactions.File;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private FileId callDataFileId;
        
        private static FileId CreateFile(byte[] callData, Client client, Duration timeoutPerTransaction, Transaction<T> ethereumTransaction)
        {
            try
            {

                // Hex encode the call data
                byte[] callDataHex = Hex.Encode(callData);
                FileCreateTransaction transaction = new()
                {
					Keys = client.GetOperatorPublicKey(),
				};

				var transaction = new FileCreateTransaction()
                    .SetContents(Array.CopyOfRange(callDataHex, 0, Math.Min(FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length)))
                    .Execute(client, timeoutPerTransaction);
                var fileId = transaction.GetReceipt(client, timeoutPerTransaction).fileId;
                var nodeId = transaction.nodeId;
                if (callDataHex.Length > FileAppendTransaction.DEFAULT_CHUNK_SIZE)
                {
                    new FileAppendTransaction()
                        .SetFileId(fileId)
                        .SetMaxChunks(1000)
                        .SetNodeAccountIds(Collections.SingletonList(nodeId))
                        .SetContents(Array.CopyOfRange(callDataHex, FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length))
                        .Execute(client, timeoutPerTransaction).GetReceipt(client);
                }

                ethereumTransaction.SetNodeAccountIds([nodeId]);
                return fileId;
            }
            catch (ReceiptStatusException e)
            {
                throw new Exception(string.Empty, e);
            }
        }

        private static Task<FileId> CreateFileAsync(byte[] callData, Client client, Duration timeoutPerTransaction, Transaction<T> ethereumTransaction)
        {

            // Hex encode the call data
            byte[] callDataHex = Hex.Encode(callData);
            return new FileCreateTransaction()
                .SetKeys(ArgumentNullException.ThrowIfNull(client.GetOperatorPublicKey()))
                .SetContents(Array.CopyOfRange(callDataHex, 0, Math.Min(FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length)))
                .ExecuteAsync(client, timeoutPerTransaction).ThenCompose((response) =>
                {
                    var nodeId = response.nodeId;
                    ethereumTransaction.SetNodeAccountIds(Collections.SingletonList(nodeId));
                    return response.ReceiptAsync(client, timeoutPerTransaction).ThenCompose((receipt) =>
                    {
                        if (callDataHex.Length > FileAppendTransaction.DEFAULT_CHUNK_SIZE)
                        {
                            return new FileAppendTransaction()
                                .SetFileId(receipt.fileId)
                                .SetNodeAccountIds(Collections.SingletonList(nodeId))
                                .SetMaxChunks(1000)
                                .SetContents(Array.CopyOfRange(callDataHex, FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length))
                                .ExecuteAsync(client, timeoutPerTransaction).ThenCompose((appendResponse) => appendResponse.GetReceiptAsync(client, timeoutPerTransaction)).ThenApply((r) => receipt.fileId);
                        }
                        else
                        {
                            return Task.FromResult(receipt.fileId);
                        }
                    });
                });
        }

        /// <summary>
        /// Sets the raw Ethereum transaction (RLP encoded type 0, 1, and 2). Complete
        /// unless the callDataFileId is set.
        /// </summary>
        public virtual byte[] EthereumData
        {
            get;
            set => field = EthereumTransactionData.FromBytes(value);
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
        public virtual Hbar MaxGasAllowance { get; set; }


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
                ethereumTransaction.SetMaxGasAllowanceHbar(MaxGasAllowance);
            }

            if (ethereumDataBytes.Length <= MAX_ETHEREUM_DATA_SIZE)
            {
                ethereumTransaction.SetEthereumData(ethereumDataBytes);
            }
            else
            {
                var callDataFileId = CreateFile(ethereumData.callData, client, timeoutPerTransaction, EthereumTransaction);
                ethereumData.callData = new byte[]
                {
                };
                ethereumTransaction.SetEthereumData(ethereumData.ToBytes()).SetCallDataFileId(callDataFileId);
            }

            return ethereumTransaction.Execute(client, EthereumTransaction);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
        /// because it uses features introduced in API level 31 (Android 12).</p>*
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>the response</returns>
        public virtual Task<TransactionResponse> ExecuteAsync(Client client)
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
        public virtual Task<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerTransaction)
        {
            if (ethereumData == null)
            {
                return Task.FromException<TransactionResponse>(new InvalidOperationException("Cannot execute a ethereum flow when ethereum data was not provided"));
            }

            var ethereumTransaction = new EthereumTransaction();
            var ethereumDataBytes = ethereumData.ToBytes();
            if (maxGasAllowance != null)
            {
                ethereumTransaction.SetMaxGasAllowanceHbar(MaxGasAllowance);
            }

            if (ethereumDataBytes.Length <= MAX_ETHEREUM_DATA_SIZE)
            {
                return ethereumTransaction.SetEthereumData(ethereumDataBytes).ExecuteAsync(client);
            }
            else
            {
                return CreateFileAsync(ethereumData.callData, client, timeoutPerTransaction, EthereumTransaction)
                    .ThenCompose((callDataFileId) => ethereumTransaction.SetEthereumData(ethereumData.ToBytes())
                    .SetCallDataFileId(callDataFileId)
                    .ExecuteAsync(client, timeoutPerTransaction));
            }
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse, Exception> callback)
        {
            ActionHelper.Action(ExecuteAsync(client), callback);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, Action<TransactionResponse, Exception> callback)
        {
            ActionHelper.Action(ExecuteAsync(client, timeoutPerTransaction), callback);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAsync(client, timeoutPerTransaction), onSuccess, onFailure);
        }
    }
}