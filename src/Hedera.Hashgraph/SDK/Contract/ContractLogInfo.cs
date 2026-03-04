// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="T:ContractLogInfo"]/*' />
    public sealed class ContractLogInfo
    {
        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="F:ContractLogInfo.ContractId"]/*' />
        public readonly ContractId ContractId;
        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="F:ContractLogInfo.Bloom"]/*' />
        public readonly ByteString Bloom;
        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="M:ContractLogInfo.#ctor(ContractId,ByteString,System.Collections.Generic.IEnumerable{ByteString},ByteString)"]/*' />
        public readonly List<ByteString> Topics;
        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="M:ContractLogInfo.#ctor(ContractId,ByteString,System.Collections.Generic.IEnumerable{ByteString},ByteString)_2"]/*' />
        public readonly ByteString Data;
        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="M:ContractLogInfo.#ctor(ContractId,ByteString,System.Collections.Generic.IEnumerable{ByteString},ByteString)_3"]/*' />
        private ContractLogInfo(ContractId contractId, ByteString bloom, IEnumerable<ByteString> topics, ByteString data)
        {
            ContractId = contractId;
            Bloom = bloom;
            Topics = [.. topics];
            Data = data;
        }

        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="M:ContractLogInfo.FromProtobuf(Proto.ContractLoginfo)"]/*' />
        public static ContractLogInfo FromProtobuf(Proto.ContractLoginfo logInfo)
        {
            return new ContractLogInfo(ContractId.FromProtobuf(logInfo.ContractID), logInfo.Bloom, logInfo.Topic, logInfo.Data);
        }

        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="M:ContractLogInfo.FromBytes(System.Byte[])"]/*' />
        public static ContractLogInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.ContractLoginfo.Parser.ParseFrom(bytes));
        }

        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="M:ContractLogInfo.ToProtobuf"]/*' />
        public Proto.ContractLoginfo ToProtobuf()
        {
            Proto.ContractLoginfo proto = new()
            {
				ContractID = ContractId.ToProtobuf(),
				Bloom = Bloom,
			};

            foreach (ByteString topic in Topics)
                proto.Topic.Add(topic);

            return proto;
        }

        /// <include file="ContractLogInfo.cs.xml" path='docs/member[@name="M:ContractLogInfo.ToBytes"]/*' />
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}