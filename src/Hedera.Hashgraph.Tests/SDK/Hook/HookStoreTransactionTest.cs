// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Proto;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    public class HookStoreTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly HookId TEST_HOOK_ID = new HookId(new HookEntityId(AccountId.FromString("0.0.5006")), 42);
        private static readonly IList<EvmHookStorageUpdate> TEST_UPDATES = List.Of(new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 }), new EvmHookStorageSlot(new byte[] { 0x03 }, new byte[] { 0x04 }));
        readonly DateTimeOffset TEST_VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private HookStoreTransaction SpawnTestTransaction()
        {
            return new HookStoreTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START)).SetHookId(TEST_HOOK_ID).SetStorageUpdates(TEST_UPDATES).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(TEST_PRIVATE_KEY);
        }

        public virtual void BytesRoundTripNoSetters()
        {
            var tx = new HookStoreTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void BytesRoundTripWithSetters()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
            Assert.IsType<HookStoreTransaction>(tx2);
        }

        // HookStoreTransaction is not schedulable; no scheduled mapping exists.
        public virtual void ConstructFromTransactionBodyProtobuf()
        {
            var hookBody = HookStoreTransactionBody.NewBuilder().SetHookId(TEST_HOOK_ID.ToProtobuf()).AddAllStorageUpdates(TEST_UPDATES.Stream().Map(EvmHookStorageUpdate.ToProtobuf()).ToList()).Build();
            var txBody = TransactionBody.NewBuilder().SetHookStore(hookBody).Build();
            var tx = new HookStoreTransaction(txBody);
            Assert.Equal(tx.GetHookId(), TEST_HOOK_ID);
            AssertThat(tx.GetStorageUpdates()).HasSize(TEST_UPDATES.Count);
        }

        public virtual void SettersAndFrozenBehavior()
        {
            var tx = new HookStoreTransaction().SetHookId(TEST_HOOK_ID).SetStorageUpdates(TEST_UPDATES);
            Assert.Equal(tx.GetHookId(), TEST_HOOK_ID);
            Assert.Equal(tx.GetStorageUpdates(), TEST_UPDATES);
            var frozen = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => frozen.SetHookId(TEST_HOOK_ID));
            Assert.Throws<InvalidOperationException>(() => frozen.SetStorageUpdates(TEST_UPDATES));
            Assert.Throws<InvalidOperationException>(() => frozen.AddStorageUpdate(TEST_UPDATES[0]));
        }

        public virtual void ValidateChecksumsWithNullHookIdDoesNotThrow()
        {
            var client = Client.ForTestnet();
            var tx = new HookStoreTransaction();

            // Should not throw when hookId is null
            tx.ValidateChecksums(client);
        }

        public virtual void ValidateChecksumsWithAccountHookIdValidatesAccountId()
        {
            var client = Client.ForTestnet();
            var accountId = AccountId.FromString("0.0.1234");
            var hookId = new HookId(new HookEntityId(accountId), 1);
            var tx = new HookStoreTransaction().SetHookId(hookId);

            // Should not throw with valid account ID
            tx.ValidateChecksums(client);
        }

        public virtual void ValidateChecksumsWithContractHookIdValidatesContractId()
        {
            var client = Client.ForTestnet();
            var contractId = ContractId.FromString("0.0.5678");
            var hookId = new HookId(new HookEntityId(contractId), 2);
            var tx = new HookStoreTransaction().SetHookId(hookId);

            // Should not throw with valid contract ID
            tx.ValidateChecksums(client);
        }

        public virtual void ValidateChecksumsWithInvalidAccountIdThrows()
        {
            var client = Client.ForTestnet();

            // Create an account ID with invalid checksum (using a known bad checksum from AccountIdTest)
            var accountId = AccountId.FromString("0.0.123-ntjli");
            var hookId = new HookId(new HookEntityId(accountId), 3);
            var tx = new HookStoreTransaction().SetHookId(hookId);

            // Should throw BadEntityIdException for invalid checksum
            Assert.Throws<BadEntityIdException>(() => tx.ValidateChecksums(client));
        }

        public virtual void ValidateChecksumsWithInvalidContractIdThrows()
        {
            var client = Client.ForTestnet();

            // Create a contract ID with invalid checksum (using a known bad checksum)
            var contractId = ContractId.FromString("0.0.123-ntjli");
            var hookId = new HookId(new HookEntityId(contractId), 4);
            var tx = new HookStoreTransaction().SetHookId(hookId);

            // Should throw BadEntityIdException for invalid checksum
            Assert.Throws<BadEntityIdException>(() => tx.ValidateChecksums(client));
        }
    }
}