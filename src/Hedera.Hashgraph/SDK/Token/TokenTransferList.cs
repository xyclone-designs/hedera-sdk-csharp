// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Nfts;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Token
{
    public class TokenTransferList
    {
        public readonly TokenId TokenId;
        public readonly uint? ExpectDecimals;
        public IList<TokenTransfer> Transfers = [];
        public IList<TokenNftTransfer> NftTransfers = [];

        public TokenTransferList(TokenId tokenId, uint? expectDecimals, TokenTransfer? transfer, TokenNftTransfer? nftTransfer)
        {
            TokenId = tokenId;
            ExpectDecimals = expectDecimals;

            if (transfer != null)
            {
				Transfers.Add(transfer);
            }

            if (nftTransfer != null)
            {
                NftTransfers.Add(nftTransfer);
            }
        }

        public virtual Proto.TokenTransferList ToProtobuf()
        {
            var transfers = new List<Proto.AccountAmount>();
            var nftTransfers = new List<Proto.NftTransfer>();

            foreach (var transfer in Transfers)
				transfers.Add(transfer.ToProtobuf());

			foreach (var nfttransfers in NftTransfers.Select(_ => _.ToProtobuf()))
				nftTransfers.Add(nfttransfers);

			Proto.TokenTransferList proto = new()
            {
                Token = TokenId.ToProtobuf(),
			};

            if (ExpectDecimals.HasValue)
                proto.ExpectedDecimals = ExpectDecimals;


			proto.Transfers.AddRange(transfers);
            proto.NftTransfers.AddRange(nftTransfers);

            return proto;
        }
    }
}