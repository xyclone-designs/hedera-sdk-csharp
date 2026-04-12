// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="T:NodeUpdateTransaction"]/*' />
	public class NodeUpdateTransaction : Transaction<NodeUpdateTransaction>
    {
        private List<Endpoint> _GossipEndpoints = [];
        private List<Endpoint> _ServiceEndpoints = [];

		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.#ctor"]/*' />
		public NodeUpdateTransaction() { }
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal NodeUpdateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal NodeUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen"]/*' />
		public ulong? NodeId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_2"]/*' />
		public AccountId? AccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_3"]/*' />
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
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_4"]/*' />
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
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_5"]/*' />
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
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_6"]/*' />
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
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_7"]/*' />
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
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_8"]/*' />
		public Key? AdminKey
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_9"]/*' />
		public bool? DeclineReward
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.RequireNotFrozen_10"]/*' />
		public Endpoint? GrpcWebProxyEndpoint
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
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
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.ToProtobuf"]/*' />
		public virtual Proto.Services.NodeUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.NodeUpdateTransactionBody
			{
				DeclineReward = DeclineReward,
			};

			if (NodeId != null)
				builder.NodeId = NodeId.Value;

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
		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.DeleteGrpcWebProxyEndpoint"]/*' />
		public virtual NodeUpdateTransaction DeleteGrpcWebProxyEndpoint()
		{
			RequireNotFrozen();
			GrpcWebProxyEndpoint = new Endpoint();
			return this;
		} // validation moved to Endpoint.validateNoIpAndDomain

		/// <include file="NodeUpdateTransaction.cs.xml" path='docs/member[@name="M:NodeUpdateTransaction.FreezeWith(Client)"]/*' />
		public override NodeUpdateTransaction FreezeWith(Client? client)
		{
			if (NodeId == null)
				throw new InvalidOperationException("NodeUpdateTransaction: 'nodeId' must be explicitly set before calling freeze().");

			return base.FreezeWith(client);
		}
		public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.NodeUpdate = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.NodeUpdate = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.AddressBookService.AddressBookServiceClient.updateNode);

			return Proto.Services.AddressBookService.Descriptor.FindMethodByName(methodname);
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
