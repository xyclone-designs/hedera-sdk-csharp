// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;
using System.Text;
using System.Threading;

using Google.Protobuf;

using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Contract;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class FeeEstimateQueryIntegrationTest
    {
        private static readonly long MIRROR_SYNC_DELAY_MILLIS = (long)TimeSpan.FromSeconds(2).TotalMilliseconds;
        
        private IntegrationTestEnv CreateFeeEstimateTestEnv()
        {
            var testEnv = new IntegrationTestEnv(1);
            if ("localhost".Equals(System.Property.Get("HEDERA_NETWORK")))
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
                var transaction = new TokenCreateTransaction
                {
					TokenName = "Test Token",
					TokenSymbol = "TEST",
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
				}.FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);

                
                WaitForMirrorNodeSync();


                // When: A fee estimate is requested
                FeeEstimateResponse response = new FeeEstimateQuery
				{
					Mode = FeeEstimateMode.State,
				}
				.SetTransaction(transaction)
				.Execute(testEnv.Client);

                // Then: The response includes appropriate fees
                
                AssertFeeComponentsPresent(response);
                
                Assert.Equal(response.Mode, FeeEstimateMode.State);

                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TransferTransactionStateModeFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(-1))
                    .AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1)).FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                
                WaitForMirrorNodeSync();

                var response = new FeeEstimateQuery
                {
					Mode = FeeEstimateMode.State,
                }
                .SetTransaction(transaction)
                .Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                
                Assert.Equal(response.Mode, FeeEstimateMode.State);

                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TransferTransactionIntrinsicModeFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(-1))
                    .AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1)).FreezeWith(testEnv.Client);
                
                WaitForMirrorNodeSync();

                var response = new FeeEstimateQuery
                {
					Mode = FeeEstimateMode.Intrinsic,
				}
				.SetTransaction(transaction)
                .Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                
                Assert.Equal(response.Mode, FeeEstimateMode.Intrinsic);

                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TransferTransactionDefaultModeIsState()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(-1))
                    .AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1)).FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                
                WaitForMirrorNodeSync();

                var response = new FeeEstimateQuery().SetTransaction(transaction).Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                
                Assert.Equal(response.Mode, FeeEstimateMode.State);

                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TokenMintTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TokenMintTransaction
                {
					TokenId = TokenId.FromString("0.0.1234"),
					Amount = 10
				
                }.FreezeWith(testEnv.Client);
                
                WaitForMirrorNodeSync();

                var response = new FeeEstimateQuery
                {
					Mode = FeeEstimateMode.Intrinsic,
				}
				.SetTransaction(transaction)
                .Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                
                Assert.NotNull(response.NodeFee.Extras);

                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TopicCreateTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TopicCreateTransaction
                {
					TopicMemo = "integration test topic"
				
                }.FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                
                WaitForMirrorNodeSync();

                var response = new FeeEstimateQuery
				{
					Mode = FeeEstimateMode.State,
				}
				.SetTransaction(transaction)
                .Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void ContractCreateTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new ContractCreateTransaction
                {
					Bytecode = new byte[] { 1, 2, 3 }, 
                    Gas = 1000, 
                    AdminKey = testEnv.OperatorKey,
				
                }.FreezeWith(testEnv.Client).SignWithOperator(testEnv.Client);
                
                WaitForMirrorNodeSync();

                var response = new FeeEstimateQuery
                {
					Mode = FeeEstimateMode.State,
				}
				.SetTransaction(transaction)
                .Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void FileCreateTransactionFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes("integration test file")
				}
                .FreezeWith(testEnv.Client)
                .SignWithOperator(testEnv.Client);

                
                WaitForMirrorNodeSync();


                var response = new FeeEstimateQuery
                {
					Mode = FeeEstimateMode.State,
				
                }.SetTransaction(transaction).Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void FileAppendTransactionFeeEstimateAggregatesChunks()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new FileAppendTransaction
                {
					FileId = FileId.FromString("0.0.1234"),
					Contents = ByteString.CopyFrom(new byte[5000])
                
                }.FreezeWith(testEnv.Client);

                
                WaitForMirrorNodeSync();


                var response = new FeeEstimateQuery
				{
					Mode = FeeEstimateMode.Intrinsic,
				}
				.SetTransaction(transaction)
				.Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TopicMessageSubmitSingleChunkFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TopicMessageSubmitTransaction
                {
					TopicId = TopicId.FromString("0.0.1234"),
					Message = ByteString.CopyFrom(new byte[128])
				
                }.FreezeWith(testEnv.Client);

                
                WaitForMirrorNodeSync();


                var response = new FeeEstimateQuery
				{
					Mode = FeeEstimateMode.Intrinsic,
				}
				.SetTransaction(transaction)
				.Execute(testEnv.Client);
                
                AssertFeeComponentsPresent(response);
                AssertComponentTotalsConsistent(response);
            }
        }

        public virtual void TopicMessageSubmitMultipleChunkFeeEstimate()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                var transaction = new TopicMessageSubmitTransaction
                {
					TopicId = TopicId.FromString("0.0.1234"),
					Message = ByteString.CopyFrom(new byte[5000])
				
                }.FreezeWith(testEnv.Client);
                
                
                WaitForMirrorNodeSync();


                var response = new FeeEstimateQuery
				{
					Mode = FeeEstimateMode.Intrinsic,
				}
				.SetTransaction(transaction)
				.Execute(testEnv.Client);
                
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
                var malformedTransaction = new Proto.Transaction { SignedTransactionBytes = invalidBytes };
                
                WaitForMirrorNodeSync();

                // When/Then: Executing the fee estimate query should throw INVALID_ARGUMENT
                Exception exception = Assert.Throws<Exception>(() => new FeeEstimateQuery
                {
                    Transaction = malformedTransaction,
                    Mode = FeeEstimateMode.State,

                }.Execute(testEnv.Client));
                
                Assert.Contains("HTTP status", exception.Message);
            }
        }

        public virtual void QueryWithoutTransactionThrowsError()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
				InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new FeeEstimateQuery
                {
                    Mode = FeeEstimateMode.State

                }.Execute(testEnv.Client));

				Assert.Contains("transaction must be set", exception.Message);
            }
        }

        public virtual void ActualFeesMatchEstimateWithinTolerance()
        {
            using (var testEnv = CreateFeeEstimateTestEnv())
            {
                // Create and freeze transaction
                var transaction = new TransferTransaction()
                    .AddHbarTransfer(testEnv.OperatorId, Hbar.FromTinybars(-1000))
                    .AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1000))
                    .FreezeWith(testEnv.Client)
                    .SignWithOperator(testEnv.Client);

                // Get estimate
                var estimate = new FeeEstimateQuery
                {
					Mode = FeeEstimateMode.State,

				}.SetTransaction(transaction).Execute(testEnv.Client);

                // Execute transaction
                var response = transaction.Execute(testEnv.Client);
                var receipt = response.GetReceipt(testEnv.Client);
                var record = response.GetRecord(testEnv.Client);
                long actualFee = record.TransactionFee.ToTinybars();
                long estimatedFee = estimate.Total;

                // Define tolerance (e.g., 20%)
                double tolerance = 0.2;
                long lowerBound = (long)(estimatedFee * (1 - tolerance));
                long upperBound = (long)(estimatedFee * (1 + tolerance));

                Assert.True(actualFee > lowerBound && actualFee < upperBound, "Actual fee should be within ±20%% of estimate");
            }
        }

        private static void WaitForMirrorNodeSync()
        {
            Thread.Sleep((int)MIRROR_SYNC_DELAY_MILLIS);
        }

        private static long Subtotal(FeeEstimate estimate)
        {
            return estimate.Base + estimate.Extras.Sum(_ => _.Subtotal);
        }

        private static void AssertFeeComponentsPresent(FeeEstimateResponse response)
        {
            // TODO adjust when NetworkService.getFeeEstimate has actual implementation
            Assert.NotNull(response);

            // Network fee validations
            Assert.NotNull(response.NetworkFee);
            Assert.True(response.NetworkFee.Multiplier > 0);
            Assert.True(response.NetworkFee.Subtotal >= 0);

            // Node fee validations
            Assert.NotNull(response.NodeFee);
            Assert.True(response.NodeFee.Base >= 0);
            Assert.NotNull(response.NodeFee.Extras);

            // Service fee validations
            Assert.NotNull(response.ServiceFee);
            Assert.True(response.ServiceFee.Base >= 0);
            Assert.NotNull(response.ServiceFee.Extras);

            // Notes and total
            Assert.NotNull(response.Notes);
            Assert.True(response.Total > 0);
        }

        private static void AssertComponentTotalsConsistent(FeeEstimateResponse response)
        {
            // TODO adjust when NetworkService.getFeeEstimate has actual implementation
            var network = response.NetworkFee;
            var node = response.NodeFee;
            var service = response.ServiceFee;
            var nodeSubtotal = Subtotal(node);
            var serviceSubtotal = Subtotal(service);
            Assert.Equal(network.Subtotal, nodeSubtotal * network.Multiplier);
            Assert.Equal(response.Total, network.Subtotal + nodeSubtotal + serviceSubtotal);
        }
    }
}