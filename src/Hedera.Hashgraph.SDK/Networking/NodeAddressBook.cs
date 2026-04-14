// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <include file="NodeAddressBook.cs.xml" path='docs/member[@name="T:NodeAddressBook"]/*' />
	public class NodeAddressBook
    {
		/// <include file="NodeAddressBook.cs.xml" path='docs/member[@name="M:NodeAddressBook.FromBytes(ByteString)"]/*' />
		public static NodeAddressBook FromBytes(ByteString bytes)
		{
			return FromProtobuf(Proto.Services.NodeAddressBook.Parser.ParseFrom(bytes));
		}
		/// <include file="NodeAddressBook.cs.xml" path='docs/member[@name="M:NodeAddressBook.FromProtobuf(Proto.Services.NodeAddressBook)"]/*' />
		public static NodeAddressBook FromProtobuf(Proto.Services.NodeAddressBook book)
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
		public static List<NodeAddress> CloneNodeAddresses(IList<NodeAddress> addresses)
		{
			List<NodeAddress> cloneAddresses = new(addresses.Count);

			foreach (var address in addresses)
				cloneAddresses.Add((NodeAddress)address.Clone());

			return cloneAddresses;
		}

		/// <include file="NodeAddressBook.cs.xml" path='docs/member[@name="M:NodeAddressBook.CloneNodeAddresses(field)"]/*' />
		public virtual List<NodeAddress> NodeAddresses
        {
            get => CloneNodeAddresses(field);
            set => field = CloneNodeAddresses(value);

        } = [];

		/// <include file="NodeAddressBook.cs.xml" path='docs/member[@name="M:NodeAddressBook.ToBytes"]/*' />
		public virtual ByteString ToBytes()
		{
			return ToProtobuf().ToByteString();
		}
		/// <include file="NodeAddressBook.cs.xml" path='docs/member[@name="M:NodeAddressBook.ToProtobuf"]/*' />
		public virtual Proto.Services.NodeAddressBook ToProtobuf()
        {
			Proto.Services.NodeAddressBook proto = new ();

            foreach (var nodeAdress in NodeAddresses)
                proto.NodeAddress.Add(nodeAdress.ToProtobuf());

			return proto;
        }
    }
}
