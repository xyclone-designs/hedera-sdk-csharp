// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="SemanticVersion.cs.xml" path='docs/member[@name="T:SemanticVersion"]/*' />
    public class SemanticVersion
    {
        /// <include file="SemanticVersion.cs.xml" path='docs/member[@name="F:SemanticVersion.Major"]/*' />
        public int Major;
        /// <include file="SemanticVersion.cs.xml" path='docs/member[@name="M:SemanticVersion.#ctor(System.Int32,System.Int32,System.Int32)"]/*' />
        public int Minor;
        /// <include file="SemanticVersion.cs.xml" path='docs/member[@name="M:SemanticVersion.#ctor(System.Int32,System.Int32,System.Int32)_2"]/*' />
        public int Patch;
        /// <include file="SemanticVersion.cs.xml" path='docs/member[@name="M:SemanticVersion.#ctor(System.Int32,System.Int32,System.Int32)_3"]/*' />
        internal SemanticVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

		/// <include file="SemanticVersion.cs.xml" path='docs/member[@name="M:SemanticVersion.FromBytes(System.Byte[])"]/*' />
		public static SemanticVersion FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.SemanticVersion.Parser.ParseFrom(bytes));
		}
		/// <include file="SemanticVersion.cs.xml" path='docs/member[@name="M:SemanticVersion.FromProtobuf(Proto.Services.SemanticVersion)"]/*' />
		public static SemanticVersion FromProtobuf(Proto.Services.SemanticVersion version)
        {
            return new SemanticVersion(version.Major, version.Minor, version.Patch);
        }

        /// <include file="SemanticVersion.cs.xml" path='docs/member[@name="M:SemanticVersion.ToProtobuf"]/*' />
        public virtual Proto.Services.SemanticVersion ToProtobuf()
        {
            return new Proto.Services.SemanticVersion
            {
                Major = Major,
                Minor = Minor,
                Patch = Patch,
            };
        }

        /// <include file="SemanticVersion.cs.xml" path='docs/member[@name="M:SemanticVersion.ToBytes"]/*' />
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}
