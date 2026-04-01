// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Contract;

using Org.BouncyCastle.Utilities.Encoders;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    class ContractIdTest
    {
        public virtual void FromString()
        {
            Verifier.Verify(ContractId.FromString("0.0.5005").ToString());
        }

        public virtual void FromSolidityAddress()
        {
            Verifier.Verify(ContractId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString());
        }

        public virtual void FromSolidityAddressWith0x()
        {
            Verifier.Verify(ContractId.FromSolidityAddress("0x000000000000000000000000000000000000138D").ToString());
        }

        public virtual void FromEvmAddress()
        {
            Verifier.Verify(ContractId.FromEvmAddress(1, 2, "98329e006610472e6B372C080833f6D79ED833cf").ToString());
        }

        public virtual void FromEvmAddressWith0x()
        {
            Verifier.Verify(ContractId.FromEvmAddress(1, 2, "0x98329e006610472e6B372C080833f6D79ED833cf").ToString());
        }

        public virtual void FromStringWithEvmAddress()
        {
            Verifier.Verify(ContractId.FromString("1.2.98329e006610472e6B372C080833f6D79ED833cf").ToString());
        }

        public virtual void ToFromBytes()
        {
            ContractId a = ContractId.FromString("1.2.3");
            Assert.Equal(ContractId.FromBytes(a.ToBytes()), a);
            ContractId b = ContractId.FromEvmAddress(1, 2, "0x98329e006610472e6B372C080833f6D79ED833cf");
            Assert.Equal(ContractId.FromBytes(b.ToBytes()), b);
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(new ContractId(0, 0, 5005).ToBytes()));
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(ContractId.FromBytes(new ContractId(0, 0, 5005).ToBytes()).ToString());
        }

        public virtual void ToSolidityAddress()
        {
            Verifier.Verify(new ContractId(0, 0, 5005).ToEvmAddress());
        }

        public virtual void ToSolidityAddress2()
        {
            Verifier.Verify(ContractId.FromEvmAddress(1, 2, "0x98329e006610472e6B372C080833f6D79ED833cf").ToEvmAddress());
        }

        public virtual void FromEvmAddressIncorrectSizeTooShort()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "abc123");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressIncorrectSizeTooLong()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressIncorrectSizeWith0xPrefix()
        {
			ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "0xabc123");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressCorrectSize()
        {
            string correctAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e";
            ContractId id = ContractId.FromEvmAddress(0, 0, correctAddress);
            Assert.NotNull(id.EvmAddress);
            Assert.Equal(Hex.ToHexString(id.EvmAddress), "742d35cc6634c0532925a3b844bc454e4438f44e");
        }

        public virtual void FromEvmAddressNormalAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            ContractId id = ContractId.FromEvmAddress(0, 0, evmAddress);
            Assert.Equal(id.Shard, 0);
            Assert.Equal(id.Realm, 0);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress, expectedBytes);
        }

        public virtual void FromEvmAddressWithDifferentShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            ContractId id = ContractId.FromEvmAddress(1, 1, evmAddress);
            Assert.Equal(id.Shard, 1);
            Assert.Equal(id.Realm, 1);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress, expectedBytes);
        }

        public virtual void FromEvmAddressLongZeroAddress()
        {
            string evmAddress = "00000000000000000000000000000000000004d2";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            ContractId id = ContractId.FromEvmAddress(0, 0, evmAddress);
            Assert.Equal(id.Shard, 0);
            Assert.Equal(id.Realm, 0);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress, expectedBytes);
        }

        public virtual void FromEvmAddressLongZeroAddressWithShardAndRealm()
        {
            string evmAddress = "00000000000000000000000000000000000004d2";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            ContractId id = ContractId.FromEvmAddress(1, 1, evmAddress);
            Assert.Equal(id.Shard, 1);
            Assert.Equal(id.Realm, 1);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress, expectedBytes);
        }

        public virtual void ToEvmAddressNormalContractId()
        {
            ContractId id = new ContractId(0, 0, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        public virtual void ToEvmAddressWithDifferentShardAndRealm()
        {
            ContractId id = new ContractId(1, 1, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        public virtual void ToEvmAddressLongZeroAddress()
        {
            string longZeroAddress = "00000000000000000000000000000000000004d2";
            ContractId id = ContractId.FromEvmAddress(1, 1, longZeroAddress);
            Assert.Equal(id.ToEvmAddress(), longZeroAddress.ToLower());
        }

        public virtual void ToEvmAddressNormalEvmAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            ContractId id = ContractId.FromEvmAddress(0, 0, evmAddress);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }

        public virtual void ToEvmAddressNormalEvmAddressWithShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            ContractId id = ContractId.FromEvmAddress(1, 1, evmAddress);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
    }
}