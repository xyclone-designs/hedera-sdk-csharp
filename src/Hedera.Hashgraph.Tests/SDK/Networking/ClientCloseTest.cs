// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Mockito.ArgumentMatchers;
using Org.Mockito.Mockito;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Org.Junit.Jupiter.Api;
using Org.Mockito;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Networking;
using Moq;
using System.Threading;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class ClientCloseTest
    {
        public virtual void DoesNotCloseExternalExecutor()
        {
            var executor = new ExecutorService();
            var network = new Dictionary<string, AccountId>();
            var client = Client.ForNetwork(network, executor);
            client.Dispose();
            Assert.False(executor.IsShutdown());
            client = Client.ForMainnet(executor);
            client.Dispose();
            Assert.False(executor.IsShutdown());
            client = Client.ForTestnet(executor);
            client.Dispose();
            Assert.False(executor.IsShutdown());
            client = Client.ForPreviewnet(executor);
            client.Dispose();
            Assert.False(executor.IsShutdown());
        }

        public virtual void CloseHandlesNetworkTimeout()
        {
            var executor = new ExecutorService();
            var networkMock = new Mock<Network>();
            var network = networkMock.Object;

            networkMock.Setup(n => n.AwaitClose(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(new TimeoutException("network timeout"));

            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);

            TimeoutException timeoutexception = Assert.Throws<TimeoutException>(client.Dispose);
            Assert.Contains(timeoutexception.Message, "network timeout");
            Assert.True(mirrorNetwork.hasShutDownNow);
        }

        public virtual void CloseHandlesNetworkInterrupted()
        {
            var interruptedException = new ThreadInterruptedException("network interrupted");
            var executor = new ExecutorService();

            var networkMock = new Mock<Network>();
            var network = networkMock.Object;

            networkMock.Setup(n => n.AwaitClose(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(interruptedException);

            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);

            Exception exception = Assert.Throws<Exception>(client.Dispose);
            Assert.Contains(exception.Message, interruptedException);
            Assert.True(mirrorNetwork.hasShutDownNow);
        }

        public virtual void CloseHandlesMirrorNetworkTimeout()
        {
            var executor = new ExecutorService();
            var network = Network.ForNetwork(executor, []);

            var mirrorNetworkMock = new Mock<MirrorNetwork>();
            var mirrorNetwork = mirrorNetworkMock.Object;

            mirrorNetworkMock
                .Setup(m => m.AwaitClose(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(new TimeoutException("mirror timeout"));

            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);

            TimeoutException timeoutexception = Assert.Throws<TimeoutException>(client.Dispose);
            Assert.Contains(timeoutexception.Message, "mirror timeout");
        }

        public virtual void CloseHandlesMirrorNetworkInterrupted()
        {
            var interruptedException = new ThreadInterruptedException("network interrupted");
            var executor = new ExecutorService();
            var network = Network.ForNetwork(executor, []);

            var mirrorNetworkMock = new Mock<MirrorNetwork>();
            var mirrorNetwork = mirrorNetworkMock.Object;

            mirrorNetworkMock.Setup(m => m.AwaitClose(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(interruptedException);

            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);

            Exception exception = Assert.Throws<Exception>(client.Dispose);
            Assert.Contains(exception.Message, interruptedException);
        }

        public virtual void CloseHandlesExecutorShutdown()
        {
            var executor = new ExecutorService();
            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            client.Dispose();
            Assert.True(executor.IsShutdown());
        }

        public virtual void CloseHandlesExecutorTerminatingInTime()
        {
            var duration = TimeSpan.FromSeconds(30);

            var executorMock = new Mock<ThreadPoolExecutor>();
            var executor = executorMock.Object;

            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);

            executorMock.Setup(e => e.AwaitTermination(30 / 2, TimeUnit.SECONDS)).Returns(true);

            client.Dispose(duration);

            executorMock.Verify(e => e.ShutdownNow(), Times.Exactly(0));
        }

        public virtual void CloseHandlesExecutorNotTerminatingInTime()
        {
            var duration = TimeSpan.FromSeconds(30);

            var executorMock = new Mock<ThreadPoolExecutor>();
            var executor = executorMock.Object;

            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);

            executorMock.Setup(e => e.AwaitTermination(30 / 2, TimeUnit.SECONDS)).Returns(false);

            client.Dispose(duration);

            executorMock.Verify(e => e.ShutdownNow(), Times.Exactly(1));
        }

        public virtual void CloseHandlesExecutorWhenThreadIsInterrupted()
        {
            var duration = TimeSpan.FromSeconds(30);

            var executorMock = new Mock<ThreadPoolExecutor>();
            var executor = executorMock.Object;

            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);

            executorMock.Setup(e => e.AwaitTermination(30 / 2, TimeUnit.SECONDS))
                .Throws(new ThreadInterruptedException());

            client.Dispose(duration);

            executorMock.Verify(e => e.ShutdownNow(), Times.Exactly(1));
        }

        public virtual void NoHealthyNodesNetwork()
        {
            var executor = new ExecutorService();
            var network = Network.ForNetwork(executor, []);
            InvalidOperationException invalidoperationexception = Assert.Throws<InvalidOperationException>(network.RandomNode);
            Assert.Contains(invalidoperationexception.Message, "No healthy node was found");
        }
    }
}