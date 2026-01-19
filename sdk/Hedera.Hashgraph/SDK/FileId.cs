using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * The ID for a file on Hedera.
     */
    public sealed class FileId : IComparable<FileId> 
    {
        /**
         * The public node address book for the current network.
         */
        public static readonly FileId ADDRESS_BOOK = new (0, 0, 102);
        /**
         * The current fee schedule for the network.
         */
        public static readonly FileId FEE_SCHEDULE = new (0, 0, 111);
        /**
         * The current exchange rate of HBAR to USD.
         */
        public static readonly FileId EXCHANGE_RATES = new (0, 0, 112);

        /**
         * The Shard number
         */
        public readonly LongNN Shard;
        /**
         * The Realm number
         */
        public readonly LongNN Realm;
        /**
         * The id number
         */
        public readonly LongNN Num;
        public string? Checksum { get; }

        /**
         * Assign the Num portion of the file id.
         *
         * @param Num                       the Num portion not negative
         *
         * Constructor that uses Shard, Realm and Num should be used instead
         * as Shard and Realm should not assume 0 value
         */
        [Obsolete]
        public FileId(LongNN num) : this(0, 0, num) { }

        /**
         * Assign the file id.
         *
         * @param Shard                     the Shard portion
         * @param Realm                     the Realm portion
         * @param Num                       the Num portion
         */
        public FileId(LongNN shard, LongNN realm, LongNN num) : this(shard, realm, num, null) { }

        /**
         * Assign the file id and optional Checksum.
         *
         * @param Shard                     the Shard portion
         * @param Realm                     the Realm portion
         * @param Num                       the Num portion
         * @param Checksum                  the optional Checksum
         */
        FileId(LongNN Shard, LongNN Realm, LongNN Num, string? Checksum) {
            this.Shard = Shard;
            this.Realm = Realm;
            this.Num = Num;
            this.Checksum = Checksum;
        }

		/**
         * Assign the file id from a string.
         *
         * @param id                        the string representation of a file id
         * @return                          the file id object
         */
		public static FileId FromString(string id)
		{
			return EntityIdHelper.FromString(id, (a, b, c, d) => new FileId(a, b, c, d));
		}
		/**
         * Assign the file id from a byte array.
         *
         * @param bytes                     the byte array representation of a file id
         * @return                          the file id object
         * @       when there is an issue with the protobuf
         */
		public static FileId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.FileID.Parser.ParseFrom(bytes));
		}
		/**
         * Create a file id object from a protobuf.
         *
         * @param fileId                    the protobuf
         * @return                          the file id object
         */
		public static FileId FromProtobuf(Proto.FileID fileId)
		{
			return new FileId(fileId.ShardNum, fileId.RealmNum, fileId.FileNum);
		}
		/**
         * Retrieve the file id from a solidity address.
         *
         * @param address                   a string representing the address
         * @return                          the file id object
         * @deprecated This method is deprecated. Use {@link #fromEvmAddress(long, long, string)} instead.
         */
		[Obsolete]
		public static FileId FromSolidityAddress(string address)
		{
			return EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new FileId(a, b, c, d));
		}
		/**
         * Get the `FileId` of the Hedera address book for the given Realm and Shard.
         * @param Shard
         * @param Realm
         * @return FileId
         */
		public static FileId GetAddressBookFileIdFor(long Shard, long Realm) 
        {
            return new FileId(Shard, Realm, 102);
        }
        /**
         * Get the `FileId` of the Hedera fee schedule for the given Realm and Shard.
         * @param Shard
         * @param Realm
         * @return FileId
         */
        public static FileId GetFeeScheduleFileIdFor(long Shard, long Realm) 
        {
            return new FileId(Shard, Realm, 111);
        }
        /**
         * Get the `FileId` of the Hedera exchange rates for the given Realm and Shard.
         * @param Shard
         * @param Realm
         * @return FileId
         */
        public static FileId GetExchangeRatesFileIdFor(long Shard, long Realm) 
        {
            return new FileId(Shard, Realm, 112);
        }

        /**
         * Extract the solidity address.
         *
         * @return                          the solidity address as a string
         * @deprecated This method is deprecated. Use {@link #toEvmAddress()} instead.
         */
        [Obsolete]
        public string ToSolidityAddress() 
        {
            return EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
        }

        /**
         * Constructs a FileId from Shard, Realm, and EVM address.
         * The EVM address must be a "long zero address" (first 12 bytes are zero).
         *
         * @param Shard      the Shard number
         * @param Realm      the Realm number
         * @param evmAddress the EVM address as a hex string
         * @return           the FileId object
         * @ if the EVM address is not a valid long zero address
         */
        public static FileId FromEvmAddress(long Shard, long Realm, string evmAddress) 
        {
            byte[] addressBytes = EntityIdHelper.DecodeEvmAddress(evmAddress);

            if (!EntityIdHelper.IsLongZeroAddress(addressBytes)) 
                throw new ArgumentException("EVM address is not a correct long zero address");
        
            ByteBuffer buf = ByteBuffer.wrap(addressBytes);
            buf.getInt();
            buf.getLong();
            long fileNum = buf.getLong();

            return new FileId(Shard, Realm, fileNum);
        }

        /**
         * Converts this FileId to an EVM address string.
         * Creates a solidity address using Shard=0, Realm=0, and the file number.
         *
         * @return the EVM address as a hex string
         */
        public string ToEvmAddress() 
        {
            return EntityIdHelper.ToSolidityAddress(0, 0, Num);
        }

        /**
         * @return                         protobuf representing the file id
         */
        public Proto.FileID ToProtobuf() 
        {
            return new Proto.FileID
            {
				ShardNum = Shard,
				RealmNum = Realm,
				FileNum = Num,
			};
        }

        /**
         * @param client to validate against
         * @ if entity ID is formatted poorly
         * @deprecated Use {@link #validateChecksum(Client)} instead.
         */
        [Obsolete]
        public void Validate(Client client)  {
            ValidateChecksum(client);
        }
        /**
         * Validate that the client is configured correctly.
         *
         * @param client                    the client to validate
         * @     if entity ID is formatted poorly
         */
        public void ValidateChecksum(Client client)  {
            EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
        }

        /**
         * Create the byte array.
         *
         * @return                          byte array representation
         */
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString() 
        {
            return EntityIdHelper.ToString(Shard, Realm, Num);
        }

        /**
         * Convert the client to a string representation with a Checksum.
         *
         * @param client                    the client to stringify
         * @return                          string representation with Checksum
         */
        public string ToStringWithChecksum(Client client) 
        {
            return EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
        }

		public int CompareTo(FileId? o)
		{
			if (o is null) return 1;

			if (Shard.CompareTo(o.Shard) is int shardComparison && shardComparison != 0)
				return shardComparison;

			if (Realm.CompareTo(o.Realm) is int realmComparison && realmComparison != 0)
				return realmComparison;

			return Num.CompareTo(o.Num);
		}
		public override int GetHashCode() 
        {
            return HashCode.Combine(Shard, Realm, Num);
        }
        public override bool Equals(object? obj) 
        {
            if (obj == this) return true;
            if (obj is not FileId fileid) return false;
            
            return Shard == fileid.Shard && Realm == fileid.Realm && Num == fileid.Num;
        }
    }
}