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

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractInfoTest
    {
        private readonly ContractGetInfoResponse.ContractInfo info = ContractGetInfoResponse.ContractInfo.NewBuilder().SetContractID(new ContractId(0, 0, 1).ToProtobuf()).SetAccountID(new AccountId(0, 0, 2).ToProtobuf()).SetContractAccountID("3").SetExpirationTime(InstantConverter.ToProtobuf(Instant.OfEpochMilli(4))).SetAutoRenewPeriod(DurationConverter.ToProtobuf(Duration.OfDays(5))).SetStorage(6).SetMemo("7").SetBalance(8).SetLedgerId(LedgerId.TESTNET.ToByteString()).Build();
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
            SnapshotMatcher.Expect(ContractInfo.FromProtobuf(info).ToString()).ToMatchSnapshot();
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(ContractInfo.FromProtobuf(info).ToProtobuf()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(ContractInfo.FromProtobuf(info).ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(ContractInfo.FromBytes(info.ToByteArray()).ToString()).ToMatchSnapshot();
        }
    }
}