// SPDX-License-Identifier: Apache-2.0
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
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

        virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(FileInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(FileInfo.FromProtobuf(info).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        virtual void FromBytes()
        {
            SnapshotMatcher.Expect(FileInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }

        virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(FileInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }
    }
}