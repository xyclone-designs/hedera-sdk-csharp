// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;

using Org.BouncyCastle.Utilities.Encoders;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    /// <include file="test-token-nft-id.cs.xml" path="docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Nfts.NftIdTest"]" />
    public class NftIdTest : BaseTestFixture
    {
		public virtual void FromBytes()
		{
			Verifier.Verify(NftId.FromBytes(new TokenId(0, 0, 5005).Nft(574489).ToBytes()).ToString());
		}
		public virtual void FromString()
        {
            Verifier.Verify(NftId.FromString(TestData.DEFAULT_ENTITY_ID + "@1234").ToString());
        }
        public virtual void FromString2()
        {
            Verifier.Verify(NftId.FromString(TestData.DEFAULT_ENTITY_ID + "/1234").ToString());
        }
        public virtual void FromStringWithChecksumOnMainnet()
        {
            Verifier.Verify(NftId.FromString($"{TestData.TEST_ID_MAINNET}/7584").ToStringWithChecksum(MainnetClient));
        }
        public virtual void FromStringWithChecksumOnTestnet()
        {
            Verifier.Verify(NftId.FromString($"{TestData.TEST_ID_TESTNET}@584903").ToStringWithChecksum(TestnetClient));
        }
        public virtual void FromStringWithChecksumOnPreviewnet()
        {
            Verifier.Verify(NftId.FromString($"{TestData.TEST_ID_PREVIEWNET}/487302").ToStringWithChecksum(PreviewnetClient));
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(new TokenId(0, 0, 5005).Nft(4920).ToBytes()));
        }
        [Fact]
        /// <include file="test-token-nft-id.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Nfts.NftIdTest.ToFromString"]" />
        public virtual void ToFromString()
		{
			var id1 = NftId.FromString(TestData.DEFAULT_ENTITY_ID + "@1234");
			var id2 = NftId.FromString(id1.ToString());

			Assert.Equal(id2.ToString(), id1.ToString());
		}
    }
}
