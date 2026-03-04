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
    /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:Obsolete(&quot;Obsolete&quot;)"]/*' />
    [Obsolete("Obsolete")]
    public class EthereumFlow
    {
        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="F:.MAX_ETHEREUM_DATA_SIZE"]/*' />
        static int MAX_ETHEREUM_DATA_SIZE = 128000;

		private static FileId CreateFile<T>(byte[] callData, Client client, TimeSpan timeoutPerTransaction, Transaction<T> ethereumTransaction) where T : Transaction<T>
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
        private static async Task<FileId> CreateFileAsync<T>(byte[] callData, Client client, TimeSpan timeoutPerTransaction, Transaction<T> ethereumTransaction) where T : Transaction<T>
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
				return transactionreceipt.FileId;
			}
        }

        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="P:.EthereumData"]/*' />
        public virtual EthereumTransactionData? EthereumData { get; set; }
        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="P:.MaxGasAllowance"]/*' />
        public virtual Hbar? MaxGasAllowance { get; set; }
        public virtual FileId? CallDataFileId { get; set; }


        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:Execute(Client)"]/*' />
        public virtual TransactionResponse Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
        }
        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:Execute(Client,System.TimeSpan)"]/*' />
        public virtual TransactionResponse Execute(Client client, TimeSpan timeoutPerTransaction)
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
				ethereumTransaction.CallDataFileId = callDataFileId;
				ethereumTransaction.EthereumData = EthereumData.ToBytes();
			}

            return ethereumTransaction.Execute(client, timeoutPerTransaction);
        }

        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:ExecuteAsync(Client)"]/*' />
        public virtual Task<TransactionResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.RequestTimeout);
        }
        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:ExecuteAsync(Client,System.TimeSpan)"]/*' />
        public virtual async Task<TransactionResponse> ExecuteAsync(Client client, TimeSpan timeoutPerTransaction)
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

				ethereumTransaction.CallDataFileId = fileid;
				ethereumTransaction.EthereumData = EthereumData.ToBytes();

                return await ethereumTransaction.ExecuteAsync(client, timeoutPerTransaction);
            }
        }

        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:ExecuteAsync(Client,System.Action{TransactionResponse,System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse?, Exception?> callback)
        {
            Utils.ActionHelper.Action(ExecuteAsync(client), callback);
        }
        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:ExecuteAsync(Client,System.TimeSpan,System.Action{TransactionResponse,System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, TimeSpan timeoutPerTransaction, Action<TransactionResponse?, Exception?> callback)
        {
            Utils.ActionHelper.Action(ExecuteAsync(client, timeoutPerTransaction), callback);
        }
        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:ExecuteAsync(Client,System.Action{TransactionResponse},System.Action{System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
        }
        /// <include file="EthereumFlow.cs.xml" path='docs/member[@name="M:ExecuteAsync(Client,System.TimeSpan,System.Action{TransactionResponse},System.Action{System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, TimeSpan timeoutPerTransaction, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(ExecuteAsync(client, timeoutPerTransaction), onSuccess, onFailure);
        }
    }
}