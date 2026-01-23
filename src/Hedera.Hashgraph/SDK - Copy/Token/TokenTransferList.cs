// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;

namespace Hedera.Hashgraph.SDK.Token
{
    class TokenTransferList
    {
        readonly TokenId tokenId;
        readonly int expectDecimals;
        IList<TokenTransfer> transfers = new ();
        IList<TokenNftTransfer> nftTransfers = new ();
        TokenTransferList(TokenId tokenId, int expectDecimals, TokenTransfer transfer, TokenNftTransfer nftTransfer)
        {
            tokenId = tokenId;
            expectDecimals = expectDecimals;
            if (transfer != null)
            {
                transfers.Add(transfer);
            }

            if (nftTransfer != null)
            {
                nftTransfers.Add(nftTransfer);
            }
        }

        virtual Proto.TokenTransferList ToProtobuf()
        {
            var transfers = new List<AccountAmount>();
            var nftTransfers = new List<NftTransfer>();
            foreach (var transfer in transfers)
            {
                transfers.Add(transfer.ToProtobuf());
            }

            foreach (var transfer in nftTransfers)
            {
                nftTransfers.Add(transfer.ToProtobuf());
            }

            var builder = Proto.TokenTransferList.NewBuilder().SetToken(tokenId.ToProtobuf()).AddAllTransfers(transfers).AddAllNftTransfers(nftTransfers);
            if (expectDecimals != null)
            {
                builder.SetExpectedDecimals(UInt32Value.NewBuilder().SetValue(expectDecimals).Build());
            }

            return proto;
        }
    }
}