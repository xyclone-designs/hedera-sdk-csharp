using Google.Protobuf;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/**
    * The ID for a cryptocurrency account on Hedera.
    */
    public sealed class AccountId : IComparable<AccountId> 
    {
        private static readonly Regex ALIAS_ID_REGEX = new ("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.((?:[0-9a-fA-F][0-9a-fA-F])+)$");

		public string? Checksum { get; }
		/**
         * The id number
         */
		public LongNN Num { get; }
		/**
         * The Realm number
         */
		public LongNN Realm { get; }
		/**
     * The Shard number
     */
		public LongNN Shard { get; }
		/**
         * The public key bytes to be used as the account's alias
         */
		public PublicKey? AliasKey { get; }
		/**
         * The ethereum account 20-byte EVM address to be used initially in place of the public key bytes
         */
		public EvmAddress? EvmAddress { get; }

        /**
         * Assign the Num part of the account id.
         *
         * @param Num                       the Num part of the account id
         *
         * Constructor that uses Shard, Realm and Num should be used instead
         * as Shard and Realm should not assume 0 value
         */
        [Obsolete]
        public AccountId(LongNN Num) : this(0, 0, Num) { }
        /**
         * Assign all parts of the account id.
         *
         * @param Shard                     the Shard part of the account id
         * @param Realm                     the Realm part of the account id
         * @param Num                       the Num part of the account id
         */
        public AccountId(LongNN shard, LongNN realm, LongNN num) : this(shard, realm, num, null) { }
		/**
         * Assign all parts of the account id.
         *
         * @param Shard                     the Shard part of the account id
         * @param Realm                     the Realm part of the account id
         * @param Num                       the Num part of the account id
         */
		AccountId(LongNN shard, LongNN realm, LongNN num, string? checksum) {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
            AliasKey = null;
            EvmAddress = null;
        }
        /**
         * Assign all parts of the account id.
         *
         * @param Shard                     the Shard part of the account id
         * @param Realm                     the Realm part of the account id
         * @param Num                       the Num part of the account id
         */
        AccountId(LongNN shard, LongNN realm, LongNN num, string? checksum, PublicKey? aliasKey, EvmAddress? evmAddress) {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
            AliasKey = aliasKey;
            EvmAddress = evmAddress;
        }

		/**
         * Retrieve the account id From a protobuf byte array.
         *
         * @param bytes                     a byte array representation of the protobuf
         * @return                          the account id object
         * @       when there is an issue with the protobuf
         */
		public static AccountId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.AccountID.Parser.ParseFrom(bytes));
		}
		/**
         * Retrieve the account id From a string.
         *
         * @param id                        a string representing a valid account id
         * @return                          the account id object
         * @ when the account id and Checksum are invalid
         */
		public static AccountId FromString(string id) 
        {
            if ((id.StartsWith("0x") && id.Length == 42) || id.Length == 40) 
                return FromEvmAddress(id, 0, 0);

            try { return EntityIdHelper.FromString(id, (a, b, c, d) => new AccountId(a, b, c, d)); } 
            catch (ArgumentException) 
            {
                var match = ALIAS_ID_REGEX.Matches(id);

                if (match.Count == 0)
					throw new ArgumentException(
						"Invalid Account ID \"" + id
						+ "\": format should look like 0.0.123 or 0.0.123-vfmkw or 0.0.1337BEEF (where 1337BEEF is a hex-encoded, DER-format public key)");

				byte[] aliasBytes = Hex.Decode(match.ElementAt(3).Value);
				bool isEvmAddress = aliasBytes.Length == 20;

				return new AccountId(
					long.Parse(match.ElementAt(1).Value),
					long.Parse(match.ElementAt(2).Value),
					0,
					null,
					isEvmAddress ? null : PublicKey.FromBytesDER(aliasBytes),
					isEvmAddress ? EvmAddress.FromBytes(aliasBytes) : null);
			}
        }

        /**
         * Retrieve the account id From an EVM address.
         *
         * @param evmAddress                a string representing the EVM address
         * @return                          the account id object
         *
         * Constructor that uses Shard, Realm and Num should be used instead
         * as Shard and Realm should not assume 0 value
         */
        [Obsolete]
        public static AccountId FromEvmAddress(string evmAddress) 
        {
            return FromEvmAddress(evmAddress, 0, 0);
        }
        /**
         * Retrieve the account id From an EVM address.
         *
         * @param evmAddress                a string representing the EVM address
         * @param Shard                     the Shard part of the account id
         * @param Realm                     the Shard Realm of the account id
         * @return                          the account id object
         *
         * In case Shard and Realm are unknown, they should be set to zero
         */
        public static AccountId FromEvmAddress(string evmAddress, LongNN Shard, LongNN Realm) 
        {
            return FromEvmAddress(EvmAddress.FromString(evmAddress), Shard, Realm);
        }

        /**
         * Retrieve the account id From an EVM address.
         *
         * @param evmAddress                an EvmAddress instance
         * @return                          the account id object
         *
         * Constructor that uses Shard, Realm and Num should be used instead
         * as Shard and Realm should not assume 0 value
         */
        [Obsolete]
        public static AccountId FromEvmAddress(EvmAddress evmAddress) 
        {
            return FromEvmAddress(evmAddress, 0, 0);
        }
        /**
         * Retrieve the account id From an EVM address.
         *
         * @param evmAddress                an EvmAddress instance
         * @param Shard                     the Shard part of the account id
         * @param Realm                     the Shard Realm of the account id
         * @return                          the account id object
         *
         * In case Shard and Realm are unknown, they should be set to zero
         */
        public static AccountId FromEvmAddress(EvmAddress evmAddress, LongNN Shard, LongNN Realm) 
        {
            EntityIdHelper.DecodeEvmAddress(evmAddress.ToString());

            return new AccountId(Shard, Realm, 0, null, null, evmAddress);
        }

        /**
         * Retrieve the account id From a solidity address.
         *
         * @param address                   a string representing the address
         * @return                          the account id object
         * @deprecated This method is deprecated. Use {@link #fromEvmAddress(EvmAddress, long, long)} instead.
         */
        [Obsolete]
        public static AccountId FromSolidityAddress(string address) 
        {
			return EntityIdHelper.IsLongZeroAddress(EntityIdHelper.DecodeEvmAddress(address))
				? EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new AccountId(a, b, c, d))
				: FromEvmAddress(address, 0, 0);
		}
        /**
         * Retrieve the account id From a protobuf.
         *
         * @param accountId                 the protobuf
         * @return                          the account id object
         */
        public static AccountId FromProtobuf(Proto.AccountID accountId) 
        {
            PublicKey? aliasKey = null;
            EvmAddress? evmAddress = null;

            if (accountId.HasAlias) 
            {
                if (accountId.Alias.Length == 20)
					evmAddress = EvmAddress.FromAliasBytes(accountId.Alias);
                else aliasKey = PublicKey.FromAliasBytes(accountId.Alias);
			}

            return new AccountId(
                    accountId.ShardNum,
                    accountId.RealmNum,
                    accountId.AccountNum,
                    null,
                    aliasKey,
                    evmAddress);
        }

		/**
         * toEvmAddress returns EVM-compatible address representation of the entity
         * @return
         */
		public string ToEvmAddress()
		{
			return EvmAddress is not null
				? Hex.ToHexString(EvmAddress.ToBytes())
				: EntityIdHelper.ToSolidityAddress(0, 0, Num);
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
         * Extract the account id protobuf.
         *
         * @return                          the account id builder
         */
        public Proto.AccountID ToProtobuf()
        {
			Proto.AccountID protobuf = new()
            {
				ShardNum = Shard,
				RealmNum = Realm,
			};

            if (AliasKey is not null) protobuf.Alias = AliasKey.ToProtobufKey().ToByteString();
            else if (EvmAddress is not null) protobuf.Alias = ByteString.CopyFrom(EvmAddress.ToBytes());
            else protobuf.AccountNum = Num;

            return protobuf;
        }

        /**
         * Gets the actual `Num` field of the `AccountId` From the Mirror Node.
         * Should be used after generating `AccountId.FromEvmAddress()` because it sets the `Num` field to `0`
         * automatically since there is no connection between the `Num` and the `evmAddress`
         * Sync version
         *
         * @param client
         * @return populated AccountId instance
         */
        public AccountId PopulateAccountNum(Client client) 
        {
            return PopulateAccountNumAsync(client)
                .GetAwaiter()
                .GetResult();
        }

		/**
         * Populates `evmAddress` field of the `AccountId` extracted From the Mirror Node.
         * Sync version
         *
         * @param client
         * @return populated AccountId instance
         */
		public AccountId PopulateAccountEvmAddress(Client client)
		{
			return PopulateAccountEvmAddressAsync(client)
				.GetAwaiter()
				.GetResult();
		}
		/**
         * Gets the actual `Num` field of the `AccountId` From the Mirror Node.
         * Should be used after generating `AccountId.FromEvmAddress()` because it sets the `Num` field to `0`
         * automatically since there is no connection between the `Num` and the `evmAddress`
         * Async version
         *
         * @deprecated Use 'populateAccountNum' instead due to its nearly identical operation.
         * @param client
         * @return populated AccountId instance
         */
		[Obsolete]
        public Task<AccountId> PopulateAccountNumAsync(Client client) 
        {
            return EntityIdHelper
                .GetAccountNumFromMirrorNodeAsync(client, EvmAddress?.ToString())
                .ContinueWith(accountnum => 
                {
                    return new AccountId(
                        Shard,
                        Realm,
						(LongNN)accountnum.Result,
                        Checksum,
                        AliasKey,
                        EvmAddress);
				});
        }
        /**
         * Populates `evmAddress` field of the `AccountId` extracted From the Mirror Node.
         * Async version
         *
         * @deprecated Use 'populateAccountEvmAddress' instead due to its nearly identical operation.
         * @param client
         * @return populated AccountId instance
         */
        [Obsolete]
        public Task<AccountId> PopulateAccountEvmAddressAsync(Client client) 
        {
			return EntityIdHelper
				.GetEvmAddressFromMirrorNodeAsync(client, (long)Num)
				.ContinueWith(evmaddress =>
				{
					return new AccountId(
						Shard,
						Realm,
						Num,
						Checksum,
						AliasKey,
						evmaddress.Result);
				});
        }

        /**
         * @param client to validate against
         * @ if entity ID is formatted poorly
         * @deprecated Use {@link #validateChecksum(Client)} instead.
         */
        [Obsolete]
        public void Validate(Client client)  
        {
            ValidateChecksum(client);
        }
        /**
         * Verify that the client has a valid Checksum.
         *
         * @param client                    the client to verify
         * @     when the account id and Checksum are invalid
         */
        public void ValidateChecksum(Client client)  
        {
            if (AliasKey == null && EvmAddress == null)
				EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
		}


        /**
         * Extract a byte array representation.
         *
         * @return                          a byte array representation
         */
        public byte[] ToBytes() 
        {
            return ToProtobuf().ToByteArray();
        }

        /**
         * Extract a string representation with the Checksum.
         *
         * @param client                    the client
         * @return                          the account id with Checksum
         */
        public string ToStringWithChecksum(Client client) 
        {
            if (AliasKey != null || EvmAddress != null)
				throw new InvalidOperationException("toStringWithChecksum cannot be applied to AccountId with aliasKey or evmAddress");

			return EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
        }

		public int CompareTo(AccountId? o)
		{
            if (o is null) return 1;

            if (Shard.CompareTo(o.Shard) is int shardComparison && shardComparison != 0)
                return shardComparison;

			if (Realm.CompareTo(o.Realm) is int realmComparison && realmComparison != 0)
				return realmComparison;

			if (Num.CompareTo(o.Num) is int numComparison && numComparison != 0)
				return numComparison;

			if (AliasKey == null != (o.AliasKey == null))
				return AliasKey != null ? 1 : -1;

			if (AliasKey != null)
				return AliasKey.ToStringDER().CompareTo(o.AliasKey?.ToStringDER());

			if (EvmAddress == null != (o.EvmAddress == null))
				return EvmAddress != null ? 1 : -1;

			if (EvmAddress == null) 
                return 0;

			return EvmAddress.ToString().CompareTo(o.EvmAddress?.ToString());
		}

		public override string ToString()
		{
			if (AliasKey != null)
				return "" + Shard + "." + Realm + "." + AliasKey.ToStringDER();

			else if (EvmAddress != null)
				return "" + Shard + "." + Realm + "." + EvmAddress.ToString();

			else 
                return EntityIdHelper.ToString(Shard, Realm, Num);
		}
		public override int GetHashCode()
        {
            byte[] aliasBytes = null;

            if (AliasKey != null) {
                aliasBytes = AliasKey.ToBytes();
            } else if (EvmAddress != null) {
                aliasBytes = EvmAddress.ToBytes();
            }

            return HashCode.Combine(Shard, Realm, Num, HashCode.Combine(aliasBytes));
        }
        public override bool Equals(object? o) 
        {
            if (this == o) return true;
            if (o is not AccountId accountid) return false;

            if (AliasKey == null != (accountid.AliasKey == null)) return false;
            if (EvmAddress == null != (accountid.EvmAddress == null)) return false;

            return Shard == accountid.Shard
                && Realm == accountid.Realm
                && Num == accountid.Num
                && (AliasKey == null || AliasKey.Equals(accountid.AliasKey))
                && (EvmAddress == null || EvmAddress.Equals(accountid.EvmAddress));
        }
    }
}