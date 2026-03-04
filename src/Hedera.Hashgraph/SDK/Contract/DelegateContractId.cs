// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="DelegateContractId.cs.xml" path='docs/member[@name="T:DelegateContractId"]/*' />
    public sealed class DelegateContractId : ContractId
    {
        /// <include file="DelegateContractId.cs.xml" path='docs/member[@name="M:DelegateContractId.#ctor(System.Int64)"]/*' />
        public DelegateContractId(long num) : base(num) { }
        /// <include file="DelegateContractId.cs.xml" path='docs/member[@name="M:DelegateContractId.#ctor(System.Int64,System.Int64,System.Int64)"]/*' />
        public DelegateContractId(long shard, long realm, long num) : base(shard, realm, num) { }
        public DelegateContractId(long shard, long realm, byte[] evmAddress) : base(shard, realm, evmAddress) { }
		/// <include file="DelegateContractId.cs.xml" path='docs/member[@name="M:DelegateContractId.DelegateContractId(System.Int64,System.Int64,System.Int64,System.String)"]/*' />
		DelegateContractId(long shard, long realm, long num, string? checksum) : base(shard, realm, num, checksum) { }

		/// <include file="DelegateContractId.cs.xml" path='docs/member[@name="M:DelegateContractId.FromString(System.String)"]/*' />
		public new static DelegateContractId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new DelegateContractId(a, b, c, d));
        }
        /// <include file="DelegateContractId.cs.xml" path='docs/member[@name="M:DelegateContractId.FromSolidityAddress(System.String)"]/*' />
        public new static DelegateContractId FromSolidityAddress(string address)
        {
            return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new DelegateContractId(a, b, c, d));
        }
        /// <include file="DelegateContractId.cs.xml" path='docs/member[@name="M:DelegateContractId.FromEvmAddress(System.Int64,System.Int64,System.String)"]/*' />
        public new static DelegateContractId FromEvmAddress(long shard, long realm, string evmAddress)
        {
            Utils.EntityIdHelper.DecodeEvmAddress(evmAddress);

            return new DelegateContractId(shard, realm, Hex.Decode(evmAddress.StartsWith("0x") ? evmAddress.Substring(2) : evmAddress));
        }
        /// <include file="DelegateContractId.cs.xml" path='docs/member[@name="M:DelegateContractId.FromProtobuf(Proto.ContractID)"]/*' />
        public new static DelegateContractId FromProtobuf(Proto.ContractID contractId)
        {
            return new DelegateContractId(contractId.ShardNum, contractId.RealmNum, contractId.ContractNum);
        }
        /// <include file="DelegateContractId.cs.xml" path='docs/member[@name="M:DelegateContractId.FromBytes(System.Byte[])"]/*' />
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