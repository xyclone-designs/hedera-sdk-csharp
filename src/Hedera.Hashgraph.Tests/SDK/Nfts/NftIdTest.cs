// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;

using Org.BouncyCastle.Utilities.Encoders;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    class NftIdTest
    {
        static Client? mainnetClient;
        static Client? testnetClient;
        static Client? previewnetClient;

        public static void BeforeAll()
        {
            mainnetClient = Client.ForMainnet();
            testnetClient = Client.ForTestnet();
            previewnetClient = Client.ForPreviewnet();
        }
        public static void AfterAll()
        {
            mainnetClient.Dispose();
            testnetClient.Dispose();
            previewnetClient.Dispose();
        }

		public virtual void FromBytes()
		{
			Verifier.Verify(NftId.FromBytes(new TokenId(0, 0, 5005).Nft(574489).ToBytes()).ToString());
		}
		public virtual void FromString()
        {
            Verifier.Verify(NftId.FromString("0.0.5005@1234").ToString());
        }
        public virtual void FromString2()
        {
            Verifier.Verify(NftId.FromString("0.0.5005/1234").ToString());
        }
        public virtual void FromStringWithChecksumOnMainnet()
        {
            Verifier.Verify(NftId.FromString("0.0.123-vfmkw/7584").ToStringWithChecksum(mainnetClient));
        }
        public virtual void FromStringWithChecksumOnTestnet()
        {
            Verifier.Verify(NftId.FromString("0.0.123-esxsf@584903").ToStringWithChecksum(testnetClient));
        }
        public virtual void FromStringWithChecksumOnPreviewnet()
        {
            Verifier.Verify(NftId.FromString("0.0.123-ogizo/487302").ToStringWithChecksum(previewnetClient));
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(new TokenId(0, 0, 5005).Nft(4920).ToBytes()));
        }
		public virtual void ToFromString()
		{
			var id1 = NftId.FromString("0.0.5005@1234");
			var id2 = NftId.FromString(id1.ToString());
			Assert.Equal(id2.ToString(), id1.ToString());
		}
    }
}