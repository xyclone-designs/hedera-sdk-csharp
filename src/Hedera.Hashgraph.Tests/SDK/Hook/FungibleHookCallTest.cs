// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Token;

using System.Linq;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    public class FungibleHookCallTest
    {
        [Fact]
        public virtual void ConstructorWithNumericIdAndType()
        {
            var evm = new EvmHookCall(new byte[] { }, 25000);
            var call = new FungibleHookCall(2, evm, FungibleHookType.PreTxAllowanceHook);

            Assert.Equal(call.Type, FungibleHookType.PreTxAllowanceHook);
        }
        [Fact]
        public virtual void HbarTransferSerializesHookByType()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 123);
            var hookPre = new FungibleHookCall(2, new EvmHookCall(new byte[] { }, 10), FungibleHookType.PreTxAllowanceHook);
            var hookPrePost = new FungibleHookCall(3, new EvmHookCall(new byte[] { }, 10), FungibleHookType.PrePostTxAllowanceHook);
            
            tx.AddHbarTransferWithHook(accountId, Hbar.FromTinybars(1), hookPre);
            tx.AddHbarTransferWithHook(new AccountId(0, 0, 124), Hbar.FromTinybars(2), hookPrePost);
            
            var body = tx.ToProtobuf();
            var list = body.Transfers.AccountAmounts;
            
            Assert.True(list.Any((a) => a.PreTxAllowanceHook is not null));
            Assert.True(list.Any((a) => a.PrePostTxAllowanceHook is not null));

            // Round-trip
            var rebuilt = new TransferTransaction(new Proto.Services.TransactionBody { CryptoTransfer = body });
            
            Assert.Equal(rebuilt.GetHbarTransfers()[accountId], Hbar.FromTinybars(1));
        }
        [Fact]
        public virtual void TokenTransferSerializesHookByType()
        {
            var tx = new TransferTransaction();
            var token = new TokenId(0, 0, 3333);
            var sender = new AccountId(0, 0, 5001);
            var hookPre = new FungibleHookCall(2, new EvmHookCall(new byte[] { }, 10), FungibleHookType.PreTxAllowanceHook);
            var hookPrePost = new FungibleHookCall(3, new EvmHookCall(new byte[] { }, 10), FungibleHookType.PrePostTxAllowanceHook);
            
            tx.AddTokenTransferWithHook(token, sender, -100, hookPre);
            tx.AddTokenTransfer(token, new AccountId(0, 0, 5002), 100);
            tx.AddTokenTransferWithHook(token, new AccountId(0, 0, 5003), -200, hookPrePost);
            tx.AddTokenTransfer(token, new AccountId(0, 0, 5004), 200);
            
            var body = tx.ToProtobuf();

            var anyPre = body.TokenTransfers.Any((tl) => tl.Transfers.Any((a) => a.PreTxAllowanceHook is not null));
            var anyPrePost = body.TokenTransfers.Any((tl) => tl.Transfers.Any((a) => a.PrePostTxAllowanceHook is not null));
            
            Assert.True(anyPre);
            Assert.True(anyPrePost);

            // Round-trip parse back
            var rebuilt = new TransferTransaction(new Proto.Services.TransactionBody { CryptoTransfer = body });
            var tokenTransfers = rebuilt.GetTokenTransfers();
            
            Assert.Equal(tokenTransfers[token][sender], -100);
        }
    }
}