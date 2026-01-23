// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Java.Util;
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
    /// A list of nodes and their metadata.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/nodeaddressbook">Hedera Documentation</a>
    /// </summary>
    public class NodeAddressBook
    {
        IList<NodeAddress> nodeAddresses = Collections.EmptyList();
        /// <summary>
        /// Constructor.
        /// </summary>
        NodeAddressBook()
        {
        }

        /// <summary>
        /// Extract the of node addresses.
        /// </summary>
        /// <returns>                         list of node addresses</returns>
        public virtual IList<NodeAddress> GetNodeAddresses()
        {
            return CloneNodeAddresses(nodeAddresses);
        }

        /// <summary>
        /// Assign the list of node addresses.
        /// </summary>
        /// <param name="nodeAddresses">list of node addresses</param>
        /// <returns>{@code this}</returns>
        public virtual NodeAddressBook SetNodeAddresses(IList<NodeAddress> nodeAddresses)
        {
            nodeAddresses = CloneNodeAddresses(nodeAddresses);
            return this;
        }

        static IList<NodeAddress> CloneNodeAddresses(IList<NodeAddress> addresses)
        {
            IList<NodeAddress> cloneAddresses = new List(addresses.Count);
            foreach (var address in addresses)
            {
                cloneAddresses.Add(address.Clone());
            }

            return cloneAddresses;
        }

        /// <summary>
        /// Create a node address book from a protobuf.
        /// </summary>
        /// <param name="book">the protobuf</param>
        /// <returns>                         the new node address book</returns>
        static NodeAddressBook FromProtobuf(Proto.NodeAddressBook book)
        {
			book.NodeAddress
			var addresses = new List<NodeAddress>(book.NodeAddress.COu);
            foreach (var address in book.NodeAddressList)
            {
                addresses.Add(NodeAddress.FromProtobuf(address));
            }

            return new NodeAddressBook().SetNodeAddresses(addresses);
        }

        /// <summary>
        /// Create a node address book from a byte string.
        /// </summary>
        /// <param name="bytes">the byte string</param>
        /// <returns>                         the new node address book</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static NodeAddressBook FromBytes(ByteString bytes)
        {
            return FromProtobuf(Proto.NodeAddressBook.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        virtual Proto.NodeAddressBook ToProtobuf()
        {
            var builder = Proto.NodeAddressBook.NewBuilder();
            foreach (var nodeAdress in nodeAddresses)
            {
                builder.AddNodeAddress(nodeAdress.ToProtobuf());
            }

            return proto;
        }

        /// <summary>
        /// Create the byte string.
        /// </summary>
        /// <returns>                         the byte string representation</returns>
        public virtual ByteString ToBytes()
        {
            return ToProtobuf().ToByteString();
        }
    }
}