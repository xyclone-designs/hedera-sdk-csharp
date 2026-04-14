// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <include file="NodeAddress.cs.xml" path='docs/member[@name="T:NodeAddress"]/*' />
    public class NodeAddress : ICloneable
    {
        /// <include file="NodeAddress.cs.xml" path='docs/member[@name="M:NodeAddress.FromProtobuf(Proto.Services.NodeAddress)"]/*' />
        public static NodeAddress FromProtobuf(Proto.Services.NodeAddress nodeAddress)
        {
			return new NodeAddress
			{
				PublicKey = nodeAddress.RSAPubKey,
				NodeId = nodeAddress.NodeId,
				CertHash = nodeAddress.NodeCertHash,
				Addresses =
				[
					.. nodeAddress.ServiceEndpoint
						.Select(_ => Endpoint.FromProtobuf(_))
						.Prepend(nodeAddress.IpAddress.Length == 0 ? null : Endpoint.FromProtobuf(new Proto.Services.ServiceEndpoint
						{
							IpAddressV4 = nodeAddress.IpAddress,
							Port = nodeAddress.Portno,
						
						})).OfType<Endpoint>()
				],
				Description = nodeAddress.Description,
				Stake = nodeAddress.Stake,
				AccountId = AccountId.FromProtobuf(nodeAddress.NodeAccountId)
			};
        }

		/// <include file="NodeAddress.cs.xml" path='docs/member[@name="P:NodeAddress.PublicKey"]/*' />
		public virtual string? PublicKey { set; get; }
		/// <include file="NodeAddress.cs.xml" path='docs/member[@name="P:NodeAddress.Description"]/*' />
		public virtual string? Description { set; get; }
		/// <include file="NodeAddress.cs.xml" path='docs/member[@name="P:NodeAddress.AccountId"]/*' />
		public virtual AccountId? AccountId { set; get; }
		/// <include file="NodeAddress.cs.xml" path='docs/member[@name="P:NodeAddress.Stake"]/*' />
		public virtual long Stake { set; get; }
		/// <include file="NodeAddress.cs.xml" path='docs/member[@name="P:NodeAddress.NodeId"]/*' />
		public virtual long NodeId { set; get; }
		/// <include file="NodeAddress.cs.xml" path='docs/member[@name="P:NodeAddress.CertHash"]/*' />
		public virtual ByteString? CertHash { set; get; }
		/// <include file="NodeAddress.cs.xml" path='docs/member[@name="P:NodeAddress.Addresses"]/*' />
		public virtual List<Endpoint> Addresses { set; get; } = [];

		public virtual object Clone()
		{
			return new NodeAddress
			{
				PublicKey = (string?)PublicKey?.Clone(),
				NodeId = NodeId,
				CertHash = CertHash?.Copy(),
				Addresses = Addresses.CloneToList(),
				Description = (string?)Description?.Clone(),
				Stake = Stake,
				AccountId = (AccountId?)AccountId?.Clone()
			};
		}
		public virtual Proto.Services.NodeAddress ToProtobuf()
        {
            Proto.Services.NodeAddress proto = new()
            {
				NodeId = NodeId
			};

            if (CertHash != null)
                proto.NodeCertHash = CertHash;

            if (PublicKey != null)
                proto.RSAPubKey = PublicKey;

            if (AccountId != null)
                proto.NodeAccountId = AccountId.ToProtobuf();

            if (Description != null)
                proto.Description = Description;

            proto.ServiceEndpoint.AddRange(Addresses.Select(_ => _.ToProtobuf()));

			return proto;
        }
    }
}
