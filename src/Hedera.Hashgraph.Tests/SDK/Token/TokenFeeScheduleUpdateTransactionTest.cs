// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Transactions;

using Google.Protobuf.WellKnownTypes;

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
			List<CustomFee> customFees =
            [
				new CustomFixedFee
			    {
				    FeeCollectorAccountId = new AccountId(0, 0, 4322),
				    DenominatingTokenId = new TokenId(0, 0, 483902),
				    Amount = 10,
			    },
			    new CustomFractionalFee
			    {
				    FeeCollectorAccountId = new AccountId(0, 0, 389042),
				    Numerator = 3,
				    Denominator = 7,
				    Min = 3,
				    Max = 100,
				    AssessmentMethod = FeeAssessmentMethod.Exclusive
			    }
			];
            return new TokenFeeScheduleUpdateTransaction
            {
				TokenId = new TokenId(0, 0, 8798),
				CustomFees = customFees,
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			
            }.Freeze();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenFeeScheduleUpdateTransaction();
            var tx2 = Transaction.FromBytes<TokenFeeScheduleUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize()
        {
            var originalUpdate = SpawnTestTransaction();
            byte[] updateBytes = originalUpdate.ToBytes();
            var copyUpdate = Transaction.FromBytes<TokenFeeScheduleUpdateTransaction>(updateBytes);
            
            Assert.Equal(copyUpdate.ToString(), originalUpdate.ToString());

            SnapshotMatcher.Expect(originalUpdate.ToString()).ToMatchSnapshot();
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenFeeScheduleUpdate = new Proto.TokenFeeScheduleUpdateTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenFeeScheduleUpdateTransaction>(transactionBody);
            Assert.IsType<TokenFeeScheduleUpdateTransaction>(tx);
        }
    }
}