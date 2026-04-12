// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Ethereum
{
    /// <include file="EthereumTransaction.cs.xml" path='docs/member[@name="T:EthereumTransaction"]/*' />
    public class EthereumTransaction : Transaction<EthereumTransaction>
    {
        public EthereumTransaction() { }
		internal EthereumTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		internal EthereumTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="EthereumTransaction.cs.xml" path='docs/member[@name="M:EthereumTransaction.RequireNotFrozen"]/*' />
		public virtual byte[] EthereumData
        {
            get;
            set { RequireNotFrozen(); field = value.CopyArray(); }
        } = [];
		/// <include file="EthereumTransaction.cs.xml" path='docs/member[@name="M:EthereumTransaction.RequireNotFrozen_2"]/*' />
		public virtual FileId? CallDataFileId
		{
			get;
			set { RequireNotFrozen(); field = value; }
		}
        /// <include file="EthereumTransaction.cs.xml" path='docs/member[@name="M:EthereumTransaction.RequireNotFrozen_3"]/*' />
        public virtual Hbar MaxGasAllowanceHbar
        {
            get;
            set { RequireNotFrozen(); field = value; }
        } = Hbar.ZERO;

		private void InitFromTransactionBody()
        {
            if (SourceTransactionBody.EthereumTransaction.CallData is not null)
				CallDataFileId = FileId.FromProtobuf(SourceTransactionBody.EthereumTransaction.CallData);

			EthereumData = SourceTransactionBody.EthereumTransaction.EthereumData.ToByteArray();
			MaxGasAllowanceHbar = Hbar.FromTinybars(SourceTransactionBody.EthereumTransaction.MaxGasAllowance);
		}

        private Proto.Services.EthereumTransactionBody ToProtobuf()
        {
			Proto.Services.EthereumTransactionBody proto = new ()
            {
                EthereumData = ByteString.CopyFrom(EthereumData),
                MaxGasAllowance = MaxGasAllowanceHbar.ToTinybars(),
            };

            if (CallDataFileId != null)
                Proto.Services.CallData = CallDataFileId.ToProtobuf();

            return proto;
        }

		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.EthereumTransaction = ToProtobuf();
        }
		public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
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

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.callEthereum);

			return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
