// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Ethereum
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

		private static FileId CreateFile<T>(byte[] callData, Client client, Duration timeoutPerTransaction, Transaction<T> ethereumTransaction) where T : Transaction<T>
        {
            try
            {
                // Hex encode the call data
                byte[] callDataHex = Hex.Encode(callData);
                FileCreateTransaction filecreatetransaction = new()
                {
                    Keys = [client.OperatorPublicKey],
					//Contents = callDataHex.Take(Math.Min(FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length))
					Contents = [.. callDataHex.Take(Math.Min(FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length))]
				};

				TransactionResponse transactionresponse = filecreatetransaction.Execute(client, timeoutPerTransaction);
                TransactionReceipt transactionreceipt = transactionresponse.GetReceipt(client, timeoutPerTransaction);
                
                if (callDataHex.Length > FileAppendTransaction.DEFAULT_CHUNK_SIZE)
					new FileAppendTransaction
					{
						FileId = transactionreceipt.FileId,
						MaxChunks = 1000,
						NodeAccountIds = [transactionresponse.NodeId],
						//Contents = Array.CopyOfRange(callDataHex, FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length),
						Contents = ByteString.CopyFrom([.. callDataHex.Skip(FileAppendTransaction.DEFAULT_CHUNK_SIZE)])
					}
					.Execute(client, timeoutPerTransaction)
					.GetReceipt(client);

				ethereumTransaction.SetNodeAccountIds([transactionresponse.NodeId]);
                
                return transactionreceipt.FileId;
            }
            catch (ReceiptStatusException e)
            {
                throw new Exception(string.Empty, e);
            }
        }
        private static async Task<FileId> CreateFileAsync<T>(byte[] callData, Client client, Duration timeoutPerTransaction, Transaction<T> ethereumTransaction) where T : Transaction<T>
		{
			// Hex encode the call data
			byte[] callDataHex = Hex.Encode(callData);

			TransactionResponse transactionresponse = await new FileCreateTransaction
			{
				Keys = [client.OperatorPublicKey],
				//Contents = Array.CopyOfRange(callDataHex, 0, Math.Min(FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length)),
				Contents = [.. callDataHex.Take(Math.Min(FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length))]

			}.ExecuteAsync(client, timeoutPerTransaction);
			TransactionReceipt transactionreceipt = await transactionresponse.GetReceiptAsync(client, timeoutPerTransaction);

			ethereumTransaction.SetNodeAccountIds([transactionresponse.NodeId]);

			if (callDataHex.Length > FileAppendTransaction.DEFAULT_CHUNK_SIZE)
			{
				TransactionResponse transactionresponse2 = await new FileAppendTransaction
				{
					FileId = transactionreceipt.FileId,
					NodeAccountIds = [transactionresponse.NodeId],
					MaxChunks = 1000,
					//Contents = Array.CopyOfRange(callDataHex, FileAppendTransaction.DEFAULT_CHUNK_SIZE, callDataHex.Length),
					Contents = ByteString.CopyFrom([.. callDataHex.Skip(FileAppendTransaction.DEFAULT_CHUNK_SIZE)])

				}.ExecuteAsync(client, timeoutPerTransaction);

				TransactionReceipt transactionreceipt2 = await transactionresponse.GetReceiptAsync(client, timeoutPerTransaction);

				return transactionreceipt2.FileId;

			}
			else
			{
				return Task.FromResult(transactionreceipt.FileId);
			}
        }

        /// <summary>
        /// Sets the raw Ethereum transaction (RLP encoded type 0, 1, and 2). Complete
        /// unless the callDataFileId is set.
        /// </summary>
        public virtual EthereumTransactionData? EthereumData { get; set; }
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
        public virtual Hbar? MaxGasAllowance { get; set; }
        public virtual FileId? CallDataFileId { get; set; }


        /// <summary>
        /// Execute the transactions in the flow with the passed in client.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>the response</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public virtual TransactionResponse Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
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
            if (EthereumData == null)
            {
                throw new InvalidOperationException("Cannot execute a ethereum flow when ethereum data was not provided");
            }

            var ethereumTransaction = new EthereumTransaction();
            var ethereumDataBytes = EthereumData.ToBytes();

            if (MaxGasAllowance != null)
                ethereumTransaction.MaxGasAllowanceHbar = MaxGasAllowance;

            if (ethereumDataBytes.Length <= MAX_ETHEREUM_DATA_SIZE)
                ethereumTransaction.EthereumData = ethereumDataBytes;
            else
            {
                var callDataFileId = CreateFile(EthereumData.CallData, client, timeoutPerTransaction, ethereumTransaction);
				ethereumTransaction.FileId = callDataFileId;
				ethereumTransaction.EthereumData = EthereumData.ToBytes();
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
        public virtual Task<TransactionResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.RequestTimeout);
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
        public virtual async Task<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerTransaction)
        {
            if (EthereumData == null)
            {
                throw new InvalidOperationException("Cannot execute a ethereum flow when ethereum data was not provided");
            }

            var ethereumTransaction = new EthereumTransaction();
            var ethereumDataBytes = EthereumData.ToBytes();
            if (MaxGasAllowance != null)
            {
                ethereumTransaction.MaxGasAllowanceHbar = MaxGasAllowance;
            }

            if (ethereumDataBytes.Length <= MAX_ETHEREUM_DATA_SIZE)
            {
                ethereumTransaction.EthereumData = ethereumDataBytes;

				return await ethereumTransaction.ExecuteAsync(client);
            }
            else
            {
                FileId fileid = await CreateFileAsync(ethereumDataBytes, client, timeoutPerTransaction, ethereumTransaction);

				ethereumTransaction.FileId = fileid;
				ethereumTransaction.EthereumData = EthereumData.ToBytes();

                return await ethereumTransaction.ExecuteAsync(client, timeoutPerTransaction);
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