// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal utility class.
    /// </summary>
    public class NetworkVersionInfo
    {
        /// <summary>
        /// Version of the protobuf schema in use by the network
        /// </summary>
        public readonly SemanticVersion ProtobufVersion;
        /// <summary>
        /// Version of the Hedera services in use by the network
        /// </summary>
        public readonly SemanticVersion ServicesVersion;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="hapi">the protobuf version</param>
        /// <param name="hedera">the hedera version</param>
        NetworkVersionInfo(SemanticVersion hapi, SemanticVersion hedera)
        {
            ProtobufVersion = hapi;
            ServicesVersion = hedera;
        }

        /// <summary>
        /// Create a network version info object from a protobuf.
        /// </summary>
        /// <param name="proto">the protobuf</param>
        /// <returns>                         the new network version object</returns>
        protected static NetworkVersionInfo FromProtobuf(Proto.NetworkGetVersionInfoResponse proto)
        {
            return new NetworkVersionInfo(
                SemanticVersion.FromProtobuf(proto.HapiProtoVersion), 
                SemanticVersion.FromProtobuf(proto.HederaServicesVersion));
        }

        /// <summary>
        /// Create a network version info object from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new network version object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static NetworkVersionInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.NetworkGetVersionInfoResponse.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        protected virtual Proto.NetworkGetVersionInfoResponse ToProtobuf()
        {
            return new Proto.NetworkGetVersionInfoResponse
            {
				HapiProtoVersion = ProtobufVersion.ToProtobuf(),
				HederaServicesVersion = ServicesVersion.ToProtobuf(),
			};
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}