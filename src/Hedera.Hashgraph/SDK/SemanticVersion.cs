// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Hedera follows semantic versioning () for both the HAPI protobufs and
    /// the Services software. This type allows the getVersionInfo query in the
    /// NetworkService to return the deployed versions of both protobufs and
    /// software on the node answering the query.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/semanticversion">Hedera Documentation</a>
    /// </summary>
    public class SemanticVersion
    {
        /// <summary>
        /// Increases with incompatible API changes
        /// </summary>
        public int Major;
        /// <summary>
        /// Increases with backwards-compatible new functionality
        /// </summary>
        public int Minor;
        /// <summary>
        /// Increases with backwards-compatible bug fixes
        /// </summary>
        public int Patch;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="major">the major part</param>
        /// <param name="minor">the minor part</param>
        /// <param name="patch">the patch part</param>
        SemanticVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

		/// <summary>
		/// Create a semantic version from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new semantic version</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static SemanticVersion FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.SemanticVersion.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a semantic version object from a protobuf.
		/// </summary>
		/// <param name="version">the protobuf</param>
		/// <returns>                         the new semantic version</returns>
		public static SemanticVersion FromProtobuf(Proto.SemanticVersion version)
        {
            return new SemanticVersion(version.Major, version.Minor, version.Patch);
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        public virtual Proto.SemanticVersion ToProtobuf()
        {
            return new Proto.SemanticVersion
            {
                Major = Major,
                Minor = Minor,
                Patch = Patch,
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