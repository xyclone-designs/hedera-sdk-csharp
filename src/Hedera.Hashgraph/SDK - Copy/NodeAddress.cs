// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Nio.Charset;
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
        /// The RSA public key of the node.
        /// </summary>
        string publicKey;
        /// <summary>
        /// The account to be paid for queries and transactions sent to this node.
        /// </summary>
        AccountId accountId;
        /// <summary>
        /// A non-sequential identifier for the node.
        /// </summary>
        long nodeId;
        /// <summary>
        /// A hash of the X509 cert used for gRPC traffic to this node.
        /// </summary>
        ByteString certHash;
        /// <summary>
        /// A node's service IP addresses and ports.
        /// </summary>
        IList<Endpoint> addresses = Collections.EmptyList();
        /// <summary>
        /// A description of the node, with UTF-8 encoding up to 100 bytes.
        /// </summary>
        string description = null;
        /// <summary>
        /// The amount of tinybars staked to the node.
        /// </summary>
        long stake;
        /// <summary>
        /// Constructor.
        /// </summary>
        NodeAddress()
        {
        }

        /// <summary>
        /// Create a node from a protobuf.
        /// </summary>
        /// <param name="nodeAddress">the protobuf</param>
        /// <returns>                         the new node</returns>
        static NodeAddress FromProtobuf(Proto.NodeAddress nodeAddress)
        {
            var address = new List<Endpoint>(nodeAddress.GetServiceEndpointCount());
            if (!nodeAddress.GetIpAddress().IsEmpty())
            {
                address.Add(Endpoint.FromProtobuf(ServiceEndpoint.NewBuilder().SetIpAddressV4(nodeAddress.GetIpAddress()).SetPort(nodeAddress.GetPortno()).Build()));
            }

            foreach (var endpoint in nodeAddress.GetServiceEndpointList())
            {
                address.Add(Endpoint.FromProtobuf(endpoint));
            }

            var node = new NodeAddress().SetPublicKey(nodeAddress.GetRSAPubKey()).SetNodeId(nodeAddress.GetNodeId()).SetCertHash(nodeAddress.GetNodeCertHash()).SetAddresses(address).SetDescription(nodeAddress.GetDescription()).SetStake(nodeAddress.GetStake());
            if (nodeAddress.HasNodeAccountId())
            {
                node.SetAccountId(AccountId.FromProtobuf(nodeAddress.GetNodeAccountId()));
            }

            return node;
        }

        /// <summary>
        /// Extract the public key.
        /// </summary>
        /// <returns>                         the public key</returns>
        public virtual string GetPublicKey()
        {
            return publicKey;
        }

        /// <summary>
        /// Assign the public key.
        /// </summary>
        /// <param name="publicKey">the public key</param>
        /// <returns>{@code this}</returns>
        public virtual NodeAddress SetPublicKey(string publicKey)
        {
            publicKey = publicKey;
            return this;
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        public virtual AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// Assign the account id.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>{@code this}</returns>
        public virtual NodeAddress SetAccountId(AccountId accountId)
        {
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the node id.
        /// </summary>
        /// <returns>                        the node id</returns>
        public virtual long GetNodeId()
        {
            return nodeId;
        }

        /// <summary>
        /// Assign the node id.
        /// </summary>
        /// <param name="nodeId">the node id</param>
        /// <returns>{@code this}</returns>
        public virtual NodeAddress SetNodeId(long nodeId)
        {
            nodeId = nodeId;
            return this;
        }

        /// <summary>
        /// Extract the certificate hash.
        /// </summary>
        /// <returns>                         the certificate hash</returns>
        public virtual ByteString GetCertHash()
        {
            return certHash;
        }

        /// <summary>
        /// Assign the certificate hash.
        /// </summary>
        /// <param name="certHash">the certificate hash</param>
        /// <returns>{@code this}</returns>
        public virtual NodeAddress SetCertHash(ByteString certHash)
        {
            certHash = certHash;
            return this;
        }

        /// <summary>
        /// Extract the list of addresses.
        /// </summary>
        /// <returns>                         the list of addresses</returns>
        public virtual IList<Endpoint> GetAddresses()
        {
            return CloneEndpoints(addresses);
        }

        /// <summary>
        /// Assign the list of addresses.
        /// </summary>
        /// <param name="addresses">the list of addresses</param>
        /// <returns>{@code this}</returns>
        public virtual NodeAddress SetAddresses(IList<Endpoint> addresses)
        {
            addresses = CloneEndpoints(addresses);
            return this;
        }

        static IList<Endpoint> CloneEndpoints(IList<Endpoint> endpoints)
        {
            IList<Endpoint> cloneEndpoints = new List(endpoints.Count);
            foreach (var endpoint in endpoints)
            {
                cloneEndpoints.Add(endpoint.Clone());
            }

            return cloneEndpoints;
        }

        /// <summary>
        /// Extract the description.
        /// </summary>
        /// <returns>                         the description</returns>
        public virtual string GetDescription()
        {
            return description;
        }

        /// <summary>
        /// Assign the description.
        /// </summary>
        /// <param name="description">the description</param>
        /// <returns>{@code this}</returns>
        public virtual NodeAddress SetDescription(string description)
        {
            description = description;
            return this;
        }

        /// <summary>
        /// Extract the tiny stake.
        /// </summary>
        /// <returns>                         the tiny stake</returns>
        public virtual long GetStake()
        {
            return stake;
        }

        /// <summary>
        /// Assign the tiny bar stake.
        /// </summary>
        /// <param name="stake">the tiny bar stake</param>
        /// <returns>{@code this}</returns>
        public virtual NodeAddress SetStake(long stake)
        {
            stake = stake;
            return this;
        }

        /// <summary>
        /// Convert the node address object into a protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation.</returns>
        virtual Proto.NodeAddress ToProtobuf()
        {
            var builder = Proto.NodeAddress.NewBuilder().SetNodeId(nodeId);
            if (certHash != null)
            {
                builder.SetNodeCertHash(certHash);
            }

            if (publicKey != null)
            {
                builder.SetRSAPubKey(publicKey);
            }

            if (accountId != null)
            {
                builder.SetNodeAccountId(accountId.ToProtobuf());
            }

            if (description != null)
            {
                builder.SetDescription(description);
            }

            foreach (var address in addresses)
            {
                builder.AddServiceEndpoint(address.ToProtobuf());
            }

            return proto;
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("publicKey", publicKey).Add("accountId", accountId).Add("nodeId", nodeId).Add("certHash", certHash != null ? new string (certHash.ToByteArray(), StandardCharsets.UTF_8) : null).Add("addresses", addresses).Add("description", description).Add("stake", stake).ToString();
        }

        public virtual NodeAddress Clone()
        {
            try
            {
                NodeAddress clone = (NodeAddress)base.Clone();
                clone.addresses = CloneEndpoints(addresses);
                return clone;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException();
            }
        }
    }
}