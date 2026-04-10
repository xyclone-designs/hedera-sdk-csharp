// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <include file="NetworkVersionInfo.cs.xml" path='docs/member[@name="T:NetworkVersionInfo"]/*' />
    public class NetworkVersionInfo
    {
        /// <include file="NetworkVersionInfo.cs.xml" path='docs/member[@name="M:NetworkVersionInfo.#ctor(SemanticVersion,SemanticVersion)"]/*' />
        public readonly SemanticVersion ProtobufVersion;
        /// <include file="NetworkVersionInfo.cs.xml" path='docs/member[@name="M:NetworkVersionInfo.#ctor(SemanticVersion,SemanticVersion)_2"]/*' />
        public readonly SemanticVersion ServicesVersion;

        /// <include file="NetworkVersionInfo.cs.xml" path='docs/member[@name="M:NetworkVersionInfo.#ctor(SemanticVersion,SemanticVersion)_3"]/*' />
        internal NetworkVersionInfo(SemanticVersion hapi, SemanticVersion hedera)
        {
            ProtobufVersion = hapi;
            ServicesVersion = hedera;
        }

		/// <include file="NetworkVersionInfo.cs.xml" path='docs/member[@name="M:NetworkVersionInfo.FromBytes(System.Byte[])"]/*' />
		public static NetworkVersionInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.NetworkGetVersionInfoResponse.Parser.ParseFrom(bytes));
		}
		/// <include file="NetworkVersionInfo.cs.xml" path='docs/member[@name="M:NetworkVersionInfo.FromProtobuf(Proto.NetworkGetVersionInfoResponse)"]/*' />
		public static NetworkVersionInfo FromProtobuf(Proto.NetworkGetVersionInfoResponse proto)
        {
            return new NetworkVersionInfo(
                SemanticVersion.FromProtobuf(proto.HapiProtoVersion), 
                SemanticVersion.FromProtobuf(proto.HederaServicesVersion));
        }

		/// <include file="NetworkVersionInfo.cs.xml" path='docs/member[@name="M:NetworkVersionInfo.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="NetworkVersionInfo.cs.xml" path='docs/member[@name="M:NetworkVersionInfo.ToProtobuf"]/*' />
		public virtual Proto.NetworkGetVersionInfoResponse ToProtobuf()
        {
            return new Proto.NetworkGetVersionInfoResponse
            {
				HapiProtoVersion = ProtobufVersion.ToProtobuf(),
				HederaServicesVersion = ServicesVersion.ToProtobuf(),
			};
        }
    }
}