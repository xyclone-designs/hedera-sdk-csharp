using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * The ID for a smart contract instance on Hedera.
     */
    public sealed class DelegateContractId : ContractId 
    {
        /**
            * Constructor.
            *
            * @param Num                       the Num portion of the contract id
            *
            * Constructor that uses Shard, Realm and Num should be used instead
            * as Shard and Realm should not assume 0 value
            */
        [Obsolete]
        public DelegateContractId(ulong Num) : base(Num) { }
		/**
            * Constructor.
            *
            * @param Shard                     the Shard portion of the contract id
            * @param Realm                     the Realm portion of the contract id
            * @param Num                       the Num portion of the contract id
            */
		public DelegateContractId(ulong Shard, ulong Realm, ulong Num) : base(shard, realm, num) { }
        /**
            * Constructor.
            *
            * @param Shard                     the Shard portion of the contract id
            * @param Realm                     the Realm portion of the contract id
            * @param Num                       the Num portion of the contract id
            * @param Checksum                  the optional Checksum
            */
        public DelegateContractId(ulong Shard, ulong Realm, ulong Num, string? Checksum) : base(shard, realm, num, Checksum) { }

		public DelegateContractId(ulong Shard, ulong Realm, byte[] evmAddress) : base(Shard, Realm, evmAddress) { }

		/**
            * Create a delegate contract id from a string.
            *
            * @param contractId                the contract id protobuf
            * @return                          the delegate contract id object
            */
		public static DelegateContractId FromProtobuf(Proto.ContractID contractId)
		{
			return new DelegateContractId((ulong)contractId.ShardNum, (ulong)contractId.RealmNum, (ulong)contractId.ContractNum);
		}
		/**
            * Create a delegate contract id from a string.
            *
            * @param id                        the contract id
            * @return                          the delegate contract id object
            */
		public new static DelegateContractId FromString(string id) 
        {
            return EntityIdHelper.FromString(id, (a, b, c, d) => new DelegateContractId((ulong)a, (ulong)b, (ulong)c, d));
        }
        /**
            * Create a delegate contract id from a string.
            *
            * @param address                   the contract id solidity address
            * @return                          the delegate contract id object
            */
        [Obsolete]
        public new static DelegateContractId FromSolidityAddress(string address) 
        {
            return EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new DelegateContractId((ulong)a, (ulong)b, (ulong)c, d));
		}
		/**
            * Create a delegate contract id from a byte array.
            *
            * @param bytes                     the byte array
            * @return                          the delegate contract id object
            * @       when there is an issue with the protobuf
            */
		public new static DelegateContractId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.ContractID.Parser.ParseFrom(bytes));
		}
		/**
            * Parse DelegateContract id from an ethereum address.
            *
            * @param Shard                     the desired Shard
            * @param Realm                     the desired Realm
            * @param evmAddress                the evm address
            * @return                          the contract id object
            */
		public new static DelegateContractId FromEvmAddress(ulong Shard, ulong Realm, string evmAddress) 
        {
            EntityIdHelper.DecodeEvmAddress(evmAddress);

            return new DelegateContractId(Shard, Realm, Hex.Decode(evmAddress.StartsWith("0x") ? evmAddress[2..] : evmAddress));
        }

		public override Proto.Key ToProtobufKey() 
        {
            return new Proto.Key 
            {
				DelegatableContractId = ToProtobuf()
			};
        }
        public override bool Equals(object? o) 
        {
            if (this == o) return true;

            if (o is DelegateContractId delegatecontractid)
				return Shard == delegatecontractid.Shard && Realm == delegatecontractid.Realm && Num == delegatecontractid.Num;

			if (o is ContractId contractid)
				return Shard == contractid.Shard && Realm == contractid.Realm && Num == contractid.Num;

			return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}