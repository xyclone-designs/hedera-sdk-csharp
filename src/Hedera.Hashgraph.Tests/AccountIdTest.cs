// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Java.Util.Concurrent;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class AccountIdTest
    {
        static Client mainnetClient;
        static Client testnetClient;
        static Client previewnetClient;
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
            mainnetClient = Client.ForMainnet();
            testnetClient = Client.ForTestnet();
            previewnetClient = Client.ForPreviewnet();
        }

        public static void AfterAll()
        {
            mainnetClient.Dispose();
            testnetClient.Dispose();
            previewnetClient.Dispose();
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void FromString()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        virtual void FromStringWithChecksumOnMainnet()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.123-vfmkw").ToStringWithChecksum(mainnetClient)).ToMatchSnapshot();
        }

        virtual void FromStringWithChecksumOnTestnet()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.123-esxsf").ToStringWithChecksum(testnetClient)).ToMatchSnapshot();
        }

        virtual void FromStringWithChecksumOnPreviewnet()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.123-ogizo").ToStringWithChecksum(previewnetClient)).ToMatchSnapshot();
        }

        virtual void GoodChecksumOnMainnet()
        {
            AccountId.FromString("0.0.123-vfmkw").ValidateChecksum(mainnetClient);
        }

        virtual void GoodChecksumOnTestnet()
        {
            AccountId.FromString("0.0.123-esxsf").ValidateChecksum(testnetClient);
        }

        virtual void GoodChecksumOnPreviewnet()
        {
            AccountId.FromString("0.0.123-ogizo").ValidateChecksum(previewnetClient);
        }

        virtual void BadChecksumOnPreviewnet()
        {
            AssertThatExceptionOfType(typeof(BadEntityIdException)).IsThrownBy(() =>
            {
                AccountId.FromString("0.0.123-ntjli").ValidateChecksum(previewnetClient);
            });
        }

        virtual void MalformedIdString()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                AccountId.FromString("0.0.");
            });
        }

        virtual void MalformedIdChecksum()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                AccountId.FromString("0.0.123-ntjl");
            });
        }

        virtual void MalformedIdChecksum2()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                AccountId.FromString("0.0.123-ntjl1");
            });
        }

        virtual void MalformedAliasKey()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf777");
            });
        }

        virtual void MalformedAliasKey2()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf777g");
            });
        }

        virtual void MalformedAliasKey3()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                AccountId.FromString("0.0.303a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777");
            });
        }

        virtual void FromStringWithAliasKey()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777").ToString()).ToMatchSnapshot();
        }

        virtual void FromStringWithEvmAddress()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82da").ToString()).ToMatchSnapshot();
        }

        virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(AccountId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        virtual void FromSolidityAddressWith0x()
        {
            SnapshotMatcher.Expect(AccountId.FromSolidityAddress("0x000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new AccountId(0, 0, 5005).ToProtobuf().ToByteArray())).ToMatchSnapshot();
        }

        virtual void ToBytesAlias()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777").ToBytes())).ToMatchSnapshot();
        }

        virtual void ToBytesEvmAddress()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82da").ToBytes())).ToMatchSnapshot();
        }

        virtual void FromBytes()
        {
            SnapshotMatcher.Expect(AccountId.FromBytes(new AccountId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        virtual void ToFromProtobuf()
        {
            var id1 = new AccountId(0, 0, 5005);
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        virtual void FromBytesAlias()
        {
            SnapshotMatcher.Expect(AccountId.FromBytes(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777").ToBytes()).ToString()).ToMatchSnapshot();
        }

        virtual void ToFromProtobufAliasKey()
        {
            var id1 = AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777");
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        virtual void ToFromProtobufEcdsaAliasKey()
        {
            var id1 = AccountId.FromString("0.0.302d300706052b8104000a032200035d348292bbb8b511fdbe24e3217ec099944b4728999d337f9a025f4193324525");
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        virtual void FromBytesEvmAddress()
        {
            SnapshotMatcher.Expect(AccountId.FromBytes(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82da").ToBytes()).ToString()).ToMatchSnapshot();
        }

        virtual void ToFromProtobufEvmAddress()
        {
            var id1 = AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82da");
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        virtual void ToFromProtobufRawEvmAddress()
        {
            var id1 = AccountId.FromString("302a300506032b6570032100114e6abc371b82da");
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new AccountId(0, 0, 5005).ToEvmAddress()).ToMatchSnapshot();
        }

        virtual void FromEvmAddress()
        {
            string evmAddress = "302a300506032b6570032100114e6abc371b82da";
            var id = AccountId.FromEvmAddress(evmAddress, 5, 9);
            AssertThat(id.evmAddress).HasToString(evmAddress);
            Assert.Equal(id.shard, 5);
            Assert.Equal(id.realm, 9);
        }

        virtual void FromEvmAddressWithPrefix()
        {
            string evmAddressString = "302a300506032b6570032100114e6abc371b82da";
            EvmAddress evmAddress = EvmAddress.FromString(evmAddressString);
            var id1 = AccountId.FromEvmAddress(evmAddress, 0, 0);
            var id2 = AccountId.FromEvmAddress("0x" + evmAddressString, 0, 0);
            Assert.Equal(id2, id1);
        }

        virtual void FromEvmAddressNormalAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            AccountId id = AccountId.FromEvmAddress(evmAddress, 0, 0);
            Assert.Equal(id.shard, 0);
            Assert.Equal(id.realm, 0);
            Assert.Equal(id.num, 0);
            Assert.Equal(id.evmAddress.ToBytes(), expectedBytes);
        }

        virtual void FromEvmAddressWithDifferentShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            AccountId id = AccountId.FromEvmAddress(evmAddress, 1, 1);
            Assert.Equal(id.shard, 1);
            Assert.Equal(id.realm, 1);
            Assert.Equal(id.num, 0);
            Assert.Equal(id.evmAddress.ToBytes(), expectedBytes);
        }

        virtual void FromEvmAddressLongZeroAddress()
        {
            string evmAddress = "00000000000000000000000000000000000004d2";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            AccountId id = AccountId.FromEvmAddress(evmAddress, 0, 0);
            Assert.Equal(id.shard, 0);
            Assert.Equal(id.realm, 0);
            Assert.Equal(id.num, 0);
            Assert.Equal(id.evmAddress.ToBytes(), expectedBytes);
        }

        virtual void FromEvmAddressLongZeroAddressWithShardAndRealm()
        {
            string evmAddress = "00000000000000000000000000000000000004d2";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            AccountId id = AccountId.FromEvmAddress(evmAddress, 1, 1);
            Assert.Equal(id.shard, 1);
            Assert.Equal(id.realm, 1);
            Assert.Equal(id.num, 0);
            Assert.Equal(id.evmAddress.ToBytes(), expectedBytes);
        }

        virtual void ToEvmAddressNormalAccountId()
        {
            AccountId id = new AccountId(0, 0, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        virtual void ToEvmAddressWithDifferentShardAndRealm()
        {
            AccountId id = new AccountId(1, 1, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        virtual void ToEvmAddressLongZeroAddress()
        {
            string longZeroAddress = "00000000000000000000000000000000000004d2";
            AccountId id = AccountId.FromEvmAddress(longZeroAddress, 1, 1);
            Assert.Equal(id.ToEvmAddress(), longZeroAddress.ToLowerCase());
        }

        virtual void ToEvmAddressNormalEvmAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            AccountId id = AccountId.FromEvmAddress(evmAddress, 0, 0);
            string expected = evmAddress.ToLowerCase();
            Assert.Equal(id.ToEvmAddress(), expected);
        }

        virtual void ToEvmAddressNormalEvmAddressWithShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            AccountId id = AccountId.FromEvmAddress(evmAddress, 1, 1);
            string expected = evmAddress.ToLowerCase();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
    }
}