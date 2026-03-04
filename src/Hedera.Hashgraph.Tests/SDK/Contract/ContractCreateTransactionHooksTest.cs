// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

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
            byte[] storageKey = [ (byte)0x01, (byte)0x02 ];
            byte[] storageValue = [ (byte)0x03, (byte)0x04 ];
            EvmHookStorageUpdate storageUpdate = new EvmHookStorageSlot(storageKey, storageValue);
            IList<EvmHookStorageUpdate> storageUpdates = [storageUpdate];

			// Build two hooks, one with admin key and storage, one simple
			var tx = new ContractCreateTransaction
			{
				Gas = 1000000,
				InitialBalance = Hbar.From(10),
				HookCreationDetails_ =
				[
					new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, new EvmHook(targetContractId, storageUpdates), adminKey.GetPublicKey()),
					new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, new EvmHook(targetContractId))
				]
			};

            var hooks = tx.HookCreationDetails_;
            Assert.Equal(2, hooks.Count);
            var first = hooks[0];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, first.ExtensionPoint);
            Assert.Equal(1, first.HookId);
            Assert.Equal(adminKey.GetPublicKey(), first.AdminKey);
            Assert.NotNull(first.Hook);
            Assert.Equal(1, first.Hook.StorageUpdates.Count);
            var second = hooks[1];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, second.ExtensionPoint);
            Assert.Equal(2, second.HookId);
            Assert.Null(second.AdminKey);
            Assert.NotNull(second.Hook);
            Assert.True(second.Hook.StorageUpdates.Count == 0);
        }

        public virtual void TestContractCreateTransactionSetHooks()
        {
            ContractId targetContractId = new ContractId(200);
            var lambdaHook = new EvmHook(targetContractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var tx = new ContractCreateTransaction
            {
				Gas = 500000,
				InitialBalance = Hbar.From(5),
				HookCreationDetails_ = [hookDetails]
			};
            var retrieved = tx.HookCreationDetails_;
            Assert.Equal(1, retrieved.Count);
            Assert.Equal(hookDetails, retrieved[0]);
        }

        public virtual void TestContractCreateTransactionHookValidationDuplicateIdsNotBlockedClientSide()
        {
            ContractId targetContractId = new ContractId(300);
            var tx = new ContractCreateTransaction
            {
                Gas = 250000,
                InitialBalance = Hbar.From(3),
                HookCreationDetails_ = 
                [
					new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, new EvmHook(targetContractId)),
				    new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, new EvmHook(targetContractId))
				]
            };

            var proto = tx.ToProtobuf();

            Assert.Equal(2, proto.HookCreationDetails.Count);
            Assert.Equal(1, proto.HookCreationDetails[0].HookId);
            Assert.Equal(1, proto.HookCreationDetails[1].HookId);
        }

        public virtual void TestContractCreateTransactionProtobufSerialization()
        {
			ContractId targetContractId = new ContractId(400);
			var tx = new ContractCreateTransaction
			{
				Gas = 750000,
				InitialBalance = Hbar.From(7),
				HookCreationDetails_ =
				[
					new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, new EvmHook(targetContractId)),
				]
			};

            var protoBody = tx.ToProtobuf();
            Assert.Equal(1, protoBody.HookCreationDetails.Count);            
            var protoHook = protoBody.HookCreationDetails[0];

            Assert.Equal(Proto.HookExtensionPoint.AccountAllowanceHook, protoHook.ExtensionPoint);
            Assert.Equal(1, protoHook.HookId);
            Assert.True(protoHook.EvmHook is not null);
        }

        public virtual void TestContractCreateTransactionEmptyHooks()
        {
            var tx = new ContractCreateTransaction
            {
				Gas = 123456,
				InitialBalance = Hbar.From(1),
			};
            var hooks = tx.HookCreationDetails_;
            Assert.True(hooks.Count == 0);
            var proto = tx.ToProtobuf();
            Assert.Equal(0, proto.HookCreationDetails.Count);
        }

        public virtual void TestContractCreateTransactionHooksPersistThroughBytesRoundTrip()
        {
            ContractId targetContractId = new ContractId(500);
            var lambdaHook = new EvmHook(targetContractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 3, lambdaHook);
            var original = new ContractCreateTransaction
            {
				Gas = 999999,
				InitialBalance = Hbar.From(9),
				HookCreationDetails_ = [hookDetails],
			};

            byte[] bytes = original.ToBytes();
            ITransaction parsed = ITransaction.FromBytes(bytes);
            Assert.True(parsed is ContractCreateTransaction);
            var parsedTx = (ContractCreateTransaction)parsed;
            var parsedHooks = parsedTx.HookCreationDetails_;
            Assert.Equal(1, parsedHooks.Count);
            var parsedHook = parsedHooks[0];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, parsedHook.ExtensionPoint);
            Assert.Equal(3, parsedHook.HookId);
            Assert.NotNull(parsedHook.Hook);
            Assert.True(parsedHook.Hook.StorageUpdates.Count == 0);
        }
    }
}