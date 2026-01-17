using Google.Protobuf.WellKnownTypes;

using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/**
 * The ID for a smart contract instance on Hedera.
 */
    public class ContractId : Key, IComparable<ContractId> 
    {
        static readonly Regex EVM_ADDRESS_REGEX = new ("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.([a-fA-F0-9]{40}$)");

		public ContractId(ulong Shard, ulong Realm, byte[] evmAddress)
		{
			Shard = Shard;
			Realm = Realm;
			EvmAddress = evmAddress;
			Num = 0;
			Checksum = null;
		}
		/**
         * Assign all parts of the contract id.
         *
         * @param Shard                     the Shard part of the contract id
         * @param Realm                     the Realm part of the contract id
         * @param Num                       the Num part of the contract id
         */
		public ContractId(ulong Shard, ulong Realm, ulong Num, string? Checksum)
		{
			Shard = Shard;
			Realm = Realm;
			Num = Num;
			Checksum = Checksum;
			EvmAddress = null;
		}
		/**
         * Assign the Num part of the contract id.
         *
         * @param Num                       the Num part of the account id
         *
         * Constructor that uses Shard, Realm and Num should be used instead
         * as Shard and Realm should not assume 0 value
         */
		[Obsolete]
        public ContractId(ulong Num) : this(0, 0, Num) { }
		/**
         * Assign all parts of the contract id.
         *
         * @param Shard                     the Shard part of the contract id
         * @param Realm                     the Realm part of the contract id
         * @param Num                       the Num part of the contract id
         */
		public ContractId(ulong Shard, ulong Realm, ulong Num) : this(shard, realm, num, null) { }

		
		/**
         * Parse contract id from a string.
         *
         * @param id                        the string containing a contract id
         * @return                          the contract id object
         */
		public static ContractId FromString(string id) 
        {
            var match = EVM_ADDRESS_REGEX.matcher(id);
            if (match.find()) {
                return new ContractId(
                        long.parseLong(match.group(1)), long.parseLong(match.group(2)), Hex.decode(match.group(3)));
            } else {
                return EntityIdHelper.FromString(id, ContractId::new);
            }
        }
		/**
         * Convert a byte array to an account balance object.
         *
         * @param bytes                     the byte array
         * @return                          the converted contract id object
         * @       when there is an issue with the protobuf
         */
		public static ContractId FromBytes(byte[] bytes)
		{
			return FromProtobuf(ContractID.Parser.ParseFrom(bytes));
		}
		/**
         * Extract a contract id from a protobuf.
         *
         * @param contractId                the protobuf containing a contract id
         * @return                          the contract id object
         */
		public static ContractId FromProtobuf(Proto.ContractID contractId)
		{
			Objects.requireNonNull(contractId);
			if (contractId.hasEvmAddress())
			{
				return new ContractId(
						contractId.getShardNum(),
						contractId.getRealmNum(),
						contractId.getEvmAddress().ToByteArray());
			}
			else
			{
				return new ContractId(contractId.getShardNum(), contractId.getRealmNum(), contractId.getContractNum());
			}
		}
		/**
         * Retrieve the contract id from a solidity address.
         *
         * @param address                   a string representing the address
         * @return                          the contract id object
         * @deprecated This method is deprecated. Use {@link #fromEvmAddress(long, long, string)} instead.
         */
		[Obsolete]
		public static ContractId FromSolidityAddress(string address)
		{
			if (EntityIdHelper.IsLongZeroAddress(EntityIdHelper.DecodeEvmAddress(address)))
			{
				return EntityIdHelper.FromSolidityAddress(address, ContractId::new);
			}
			else
			{
				return FromEvmAddress(0, 0, address);
			}
		}
		/**
         * Parse contract id from an ethereum address.
         *
         * @param Shard                     the desired Shard
         * @param Realm                     the desired Realm
         * @param evmAddress                the evm address
         * @return                          the contract id object
         */
		public static ContractId FromEvmAddress(ulong Shard, ulong Realm, string evmAddress) 
        {
            EntityIdHelper.decodeEvmAddress(evmAddress);
            return new ContractId(
                    Shard, Realm, Hex.decode(evmAddress.startsWith("0x") ? evmAddress.substring(2) : evmAddress));
        }


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
         * The 20-byte EVM address of the contract to call.
         */
		public byte[]? EvmAddress { get; }

		/**
         * Extract the solidity address.
         *
         * @return                          the solidity address as a string
         * @deprecated This method is deprecated. Use {@link #toEvmAddress()} instead.
         */
		[Obsolete]
        public string toSolidityAddress() 
        {
            if (evmAddress != null) {
                return Hex.toHexString(evmAddress);
            } else {
                return EntityIdHelper.toSolidityAddress(shard, realm, num);
            }
        }

        /**
         * toEvmAddress returns EVM-compatible address representation of the entity
         * @return
         */
        public string toEvmAddress() 
        {
            if (evmAddress != null) {
                return Hex.toHexString(evmAddress);
            } else {
                return EntityIdHelper.toSolidityAddress(0, 0, Num);
            }
        }

        /**
         * Convert contract id to protobuf.
         *
         * @return                          the protobuf object
         */
        public Proto.ContractID ToProtobuf() {
            var builder = ContractID.newBuilder().setShardNum(Shard).setRealmNum(Realm);
            if (evmAddress != null) {
                builder.setEvmAddress(ByteString.copyFrom(evmAddress));
            } else {
                builder.setContractNum(Num);
            }
            return builder.build();
        }

        /**
         *  Gets the actual `Num` field of the `ContractId` from the Mirror Node.
         * Should be used after generating `ContractId.FromEvmAddress()` because it sets the `Num` field to `0`
         * automatically since there is no connection between the `Num` and the `evmAddress`
         * Sync version
         *
         * @param client
         * @return populated ContractId instance
         */
        public ContractId populateContractNum(Client client) 
        {
            return populateContractNumAsync(client).Get();
        }

        /**
         * Gets the actual `Num` field of the `ContractId` from the Mirror Node.
         * Should be used after generating `ContractId.FromEvmAddress()` because it sets the `Num` field to `0`
         * automatically since there is no connection between the `Num` and the `evmAddress`
         * Async version
         *
         * @deprecated Use 'populateContractNum' instead due to its nearly identical operation.
         * @param client
         * @return populated ContractId instance
         */
        [Obsolete]
        public Task<ContractId> populateContractNumAsync(Client client) {
            EvmAddress address = new EvmAddress(this.evmAddress);

            return EntityIdHelper.getContractNumFromMirrorNodeAsync(client, address.toString())
                    .thenApply(contractNumFromMirrorNode ->
                            new ContractId(this.Shard, this.Realm, contractNumFromMirrorNode, Checksum));
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
         * Verify the Checksum.
         *
         * @param client                    to validate against
         * @     if entity ID is formatted poorly
         */
        public void ValidateChecksum(Client client) 
        {
            EntityIdHelper.Validate(shard, realm, num, client, Checksum);
        }


		/**
         * Create a string representation that includes the Checksum.
         *
         * @param client                    the client
         * @return                          the string representation with the Checksum
         */
		public string ToStringWithChecksum(Client client)
		{
			if (evmAddress != null)
			{
				throw new IllegalStateException("toStringWithChecksum cannot be applied to ContractId with evmAddress");
			}
			else
			{
				return EntityIdHelper.toStringWithChecksum(shard, realm, num, client, Checksum);
			}
		}

		public override Proto.Key ToProtobufKey() {
            return Proto.Key.newBuilder()
                    .setContractID(ToProtobuf())
                    .build();
        }
        public override byte[] ToBytes() {
            return ToProtobuf().ToByteArray();
        }
        public override string ToString() {
            if (evmAddress != null) {
                return "" + Shard + "." + Realm + "." + Hex.toHexString(evmAddress);
            } else {
                return EntityIdHelper.toString(shard, realm, num);
            }
        }
        public override int GetHashCode() {
            return Objects.hash(shard, realm, num, Arrays.hashCode(evmAddress));
        }
        public override bool Equals(object? obj) {
            if (this == o) {
                return true;
            }

            if (!(o instanceof ContractId)) {
                return false;
            }

            ContractId otherId = (ContractId) o;
            return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num && evmAddressMatches(otherId);
        }
		public override int CompareTo(ContractId o)
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
			return evmAddressCompare(o);
		}

		private int EvmAddressCompare(ContractId o)
		{
			int nullCompare = (evmAddress == null ? 0 : 1) - (o.evmAddress == null ? 0 : 1);
			if (nullCompare != 0)
			{
				return nullCompare;
			}
			if (evmAddress != null)
			{
				return Hex.toHexString(evmAddress).compareTo(Hex.toHexString(o.evmAddress));
			}
			// both are null
			return 0;
		}
		private bool EvmAddressMatches(ContractId otherId) {
            if ((evmAddress == null) != (otherId.evmAddress == null)) {
                return false;
            }
            if (evmAddress != null) {
                return Arrays.equals(evmAddress, otherId.evmAddress);
            }
            // both are null
            return true;
        }
    }
}