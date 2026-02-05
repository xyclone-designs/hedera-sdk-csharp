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

namespace Com.Hedera.Hashgraph.Sdk
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

        virtual void FromString()
        {
            SnapshotMatcher.Expect(DelegateContractId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(DelegateContractId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        virtual void FromSolidityAddressWith0x()
        {
            SnapshotMatcher.Expect(DelegateContractId.FromSolidityAddress("0x000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new DelegateContractId(0, 0, 5005).ToBytes())).ToMatchSnapshot();
        }

        virtual void FromBytes()
        {
            SnapshotMatcher.Expect(DelegateContractId.FromBytes(new DelegateContractId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new DelegateContractId(0, 0, 5005).ToEvmAddress()).ToMatchSnapshot();
        }

        virtual void FromEvmAddressIncorrectSizeTooShort()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "abc123");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        virtual void FromEvmAddressIncorrectSizeTooLong()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        virtual void FromEvmAddressIncorrectSizeWith0xPrefix()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "0xabc123");
            }).WithMessageContaining("Solidity addresses must be 20 bytes or 40 hex chars");
        }

        virtual void FromEvmAddressCorrectSize()
        {
            string correctAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(0, 0, correctAddress);
            AssertThat(id.evmAddress).IsNotNull();
            Assert.Equal(Hex.ToHexString(id.evmAddress), "742d35cc6634c0532925a3b844bc454e4438f44e");
        }

        virtual void ToEvmAddressNormalContractId()
        {
            DelegateContractId id = new DelegateContractId(0, 0, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        virtual void ToEvmAddressWithDifferentShardAndRealm()
        {
            DelegateContractId id = new DelegateContractId(1, 1, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        virtual void ToEvmAddressLongZeroAddress()
        {
            string longZeroAddress = "00000000000000000000000000000000000004d2";
            DelegateContractId id = DelegateContractId.FromEvmAddress(1, 1, longZeroAddress);
            Assert.Equal(id.ToEvmAddress(), longZeroAddress.ToLowerCase());
        }

        virtual void ToEvmAddressNormalEvmAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(0, 0, evmAddress);
            string expected = evmAddress.ToLowerCase();
            Assert.Equal(id.ToEvmAddress(), expected);
        }

        virtual void ToEvmAddressNormalEvmAddressWithShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(1, 1, evmAddress);
            string expected = evmAddress.ToLowerCase();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
    }
}