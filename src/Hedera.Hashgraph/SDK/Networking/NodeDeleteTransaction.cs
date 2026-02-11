// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <summary>
    /// A transaction to delete a node from the network address book.
    /// 
    /// This transaction body SHALL be considered a "privileged transaction".
    /// 
    /// - A transaction MUST be signed by the governing council.
    /// - Upon success, the address book entry SHALL enter a "pending delete"
    ///   state.
    /// - All address book entries pending deletion SHALL be removed from the
    ///   active network configuration during the next `freeze` transaction with
    ///   the field `freeze_type` set to `PREPARE_UPGRADE`.<br/>
    /// - A deleted address book node SHALL be removed entirely from network state.
    /// - A deleted address book node identifier SHALL NOT be reused.
    /// 
    /// ### Record Stream Effects
    /// Upon completion the "deleted" `node_id` SHALL be in the transaction
    /// receipt.
    /// </summary>
    public class NodeDeleteTransaction : Transaction<NodeDeleteTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public NodeDeleteTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal NodeDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal NodeDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// A consensus node identifier in the network state.
		/// <p>
		/// The node identified MUST exist in the network address book.<br/>
		/// The node identified MUST NOT be deleted.<br/>
		/// This value is REQUIRED.
		/// </summary>
		public virtual ulong? NodeId
        {
            get;
            set
            {
				RequireNotFrozen();
				if (field < 0)
					throw new ArgumentException("NodeDeleteTransaction: 'nodeId' must be non-negative");
                field = value;
			}
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.NodeDelete;
            
            NodeId = body.NodeId;
        }
		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.NodeDeleteTransactionBody}</returns>
		public virtual Proto.NodeDeleteTransactionBody ToProtobuf()
		{
			var builder = new Proto.NodeDeleteTransactionBody();

			if (NodeId != null)
				builder.NodeId = NodeId.Value;

			return builder;
		}

		public override void ValidateChecksums(Client client)
        {
        }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.NodeDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.NodeDelete = ToProtobuf();
        }

		/// <summary>
		/// Freeze this transaction with the given client.
		/// </summary>
		/// <param name="client">the client to freeze with</param>
		/// <returns>this transaction</returns>
		/// <exception cref="IllegalStateException">if nodeId is not set</exception>
		public override NodeDeleteTransaction FreezeWith(Client client)
		{
			if (NodeId == null)
			{
				throw new InvalidOperationException("NodeDeleteTransaction: 'nodeId' must be explicitly set before calling freeze().");
			}

			return base.FreezeWith(client);
		}
		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return AddressBookServiceGrpc.GetDeleteNodeMethod();
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