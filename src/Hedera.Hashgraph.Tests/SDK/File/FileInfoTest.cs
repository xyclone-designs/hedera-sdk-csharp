// SPDX-License-Identifier: Apache-2.0
using Com.Google.Protobuf;
using Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.File
{
    public class FileInfoTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly FileGetInfoResponse.FileInfo info = FileGetInfoResponse.FileInfo.NewBuilder().SetFileID(new FileId(0, 0, 1).ToProtobuf()).SetSize(2).SetExpirationTime(InstantConverter.ToProtobuf(Instant.OfEpochMilli(3))).SetDeleted(true).SetKeys(KeyList.NewBuilder().AddKeys(privateKey.GetPublicKey().ToProtobufKey())).SetLedgerId(LedgerId.MAINNET.ToByteString()).Build();
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(FileInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(FileInfo.FromProtobuf(info).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(FileInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(FileInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }
    }
}