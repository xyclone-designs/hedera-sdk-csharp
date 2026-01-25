// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Transactions.Ethereum
{
    /// <summary>
    /// A transaction in Ethereum format.<br/>
    /// Make an Ethereum transaction "call" with all data in Ethereum formats,
    /// including the contract alias. Call data may be in the transaction, or
    /// stored within an Hedera File.
    /// 
    /// The caller MAY offer additional gas above what is offered in the call data,
    /// but MAY be charged up to 80% of that value if the amount required is less
    /// than this "floor" amount.
    /// 
    /// ### Block Stream Effects
    /// An `EthereumOutput` message SHALL be emitted for each transaction.
    /// </summary>
    public class EthereumTransaction : Transaction<EthereumTransaction>
    {
        private byte[] ethereumData = new byte[0];
        private FileId callDataFileId = null;
        private Hbar maxGasAllowanceHbar = Hbar.ZERO;
        /// <summary>
        /// Constructor
        /// </summary>
        public EthereumTransaction()
        {
        }

        EthereumTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        EthereumTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Gets the raw Ethereum transaction
        /// </summary>
        /// <returns>the raw Ethereum transaction</returns>
        public virtual byte[] GetEthereumData()
        {
            return Array.CopyOf(ethereumData, ethereumData.Length);
        }

        /// <summary>
        /// Sets the raw Ethereum transaction (RLP encoded type 0, 1, and 2). Complete
        /// unless the callDataFileId is set.
        /// </summary>
        /// <param name="ethereumData">raw ethereum transaction bytes</param>
        /// <returns>{@code this}</returns>
        public virtual EthereumTransaction SetEthereumData(byte[] ethereumData)
        {
            Objects.RequireNonNull(ethereumData);
            RequireNotFrozen();
            ethereumData = Array.CopyOf(ethereumData, ethereumData.Length);
            return this;
        }

        /// <summary>
        /// Gets the FileId of the call data
        /// </summary>
        /// <returns>the FileId of the call data</returns>
        public virtual FileId GetCallDataFileId()
        {
            return callDataFileId;
        }

        /// <summary>
        /// For large transactions (for example contract create) this should be used to
        /// set the FileId of an HFS file containing the callData
        /// of the ethereumData. The data in the ethereumData will be re-written with
        /// the callData element as a zero length string with the original contents in
        /// the referenced file at time of execution. The ethereumData will need to be
        /// "rehydrated" with the callData for signature validation to pass.
        /// </summary>
        /// <param name="fileId">File ID of an HFS file containing the callData</param>
        /// <returns>{@code this}</returns>
        public virtual EthereumTransaction SetCallDataFileId(FileId fileId)
        {
            Objects.RequireNonNull(fileId);
            RequireNotFrozen();
            callDataFileId = fileId;
            return this;
        }

        /// <summary>
        /// Gets the maximum amount that the payer of the hedera transaction
        /// is willing to pay to complete the transaction.
        /// </summary>
        /// <returns>the max gas allowance</returns>
        public virtual Hbar GetMaxGasAllowanceHbar()
        {
            return maxGasAllowanceHbar;
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
        /// <param name="maxGasAllowanceHbar">the maximum gas allowance</param>
        /// <returns>{@code this}</returns>
        public virtual EthereumTransaction SetMaxGasAllowanceHbar(Hbar maxGasAllowanceHbar)
        {
            Objects.RequireNonNull(maxGasAllowanceHbar);
            RequireNotFrozen();
            maxGasAllowanceHbar = maxGasAllowanceHbar;
            return this;
        }

        private void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetEthereumTransaction();
            ethereumData = body.GetEthereumData().ToByteArray();
            if (body.HasCallData())
            {
                callDataFileId = FileId.FromProtobuf(body.GetCallData());
            }

            maxGasAllowanceHbar = Hbar.FromTinybars(body.GetMaxGasAllowance());
        }

        private EthereumTransactionBody.Builder Build()
        {
            var builder = EthereumTransactionBody.NewBuilder().SetEthereumData(ByteString.CopyFrom(ethereumData)).SetMaxGasAllowance(maxGasAllowanceHbar.ToTinybars());
            if (callDataFileId != null)
            {
                builder.SetCallData(callDataFileId.ToProtobuf());
            }

            return builder;
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetCallEthereumMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetEthereumTransaction(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            throw new NotSupportedException("Cannot schedule EthereumTransaction");
        }

        override void ValidateChecksums(Client client)
        {
            if (callDataFileId != null)
            {
                callDataFileId.ValidateChecksum(client);
            }
        }
    }
}