using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Staking metadata for an account or a contract returned in CryptoGetInfo or ContractGetInfo queries
 */
	public class StakingInfo
	{
		/**
		 * Constructor
		 *
		 * @param declineStakingReward  the declineStakingReward
		 * @param stakePeriodStart      the stakePeriodStart
		 * @param pendingReward         the amount in Hbar that will be received in the next reward situation
		 * @param stakedToMe            the total of balance of all accounts staked to this account or contract
		 * @param stakedAccountId       the account to which this account or contract is staking
		 * @param stakedNodeId          the ID of the node this account or contract is staked to
		 */
		public StakingInfo(
			bool declineStakingReward,
			DateTimeOffset stakePeriodStart,
			Hbar pendingReward,
			Hbar stakedToMe,
			AccountId? stakedAccountId,
			long? stakedNodeId)
		{
			DeclineStakingReward = declineStakingReward;
			StakePeriodStart = stakePeriodStart;
			PendingReward = pendingReward;
			StakedToMe = stakedToMe;
			StakedAccountId = stakedAccountId;
			StakedNodeId = stakedNodeId;
		}

		/**
		 * If true, the contract declines receiving a staking reward. The default value is false.
		 */
		public bool DeclineStakingReward { get; }
		/**
		 * The staking period during which either the staking settings for this account or contract changed (such as starting
		 * staking or changing staked_node_id) or the most recent reward was earned, whichever is later. If this account or contract
		 * is not currently staked to a node, then this field is not set.
		 */
		public DateTimeOffset StakePeriodStart { get; }
		/**
		 * The amount in Hbar that will be received in the next reward situation.
		 */
		public Hbar PendingReward { get; }
		/**
		 * The total of balance of all accounts staked to this account or contract.
		 */
		public Hbar StakedToMe { get; }
		/**
		 * The account to which this account or contract is staking.
		 */
		public AccountId? StakedAccountId { get; }
		/**
		 * The ID of the node this account or contract is staked to.
		 */
		public long? StakedNodeId { get; }

		/**
		 * Convert a byte array to a staking info object.
		 *
		 * @param bytes                     the byte array
		 * @return                          the converted staking info object
		 * @       when there is an issue with the protobuf
		 */
		public static StakingInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.StakingInfo.Parser.ParseFrom(bytes));
		}
		public static StakingInfo FromProtobuf(Proto.StakingInfo info)
		{
			return new StakingInfo(
				info.DeclineReward,
				DateTimeOffsetConverter.FromProtobuf(info.StakePeriodStart),
				Hbar.FromTinybars(info.PendingReward),
				Hbar.FromTinybars(info.StakedToMe),
				info.StakedAccountId is null ? null : AccountId.FromProtobuf(info.StakedAccountId),
				info.StakedNodeId);
		}

		/**
		 * Convert the staking info object to a byte array.
		 *
		 * @return                          the converted staking info object
		 */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		public Proto.StakingInfo ToProtobuf()
		{
			Proto.StakingInfo stakinginfo = new ()
			{
				DeclineReward = DeclineStakingReward,
				StakePeriodStart = DateTimeOffsetConverter.ToProtobuf(StakePeriodStart),
				PendingReward = PendingReward.ToTinybars(),
				StakedToMe = StakedToMe.ToTinybars(),
			};

			if (StakedNodeId != null) stakinginfo.StakedNodeId = StakedNodeId.Value;
			if (StakedAccountId != null) stakinginfo.StakedAccountId = StakedAccountId.ToProtobuf();

			return stakinginfo;
		}
	}
}