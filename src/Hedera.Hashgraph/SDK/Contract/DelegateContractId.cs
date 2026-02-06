// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// The ID for a smart contract instance on Hedera.
    /// </summary>
    public sealed class DelegateContractId : ContractId
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="num">the num portion of the contract id
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public DelegateContractId(long num) : base(num) { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shard">the shard portion of the contract id</param>
        /// <param name="realm">the realm portion of the contract id</param>
        /// <param name="num">the num portion of the contract id</param>
        public DelegateContractId(long shard, long realm, long num) : base(shard, realm, num) { }
        public DelegateContractId(long shard, long realm, byte[] evmAddress) : base(shard, realm, evmAddress) { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shard">the shard portion of the contract id</param>
		/// <param name="realm">the realm portion of the contract id</param>
		/// <param name="num">the num portion of the contract id</param>
		/// <param name="checksum">the optional checksum</param>
		DelegateContractId(long shard, long realm, long num, string? checksum) : base(shard, realm, num, checksum) { }

		/// <summary>
		/// Create a delegate contract id from a string.
		/// </summary>
		/// <param name="id">the contract id</param>
		/// <returns>                         the delegate contract id object</returns>
		public new static DelegateContractId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new DelegateContractId(a, b, c, d));
        }
        /// <summary>
        /// Create a delegate contract id from a string.
        /// </summary>
        /// <param name="address">the contract id solidity address</param>
        /// <returns>                         the delegate contract id object</returns>
        public new static DelegateContractId FromSolidityAddress(string address)
        {
            return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new DelegateContractId(a, b, c, d));
        }
        /// <summary>
        /// Parse DelegateContract id from an ethereum address.
        /// </summary>
        /// <param name="shard">the desired shard</param>
        /// <param name="realm">the desired realm</param>
        /// <param name="evmAddress">the evm address</param>
        /// <returns>                         the contract id object</returns>
        public new static DelegateContractId FromEvmAddress(long shard, long realm, string evmAddress)
        {
            Utils.EntityIdHelper.DecodeEvmAddress(evmAddress);

            return new DelegateContractId(shard, realm, Hex.Decode(evmAddress.StartsWith("0x") ? evmAddress.Substring(2) : evmAddress));
        }
        /// <summary>
        /// Create a delegate contract id from a string.
        /// </summary>
        /// <param name="contractId">the contract id protobuf</param>
        /// <returns>                         the delegate contract id object</returns>
        public new static DelegateContractId FromProtobuf(Proto.ContractID contractId)
        {
            return new DelegateContractId(contractId.ShardNum, contractId.RealmNum, contractId.ContractNum);
        }
        /// <summary>
        /// Create a delegate contract id from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the delegate contract id object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public new static DelegateContractId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.ContractID.Parser.ParseFrom(bytes));
        }

        public override Proto.Key ToProtobufKey()
        {
            return new Proto.Key
            {
				DelegatableContractId = ToProtobuf()
			};
        }
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o is DelegateContractId otherId)
            {
                return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num;
            }
            else if (o is ContractId _otherId)
			{
                return Shard == _otherId.Shard && Realm == _otherId.Realm && Num == _otherId.Num;
            }
            else
            {
                return false;
            }
        }
    }
}