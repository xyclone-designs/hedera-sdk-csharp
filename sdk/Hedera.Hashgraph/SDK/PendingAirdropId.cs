
namespace Hedera.Hashgraph.SDK
{
	/**
	 * A unique, composite, identifier for a pending airdrop.
	 *
	 * Each pending airdrop SHALL be uniquely identified by a PendingAirdropId.
	 * A PendingAirdropId SHALL be recorded when created and MUST be provided in any transaction
	 * that would modify that pending airdrop (such as a `claimAirdrop` or `cancelAirdrop`).
	 */
	public class PendingAirdropId
	{
		public PendingAirdropId(AccountId sender, AccountId receiver, TokenId tokenId)
		{
			Sender = sender;
			Receiver = receiver;
			TokenId = tokenId;
			NftId = null;
		}

		public PendingAirdropId(AccountId sender, AccountId receiver, NftId nftId)
		{
			Sender = sender;
			Receiver = receiver;
			NftId = nftId;
			TokenId = null;
		}

		public AccountId Sender { get; }
		public AccountId Receiver { get; }
		public TokenId? TokenId { get; }
		public NftId? NftId { get; }

		public static PendingAirdropId FromProtobuf(Proto.PendingAirdropId pendingAirdropId)
		{
			if (pendingAirdropId.FungibleTokenType is not null)
			{
				return new PendingAirdropId(
						AccountId.FromProtobuf(pendingAirdropId.SenderId),
						AccountId.FromProtobuf(pendingAirdropId.ReceiverId),
						TokenId.FromProtobuf(pendingAirdropId.FungibleTokenType));
			}
			else
			{
				return new PendingAirdropId(
						AccountId.FromProtobuf(pendingAirdropId.SenderId),
						AccountId.FromProtobuf(pendingAirdropId.ReceiverId),
						NftId.FromProtobuf(pendingAirdropId.NonFungibleToken));
			}
		}

		public Proto.PendingAirdropId ToProtobuf()
		{
			Proto.PendingAirdropId proto = new()
			{
				SenderId = Sender.ToProtobuf(),
				ReceiverId = Receiver.ToProtobuf(),
			};

			if (TokenId?.ToProtobuf() is Proto.TokenID tokenid) proto.FungibleTokenType = tokenid;
			if (NftId?.ToProtobuf() is Proto.NftID nftid) proto.NonFungibleToken = nftid;

			return proto;
		}
	}
}