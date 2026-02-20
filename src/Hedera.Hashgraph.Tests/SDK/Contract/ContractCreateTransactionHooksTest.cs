// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Contract
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
            IList<EvmHookStorageUpdate> storageUpdates = [storageUpdate];

            // Build two hooks, one with admin key and storage, one simple
            ContractCreateTransaction tx = new ContractCreateTransaction().SetGas(1000000).SetInitialBalance(Hbar.From(10)).AddHook(new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, new EvmHook(targetContractId, storageUpdates), adminKey.GetPublicKey())).AddHook(new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, new EvmHook(targetContractId)));
            var hooks = tx.GetHooks();
            Assert.Equal(2, hooks.Count);
            var first = hooks[0];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, first.GetExtensionPoint());
            Assert.Equal(1, first.GetHookId());
            Assert.Equal(adminKey.GetPublicKey(), first.GetAdminKey());
            Assert.NotNull(first.GetHook());
            Assert.Equal(1, first.GetHook().GetStorageUpdates().Count);
            var second = hooks[1];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, second.GetExtensionPoint());
            Assert.Equal(2, second.GetHookId());
            Assert.Null(second.GetAdminKey());
            Assert.NotNull(second.GetHook());
            Assert.True(second.GetHook().GetStorageUpdates().Count == 0);
        }

        public virtual void TestContractCreateTransactionSetHooks()
        {
            ContractId targetContractId = new ContractId(200);
            var lambdaHook = new EvmHook(targetContractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var tx = new ContractCreateTransaction().SetGas(500000).SetInitialBalance(Hbar.From(5)).SetHooks([hookDetails]);
            var retrieved = tx.GetHooks();
            Assert.Equal(1, retrieved.Count);
            Assert.Equal(hookDetails, retrieved[0]);
        }

        public virtual void TestContractCreateTransactionHookValidationDuplicateIdsNotBlockedClientSide()
        {
            ContractId targetContractId = new ContractId(300);
            var tx = new ContractCreateTransaction().SetGas(250000).SetInitialBalance(Hbar.From(3)).AddHook(new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, new EvmHook(targetContractId))).AddHook(new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, new EvmHook(targetContractId)));
            var proto = tx.Build();
            Assert.Equal(2, proto.GetHookCreationDetailsCount());
            Assert.Equal(1, proto.GetHookCreationDetails(0).GetHookId());
            Assert.Equal(1, proto.GetHookCreationDetails(1).GetHookId());
        }

        public virtual void TestContractCreateTransactionProtobufSerialization()
        {
            ContractId targetContractId = new ContractId(400);
            var tx = new ContractCreateTransaction().SetGas(750000).SetInitialBalance(Hbar.From(7)).AddHook(new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, new EvmHook(targetContractId)));
            var protoBody = tx.Build();
            Assert.Equal(1, protoBody.GetHookCreationDetailsCount());
            var protoHook = protoBody.GetHookCreationDetails(0);
            Assert.Equal(Proto.HookExtensionPoint.AccountAllowanceHook, protoHook.GetExtensionPoint());
            Assert.Equal(1, protoHook.GetHookId());
            Assert.True(protoHook.HasEvmHook());
        }

        public virtual void TestContractCreateTransactionEmptyHooks()
        {
            var tx = new ContractCreateTransaction().SetGas(123456).SetInitialBalance(Hbar.From(1));
            var hooks = tx.GetHooks();
            Assert.True(hooks.Count == 0);
            var proto = tx.Build();
            Assert.Equal(0, proto.GetHookCreationDetailsCount());
        }

        public virtual void TestContractCreateTransactionHooksPersistThroughBytesRoundTrip()
        {
            ContractId targetContractId = new ContractId(500);
            var lambdaHook = new EvmHook(targetContractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 3, lambdaHook);
            var original = new ContractCreateTransaction().SetGas(999999).SetInitialBalance(Hbar.From(9)).SetHooks([hookDetails]);
            byte[] bytes = original.ToBytes();
            Transaction<TWildcardTodo> parsed = Transaction.FromBytes(bytes);
            Assert.True(parsed is ContractCreateTransaction);
            var parsedTx = (ContractCreateTransaction)parsed;
            var parsedHooks = parsedTx.GetHooks();
            Assert.Equal(1, parsedHooks.Count);
            var parsedHook = parsedHooks[0];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, parsedHook.GetExtensionPoint());
            Assert.Equal(3, parsedHook.GetHookId());
            Assert.NotNull(parsedHook.GetHook());
            Assert.True(parsedHook.GetHook().GetStorageUpdates().Count == 0);
        }
    }
}