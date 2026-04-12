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
	/// <include file="AccountId.cs.xml" path='docs/member[@name="T:AccountId"]/*' />
	public sealed class AccountId : IComparable<AccountId>, ICloneable
    {
        private static readonly Regex ALIAS_ID_REGEX = new ("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.((?:[0-9a-fA-F][0-9a-fA-F])+)$");

		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.#ctor(System.Int64)"]/*' />
		public AccountId(long num) : this(0, 0, num) { }
		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.#ctor(System.Int64,System.Int64,System.Int64)"]/*' />
		public AccountId(long shard, long realm, long num) : this(shard, realm, num, null) { }
		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.#ctor(System.Int64,System.Int64,System.Int64,System.String)"]/*' />
		public AccountId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
			Checksum = checksum;
            AliasKey = null;
            EvmAddress = null;
        }
		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.#ctor(System.Int64,System.Int64,System.Int64,System.String,PublicKey,EvmAddress)"]/*' />
		public AccountId(long shard, long realm, long num, string? checksum, PublicKey? aliasKey, EvmAddress? evmAddress)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
            AliasKey = aliasKey;
            EvmAddress = evmAddress;
        }

        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.FromString(System.String)"]/*' />
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
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.FromEvmAddress(System.String)"]/*' />
        public static AccountId FromEvmAddress(string evmAddress)
        {
            return FromEvmAddress(evmAddress, 0, 0);
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.FromEvmAddress(System.String,System.Int64,System.Int64)"]/*' />
        public static AccountId FromEvmAddress(string evmAddress, long shard, long realm)
        {
            return FromEvmAddress(EvmAddress.FromString(evmAddress), shard, realm);
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.FromEvmAddress(EvmAddress)"]/*' />
        public static AccountId FromEvmAddress(EvmAddress evmAddress)
        {
            return FromEvmAddress(evmAddress, 0, 0);
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.FromEvmAddress(EvmAddress,System.Int64,System.Int64)"]/*' />
        public static AccountId FromEvmAddress(EvmAddress evmAddress, long shard, long realm)
        {
            Utils.EntityIdHelper.DecodeEvmAddress(evmAddress.ToString());

            return new AccountId(shard, realm, 0, null, null, evmAddress);
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.FromSolidityAddress(System.String)"]/*' />
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
		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.FromProtobuf(Proto.Services.AccountID)"]/*' />
		public static AccountId FromProtobuf(Proto.Services.AccountID accountId)
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
		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.FromBytes(System.Byte[])"]/*' />
		public static AccountId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.AccountID.Parser.ParseFrom(bytes));
        }

		/// <include file="AccountId.cs.xml" path='docs/member[@name="P:AccountId.Shard"]/*' />
		public long Shard { get; }
		/// <include file="AccountId.cs.xml" path='docs/member[@name="P:AccountId.Realm"]/*' />
		public long Realm { get; }
		/// <include file="AccountId.cs.xml" path='docs/member[@name="P:AccountId.Num"]/*' />
		public long Num { get; }
		/// <include file="AccountId.cs.xml" path='docs/member[@name="P:AccountId.AliasKey"]/*' />
		public PublicKey? AliasKey { get; }
		/// <include file="AccountId.cs.xml" path='docs/member[@name="P:AccountId.EvmAddress"]/*' />
		public EvmAddress? EvmAddress { get; }
		private string? Checksum { get; }

		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.ToSolidityAddress"]/*' />
		public string ToSolidityAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.ToEvmAddress"]/*' />
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

		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.ToProtobuf"]/*' />
		public Proto.Services.AccountID ToProtobuf()
        {
            Proto.Services.AccountID proto = new()
            {
                ShardNum = Shard,
                RealmNum = Realm,
            };

			if (AliasKey != null)
				Proto.Services.Alias = AliasKey.ToProtobufKey().ToByteString();
			else if (EvmAddress != null)
				Proto.Services.Alias = ByteString.CopyFrom(EvmAddress.ToBytes());
			else Proto.Services.AccountNum = Num;

			return proto;
        }

        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.PopulateAccountNum(Client)"]/*' />
        public AccountId PopulateAccountNum(Client client)
        {
            return PopulateAccountNumAsync(client).GetAwaiter().GetResult();
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.PopulateAccountNumAsync(Client)"]/*' />
        public async Task<AccountId> PopulateAccountNumAsync(Client client)
        {
            return new AccountId(Shard, Realm, await Utils.EntityIdHelper.GetAccountNumFromMirrorNodeAsync(client, EvmAddress?.ToString()), Checksum, AliasKey, EvmAddress);
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.PopulateAccountEvmAddress(Client)"]/*' />
        public AccountId PopulateAccountEvmAddress(Client client)
        {
            return PopulateAccountEvmAddressAsync(client).GetAwaiter().GetResult();
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.PopulateAccountEvmAddressAsync(Client)"]/*' />
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

		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.Validate(Client)"]/*' />
		public void Validate(Client client)
        {
            ValidateChecksum(client);
        }
        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.ValidateChecksum(Client)"]/*' />
        public void ValidateChecksum(Client client)
        {
            if (AliasKey == null && EvmAddress == null)
            {
                Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
            }
        }

        /// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.ToBytes"]/*' />
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <include file="AccountId.cs.xml" path='docs/member[@name="M:AccountId.ToStringWithChecksum(Client)"]/*' />
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
