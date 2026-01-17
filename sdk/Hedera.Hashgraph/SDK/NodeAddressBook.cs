namespace Hedera.Hashgraph.SDK
{
	/**
 * A list of nodes and their metadata.
 *
 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/nodeaddressbook">Hedera Documentation</a>
 */
	public class NodeAddressBook
	{
		List<NodeAddress> nodeAddresses = Collections.emptyList();

		/**
		 * Constructor.
		 */
		NodeAddressBook() { }

		/**
		 * Extract the of node addresses.
		 *
		 * @return                          list of node addresses
		 */
		public List<NodeAddress> getNodeAddresses()
		{
			return cloneNodeAddresses(nodeAddresses);
		}

		/**
		 * Assign the list of node addresses.
		 *
		 * @param nodeAddresses             list of node addresses
		 * @return {@code this}
		 */
		public NodeAddressBook setNodeAddresses(List<NodeAddress> nodeAddresses)
		{
			this.nodeAddresses = cloneNodeAddresses(nodeAddresses);
			return this;
		}

		static List<NodeAddress> cloneNodeAddresses(List<NodeAddress> addresses)
		{
			List<NodeAddress> cloneAddresses = new ArrayList<>(addresses.size());
			for (var address : addresses)
			{
				cloneAddresses.Add(address.clone());
			}
			return cloneAddresses;
		}

		/**
		 * Create a node address book from a protobuf.
		 *
		 * @param book                      the protobuf
		 * @return                          the new node address book
		 */
		static NodeAddressBook FromProtobuf(Proto.NodeAddressBook book)
		{
			var addresses = new ArrayList<NodeAddress>(book.getNodeAddressCount());

			for (var address : book.getNodeAddressList())
			{
				addresses.Add(NodeAddress.FromProtobuf(address));
			}

			return new NodeAddressBook().setNodeAddresses(addresses);
		}

		/**
		 * Create a node address book from a byte string.
		 *
		 * @param bytes                     the byte string
		 * @return                          the new node address book
		 * @       when there is an issue with the protobuf
		 */
		public static NodeAddressBook FromBytes(ByteString bytes) 
		{
        return FromProtobuf(Proto.NodeAddressBook.Parser.ParseFrom(bytes));
    }

    /**
     * Create the protobuf.
     *
     * @return                          the protobuf representation
     */
    Proto.NodeAddressBook ToProtobuf()
		{
			var builder = Proto.NodeAddressBook.newBuilder();

			for (var nodeAdress : nodeAddresses)
			{
				builder.AddNodeAddress(nodeAdress.ToProtobuf());
			}

			return builder.build();
		}

		/**
		 * Create the byte string.
		 *
		 * @return                          the byte string representation
		 */
		public ByteString toBytes()
		{
			return ToProtobuf().toByteString();
		}


	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("nodeAddresses", nodeAddresses)
				.toString();
	}
}

}