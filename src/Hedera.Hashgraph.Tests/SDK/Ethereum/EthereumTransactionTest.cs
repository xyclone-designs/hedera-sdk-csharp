// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.File;
using Org.BouncyCastle.Utilities.Encoders;
using Hedera.Hashgraph.SDK.Keys;

namespace Hedera.Hashgraph.Tests.SDK.Ethereum
{
    public class EthereumTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual EthereumTransaction SpawnTestTransaction()
        {
            return new EthereumTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				EthereumData = Hex.Decode("deadbeef"),
				CallDataFileId = FileId.FromString("4.5.6"),
				MaxGasAllowanceHbar = Hbar.FromString("3"),
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new EthereumTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx2.ToString());
        }
    }
}