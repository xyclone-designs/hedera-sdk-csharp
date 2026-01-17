namespace Hedera.Hashgraph.SDK
{
	/**
 * Internal utility class.
 */
	public class NetworkVersionInfo
	{
		/**
		 * Version of the protobuf schema in use by the network
		 */
		public SemanticVersion ProtobufVersion { get; }

		/**
		 * Version of the Hedera services in use by the network
		 */
		public SemanticVersion ServicesVersion { get; }

		/**
		* Constructor.
		*
		* @param hapi                      the protobuf version
		* @param hedera                    the hedera version
		*/
		NetworkVersionInfo(SemanticVersion hapi, SemanticVersion hedera)
		{
			ProtobufVersion = hapi;
			ServicesVersion = hedera;
		}

		/**
		 * Create a network version info object from a protobuf.
		 *
		 * @param proto                     the protobuf
		 * @return                          the new network version object
		 */
		protected static NetworkVersionInfo FromProtobuf(NetworkGetVersionInfoResponse proto)
		{
			return new NetworkVersionInfo(
					SemanticVersion.FromProtobuf(proto.getHapiProtoVersion()),
					SemanticVersion.FromProtobuf(proto.getHederaServicesVersion()));
		}

		/**
		 * Create a network version info object from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new network version object
		 * @       when there is an issue with the protobuf
		 */
		public static NetworkVersionInfo FromBytes(byte[] bytes) 
		{
			return FromProtobuf(NetworkGetVersionInfoResponse.Parser.ParseFrom(bytes));
		}

		/**
		 * Create the protobuf.
		 *
		 * @return                          the protobuf representation
		 */
		protected NetworkGetVersionInfoResponse ToProtobuf()
		{
			return NetworkGetVersionInfoResponse.newBuilder()
					.setHapiProtoVersion(protobufVersion.ToProtobuf())
					.setHederaServicesVersion(servicesVersion.ToProtobuf())
					.build();
		}

		/**
		 * Create the byte array.
		 *
		 * @return                          the byte array representation
		 */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
	}
}