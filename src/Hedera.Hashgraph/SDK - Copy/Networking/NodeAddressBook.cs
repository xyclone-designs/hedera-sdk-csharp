// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <summary>
	/// A list of nodes and their metadata.
	/// 
	/// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/nodeaddressbook">Hedera Documentation</a>
	/// </summary>
	public class NodeAddressBook
    {
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
		/// Create a node address book from a protobuf.
		/// </summary>
		/// <param name="book">the protobuf</param>
		/// <returns>                         the new node address book</returns>
		public static NodeAddressBook FromProtobuf(Proto.NodeAddressBook book)
		{
			var addresses = new List<NodeAddress>(book.NodeAddress.Count);
			foreach (var address in book.NodeAddress)
			{
				addresses.Add(NodeAddress.FromProtobuf(address));
			}

			return new NodeAddressBook
			{
				NodeAddresses = addresses
			};
		}
		public static IList<NodeAddress> CloneNodeAddresses(IList<NodeAddress> addresses)
		{
			List<NodeAddress> cloneAddresses = new(addresses.Count);

			foreach (var address in addresses)
				cloneAddresses.Add((NodeAddress)address.Clone());

			return cloneAddresses;
		}

		/// <summary>
		/// Extract the of node addresses.
		/// </summary>
		/// <returns>                         list of node addresses</returns>
		public virtual IList<NodeAddress> NodeAddresses
        {
            get => CloneNodeAddresses(field);
            set => field = CloneNodeAddresses(value);

        } = [];

		/// <summary>
		/// Create the byte string.
		/// </summary>
		/// <returns>                         the byte string representation</returns>
		public virtual ByteString ToBytes()
		{
			return ToProtobuf().ToByteString();
		}
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public virtual Proto.NodeAddressBook ToProtobuf()
        {
			Proto.NodeAddressBook proto = new ();

            foreach (var nodeAdress in NodeAddresses)
				proto.NodeAddress.Add(nodeAdress.ToProtobuf());

			return proto;
        }
    }
}