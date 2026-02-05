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

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ClientCloseTest
    {
        virtual void DoesNotCloseExternalExecutor()
        {
            var executor = Client.CreateExecutor();
            var network = new HashMap<string, AccountId>();
            var client = Client.ForNetwork(network, executor);
            client.Dispose();
            AssertThat(executor.IsShutdown()).IsFalse();
            client = Client.ForMainnet(executor);
            client.Dispose();
            AssertThat(executor.IsShutdown()).IsFalse();
            client = Client.ForTestnet(executor);
            client.Dispose();
            AssertThat(executor.IsShutdown()).IsFalse();
            client = Client.ForPreviewnet(executor);
            client.Dispose();
            AssertThat(executor.IsShutdown()).IsFalse();
        }

        virtual void CloseHandlesNetworkTimeout()
        {
            var executor = Client.CreateExecutor();
            var network = Mockito.Mock(typeof(Network));
            When(network.AwaitClose(Any(), Any())).ThenReturn(new TimeoutException("network timeout"));
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, Collections.EmptyList());
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            AssertThatExceptionOfType(typeof(TimeoutException)).IsThrownBy(client.Dispose()).WithMessage("network timeout");
            AssertThat(mirrorNetwork.hasShutDownNow).IsTrue();
        }

        virtual void CloseHandlesNetworkInterrupted()
        {
            var interruptedException = new InterruptedException("network interrupted");
            var executor = Client.CreateExecutor();
            var network = Mockito.Mock(typeof(Network));
            When(network.AwaitClose(Any(), Any())).ThenReturn(interruptedException);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, Collections.EmptyList());
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(client.Dispose()).WithCause(interruptedException);
            AssertThat(mirrorNetwork.hasShutDownNow).IsTrue();
        }

        virtual void CloseHandlesMirrorNetworkTimeout()
        {
            var executor = Client.CreateExecutor();
            var network = Network.ForNetwork(executor, Collections.EmptyMap());
            var mirrorNetwork = Mockito.Mock(typeof(MirrorNetwork));
            When(mirrorNetwork.AwaitClose(Any(), Any())).ThenReturn(new TimeoutException("mirror timeout"));
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            AssertThatExceptionOfType(typeof(TimeoutException)).IsThrownBy(client.Dispose()).WithMessage("mirror timeout");
            AssertThat(network.hasShutDownNow).IsFalse();
        }

        virtual void CloseHandlesMirrorNetworkInterrupted()
        {
            var interruptedException = new InterruptedException("network interrupted");
            var executor = Client.CreateExecutor();
            var network = Network.ForNetwork(executor, Collections.EmptyMap());
            var mirrorNetwork = Mockito.Mock(typeof(MirrorNetwork));
            When(mirrorNetwork.AwaitClose(Any(), Any())).ThenReturn(interruptedException);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            AssertThatExceptionOfType(typeof(Exception)).IsThrownBy(client.Dispose()).WithCause(interruptedException);
            AssertThat(network.hasShutDownNow).IsFalse();
        }

        virtual void CloseHandlesExecutorShutdown()
        {
            var executor = Client.CreateExecutor();
            var network = Network.ForNetwork(executor, Collections.EmptyMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, Collections.EmptyList());
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            client.Dispose();
            AssertThat(executor.IsShutdown()).IsTrue();
        }

        virtual void CloseHandlesExecutorTerminatingInTime()
        {
            var duration = Duration.OfSeconds(30);
            var executor = Mock(typeof(ThreadPoolExecutor));
            var network = Network.ForNetwork(executor, Collections.EmptyMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, Collections.EmptyList());
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            DoReturn(true).When(executor).AwaitTermination(30 / 2, TimeUnit.SECONDS);
            client.Dispose(duration);
            Verify(executor, Times(0)).ShutdownNow();
        }

        virtual void CloseHandlesExecutorNotTerminatingInTime()
        {
            var duration = Duration.OfSeconds(30);
            var executor = Mock(typeof(ThreadPoolExecutor));
            var network = Network.ForNetwork(executor, Collections.EmptyMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, Collections.EmptyList());
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            DoReturn(false).When(executor).AwaitTermination(30 / 2, TimeUnit.SECONDS);
            client.Dispose(duration);
            Verify(executor, Times(1)).ShutdownNow();
        }

        virtual void CloseHandlesExecutorWhenThreadIsInterrupted()
        {
            var duration = Duration.OfSeconds(30);
            var executor = Mock(typeof(ThreadPoolExecutor));
            var network = Network.ForNetwork(executor, Collections.EmptyMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, Collections.EmptyList());
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            DoThrow(new InterruptedException()).When(executor).AwaitTermination(30 / 2, TimeUnit.SECONDS);
            client.Dispose(duration);
            Verify(executor, Times(1)).ShutdownNow();
        }

        virtual void NoHealthyNodesNetwork()
        {
            var executor = Client.CreateExecutor();
            var network = Network.ForNetwork(executor, Collections.EmptyMap());
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(network.GetRandomNode()).WithMessage("No healthy node was found");
        }
    }
}