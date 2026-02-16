// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
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

        public virtual void FromString()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        public virtual void FromStringWithChecksumOnMainnet()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.123-vfmkw").ToStringWithChecksum(mainnetClient)).ToMatchSnapshot();
        }

        public virtual void FromStringWithChecksumOnTestnet()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.123-esxsf").ToStringWithChecksum(testnetClient)).ToMatchSnapshot();
        }

        public virtual void FromStringWithChecksumOnPreviewnet()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.123-ogizo").ToStringWithChecksum(previewnetClient)).ToMatchSnapshot();
        }

        public virtual void GoodChecksumOnMainnet()
        {
            AccountId.FromString("0.0.123-vfmkw").ValidateChecksum(mainnetClient);
        }

        public virtual void GoodChecksumOnTestnet()
        {
            AccountId.FromString("0.0.123-esxsf").ValidateChecksum(testnetClient);
        }

        public virtual void GoodChecksumOnPreviewnet()
        {
            AccountId.FromString("0.0.123-ogizo").ValidateChecksum(previewnetClient);
        }

        public virtual void BadChecksumOnPreviewnet()
        {
            Assert.Throws<BadEntityIdException>(() =>
            {
                AccountId.FromString("0.0.123-ntjli").ValidateChecksum(previewnetClient);
            });
        }

        public virtual void MalformedIdString()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString("0.0.");
            });
        }

        public virtual void MalformedIdChecksum()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString("0.0.123-ntjl");
            });
        }

        public virtual void MalformedIdChecksum2()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString("0.0.123-ntjl1");
            });
        }

        public virtual void MalformedAliasKey()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf777");
            });
        }

        public virtual void MalformedAliasKey2()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf777g");
            });
        }

        public virtual void MalformedAliasKey3()
        {
			Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString("0.0.303a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777");
            });
        }

        public virtual void FromStringWithAliasKey()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777").ToString()).ToMatchSnapshot();
        }

        public virtual void FromStringWithEvmAddress()
        {
            SnapshotMatcher.Expect(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82da").ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(AccountId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddressWith0x()
        {
            SnapshotMatcher.Expect(AccountId.FromSolidityAddress("0x000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new AccountId(0, 0, 5005).ToProtobuf().ToByteArray())).ToMatchSnapshot();
        }

        public virtual void ToBytesAlias()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777").ToBytes())).ToMatchSnapshot();
        }

        public virtual void ToBytesEvmAddress()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82da").ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(AccountId.FromBytes(new AccountId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToFromProtobuf()
        {
            var id1 = new AccountId(0, 0, 5005);
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        public virtual void FromBytesAlias()
        {
            SnapshotMatcher.Expect(AccountId.FromBytes(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777").ToBytes()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToFromProtobufAliasKey()
        {
            var id1 = AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777");
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        public virtual void ToFromProtobufEcdsaAliasKey()
        {
            var id1 = AccountId.FromString("0.0.302d300706052b8104000a032200035d348292bbb8b511fdbe24e3217ec099944b4728999d337f9a025f4193324525");
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        public virtual void FromBytesEvmAddress()
        {
            SnapshotMatcher.Expect(AccountId.FromBytes(AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82da").ToBytes()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToFromProtobufEvmAddress()
        {
            var id1 = AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82da");
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        public virtual void ToFromProtobufRawEvmAddress()
        {
            var id1 = AccountId.FromString("302a300506032b6570032100114e6abc371b82da");
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        public virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new AccountId(0, 0, 5005).ToEvmAddress()).ToMatchSnapshot();
        }

        public virtual void FromEvmAddress()
        {
            string evmAddress = "302a300506032b6570032100114e6abc371b82da";
            var id = AccountId.FromEvmAddress(evmAddress, 5, 9);
            AssertThat(id.evmAddress).HasToString(evmAddress);
            Assert.Equal(id.Shard, 5);
            Assert.Equal(id.Realm, 9);
        }

        public virtual void FromEvmAddressWithPrefix()
        {
            string evmAddressString = "302a300506032b6570032100114e6abc371b82da";
            EvmAddress evmAddress = EvmAddress.FromString(evmAddressString);
            var id1 = AccountId.FromEvmAddress(evmAddress, 0, 0);
            var id2 = AccountId.FromEvmAddress("0x" + evmAddressString, 0, 0);
            Assert.Equal(id2, id1);
        }

        public virtual void FromEvmAddressNormalAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            AccountId id = AccountId.FromEvmAddress(evmAddress, 0, 0);
            Assert.Equal(id.Shard, 0);
            Assert.Equal(id.Realm, 0);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress.ToBytes(), expectedBytes);
        }

        public virtual void FromEvmAddressWithDifferentShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            AccountId id = AccountId.FromEvmAddress(evmAddress, 1, 1);
            Assert.Equal(id.Shard, 1);
            Assert.Equal(id.Realm, 1);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress.ToBytes(), expectedBytes);
        }

        public virtual void FromEvmAddressLongZeroAddress()
        {
            string evmAddress = "00000000000000000000000000000000000004d2";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            AccountId id = AccountId.FromEvmAddress(evmAddress, 0, 0);
            Assert.Equal(id.Shard, 0);
            Assert.Equal(id.Realm, 0);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress.ToBytes(), expectedBytes);
        }

        public virtual void FromEvmAddressLongZeroAddressWithShardAndRealm()
        {
            string evmAddress = "00000000000000000000000000000000000004d2";
            byte[] expectedBytes = Hex.Decode(evmAddress);
            AccountId id = AccountId.FromEvmAddress(evmAddress, 1, 1);
            Assert.Equal(id.Shard, 1);
            Assert.Equal(id.Realm, 1);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress.ToBytes(), expectedBytes);
        }

        public virtual void ToEvmAddressNormalAccountId()
        {
            AccountId id = new AccountId(0, 0, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        public virtual void ToEvmAddressWithDifferentShardAndRealm()
        {
            AccountId id = new AccountId(1, 1, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        public virtual void ToEvmAddressLongZeroAddress()
        {
            string longZeroAddress = "00000000000000000000000000000000000004d2";
            AccountId id = AccountId.FromEvmAddress(longZeroAddress, 1, 1);
            Assert.Equal(id.ToEvmAddress(), longZeroAddress.ToLower());
        }

        public virtual void ToEvmAddressNormalEvmAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            AccountId id = AccountId.FromEvmAddress(evmAddress, 0, 0);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }

        public virtual void ToEvmAddressNormalEvmAddressWithShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            AccountId id = AccountId.FromEvmAddress(evmAddress, 1, 1);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
    }
}