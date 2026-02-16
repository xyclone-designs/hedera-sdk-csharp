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

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    class NftIdTest
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
            SnapshotMatcher.Expect(NftId.FromString("0.0.5005@1234").ToString()).ToMatchSnapshot();
        }

        public virtual void FromString2()
        {
            SnapshotMatcher.Expect(NftId.FromString("0.0.5005/1234").ToString()).ToMatchSnapshot();
        }

        public virtual void ToFromString()
        {
            var id1 = NftId.FromString("0.0.5005@1234");
            var id2 = NftId.FromString(id1.ToString());
            Assert.Equal(id2.ToString(), id1.ToString());
        }

        public virtual void FromStringWithChecksumOnMainnet()
        {
            SnapshotMatcher.Expect(NftId.FromString("0.0.123-vfmkw/7584").ToStringWithChecksum(mainnetClient)).ToMatchSnapshot();
        }

        public virtual void FromStringWithChecksumOnTestnet()
        {
            SnapshotMatcher.Expect(NftId.FromString("0.0.123-esxsf@584903").ToStringWithChecksum(testnetClient)).ToMatchSnapshot();
        }

        public virtual void FromStringWithChecksumOnPreviewnet()
        {
            SnapshotMatcher.Expect(NftId.FromString("0.0.123-ogizo/487302").ToStringWithChecksum(previewnetClient)).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new TokenId(0, 0, 5005).Nft(4920).ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(NftId.FromBytes(new TokenId(0, 0, 5005).Nft(574489).ToBytes()).ToString()).ToMatchSnapshot();
        }
    }
}