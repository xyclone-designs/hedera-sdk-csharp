// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;
using Org.BouncyCastle.Utilities.Encoders;
using System;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class TransactionRecordTest
    {
        static readonly DateTimeOffset time = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private static readonly byte[] callResult = Hex.Decode(CALL_RESULT_HEX);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private static TransactionRecord SpawnRecordExample(ByteString? prngBytes, int? prngNumber)
        {
            return new TransactionRecord(SpawnReceiptExample(), ByteString.CopyFrom("hello", StandardCharsets.UTF_8), time, TransactionId.WithValidStart(AccountId.FromString("3.3.3"), time), "memo", 3000, 
                new ContractFunctionResult(Proto.ContractFunctionResult.NewBuilder()
                    .SetContractID(ContractId.FromString("1.2.3").ToProtobuf())
                    .SetContractCallResult(ByteString.CopyFrom(callResult))
                    .SetEvmAddress(BytesValue.NewBuilder()
                        .SetValue(ByteString.CopyFrom(Hex.Decode("98329e006610472e6B372C080833f6D79ED833cf"))).Build())
                    .SetSenderId(AccountId.FromString("1.2.3").ToProtobuf())), List.Of(new Transfer(AccountId.FromString("4.4.4"), Hbar.From(5))), Map.Of(TokenId.FromString("6.6.6"), Map.Of(AccountId.FromString("1.1.1"), 4)), List.Of(new TokenTransfer(TokenId.FromString("8.9.10"), AccountId.FromString("1.2.3"), 4, 3, true)), Map.Of(TokenId.FromString("4.4.4"), List.Of(new TokenNftTransfer(TokenId.FromString("4.4.4"), AccountId.FromString("1.2.3"), AccountId.FromString("3.2.1"), 4, true))), ScheduleId.FromString("3.3.3"), List.Of(new AssessedCustomFee(4, TokenId.FromString("4.5.6"), AccountId.FromString("8.6.5"), List.Of(AccountId.FromString("3.3.3")))), List.Of(new TokenAssociation(TokenId.FromString("5.4.3"), AccountId.FromString("8.7.6"))), PrivateKey.FromStringECDSA("8776c6b831a1b61ac10dac0304a2843de4716f54b1919bb91a2685d0fe3f3048").GetPublicKey(), new List(), new List(), time, ByteString.CopyFrom("Some hash", StandardCharsets.UTF_8), List.Of(new Transfer(AccountId.FromString("1.2.3"), Hbar.From(8))), prngBytes, prngNumber, ByteString.CopyFrom("0x00", StandardCharsets.UTF_8), List.Of(new PendingAirdropRecord(new PendingAirdropId(AccountId.FromString("0.0.123"), AccountId.FromString("0.0.124"), NftId.FromString("0.0.5005/1234")), 123), new PendingAirdropRecord(new PendingAirdropId(AccountId.FromString("0.0.123"), AccountId.FromString("0.0.124"), TokenId.FromString("0.0.12345")), 123)));
        }

        public virtual void ShouldSerialize()
        {
            var originalRecord = SpawnRecordExample(ByteString.CopyFromUtf8("very random bytes"), null);
            byte[] recordBytes = originalRecord.ToBytes();
            var copyRecord = TransactionRecord.FromBytes(recordBytes);
            Assert.Equal(copyRecord.ToString(), originalRecord.ToString());

            SnapshotMatcher.Expect(originalRecord.ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldSerialize2()
        {
            var originalRecord = SpawnRecordExample(null, 4);
            byte[] recordBytes = originalRecord.ToBytes();
            var copyRecord = TransactionRecord.FromBytes(recordBytes);
            Assert.Equal(copyRecord.ToString(), originalRecord.ToString());

            SnapshotMatcher.Expect(originalRecord.ToString()).ToMatchSnapshot();
        }
    }
}