// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Grpc.Core;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions;

using Moq;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.Tests.SDK
{
    class ExecutableTest
    {
        Client client;
        Network network;
        Node node3, node4, node5;
        IList<AccountId> nodeAccountIds;

        // Keep references to mocks
        Mock<Network> networkMock;
        Mock<Node> node3Mock, node4Mock, node5Mock;

        public virtual void Setup()
        {
            networkMock = new Mock<Network>();

            client = Client.ForMainnet(c =>
            {
                c.Network_ = network = networkMock.Object;
            });

            node3Mock = new Mock<Node>();
            node4Mock = new Mock<Node>();
            node5Mock = new Mock<Node>();

            node3 = node3Mock.Object;
            node4 = node4Mock.Object;
            node5 = node5Mock.Object;

            // Property setups
            node3Mock.Setup(n => n.AccountId).Returns(new AccountId(0, 0, 3));
            node4Mock.Setup(n => n.AccountId).Returns(new AccountId(0, 0, 4));
            node5Mock.Setup(n => n.AccountId).Returns(new AccountId(0, 0, 5));

            // Method setups
            networkMock.Setup(n => n.GetNodeProxies(new AccountId(0, 0, 3))).Returns(new[] { node3 });
            networkMock.Setup(n => n.GetNodeProxies(new AccountId(0, 0, 4))).Returns(new[] { node4 });
            networkMock.Setup(n => n.GetNodeProxies(new AccountId(0, 0, 5))).Returns(new[] { node5 });

            nodeAccountIds = new List<AccountId>
            {
                new AccountId(0, 0, 3),
                new AccountId(0, 0, 4),
                new AccountId(0, 0, 5)
            };
        }
        
        [Fact]
        public virtual void FirstNodeHealthy()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [.. nodeAccountIds],
                MinBackoff = TimeSpan.FromMilliseconds(10),
                MaxBackoff = TimeSpan.FromMilliseconds(1000),
            };

            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node3);
        }
        
        [Fact]
        public virtual void CalloptionsShouldRespectGrpcDeadline()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [.. nodeAccountIds],
                MinBackoff = TimeSpan.FromMilliseconds(10),
                MaxBackoff = TimeSpan.FromMilliseconds(1000),
                GrpcDeadline = TimeSpan.FromSeconds(10),
            };

            var grpcRequest = tx.GetGrpcRequest(1);
            var timeRemaining = (DateTime.UtcNow - grpcRequest.CallOptions.Deadline)?.TotalMilliseconds;
            Assert.True(timeRemaining < 10000);
            Assert.True(timeRemaining > 9000);
        }
        
        [Fact]
        public virtual void ExecutableShouldUseGrpcDeadline()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [.. nodeAccountIds],
                MinBackoff = TimeSpan.FromMilliseconds(10),
                MaxBackoff = TimeSpan.FromMilliseconds(1000),
                MaxAttempts = 10,
            };

            var timeout = TimeSpan.FromSeconds(5);
            long currentTimeRemaining = (long)timeout.TotalMilliseconds;
            long minimumRetryDelayMs = 100;
            long defaultDeadlineMs = (long)(timeout.TotalMilliseconds - (minimumRetryDelayMs * (tx.MaxAttempts / 2)));

            // later on when the transaction is executed its grpc deadline should not be modified...
            tx.GrpcDeadline = TimeSpan.FromMilliseconds(defaultDeadlineMs);
            tx.BlockingUnaryCall = (grpcRequest) =>
            {
                var grpcTimeRemaining = (DateTime.UtcNow - grpcRequest.CallOptions.Deadline)?.TotalMilliseconds;

                // the actual grpc deadline should be no larger than the smaller of the two values -
                // the default transaction level grpc deadline and the remaining timeout
                Assert.True(grpcTimeRemaining <= defaultDeadlineMs);
                Assert.True(grpcTimeRemaining <= Volatile.Read(ref currentTimeRemaining));
                Assert.True(grpcTimeRemaining > 0);

                // transaction's grpc deadline should keep its original value
                Assert.Equal(tx.GrpcDeadline.TotalMilliseconds, defaultDeadlineMs);
                Volatile.Write(ref currentTimeRemaining, Volatile.Read(ref currentTimeRemaining) - minimumRetryDelayMs);
                if (Volatile.Read(ref currentTimeRemaining) > 0)
                {
                    try
                    {
                        Thread.Sleep((int)minimumRetryDelayMs);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        throw new Exception(e.Message, e);
                    }

                    // StatusCode.Unavailable tells the Executable to retry the request
                    throw new RpcException(new Status(StatusCode.Unavailable, string.Empty));
                }

                throw new RpcException(new Status(StatusCode.Aborted, string.Empty));
            };
            MaxAttemptsExceededException exception = Assert.Throws<MaxAttemptsExceededException>(() =>
            {
                tx.Execute(client, timeout);
            });
        }
        
        [Fact]
        public virtual void MultipleNodesUnhealthy()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(false);
            node4Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(1000);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [.. nodeAccountIds],
                MinBackoff = TimeSpan.FromMilliseconds(10),
                MaxBackoff = TimeSpan.FromMilliseconds(1000),
            };
            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node4);
        }
        
        [Fact]
        public virtual void AllNodesUnhealthy()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(false);
            node4Mock.Setup(n => n.IsHealthy()).Returns(false);
            node5Mock.Setup(n => n.IsHealthy()).Returns(false);
            node3Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(4000);
            node4Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(3000);
            node5Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(5000);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [.. nodeAccountIds],
                MinBackoff = TimeSpan.FromMilliseconds(10),
                MaxBackoff = TimeSpan.FromMilliseconds(1000),
            };

            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node4);
        }
        
        [Fact]
        public virtual void MultipleRequestsWithSingleHealthyNode()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node4Mock.Setup(n => n.IsHealthy()).Returns(false);
            node5Mock.Setup(n => n.IsHealthy()).Returns(false);
            node4Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(4000);
            node5Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(3000);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [.. nodeAccountIds],
                MinBackoff = TimeSpan.FromMilliseconds(10),
                MaxBackoff = TimeSpan.FromMilliseconds(1000),
            };
            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node3);
            tx.NodeAccountIds.Advance();
            node = tx.GetNodeForExecute(2);
            Assert.Equal(node, node3);
            node4Mock.Verify(n => n.GetRemainingTimeForBackoff());
            node5Mock.Verify(n => n.GetRemainingTimeForBackoff());
        }
        
        [Fact]
        public virtual void MultipleRequestsWithNoHealthyNodes()
        {
            int i = 0;
            node3Mock.Setup(n => n.IsHealthy()).Returns(false);
            node4Mock.Setup(n => n.IsHealthy()).Returns(false);
            node5Mock.Setup(n => n.IsHealthy()).Returns(false);

            long[] node3Times = new long[] { 4000, 3000, 1000 };
            long[] node4Times = new long[] { 3000, 1000, 4000 };
            long[] node5Times = new long[] { 1000, 3000, 4000 };

            node3Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(() => node3Times[Volatile.Read(ref i)]);
            node4Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(() => node4Times[Volatile.Read(ref i)]);
            node5Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(() => node5Times[Volatile.Read(ref i)]);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [.. nodeAccountIds],
                MinBackoff = TimeSpan.FromMilliseconds(10),
                MaxBackoff = TimeSpan.FromMilliseconds(1000),
            };
            var node = tx.GetNodeForExecute(1);
            Assert.Equal(node, node5);
            Interlocked.Increment(ref i);
            node = tx.GetNodeForExecute(2);
            Assert.Equal(node, node4);
            Interlocked.Increment(ref i);
            node = tx.GetNodeForExecute(3);
            Assert.Equal(node, node3);
        }

        [Fact]
        public virtual void SuccessfulExecute()
        {
            var now = DateTimeOffset.UtcNow;
            var tx = new AnonymousDummyTransaction(now)
            {
                NodeAccountIds = [new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5)]
            };

            var txResp = new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.Ok };
            tx.BlockingUnaryCall = (grpcRequest) => txResp;

            TransactionResponse resp = (TransactionResponse)tx.Execute(client);
            Assert.Equal(resp.NodeId, new AccountId(0, 0, 3));
            Assert.True(resp.ValidateStatus);
            Assert.NotNull(resp.ToString());
        }

        private sealed class AnonymousDummyTransaction : DummyTransaction
        {
            private readonly DateTimeOffset _now;

            public AnonymousDummyTransaction(DateTimeOffset now)
            {
                _now = now;
            }

            public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
            {
                return new TransactionResponse(new AccountId(0, 0, 3), TransactionId.WithValidStart(new AccountId(0, 0, 3), _now), new byte[] { 1, 2, 3 }, null, null)
                {
                    ValidateStatus = true
                };
            }
        }

        [Fact]
        public virtual void ExecuteWithChannelFailure()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node4Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(true);
            node4Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);

            var now = DateTimeOffset.UtcNow;
            var tx = new AnonymousDummyTransaction1(now)
            {
                NodeAccountIds = [new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5)]
            };
            var txResp = new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.Ok };
            tx.BlockingUnaryCall = (grpcRequest) => txResp;

            TransactionResponse resp = (TransactionResponse)tx.Execute(client);
            node3Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()));
            node4Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()));

            Assert.Equal(resp.NodeId, new AccountId(0, 0, 4));
        }

        private sealed class AnonymousDummyTransaction1 : DummyTransaction
        {
            private readonly DateTimeOffset _now;

            public AnonymousDummyTransaction1(DateTimeOffset now)
            {
                _now = now;
            }

            public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
            {
                return new TransactionResponse(new AccountId(0, 0, 4), TransactionId.WithValidStart(new AccountId(0, 0, 4), _now), new byte[] { 1, 2, 3 }, null, null);
            }
        }

        [Fact]
        public virtual void ExecuteWithAllUnhealthyNodes()
        {
            int i = 0;

            // 1st round, pick node3, fail channel connect
            // 2nd round, pick node4, fail channel connect
            // 3rd round, pick node5, fail channel connect
            // 4th round, pick node3, wait for delay, channel connect ok
            node3Mock.Setup(n => n.IsHealthy()).Returns(() => i == 0);
            node4Mock.Setup(n => n.IsHealthy()).Returns(() => i == 0);
            node5Mock.Setup(n => n.IsHealthy()).Returns(() => i == 0);

            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(() => i == 0);
            node4Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(() => i == 0);
            node5Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(() => i++ == 0);

            node3Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(500);
            node4Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(600);
            node5Mock.Setup(n => n.GetRemainingTimeForBackoff()).Returns(700);

            var now = DateTimeOffset.UtcNow;
            var tx = new AnonymousDummyTransaction2(now)
            {
                NodeAccountIds = [new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5)]
            };
            var txResp = new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.Ok };
            tx.BlockingUnaryCall = (grpcRequest) => txResp;

            TransactionResponse resp = (TransactionResponse)tx.Execute(client);

            node3Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()), Times.Exactly(2));
            node4Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()));
            node5Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()));

            Assert.Equal(resp.NodeId, new AccountId(0, 0, 3));
        }

        private sealed class AnonymousDummyTransaction2 : DummyTransaction
        {
            private readonly DateTimeOffset _now;

            public AnonymousDummyTransaction2(DateTimeOffset now)
            {
                _now = now;
            }

            public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
            {
                return new TransactionResponse(new AccountId(0, 0, 3), TransactionId.WithValidStart(new AccountId(0, 0, 3), _now), new byte[] { 1, 2, 3 }, null, null);
            }
        }

        [Fact]
        public virtual void ExecuteExhaustRetries()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node4Mock.Setup(n => n.IsHealthy()).Returns(true);
            node5Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(true);
            node4Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(true);
            node5Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(true);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5)]
            };

            MaxAttemptsExceededException exception = Assert.Throws<MaxAttemptsExceededException>(() => { tx.Execute(client); });
        }

        [Fact]
        public virtual void ExecuteRetriableErrorDuringCall()
        {
            int i = 0;

            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node4Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);
            node4Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);

            var tx = new DummyTransaction
            {
                NodeAccountIds = [new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5)],
            };

            tx.BlockingUnaryCall = (grpcRequest) =>
            {
                if (Interlocked.Increment(ref i) == 1)
                {
                    throw new RpcException(new Status(StatusCode.Unavailable, string.Empty));
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.Aborted, string.Empty));
                }
            };
            Exception exception = Assert.Throws<Exception>(() => { tx.Execute(client); });
            node3Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()));
            node4Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()));
        }

        [Fact]
        public virtual void TestChannelFailedToConnectTimeout()
        {
            TransactionResponse transactionResponse = new(new AccountId(0, 0, 3), TransactionId.WithValidStart(new AccountId(0, 0, 3), DateTimeOffset.UtcNow), new byte[] { 1, 2, 3 }, null, null);
            var tx = new DummyTransaction();
            tx.BlockingUnaryCall = (grpcRequest) =>
            {
                throw new RpcException(new Status(StatusCode.Unavailable, string.Empty));
            };
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(true);
            MaxAttemptsExceededException exception = Assert.Throws<MaxAttemptsExceededException>(() => transactionResponse.GetReceipt(client, TimeSpan.FromSeconds(2)));
        }

        [Fact]
        public virtual void ExecuteQueryDelay()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node4Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);
            node4Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);

            int i = 0;
            var tx = new AnonymousDummyQuery(i)
            {
                NodeAccountIds = [new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5)]
            };

            var receipt = new Proto.TransactionReceipt { Status = Proto.ResponseCodeEnum.Ok };
            var receiptResp = new Proto.TransactionGetReceiptResponse { Receipt = receipt };
            var resp = new Proto.Response { TransactionGetReceipt = receiptResp };
            tx.BlockingUnaryCall = (grpcRequest) => resp;
            tx.Execute(client);

            // RETRY case doesn't advance to next node, so it checks the same node twice: once for first attempt, once for
            // retry attempt
            node3Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()), Times.Exactly(2));
        }

        private sealed class AnonymousDummyQuery : DummyQuery
        {
            private int _i;

            public AnonymousDummyQuery(int i)
            {
                _i = i;
            }

            public override ResponseStatus MapResponseStatus(Proto.Response response)
            {
                return ResponseStatus.ReceiptNotFound;
            }
            public override ExecutionState GetExecutionState(ResponseStatus status, Proto.Response response)
            {
                return Interlocked.Increment(ref _i) == 1 ? ExecutionState.Retry : ExecutionState.Success;
            }
        }

        public virtual void ExecuteUserError()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);
            var tx = new AnonymousDummyTransaction3
            {
                NodeAccountIds = [new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5)]
            };

            var txResp = new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.AccountDeleted };
            tx.BlockingUnaryCall = (grpcRequest) => txResp;
            PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() => tx.Execute(client));

            node3Mock.Verify(n => n.ChannelFailedToConnect(It.IsAny<DateTime>()));
        }

        private sealed class AnonymousDummyTransaction3 : DummyTransaction
        {
            public override ResponseStatus MapResponseStatus(Proto.Response response)
            {
                return ResponseStatus.AccountDeleted;
            }
        }

        [Fact]
        public virtual void ShouldRetryReturnsCorrectStates()
        {
            var tx = new DummyTransaction();

            Assert.Equal(tx.GetExecutionState(ResponseStatus.PlatformTransactionNotCreated, null), ExecutionState.ServerError);
            Assert.Equal(tx.GetExecutionState(ResponseStatus.PlatformNotActive, null), ExecutionState.ServerError);
            Assert.Equal(tx.GetExecutionState(ResponseStatus.Busy, null), ExecutionState.Retry);
            Assert.Equal(tx.GetExecutionState(ResponseStatus.InvalidNodeAccount, null), ExecutionState.Retry);
            Assert.Equal(tx.GetExecutionState(ResponseStatus.Ok, null), ExecutionState.Success);
            Assert.Equal(tx.GetExecutionState(ResponseStatus.AccountDeleted, null), ExecutionState.RequestError);
        }

        [Fact]
        public virtual void ShouldSetMaxRetry()
        {
            var tx = new DummyTransaction();
            tx.MaxRetry = 1;
            Assert.Equal(tx.MaxRetry, 1);
            Assert.Throws<ArgumentException>(() => { tx.MaxRetry = 0; });
        }

        [Fact]
        public virtual void ShouldMarkNodeAsUnusableOnInvalidNodeAccountId()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);
            node4Mock.Setup(n => n.IsHealthy()).Returns(true);
            node4Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);
            node5Mock.Setup(n => n.IsHealthy()).Returns(true);
            node5Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);
            var tx = new AnonymousDummyTransaction4
            {
                NodeAccountIds = [new AccountId(0, 0, 3), new AccountId(0, 0, 4), new AccountId(0, 0, 5)]
            };

            var txResp = new Proto.TransactionResponse { NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.InvalidNodeAccount };
            tx.BlockingUnaryCall = (grpcRequest) => txResp;

            // INVALID_NODE_ACCOUNT maps to RETRY, so it retries on the same node (doesn't advance)
            MaxAttemptsExceededException exception = Assert.Throws<MaxAttemptsExceededException>(() => tx.Execute(client));

            // Verify that increaseBackoff was called on the network for the node that returned INVALID_NODE_ACCOUNT
            networkMock.Verify(n => n.IncreaseBackoff(It.IsAny<Node>()), Times.AtLeastOnce());
        }

        private sealed class AnonymousDummyTransaction4 : DummyTransaction
        {
            public override ResponseStatus MapResponseStatus(Proto.Response response)
            {
                return ResponseStatus.InvalidNodeAccount;
            }
        }

        [Fact]
        public virtual void ShouldTriggerAddressBookUpdateOnInvalidNodeAccountId()
        {
            node3Mock.Setup(n => n.IsHealthy()).Returns(true);
            node3Mock.Setup(n => n.ChannelFailedToConnect(It.IsAny<DateTime>())).Returns(false);
            var tx = new AnonymousDummyTransaction5
            {
                NodeAccountIds = [new AccountId(0, 0, 3)]
            };
            var txResp = new Proto.TransactionResponse
            {
                NodeTransactionPrecheckCode = Proto.ResponseCodeEnum.InvalidNodeAccount
            };
            tx.BlockingUnaryCall = (grpcRequest) => txResp;

            // This should trigger address book update due to INVALID_NODE_ACCOUNT
            MaxAttemptsExceededException exception = Assert.Throws<MaxAttemptsExceededException>(() => tx.Execute(client));

            // Verify that increaseBackoff was called (node marking)
            networkMock.Verify(n => n.IncreaseBackoff(It.IsAny<Node>()), Times.AtLeastOnce());
            // Note: We can't easily test the address book update in this unit test since it's async
            // and involves network calls. The integration test would be more appropriate for that.
        }

        private sealed class AnonymousDummyTransaction5 : DummyTransaction
        {
            public override ResponseStatus MapResponseStatus(Proto.Response response)
            {
                return ResponseStatus.InvalidNodeAccount;
            }
        }

        class DummyTransaction : Executable<object, Proto.Transaction, Proto.TransactionResponse, TransactionResponse>
        {
            public override TransactionId TransactionIdInternal => null;
            public override void OnExecute(Client client) { }
            public override Task OnExecuteAsync(Client client) { return Task.CompletedTask; }
            public override Proto.Transaction MakeRequest() { return null; }
            public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request) { return null; }
            public override MethodDescriptor GetMethodDescriptor() { return null; }
            public override ResponseStatus MapResponseStatus(Proto.Response response) { return ResponseStatus.Ok; }
            public override Method<Proto.Transaction, Proto.TransactionResponse> GetMethod() { return null; }
        }

        class DummyQuery : Query<TransactionReceipt, TransactionReceiptQuery>
        {
            public override void OnExecute(Client client) { }
            public override TransactionReceipt MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request) { return null; }
            public override MethodDescriptor GetMethodDescriptor() { return null; }
            public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header) { }
            public override Proto.ResponseHeader MapResponseHeader(Proto.Response response) { return null; }
            public override Proto.QueryHeader MapRequestHeader(Proto.Query request) { return null; }
            public override ResponseStatus MapResponseStatus(Proto.Response response) { return ResponseStatus.Ok; }
            public override void ValidateChecksums(Client client) { }
        }
    }
}