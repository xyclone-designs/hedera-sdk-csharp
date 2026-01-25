// SPDX-License-Identifier: Apache-2.0
using Com.Google;
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
    /// A transaction to modify address book node attributes.
    /// 
    /// - This transaction SHALL enable the node operator, as identified by the
    ///   `admin_key`, to modify operational attributes of the node.
    /// - This transaction MUST be signed by the active `admin_key` for the node.
    /// - If this transaction sets a new value for the `admin_key`, then both the
    ///   current `admin_key`, and the new `admin_key` MUST sign this transaction.
    /// - This transaction SHALL NOT change any field that is not set (is null) in
    ///   this transaction body.
    /// - This SHALL create a pending update to the node, but the change SHALL NOT
    ///   be immediately applied to the active configuration.
    /// - All pending node updates SHALL be applied to the active network
    ///   configuration during the next `freeze` transaction with the field
    ///   `freeze_type` set to `PREPARE_UPGRADE`.
    /// 
    /// ### Record Stream Effects
    /// Upon completion the `node_id` for the updated entry SHALL be in the
    /// transaction receipt.
    /// </summary>
    public class NodeUpdateTransaction : Transaction<NodeUpdateTransaction>
    {
        private long nodeId;
        private AccountId accountId = null;
        private string description = null;
        private IList<Endpoint> gossipEndpoints = new ();
        private IList<Endpoint> serviceEndpoints = new ();
        private byte[] gossipCaCertificate = null;
        private byte[] grpcCertificateHash = null;
        private Key adminKey = null;
        private bool declineReward = null;
        private Endpoint grpcWebProxyEndpoint = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public NodeUpdateTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        NodeUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        NodeUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
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
                throw new InvalidOperationException("NodeUpdateTransaction: 'nodeId' has not been set");
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
        public virtual NodeUpdateTransaction SetNodeId(long nodeId)
        {
            RequireNotFrozen();
            if (nodeId < 0)
            {
                throw new ArgumentException("nodeId must be non-negative");
            }

            nodeId = nodeId;
            return this;
        }

        /// <summary>
        /// Extract the Account ID of the Node.
        /// </summary>
        /// <returns>the Account ID of the Node.</returns>
        public virtual AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// An account identifier.
        /// <p>
        /// If set, this SHALL replace the node account identifier.<br/>
        /// If set, this transaction MUST be signed by the active `key` for _both_
        /// the current node account _and_ the identified new node account.
        /// </summary>
        /// <param name="accountId">the Account ID of the Node.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeUpdateTransaction SetAccountId(AccountId accountId)
        {
            RequireNotFrozen();
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the description of the node.
        /// </summary>
        /// <returns>the node's description.</returns>
        public virtual string GetDescription()
        {
            return description;
        }

        /// <summary>
        /// A short description of the node.
        /// <p>
        /// This value, if set, MUST NOT exceed 100 bytes when encoded as UTF-8.<br/>
        /// If set, this value SHALL replace the previous value.
        /// </summary>
        /// <param name="description">The String to be set as the description of the node.</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="IllegalArgumentException">if description exceeds 100 bytes when encoded as UTF-8</exception>
        public virtual NodeUpdateTransaction SetDescription(string description)
        {
            RequireNotFrozen();
            if (description != null && description.GetBytes(java.nio.charset.StandardCharsets.UTF_8).Length > 100)
            {
                throw new ArgumentException("Description must not exceed 100 bytes when encoded as UTF-8");
            }

            description = description;
            return this;
        }

        /// <summary>
        /// Remove the description contents.
        /// </summary>
        /// <returns>{@code this}</returns>
        public virtual NodeUpdateTransaction ClearDescription()
        {
            RequireNotFrozen();
            description = "";
            return this;
        }

        /// <summary>
        /// Extract the list of service endpoints for gossip.
        /// </summary>
        /// <returns>the list of service endpoints for gossip.</returns>
        public virtual IList<Endpoint> GetGossipEndpoints()
        {
            return gossipEndpoints;
        }

        /// <summary>
        /// A list of service endpoints for gossip.
        /// <p>
        /// If set, this list MUST meet the following requirements.
        /// <hr/>
        /// These endpoints SHALL represent the published endpoints to which other
        /// consensus nodes may _gossip_ transactions.<br/>
        /// These endpoints SHOULD NOT specify both address and DNS name.<br/>
        /// This list MUST NOT be empty.<br/>
        /// This list MUST NOT contain more than `10` entries.<br/>
        /// The first two entries in this list SHALL be the endpoints published to
        /// all consensus nodes.<br/>
        /// All other entries SHALL be reserved for future use.
        /// <p>
        /// Each network may have additional requirements for these endpoints.
        /// A client MUST check network-specific documentation for those
        /// details.<br/>
        /// <blockquote>Example<blockquote>
        /// Hedera Mainnet _requires_ that address be specified, and does not
        /// permit DNS name (FQDN) to be specified.<br/>
        /// Mainnet also requires that the first entry be an "internal" IP
        /// address and the second entry be an "external" IP address.
        /// </blockquote>
        /// <blockquote>
        /// Solo, however, _requires_ DNS name (FQDN) but also permits
        /// address.
        /// </blockquote></blockquote>
        /// <p>
        /// If set, the new list SHALL replace the existing list.
        /// </summary>
        /// <param name="gossipEndpoints">the list of service endpoints for gossip.</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="IllegalArgumentException">if the list is empty or contains more than 10 endpoints</exception>
        public virtual NodeUpdateTransaction SetGossipEndpoints(IList<Endpoint> gossipEndpoints)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(gossipEndpoints);
            if (gossipEndpoints.IsEmpty())
            {
                throw new ArgumentException("Gossip endpoints list must not be empty");
            }

            if (gossipEndpoints.Count > 10)
            {
                throw new ArgumentException("Gossip endpoints list must not contain more than 10 entries");
            }

            foreach (Endpoint endpoint in gossipEndpoints)
            {
                Endpoint.ValidateNoIpAndDomain(endpoint);
            }

            gossipEndpoints = new List(gossipEndpoints);
            return this;
        }

        /// <summary>
        /// Add an endpoint for gossip to the list of service endpoints for gossip.
        /// </summary>
        /// <param name="gossipEndpoint">endpoints for gossip to add.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeUpdateTransaction AddGossipEndpoint(Endpoint gossipEndpoint)
        {
            RequireNotFrozen();
            gossipEndpoints.Add(gossipEndpoint);
            return this;
        }

        /// <summary>
        /// Extract the list of service endpoints for gRPC calls.
        /// </summary>
        /// <returns>the list of service endpoints for gRPC calls.</returns>
        public virtual IList<Endpoint> GetServiceEndpoints()
        {
            return serviceEndpoints;
        }

        /// <summary>
        /// A list of service endpoints for gRPC calls.
        /// <p>
        /// If set, this list MUST meet the following requirements.
        /// <hr/>
        /// These endpoints SHALL represent the published endpoints to which clients
        /// may submit transactions.<br/>
        /// These endpoints SHOULD specify address and port.<br/>
        /// These endpoints MAY specify a DNS name.<br/>
        /// These endpoints SHOULD NOT specify both address and DNS name.<br/>
        /// This list MUST NOT be empty.<br/>
        /// This list MUST NOT contain more than `8` entries.
        /// <p>
        /// Each network may have additional requirements for these endpoints.
        /// A client MUST check network-specific documentation for those
        /// details.
        /// <p>
        /// If set, the new list SHALL replace the existing list.
        /// </summary>
        /// <param name="serviceEndpoints">list of service endpoints for gRPC calls.</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="IllegalArgumentException">if the list is empty or contains more than 8 endpoints</exception>
        public virtual NodeUpdateTransaction SetServiceEndpoints(IList<Endpoint> serviceEndpoints)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(serviceEndpoints);
            if (serviceEndpoints.IsEmpty())
            {
                throw new ArgumentException("Service endpoints list must not be empty");
            }

            if (serviceEndpoints.Count > 8)
            {
                throw new ArgumentException("Service endpoints list must not contain more than 8 entries");
            }

            foreach (Endpoint endpoint in serviceEndpoints)
            {
                Endpoint.ValidateNoIpAndDomain(endpoint);
            }

            serviceEndpoints = new List(serviceEndpoints);
            return this;
        }

        /// <summary>
        /// Add an endpoint for gRPC calls to the list of service endpoints for gRPC calls.
        /// </summary>
        /// <param name="serviceEndpoint">endpoints for gRPC calls to add.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeUpdateTransaction AddServiceEndpoint(Endpoint serviceEndpoint)
        {
            RequireNotFrozen();
            serviceEndpoints.Add(serviceEndpoint);
            return this;
        }

        /// <summary>
        /// Extract the certificate used to sign gossip events.
        /// </summary>
        /// <returns>the DER encoding of the certificate presented.</returns>
        public virtual byte[] GetGossipCaCertificate()
        {
            return gossipCaCertificate;
        }

        /// <summary>
        /// A certificate used to sign gossip events.
        /// <p>
        /// This value MUST be a certificate of a type permitted for gossip
        /// signatures.<br/>
        /// This value MUST be the DER encoding of the certificate presented.
        /// <p>
        /// If set, the new value SHALL replace the existing bytes value.
        /// </summary>
        /// <param name="gossipCaCertificate">the DER encoding of the certificate presented.</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="IllegalArgumentException">if gossipCaCertificate is null or empty</exception>
        public virtual NodeUpdateTransaction SetGossipCaCertificate(byte[] gossipCaCertificate)
        {
            RequireNotFrozen();
            if (gossipCaCertificate == null || gossipCaCertificate.Length == 0)
            {
                throw new ArgumentException("Gossip CA certificate must not be null or empty");
            }

            gossipCaCertificate = gossipCaCertificate;
            return this;
        }

        /// <summary>
        /// Extract the hash of the node gRPC TLS certificate.
        /// </summary>
        /// <returns>SHA-384 hash of the node gRPC TLS certificate.</returns>
        public virtual byte[] GetGrpcCertificateHash()
        {
            return grpcCertificateHash;
        }

        /// <summary>
        /// A hash of the node gRPC TLS certificate.
        /// <p>
        /// This value MAY be used to verify the certificate presented by the node
        /// during TLS negotiation for gRPC.<br/>
        /// This value MUST be a SHA-384 hash.<br/>
        /// The TLS certificate to be hashed MUST first be in PEM format and MUST be
        /// encoded with UTF-8 NFKD encoding to a stream of bytes provided to
        /// the hash algorithm.<br/>
        /// <p>
        /// If set, the new value SHALL replace the existing hash value.
        /// </summary>
        /// <param name="grpcCertificateHash">SHA-384 hash of the node gRPC TLS certificate.</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="IllegalArgumentException">if grpcCertificateHash is not 48 bytes (SHA-384 size) when non-empty</exception>
        public virtual NodeUpdateTransaction SetGrpcCertificateHash(byte[] grpcCertificateHash)
        {
            RequireNotFrozen();
            if (grpcCertificateHash != null && grpcCertificateHash.Length > 0 && grpcCertificateHash.Length != 48)
            {
                throw new ArgumentException("gRPC certificate hash must be exactly 48 bytes (SHA-384)");
            }

            grpcCertificateHash = grpcCertificateHash;
            return this;
        }

        /// <summary>
        /// Get an administrative key controlled by the node operator.
        /// </summary>
        /// <returns>an administrative key controlled by the node operator.</returns>
        public virtual Key GetAdminKey()
        {
            return adminKey;
        }

        /// <summary>
        /// An administrative key controlled by the node operator.
        /// <p>
        /// This field is OPTIONAL.<br/>
        /// If set, this key MUST sign this transaction.<br/>
        /// If set, this key MUST sign each subsequent transaction to
        /// update this node.<br/>
        /// If set, this field MUST contain a valid `Key` value.<br/>
        /// If set, this field MUST NOT be set to an empty `KeyList`.
        /// </summary>
        /// <param name="adminKey">an administrative key to be set.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeUpdateTransaction SetAdminKey(Key adminKey)
        {
            RequireNotFrozen();
            adminKey = adminKey;
            return this;
        }

        /// <summary>
        /// Gets whether this node declines rewards.
        /// </summary>
        /// <returns>true if the node declines rewards; false if it accepts rewards.</returns>
        public virtual bool GetDeclineReward()
        {
            return declineReward;
        }

        /// <summary>
        /// Sets whether this node should decline rewards.
        /// </summary>
        /// <param name="decline">true to decline rewards; false to accept them. If left null no change will be made.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeUpdateTransaction SetDeclineReward(bool decline)
        {
            RequireNotFrozen();
            declineReward = decline;
            return this;
        }

        /// <summary>
        /// Get a web proxy for gRPC from non-gRPC clients.
        /// </summary>
        public virtual Endpoint GetGrpcWebProxyEndpoint()
        {
            return grpcWebProxyEndpoint;
        }

        /// <summary>
        /// A web proxy for gRPC from non-gRPC clients.
        /// <p>
        /// This endpoint SHALL be a Fully Qualified Domain Name (FQDN) using the HTTPS
        /// protocol, and SHALL support gRPC-Web for use by browser-based clients.<br/>
        /// This endpoint MUST be signed by a trusted certificate authority.<br/>
        /// This endpoint MUST use a valid port and SHALL be reachable over TLS.<br/>
        /// This field MAY be omitted if the node does not support gRPC-Web access.<br/>
        /// This field MUST be updated if the gRPC-Web endpoint changes.<br/>
        /// This field SHALL enable frontend clients to avoid hard-coded proxy endpoints.
        /// </summary>
        public virtual NodeUpdateTransaction SetGrpcWebProxyEndpoint(Endpoint grpcWebProxyEndpoint)
        {
            RequireNotFrozen();
            grpcWebProxyEndpoint = grpcWebProxyEndpoint;
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.NodeUpdateTransactionBody}</returns>
        virtual NodeUpdateTransactionBody.Builder Build()
        {
            var builder = NodeUpdateTransactionBody.NewBuilder();
            if (nodeId != null)
            {
                builder.SetNodeId(nodeId);
            }

            if (accountId != null)
            {
                builder.SetAccountId(accountId.ToProtobuf());
            }

            if (description != null)
            {
                builder.SetDescription(StringValue.Of(description));
            }

            foreach (Endpoint gossipEndpoint in gossipEndpoints)
            {
                builder.AddGossipEndpoint(gossipEndpoint.ToProtobuf());
            }

            foreach (Endpoint serviceEndpoint in serviceEndpoints)
            {
                builder.AddServiceEndpoint(serviceEndpoint.ToProtobuf());
            }

            if (gossipCaCertificate != null)
            {
                builder.SetGossipCaCertificate(BytesValue.Of(ByteString.CopyFrom(gossipCaCertificate)));
            }

            if (grpcCertificateHash != null)
            {
                builder.SetGrpcCertificateHash(BytesValue.Of(ByteString.CopyFrom(grpcCertificateHash)));
            }

            if (adminKey != null)
            {
                builder.SetAdminKey(adminKey.ToProtobufKey());
            }

            if (declineReward != null)
            {
                builder.SetDeclineReward(BoolValue.Of(declineReward));
            }

            if (grpcWebProxyEndpoint != null)
            {
                builder.SetGrpcProxyEndpoint(grpcWebProxyEndpoint.ToProtobuf());
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetNodeUpdate();
            nodeId = body.GetNodeId();
            if (body.HasAccountId())
            {
                accountId = AccountId.FromProtobuf(body.GetAccountId());
            }

            if (body.HasDescription())
            {
                description = body.GetDescription().GetValue();
            }

            foreach (var gossipEndpoint in body.GetGossipEndpointList())
            {
                gossipEndpoints.Add(Endpoint.FromProtobuf(gossipEndpoint));
            }

            foreach (var serviceEndpoint in body.GetServiceEndpointList())
            {
                serviceEndpoints.Add(Endpoint.FromProtobuf(serviceEndpoint));
            }

            if (body.HasGossipCaCertificate())
            {
                gossipCaCertificate = body.GetGossipCaCertificate().GetValue().ToByteArray();
            }

            if (body.HasGrpcCertificateHash())
            {
                grpcCertificateHash = body.GetGrpcCertificateHash().GetValue().ToByteArray();
            }

            if (body.HasAdminKey())
            {
                adminKey = Key.FromProtobufKey(body.GetAdminKey());
            }

            if (body.HasDeclineReward())
            {
                declineReward = body.GetDeclineReward().GetValue();
            }

            if (body.HasGrpcProxyEndpoint())
            {
                grpcWebProxyEndpoint = Endpoint.FromProtobuf(body.GetGrpcProxyEndpoint());
            }
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return AddressBookServiceGrpc.GetUpdateNodeMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetNodeUpdate(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetNodeUpdate(Build());
        }

        /// <summary>
        /// Freeze this transaction with the given client.
        /// </summary>
        /// <param name="client">the client to freeze with</param>
        /// <returns>this transaction</returns>
        /// <exception cref="IllegalStateException">if nodeId is not set</exception>
        public override NodeUpdateTransaction FreezeWith(Client client)
        {
            if (nodeId == null)
            {
                throw new InvalidOperationException("NodeUpdateTransaction: 'nodeId' must be explicitly set before calling freeze().");
            }

            return base.FreezeWith(client);
        }

        /// <summary>
        /// Delete the gRPC web proxy endpoint.
        /// <p>
        /// This method clears the gRPC web proxy endpoint by setting it to an empty
        /// Endpoint, which will effectively delete it in the mirror node.
        /// </summary>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Validates that an endpoint does not contain both IP address and domain name.
        /// </summary>
        /// <param name="endpoint">the endpoint to validate</param>
        /// <exception cref="IllegalArgumentException">if endpoint contains both IP address and domain name</exception>
        public virtual NodeUpdateTransaction DeleteGrpcWebProxyEndpoint()
        {
            RequireNotFrozen();
            grpcWebProxyEndpoint = new Endpoint();
            return this;
        } // validation moved to Endpoint.validateNoIpAndDomain
    }
}