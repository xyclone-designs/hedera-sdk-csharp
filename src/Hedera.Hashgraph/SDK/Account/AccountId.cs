// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Keys;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <summary>
    /// The ID for a cryptocurrency account on Hedera.
    /// </summary>
    public sealed class AccountId : IComparable<AccountId>, ICloneable
    {
        private static readonly Regex ALIAS_ID_REGEX = new ("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.((?:[0-9a-fA-F][0-9a-fA-F])+)$");

        /// <summary>
        /// Assign the num part of the account id.
        /// </summary>
        /// <param name="num">the num part of the account id
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public AccountId(long num) : this(0, 0, num) { }
        /// <summary>
        /// Assign all parts of the account id.
        /// </summary>
        /// <param name="shard">the shard part of the account id</param>
        /// <param name="realm">the realm part of the account id</param>
        /// <param name="num">the num part of the account id</param>
        public AccountId(long shard, long realm, long num) : this(shard, realm, num, null) { }
		/// <summary>
		/// Assign all parts of the account id.
		/// </summary>
		/// <param name="shard">the shard part of the account id</param>
		/// <param name="realm">the realm part of the account id</param>
		/// <param name="num">the num part of the account id</param>
		public AccountId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
			Checksum = checksum;
            AliasKey = null;
            EvmAddress = null;
        }
        /// <summary>
        /// Assign all parts of the account id.
        /// </summary>
        /// <param name="shard">the shard part of the account id</param>
        /// <param name="realm">the realm part of the account id</param>
        /// <param name="num">the num part of the account id</param>
        public AccountId(long shard, long realm, long num, string? checksum, PublicKey? aliasKey, EvmAddress? evmAddress)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
            AliasKey = aliasKey;
            EvmAddress = evmAddress;
        }

        /// <summary>
        /// Retrieve the account id from a string.
        /// </summary>
        /// <param name="id">a string representing a valid account id</param>
        /// <returns>                         the account id object</returns>
        /// <exception cref="IllegalArgumentException">when the account id and checksum are invalid</exception>
        public static AccountId FromString(string id)
        {
            if ((id.StartsWith("0x") && id.Length == 42) || id.Length == 40)
                return FromEvmAddress(id, 0, 0);
            try
            {
                return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new AccountId(a, b, c, d));
            }
            catch (ArgumentException)
            {
                MatchCollection matches = ALIAS_ID_REGEX.Matches(id);

                if (matches.Count == 0)
					throw new ArgumentException("Invalid Account ID \"" + id + "\": format should look like 0.0.123 or 0.0.123-vfmkw or 0.0.1337BEEF (where 1337BEEF is a hex-encoded, DER-format public key)");

				byte[] aliasBytes = Hex.Decode(matches.ElementAt(3).Value);
				bool isEvmAddress = aliasBytes.Length == 20;
				
                return new AccountId(
                    long.Parse(matches.ElementAt(1).Value),
                    long.Parse(matches.ElementAt(2).Value),
                    0, 
                    null, 
                    isEvmAddress ? null : PublicKey.FromBytesDER(aliasBytes), 
                    isEvmAddress ? EvmAddress.FromBytes(aliasBytes) : null);
			}
        }
        /// <summary>
        /// Retrieve the account id from an EVM address.
        /// </summary>
        /// <param name="evmAddress">a string representing the EVM address</param>
        /// <returns>                         the account id object
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</returns>
        public static AccountId FromEvmAddress(string evmAddress)
        {
            return FromEvmAddress(evmAddress, 0, 0);
        }
        /// <summary>
        /// Retrieve the account id from an EVM address.
        /// </summary>
        /// <param name="evmAddress">a string representing the EVM address</param>
        /// <param name="shard">the shard part of the account id</param>
        /// <param name="realm">the shard realm of the account id</param>
        /// <returns>                         the account id object
        /// 
        /// In case shard and realm are unknown, they should be set to zero</returns>
        public static AccountId FromEvmAddress(string evmAddress, long shard, long realm)
        {
            return FromEvmAddress(EvmAddress.FromString(evmAddress), shard, realm);
        }
        /// <summary>
        /// Retrieve the account id from an EVM address.
        /// </summary>
        /// <param name="evmAddress">an EvmAddress instance</param>
        /// <returns>                         the account id object
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</returns>
        public static AccountId FromEvmAddress(EvmAddress evmAddress)
        {
            return FromEvmAddress(evmAddress, 0, 0);
        }
        /// <summary>
        /// Retrieve the account id from an EVM address.
        /// </summary>
        /// <param name="evmAddress">an EvmAddress instance</param>
        /// <param name="shard">the shard part of the account id</param>
        /// <param name="realm">the shard realm of the account id</param>
        /// <returns>                         the account id object
        /// 
        /// In case shard and realm are unknown, they should be set to zero</returns>
        public static AccountId FromEvmAddress(EvmAddress evmAddress, long shard, long realm)
        {
            Utils.EntityIdHelper.DecodeEvmAddress(evmAddress.ToString());

            return new AccountId(shard, realm, 0, null, null, evmAddress);
        }
        /// <summary>
        /// Retrieve the account id from a solidity address.
        /// </summary>
        /// <param name="address">a string representing the address</param>
        /// <returns>                         the account id object</returns>
        /// <remarks>@deprecatedThis method is deprecated. Use {@link #fromEvmAddress(EvmAddress, long, long)} instead.</remarks>
        public static AccountId FromSolidityAddress(string address)
        {
            if (Utils.EntityIdHelper.IsLongZeroAddress(Utils.EntityIdHelper.DecodeEvmAddress(address)))
            {
                return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new AccountId(a, b, c, d));
            }
            else
            {
                return FromEvmAddress(address, 0, 0);
            }
        }
        /// <summary>
        /// Retrieve the account id from a protobuf.
        /// </summary>
        /// <param name="accountId">the protobuf</param>
        /// <returns>                         the account id object</returns>
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

            return new AccountId(accountId.ShardNum, accountId.RealmNum, accountId.AccountNum, null, aliasKey, evmAddress);
        }
        /// <summary>
        /// Retrieve the account id from a protobuf byte array.
        /// </summary>
        /// <param name="bytes">a byte array representation of the protobuf</param>
        /// <returns>                         the account id object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static AccountId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.AccountID.Parser.ParseFrom(bytes));
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
		/// The public key bytes to be used as the account's alias
		/// </summary>
		public PublicKey? AliasKey { get; }
		/// <summary>
		/// The ethereum account 20-byte EVM address to be used initially in place of the public key bytes
		/// </summary>
		public EvmAddress? EvmAddress { get; }
		private string? Checksum { get; }

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
        /// toEvmAddress returns EVM-compatible address representation of the entity
        /// </summary>
        /// <returns></returns>
        public string ToEvmAddress()
        {
            if (EvmAddress != null)
            {
                return Hex.ToHexString(EvmAddress.ToBytes());
            }
            else
            {
                return Utils.EntityIdHelper.ToSolidityAddress(0, 0, Num);
            }
        }

		/// <summary>
		/// Extract the account id protobuf.
		/// </summary>
		/// <returns>                         the account id builder</returns>
		public Proto.AccountID ToProtobuf()
        {
            Proto.AccountID proto = new()
            {
                ShardNum = Shard,
                RealmNum = Realm,
            };

			if (AliasKey != null)
				proto.Alias = AliasKey.ToProtobufKey().ToByteString();
			else if (EvmAddress != null)
				proto.Alias = ByteString.CopyFrom(EvmAddress.ToBytes());
			else proto.AccountNum = Num;

			return proto;
        }

        /// <summary>
        /// Gets the actual `num` field of the `AccountId` from the Mirror Node.
        /// Should be used after generating `AccountId.fromEvmAddress()` because it sets the `num` field to `0`
        /// automatically since there is no connection between the `num` and the `evmAddress`
        /// Sync version
        /// </summary>
        /// <param name="client"></param>
        /// <returns>populated AccountId instance</returns>
        public AccountId PopulateAccountNum(Client client)
        {
            return PopulateAccountNumAsync(client).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the actual `num` field of the `AccountId` from the Mirror Node.
        /// Should be used after generating `AccountId.fromEvmAddress()` because it sets the `num` field to `0`
        /// automatically since there is no connection between the `num` and the `evmAddress`
        /// Async version
        /// </summary>
        /// <param name="client"></param>
        /// <returns>populated AccountId instance</returns>
        /// <remarks>@deprecatedUse 'populateAccountNum' instead due to its nearly identical operation.</remarks>
        public async Task<AccountId> PopulateAccountNumAsync(Client client)
        {
            return new AccountId(Shard, Realm, await Utils.EntityIdHelper.GetAccountNumFromMirrorNodeAsync(client, EvmAddress?.ToString()), Checksum, AliasKey, EvmAddress);
        }
        /// <summary>
        /// Populates `evmAddress` field of the `AccountId` extracted from the Mirror Node.
        /// Sync version
        /// </summary>
        /// <param name="client"></param>
        /// <returns>populated AccountId instance</returns>
        public AccountId PopulateAccountEvmAddress(Client client)
        {
            return PopulateAccountEvmAddressAsync(client).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Populates `evmAddress` field of the `AccountId` extracted from the Mirror Node.
        /// Async version
        /// </summary>
        /// <param name="client"></param>
        /// <returns>populated AccountId instance</returns>
        /// <remarks>@deprecatedUse 'populateAccountEvmAddress' instead due to its nearly identical operation.</remarks>
        public async Task<AccountId> PopulateAccountEvmAddressAsync(Client client)
        {
            return new AccountId(Shard, Realm, Num, Checksum, AliasKey, await Utils.EntityIdHelper.GetEvmAddressFromMirrorNodeAsync(client, Num));
        }

		public object Clone()
		{
			return new AccountId(Shard, Realm, Num, Checksum, AliasKey, EvmAddress);
		}
		public int CompareTo(AccountId? o)
		{
			int shardComparison = Shard.CompareTo(o?.Shard);
			if (shardComparison != 0)
			{
				return shardComparison;
			}

			int realmComparison = Realm.CompareTo(o?.Realm);
			if (realmComparison != 0)
			{
				return realmComparison;
			}

			int numComparison = Num.CompareTo(o?.Num);
			if (numComparison != 0)
			{
				return numComparison;
			}

			if ((AliasKey == null) != (o?.AliasKey == null))
			{
				return AliasKey != null ? 1 : -1;
			}

			if (AliasKey != null)
			{
				return AliasKey.ToStringDER().CompareTo(o?.AliasKey?.ToStringDER());
			}

			if ((EvmAddress == null) != (o?.EvmAddress == null))
			{
				return EvmAddress != null ? 1 : -1;
			}

			if (EvmAddress == null)
			{
				return 0;
			}

			return EvmAddress.ToString().CompareTo(o?.EvmAddress?.ToString());
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
        /// Verify that the client has a valid checksum.
        /// </summary>
        /// <param name="client">the client to verify</param>
        /// <exception cref="BadEntityIdException">when the account id and checksum are invalid</exception>
        public void ValidateChecksum(Client client)
        {
            if (AliasKey == null && EvmAddress == null)
            {
                Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
            }
        }

        /// <summary>
        /// Extract a byte array representation.
        /// </summary>
        /// <returns>                         a byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <summary>
		/// Extract a string representation with the checksum.
		/// </summary>
		/// <param name="client">the client</param>
		/// <returns>                         the account id with checksum</returns>
		public string ToStringWithChecksum(Client client)
        {
            if (AliasKey != null || EvmAddress != null)
            {
                throw new InvalidOperationException("toStringWithChecksum cannot be applied to AccountId with aliasKey or evmAddress");
            }
            else
            {
                return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
            }
        }

		public override int GetHashCode()
        {
            return HashCode.Combine(Shard, Realm, Num, HashCode.Combine(AliasKey?.ToBytes() ?? EvmAddress?.ToBytes()));
        }
        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o is not AccountId otherId)
            {
                return false;
            }

            if ((AliasKey == null) != (otherId.AliasKey == null))
            {
                return false;
            }

            if ((EvmAddress == null) != (otherId.EvmAddress == null))
            {
                return false;
            }

            return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num && (AliasKey == null || AliasKey.Equals(otherId.AliasKey)) && (EvmAddress == null || EvmAddress.Equals(otherId.EvmAddress));
        }
		public override string ToString()
		{
			if (AliasKey != null)
			{
				return "" + Shard + "." + Realm + "." + AliasKey.ToStringDER();
			}
			else if (EvmAddress != null)
			{
				return "" + Shard + "." + Realm + "." + EvmAddress.ToString();
			}
			else
			{
				return Utils.EntityIdHelper.ToString(Shard, Realm, Num);
			}
		}
    }
}