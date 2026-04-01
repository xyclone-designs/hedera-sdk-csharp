// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Airdrops;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using Hedera.Hashgraph.Tests.SDK.Contract;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Collections.Generic;
using System.Text;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class TransactionRecordTest
    {
        static readonly DateTimeOffset time = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private static readonly byte[] callResult = Hex.Decode(ContractFunctionResultTest.CALL_RESULT_HEX);

        private static TransactionRecord SpawnRecordExample(ByteString? prngBytes, int? prngNumber)
        {
            return new TransactionRecord(
                transactionReceipt: TransactionReceiptTest.SpawnReceiptExample(),
                transactionHash: ByteString.CopyFromUtf8("hello"),
                consensusTimestamp: time,
                transactionId: TransactionId.WithValidStart(AccountId.FromString("3.3.3"), time),
                transactionMemo: "memo",
                transactionFee: 3000,
                contractFunctionResult: new ContractFunctionResult(new Proto.ContractFunctionResult
                {
                    ContractID = ContractId.FromString("1.2.3").ToProtobuf(),
                    ContractCallResult = ByteString.CopyFrom(callResult),
                    EvmAddress = ByteString.CopyFrom(Hex.Decode("98329e006610472e6B372C080833f6D79ED833cf")),
                    SenderId = AccountId.FromString("1.2.3").ToProtobuf(),
                }),
                transfers: [new Transfer(AccountId.FromString("4.4.4"), Hbar.From(5))],
                tokenTransfers: new Dictionary<TokenId, IDictionary<AccountId, long>>
                {
                    { TokenId.FromString("6.6.6"), new Dictionary<AccountId, long> { { AccountId.FromString("1.1.1"), 4 } } }
                },
                tokenTransferList: [new TokenTransfer(TokenId.FromString("8.9.10"), AccountId.FromString("1.2.3"), 4, 3, true)],
                tokenNftTransfers: new Dictionary<TokenId, IList<TokenNftTransfer>>
                {
                    {
                        TokenId.FromString("4.4.4"),
                        [new TokenNftTransfer(TokenId.FromString("4.4.4"), AccountId.FromString("1.2.3"), AccountId.FromString("3.2.1"), 4, true, null, null)]
                    }
                },
                scheduleRef: ScheduleId.FromString("3.3.3"),
                assessedCustomFees: 
                [
                    new AssessedCustomFee(
                        4,
                        TokenId.FromString("4.5.6"),
                        AccountId.FromString("8.6.5"),
                        [AccountId.FromString("3.3.3")])
                ],
                automaticTokenAssociations: [new TokenAssociation(TokenId.FromString("5.4.3"), AccountId.FromString("8.7.6"))],
                aliasKey: PrivateKey.FromStringECDSA("8776c6b831a1b61ac10dac0304a2843de4716f54b1919bb91a2685d0fe3f3048").GetPublicKey(),
                children: [],
                duplicates: [],
                parentConsensusTimestamp: time,
                ethereumHash: ByteString.CopyFrom(Encoding.UTF8.GetBytes("Some hash")),
                paidStakingRewards : [new Transfer(AccountId.FromString("1.2.3"), Hbar.From(8))],
                prngBytes: prngBytes,
                prngNumber: prngNumber ?? 0,
                evmAddress: ByteString.CopyFrom(Encoding.UTF8.GetBytes("0x00")),
                pendingAirdropRecords: 
                [
                    new PendingAirdropRecord(new PendingAirdropId(AccountId.FromString("0.0.123"), AccountId.FromString("0.0.124"), NftId.FromString("0.0.5005/1234")), 123),
                    new PendingAirdropRecord(new PendingAirdropId(AccountId.FromString("0.0.123"), AccountId.FromString("0.0.124"), TokenId.FromString("0.0.12345")), 123)
                ]
            );
        }
        [Fact]
        public virtual void ShouldSerialize()
        {
            var originalRecord = SpawnRecordExample(ByteString.CopyFromUtf8("very random bytes"), null);
            byte[] recordBytes = originalRecord.ToBytes();
            var copyRecord = TransactionRecord.FromBytes(recordBytes);
            Assert.Equal(copyRecord.ToString(), originalRecord.ToString());

            Verifier.Verify(originalRecord.ToString());
        }
        [Fact]
        public virtual void ShouldSerialize2()
        {
            var originalRecord = SpawnRecordExample(null, 4);
            byte[] recordBytes = originalRecord.ToBytes();
            var copyRecord = TransactionRecord.FromBytes(recordBytes);
            Assert.Equal(copyRecord.ToString(), originalRecord.ToString());

            Verifier.Verify(originalRecord.ToString());
        }
    }
}