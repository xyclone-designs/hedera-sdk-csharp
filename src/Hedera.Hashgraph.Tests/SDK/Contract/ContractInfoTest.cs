// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Networking;

using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractInfoTest
    {
        private readonly Proto.ContractGetInfoResponse.Types.ContractInfo info = new Proto.ContractGetInfoResponse.Types.ContractInfo
        {
			ContractID = new ContractId(0, 0, 1).ToProtobuf(),
			AccountID = new AccountId(0, 0, 2).ToProtobuf(),
			ContractAccountID = "3",
			ExpirationTime = DateTimeOffset.UnixEpoch.AddMilliseconds(4).ToProtoTimestamp(),
			AutoRenewPeriod = TimeSpan.FromDays(5).ToProtoDuration(),
			Storage = 6,
			Memo = "7",
			Balance = 8,
			LedgerId = LedgerId.TESTNET.ToByteString()
		};
        
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