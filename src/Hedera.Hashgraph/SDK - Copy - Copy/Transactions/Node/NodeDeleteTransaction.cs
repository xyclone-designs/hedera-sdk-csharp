// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hapi.Node.Addressbook;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Node
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
        private ulong nodeId;
        /// <summary>
        /// Constructor.
        /// </summary>
        public NodeDeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        NodeDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        NodeDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the consensus node identifier in the network state.
        /// </summary>
        /// <returns>the consensus node identifier in the network state.</returns>
        /// <exception cref="IllegalStateException">when node is not being set</exception>
        public virtual ulong GetNodeId()
        {
            if (nodeId == null)
            {
                throw new InvalidOperationException("NodeDeleteTransaction: 'nodeId' has not been set");
            }

            return nodeId;
        }

        /// <summary>
        /// A consensus node identifier in the network state.
        /// <p>
        /// The node identified MUST exist in the network address book.<br/>
        /// The node identified MUST NOT be deleted.<br/>
        /// This value is REQUIRED.
        /// </summary>
        /// <param name="nodeId">the consensus node identifier in the network state.</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="IllegalArgumentException">if nodeId is negative</exception>
        public virtual NodeDeleteTransaction SetNodeId(long nodeId)
        {
            RequireNotFrozen();
            if (nodeId < 0)
            {
                throw new ArgumentException("NodeDeleteTransaction: 'nodeId' must be non-negative");
            }

            nodeId = nodeId;
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.NodeDeleteTransactionBody}</returns>
        public virtual NodeDeleteTransactionBody Build()
        {
            var builder = new NodeDeleteTransactionBody();
            if (nodeId != null)
            {
                builder.NodeId = nodeId;
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.NodeDelete;
            nodeId = body.NodeId;
        }

        public override void ValidateChecksums(Client client)
        {
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return AddressBookServiceGrpc.GetDeleteNodeMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.NodeDelete = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.NodeDelete = Build();
        }

        /// <summary>
        /// Freeze this transaction with the given client.
        /// </summary>
        /// <param name="client">the client to freeze with</param>
        /// <returns>this transaction</returns>
        /// <exception cref="IllegalStateException">if nodeId is not set</exception>
        public override NodeDeleteTransaction FreezeWith(Client client)
        {
            if (nodeId == null)
            {
                throw new InvalidOperationException("NodeDeleteTransaction: 'nodeId' must be explicitly set before calling freeze().");
            }

            return base.FreezeWith(client);
        }
    }
}