// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;
using System.IO;

namespace Hedera.Hashgraph.SDK.File
{
    /// <summary>
    /// The ID for a file on Hedera.
    /// </summary>
    public sealed class FileId : IComparable<FileId>
    {
        /// <summary>
        /// The public node address book for the current network.
        /// </summary>
        public static readonly FileId ADDRESS_BOOK = new (0, 0, 102);
        /// <summary>
        /// The current fee schedule for the network.
        /// </summary>
        public static readonly FileId FEE_SCHEDULE = new (0, 0, 111);
        /// <summary>
        /// The current exchange rate of HBAR to USD.
        /// </summary>
        public static readonly FileId EXCHANGE_RATES = new (0, 0, 112);

        /// <summary>
        /// Assign the num portion of the file id.
        /// </summary>
        /// <param name="num">the num portion not negative
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public FileId(long num) : this(0, 0, num) { }
        /// <summary>
        /// Assign the file id.
        /// </summary>
        /// <param name="shard">the shard portion</param>
        /// <param name="realm">the realm portion</param>
        /// <param name="num">the num portion</param>
        public FileId(long shard, long realm, long num) : this(shard, realm, num, null) { }
        /// <summary>
        /// Assign the file id and optional checksum.
        /// </summary>
        /// <param name="shard">the shard portion</param>
        /// <param name="realm">the realm portion</param>
        /// <param name="num">the num portion</param>
        /// <param name="checksum">the optional checksum</param>
        private FileId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
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
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new FileId(a, b, c, d));
        }
        /// <summary>
        /// Assign the file id from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array representation of a file id</param>
        /// <returns>                         the file id object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static FileId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.FileID.Parser.ParseFrom(bytes));
        }
        /// <summary>
        /// Create a file id object from a protobuf.
        /// </summary>
        /// <param name="fileId">the protobuf</param>
        /// <returns>                         the file id object</returns>
        public static FileId FromProtobuf(Proto.FileID fileId)
        {
            return new FileId(fileId.ShardNum, fileId.RealmNum, fileId.FileNum);
        }
		/// <summary>
		/// Retrieve the file id from a solidity address.
		/// </summary>
		/// <param name="address">a string representing the address</param>
		/// <returns>                         the file id object</returns>
		/// <remarks>@deprecatedThis method is deprecated. Use {@link #fromEvmAddress(long, long, String)} instead.</remarks>
		public static FileId FromSolidityAddress(string address)
		{
			return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new FileId(a, b, c, d));
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
				throw new ArgumentException("EVM address is not a correct long zero address");

			using MemoryStream ms = new (addressBytes);
			using BinaryReader reader = new (ms);

			reader.ReadInt32();
			reader.ReadInt64();

			long fileNum = reader.ReadInt64();

			return new FileId(shard, realm, fileNum);
		}

		/// <summary>
		/// The shard number
		/// </summary>
		public long Shard { get; }
		/// <summary>
		/// The realm number
		/// </summary>
		public long Realm { get; }
		/// <summary>
		/// The id number
		/// </summary>
		public long Num { get; }
		/// <summary>
		/// Extract the checksum.
		/// </summary>
		public string? Checksum { get; }

		public int CompareTo(FileId? o)
		{
			int shardComparison = Shard.CompareTo(o?.Shard);

			if (shardComparison != 0)
				return shardComparison;

			int realmComparison = Realm.CompareTo(o?.Realm);

			if (realmComparison != 0)
				return realmComparison;

			return Num.CompareTo(o?.Num);
		}

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>                         byte array representation</returns>
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Converts this FileId to an EVM address string.
		/// Creates a solidity address using shard=0, realm=0, and the file number.
		/// </summary>
		/// <returns>the EVM address as a hex string</returns>
		public string ToEvmAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(0, 0, Num);
        }
		/// <summary>
		/// Extract the solidity address.
		/// </summary>
		/// <returns>                         the solidity address as a string</returns>
		/// <remarks>@deprecatedThis method is deprecated. Use {@link #toEvmAddress()} instead.</remarks>
		public string ToSolidityAddress()
		{
			return Utils.EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
		}
		/// <summary>
		/// </summary>
		/// <returns>                        protobuf representing the file id</returns>
		public Proto.FileID ToProtobuf()
        {
			return new Proto.FileID
			{
				ShardNum = this.Shard,
				RealmNum = this.Realm,
				FileNum = this.Num,
			};
        }
		/// <summary>
		/// Convert the client to a string representation with a checksum.
		/// </summary>
		/// <param name="client">the client to stringify</param>
		/// <returns>                         string representation with checksum</returns>
		public string ToStringWithChecksum(Client client)
		{
			return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
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
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
        }

		public override int GetHashCode()
		{
			return HashCode.Combine(Shard, Realm, Num);
		}
		public override bool Equals(object? o)
		{
			if (this == o)
				return true;

			if (o is not FileId otherId)
				return false;

			return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num;
		}
		public override string ToString()
        {
            return Utils.EntityIdHelper.ToString(Shard, Realm, Num);
        }
    }
}