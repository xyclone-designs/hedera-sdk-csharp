using Google.Protobuf;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Hedera follows semantic versioning () for both the HAPI protobufs and
	 * the Services software. This type allows the getVersionInfo query in the
	 * NetworkService to return the deployed versions of both protobufs and
	 * software on the node answering the query.
	 *
	 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/semanticversion">Hedera Documentation</a>
	 */
	public class SemanticVersion
	{
		/**
		 * Constructor.
		 *
		 * @param major                     the major part
		 * @param minor                     the minor part
		 * @param patch                     the patch part
		 */
		SemanticVersion(int major, int minor, int patch)
		{
			Major = major;
			Minor = minor;
			Patch = patch;
		}

		/**
		 * Increases with incompatible API changes
		 */
		public int Major { get; }
		/**
		 * Increases with backwards-compatible new functionality
		 */
		public int Minor { get; }
		/**
		 * Increases with backwards-compatible bug fixes
		 */
		public int Patch { get; }

		/**
		 * Create the protobuf.
		 *
		 * @return                          the protobuf representation
		 */
		protected Proto.SemanticVersion ToProtobuf()
		{
			return new Proto.SemanticVersion
			{
				Major = Major,
				Minor = Minor,
				Patch = Patch,
			};
		}
		/**
		 * Create a semantic version object from a protobuf.
		 *
		 * @param version                   the protobuf
		 * @return                          the new semantic version
		 */
		protected static SemanticVersion FromProtobuf(Proto.SemanticVersion version)
		{
			return new SemanticVersion(version.Major, version.Minor, version.Patch);
		}

		/**
		 * Create a semantic version from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new semantic version
		 * @       when there is an issue with the protobuf
		 */
		public static SemanticVersion FromBytes(byte[] bytes) 
		{
			return FromProtobuf(Proto.SemanticVersion.Parser.ParseFrom(bytes));
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

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch); 
        }
	}
}