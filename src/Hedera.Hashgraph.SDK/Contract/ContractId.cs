// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Keys;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractId.cs.xml" path='docs/member[@name="T:ContractId"]/*' />
    public class ContractId : Key, IComparable<ContractId>
    {
        public static readonly Regex EVM_ADDRESS_REGEX = new ("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.([a-fA-F0-9]{40}$)");

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.#ctor(System.Int64)"]/*' />
        public ContractId(long num) : this(0, 0, num) { }
        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.#ctor(System.Int64,System.Int64,System.Int64)"]/*' />
        public ContractId(long shard, long realm, long num) : this(shard, realm, num, null) { }

		internal ContractId(long shard, long realm, byte[] evmAddress)
		{
			Shard = shard;
			Realm = realm;
			EvmAddress = evmAddress;
			Num = 0;
			Checksum = null;
		}
		/// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.#ctor(System.Int64,System.Int64,System.Int64,System.String)"]/*' />
		internal ContractId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
            EvmAddress = null;
        }

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.FromString(System.String)"]/*' />
        public static ContractId FromString(string id)
        {
            MatchCollection match = EVM_ADDRESS_REGEX.Matches(id);

            if (match.Count == 0)
            {
                return new ContractId(long.Parse(match.ElementAt(1).Value), long.Parse(match.ElementAt(2).Value), Hex.Decode(match.ElementAt(3).Value));
            }
            else
            {
                return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new ContractId(a, b, c, d));
            }
        }
        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.FromSolidityAddress(System.String)"]/*' />
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
        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.FromEvmAddress(System.Int64,System.Int64,System.String)"]/*' />
        public static ContractId FromEvmAddress(long shard, long realm, string evmAddress)
        {
            Utils.EntityIdHelper.DecodeEvmAddress(evmAddress);
            return new ContractId(shard, realm, Hex.Decode(evmAddress.StartsWith("0x") ? evmAddress.Substring(2) : evmAddress));
        }
        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.FromProtobuf(Proto.Services.ContractId)"]/*' />
        public static ContractId FromProtobuf(Proto.Services.ContractID contractId)
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
        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.FromBytes(System.Byte[])"]/*' />
        public new static ContractId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.ContractID.Parser.ParseFrom(bytes));
        }

		/// <include file="ContractId.cs.xml" path='docs/member[@name="P:ContractId.Shard"]/*' />
		public long Shard { get; }
		/// <include file="ContractId.cs.xml" path='docs/member[@name="P:ContractId.Realm"]/*' />
		public long Realm { get; }
		/// <include file="ContractId.cs.xml" path='docs/member[@name="P:ContractId.Num"]/*' />
		public long Num { get; }
		/// <include file="ContractId.cs.xml" path='docs/member[@name="P:ContractId.EvmAddress"]/*' />
		public byte[]? EvmAddress { get; }
		/// <include file="ContractId.cs.xml" path='docs/member[@name="P:ContractId.Checksum"]/*' />
		public string? Checksum { get; }

		/// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.ToSolidityAddress"]/*' />
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

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.ToEvmAddress"]/*' />
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

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.ToProtobuf"]/*' />
        public virtual Proto.Services.ContractID ToProtobuf()
        {
			Proto.Services.ContractID proto = new ()
            {
				ShardNum = Shard,
				RealmNum = Realm,
                ContractNum = Num,
			};

            if (EvmAddress != null) proto.EvmAddress = ByteString.CopyFrom(EvmAddress);

            return proto;
        }

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.PopulateContractNum(Client)"]/*' />
        public virtual ContractId PopulateContractNum(Client client)
        {
            return PopulateContractNumAsync(client).GetAwaiter().GetResult();
        }

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.PopulateContractNumAsync(Client)"]/*' />
        public virtual async Task<ContractId> PopulateContractNumAsync(Client client)
        {
			long contractnum = await Utils.EntityIdHelper.GetContractNumFromMirrorNodeAsync(client, Ethereum.EvmAddress.FromBytes(EvmAddress).ToString());

			return new ContractId(Shard, Realm, contractnum, Checksum);
        }

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.Validate(Client)"]/*' />
        public virtual void Validate(Client client)
        {
            ValidateChecksum(client);
        }

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.ValidateChecksum(Client)"]/*' />
        public virtual void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
        }

        public override Proto.Services.Key ToProtobufKey()
        {
            return new Proto.Services.Key 
            {
                ContractId = ToProtobuf(),
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

        /// <include file="ContractId.cs.xml" path='docs/member[@name="M:ContractId.ToStringWithChecksum(Client)"]/*' />
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
