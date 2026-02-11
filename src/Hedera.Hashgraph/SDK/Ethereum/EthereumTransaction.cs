// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Ethereum
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
        public EthereumTransaction() { }
		public EthereumTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		public EthereumTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        private FileId CallDataFileId { get; set; }

		/// <summary>
		/// Sets the raw Ethereum transaction (RLP encoded type 0, 1, and 2). Complete
		/// unless the callDataFileId is set.
		/// </summary>
		public virtual byte[] EthereumData
        {
            get;
            set { RequireNotFrozen(); field = value.CopyArray(); }
        }
		/// <summary>
		/// For large transactions (for example contract create) this should be used to
		/// set the FileId of an HFS file containing the callData
		/// of the ethereumData. The data in the ethereumData will be re-written with
		/// the callData element as a zero length string with the original contents in
		/// the referenced file at time of execution. The ethereumData will need to be
		/// "rehydrated" with the callData for signature validation to pass.
		/// </summary>
		public virtual FileId FileId
		{
			get;
			set { RequireNotFrozen(); field = value; }
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
        public virtual Hbar MaxGasAllowanceHbar
        {
            get => field ?? Hbar.ZERO;
			set { RequireNotFrozen(); field = value; }
        }

        private void InitFromTransactionBody()
        {
            if (SourceTransactionBody.EthereumTransaction.CallData is not null)
				CallDataFileId = FileId.FromProtobuf(SourceTransactionBody.EthereumTransaction.CallData);

			EthereumData = SourceTransactionBody.EthereumTransaction.EthereumData.ToByteArray();
			MaxGasAllowanceHbar = Hbar.FromTinybars(SourceTransactionBody.EthereumTransaction.MaxGasAllowance);
		}

        private Proto.EthereumTransactionBody ToProtobuf()
        {
			Proto.EthereumTransactionBody proto = new ()
            {
                EthereumData = ByteString.CopyFrom(EthereumData),
                MaxGasAllowance = MaxGasAllowanceHbar.ToTinybars(),
            };

            if (CallDataFileId != null)
                proto.CallData = CallDataFileId.ToProtobuf();

            return proto;
        }

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.EthereumTransaction = ToProtobuf();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule EthereumTransaction");
        }
		public override void ValidateChecksums(Client client)
        {
            CallDataFileId?.ValidateChecksum(client);
        }
        public override void OnExecute(Client client)
        {
            throw new NotImplementedException();
        }

		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return SmartContractServiceGrpc.GetCallEthereumMethod();
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}