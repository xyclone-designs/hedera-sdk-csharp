// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

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
        public readonly ContractId contractId;
        /// <summary>
        /// Bloom filter for a particular log.
        /// </summary>
        public readonly ByteString bloom;
        /// <summary>
        /// Topics of a particular event.
        /// </summary>
        public readonly IList<ByteString> topics;
        /// <summary>
        /// The event data.
        /// </summary>
        public readonly ByteString data;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contractId">the contract id</param>
        /// <param name="bloom">the bloom filter</param>
        /// <param name="topics">list of topics</param>
        /// <param name="data">the event data</param>
        private ContractLogInfo(ContractId contractId, ByteString bloom, IList<ByteString> topics, ByteString data)
        {
            contractId = contractId;
            bloom = bloom;
            topics = topics;
            data = data;
        }

        /// <summary>
        /// Convert to a protobuf.
        /// </summary>
        /// <param name="logInfo">the log info object</param>
        /// <returns>                         the protobuf</returns>
        static ContractLogInfo FromProtobuf(Proto.ContractLoginfo logInfo)
        {
            return new ContractLogInfo(ContractId.FromProtobuf(logInfo.GetContractID()), logInfo.GetBloom(), logInfo.GetTopicList(), logInfo.GetData());
        }

        /// <summary>
        /// Create the contract log info from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the contract log info object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static ContractLogInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(ContractLoginfo.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        Proto.ContractLoginfo ToProtobuf()
        {
            var contractLogInfo = Proto.ContractLoginfo.SetContractID(contractId.ToProtobuf()).SetBloom(bloom);
            foreach (ByteString topic in topics)
            {
                contractLogInfo.AddTopic(topic);
            }

            return contractLogInfo.Build();
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString()
        {
            var stringHelper = MoreObjects.ToStringHelper(this).Add("contractId", contractId).Add("bloom", Hex.ToHexString(bloom.ToByteArray()));
            var topicList = new ();
            foreach (var topic in topics)
            {
                topicList.Add(Hex.ToHexString(topic.ToByteArray()));
            }

            return stringHelper.Add("topics", topicList).ToString();
        }
    }
}