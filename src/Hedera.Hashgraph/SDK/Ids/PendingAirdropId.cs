// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Ids
{
    /// <summary>
    /// A unique, composite, identifier for a pending airdrop.
    /// 
    /// Each pending airdrop SHALL be uniquely identified by a PendingAirdropId.
    /// A PendingAirdropId SHALL be recorded when created and MUST be provided in any transaction
    /// that would modify that pending airdrop (such as a `claimAirdrop` or `cancelAirdrop`).
    /// </summary>
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

		public static PendingAirdropId FromProtobuf(Proto.PendingAirdropId pendingAirdropId)
		{
			if (pendingAirdropId.FungibleTokenType is Proto.TokenID tokenid)
				return new PendingAirdropId(
					AccountId.FromProtobuf(pendingAirdropId.SenderId),
					AccountId.FromProtobuf(pendingAirdropId.ReceiverId),
					TokenId.FromProtobuf(tokenid));

			if (pendingAirdropId.NonFungibleToken is Proto.NftID nftid)
				return new PendingAirdropId(
					AccountId.FromProtobuf(pendingAirdropId.SenderId),
					AccountId.FromProtobuf(pendingAirdropId.ReceiverId),
					NftId.FromProtobuf(nftid));

			throw new ArgumentException("pendingAirdropId does not contain a token \n " + pendingAirdropId);
		}

		public virtual AccountId Sender { get; set; }
		public virtual AccountId Receiver { get; set; }
		public virtual TokenId? TokenId { get; set; }
		public virtual NftId? NftId { get; set; }

		public virtual Proto.PendingAirdropId ToProtobuf()
		{
			Proto.PendingAirdropId proto = new()
			{
				SenderId = Sender.ToProtobuf(),
				ReceiverId = Receiver.ToProtobuf(),

			};

			if (TokenId != null)
				proto.FungibleTokenType = TokenId.ToProtobuf();

			if (NftId != null)
				proto.NonFungibleToken = NftId.ToProtobuf();

			return proto;
		}
    }
}