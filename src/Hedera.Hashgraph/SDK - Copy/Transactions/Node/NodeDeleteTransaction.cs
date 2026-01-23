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
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;

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
        private long nodeId;
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
        NodeDeleteTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        NodeDeleteTransaction(TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the consensus node identifier in the network state.
        /// </summary>
        /// <returns>the consensus node identifier in the network state.</returns>
        /// <exception cref="IllegalStateException">when node is not being set</exception>
        public virtual long GetNodeId()
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
        virtual NodeDeleteTransactionBody.Builder Build()
        {
            var builder = NodeDeleteTransactionBody.NewBuilder();
            if (nodeId != null)
            {
                builder.SetNodeId(nodeId);
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetNodeDelete();
            nodeId = body.GetNodeId();
        }

        override void ValidateChecksums(Client client)
        {
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return AddressBookServiceGrpc.GetDeleteNodeMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetNodeDelete(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetNodeDelete(Build());
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