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
            IList<EvmHookStorageUpdate> storageUpdates = Collections.SingletonList(storageUpdate);

            // Create account create transaction with hooks
            var lambdaHookWithStorage = new EvmHook(contractId, storageUpdates);
            var hookWithAdmin = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHookWithStorage, adminKey.GetPublicKey());
            var simpleLambdaHook = new EvmHook(contractId);
            var simpleHook = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 2, simpleLambdaHook);
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(100)).AddHook(hookWithAdmin).AddHook(simpleHook); // Simple hook without admin key or storage

            // Verify hooks were added
            IList<HookCreationDetails> hookDetails = transaction.GetHooks();
            AssertEquals(2, hookDetails.Count);

            // Verify first hook
            HookCreationDetails firstHook = hookDetails[0];
            AssertEquals(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, firstHook.GetExtensionPoint());
            AssertEquals(1, firstHook.GetHookId());
            AssertEquals(adminKey.GetPublicKey(), firstHook.GetAdminKey());
            AssertNotNull(firstHook.GetHook());
            AssertEquals(1, firstHook.GetHook().GetStorageUpdates().Count);

            // Verify second hook
            HookCreationDetails secondHook = hookDetails[1];
            AssertEquals(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, secondHook.GetExtensionPoint());
            AssertEquals(2, secondHook.GetHookId());
            Assert.Null(secondHook.GetAdminKey());
            AssertNotNull(secondHook.GetHook());
            AssertTrue(secondHook.GetHook().GetStorageUpdates().IsEmpty());
        }

        public virtual void TestAccountCreateTransactionSetHooks()
        {
            ContractId contractId = new ContractId(200);

            // Create hook details manually
            EvmHook lambdaEvmHook = new EvmHook(contractId);
            HookCreationDetails hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaEvmHook);

            // Set hooks using setHookCreationDetails
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(50)).SetHooks(Collections.SingletonList(hookDetails));

            // Verify hooks were set
            IList<HookCreationDetails> retrievedHooks = transaction.GetHooks();
            AssertEquals(1, retrievedHooks.Count);
            AssertEquals(hookDetails, retrievedHooks[0]);
        }

        public virtual void TestAccountCreateTransactionHookValidation()
        {
            ContractId contractId = new ContractId(300);

            // Test duplicate hook IDs
            var lambdaHook = new EvmHook(contractId);
            var hook1 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            var hook2 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook); // Duplicate ID
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(25)).AddHook(hook1).AddHook(hook2); // Duplicate hook ID

            // Client-side duplicate ID validation was removed; ensure build includes both entries
            var proto = transaction.Build();
            AssertEquals(2, proto.GetHookCreationDetailsCount());
            AssertEquals(1, proto.GetHookCreationDetails(0).GetHookId());
            AssertEquals(1, proto.GetHookCreationDetails(1).GetHookId());
        }

        public virtual void TestAccountCreateTransactionProtobufSerialization()
        {
            ContractId contractId = new ContractId(400);

            // Create transaction with hooks
            var lambdaHook = new EvmHook(contractId);
            var hook = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(75)).AddHook(hook);

            // Build the protobuf
            var protoBody = transaction.Build();

            // Verify hook creation details are included
            AssertEquals(1, protoBody.GetHookCreationDetailsCount());
            var protoHookDetails = protoBody.GetHookCreationDetails(0);
            AssertEquals(com.hedera.hashgraph.sdk.proto.HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, protoHookDetails.GetExtensionPoint());
            AssertEquals(1, protoHookDetails.GetHookId());
            AssertTrue(protoHookDetails.HasEvmHook());
        }

        public virtual void TestAccountCreateTransactionEmptyHooks()
        {

            // Test transaction without hooks
            AccountCreateTransaction transaction = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(100));

            // Verify no hooks
            IList<HookCreationDetails> hookDetails = transaction.GetHooks();
            AssertTrue(hookDetails.IsEmpty());

            // Should build successfully
            var protoBody = transaction.Build();
            AssertEquals(0, protoBody.GetHookCreationDetailsCount());
        }

        public virtual void TestAccountCreateTransactionHooksPersistThroughBytesRoundTrip()
        {

            // Create contract and hook details
            ContractId contractId = new ContractId(500);
            EvmHook lambdaEvmHook = new EvmHook(contractId);
            HookCreationDetails hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 3, lambdaEvmHook);

            // Create transaction with set hooks
            AccountCreateTransaction originalTx = new AccountCreateTransaction().SetKey(PrivateKey.GenerateED25519().GetPublicKey()).SetInitialBalance(Hbar.From(123)).SetHooks(Collections.SingletonList(hookDetails));

            // Serialize to bytes then deserialize back
            byte[] bytes = originalTx.ToBytes();
            Transaction<TWildcardTodo> parsed = Transaction.FromBytes(bytes);
            AssertTrue(parsed is AccountCreateTransaction);
            AccountCreateTransaction parsedTx = (AccountCreateTransaction)parsed;

            // Verify hook information persisted
            IList<HookCreationDetails> parsedHooks = parsedTx.GetHooks();
            AssertEquals(1, parsedHooks.Count);
            HookCreationDetails parsedHook = parsedHooks[0];
            AssertEquals(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, parsedHook.GetExtensionPoint());
            AssertEquals(3, parsedHook.GetHookId());
            AssertNotNull(parsedHook.GetHook());
            AssertTrue(parsedHook.GetHook().GetStorageUpdates().IsEmpty());
        }
    }
}