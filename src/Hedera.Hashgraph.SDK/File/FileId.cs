// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;
using System.IO;

namespace Hedera.Hashgraph.SDK.File
{
    /// <include file="FileId.cs.xml" path='docs/member[@name="T:FileId"]/*' />
    public sealed class FileId : IComparable<FileId>
    {
        /// <include file="FileId.cs.xml" path='docs/member[@name="T:FileId_2"]/*' />
        public static readonly FileId ADDRESS_BOOK = new (0, 0, 102);
        /// <include file="FileId.cs.xml" path='docs/member[@name="T:FileId_3"]/*' />
        public static readonly FileId FEE_SCHEDULE = new (0, 0, 111);
        /// <include file="FileId.cs.xml" path='docs/member[@name="T:FileId_4"]/*' />
        public static readonly FileId EXCHANGE_RATES = new (0, 0, 112);

        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.#ctor(System.Int64)"]/*' />
        public FileId(long num) : this(0, 0, num) { }
        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.#ctor(System.Int64,System.Int64,System.Int64)"]/*' />
        public FileId(long shard, long realm, long num) : this(shard, realm, num, null) { }
        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.#ctor(System.Int64,System.Int64,System.Int64,System.String)"]/*' />
        private FileId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
        }
		
		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.GetAddressBookFileIdFor(System.Int64,System.Int64)"]/*' />
		public static FileId GetAddressBookFileIdFor(long shard, long realm)
        {
            return new FileId(shard, realm, 102);
        }
        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.GetFeeScheduleFileIdFor(System.Int64,System.Int64)"]/*' />
        public static FileId GetFeeScheduleFileIdFor(long shard, long realm)
        {
            return new FileId(shard, realm, 111);
        }
        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.GetExchangeRatesFileIdFor(System.Int64,System.Int64)"]/*' />
        public static FileId GetExchangeRatesFileIdFor(long shard, long realm)
        {
            return new FileId(shard, realm, 112);
        }

        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.FromString(System.String)"]/*' />
        public static FileId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new FileId(a, b, c, d));
        }
        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.FromBytes(System.Byte[])"]/*' />
        public static FileId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.FileID.Parser.ParseFrom(bytes));
        }
        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.FromProtobuf(Proto.Services.FileId)"]/*' />
        public static FileId FromProtobuf(Proto.Services.FileID fileId)
        {
            return new FileId(fileId.ShardNum, fileId.RealmNum, fileId.FileNum);
        }
		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.FromSolidityAddress(System.String)"]/*' />
		public static FileId FromSolidityAddress(string address)
		{
			return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new FileId(a, b, c, d));
		}
		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.FromEvmAddress(System.Int64,System.Int64,System.String)"]/*' />
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

		/// <include file="FileId.cs.xml" path='docs/member[@name="P:FileId.Shard"]/*' />
		public long Shard { get; }
		/// <include file="FileId.cs.xml" path='docs/member[@name="P:FileId.Realm"]/*' />
		public long Realm { get; }
		/// <include file="FileId.cs.xml" path='docs/member[@name="P:FileId.Num"]/*' />
		public long Num { get; }
		/// <include file="FileId.cs.xml" path='docs/member[@name="P:FileId.Checksum"]/*' />
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

		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.ToBytes"]/*' />
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.ToEvmAddress"]/*' />
		public string ToEvmAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(0, 0, Num);
        }
		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.ToSolidityAddress"]/*' />
		public string ToSolidityAddress()
		{
			return Utils.EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
		}
		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.ToProtobuf"]/*' />
		public Proto.Services.FileID ToProtobuf()
        {
			return new Proto.Services.FileID
			{
				ShardNum = this.Shard,
				RealmNum = this.Realm,
				FileNum = this.Num,
			};
        }
		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.ToStringWithChecksum(Client)"]/*' />
		public string ToStringWithChecksum(Client client)
		{
			return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
		}

		/// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.Validate(Client)"]/*' />
		public void Validate(Client client)
        {
            ValidateChecksum(client);
        }
        /// <include file="FileId.cs.xml" path='docs/member[@name="M:FileId.ValidateChecksum(Client)"]/*' />
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
