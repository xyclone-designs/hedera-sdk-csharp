// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hapi.Node.Addressbook;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hedera.Hashgraph.SDK.Transactions.Node
{
    /// <summary>
    /// A transaction to create a new node in the network address book.
    /// The transaction, once complete, enables a new consensus node
    /// to join the network, and requires governing council authorization.
    /// <p>
    /// This transaction body SHALL be considered a "privileged transaction".
    /// <p>
    /// 
    /// - MUST be signed by the governing council.
    /// - MUST be signed by the `Key` assigned to the
    ///   `admin_key` field.
    /// - The newly created node information SHALL be added to the network address
    ///   book information in the network state.
    /// - The new entry SHALL be created in "state" but SHALL NOT participate in
    ///   network consensus and SHALL NOT be present in network "configuration"
    ///   until the next "upgrade" transaction (as noted below).
    /// - All new address book entries SHALL be added to the active network
    ///   configuration during the next `freeze` transaction with the field
    ///   `freeze_type` set to `PREPARE_UPGRADE`.
    /// 
    /// ### Record Stream Effects
    /// Upon completion the newly assigned `node_id` SHALL be in the transaction
    /// receipt.
    /// </summary>
    public class NodeCreateTransaction : Transaction<NodeCreateTransaction>
    {
        private AccountId? accountId = null;
        private string description = "";
        private IList<Endpoint> gossipEndpoints = [];
        private IList<Endpoint> serviceEndpoints = [];
        private byte[]? gossipCaCertificate = null;
        private byte[]? grpcCertificateHash = null;
        private Key? adminKey = null;
        private bool? declineReward = null;
        private Endpoint? grpcWebProxyEndpoint = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public NodeCreateTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        NodeCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        NodeCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
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
        /// A Node account identifier.
        /// <p>
        /// This account identifier MUST be in the "account number" form.<br/>
        /// This account identifier MUST NOT use the alias field.<br/>
        /// If the identified account does not exist, this transaction SHALL fail.<br/>
        /// Multiple nodes MAY share the same node account.<br/>
        /// This field is REQUIRED.
        /// </summary>
        /// <param name="accountId">the Account ID of the Node.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction SetAccountId(AccountId accountId)
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
        /// This field is OPTIONAL.
        /// </summary>
        /// <param name="description">The String to be set as the description of the node.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction SetDescription(string description)
        {
            RequireNotFrozen();
            if (description == null)
            {
                description = "";
                return this;
            }

            int byteLen = Encoding.UTF8.GetByteCount(description);
            if (byteLen > 100)
            {
                throw new ArgumentException("description must not exceed 100 bytes when UTF-8 encoded");
            }

            description = description;
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
        /// These endpoints SHALL represent the published endpoints to which other
        /// consensus nodes may _gossip_ transactions.<br/>
        /// These endpoints MUST specify a port.<br/>
        /// This list MUST NOT be empty.<br/>
        /// This list MUST NOT contain more than `10` entries.<br/>
        /// The first two entries in this list SHALL be the endpoints published to
        /// all consensus nodes.<br/>
        /// All other entries SHALL be reserved for future use.
        /// <p>
        /// Each network may have additional requirements for these endpoints.
        /// A client MUST check network-specific documentation for those
        /// details.<br/>
        /// If the network configuration value `gossipFqdnRestricted` is set, then
        /// all endpoints in this list MUST supply only IP address.<br/>
        /// If the network configuration value `gossipFqdnRestricted` is _not_ set,
        /// then endpoints in this list MAY supply either IP address or FQDN, but
        /// MUST NOT supply both values for the same endpoint.
        /// </summary>
        /// <param name="gossipEndpoints">the list of service endpoints for gossip.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction SetGossipEndpoints(IList<Endpoint> gossipEndpoints)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(gossipEndpoints);
            if (gossipEndpoints.Count > 10)
            {
                throw new ArgumentException("gossipEndpoints must not contain more than 10 entries");
            }

            foreach (Endpoint endpoint in gossipEndpoints)
            {
                Endpoint.ValidateNoIpAndDomain(endpoint);
            }

            gossipEndpoints = [..gossipEndpoints];
            return this;
        }

        /// <summary>
        /// Add an endpoint for gossip to the list of service endpoints for gossip.
        /// </summary>
        /// <param name="gossipEndpoint">endpoints for gossip to add.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction AddGossipEndpoint(Endpoint gossipEndpoint)
        {
            RequireNotFrozen();
            if (gossipEndpoints.Count >= 10)
            {
                throw new ArgumentException("gossipEndpoints must not contain more than 10 entries");
            }

            Endpoint.ValidateNoIpAndDomain(gossipEndpoint);
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
        /// These endpoints SHALL represent the published gRPC endpoints to which
        /// clients may submit transactions.<br/>
        /// These endpoints MUST specify a port.<br/>
        /// Endpoints in this list MAY supply either IP address or FQDN, but MUST
        /// NOT supply both values for the same endpoint.<br/>
        /// This list MUST NOT be empty.<br/>
        /// This list MUST NOT contain more than `8` entries.
        /// </summary>
        /// <param name="serviceEndpoints">list of service endpoints for gRPC calls.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction SetServiceEndpoints(IList<Endpoint> serviceEndpoints)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(serviceEndpoints);
            if (serviceEndpoints.Count > 8)
            {
                throw new ArgumentException("serviceEndpoints must not contain more than 8 entries");
            }

            foreach (Endpoint endpoint in serviceEndpoints)
            {
                Endpoint.ValidateNoIpAndDomain(endpoint);
            }

            serviceEndpoints = new List<Endpoint>(serviceEndpoints);
            return this;
        }

        /// <summary>
        /// Add an endpoint for gRPC calls to the list of service endpoints for gRPC calls.
        /// </summary>
        /// <param name="serviceEndpoint">endpoints for gRPC calls to add.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction AddServiceEndpoint(Endpoint serviceEndpoint)
        {
            RequireNotFrozen();
            if (serviceEndpoints.Count >= 8)
            {
                throw new ArgumentException("serviceEndpoints must not contain more than 8 entries");
            }

            Endpoint.ValidateNoIpAndDomain(serviceEndpoint);
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
        /// This value MUST be the DER encoding of the certificate presented.<br/>
        /// This field is REQUIRED and MUST NOT be empty.
        /// </summary>
        /// <param name="gossipCaCertificate">the DER encoding of the certificate presented.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction SetGossipCaCertificate(byte[] gossipCaCertificate)
        {
            RequireNotFrozen();
            if (gossipCaCertificate != null)
            {
                if (gossipCaCertificate.Length == 0)
                {
                    throw new ArgumentException("gossipCaCertificate must not be empty");
                }
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
        /// This value MUST be an SHA-384 hash.<br/>
        /// The TLS certificate to be hashed MUST first be in PEM format and MUST be
        /// encoded with UTF-8 NFKD encoding to a stream of bytes provided to
        /// the hash algorithm.<br/>
        /// This field is OPTIONAL.
        /// </summary>
        /// <param name="grpcCertificateHash">SHA-384 hash of the node gRPC TLS certificate.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction SetGrpcCertificateHash(byte[] grpcCertificateHash)
        {
            RequireNotFrozen();
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
        /// This key MUST sign this transaction.<br/>
        /// This key MUST sign each transaction to update this node.<br/>
        /// This field MUST contain a valid `Key` value.<br/>
        /// This field is REQUIRED and MUST NOT be set to an empty `KeyList`.
        /// </summary>
        /// <param name="adminKey">an administrative key to be set.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction SetAdminKey(Key adminKey)
        {
            RequireNotFrozen();
            adminKey = adminKey;
            return this;
        }

        /// <summary>
        /// Gets whether this node declines rewards.
        /// If null, the default behavior is to accept rewards.
        /// </summary>
        /// <returns>true if rewards are declined; false if accepted; null if unset.</returns>
        public virtual bool GetDeclineReward()
        {
            return declineReward;
        }

        /// <summary>
        /// Sets whether this node should decline rewards.
        /// </summary>
        /// <param name="decline">true to decline rewards, false to accept.</param>
        /// <returns>{@code this}</returns>
        public virtual NodeCreateTransaction SetDeclineReward(bool decline)
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
        public virtual NodeCreateTransaction SetGrpcWebProxyEndpoint(Endpoint grpcWebProxyEndpoint)
        {
            RequireNotFrozen();
            grpcWebProxyEndpoint = grpcWebProxyEndpoint;
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.NodeCreateTransactionBody}</returns>
        public virtual NodeCreateTransactionBody  Build()
        {
            var builder = new NodeCreateTransactionBody();
            if (accountId != null)
            {
                builder.AccountId = accountId.ToProtobuf();
            }

            builder.Description = description;

            // If gossip endpoints include FQDN but the network forbids it, prefer using an available IP
            // from service endpoints. We rewrite such gossip endpoints to use the first available service IP.
            byte[] fallbackServiceIp = null;
            foreach (Endpoint serviceEndpoint in serviceEndpoints)
            {
                if (serviceEndpoint.Address != null)
                {
                    fallbackServiceIp = serviceEndpoint.Address.CopyArray();
                    break;
                }
            }

            foreach (Endpoint gossipEndpoint in gossipEndpoints)
            {
                bool hasFqdn = gossipEndpoint.DomainName != null && gossipEndpoint.DomainName.Length != 0;
                bool hasIp = gossipEndpoint.Address != null;
                if (!hasIp && hasFqdn && fallbackServiceIp != null)
                {

                    // rewrite to IP-only endpoint preserving the port
                    Endpoint rewritten = new Endpoint()
                    {
						Port = gossipEndpoint.Port,
						Address = fallbackServiceIp.CopyArray(),
					};

                    builder.GossipEndpoint.Add(rewritten.ToProtobuf());
                }
                else
                {
                    builder.GossipEndpoint.Add(gossipEndpoint.ToProtobuf());
				}
            }

            foreach (Endpoint serviceEndpoint in serviceEndpoints)
            {
                builder.ServiceEndpoint.Add(serviceEndpoint.ToProtobuf());
            }

            if (gossipCaCertificate != null)
            {
                builder.GossipCaCertificate = ByteString.CopyFrom(gossipCaCertificate);
            }

            if (grpcCertificateHash != null)
            {
                builder.GrpcCertificateHash = ByteString.CopyFrom(grpcCertificateHash);
            }

            if (adminKey != null)
            {
                builder.AdminKey = adminKey.ToProtobufKey();
            }

            if (declineReward != null)
            {
                builder.DeclineReward = declineReward;
            }

            if (grpcWebProxyEndpoint != null)
            {
                builder.GrpcProxyEndpoint = grpcWebProxyEndpoint.ToProtobuf();
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.NodeCreate;

            if (body.AccountId is not null)
            {
                accountId = AccountId.FromProtobuf(body.AccountId);
            }

            description = body.Description;
            foreach (var gossipEndpoint in body.GossipEndpoint)
            {
                gossipEndpoints.Add(Endpoint.FromProtobuf(gossipEndpoint));
            }

            foreach (var serviceEndpoint in body.ServiceEndpoint)
            {
                serviceEndpoints.Add(Endpoint.FromProtobuf(serviceEndpoint));
            }

            var protobufGossipCert = body.GossipCaCertificate;
            gossipCaCertificate = protobufGossipCert.Equals(ByteString.Empty) ? null : protobufGossipCert.ToByteArray();
            var protobufGrpcCert = body.GrpcCertificateHash;
            grpcCertificateHash = protobufGrpcCert.Equals(ByteString.Empty) ? null : protobufGrpcCert.ToByteArray();
            if (body.AdminKey is not null)
            {
                adminKey = Key.FromProtobufKey(body.AdminKey);
            }

            declineReward = body.DeclineReward;
            if (body.GrpcProxyEndpoint is not null)
            {
                grpcWebProxyEndpoint = Endpoint.FromProtobuf(body.GrpcProxyEndpoint);
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
            return AddressBookServiceGrpc.GetCreateNodeMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.NodeCreate = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.NodeCreate = Build();
        }
    }
}