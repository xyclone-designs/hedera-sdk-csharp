// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Contract;

using Org.BouncyCastle.Utilities.Encoders;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    /// <include file="test-contract-id.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest"]' />
    public class ContractIdTest
    {
        public virtual void FromString()
        {
            Verifier.Verify(ContractId.FromString(TestData.DEFAULT_ENTITY_ID).ToString());
        }

        public virtual void FromSolidityAddress()
        {
            Verifier.Verify(ContractId.FromSolidityAddress(TestData.SOLIDITY_ADDRESS).ToString());
        }

        public virtual void FromSolidityAddressWith0x()
        {
            Verifier.Verify(ContractId.FromSolidityAddress($"0x{TestData.SOLIDITY_ADDRESS}").ToString());
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
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.ToFromBytes"]' />
        public virtual void ToFromBytes()
        {
            ContractId a = ContractId.FromString("1.2.3");
            Assert.Equal(ContractId.FromBytes(a.ToBytes()), a);
            ContractId b = ContractId.FromEvmAddress(1, 2, "0x98329e006610472e6B372C080833f6D79ED833cf");
            Assert.Equal(ContractId.FromBytes(b.ToBytes()), b);
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(ContractId.FromString(TestData.DEFAULT_ENTITY_ID).ToBytes()));
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(ContractId.FromBytes(ContractId.FromString(TestData.DEFAULT_ENTITY_ID).ToBytes()).ToString());
        }

        public virtual void ToSolidityAddress()
        {
            Verifier.Verify(ContractId.FromString(TestData.DEFAULT_ENTITY_ID).ToEvmAddress());
        }

        public virtual void ToSolidityAddress2()
        {
            Verifier.Verify(ContractId.FromEvmAddress(1, 2, "0x98329e006610472e6B372C080833f6D79ED833cf").ToEvmAddress());
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.FromEvmAddressIncorrectSizeTooShort"]' />
        public virtual void FromEvmAddressIncorrectSizeTooShort()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "abc123");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.FromEvmAddressIncorrectSizeTooLong"]' />
        public virtual void FromEvmAddressIncorrectSizeTooLong()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.FromEvmAddressIncorrectSizeWith0xPrefix"]' />
        public virtual void FromEvmAddressIncorrectSizeWith0xPrefix()
        {
			ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "0xabc123");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.FromEvmAddressCorrectSize"]' />
        public virtual void FromEvmAddressCorrectSize()
        {
            string correctAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e";
            ContractId id = ContractId.FromEvmAddress(0, 0, correctAddress);
            Assert.NotNull(id.EvmAddress);
            Assert.Equal(Hex.ToHexString(id.EvmAddress), "742d35cc6634c0532925a3b844bc454e4438f44e");
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.FromEvmAddressNormalAddress"]' />
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
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.FromEvmAddressWithDifferentShardAndRealm"]' />
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
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.FromEvmAddressLongZeroAddress"]' />
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
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.FromEvmAddressLongZeroAddressWithShardAndRealm"]' />
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
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.ToEvmAddressNormalContractId"]' />
        public virtual void ToEvmAddressNormalContractId()
        {
            ContractId id = new ContractId(0, 0, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.ToEvmAddressWithDifferentShardAndRealm"]' />
        public virtual void ToEvmAddressWithDifferentShardAndRealm()
        {
            ContractId id = new ContractId(1, 1, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.ToEvmAddressLongZeroAddress"]' />
        public virtual void ToEvmAddressLongZeroAddress()
        {
            string longZeroAddress = "00000000000000000000000000000000000004d2";
            ContractId id = ContractId.FromEvmAddress(1, 1, longZeroAddress);
            Assert.Equal(id.ToEvmAddress(), longZeroAddress.ToLower());
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.ToEvmAddressNormalEvmAddress"]' />
        public virtual void ToEvmAddressNormalEvmAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            ContractId id = ContractId.FromEvmAddress(0, 0, evmAddress);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
        [Fact]
        /// <include file="test-contract-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Contract.ContractIdTest.ToEvmAddressNormalEvmAddressWithShardAndRealm"]' />
        public virtual void ToEvmAddressNormalEvmAddressWithShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            ContractId id = ContractId.FromEvmAddress(1, 1, evmAddress);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
    }
}
