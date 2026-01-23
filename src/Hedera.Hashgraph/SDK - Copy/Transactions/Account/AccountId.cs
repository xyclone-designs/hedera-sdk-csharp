// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Proto;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Regex;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /// <summary>
    /// The ID for a cryptocurrency account on Hedera.
    /// </summary>
    public sealed class AccountId : IComparable<AccountId>
    {
        private static readonly Regex ALIAS_ID_REGEX = new ("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.((?:[0-9a-fA-F][0-9a-fA-F])+)$");
        /// <summary>
        /// The shard number
        /// </summary>
        public readonly long Shard;
        /// <summary>
        /// The realm number
        /// </summary>
        public readonly long Realm;
        /// <summary>
        /// The id number
        /// </summary>
        public readonly long Num;
        /// <summary>
        /// The public key bytes to be used as the account's alias
        /// </summary>
        public readonly PublicKey AliasKey;
        /// <summary>
        /// The ethereum account 20-byte EVM address to be used initially in place of the public key bytes
        /// </summary>
        public readonly EvmAddress EvmAddress;
        private readonly string checksum;
        /// <summary>
        /// Assign the num part of the account id.
        /// </summary>
        /// <param name="num">the num part of the account id
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public AccountId(long num) : this(0, 0, num)
        {
        }

        /// <summary>
        /// Assign all parts of the account id.
        /// </summary>
        /// <param name="shard">the shard part of the account id</param>
        /// <param name="realm">the realm part of the account id</param>
        /// <param name="num">the num part of the account id</param>
        public AccountId(long shard, long realm, long num) : this(shard, realm, num, null)
        {
        }

        /// <summary>
        /// Assign all parts of the account id.
        /// </summary>
        /// <param name="shard">the shard part of the account id</param>
        /// <param name="realm">the realm part of the account id</param>
        /// <param name="num">the num part of the account id</param>
        AccountId(long shard, long realm, long num, string checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            checksum = checksum;
            AliasKey = null;
            EvmAddress = null;
        }

        /// <summary>
        /// Assign all parts of the account id.
        /// </summary>
        /// <param name="shard">the shard part of the account id</param>
        /// <param name="realm">the realm part of the account id</param>
        /// <param name="num">the num part of the account id</param>
        AccountId(long shard, long realm, long num, string checksum, PublicKey aliasKey, EvmAddress evmAddress)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            checksum = checksum;
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
            catch (ArgumentException error)
            {
                var match = ALIAS_ID_REGEX.Matcher(id);
                if (!match.Find())
                {
                    throw new ArgumentException("Invalid Account ID \"" + id + "\": format should look like 0.0.123 or 0.0.123-vfmkw or 0.0.1337BEEF (where 1337BEEF is a hex-encoded, DER-format public key)");
                }
                else
                {
                    byte[] aliasBytes = Hex.Decode(match.Group(3));
                    bool isEvmAddress = aliasBytes.Length == 20;
                    return new AccountId(long.Parse(match.Group(1)), long.Parse(match.Group(2)), 0, null, isEvmAddress ? null : PublicKey.FromBytesDER(aliasBytes), isEvmAddress ? EvmAddress.FromBytes(aliasBytes) : null);
                }
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
            PublicKey aliasKey = null;
            EvmAddress evmAddress = null;
            if (accountId.HasAlias())
            {
                if (accountId.GetAlias().Count == 20)
                {
                    evmAddress = SDK.EvmAddress.FromAliasBytes(accountId.GetAlias());
                }
                else
                {
                    aliasKey = PublicKey.FromAliasBytes(accountId.GetAlias());
                }
            }

            Objects.RequireNonNull(accountId);
            return new AccountId(accountId.ShardNum, accountId.RealmNum, accountId.GetAccountNum(), null, aliasKey, evmAddress);
        }

        /// <summary>
        /// Retrieve the account id from a protobuf byte array.
        /// </summary>
        /// <param name="bytes">a byte array representation of the protobuf</param>
        /// <returns>                         the account id object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static AccountId FromBytes(byte[] bytes)
        {
            return FromProtobuf(AccountID.Parser.ParseFrom(bytes));
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
            var accountIdBuilder = AccountID.NewBuilder().SetShardNum(Shard).SetRealmNum(Realm);
            if (AliasKey != null)
            {
                accountIdBuilder.SetAlias(AliasKey.ToProtobufKey().ToByteString());
            }
            else if (EvmAddress != null)
            {
                accountIdBuilder.SetAlias(ByteString.CopyFrom(EvmAddress.ToBytes()));
            }
            else
            {
                accountIdBuilder.SetAccountNum(Num);
            }

            return accountIdBuilder.Build();
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
            return PopulateAccountNumAsync(client).Get();
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
        public CompletableFuture<AccountId> PopulateAccountNumAsync(Client client)
        {
            return Utils.EntityIdHelper.GetAccountNumFromMirrorNodeAsync(client, EvmAddress.ToString()).ThenApply((accountNumFromMirrorNode) => new AccountId(Shard, Realm, accountNumFromMirrorNode, checksum, AliasKey, EvmAddress));
        }

        /// <summary>
        /// Populates `evmAddress` field of the `AccountId` extracted from the Mirror Node.
        /// Sync version
        /// </summary>
        /// <param name="client"></param>
        /// <returns>populated AccountId instance</returns>
        public AccountId PopulateAccountEvmAddress(Client client)
        {
            return PopulateAccountEvmAddressAsync(client).Get();
        }

        /// <summary>
        /// Populates `evmAddress` field of the `AccountId` extracted from the Mirror Node.
        /// Async version
        /// </summary>
        /// <param name="client"></param>
        /// <returns>populated AccountId instance</returns>
        /// <remarks>@deprecatedUse 'populateAccountEvmAddress' instead due to its nearly identical operation.</remarks>
        public CompletableFuture<AccountId> PopulateAccountEvmAddressAsync(Client client)
        {
            return Utils.EntityIdHelper.GetEvmAddressFromMirrorNodeAsync(client, Num).ThenApply((evmAddressFromMirrorNode) => new AccountId(Shard, Realm, Num, checksum, AliasKey, evmAddressFromMirrorNode));
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
                Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, checksum);
            }
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
        /// Extract a byte array representation.
        /// </summary>
        /// <returns>                         a byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
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

        /// <summary>
        /// Extract a string representation with the checksum.
        /// </summary>
        /// <param name="client">the client</param>
        /// <returns>                         the account id with checksum</returns>
        public override string ToStringWithChecksum(Client client)
        {
            if (AliasKey != null || EvmAddress != null)
            {
                throw new InvalidOperationException("toStringWithChecksum cannot be applied to AccountId with aliasKey or evmAddress");
            }
            else
            {
                return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, checksum);
            }
        }

        public override int GetHashCode()
        {
            byte[] aliasBytes = null;
            if (AliasKey != null)
            {
                aliasBytes = AliasKey.ToBytes();
            }
            else if (EvmAddress != null)
            {
                aliasBytes = EvmAddress.ToBytes();
            }

            return HashCode.Combine(Shard, Realm, Num, HashCode.Combine(aliasBytes));
        }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is AccountId))
            {
                return false;
            }

            AccountId otherId = (AccountId)o;
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

        public override int CompareTo(AccountId o)
        {
            Objects.RequireNonNull(o);
            int shardComparison = Long.Compare(Shard, o.Shard);
            if (shardComparison != 0)
            {
                return shardComparison;
            }

            int realmComparison = Long.Compare(Realm, o.Realm);
            if (realmComparison != 0)
            {
                return realmComparison;
            }

            int numComparison = Long.Compare(Num, o.Num);
            if (numComparison != 0)
            {
                return numComparison;
            }

            if ((AliasKey == null) != (o.AliasKey == null))
            {
                return AliasKey != null ? 1 : -1;
            }

            if (AliasKey != null)
            {
                return AliasKey.ToStringDER().CompareTo(o.AliasKey.ToStringDER());
            }

            if ((EvmAddress == null) != (o.EvmAddress == null))
            {
                return EvmAddress != null ? 1 : -1;
            }

            if (EvmAddress == null)
            {
                return 0;
            }

            return EvmAddress.ToString().CompareTo(o.EvmAddress.ToString());
        }
    }
}