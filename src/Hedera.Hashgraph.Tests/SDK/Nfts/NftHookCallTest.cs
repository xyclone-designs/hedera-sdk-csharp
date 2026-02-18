// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    class NftHookCallTest
    {
        public virtual void ConstructorWithNumericIdAndType()
        {
            var evm = new EvmHookCall(new byte[] { }, 25000);
            var call = new NftHookCall(2, evm, NftHookType.PreHookSender);
            
            Assert.Equal(call.Type, NftHookType.PreHookSender);
        }

        public virtual void NullTypeThrows()
        {
            var evm = new EvmHookCall(new byte[] { }, 1);

            AssertThatThrownBy(() => new NftHookCall(1, evm, null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("type cannot be null");
        }

        public virtual void NftTransferSerializesSenderAndReceiverHooksByType()
        {
            var tx = new TransferTransaction();
            var token = new TokenId(0, 0, 7777);
            var nftId = new NftId(token, 1);
            var sender = new AccountId(0, 0, 8001);
            var receiver = new AccountId(0, 0, 8002);
            var senderHook = new NftHookCall(2, new EvmHookCall(new byte[] { }, 10), NftHookType.PreHookSender);
            var receiverHook = new NftHookCall(3, new EvmHookCall(new byte[] { }, 10), NftHookType.PrePostHookReceiver);
            tx.AddNftTransferWithHook(nftId, sender, receiver, senderHook, receiverHook);
            var body = tx.Build();
            var hasSenderPre = body.GetTokenTransfersList().Stream().FlatMap((tl) => tl.GetNftTransfersList().Stream()).AnyMatch((t) => t.HasPreTxSenderAllowanceHook());
            var hasReceiverPrePost = body.GetTokenTransfersList().Stream().FlatMap((tl) => tl.GetNftTransfersList().Stream()).AnyMatch((t) => t.HasPrePostTxReceiverAllowanceHook());
            Assert.True(hasSenderPre);
            Assert.True(hasReceiverPrePost);

            // Round-trip parse back
            var rebuilt = new TransferTransaction(new Proto.TransactionBody { CryptoTransfer = body });
            var rebuiltNfts = rebuilt.GetTokenNftTransfers();
            Assert.Single(rebuiltNfts[token]);
        }
    }
}