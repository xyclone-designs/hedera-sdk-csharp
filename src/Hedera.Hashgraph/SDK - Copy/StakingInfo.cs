// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions.Account;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Staking metadata for an account or a contract returned in CryptoGetInfo or ContractGetInfo queries
    /// </summary>
    public class StakingInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="declineStakingReward">the declineStakingReward</param>
        /// <param name="stakePeriodStart">the stakePeriodStart</param>
        /// <param name="pendingReward">the amount in Hbar that will be received in the next reward situation</param>
        /// <param name="stakedToMe">the total of balance of all accounts staked to this account or contract</param>
        /// <param name="stakedAccountId">the account to which this account or contract is staking</param>
        /// <param name="stakedNodeId">the ID of the node this account or contract is staked to</param>
        public StakingInfo(bool declineStakingReward, Timestamp stakePeriodStart, Hbar pendingReward, Hbar stakedToMe, AccountId stakedAccountId, long stakedNodeId)
        {
            DeclineStakingReward = declineStakingReward;
            StakePeriodStart = stakePeriodStart;
            PendingReward = pendingReward;
            StakedToMe = stakedToMe;
            StakedAccountId = stakedAccountId;
            StakedNodeId = stakedNodeId;
        }
        
        public static StakingInfo FromProtobuf(Proto.StakingInfo info)
        {
            return new StakingInfo(
                info.DeclineReward,
                Utils.TimestampConverter.FromProtobuf(info.StakePeriodStart),
                Hbar.FromTinybars(info.PendingReward),
                Hbar.FromTinybars(info.StakedToMe),
                AccountId.FromProtobuf(info.StakedAccountId),
                info.StakedNodeId);
        }
        /// <summary>
        /// Convert a byte array to a staking info object.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the converted staking info object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static StakingInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.StakingInfo.Parser.ParseFrom(bytes));
        }

		/// <summary>
		/// If true, the contract declines receiving a staking reward. The default value is false.
		/// </summary>
		public bool DeclineStakingReward { get; }
		/// <summary>
		/// The staking period during which either the staking settings for this account or contract changed (such as starting
		/// staking or changing staked_node_id) or the most recent reward was earned, whichever is later. If this account or contract
		/// is not currently staked to a node, then this field is not set.
		/// </summary>
		public Timestamp StakePeriodStart { get; }
		/// <summary>
		/// The amount in Hbar that will be received in the next reward situation.
		/// </summary>
		public Hbar PendingReward { get; }
		/// <summary>
		/// The total of balance of all accounts staked to this account or contract.
		/// </summary>
		public Hbar StakedToMe { get; }
		/// <summary>
		/// The account to which this account or contract is staking.
		/// </summary>
		public AccountId StakedAccountId { get; }
		/// <summary>
		/// The ID of the node this account or contract is staked to.
		/// </summary>
		public long StakedNodeId { get; }

		/// <summary>
		/// Convert the staking info object to a byte array.
		/// </summary>
		/// <returns>                         the converted staking info object</returns>
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		public virtual Proto.StakingInfo ToProtobuf()
        {
            return new Proto.StakingInfo
            {
				DeclineReward = DeclineStakingReward,
				StakePeriodStart = Utils.TimestampConverter.ToProtobuf(StakePeriodStart),
				PendingReward = PendingReward.ToTinybars(),
				StakedToMe = StakedToMe.ToTinybars(),
				StakedAccountId = StakedAccountId.ToProtobuf(),
				StakedNodeId = StakedNodeId
			};
        }
    }
}