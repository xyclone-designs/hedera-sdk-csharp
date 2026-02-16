// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Assertj.Core.Api.AssertionsForClassTypes;
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    class ContractIdTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void FromString()
        {
            SnapshotMatcher.Expect(ContractId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(ContractId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddressWith0x()
        {
            SnapshotMatcher.Expect(ContractId.FromSolidityAddress("0x000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void FromEvmAddress()
        {
            SnapshotMatcher.Expect(ContractId.FromEvmAddress(1, 2, "98329e006610472e6B372C080833f6D79ED833cf").ToString()).ToMatchSnapshot();
        }

        public virtual void FromEvmAddressWith0x()
        {
            SnapshotMatcher.Expect(ContractId.FromEvmAddress(1, 2, "0x98329e006610472e6B372C080833f6D79ED833cf").ToString()).ToMatchSnapshot();
        }

        public virtual void FromStringWithEvmAddress()
        {
            SnapshotMatcher.Expect(ContractId.FromString("1.2.98329e006610472e6B372C080833f6D79ED833cf").ToString()).ToMatchSnapshot();
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
            SnapshotMatcher.Expect(Hex.ToHexString(new ContractId(0, 0, 5005).ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(ContractId.FromBytes(new ContractId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new ContractId(0, 0, 5005).ToEvmAddress()).ToMatchSnapshot();
        }

        public virtual void ToSolidityAddress2()
        {
            SnapshotMatcher.Expect(ContractId.FromEvmAddress(1, 2, "0x98329e006610472e6B372C080833f6D79ED833cf").ToEvmAddress()).ToMatchSnapshot();
        }

        public virtual void FromEvmAddressIncorrectSizeTooShort()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "abc123");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressIncorrectSizeTooLong()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressIncorrectSizeWith0xPrefix()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractId.FromEvmAddress(0, 0, "0xabc123");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressCorrectSize()
        {
            string correctAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e";
            ContractId id = ContractId.FromEvmAddress(0, 0, correctAddress);
            AssertThat(id.evmAddress).IsNotNull();
            Assert.Equal(Hex.ToHexString(id.evmAddress), "742d35cc6634c0532925a3b844bc454e4438f44e");
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