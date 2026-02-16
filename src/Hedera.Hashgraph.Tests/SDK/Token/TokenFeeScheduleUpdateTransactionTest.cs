// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenFeeScheduleUpdateTransactionTest
    {
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private TokenFeeScheduleUpdateTransaction SpawnTestTransaction()
        {
            var customFees = new List<CustomFee>();
            customFees.Add(new CustomFixedFee().SetFeeCollectorAccountId(new AccountId(0, 0, 4322)).SetDenominatingTokenId(new TokenId(0, 0, 483902)).SetAmount(10));
            customFees.Add(new CustomFractionalFee().SetFeeCollectorAccountId(new AccountId(0, 0, 389042)).SetNumerator(3).SetDenominator(7).SetMin(3).SetMax(100).SetAssessmentMethod(FeeAssessmentMethod.EXCLUSIVE));
            return new TokenFeeScheduleUpdateTransaction().SetTokenId(new TokenId(0, 0, 8798)).SetCustomFees(customFees).SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).Freeze();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenFeeScheduleUpdateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize()
        {
            var originalUpdate = SpawnTestTransaction();
            byte[] updateBytes = originalUpdate.ToBytes();
            var copyUpdate = TokenFeeScheduleUpdateTransaction.FromBytes(updateBytes);
            Assert.Equal(copyUpdate.ToString(), originalUpdate.ToString());
            SnapshotMatcher.Expect(originalUpdate.ToString()).ToMatchSnapshot();
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenFeeScheduleUpdate(TokenFeeScheduleUpdateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenFeeScheduleUpdateTransaction>(tx);
        }
    }
}