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
	/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="T:NodeCreateTransaction"]/*' />
	public class NodeCreateTransaction : Transaction<NodeCreateTransaction>
    {
		private List<Endpoint> _GossipEndpoints = [];
		private List<Endpoint> _ServiceEndpoints = [];

		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.#ctor"]/*' />
		public NodeCreateTransaction() { }
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal NodeCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal NodeCreateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen"]/*' />
		public AccountId? AccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen_2"]/*' />
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
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen_3"]/*' />
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
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen_4"]/*' />
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
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen_5"]/*' />
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
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen_6"]/*' />
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
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen_7"]/*' />
		public Key? AdminKey
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen_8"]/*' />
		public bool? DeclineReward
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.RequireNotFrozen_9"]/*' />
		public Endpoint? GrpcWebProxyEndpoint
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
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
		/// <include file="NodeCreateTransaction.cs.xml" path='docs/member[@name="M:NodeCreateTransaction.ToProtobuf"]/*' />
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

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.AddressBookService.AddressBookServiceClient.createNode);

			return Proto.AddressBookService.Descriptor.FindMethodByName(methodname);
		}
		public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
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
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}