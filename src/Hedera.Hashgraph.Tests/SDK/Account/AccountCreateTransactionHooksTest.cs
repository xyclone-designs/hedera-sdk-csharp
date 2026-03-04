// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountCreateTransactionHooksTest
    {
        public virtual void TestAccountCreateTransactionWithHooks()
        {
            // Create a test contract ID
            ContractId contractId = new ContractId(100);

            // Create a test admin key
            PrivateKey adminKey = PrivateKey.GenerateED25519();

            // Create storage updates
            byte[] storageKey = [ 0x01, 0x02 ];
            byte[] storageValue = [ 0x03, 0x04 ];
            EvmHookStorageUpdate storageUpdate = new EvmHookStorageSlot(storageKey, storageValue);
            IList<EvmHookStorageUpdate> storageUpdates = [storageUpdate];

            // Create account create transaction with hooks
            var lambdaHookWithStorage = new EvmHook(contractId, storageUpdates);
            var hookWithAdmin = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHookWithStorage, adminKey.GetPublicKey());
            var simpleLambdaHook = new EvmHook(contractId);
            var simpleHook = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, simpleLambdaHook);
            AccountCreateTransaction transaction = new AccountCreateTransaction
            {
				Key = PrivateKey.GenerateED25519().GetPublicKey(),
				InitialBalance = Hbar.From(100),
                HookCreationDetails = [hookWithAdmin, simpleHook]
                
			}; // Simple hook without admin key or storage

            // Verify hooks were added
            IList<HookCreationDetails> hookDetails = transaction.HookCreationDetails;
            Assert.Equal(2, hookDetails.Count);

            // Verify first hook
            HookCreationDetails firstHook = hookDetails[0];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, firstHook.ExtensionPoint);
            Assert.Equal(1, firstHook.HookId);
            Assert.Equal(adminKey.GetPublicKey(), firstHook.AdminKey);
            Assert.NotNull(firstHook.HookId);
            Assert.Equal(1, firstHook.Hook.StorageUpdates.Count);

            // Verify second hook
            HookCreationDetails secondHook = hookDetails[1];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, secondHook.ExtensionPoint);
            Assert.Equal(2, secondHook.HookId);
            Assert.Null(secondHook.AdminKey);
            Assert.NotNull(secondHook.HookId);
            Assert.True(secondHook.Hook.StorageUpdates.Count == 0);
        }

        public virtual void TestAccountCreateTransactionSetHooks()
        {
            ContractId contractId = new ContractId(200);

            // Create hook details manually
            EvmHook lambdaEvmHook = new EvmHook(contractId);
            HookCreationDetails hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaEvmHook);

            // Set hooks using setHookCreationDetails
            AccountCreateTransaction transaction = new AccountCreateTransaction
            {
				Key = PrivateKey.GenerateED25519().GetPublicKey(),
				InitialBalance = Hbar.From(50),
			    HookCreationDetails = [hookDetails],
			};

            // Verify hooks were set
            IList<HookCreationDetails> retrievedHooks = transaction.HookCreationDetails;
            Assert.Equal(1, retrievedHooks.Count);
            Assert.Equal(hookDetails, retrievedHooks[0]);
        }

        public virtual void TestAccountCreateTransactionHookValidation()
        {
            ContractId contractId = new ContractId(300);

            // Test duplicate hook IDs
            var lambdaHook = new EvmHook(contractId);
            var hook1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var hook2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook); // Duplicate ID
            AccountCreateTransaction transaction = new AccountCreateTransaction
            {
				Key = PrivateKey.GenerateED25519().GetPublicKey(),
				InitialBalance = Hbar.From(25),
                HookCreationDetails = [hook1, hook2]

			}; // Duplicate hook ID

            // Client-side duplicate ID validation was removed; ensure build includes both entries
            var proto = transaction.ToProtobuf();

            Assert.Equal(2, proto.HookCreationDetails.Count);
            Assert.Equal(1, proto.HookCreationDetails[0].HookId);
            Assert.Equal(1, proto.HookCreationDetails[1].HookId);
        }

        public virtual void TestAccountCreateTransactionProtobufSerialization()
        {
            ContractId contractId = new ContractId(400);

            // Create transaction with hooks
            var lambdaHook = new EvmHook(contractId);
            var hook = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            AccountCreateTransaction transaction = new AccountCreateTransaction
            {
				Key = PrivateKey.GenerateED25519().GetPublicKey(),
				InitialBalance = Hbar.From(75),
                HookCreationDetails = [hook]
			};

            // Build the protobuf
            var protoBody = transaction.ToProtobuf();

            // Verify hook creation details are included
            Assert.Equal(1, protoBody.HookCreationDetails.Count);
            var protoHookDetails = protoBody.HookCreationDetails[0];
            Assert.Equal(Proto.HookExtensionPoint.AccountAllowanceHook, protoHookDetails.ExtensionPoint);
            Assert.Equal(1, protoHookDetails.HookId);
            Assert.True(protoHookDetails.EvmHook is not null);
        }

        public virtual void TestAccountCreateTransactionEmptyHooks()
        {
            // Test transaction without hooks
            AccountCreateTransaction transaction = new AccountCreateTransaction
            {
				Key = PrivateKey.GenerateED25519().GetPublicKey(),
				InitialBalance = Hbar.From(100)
			};

            // Verify no hooks
            IList<HookCreationDetails> hookDetails = transaction.HookCreationDetails;
            Assert.True(hookDetails.Count == 0);

            // Should build successfully
            var protoBody = transaction.ToProtobuf();

            Assert.Equal(0, protoBody.HookCreationDetails.Count);
        }

        public virtual void TestAccountCreateTransactionHooksPersistThroughBytesRoundTrip()
        {
            // Create contract and hook details
            ContractId contractId = new ContractId(500);
            EvmHook lambdaEvmHook = new EvmHook(contractId);
            HookCreationDetails hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 3, lambdaEvmHook);

            // Create transaction with set hooks
            AccountCreateTransaction originalTx = new AccountCreateTransaction
            {
				Key = PrivateKey.GenerateED25519().GetPublicKey(),
				InitialBalance = Hbar.From(123),
				HookCreationDetails = [hookDetails]
			};

            // Serialize to bytes then deserialize back
            byte[] bytes = originalTx.ToBytes();
            ITransaction parsed = ITransaction.FromBytes(bytes);
            Assert.True(parsed is AccountCreateTransaction);
            AccountCreateTransaction parsedTx = (AccountCreateTransaction)parsed;

            // Verify hook information persisted
            IList<HookCreationDetails> parsedHooks = parsedTx.HookCreationDetails;

            Assert.Equal(1, parsedHooks.Count);
            HookCreationDetails parsedHook = parsedHooks[0];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, parsedHook.ExtensionPoint);
            Assert.Equal(3, parsedHook.HookId);
            Assert.NotNull(parsedHook.HookId);
            Assert.True(parsedHook.Hook.StorageUpdates.Count == 0);
        }
    }
}