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
        private IList<Endpoint> _GossipEndpoints = [];
        private IList<Endpoint> _ServiceEndpoints = [];

		/// <summary>
		/// Constructor.
		/// </summary>
		public NodeUpdateTransaction()
        {
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal NodeUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal NodeUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
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
		/// <param name="nodeId">the consensus node identifier in the network state.</param>
		/// <returns>{@code this}</returns>
		/// <exception cref="IllegalArgumentException">if nodeId is negative</exception>
		public ulong NodeId
		{
			get;
			set
			{
				RequireNotFrozen();
				if (value < 0) throw new ArgumentException("nodeId must be non-negative");
				field = value;
			}
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
		/// If set, this value SHALL replace the previous value.
		/// </summary>
		/// <param name="description">The String to be set as the description of the node.</param>
		/// <returns>{@code this}</returns>
		/// <exception cref="IllegalArgumentException">if description exceeds 100 bytes when encoded as UTF-8</exception>
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
		/// This value MUST be the DER encoding of the certificate presented.
		/// <p>
		/// If set, the new value SHALL replace the existing bytes value.
		/// </summary>
		/// <param name="gossipCaCertificate">the DER encoding of the certificate presented.</param>
		/// <returns>{@code this}</returns>
		/// <exception cref="IllegalArgumentException">if gossipCaCertificate is null or empty</exception>
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
		/// This field is OPTIONAL.<br/>
		/// If set, this key MUST sign this transaction.<br/>
		/// If set, this key MUST sign each subsequent transaction to
		/// update this node.<br/>
		/// If set, this field MUST contain a valid `Key` value.<br/>
		/// If set, this field MUST NOT be set to an empty `KeyList`.
		/// </summary>
		/// <param name="adminKey">an administrative key to be set.</param>
		/// <returns>{@code this}</returns>
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
		/// <param name="decline">true to decline rewards; false to accept them. If left null no change will be made.</param>
		/// <returns>{@code this}</returns>
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
		/// Initialize from the transaction body.
		/// </summary>
		public virtual void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.NodeUpdate;

			NodeId = body.NodeId;

			if (body.AccountId is not null)
			    AccountId = AccountId.FromProtobuf(body.AccountId);

			if (body.Description is not null)
			    Description = body.Description;

			foreach (var gossipEndpoint in body.GossipEndpoint)
			    GossipEndpoints.Add(Endpoint.FromProtobuf(gossipEndpoint));

			foreach (var serviceEndpoint in body.ServiceEndpoint)
			    ServiceEndpoints.Add(Endpoint.FromProtobuf(serviceEndpoint));

			if (body.GossipCaCertificate is not null)
			    GossipCaCertificate = body.GossipCaCertificate.ToByteArray();

			if (body.GrpcCertificateHash is not null)
			    GrpcCertificateHash = body.GrpcCertificateHash.ToByteArray();

			if (body.AdminKey is not null)
			    AdminKey = Key.FromProtobufKey(body.AdminKey);

			if (body.DeclineReward is not null)
			    DeclineReward = body.DeclineReward.Value;

			if (body.GrpcProxyEndpoint is not null)
			    GrpcWebProxyEndpoint = Endpoint.FromProtobuf(body.GrpcProxyEndpoint);
		}
		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.NodeUpdateTransactionBody}</returns>
		public virtual Proto.NodeUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.NodeUpdateTransactionBody
			{
				NodeId = NodeId,
				DeclineReward = DeclineReward,
			};

			if (AccountId != null)
				builder.AccountId = AccountId.ToProtobuf();

            if (Description != null)
				builder.Description = Description;

            foreach (Endpoint gossipEndpoint in GossipEndpoints)
				builder.GossipEndpoint.Add(gossipEndpoint.ToProtobuf());

            foreach (Endpoint serviceEndpoint in ServiceEndpoints)
				builder.ServiceEndpoint.Add(serviceEndpoint.ToProtobuf());

            if (GossipCaCertificate != null)
				builder.GossipCaCertificate = ByteString.CopyFrom(GossipCaCertificate);

            if (GrpcCertificateHash != null)
				builder.GrpcCertificateHash = ByteString.CopyFrom(GrpcCertificateHash);

            if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

			if (GrpcWebProxyEndpoint != null)
				builder.GrpcProxyEndpoint = GrpcWebProxyEndpoint.ToProtobuf();

			return builder;
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
			GrpcWebProxyEndpoint = new Endpoint();
			return this;
		} // validation moved to Endpoint.validateNoIpAndDomain

		/// <summary>
		/// Freeze this transaction with the given client.
		/// </summary>
		/// <param name="client">the client to freeze with</param>
		/// <returns>this transaction</returns>
		/// <exception cref="IllegalStateException">if nodeId is not set</exception>
		public override NodeUpdateTransaction FreezeWith(Client client)
		{
			if (NodeId == null)
				throw new InvalidOperationException("NodeUpdateTransaction: 'nodeId' must be explicitly set before calling freeze().");

			return base.FreezeWith(client);
		}
		public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.NodeUpdate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.NodeUpdate = ToProtobuf();
        }
		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return AddressBookServiceGrpc.GetUpdateNodeMethod();
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