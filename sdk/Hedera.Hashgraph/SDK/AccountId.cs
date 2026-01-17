using Google.Protobuf.WellKnownTypes;

using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

using System;
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

        /**
     * The Shard number
     */
        public ulong Shard { get; }
        /**
         * The Realm number
         */
        public ulong Realm { get; }
        /**
         * The id number
         */
        public ulong Num { get; }
		public string? Checksum { get; }

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
        public AccountId(ulong Num) : this(0, 0, Num) { }
        /**
         * Assign all parts of the account id.
         *
         * @param Shard                     the Shard part of the account id
         * @param Realm                     the Realm part of the account id
         * @param Num                       the Num part of the account id
         */
        public AccountId(ulong Shard, ulong Realm, ulong Num) : this(shard, realm, num, null) { }
		/**
         * Assign all parts of the account id.
         *
         * @param Shard                     the Shard part of the account id
         * @param Realm                     the Realm part of the account id
         * @param Num                       the Num part of the account id
         */
		AccountId(ulong Shard, ulong Realm, ulong Num, string? Checksum) {
            Shard = Shard;
            Realm = Realm;
            Num = Num;
            Checksum = Checksum;
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
        AccountId(ulong Shard, ulong Realm, ulong Num, string? Checksum, PublicKey? aliasKey, EvmAddress? evmAddress) {
            Shard = Shard;
            Realm = Realm;
            Num = Num;
            Checksum = Checksum;
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
			return FromProtobuf(AccountID.Parser.ParseFrom(bytes));
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
            if ((id.startsWith("0x") && id.Length() == 42) || id.Length() == 40) return FromEvmAddress(id, 0, 0);

            try {
                return EntityIdHelper.FromString(id, AccountId::new);
            } catch (ArgumentException error) {
                var match = ALIAS_ID_REGEX.matcher(id);
                if (!match.find()) {
                    throw new ArgumentException(
                            "Invalid Account ID \"" + id
                                    + "\": format should look like 0.0.123 or 0.0.123-vfmkw or 0.0.1337BEEF (where 1337BEEF is a hex-encoded, DER-format public key)");
                } else {
                    byte[] aliasBytes = Hex.decode(match.group(3));
                    bool isEvmAddress = aliasBytes.Length == 20;
                    return new AccountId(
                            long.parseLong(match.group(1)),
                            long.parseLong(match.group(2)),
                            0,
                            null,
                            isEvmAddress ? null : PublicKey.FromBytesDER(aliasBytes),
                            isEvmAddress ? EvmAddress.FromBytes(aliasBytes) : null);
                }
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
        public static AccountId FromEvmAddress(string evmAddress, ulong Shard, ulong Realm) 
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
        public static AccountId FromEvmAddress(EvmAddress evmAddress, ulong Shard, ulong Realm) 
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
            if (EntityIdHelper.IsLongZeroAddress(EntityIdHelper.DecodeEvmAddress(address))) {
                return EntityIdHelper.FromSolidityAddress(address, AccountId::new);
            } else {
                return FromEvmAddress(address, 0, 0);
            }
        }
        /**
         * Retrieve the account id From a protobuf.
         *
         * @param accountId                 the protobuf
         * @return                          the account id object
         */
        public static AccountId FromProtobuf(Proto.AccountID accountId) 
        {
            PublicKey aliasKey = null;
            EvmAddress evmAddress = null;

            if (accountId.hasAlias()) {
                if (accountId.getAlias().size() == 20) {
                    evmAddress = EvmAddress.FromAliasBytes(accountId.getAlias());
                } else {
                    aliasKey = PublicKey.FromAliasBytes(accountId.getAlias());
                }
            }
            Objects.requireNonNull(accountId);
            return new AccountId(
                    accountId.getShardNum(),
                    accountId.getRealmNum(),
                    accountId.getAccountNum(),
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
            return EntityIdHelper.ToSolidityAddress(shard, realm, num);
        }

        /**
         * Extract the account id protobuf.
         *
         * @return                          the account id builder
         */
        public Proto.AccountID ToProtobuf()
        {
            var accountIdBuilder = AccountID.newBuilder().setShardNum(Shard).setRealmNum(Realm);
            if (aliasKey != null) {
                accountIdBuilder.setAlias(aliasKey.ToProtobufKey().toByteString());
            } else if (evmAddress != null) {
                accountIdBuilder.setAlias(ByteString.copyFrom(evmAddress.toBytes()));
            } else {
                accountIdBuilder.setAccountNum(Num);
            }
            return accountIdBuilder.build();
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
            return PopulateAccountNumAsync(client).get();
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
			return PopulateAccountEvmAddressAsync(client).Get();
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
						(ulong)accountnum.Result,
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
            if (aliasKey == null && evmAddress == null) {
                EntityIdHelper.Validate(shard, realm, num, client, Checksum);
            }
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
        public string ToStringWithChecksum(Client client) {
            if (aliasKey != null || evmAddress != null) {
                throw new IllegalStateException(
                        "toStringWithChecksum cannot be applied to AccountId with aliasKey or evmAddress");
            } else {
                return EntityIdHelper.toStringWithChecksum(shard, realm, num, client, Checksum);
            }
        }

		public int CompareTo(AccountId? o)
		{
			Objects.requireNonNull(o);
			int shardComparison = long.compare(Shard, o.Shard);
			if (shardComparison != 0)
			{
				return shardComparison;
			}
			int realmComparison = long.compare(Realm, o.Realm);
			if (realmComparison != 0)
			{
				return realmComparison;
			}
			int numComparison = long.compare(Num, o.Num);
			if (numComparison != 0)
			{
				return numComparison;
			}
			if ((aliasKey == null) != (o.aliasKey == null))
			{
				return aliasKey != null ? 1 : -1;
			}
			if (aliasKey != null)
			{
				return aliasKey.toStringDER().compareTo(o.aliasKey.toStringDER());
			}
			if ((evmAddress == null) != (o.evmAddress == null))
			{
				return evmAddress != null ? 1 : -1;
			}
			if (evmAddress == null)
			{
				return 0;
			}
			return evmAddress.toString().compareTo(o.evmAddress.toString());
		}

		public override string ToString()
		{
			if (aliasKey != null)
			{
				return "" + Shard + "." + Realm + "." + aliasKey.toStringDER();
			}
			else if (evmAddress != null)
			{
				return "" + Shard + "." + Realm + "." + evmAddress.toString();
			}
			else
			{
				return EntityIdHelper.toString(shard, realm, num);
			}
		}
		public override int GetHashCode() {
            byte[] aliasBytes = null;

            if (aliasKey != null) {
                aliasBytes = aliasKey.toBytes();
            } else if (evmAddress != null) {
                aliasBytes = evmAddress.toBytes();
            }

            return Objects.hash(shard, realm, num, Arrays.hashCode(aliasBytes));
        }
        public override bool Equals(object? obj) {
            if (this == o) {
                return true;
            }

            if (!(o instanceof AccountId)) {
                return false;
            }

            AccountId otherId = (AccountId) o;
            if ((aliasKey == null) != (otherId.aliasKey == null)) {
                return false;
            }
            if ((evmAddress == null) != (otherId.evmAddress == null)) {
                return false;
            }
            return Shard == otherId.Shard
                    && Realm == otherId.Realm
                    && Num == otherId.Num
                    && (aliasKey == null || aliasKey.equals(otherId.aliasKey))
                    && (evmAddress == null || evmAddress.equals(otherId.evmAddress));
        }
    }
}