// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;

using System;

namespace Hedera.Hashgraph.SDK.HBar
{
    /// <include file="StakingInfo.cs.xml" path='docs/member[@name="T:StakingInfo"]/*' />
    public class StakingInfo(bool declineStakingReward, DateTimeOffset stakePeriodStart, Hbar pendingReward, Hbar stakedToMe, AccountId? stakedAccountId, long? stakedNodeId)
    {
        /// <include file="StakingInfo.cs.xml" path='docs/member[@name="M:StakingInfo.FromBytes(System.Byte[])"]/*' />
        public static StakingInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.StakingInfo.Parser.ParseFrom(bytes));
		}
		public static StakingInfo FromProtobuf(Proto.StakingInfo info)
        {
            return new StakingInfo(
                info.DeclineReward,
                info.StakePeriodStart.ToDateTimeOffset(),
                Hbar.FromTinybars(info.PendingReward),
                Hbar.FromTinybars(info.StakedToMe),
                AccountId.FromProtobuf(info.StakedAccountId),
                info.StakedNodeId);
        }

        /// <include file="StakingInfo.cs.xml" path='docs/member[@name="P:StakingInfo.DeclineStakingReward"]/*' />
        public bool DeclineStakingReward { get; } = declineStakingReward;
        /// <include file="StakingInfo.cs.xml" path='docs/member[@name="P:StakingInfo.StakePeriodStart"]/*' />
        public DateTimeOffset StakePeriodStart { get; } = stakePeriodStart;
        /// <include file="StakingInfo.cs.xml" path='docs/member[@name="P:StakingInfo.PendingReward"]/*' />
        public Hbar PendingReward { get; } = pendingReward;
        /// <include file="StakingInfo.cs.xml" path='docs/member[@name="P:StakingInfo.StakedToMe"]/*' />
        public Hbar StakedToMe { get; } = stakedToMe;
        /// <include file="StakingInfo.cs.xml" path='docs/member[@name="P:StakingInfo.StakedAccountId"]/*' />
        public AccountId? StakedAccountId { get; } = stakedAccountId;
        /// <include file="StakingInfo.cs.xml" path='docs/member[@name="P:StakingInfo.StakedNodeId"]/*' />
        public long? StakedNodeId { get; } = stakedNodeId;

        /// <include file="StakingInfo.cs.xml" path='docs/member[@name="M:StakingInfo.ToBytes"]/*' />
        public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		public virtual Proto.StakingInfo ToProtobuf()
        {
			Proto.StakingInfo proto = new ()
            {
				DeclineReward = DeclineStakingReward,
				StakePeriodStart = StakePeriodStart.ToProtoTimestamp(),
				PendingReward = PendingReward.ToTinybars(),
				StakedToMe = StakedToMe.ToTinybars(),
			};

            if (StakedAccountId is not null)
				proto.StakedAccountId = StakedAccountId.ToProtobuf();

            if (StakedNodeId is not null)
				proto.StakedNodeId = StakedNodeId.Value;

            return proto;

		}
    }
}