// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

using System;

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

            // Create account create transaction with hooks
            var lambdaHookWithStorage = new EvmHook(contractId, storageUpdates);
            var hookWithAdmin = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHookWithStorage, adminKey.GetPublicKey());
            var simpleLambdaHook = new EvmHook(contractId);
            var simpleHook = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, simpleLambdaHook);
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(100)).AddHook(hookWithAdmin).AddHook(simpleHook); // Simple hook without admin key or storage

            // Verify hooks were added
            IList<HookCreationDetails> hookDetails = transaction.GetHooks();
            Assert.Equal(2, hookDetails.Count);

            // Verify first hook
            HookCreationDetails firstHook = hookDetails[0];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, firstHook.GetExtensionPoint());
            Assert.Equal(1, firstHook.GetHookId());
            Assert.Equal(adminKey.GetPublicKey(), firstHook.GetAdminKey());
            Assert.NotNull(firstHook.GetHook());
            Assert.Equal(1, firstHook.GetHook().GetStorageUpdates().Count);

            // Verify second hook
            HookCreationDetails secondHook = hookDetails[1];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, secondHook.GetExtensionPoint());
            Assert.Equal(2, secondHook.GetHookId());
            Assert.Null(secondHook.GetAdminKey());
            Assert.NotNull(secondHook.GetHook());
            Assert.True(secondHook.GetHook().GetStorageUpdates().Count == 0);
        }

        public virtual void TestAccountCreateTransactionSetHooks()
        {
            ContractId contractId = new ContractId(200);

            // Create hook details manually
            EvmHook lambdaEvmHook = new EvmHook(contractId);
            HookCreationDetails hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaEvmHook);

            // Set hooks using setHookCreationDetails
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(50)).SetHooks([hookDetails]);

            // Verify hooks were set
            IList<HookCreationDetails> retrievedHooks = transaction.GetHooks();
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
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(25)).AddHook(hook1).AddHook(hook2); // Duplicate hook ID

            // Client-side duplicate ID validation was removed; ensure build includes both entries
            var proto = transaction.Build();
            Assert.Equal(2, proto.GetHookCreationDetailsCount());
            Assert.Equal(1, proto.GetHookCreationDetails(0).GetHookId());
            Assert.Equal(1, proto.GetHookCreationDetails(1).GetHookId());
        }

        public virtual void TestAccountCreateTransactionProtobufSerialization()
        {
            ContractId contractId = new ContractId(400);

            // Create transaction with hooks
            var lambdaHook = new EvmHook(contractId);
            var hook = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(75)).AddHook(hook);

            // Build the protobuf
            var protoBody = transaction.Build();

            // Verify hook creation details are included
            Assert.Equal(1, protoBody.GetHookCreationDetailsCount());
            var protoHookDetails = protoBody.GetHookCreationDetails(0);
            Assert.Equal(Proto.HookExtensionPoint.AccountAllowanceHook, protoHookDetails.GetExtensionPoint());
            Assert.Equal(1, protoHookDetails.GetHookId());
            Assert.True(protoHookDetails.HasEvmHook());
        }

        public virtual void TestAccountCreateTransactionEmptyHooks()
        {

            // Test transaction without hooks
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(100));

            // Verify no hooks
            IList<HookCreationDetails> hookDetails = transaction.GetHooks();
            Assert.True(hookDetails.Count == 0);

            // Should build successfully
            var protoBody = transaction.Build();
            Assert.Equal(0, protoBody.GetHookCreationDetailsCount());
        }

        public virtual void TestAccountCreateTransactionHooksPersistThroughBytesRoundTrip()
        {

            // Create contract and hook details
            ContractId contractId = new ContractId(500);
            EvmHook lambdaEvmHook = new EvmHook(contractId);
            HookCreationDetails hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 3, lambdaEvmHook);

            // Create transaction with set hooks
            AccountCreateTransaction originalTx = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(123)).SetHooks([hookDetails]);

            // Serialize to bytes then deserialize back
            byte[] bytes = originalTx.ToBytes();
            Transaction<TWildcardTodo> parsed = Transaction.FromBytes(bytes);
            Assert.True(parsed is AccountCreateTransaction);
            AccountCreateTransaction parsedTx = (AccountCreateTransaction)parsed;

            // Verify hook information persisted
            IList<HookCreationDetails> parsedHooks = parsedTx.GetHooks();
            Assert.Equal(1, parsedHooks.Count);
            HookCreationDetails parsedHook = parsedHooks[0];
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, parsedHook.GetExtensionPoint());
            Assert.Equal(3, parsedHook.GetHookId());
            Assert.NotNull(parsedHook.GetHook());
            Assert.True(parsedHook.GetHook().GetStorageUpdates().Count == 0);
        }
    }
}