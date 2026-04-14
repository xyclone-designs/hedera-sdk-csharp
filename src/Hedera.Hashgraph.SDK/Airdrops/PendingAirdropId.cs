// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.SDK.Airdrops
{
    /// <include file="PendingAirdropId.cs.xml" path='docs/member[@name="T:PendingAirdropId"]/*' />
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

		public static PendingAirdropId FromProtobuf(Proto.Services.PendingAirdropId pendingAirdropId)
		{
			if (pendingAirdropId.FungibleTokenType is Proto.Services.TokenID tokenid)
				return new PendingAirdropId(
					AccountId.FromProtobuf(pendingAirdropId.SenderId),
					AccountId.FromProtobuf(pendingAirdropId.ReceiverId),
					TokenId.FromProtobuf(tokenid));

			if (pendingAirdropId.NonFungibleToken is Proto.Services.NftID nftid)
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

		public virtual Proto.Services.PendingAirdropId ToProtobuf()
		{
			Proto.Services.PendingAirdropId proto = new()
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
