using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
	public class TokenTransferList
	{
		public TokenId TokenId { get; }
		public uint? ExpectDecimals { get; }
		public List<TokenTransfer> Transfers { get; }
		public List<TokenNftTransfer> NftTransfers { get; }

		public TokenTransferList(TokenId tokenId, uint? expectDecimals, TokenTransfer? transfer, TokenNftTransfer? nftTransfer)
		{
			Transfers = [];
			NftTransfers = [];
			TokenId = tokenId;
			ExpectDecimals = expectDecimals;

			if (transfer != null) Transfers.Add(transfer);
			if (nftTransfer != null) NftTransfers.Add(nftTransfer);
		}

		public Proto.TokenTransferList ToProtobuf()
		{
			Proto.TokenTransferList proto = new () 
			{
				Token = TokenId.ToProtobuf(),
				ExpectedDecimals = ExpectDecimals
			};

			proto.Transfers.Add(Transfers.Select(_ => _.ToProtobuf()));
			proto.NftTransfers.Add(NftTransfers.Select(_ => _.ToProtobuf()));

			return proto;
		}
	}
}