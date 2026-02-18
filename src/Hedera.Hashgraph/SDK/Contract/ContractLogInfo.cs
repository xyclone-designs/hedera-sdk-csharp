// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// The log information for an event returned by a smart contract function call.
    /// One function call may return several such events.
    /// </summary>
    public sealed class ContractLogInfo
    {
        /// <summary>
        /// Address of a contract that emitted the event.
        /// </summary>
        public readonly ContractId ContractId;
        /// <summary>
        /// Bloom filter for a particular log.
        /// </summary>
        public readonly ByteString Bloom;
        /// <summary>
        /// Topics of a particular event.
        /// </summary>
        public readonly List<ByteString> Topics;
        /// <summary>
        /// The event data.
        /// </summary>
        public readonly ByteString Data;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contractId">the contract id</param>
        /// <param name="bloom">the bloom filter</param>
        /// <param name="topics">list of topics</param>
        /// <param name="data">the event data</param>
        private ContractLogInfo(ContractId contractId, ByteString bloom, IList<ByteString> topics, ByteString data)
        {
            ContractId = contractId;
            Bloom = bloom;
            Topics = topics;
            Data = data;
        }

        /// <summary>
        /// Convert to a protobuf.
        /// </summary>
        /// <param name="logInfo">the log info object</param>
        /// <returns>                         the protobuf</returns>
        public static ContractLogInfo FromProtobuf(Proto.ContractLoginfo logInfo)
        {
            return new ContractLogInfo(ContractId.FromProtobuf(logInfo.ContractID), logInfo.Bloom, logInfo.Topic, logInfo.Data);
        }

        /// <summary>
        /// Create the contract log info from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the contract log info object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static ContractLogInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.ContractLoginfo.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
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

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}