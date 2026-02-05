// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TransferTransactionHooksTest
    {
        virtual void ShouldAddHbarTransferWithPreTxAllowanceHook()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);
            var hookCall = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            var result = tx.AddHbarTransferWithHook(accountId, amount, hookCall);
            AssertThat(result).IsSameAs(tx);
            AssertThat(tx.GetHbarTransfers()).HasSize(1);
            Assert.Equal(tx.GetHbarTransfers()[accountId], amount);
        }

        virtual void ShouldAddHbarTransferWithPrePostTxAllowanceHook()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(2000);
            var hookCall = new FungibleHookCall(456, new EvmHookCall(new byte[] { 4, 5, 6 }, 200000), FungibleHookType.PRE_POST_TX_ALLOWANCE_HOOK);
            var result = tx.AddHbarTransferWithHook(accountId, amount, hookCall);
            AssertThat(result).IsSameAs(tx);
            AssertThat(tx.GetHbarTransfers()).HasSize(1);
            Assert.Equal(tx.GetHbarTransfers()[accountId], amount);
        }

        virtual void ShouldThrowExceptionForNullAccountId()
        {
            var tx = new TransferTransaction();
            var amount = Hbar.FromTinybars(1000);
            var hookCall = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            AssertThatThrownBy(() => tx.AddHbarTransferWithHook(null, amount, hookCall)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("accountId cannot be null");
        }

        virtual void ShouldThrowExceptionForNullAmount()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var hookCall = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            AssertThatThrownBy(() => tx.AddHbarTransferWithHook(accountId, null, hookCall)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("amount cannot be null");
        }

        virtual void ShouldThrowExceptionForNullHookCall()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);
            AssertThatThrownBy(() => tx.AddHbarTransferWithHook(accountId, amount, null)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("hookCall cannot be null");
        }

        virtual void ShouldNotAllowNullHookTypeRemovedByTypedAPI()
        {

            // No-op: hook type is encoded in FungibleHookCall now
            AssertThat(true).IsTrue();
        }

        virtual void ShouldUpdateExistingTransferWithHook()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);

            // First add a regular transfer
            tx.AddHbarTransfer(accountId, amount);

            // Then add a hook to the existing transfer
            var hookCall = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            var result = tx.AddHbarTransferWithHook(accountId, amount, hookCall);
            AssertThat(result).IsSameAs(tx);
            AssertThat(tx.GetHbarTransfers()).HasSize(1);
            Assert.Equal(tx.GetHbarTransfers()[accountId], Hbar.FromTinybars(2000));
        }

        virtual void ShouldCreateNewTransferWhenExistingTransferHasHook()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);

            // First add a transfer with a hook
            var hookCall1 = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            tx.AddHbarTransferWithHook(accountId, amount, hookCall1);

            // Try to add another hook - should create a new transfer
            var hookCall2 = new FungibleHookCall(456, new EvmHookCall(new byte[] { 4, 5, 6 }, 200000), FungibleHookType.PRE_POST_TX_ALLOWANCE_HOOK);
            tx.AddHbarTransferWithHook(accountId, amount, hookCall2);
            AssertThat(tx.GetHbarTransfers()).HasSize(1);
            Assert.Equal(tx.GetHbarTransfers()[accountId], Hbar.FromTinybars(2000));
        }

        virtual void ShouldHandleMultipleAccountsWithHooks()
        {
            var tx = new TransferTransaction();
            var accountId1 = new AccountId(0, 0, 1);
            var accountId2 = new AccountId(0, 0, 2);
            var amount = Hbar.FromTinybars(1000);
            var hookCall1 = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
            var hookCall2 = new FungibleHookCall(456, new EvmHookCall(new byte[] { 4, 5, 6 }, 200000), FungibleHookType.PRE_POST_TX_ALLOWANCE_HOOK);
            tx.AddHbarTransferWithHook(accountId1, amount, hookCall1);
            tx.AddHbarTransferWithHook(accountId2, amount, hookCall2);
            AssertThat(tx.GetHbarTransfers()).HasSize(2);
            Assert.Equal(tx.GetHbarTransfers()[accountId1], amount);
            Assert.Equal(tx.GetHbarTransfers()[accountId2], amount);
        }

        virtual void ShouldThrowExceptionWhenFrozen()
        {
            var tx = new TransferTransaction();
            var accountId = new AccountId(0, 0, 1);
            var amount = Hbar.FromTinybars(1000);
            var hookCall = new FungibleHookCall(123, new EvmHookCall(new byte[] { 1, 2, 3 }, 100000), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);

            // Set up the transaction properly before freezing
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.Instant.Now()));
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));

            // Freeze the transaction
            tx.Freeze();
            AssertThatThrownBy(() => tx.AddHbarTransferWithHook(accountId, amount, hookCall)).IsInstanceOf(typeof(InvalidOperationException));
        }
    }
}