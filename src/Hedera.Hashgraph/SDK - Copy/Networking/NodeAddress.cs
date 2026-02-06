// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// The metadata for a Node – including IP Address, and the crypto account associated with the Node.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/nodeaddress">Hedera Documentation</a>
    /// </summary>
    public class NodeAddress : ICloneable
    {
        /// <summary>
        /// Create a node from a protobuf.
        /// </summary>
        /// <param name="nodeAddress">the protobuf</param>
        /// <returns>                         the new node</returns>
        public static NodeAddress FromProtobuf(Proto.NodeAddress nodeAddress)
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
						.Prepend(nodeAddress.IpAddress.Length == 0 ? null : Endpoint.FromProtobuf(new Proto.ServiceEndpoint
						{
							IpAddressV4 = nodeAddress.IpAddress,
							Port = nodeAddress.Portno,
						
						})).OfType<Endpoint>()
				],
				Description = nodeAddress.Description,
				Stake = nodeAddress.Stake,
				AccountId = nodeAddress.NodeAccountId is not null
					? AccountId.FromProtobuf(nodeAddress.NodeAccountId)
					: null
			};
        }

		/// <summary>
		/// The RSA public key of the node.
		/// </summary>
		public virtual string? PublicKey { set; get; }
		/// <summary>
		/// A description of the node, with UTF-8 encoding up to 100 bytes.
		/// </summary>
		public virtual string? Description { set; get; }
		/// <summary>
		/// The account to be paid for queries and transactions sent to this node.
		/// </summary>
		public virtual AccountId? AccountId { set; get; }
		/// <summary>
		/// The amount of tinybars staked to the node.
		/// </summary>
		public virtual long Stake { set; get; }
		/// <summary>
		/// A non-sequential identifier for the node.
		/// </summary>
		public virtual long NodeId { set; get; }
		/// <summary>
		/// A hash of the X509 cert used for gRPC traffic to this node.
		/// </summary>
		public virtual ByteString? CertHash { set; get; }
		/// <summary>
		/// A node's service IP addresses and ports.
		/// </summary>
		public virtual IList<Endpoint> Addresses { set; get; } = [];

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
		public virtual Proto.NodeAddress ToProtobuf()
        {
            Proto.NodeAddress proto = new()
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