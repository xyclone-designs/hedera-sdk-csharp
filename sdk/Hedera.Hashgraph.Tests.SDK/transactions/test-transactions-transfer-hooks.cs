// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    /// <include file="test-transactions-transfer-hooks.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Transactions.TransferTransactionHooksTest"]' />
    public class TransferTransactionHooksTest
    {
        [Fact]
        /// <include file="test-transactions-transfer-hooks.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.TransferTransactionHooksTest.ShouldAddHbarTransferWithPreTxAllowanceHook"]' />
        public virtual void ShouldAddHbarTransferWithPreTxAllowanceHook()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);
            var hookCall = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PreTxAllowanceHook);
            var result = tx.AddHbarTransferWithHook(accountId, amount, hookCall);
            Assert.Equal(result, tx);
            Assert.Single(tx.GetHbarTransfers());
            Assert.Equal(tx.GetHbarTransfers()[accountId], amount);
        }
        [Fact]
        /// <include file="test-transactions-transfer-hooks.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.TransferTransactionHooksTest.ShouldAddHbarTransferWithPrePostTxAllowanceHook"]' />
        public virtual void ShouldAddHbarTransferWithPrePostTxAllowanceHook()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(2000);
            var hookCall = new FungibleHookCall(456, new EvmHookCall(new byte[] { 4, 5, 6 }, 200000), FungibleHookType.PrePostTxAllowanceHook);
            var result = tx.AddHbarTransferWithHook(accountId, amount, hookCall);
            Assert.Equal(result, tx);
            Assert.Single(tx.GetHbarTransfers());
            Assert.Equal(tx.GetHbarTransfers()[accountId], amount);
        }
        [Fact]
        /// <include file="test-transactions-transfer-hooks.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.TransferTransactionHooksTest.ShouldNotAllowNullHookTypeRemovedByTypedAPI"]' />
        public virtual void ShouldNotAllowNullHookTypeRemovedByTypedAPI()
        {
            // No-op: hook type is encoded in FungibleHookCall now
            Assert.True(true);
        }
        [Fact]
        /// <include file="test-transactions-transfer-hooks.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.TransferTransactionHooksTest.ShouldUpdateExistingTransferWithHook"]' />
        public virtual void ShouldUpdateExistingTransferWithHook()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);

            // First add a regular transfer
            tx.AddHbarTransfer(accountId, amount);

            // Then add a hook to the existing transfer
            var hookCall = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PreTxAllowanceHook);
            var result = tx.AddHbarTransferWithHook(accountId, amount, hookCall);
            Assert.Equal(result, tx);
            Assert.Single(tx.GetHbarTransfers());
            Assert.Equal(tx.GetHbarTransfers()[accountId], Hbar.FromTinybars(2000));
        }
        [Fact]
        /// <include file="test-transactions-transfer-hooks.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.TransferTransactionHooksTest.ShouldCreateNewTransferWhenExistingTransferHasHook"]' />
        public virtual void ShouldCreateNewTransferWhenExistingTransferHasHook()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);

            // First add a transfer with a hook
            var hookCall1 = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PreTxAllowanceHook);
            tx.AddHbarTransferWithHook(accountId, amount, hookCall1);

            // Try to add another hook - should create a new transfer
            var hookCall2 = new FungibleHookCall(456, new EvmHookCall(new byte[] { 4, 5, 6 }, 200000), FungibleHookType.PrePostTxAllowanceHook);
            tx.AddHbarTransferWithHook(accountId, amount, hookCall2);
            Assert.Single(tx.GetHbarTransfers());
            Assert.Equal(tx.GetHbarTransfers()[accountId], Hbar.FromTinybars(2000));
        }
        [Fact]
        /// <include file="test-transactions-transfer-hooks.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.TransferTransactionHooksTest.ShouldHandleMultipleAccountsWithHooks"]' />
        public virtual void ShouldHandleMultipleAccountsWithHooks()
        {
            var tx = new TransferTransaction();
            var accountId1 = new AccountId(0, 0, 1);
            var accountId2 = new AccountId(0, 0, 2);
            var amount = Hbar.FromTinybars(1000);
            var hookCall1 = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PreTxAllowanceHook);
            var hookCall2 = new FungibleHookCall(456, new EvmHookCall(new byte[] { 4, 5, 6 }, 200000), FungibleHookType.PrePostTxAllowanceHook);
            tx.AddHbarTransferWithHook(accountId1, amount, hookCall1);
            tx.AddHbarTransferWithHook(accountId2, amount, hookCall2);
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            Assert.Equal(tx.GetHbarTransfers()[accountId1], amount);
            Assert.Equal(tx.GetHbarTransfers()[accountId2], amount);
        }
        [Fact]
        /// <include file="test-transactions-transfer-hooks.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.TransferTransactionHooksTest.ShouldThrowExceptionWhenFrozen"]' />
        public virtual void ShouldThrowExceptionWhenFrozen()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);
            var hookCall = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PreTxAllowanceHook);

            // Set up the transaction properly before freezing
            tx.TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.UtcNow);
            tx.NodeAccountIds = [AccountId.FromString("0.0.5005")];

            // Freeze the transaction
            tx.Freeze();

            Assert.Throws<InvalidOperationException>(() => tx.AddHbarTransferWithHook(accountId, amount, hookCall));
        }
    }
}
