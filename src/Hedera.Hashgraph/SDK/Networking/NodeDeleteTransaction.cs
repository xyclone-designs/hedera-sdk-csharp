// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <include file="NodeDeleteTransaction.cs.xml" path='docs/member[@name="T:NodeDeleteTransaction"]/*' />
    public class NodeDeleteTransaction : Transaction<NodeDeleteTransaction>
    {
        /// <include file="NodeDeleteTransaction.cs.xml" path='docs/member[@name="M:NodeDeleteTransaction.#ctor"]/*' />
        public NodeDeleteTransaction() { }
		/// <include file="NodeDeleteTransaction.cs.xml" path='docs/member[@name="M:NodeDeleteTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal NodeDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="NodeDeleteTransaction.cs.xml" path='docs/member[@name="M:NodeDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal NodeDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="NodeDeleteTransaction.cs.xml" path='docs/member[@name="M:NodeDeleteTransaction.RequireNotFrozen"]/*' />
		public virtual ulong? NodeId
        {
            get;
            set
            {
				RequireNotFrozen();
                field = value;
			}
        }

        /// <include file="NodeDeleteTransaction.cs.xml" path='docs/member[@name="M:NodeDeleteTransaction.InitFromTransactionBody"]/*' />
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.NodeDelete;
            
            NodeId = body.NodeId;
        }
		/// <include file="NodeDeleteTransaction.cs.xml" path='docs/member[@name="M:NodeDeleteTransaction.ToProtobuf"]/*' />
		public virtual Proto.NodeDeleteTransactionBody ToProtobuf()
		{
			var builder = new Proto.NodeDeleteTransactionBody();

			if (NodeId != null)
				builder.NodeId = NodeId.Value;

			return builder;
		}

		public override void ValidateChecksums(Client client) { /* No op */ }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.NodeDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.NodeDelete = ToProtobuf();
        }

		/// <include file="NodeDeleteTransaction.cs.xml" path='docs/member[@name="M:NodeDeleteTransaction.FreezeWith(Client)"]/*' />
		public override NodeDeleteTransaction FreezeWith(Client? client)
		{
			if (NodeId == null)
			{
				throw new InvalidOperationException("NodeDeleteTransaction: 'nodeId' must be explicitly set before calling freeze().");
			}

			return base.FreezeWith(client);
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.AddressBookService.AddressBookServiceClient.deleteNode);

			return Proto.AddressBookService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}