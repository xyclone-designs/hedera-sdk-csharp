// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Exceptions;

using Org.BouncyCastle.Utilities.Encoders;

using System;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    /// <include file="test-account-id.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest"]' />
    public class AccountIdTest : BaseTestFixture
    {
        public virtual void FromString()
        {
            Verifier.Verify(AccountId.FromString(TestData.DEFAULT_ENTITY_ID).ToString());
        }

        public virtual void FromStringWithChecksumOnMainnet()
        {
            Verifier.Verify(AccountId.FromString(TestData.TEST_ID_MAINNET).ToStringWithChecksum(MainnetClient));
        }

        public virtual void FromStringWithChecksumOnTestnet()
        {
            Verifier.Verify(AccountId.FromString(TestData.TEST_ID_TESTNET).ToStringWithChecksum(TestnetClient));
        }

        public virtual void FromStringWithChecksumOnPreviewnet()
        {
            Verifier.Verify(AccountId.FromString(TestData.TEST_ID_PREVIEWNET).ToStringWithChecksum(PreviewnetClient));
        }

        public virtual void GoodChecksumOnMainnet()
        {
            AccountId.FromString(TestData.TEST_ID_MAINNET).ValidateChecksum(MainnetClient);
        }

        public virtual void GoodChecksumOnTestnet()
        {
            AccountId.FromString(TestData.TEST_ID_TESTNET).ValidateChecksum(TestnetClient);
        }

        public virtual void GoodChecksumOnPreviewnet()
        {
            AccountId.FromString(TestData.TEST_ID_PREVIEWNET).ValidateChecksum(PreviewnetClient);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.BadChecksumOnPreviewnet"]' />
        public virtual void BadChecksumOnPreviewnet()
        {
            Assert.Throws<BadEntityIdException>(() =>
            {
                AccountId.FromString(TestData.TEST_ID_BAD_CHECKSUM).ValidateChecksum(PreviewnetClient);
            });
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.MalformedIdString"]' />
        public virtual void MalformedIdString()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString(TestData.MALFORMED_ID_EMPTY);
            });
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.MalformedIdChecksum"]' />
        public virtual void MalformedIdChecksum()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString(TestData.MALFORMED_CHECKSUM_SHORT);
            });
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.MalformedIdChecksum2"]' />
        public virtual void MalformedIdChecksum2()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString(TestData.MALFORMED_CHECKSUM_LONG);
            });
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.MalformedAliasKey"]' />
        public virtual void MalformedAliasKey()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString(TestData.MALFORMED_ALIAS_KEY);
            });
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.MalformedAliasKey2"]' />
        public virtual void MalformedAliasKey2()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString(TestData.MALFORMED_ALIAS_KEY_INVALID_CHAR);
            });
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.MalformedAliasKey3"]' />
        public virtual void MalformedAliasKey3()
        {
			Assert.Throws<ArgumentException>(() =>
            {
                AccountId.FromString(TestData.MALFORMED_ALIAS_KEY_EXTRA_BYTE);
            });
        }

        public virtual void FromStringWithAliasKey()
        {
            Verifier.Verify(AccountId.FromString(TestData.ALIAS_KEY_HEX).ToString());
        }

        public virtual void FromStringWithEvmAddress()
        {
            Verifier.Verify(AccountId.FromString(TestData.EVM_ADDRESS_SHORT).ToString());
        }

        public virtual void FromSolidityAddress()
        {
            Verifier.Verify(AccountId.FromSolidityAddress(TestData.SOLIDITY_ADDRESS).ToString());
        }

        public virtual void FromSolidityAddressWith0x()
        {
            Verifier.Verify(AccountId.FromSolidityAddress($"0x{TestData.SOLIDITY_ADDRESS}").ToString());
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(new AccountId(0, 0, 5005).ToProtobuf().ToByteArray()));
        }

        public virtual void ToBytesAlias()
        {
            Verifier.Verify(Hex.ToHexString(AccountId.FromString(TestData.ALIAS_KEY_HEX).ToBytes()));
        }

        public virtual void ToBytesEvmAddress()
        {
            Verifier.Verify(Hex.ToHexString(AccountId.FromString(TestData.EVM_ADDRESS_SHORT).ToBytes()));
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(AccountId.FromBytes(new AccountId(0, 0, 5005).ToBytes()).ToString());
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToFromProtobuf"]' />
        public virtual void ToFromProtobuf()
        {
            var id1 = new AccountId(0, 0, 5005);
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        public virtual void FromBytesAlias()
        {
            Verifier.Verify(AccountId.FromBytes(AccountId.FromString(TestData.ALIAS_KEY_HEX).ToBytes()).ToString());
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToFromProtobufAliasKey"]' />
        public virtual void ToFromProtobufAliasKey()
        {
            var id1 = AccountId.FromString(TestData.ALIAS_KEY_HEX);
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToFromProtobufEcdsaAliasKey"]' />
        public virtual void ToFromProtobufEcdsaAliasKey()
        {
            var id1 = AccountId.FromString(TestData.ECDSA_ALIAS_KEY_HEX);
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        public virtual void FromBytesEvmAddress()
        {
            Verifier.Verify(AccountId.FromBytes(AccountId.FromString(TestData.EVM_ADDRESS_SHORT).ToBytes()).ToString());
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToFromProtobufEvmAddress"]' />
        public virtual void ToFromProtobufEvmAddress()
        {
            var id1 = AccountId.FromString(TestData.EVM_ADDRESS_SHORT);
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToFromProtobufRawEvmAddress"]' />
        public virtual void ToFromProtobufRawEvmAddress()
        {
            var id1 = AccountId.FromString(TestData.EVM_ADDRESS_HEX);
            var id2 = AccountId.FromProtobuf(id1.ToProtobuf());
            Assert.Equal(id2, id1);
        }

        public virtual void ToSolidityAddress()
        {
            Verifier.Verify(new AccountId(0, 0, 5005).ToEvmAddress());
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.FromEvmAddress"]' />
        public virtual void FromEvmAddress()
        {
            var id = AccountId.FromEvmAddress(TestData.EVM_ADDRESS_HEX, 5, 9);
            Assert.Equal(id.EvmAddress.ToString(), TestData.EVM_ADDRESS_HEX);
            Assert.Equal(id.Shard, 5);
            Assert.Equal(id.Realm, 9);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.FromEvmAddressWithPrefix"]' />
        public virtual void FromEvmAddressWithPrefix()
        {
            EvmAddress evmAddress = EvmAddress.FromString(TestData.EVM_ADDRESS_HEX);
            var id1 = AccountId.FromEvmAddress(evmAddress, 0, 0);
            var id2 = AccountId.FromEvmAddress($"0x{TestData.EVM_ADDRESS_HEX}", 0, 0);
            Assert.Equal(id2, id1);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.FromEvmAddressNormalAddress"]' />
        public virtual void FromEvmAddressNormalAddress()
        {
            byte[] expectedBytes = Hex.Decode(TestData.EVM_ADDRESS_NORMAL);
            AccountId id = AccountId.FromEvmAddress(TestData.EVM_ADDRESS_NORMAL, 0, 0);
            Assert.Equal(id.Shard, 0);
            Assert.Equal(id.Realm, 0);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress.ToBytes(), expectedBytes);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.FromEvmAddressWithDifferentShardAndRealm"]' />
        public virtual void FromEvmAddressWithDifferentShardAndRealm()
        {
            byte[] expectedBytes = Hex.Decode(TestData.EVM_ADDRESS_NORMAL);
            AccountId id = AccountId.FromEvmAddress(TestData.EVM_ADDRESS_NORMAL, 1, 1);
            Assert.Equal(id.Shard, 1);
            Assert.Equal(id.Realm, 1);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress.ToBytes(), expectedBytes);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.FromEvmAddressLongZeroAddress"]' />
        public virtual void FromEvmAddressLongZeroAddress()
        {
            byte[] expectedBytes = Hex.Decode(TestData.EVM_ADDRESS_LONG_ZERO);
            AccountId id = AccountId.FromEvmAddress(TestData.EVM_ADDRESS_LONG_ZERO, 0, 0);
            Assert.Equal(id.Shard, 0);
            Assert.Equal(id.Realm, 0);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress.ToBytes(), expectedBytes);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.FromEvmAddressLongZeroAddressWithShardAndRealm"]' />
        public virtual void FromEvmAddressLongZeroAddressWithShardAndRealm()
        {
            byte[] expectedBytes = Hex.Decode(TestData.EVM_ADDRESS_LONG_ZERO);
            AccountId id = AccountId.FromEvmAddress(TestData.EVM_ADDRESS_LONG_ZERO, 1, 1);
            Assert.Equal(id.Shard, 1);
            Assert.Equal(id.Realm, 1);
            Assert.Equal(id.Num, 0);
            Assert.Equal(id.EvmAddress.ToBytes(), expectedBytes);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToEvmAddressNormalAccountId"]' />
        public virtual void ToEvmAddressNormalAccountId()
        {
            AccountId id = new AccountId(0, 0, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToEvmAddressWithDifferentShardAndRealm"]' />
        public virtual void ToEvmAddressWithDifferentShardAndRealm()
        {
            AccountId id = new AccountId(1, 1, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToEvmAddressLongZeroAddress"]' />
        public virtual void ToEvmAddressLongZeroAddress()
        {
            AccountId id = AccountId.FromEvmAddress(TestData.EVM_ADDRESS_LONG_ZERO, 1, 1);
            Assert.Equal(id.ToEvmAddress(), TestData.EVM_ADDRESS_LONG_ZERO.ToLower());
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToEvmAddressNormalEvmAddress"]' />
        public virtual void ToEvmAddressNormalEvmAddress()
        {
            AccountId id = AccountId.FromEvmAddress(TestData.EVM_ADDRESS_NORMAL, 0, 0);
            string expected = TestData.EVM_ADDRESS_NORMAL.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
        [Fact]
        /// <include file="test-account-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Account.AccountIdTest.ToEvmAddressNormalEvmAddressWithShardAndRealm"]' />
        public virtual void ToEvmAddressNormalEvmAddressWithShardAndRealm()
        {
            AccountId id = AccountId.FromEvmAddress(TestData.EVM_ADDRESS_NORMAL, 1, 1);
            string expected = TestData.EVM_ADDRESS_NORMAL.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
    }
}
