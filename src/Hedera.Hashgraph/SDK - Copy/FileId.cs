// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Nio;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// The ID for a file on Hedera.
    /// </summary>
    public sealed class FileId : IComparable<FileId>
    {
        /// <summary>
        /// The public node address book for the current network.
        /// </summary>
        public static readonly FileId ADDRESS_BOOK = new FileId(0, 0, 102);
        /// <summary>
        /// The current fee schedule for the network.
        /// </summary>
        public static readonly FileId FEE_SCHEDULE = new FileId(0, 0, 111);
        /// <summary>
        /// The current exchange rate of HBAR to USD.
        /// </summary>
        public static readonly FileId EXCHANGE_RATES = new FileId(0, 0, 112);
        /// <summary>
        /// The shard number
        /// </summary>
        public readonly long shard;
        /// <summary>
        /// The realm number
        /// </summary>
        public readonly long realm;
        /// <summary>
        /// The id number
        /// </summary>
        public readonly long num;
        private readonly string checksum;
        /// <summary>
        /// Assign the num portion of the file id.
        /// </summary>
        /// <param name="num">the num portion not negative
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public FileId(long num) : this(0, 0, num)
        {
        }

        /// <summary>
        /// Assign the file id.
        /// </summary>
        /// <param name="shard">the shard portion</param>
        /// <param name="realm">the realm portion</param>
        /// <param name="num">the num portion</param>
        public FileId(long shard, long realm, long num) : this(shard, realm, num, null)
        {
        }

        /// <summary>
        /// Assign the file id and optional checksum.
        /// </summary>
        /// <param name="shard">the shard portion</param>
        /// <param name="realm">the realm portion</param>
        /// <param name="num">the num portion</param>
        /// <param name="checksum">the optional checksum</param>
        FileId(long shard, long realm, long num, string checksum)
        {
            shard = shard;
            realm = realm;
            num = num;
            checksum = checksum;
        }

        /// <summary>
        /// Get the `FileId` of the Hedera address book for the given realm and shard.
        /// </summary>
        /// <param name="shard"></param>
        /// <param name="realm"></param>
        /// <returns>FileId</returns>
        public static FileId GetAddressBookFileIdFor(long shard, long realm)
        {
            return new FileId(shard, realm, 102);
        }

        /// <summary>
        /// Get the `FileId` of the Hedera fee schedule for the given realm and shard.
        /// </summary>
        /// <param name="shard"></param>
        /// <param name="realm"></param>
        /// <returns>FileId</returns>
        public static FileId GetFeeScheduleFileIdFor(long shard, long realm)
        {
            return new FileId(shard, realm, 111);
        }

        /// <summary>
        /// Get the `FileId` of the Hedera exchange rates for the given realm and shard.
        /// </summary>
        /// <param name="shard"></param>
        /// <param name="realm"></param>
        /// <returns>FileId</returns>
        public static FileId GetExchangeRatesFileIdFor(long shard, long realm)
        {
            return new FileId(shard, realm, 112);
        }

        /// <summary>
        /// Assign the file id from a string.
        /// </summary>
        /// <param name="id">the string representation of a file id</param>
        /// <returns>                         the file id object</returns>
        public static FileId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, FileId.New());
        }

        /// <summary>
        /// Assign the file id from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array representation of a file id</param>
        /// <returns>                         the file id object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static FileId FromBytes(byte[] bytes)
        {
            return FromProtobuf(FileID.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create a file id object from a protobuf.
        /// </summary>
        /// <param name="fileId">the protobuf</param>
        /// <returns>                         the file id object</returns>
        static FileId FromProtobuf(FileID fileId)
        {
            Objects.RequireNonNull(fileId);
            return new FileId(fileId.ShardNum, fileId.RealmNum, fileId.GetFileNum());
        }

        /// <summary>
        /// Retrieve the file id from a solidity address.
        /// </summary>
        /// <param name="address">a string representing the address</param>
        /// <returns>                         the file id object</returns>
        /// <remarks>@deprecatedThis method is deprecated. Use {@link #fromEvmAddress(long, long, String)} instead.</remarks>
        public static FileId FromSolidityAddress(string address)
        {
            return Utils.EntityIdHelper.FromSolidityAddress(address, FileId.New());
        }

        /// <summary>
        /// Extract the solidity address.
        /// </summary>
        /// <returns>                         the solidity address as a string</returns>
        /// <remarks>@deprecatedThis method is deprecated. Use {@link #toEvmAddress()} instead.</remarks>
        public string ToSolidityAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(shard, realm, num);
        }

        /// <summary>
        /// Constructs a FileId from shard, realm, and EVM address.
        /// The EVM address must be a "long zero address" (first 12 bytes are zero).
        /// </summary>
        /// <param name="shard">the shard number</param>
        /// <param name="realm">the realm number</param>
        /// <param name="evmAddress">the EVM address as a hex string</param>
        /// <returns>          the FileId object</returns>
        /// <exception cref="IllegalArgumentException">if the EVM address is not a valid long zero address</exception>
        public static FileId FromEvmAddress(long shard, long realm, string evmAddress)
        {
            byte[] addressBytes = Utils.EntityIdHelper.DecodeEvmAddress(evmAddress);
            if (!Utils.EntityIdHelper.IsLongZeroAddress(addressBytes))
            {
                throw new ArgumentException("EVM address is not a correct long zero address");
            }

            ByteBuffer buf = ByteBuffer.Wrap(addressBytes);
            buf.GetInt();
            buf.GetLong();
            long fileNum = buf.GetLong();
            return new FileId(shard, realm, fileNum);
        }

        /// <summary>
        /// Converts this FileId to an EVM address string.
        /// Creates a solidity address using shard=0, realm=0, and the file number.
        /// </summary>
        /// <returns>the EVM address as a hex string</returns>
        public string ToEvmAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(0, 0, num);
        }

        /// <summary>
        /// </summary>
        /// <returns>                        protobuf representing the file id</returns>
        FileID ToProtobuf()
        {
            return FileID.NewBuilder().SetShardNum(shard).SetRealmNum(realm).SetFileNum(num).Build();
        }

        /// <summary>
        /// </summary>
        /// <param name="client">to validate against</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        /// <remarks>@deprecatedUse {@link #validateChecksum(Client)} instead.</remarks>
        public void Validate(Client client)
        {
            ValidateChecksum(client);
        }

        /// <summary>
        /// Validate that the client is configured correctly.
        /// </summary>
        /// <param name="client">the client to validate</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        public void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(shard, realm, num, client, checksum);
        }

        /// <summary>
        /// Extract the checksum.
        /// </summary>
        /// <returns>                         the checksum</returns>
        public string GetChecksum()
        {
            return checksum;
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString()
        {
            return Utils.EntityIdHelper.ToString(shard, realm, num);
        }

        /// <summary>
        /// Convert the client to a string representation with a checksum.
        /// </summary>
        /// <param name="client">the client to stringify</param>
        /// <returns>                         string representation with checksum</returns>
        public override string ToStringWithChecksum(Client client)
        {
            return Utils.EntityIdHelper.ToStringWithChecksum(shard, realm, num, client, checksum);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(shard, realm, num);
        }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is FileId))
            {
                return false;
            }

            FileId otherId = (FileId)o;
            return shard == otherId.shard && realm == otherId.realm && num == otherId.num;
        }

        public int CompareTo(FileId o)
        {
            Objects.RequireNonNull(o);
            int shardComparison = Long.Compare(shard, o.shard);
            if (shardComparison != 0)
            {
                return shardComparison;
            }

            int realmComparison = Long.Compare(realm, o.realm);
            if (realmComparison != 0)
            {
                return realmComparison;
            }

            return Long.Compare(num, o.num);
        }
    }
}