// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Hedera.Hashgraph.SDK.Networking
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
		private IList<Endpoint> _GossipEndpoints = [];
		private IList<Endpoint> _ServiceEndpoints = [];

		/// <summary>
		/// Constructor.
		/// </summary>
		public NodeCreateTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal NodeCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal NodeCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
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
		public AccountId? AccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// A short description of the node.
		/// <p>
		/// This value, if set, MUST NOT exceed 100 bytes when encoded as UTF-8.<br/>
		/// This field is OPTIONAL.
		/// </summary>
		/// <param name="description">The String to be set as the description of the node.</param>
		/// <returns>{@code this}</returns>
		public string? Description
		{
			get;
			set
			{
				RequireNotFrozen();
				if (value != null && Encoding.UTF8.GetByteCount(value) > 100)
					throw new ArgumentException("Description must not exceed 100 bytes when encoded as UTF-8");
				field = value;
			}
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
		public IList<Endpoint> GossipEndpoints
		{
			get { RequireNotFrozen(); return _GossipEndpoints; }
			set
			{
				if (value.Count == 0)
				{
					throw new ArgumentException("Gossip endpoints list must not be empty");
				}

				if (value.Count > 10)
				{
					throw new ArgumentException("Gossip endpoints list must not contain more than 10 entries");
				}

				foreach (Endpoint endpoint in value)
				{
					Endpoint.ValidateNoIpAndDomain(endpoint);
				}

				_GossipEndpoints = [.. value];
			}
		}
		public IReadOnlyList<Endpoint> GossipEndpoints_Read => _GossipEndpoints.AsReadOnly();
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
		public IList<Endpoint> ServiceEndpoints
		{
			get { RequireNotFrozen(); return _ServiceEndpoints; }
			set
			{
				if (value.Count == 0)
				{
					throw new ArgumentException("Service endpoints list must not be empty");
				}

				if (value.Count > 8)
				{
					throw new ArgumentException("Service endpoints list must not contain more than 8 entries");
				}

				foreach (Endpoint endpoint in _ServiceEndpoints)
				{
					Endpoint.ValidateNoIpAndDomain(endpoint);
				}

				_ServiceEndpoints = [.. value];
			}
		}
		public IReadOnlyList<Endpoint> ServiceEndpoints_Read => ServiceEndpoints.AsReadOnly();
		/// <summary>
		/// A certificate used to sign gossip events.
		/// <p>
		/// This value MUST be a certificate of a type permitted for gossip
		/// signatures.<br/>
		/// This value MUST be the DER encoding of the certificate presented.<br/>
		/// This field is REQUIRED and MUST NOT be empty.
		/// </summary>
		public byte[]? GossipCaCertificate
		{
			get;
			set
			{
				RequireNotFrozen();
				if (value == null || value.Length == 0)
				{
					throw new ArgumentException("Gossip CA certificate must not be null or empty");
				}
				field = value;
			}
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
		public byte[]? GrpcCertificateHash
		{
			get;
			set
			{
				RequireNotFrozen();
				if (value != null && value.Length > 0 && value.Length != 48)
				{
					throw new ArgumentException("gRPC certificate hash must be exactly 48 bytes (SHA-384)");
				}
				field = value;
			}
		}
		/// <summary>
		/// An administrative key controlled by the node operator.
		/// <p>
		/// This key MUST sign this transaction.<br/>
		/// This key MUST sign each transaction to update this node.<br/>
		/// This field MUST contain a valid `Key` value.<br/>
		/// This field is REQUIRED and MUST NOT be set to an empty `KeyList`.
		/// </summary>
		public Key? AdminKey
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Sets whether this node should decline rewards.
		/// </summary>
		public bool? DeclineReward
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
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
		public Endpoint? GrpcWebProxyEndpoint
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.NodeCreateTransactionBody}</returns>
		public virtual Proto.NodeCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.NodeCreateTransactionBody();

            if (AccountId != null)
				builder.AccountId = AccountId.ToProtobuf();

			builder.Description = Description;

            // If gossip endpoints include FQDN but the network forbids it, prefer using an available IP
            // from service endpoints. We rewrite such gossip endpoints to use the first available service IP.
            byte[]? fallbackServiceIp = null;

            foreach (Endpoint serviceEndpoint in ServiceEndpoints)
            {
                if (serviceEndpoint.Address != null)
                {
                    fallbackServiceIp = serviceEndpoint.Address.CopyArray();
                    break;
                }
            }

            foreach (Endpoint gossipEndpoint in GossipEndpoints)
            {
                bool hasFqdn = gossipEndpoint.DomainName != null && gossipEndpoint.DomainName.Length != 0;
                bool hasIp = gossipEndpoint.Address != null;
                if (!hasIp && hasFqdn && fallbackServiceIp != null)
                {

                    // rewrite to IP-only endpoint preserving the port
                    Endpoint rewritten = new ()
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

            foreach (Endpoint serviceEndpoint in ServiceEndpoints)
				builder.ServiceEndpoint.Add(serviceEndpoint.ToProtobuf());

            if (GossipCaCertificate != null)
				builder.GossipCaCertificate = ByteString.CopyFrom(GossipCaCertificate);

            if (GrpcCertificateHash != null)
				builder.GrpcCertificateHash = ByteString.CopyFrom(GrpcCertificateHash);

            if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

            if (DeclineReward != null)
				builder.DeclineReward = DeclineReward.Value;

            if (GrpcWebProxyEndpoint != null)
				builder.GrpcProxyEndpoint = GrpcWebProxyEndpoint.ToProtobuf();

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.NodeCreate;

            if (body.AccountId is not null)
            {
                AccountId = AccountId.FromProtobuf(body.AccountId);
            }

            Description = body.Description;
            foreach (var gossipEndpoint in body.GossipEndpoint)
            {
                GossipEndpoints.Add(Endpoint.FromProtobuf(gossipEndpoint));
            }

            foreach (var serviceEndpoint in body.ServiceEndpoint)
            {
                ServiceEndpoints.Add(Endpoint.FromProtobuf(serviceEndpoint));
            }

            var protobufGossipCert = body.GossipCaCertificate;
            GossipCaCertificate = protobufGossipCert.Equals(ByteString.Empty) ? null : protobufGossipCert.ToByteArray();
            
			var protobufGrpcCert = body.GrpcCertificateHash;
            GrpcCertificateHash = protobufGrpcCert.Equals(ByteString.Empty) ? null : protobufGrpcCert.ToByteArray();
            
			if (body.AdminKey is not null)
            {
                AdminKey = Key.FromProtobufKey(body.AdminKey);
            }

            DeclineReward = body.DeclineReward;
            
			if (body.GrpcProxyEndpoint is not null)
            {
                GrpcWebProxyEndpoint = Endpoint.FromProtobuf(body.GrpcProxyEndpoint);
            }
        }

        public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.AddressBookService.AddressBookServiceClient.createNode);

			return Proto.AddressBookService.Descriptor.FindMethodByName(methodname);
		}

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.NodeCreate = ToProtobuf();
        }

        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.NodeCreate = ToProtobuf();
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