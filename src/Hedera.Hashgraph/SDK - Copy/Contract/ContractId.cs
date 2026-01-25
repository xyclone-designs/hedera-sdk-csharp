// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Keys;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// The ID for a smart contract instance on Hedera.
    /// </summary>
    public class ContractId : Key, IComparable<ContractId>
    {
        public static readonly Regex EVM_ADDRESS_REGEX = new ("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.([a-fA-F0-9]{40}$)");

        /// <summary>
        /// Assign the num part of the contract id.
        /// </summary>
        /// <param name="num">the num part of the account id
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public ContractId(long num) : this(0, 0, num)
        {
        }
        /// <summary>
        /// Assign all parts of the contract id.
        /// </summary>
        /// <param name="shard">the shard part of the contract id</param>
        /// <param name="realm">the realm part of the contract id</param>
        /// <param name="num">the num part of the contract id</param>
        public ContractId(long shard, long realm, long num) : this(shard, realm, num, null)
        {
        }

		internal ContractId(long shard, long realm, byte[] evmAddress)
		{
			Shard = shard;
			Realm = realm;
			EvmAddress = evmAddress;
			Num = 0;
			Checksum = null;
		}
		/// <summary>
		/// Assign all parts of the contract id.
		/// </summary>
		/// <param name="shard">the shard part of the contract id</param>
		/// <param name="realm">the realm part of the contract id</param>
		/// <param name="num">the num part of the contract id</param>
		internal ContractId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
            EvmAddress = null;
        }

        /// <summary>
        /// Parse contract id from a string.
        /// </summary>
        /// <param name="id">the string containing a contract id</param>
        /// <returns>                         the contract id object</returns>
        public static ContractId FromString(string id)
        {
            MatchCollection match = EVM_ADDRESS_REGEX.Matches(id);
            if (match.Any())
            {
                return new ContractId(long.Parse(match.ElementAt(1).Value), long.Parse(match.ElementAt(2).Value), Hex.Decode(match.ElementAt(3).Value));
            }
            else
            {
                return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new ContractId(a, b, c, d));
            }
        }
        /// <summary>
        /// Retrieve the contract id from a solidity address.
        /// </summary>
        /// <param name="address">a string representing the address</param>
        /// <returns>                         the contract id object</returns>
        /// <remarks>@deprecatedThis method is deprecated. Use {@link #fromEvmAddress(long, long, String)} instead.</remarks>
        public static ContractId FromSolidityAddress(string address)
        {
            if (Utils.EntityIdHelper.IsLongZeroAddress(Utils.EntityIdHelper.DecodeEvmAddress(address)))
            {
                return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new ContractId(a, b, c, d));
            }
            else
            {
                return FromEvmAddress(0, 0, address);
            }
        }
        /// <summary>
        /// Parse contract id from an ethereum address.
        /// </summary>
        /// <param name="shard">the desired shard</param>
        /// <param name="realm">the desired realm</param>
        /// <param name="evmAddress">the evm address</param>
        /// <returns>                         the contract id object</returns>
        public static ContractId FromEvmAddress(long shard, long realm, string evmAddress)
        {
            Utils.EntityIdHelper.DecodeEvmAddress(evmAddress);
            return new ContractId(shard, realm, Hex.Decode(evmAddress.StartsWith("0x") ? evmAddress.Substring(2) : evmAddress));
        }
        /// <summary>
        /// Extract a contract id from a protobuf.
        /// </summary>
        /// <param name="contractId">the protobuf containing a contract id</param>
        /// <returns>                         the contract id object</returns>
        public static ContractId FromProtobuf(Proto.ContractID contractId)
        {
            if (contractId.HasEvmAddress)
            {
                return new ContractId(contractId.ShardNum, contractId.RealmNum, contractId.EvmAddress.ToByteArray());
            }
            else
            {
                return new ContractId(contractId.ShardNum, contractId.RealmNum, contractId.ContractNum);
            }
        }
        /// <summary>
        /// Convert a byte array to an account balance object.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the converted contract id object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public new static ContractId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.ContractID.Parser.ParseFrom(bytes));
        }

		private string? Checksum { get; }

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
		/// The 20-byte EVM address of the contract to call.
		/// </summary>
		public byte[]? EvmAddress { get; }

		/// <summary>
		/// Extract the solidity address.
		/// </summary>
		/// <returns>                         the solidity address as a string</returns>
		/// <remarks>@deprecatedThis method is deprecated. Use {@link #toEvmAddress()} instead.</remarks>
		public virtual string ToSolidityAddress()
        {
            if (EvmAddress != null)
            {
                return Hex.ToHexString(EvmAddress);
            }
            else
            {
                return Utils.EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
            }
        }

        /// <summary>
        /// toEvmAddress returns EVM-compatible address representation of the entity
        /// </summary>
        /// <returns></returns>
        public virtual string ToEvmAddress()
        {
            if (EvmAddress != null)
            {
                return Hex.ToHexString(EvmAddress);
            }
            else
            {
                return Utils.EntityIdHelper.ToSolidityAddress(0, 0, Num);
            }
        }

        /// <summary>
        /// Convert contract id to protobuf.
        /// </summary>
        /// <returns>                         the protobuf object</returns>
        public virtual Proto.ContractID ToProtobuf()
        {
			Proto.ContractID proto = new ()
            {
				ShardNum = Shard,
				RealmNum = Realm,
                ContractNum = Num,
			};

            if (EvmAddress != null) proto.EvmAddress = ByteString.CopyFrom(EvmAddress);

            return proto;
        }

        /// <summary>
        ///  Gets the actual `num` field of the `ContractId` from the Mirror Node.
        /// Should be used after generating `ContractId.fromEvmAddress()` because it sets the `num` field to `0`
        /// automatically since there is no connection between the `num` and the `evmAddress`
        /// Sync version
        /// </summary>
        /// <param name="client"></param>
        /// <returns>populated ContractId instance</returns>
        public virtual ContractId PopulateContractNum(Client client)
        {
            return PopulateContractNumAsync(client).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the actual `num` field of the `ContractId` from the Mirror Node.
        /// Should be used after generating `ContractId.fromEvmAddress()` because it sets the `num` field to `0`
        /// automatically since there is no connection between the `num` and the `evmAddress`
        /// Async version
        /// </summary>
        /// <param name="client"></param>
        /// <returns>populated ContractId instance</returns>
        /// <remarks>@deprecatedUse 'populateContractNum' instead due to its nearly identical operation.</remarks>
        public virtual Task<ContractId> PopulateContractNumAsync(Client client)
        {
            EvmAddress address = new (EvmAddress);
            return Utils.EntityIdHelper.GetContractNumFromMirrorNodeAsync(client, address.ToString()).ThenApply((contractNumFromMirrorNode) => new ContractId(Shard, Realm, contractNumFromMirrorNode, Checksum));
        }

        /// <summary>
        /// </summary>
        /// <param name="client">to validate against</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        /// <remarks>@deprecatedUse {@link #validateChecksum(Client)} instead.</remarks>
        public virtual void Validate(Client client)
        {
            ValidateChecksum(client);
        }

        /// <summary>
        /// Verify the checksum.
        /// </summary>
        /// <param name="client">to validate against</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        public virtual void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
        }

        /// <summary>
        /// Extract the checksum.
        /// </summary>
        /// <returns>                         the checksum</returns>
        public virtual string GetChecksum()
        {
            return Checksum;
        }

        public override Proto.Key ToProtobufKey()
        {
            return new Proto.Key 
            {
                ContractID = ToProtobuf(),
            };
        }

        public override byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString()
        {
            if (EvmAddress != null)
            {
                return "" + Shard + "." + Realm + "." + Hex.ToHexString(EvmAddress);
            }
            else
            {
                return Utils.EntityIdHelper.ToString(Shard, Realm, Num);
            }
        }

        /// <summary>
        /// Create a string representation that includes the checksum.
        /// </summary>
        /// <param name="client">the client</param>
        /// <returns>                         the string representation with the checksum</returns>
        public virtual string ToStringWithChecksum(Client client)
        {
            if (EvmAddress != null)
            {
                throw new InvalidOperationException("toStringWithChecksum cannot be applied to ContractId with evmAddress");
            }
            else
            {
                return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Shard, Realm, Num, HashCode.Combine(EvmAddress));
        }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is ContractId))
            {
                return false;
            }

            ContractId otherId = (ContractId)o;
            return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num && EvmAddressMatches(otherId);
        }

        private bool EvmAddressMatches(ContractId otherId)
        {
            if ((EvmAddress == null) != (otherId.EvmAddress == null))
            {
                return false;
            }

            if (EvmAddress != null)
            {
                return Equals(EvmAddress, otherId.EvmAddress);
            }


            // both are null
            return true;
        }

        public int CompareTo(ContractId? o)
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

            return EvmAddressCompare(o);
        }

        private int EvmAddressCompare(ContractId? o)
        {
            int nullCompare = (EvmAddress == null ? 0 : 1) - (o?.EvmAddress == null ? 0 : 1);
            if (nullCompare != 0)
            {
                return nullCompare;
            }

            if (EvmAddress != null)
            {
                return Hex.ToHexString(EvmAddress).CompareTo(Hex.ToHexString(o?.EvmAddress));
            }


            // both are null
            return 0;
        }
    }
}