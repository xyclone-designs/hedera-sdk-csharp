// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Mockito.ArgumentMatchers;
using Org.Mockito.Mockito;
using Com.Hedera.Hashgraph.Sdk.Executable;
using Com.Hedera.Hashgraph.Sdk.Logger;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Grpc;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Concurrent.Atomic;
using Javax.Annotation;
using Org.Junit.Jupiter.Api;
using Org.Mockito;
using Org.Mockito.Stubbing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class ExecutableTest
    {
        Client client;
        Network network;
        Node node3, node4, node5;
        IList<AccountId> nodeAccountIds;
        virtual void Setup()
        {
            client = Client.ForMainnet();
            network = Mockito.Mock(typeof(Network));
            client.network = network;
            client.SetLogger(new Logger(LogLevel.WARN));
            node3 = Mockito.Mock(typeof(Node));
            node4 = Mockito.Mock(typeof(Node));
            node5 = Mockito.Mock(typeof(Node));
            When(node3.GetAccountId()).ThenReturn(new AccountId(0, 0, 3));
            When(node4.GetAccountId()).ThenReturn(new AccountId(0, 0, 4));
            When(node5.GetAccountId()).ThenReturn(new AccountId(0, 0, 5));
            When(network.GetNodeProxies(new AccountId(0, 0, 3))).ThenReturn(List.Of(node3));
            When(network.GetNodeProxies(new AccountId(0, 0, 4))).ThenReturn(List.Of(node4));
            When(network.GetNodeProxies(new AccountId(0, 0, 5))).ThenReturn(List.Of(node5));
            nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
        }

        virtual void FirstNodeHealthy()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            var tx = new DummyTransaction();
            tx.SetNodeAccountIds(nodeAccountIds);
            tx.SetNodesFromNodeAccountIds(client);
            tx.SetMinBackoff(Duration.OfMillis(10));
            tx.SetMaxBackoff(Duration.OfMillis(1000));
            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node3);
        }

        virtual void CalloptionsShouldRespectGrpcDeadline()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            var tx = new DummyTransaction();
            tx.SetNodeAccountIds(nodeAccountIds);
            tx.SetNodesFromNodeAccountIds(client);
            tx.SetMinBackoff(Duration.OfMillis(10));
            tx.SetMaxBackoff(Duration.OfMillis(1000));
            tx.SetGrpcDeadline(Duration.OfSeconds(10));
            var grpcRequest = tx.GetGrpcRequest(1);
            var timeRemaining = grpcRequest.GetCallOptions().GetDeadline().TimeRemaining(TimeUnit.MILLISECONDS);
            AssertThat(timeRemaining).IsLessThan(10000);
            AssertThat(timeRemaining).IsGreaterThan(9000);
        }

        virtual void ExecutableShouldUseGrpcDeadline()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            var tx = new DummyTransaction();
            tx.SetNodeAccountIds(nodeAccountIds);
            tx.SetNodesFromNodeAccountIds(client);
            tx.SetMinBackoff(Duration.OfMillis(10));
            tx.SetMaxBackoff(Duration.OfMillis(1000));
            tx.SetMaxAttempts(10);
            var timeout = Duration.OfSeconds(5);
            var currentTimeRemaining = new AtomicLong(timeout.ToMillis());
            long minimumRetryDelayMs = 100;
            long defaultDeadlineMs = timeout.ToMillis() - (minimumRetryDelayMs * (tx.GetMaxAttempts() / 2));

            // later on when the transaction is executed its grpc deadline should not be modified...
            tx.SetGrpcDeadline(Duration.OfMillis(defaultDeadlineMs));
            tx.blockingUnaryCall = (grpcRequest) =>
            {
                var grpc = (GrpcRequest)grpcRequest;
                var grpcTimeRemaining = grpc.GetCallOptions().GetDeadline().TimeRemaining(TimeUnit.MILLISECONDS);

                // the actual grpc deadline should be no larger than the smaller of the two values -
                // the default transaction level grpc deadline and the remaining timeout
                AssertThat(grpcTimeRemaining).IsLessThanOrEqualTo(defaultDeadlineMs);
                AssertThat(grpcTimeRemaining).IsLessThanOrEqualTo(currentTimeRemaining.Get());
                AssertThat(grpcTimeRemaining).IsGreaterThan(0);

                // transaction's grpc deadline should keep its original value
                Assert.Equal(tx.GrpcDeadline().ToMillis(), defaultDeadlineMs);
                currentTimeRemaining.Set(currentTimeRemaining.Get() - minimumRetryDelayMs);
                if (currentTimeRemaining.Get() > 0)
                {
                    try
                    {
                        Thread.Sleep(minimumRetryDelayMs);
                    }
                    catch (InterruptedException e)
                    {
                        throw new Exception(e);
                    }


                    // Status.UNAVAILABLE tells the Executable to retry the request
                    throw new StatusRuntimeException(io.grpc.Status.UNAVAILABLE);
                }

                throw new StatusRuntimeException(io.grpc.Status.ABORTED);
            };
            AssertThatExceptionOfType(typeof(MaxAttemptsExceededException)).IsThrownBy(() =>
            {
                tx.Execute(client, timeout);
            });
        }

        virtual void MultipleNodesUnhealthy()
        {
            When(node3.IsHealthy()).ThenReturn(false);
            When(node4.IsHealthy()).ThenReturn(true);
            When(node3.GetRemainingTimeForBackoff()).ThenReturn(1000);
            var tx = new DummyTransaction();
            tx.SetNodeAccountIds(nodeAccountIds);
            tx.SetNodesFromNodeAccountIds(client);
            tx.SetMinBackoff(Duration.OfMillis(10));
            tx.SetMaxBackoff(Duration.OfMillis(1000));
            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node4);
        }

        virtual void AllNodesUnhealthy()
        {
            When(node3.IsHealthy()).ThenReturn(false);
            When(node4.IsHealthy()).ThenReturn(false);
            When(node5.IsHealthy()).ThenReturn(false);
            When(node3.GetRemainingTimeForBackoff()).ThenReturn(4000);
            When(node4.GetRemainingTimeForBackoff()).ThenReturn(3000);
            When(node5.GetRemainingTimeForBackoff()).ThenReturn(5000);
            var tx = new DummyTransaction();
            tx.SetNodeAccountIds(nodeAccountIds);
            tx.SetNodesFromNodeAccountIds(client);
            tx.SetMinBackoff(Duration.OfMillis(10));
            tx.SetMaxBackoff(Duration.OfMillis(1000));
            tx.nodeAccountIds.SetIndex(1);
            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node4);
        }

        virtual void MultipleRequestsWithSingleHealthyNode()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            When(node4.IsHealthy()).ThenReturn(false);
            When(node5.IsHealthy()).ThenReturn(false);
            When(node4.GetRemainingTimeForBackoff()).ThenReturn(4000);
            When(node5.GetRemainingTimeForBackoff()).ThenReturn(3000);
            var tx = new DummyTransaction();
            tx.SetNodeAccountIds(nodeAccountIds);
            tx.SetNodesFromNodeAccountIds(client);
            tx.SetMinBackoff(Duration.OfMillis(10));
            tx.SetMaxBackoff(Duration.OfMillis(1000));
            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node3);
            tx.nodeAccountIds.Advance();
            tx.nodes.Advance();
            node = tx.GetNodeForExecute(2);
            Assert.Equal(node, node3);
            Verify(node4).GetRemainingTimeForBackoff();
            Verify(node5).GetRemainingTimeForBackoff();
        }

        virtual void MultipleRequestsWithNoHealthyNodes()
        {
            AtomicInteger i = new AtomicInteger();
            When(node3.IsHealthy()).ThenReturn(false);
            When(node4.IsHealthy()).ThenReturn(false);
            When(node5.IsHealthy()).ThenReturn(false);
            long[] node3Times = new[]
            {
                4000,
                3000,
                1000
            };
            long[] node4Times = new[]
            {
                3000,
                1000,
                4000
            };
            long[] node5Times = new[]
            {
                1000,
                3000,
                4000
            };
            When(node3.GetRemainingTimeForBackoff()).ThenAnswer((Answer<long>)(invocation) => node3Times[i.Get()]);
            When(node4.GetRemainingTimeForBackoff()).ThenAnswer((Answer<long>)(invocation) => node4Times[i.Get()]);
            When(node5.GetRemainingTimeForBackoff()).ThenAnswer((Answer<long>)(invocation) => node5Times[i.Get()]);
            var tx = new DummyTransaction();
            tx.SetNodeAccountIds(nodeAccountIds);
            tx.SetNodesFromNodeAccountIds(client);
            tx.SetMinBackoff(Duration.OfMillis(10));
            tx.SetMaxBackoff(Duration.OfMillis(1000));
            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node5);
            i.IncrementAndGet();
            node = tx.GetNodeForExecute(2);
            Assert.Equal(node, node4);
            i.IncrementAndGet();
            node = tx.GetNodeForExecute(3);
            Assert.Equal(node, node3);
        }

        virtual void SuccessfulExecute()
        {
            var now = java.time.Instant.Now();
            var tx = new AnonymousDummyTransaction(this);
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
            tx.SetNodeAccountIds(nodeAccountIds);
            var txResp = com.hedera.hashgraph.sdk.proto.TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.OK).Build();
            tx.blockingUnaryCall = (grpcRequest) => txResp;
            com.hedera.hashgraph.sdk.TransactionResponse resp = (com.hedera.hashgraph.sdk.TransactionResponse)tx.Execute(client);
            Assert.Equal(resp.nodeId, new AccountId(0, 0, 3));
            AssertThat(resp.GetValidateStatus()).IsTrue();
            AssertThat(resp.ToString()).IsNotNull();
        }

        private sealed class AnonymousDummyTransaction : DummyTransaction
        {
            public AnonymousDummyTransaction(ExecutableTest parent)
            {
                this.parent = parent;
            }

            private readonly ExecutableTest parent;
            TransactionResponse MapResponse(com.hedera.hashgraph.sdk.proto.TransactionResponse response, AccountId nodeId, com.hedera.hashgraph.sdk.proto.Transaction request)
            {
                return new TransactionResponse(new AccountId(0, 0, 3), TransactionId.WithValidStart(new AccountId(0, 0, 3), now), new byte[] { 1, 2, 3 }, null, null).SetValidateStatus(true);
            }
        }

        virtual void ExecuteWithChannelFailure()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            When(node4.IsHealthy()).ThenReturn(true);
            When(node3.ChannelFailedToConnect(Any(typeof(Instant)))).ThenReturn(true);
            When(node4.ChannelFailedToConnect(Any(typeof(Instant)))).ThenReturn(false);
            var now = java.time.Instant.Now();
            var tx = new AnonymousDummyTransaction1(this);
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
            tx.SetNodeAccountIds(nodeAccountIds);
            var txResp = com.hedera.hashgraph.sdk.proto.TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.OK).Build();
            tx.blockingUnaryCall = (grpcRequest) => txResp;
            com.hedera.hashgraph.sdk.TransactionResponse resp = (com.hedera.hashgraph.sdk.TransactionResponse)tx.Execute(client);
            Verify(node3).ChannelFailedToConnect(Any(typeof(Instant)));
            Verify(node4).ChannelFailedToConnect(Any(typeof(Instant)));
            Assert.Equal(resp.nodeId, new AccountId(0, 0, 4));
        }

        private sealed class AnonymousDummyTransaction1 : DummyTransaction
        {
            public AnonymousDummyTransaction1(ExecutableTest parent)
            {
                this.parent = parent;
            }

            private readonly ExecutableTest parent;
            TransactionResponse MapResponse(com.hedera.hashgraph.sdk.proto.TransactionResponse response, AccountId nodeId, com.hedera.hashgraph.sdk.proto.Transaction request)
            {
                return new TransactionResponse(new AccountId(0, 0, 4), TransactionId.WithValidStart(new AccountId(0, 0, 4), now), new byte[] { 1, 2, 3 }, null, null);
            }
        }

        virtual void ExecuteWithAllUnhealthyNodes()
        {
            AtomicInteger i = new AtomicInteger();

            // 1st round, pick node3, fail channel connect
            // 2nd round, pick node4, fail channel connect
            // 3rd round, pick node5, fail channel connect
            // 4th round, pick node 3, wait for delay, channel connect ok
            When(node3.IsHealthy()).ThenAnswer((Answer<bool>)(inv) => i.Get() == 0);
            When(node4.IsHealthy()).ThenAnswer((Answer<bool>)(inv) => i.Get() == 0);
            When(node5.IsHealthy()).ThenAnswer((Answer<bool>)(inv) => i.Get() == 0);
            When(node3.ChannelFailedToConnect(Any(typeof(Instant)))).ThenAnswer((Answer<bool>)(inv) => i.Get() == 0);
            When(node4.ChannelFailedToConnect(Any(typeof(Instant)))).ThenAnswer((Answer<bool>)(inv) => i.Get() == 0);
            When(node5.ChannelFailedToConnect(Any(typeof(Instant)))).ThenAnswer((Answer<bool>)(inv) => i.GetAndIncrement() == 0);
            When(node3.GetRemainingTimeForBackoff()).ThenReturn(500);
            When(node4.GetRemainingTimeForBackoff()).ThenReturn(600);
            When(node5.GetRemainingTimeForBackoff()).ThenReturn(700);
            var now = java.time.Instant.Now();
            var tx = new AnonymousDummyTransaction2(this);
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
            tx.SetNodeAccountIds(nodeAccountIds);
            var txResp = com.hedera.hashgraph.sdk.proto.TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.OK).Build();
            tx.blockingUnaryCall = (grpcRequest) => txResp;
            com.hedera.hashgraph.sdk.TransactionResponse resp = (com.hedera.hashgraph.sdk.TransactionResponse)tx.Execute(client);
            Verify(node3, Times(2)).ChannelFailedToConnect(Any(typeof(Instant)));
            Verify(node4).ChannelFailedToConnect(Any(typeof(Instant)));
            Verify(node5).ChannelFailedToConnect(Any(typeof(Instant)));
            Assert.Equal(resp.nodeId, new AccountId(0, 0, 3));
        }

        private sealed class AnonymousDummyTransaction2 : DummyTransaction
        {
            public AnonymousDummyTransaction2(ExecutableTest parent)
            {
                this.parent = parent;
            }

            private readonly ExecutableTest parent;
            TransactionResponse MapResponse(com.hedera.hashgraph.sdk.proto.TransactionResponse response, AccountId nodeId, com.hedera.hashgraph.sdk.proto.Transaction request)
            {
                return new TransactionResponse(new AccountId(0, 0, 3), TransactionId.WithValidStart(new AccountId(0, 0, 3), now), new byte[] { 1, 2, 3 }, null, null);
            }
        }

        virtual void ExecuteExhaustRetries()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            When(node4.IsHealthy()).ThenReturn(true);
            When(node5.IsHealthy()).ThenReturn(true);
            When(node3.ChannelFailedToConnect(Any(typeof(Instant)))).ThenReturn(true);
            When(node4.ChannelFailedToConnect(Any(typeof(Instant)))).ThenReturn(true);
            When(node5.ChannelFailedToConnect(Any(typeof(Instant)))).ThenReturn(true);
            var tx = new DummyTransaction();
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
            tx.SetNodeAccountIds(nodeAccountIds);
            AssertThatExceptionOfType(typeof(MaxAttemptsExceededException)).IsThrownBy(() => tx.Execute(client));
        }

        virtual void ExecuteRetriableErrorDuringCall()
        {
            AtomicInteger i = new AtomicInteger();
            When(node3.IsHealthy()).ThenReturn(true);
            When(node4.IsHealthy()).ThenReturn(true);
            When(node3.ChannelFailedToConnect(Any(typeof(Instant)))).ThenReturn(false);
            When(node4.ChannelFailedToConnect(Any(typeof(Instant)))).ThenReturn(false);
            var tx = new DummyTransaction();
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
            tx.SetNodeAccountIds(nodeAccountIds);
            tx.blockingUnaryCall = (grpcRequest) =>
            {
                if (i.GetAndIncrement() == 0)
                {
                    throw new StatusRuntimeException(io.grpc.Status.UNAVAILABLE);
                }
                else
                {
                    throw new StatusRuntimeException(io.grpc.Status.ABORTED);
                }
            };
            AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(() => tx.Execute(client));
            Verify(node3).ChannelFailedToConnect(Any(typeof(Instant)));
            Verify(node4).ChannelFailedToConnect(Any(typeof(Instant)));
        }

        virtual void TestChannelFailedToConnectTimeout()
        {
            TransactionResponse transactionResponse = new TransactionResponse(new AccountId(0, 0, 3), TransactionId.WithValidStart(new AccountId(0, 0, 3), java.time.Instant.Now()), new byte[] { 1, 2, 3 }, null, null);
            var tx = new DummyTransaction();
            tx.blockingUnaryCall = (grpcRequest) =>
            {
                throw new StatusRuntimeException(io.grpc.Status.UNAVAILABLE);
            };
            When(node3.IsHealthy()).ThenReturn(true);
            When(node3.ChannelFailedToConnect(Any(typeof(Instant)))).ThenReturn(true);
            AssertThatExceptionOfType(typeof(MaxAttemptsExceededException)).IsThrownBy(() => transactionResponse.GetReceipt(client, Duration.OfSeconds(2)));
        }

        virtual void ExecuteQueryDelay()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            When(node4.IsHealthy()).ThenReturn(true);
            When(node3.ChannelFailedToConnect()).ThenReturn(false);
            When(node4.ChannelFailedToConnect()).ThenReturn(false);
            AtomicInteger i = new AtomicInteger();
            var tx = new AnonymousDummyQuery(this);
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
            tx.SetNodeAccountIds(nodeAccountIds);
            var receipt = com.hedera.hashgraph.sdk.proto.TransactionReceipt.NewBuilder().SetStatus(ResponseCodeEnum.OK).Build();
            var receiptResp = com.hedera.hashgraph.sdk.proto.TransactionGetReceiptResponse.NewBuilder().SetReceipt(receipt).Build();
            var resp = Response.NewBuilder().SetTransactionGetReceipt(receiptResp).Build();
            tx.blockingUnaryCall = (grpcRequest) => resp;
            tx.Execute(client);

            // RETRY case doesn't advance to next node, so it checks the same node twice: once for first attempt, once for
            // retry attempt
            Verify(node3, Times(2)).ChannelFailedToConnect(Any(typeof(Instant)));
        }

        private sealed class AnonymousDummyQuery : DummyQuery
        {
            public AnonymousDummyQuery(ExecutableTest parent)
            {
                this.parent = parent;
            }

            private readonly ExecutableTest parent;
            Status MapResponseStatus(com.hedera.hashgraph.sdk.proto.Response response)
            {
                return Status.RECEIPT_NOT_FOUND;
            }

            ExecutionState GetExecutionState(Status status, Response response)
            {
                return i.GetAndIncrement() == 0 ? ExecutionState.RETRY : ExecutionState.SUCCESS;
            }
        }

        virtual void ExecuteUserError()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            When(node3.ChannelFailedToConnect()).ThenReturn(false);
            var tx = new AnonymousDummyTransaction3(this);
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
            tx.SetNodeAccountIds(nodeAccountIds);
            var txResp = com.hedera.hashgraph.sdk.proto.TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.ACCOUNT_DELETED).Build();
            tx.blockingUnaryCall = (grpcRequest) => txResp;
            AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() => tx.Execute(client));
            Verify(node3).ChannelFailedToConnect(Any(typeof(Instant)));
        }

        private sealed class AnonymousDummyTransaction3 : DummyTransaction
        {
            public AnonymousDummyTransaction3(ExecutableTest parent)
            {
                this.parent = parent;
            }

            private readonly ExecutableTest parent;
            Status MapResponseStatus(com.hedera.hashgraph.sdk.proto.TransactionResponse response)
            {
                return Status.ACCOUNT_DELETED;
            }
        }

        virtual void ShouldRetryReturnsCorrectStates()
        {
            var tx = new DummyTransaction();
            Assert.Equal(tx.GetExecutionState(Status.PLATFORM_TRANSACTION_NOT_CREATED, null), ExecutionState.SERVER_ERROR);
            Assert.Equal(tx.GetExecutionState(Status.PLATFORM_NOT_ACTIVE, null), ExecutionState.SERVER_ERROR);
            Assert.Equal(tx.GetExecutionState(Status.BUSY, null), ExecutionState.RETRY);
            Assert.Equal(tx.GetExecutionState(Status.INVALID_NODE_ACCOUNT, null), ExecutionState.RETRY);
            Assert.Equal(tx.GetExecutionState(Status.OK, null), ExecutionState.SUCCESS);
            Assert.Equal(tx.GetExecutionState(Status.ACCOUNT_DELETED, null), ExecutionState.REQUEST_ERROR);
        }

        virtual void ShouldSetMaxRetry()
        {
            var tx = new DummyTransaction();
            tx.SetMaxRetry(1);
            Assert.Equal(tx.GetMaxRetry(), 1);
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => tx.SetMaxRetry(0));
        }

        virtual void ShouldMarkNodeAsUnusableOnInvalidNodeAccountId()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            When(node3.ChannelFailedToConnect()).ThenReturn(false);
            When(node4.IsHealthy()).ThenReturn(true);
            When(node4.ChannelFailedToConnect()).ThenReturn(false);
            When(node5.IsHealthy()).ThenReturn(true);
            When(node5.ChannelFailedToConnect()).ThenReturn(false);
            var tx = new AnonymousDummyTransaction4(this);
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5));
            tx.SetNodeAccountIds(nodeAccountIds);
            var txResp = com.hedera.hashgraph.sdk.proto.TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.INVALID_NODE_ACCOUNT).Build();
            tx.blockingUnaryCall = (grpcRequest) => txResp;

            // INVALID_NODE_ACCOUNT maps to RETRY, so it retries on the same node (doesn't advance)
            AssertThatExceptionOfType(typeof(MaxAttemptsExceededException)).IsThrownBy(() => tx.Execute(client));

            // Verify that increaseBackoff was called on the network for the node that returned INVALID_NODE_ACCOUNT
            Verify(network, AtLeastOnce()).IncreaseBackoff(Any(typeof(Node)));
        }

        private sealed class AnonymousDummyTransaction4 : DummyTransaction
        {
            public AnonymousDummyTransaction4(ExecutableTest parent)
            {
                this.parent = parent;
            }

            private readonly ExecutableTest parent;
            Status MapResponseStatus(com.hedera.hashgraph.sdk.proto.TransactionResponse response)
            {
                return Status.INVALID_NODE_ACCOUNT;
            }
        }

        virtual void ShouldTriggerAddressBookUpdateOnInvalidNodeAccountId()
        {
            When(node3.IsHealthy()).ThenReturn(true);
            When(node3.ChannelFailedToConnect()).ThenReturn(false);
            var tx = new AnonymousDummyTransaction5(this);
            var nodeAccountIds = Arrays.AsList(new AccountId(0, 0, 3));
            tx.SetNodeAccountIds(nodeAccountIds);
            var txResp = com.hedera.hashgraph.sdk.proto.TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.INVALID_NODE_ACCOUNT).Build();
            tx.blockingUnaryCall = (grpcRequest) => txResp;

            // This should trigger address book update due to INVALID_NODE_ACCOUNT
            AssertThatExceptionOfType(typeof(MaxAttemptsExceededException)).IsThrownBy(() => tx.Execute(client));

            // Verify that increaseBackoff was called (node marking)
            Verify(network, AtLeastOnce()).IncreaseBackoff(Any(typeof(Node))); // Note: We can't easily test the address book update in this unit test since it's async
            // and involves network calls. The integration test would be more appropriate for that.
        }

        private sealed class AnonymousDummyTransaction5 : DummyTransaction
        {
            public AnonymousDummyTransaction5(ExecutableTest parent)
            {
                this.parent = parent;
            }

            private readonly ExecutableTest parent;
            Status MapResponseStatus(com.hedera.hashgraph.sdk.proto.TransactionResponse response)
            {
                return Status.INVALID_NODE_ACCOUNT;
            }
        }

        class DummyTransaction<T> : Executable<T, com.hedera.hashgraph.sdk.proto.Transaction, com.hedera.hashgraph.sdk.proto.TransactionResponse, TransactionResponse> where T : Transaction<T>
        {
            override void OnExecute(Client client)
            {
            }

            override CompletableFuture<Void> OnExecuteAsync(Client client)
            {
                return null;
            }

            override com.hedera.hashgraph.sdk.proto.Transaction MakeRequest()
            {
                return null;
            }

            override TransactionResponse MapResponse(com.hedera.hashgraph.sdk.proto.TransactionResponse response, AccountId nodeId, com.hedera.hashgraph.sdk.proto.Transaction request)
            {
                return null;
            }

            override Status MapResponseStatus(com.hedera.hashgraph.sdk.proto.TransactionResponse response)
            {
                return Status.OK;
            }

            override MethodDescriptor<com.hedera.hashgraph.sdk.proto.Transaction, com.hedera.hashgraph.sdk.proto.TransactionResponse> GetMethodDescriptor()
            {
                return null;
            }

            override TransactionId GetTransactionIdInternal()
            {
                return null;
            }
        }

        class DummyQuery : Query<TransactionReceipt, TransactionReceiptQuery>
        {
            override void OnExecute(Client client)
            {
            }

            override TransactionReceipt MapResponse(Response response, AccountId nodeId, com.hedera.hashgraph.sdk.proto.Query request)
            {
                return null;
            }

            override Status MapResponseStatus(com.hedera.hashgraph.sdk.proto.Response response)
            {
                return Status.OK;
            }

            override MethodDescriptor<com.hedera.hashgraph.sdk.proto.Query, Response> GetMethodDescriptor()
            {
                return null;
            }

            override void OnMakeRequest(com.hedera.hashgraph.sdk.proto.Query.Builder queryBuilder, QueryHeader header)
            {
            }

            override ResponseHeader MapResponseHeader(Response response)
            {
                return null;
            }

            override QueryHeader MapRequestHeader(com.hedera.hashgraph.sdk.proto.Query request)
            {
                return null;
            }

            override void ValidateChecksums(Client client)
            {
            }
        }
    }
}