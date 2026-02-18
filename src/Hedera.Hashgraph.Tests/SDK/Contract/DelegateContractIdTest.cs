// SPDX-License-Identifier: Apache-2.0
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
    class DelegateContractIdTest
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
            SnapshotMatcher.Expect(DelegateContractId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(DelegateContractId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddressWith0x()
        {
            SnapshotMatcher.Expect(DelegateContractId.FromSolidityAddress("0x000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new DelegateContractId(0, 0, 5005).ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(DelegateContractId.FromBytes(new DelegateContractId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new DelegateContractId(0, 0, 5005).ToEvmAddress()).ToMatchSnapshot();
        }

        public virtual void FromEvmAddressIncorrectSizeTooShort()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "abc123");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressIncorrectSizeTooLong()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressIncorrectSizeWith0xPrefix()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "0xabc123");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressCorrectSize()
        {
            string correctAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(0, 0, correctAddress);
            Assert.NotNull(id.evmAddress);
            Assert.Equal(Hex.ToHexString(id.evmAddress), "742d35cc6634c0532925a3b844bc454e4438f44e");
        }

        public virtual void ToEvmAddressNormalContractId()
        {
            DelegateContractId id = new DelegateContractId(0, 0, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        public virtual void ToEvmAddressWithDifferentShardAndRealm()
        {
            DelegateContractId id = new DelegateContractId(1, 1, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        public virtual void ToEvmAddressLongZeroAddress()
        {
            string longZeroAddress = "00000000000000000000000000000000000004d2";
            DelegateContractId id = DelegateContractId.FromEvmAddress(1, 1, longZeroAddress);
            Assert.Equal(id.ToEvmAddress(), longZeroAddress.ToLower());
        }

        public virtual void ToEvmAddressNormalEvmAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(0, 0, evmAddress);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }

        public virtual void ToEvmAddressNormalEvmAddressWithShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(1, 1, evmAddress);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
    }
}