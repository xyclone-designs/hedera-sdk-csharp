// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph;
using Java.Util;
using Java.Util.Concurrent;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class FeeEstimateQueryIntegrationTest
    {
        private static readonly long MIRROR_SYNC_DELAY_MILLIS = TimeUnit.SECONDS.ToMillis(2);
        private IntegrationTestEnv CreateFeeEstimateTestEnv()
        {
            var testEnv = new IntegrationTestEnv(1);
            if ("localhost".Equals(System.GetProperty("HEDERA_NETWORK")))
            {
                testEnv.Client.SetMirrorNetwork(List.Of("127.0.0.1:8084"));
            }

            return testEnv;
        }

        public virtual void TokenCreateTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {

                // Given: A TokenCreateTransaction is created
                var transaction = new TokenCreateTransaction().SetTokenName("Test Token").SetTokenSymbol("TEST")Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,.FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                WaitForMirrorNodeSync();

                // When: A fee estimate is requested
                FeeEstimateResponse response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.STATE).Execute(testEnv.Client);

                // Then: The response includes appropriate fees
                AssertFeeComponentsPresent(response);
                Assert.Equal(response.GetMode(), FeeEstimateMode.STATE);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TransferTransactionStateModeFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TransferTransaction().AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1)).FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.STATE).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                Assert.Equal(response.GetMode(), FeeEstimateMode.STATE);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TransferTransactionIntrinsicModeFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TransferTransaction().AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1)).FreezeWith(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.INTRINSIC).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                Assert.Equal(response.GetMode(), FeeEstimateMode.INTRINSIC);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TransferTransactionDefaultModeIsState()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TransferTransaction().AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(-1)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1)).FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                Assert.Equal(response.GetMode(), FeeEstimateMode.STATE);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TokenMintTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TokenMintTransaction().SetTokenId(TokenId.FromString("0.0.1234")).SetAmount(10).FreezeWith(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.INTRINSIC).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                Assert.NotNull(response.GetNodeFee().GetExtras());
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TopicCreateTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TopicCreateTransaction().SetTopicMemo("integration test topic").FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.STATE).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void ContractCreateTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new ContractCreateTransaction().SetBytecode(new byte[] { 1, 2, 3 }).SetGas(1000)AdminKey = testEnv.OperatorKey,.FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.STATE).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void FileCreateTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents("integration test file").FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.STATE).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void FileAppendTransactionFeeEstimateAggregatesChunks()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new FileAppendTransaction().SetFileId(FileId.FromString("0.0.1234")).SetContents(new byte[5000]).FreezeWith(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.INTRINSIC).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TopicMessageSubmitSingleChunkFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TopicMessageSubmitTransaction().SetTopicId(TopicId.FromString("0.0.1234")).SetMessage(new byte[128]).FreezeWith(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.INTRINSIC).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TopicMessageSubmitMultipleChunkFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TopicMessageSubmitTransaction().SetTopicId(TopicId.FromString("0.0.1234")).SetMessage(new byte[5000]).FreezeWith(testEnv.Client);
                WaitForMirrorNodeSync();
                var response = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.INTRINSIC).Execute(testEnv.Client);
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void MalformedTransactionReturnsInvalidArgumentError()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {

                // Given: A malformed transaction payload (invalid signed bytes)
                ByteString invalidBytes = ByteString.CopyFrom(new byte[] { 0x00, 0x01, 0x02, 0x03 });
                var malformedTransaction = Proto.Transaction.NewBuilder().SetSignedTransactionBytes(invalidBytes).Build();
                WaitForMirrorNodeSync();

                // When/Then: Executing the fee estimate query should throw INVALID_ARGUMENT
                AssertThatThrownBy(() => new FeeEstimateQuery().SetTransaction(malformedTransaction).SetMode(FeeEstimateMode.STATE).Execute(testEnv.Client)).IsInstanceOf(typeof(Exception)).HasMessageContaining("HTTP status");
            }
        }

        public virtual void QueryWithoutTransactionThrowsError()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                AssertThatThrownBy(() => new FeeEstimateQuery().SetMode(FeeEstimateMode.STATE).Execute(testEnv.Client)).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction must be set");
            }
        }

        public virtual void ActualFeesMatchEstimateWithinTolerance()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {

                // Create and freeze transaction
                var transaction = new TransferTransaction().AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(-1000)).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1000)).FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);

                // Get estimate
                var estimate = new FeeEstimateQuery().SetTransaction(transaction).SetMode(FeeEstimateMode.STATE).Execute(testEnv.Client);

                // Execute transaction
                var response = transaction.Execute(testEnv.Client);
                var receipt = response.GetReceipt(testEnv.Client);
                var record = response.GetRecord(testEnv.Client);
                long actualFee = record.transactionFee.ToTinybars();
                long estimatedFee = estimate.GetTotal();

                // Define tolerance (e.g., 20%)
                double tolerance = 0.2;
                long lowerBound = (long)(estimatedFee * (1 - tolerance));
                long upperBound = (long)(estimatedFee * (1 + tolerance));
                AssertThat(actualFee).As("Actual fee should be within ±20%% of estimate").IsBetween(lowerBound, upperBound);
            }
        }

        private static void WaitForMirrorNodeSync()
        {
            Thread.Sleep(MIRROR_SYNC_DELAY_MILLIS);
        }

        private static long Subtotal(FeeEstimate estimate)
        {
            return estimate.GetBase() + estimate.GetExtras().Stream().MapToLong(FeeExtra.GetSubtotal()).Sum();
        }

        private static void AssertFeeComponentsPresent(FeeEstimateResponse response)
        {

            // TODO adjust when NetworkService.getFeeEstimate has actual implementation
            Assert.NotNull(response);

            // Network fee validations
            Assert.NotNull(response.GetNetworkFee());
            AssertThat(response.GetNetworkFee().GetMultiplier()).IsGreaterThan(0);
            AssertThat(response.GetNetworkFee().GetSubtotal()).IsGreaterThanOrEqualTo(0);

            // Node fee validations
            Assert.NotNull(response.GetNodeFee());
            AssertThat(response.GetNodeFee().GetBase()).IsGreaterThanOrEqualTo(0);
            Assert.NotNull(response.GetNodeFee().GetExtras());

            // Service fee validations
            Assert.NotNull(response.GetServiceFee());
            AssertThat(response.GetServiceFee().GetBase()).IsGreaterThanOrEqualTo(0);
            Assert.NotNull(response.GetServiceFee().GetExtras());

            // Notes and total
            Assert.NotNull(response.GetNotes());
            AssertThat(response.GetTotal()).IsGreaterThan(0);
        }

        private static void AssertComponentTotalsConsistent(FeeEstimateResponse response)
        {

            // TODO adjust when NetworkService.getFeeEstimate has actual implementation
            var network = response.GetNetworkFee();
            var node = response.GetNodeFee();
            var service = response.GetServiceFee();
            var nodeSubtotal = Subtotal(node);
            var serviceSubtotal = Subtotal(service);
            Assert.Equal(network.GetSubtotal(), nodeSubtotal * network.GetMultiplier());
            Assert.Equal(response.GetTotal(), network.GetSubtotal() + nodeSubtotal + serviceSubtotal);
        }
    }
}