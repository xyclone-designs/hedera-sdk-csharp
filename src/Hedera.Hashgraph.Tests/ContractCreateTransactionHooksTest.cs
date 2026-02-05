// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ContractCreateTransactionHooksTest
    {
        public virtual void TestContractCreateTransactionWithHooks()
        {

            // Create a test contract ID that the hook will reference (it can be any number for unit tests)
            ContractId targetContractId = new ContractId(100);

            // Create a test admin key
            PrivateKey adminKey = PrivateKey.GenerateED25519();

            // Create storage updates
            byte[] storageKey = new[]
            {
                0x01,
                0x02
            };
            byte[] storageValue = new[]
            {
                0x03,
                0x04
            };
            EvmHookStorageUpdate storageUpdate = new EvmHookStorageSlot(storageKey, storageValue);
            IList<EvmHookStorageUpdate> storageUpdates = Collections.SingletonList(storageUpdate);

            // Build two hooks, one with admin key and storage, one simple
            ContractCreateTransaction tx = new ContractCreateTransaction().SetGas(1000000).SetInitialBalance(Hbar.From(10)).AddHook(new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, new EvmHook(targetContractId, storageUpdates), adminKey.GetPublicKey())).AddHook(new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 2, new EvmHook(targetContractId)));
            var hooks = tx.GetHooks();
            AssertEquals(2, hooks.Count);
            var first = hooks[0];
            AssertEquals(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, first.GetExtensionPoint());
            AssertEquals(1, first.GetHookId());
            AssertEquals(adminKey.GetPublicKey(), first.GetAdminKey());
            AssertNotNull(first.GetHook());
            AssertEquals(1, first.GetHook().GetStorageUpdates().Count);
            var second = hooks[1];
            AssertEquals(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, second.GetExtensionPoint());
            AssertEquals(2, second.GetHookId());
            Assert.Null(second.GetAdminKey());
            AssertNotNull(second.GetHook());
            AssertTrue(second.GetHook().GetStorageUpdates().IsEmpty());
        }

        public virtual void TestContractCreateTransactionSetHooks()
        {
            ContractId targetContractId = new ContractId(200);
            var lambdaHook = new EvmHook(targetContractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            var tx = new ContractCreateTransaction().SetGas(500000).SetInitialBalance(Hbar.From(5)).SetHooks(Collections.SingletonList(hookDetails));
            var retrieved = tx.GetHooks();
            AssertEquals(1, retrieved.Count);
            AssertEquals(hookDetails, retrieved[0]);
        }

        public virtual void TestContractCreateTransactionHookValidationDuplicateIdsNotBlockedClientSide()
        {
            ContractId targetContractId = new ContractId(300);
            var tx = new ContractCreateTransaction().SetGas(250000).SetInitialBalance(Hbar.From(3)).AddHook(new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, new EvmHook(targetContractId))).AddHook(new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, new EvmHook(targetContractId)));
            var proto = tx.Build();
            AssertEquals(2, proto.GetHookCreationDetailsCount());
            AssertEquals(1, proto.GetHookCreationDetails(0).GetHookId());
            AssertEquals(1, proto.GetHookCreationDetails(1).GetHookId());
        }

        public virtual void TestContractCreateTransactionProtobufSerialization()
        {
            ContractId targetContractId = new ContractId(400);
            var tx = new ContractCreateTransaction().SetGas(750000).SetInitialBalance(Hbar.From(7)).AddHook(new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, new EvmHook(targetContractId)));
            var protoBody = tx.Build();
            AssertEquals(1, protoBody.GetHookCreationDetailsCount());
            var protoHook = protoBody.GetHookCreationDetails(0);
            AssertEquals(com.hedera.hashgraph.sdk.proto.HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, protoHook.GetExtensionPoint());
            AssertEquals(1, protoHook.GetHookId());
            AssertTrue(protoHook.HasEvmHook());
        }

        public virtual void TestContractCreateTransactionEmptyHooks()
        {
            var tx = new ContractCreateTransaction().SetGas(123456).SetInitialBalance(Hbar.From(1));
            var hooks = tx.GetHooks();
            AssertTrue(hooks.IsEmpty());
            var proto = tx.Build();
            AssertEquals(0, proto.GetHookCreationDetailsCount());
        }

        public virtual void TestContractCreateTransactionHooksPersistThroughBytesRoundTrip()
        {
            ContractId targetContractId = new ContractId(500);
            var lambdaHook = new EvmHook(targetContractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 3, lambdaHook);
            var original = new ContractCreateTransaction().SetGas(999999).SetInitialBalance(Hbar.From(9)).SetHooks(Collections.SingletonList(hookDetails));
            byte[] bytes = original.ToBytes();
            Transaction<TWildcardTodo> parsed = Transaction.FromBytes(bytes);
            AssertTrue(parsed is ContractCreateTransaction);
            var parsedTx = (ContractCreateTransaction)parsed;
            var parsedHooks = parsedTx.GetHooks();
            AssertEquals(1, parsedHooks.Count);
            var parsedHook = parsedHooks[0];
            AssertEquals(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, parsedHook.GetExtensionPoint());
            AssertEquals(3, parsedHook.GetHookId());
            AssertNotNull(parsedHook.GetHook());
            AssertTrue(parsedHook.GetHook().GetStorageUpdates().IsEmpty());
        }
    }
}