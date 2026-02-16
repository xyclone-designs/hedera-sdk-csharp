// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Proto;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    class NftHookCallTest
    {
        public virtual void ConstructorWithNumericIdAndType()
        {
            var evm = new EvmHookCall(new byte[] { }, 25000);
            var call = new NftHookCall(2, evm, NftHookType.PRE_HOOK_SENDER);
            Assert.Equal(call.GetType(), NftHookType.PRE_HOOK_SENDER);
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
            var senderHook = new NftHookCall(2, new EvmHookCall(new byte[] { }, 10), NftHookType.PRE_HOOK_SENDER);
            var receiverHook = new NftHookCall(3, new EvmHookCall(new byte[] { }, 10), NftHookType.PRE_POST_HOOK_RECEIVER);
            tx.AddNftTransferWithHook(nftId, sender, receiver, senderHook, receiverHook);
            var body = tx.Build();
            var hasSenderPre = body.GetTokenTransfersList().Stream().FlatMap((tl) => tl.GetNftTransfersList().Stream()).AnyMatch((t) => t.HasPreTxSenderAllowanceHook());
            var hasReceiverPrePost = body.GetTokenTransfersList().Stream().FlatMap((tl) => tl.GetNftTransfersList().Stream()).AnyMatch((t) => t.HasPrePostTxReceiverAllowanceHook());
            AssertThat(hasSenderPre).IsTrue();
            AssertThat(hasReceiverPrePost).IsTrue();

            // Round-trip parse back
            var rebuilt = new TransferTransaction(TransactionBody.NewBuilder().SetCryptoTransfer(body).Build());
            var rebuiltNfts = rebuilt.GetTokenNftTransfers();
            Assert.Single(rebuiltNfts[token]);
        }
    }
}