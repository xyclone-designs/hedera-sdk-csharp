// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class FungibleHookCallTest
    {
        virtual void ConstructorWithNumericIdAndType()
        {
            var evm = new EvmHookCall(new byte[] { }, 25000);
            var call = new FungibleHookCall(2, evm, FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            Assert.Equal(call.GetType(), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
        }

        virtual void NullTypeThrows()
        {
            var evm = new EvmHookCall(new byte[] { }, 1);
            AssertThatThrownBy(() => new FungibleHookCall(1, evm, null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("type cannot be null");
        }

        virtual void HbarTransferSerializesHookByType()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 123);
            var hookPre = new FungibleHookCall(2, new EvmHookCall(new byte[] { }, 10), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            var hookPrePost = new FungibleHookCall(3, new EvmHookCall(new byte[] { }, 10), FungibleHookType.PRE_POST_TX_ALLOWANCE_HOOK);
            tx.AddHbarTransferWithHook(accountId, Hbar.FromTinybars(1), hookPre);
            tx.AddHbarTransferWithHook(new AccountId(0, 0, 124), Hbar.FromTinybars(2), hookPrePost);
            var body = tx.Build();
            var list = body.GetTransfers().GetAccountAmountsList();
            AssertThat(list.Stream().AnyMatch((a) => a.HasPreTxAllowanceHook())).IsTrue();
            AssertThat(list.Stream().AnyMatch((a) => a.HasPrePostTxAllowanceHook())).IsTrue();

            // Round-trip
            var rebuilt = new TransferTransaction(TransactionBody.NewBuilder().SetCryptoTransfer(body).Build());
            Assert.Equal(rebuilt.GetHbarTransfers()[accountId], Hbar.FromTinybars(1));
        }

        virtual void TokenTransferSerializesHookByType()
        {
            var tx = new TransferTransaction();
            var token = new TokenId(0, 0, 3333);
            var sender = new AccountId(0, 0, 5001);
            var hookPre = new FungibleHookCall(2, new EvmHookCall(new byte[] { }, 10), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            var hookPrePost = new FungibleHookCall(3, new EvmHookCall(new byte[] { }, 10), FungibleHookType.PRE_POST_TX_ALLOWANCE_HOOK);
            tx.AddTokenTransferWithHook(token, sender, -100, hookPre);
            tx.AddTokenTransfer(token, new AccountId(0, 0, 5002), 100);
            tx.AddTokenTransferWithHook(token, new AccountId(0, 0, 5003), -200, hookPrePost);
            tx.AddTokenTransfer(token, new AccountId(0, 0, 5004), 200);
            var body = tx.Build();
            var anyPre = body.GetTokenTransfersList().Stream().FlatMap((tl) => tl.GetTransfersList().Stream()).AnyMatch((a) => a.HasPreTxAllowanceHook());
            var anyPrePost = body.GetTokenTransfersList().Stream().FlatMap((tl) => tl.GetTransfersList().Stream()).AnyMatch((a) => a.HasPrePostTxAllowanceHook());
            AssertThat(anyPre).IsTrue();
            AssertThat(anyPrePost).IsTrue();

            // Round-trip parse back
            var rebuilt = new TransferTransaction(TransactionBody.NewBuilder().SetCryptoTransfer(body).Build());
            var tokenTransfers = rebuilt.GetTokenTransfers();
            Assert.Equal(tokenTransfers[token][sender], -100);
        }
    }
}