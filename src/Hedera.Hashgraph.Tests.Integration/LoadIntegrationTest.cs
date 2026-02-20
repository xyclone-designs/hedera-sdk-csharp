// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util.Concurrent;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class LoadIntegrationTest
    {
        public virtual void LoadTest()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var operatorPrivateKey = PrivateKey.FromString(System.GetProperty("OPERATOR_KEY"));
                var operatorId = AccountId.FromString(System.GetProperty("OPERATOR_ID"));
                int nThreads = 10;
                var clientExecutor = Executors.NewFixedThreadPool(16);
                var threadPoolExecutor = (ThreadPoolExecutor)Executors.NewFixedThreadPool(nThreads);
                long startTime = System.CurrentTimeMillis();
                Console.WriteLine("Finished executing tasks:");
                for (int i = 0; i < nThreads; i++)
                {
                    int finalI = i;
                    threadPoolExecutor.Submit(() =>
                    {
                        try
                        {
                            using (var client = Client.ForNetwork(testEnv.Client.Network, clientExecutor))
                            {
                                client.OperatorSet(operatorId, operatorPrivateKey);
                                client.SetMaxAttempts(10);
                                new AccountCreateTransaction()Key = PrivateKey.GenerateED25519(,).Execute(client).GetReceipt(client);
                                Console.WriteLine(finalI);
                            }
                        }
                        catch (Exception e)
                        {
                            Fail("AccountCreateTransaction failed, " + e);
                        }
                    });
                }

                threadPoolExecutor.Shutdown();

                // Wait for all tasks to finish
                try
                {
                    if (!threadPoolExecutor.AwaitTermination(60, TimeUnit.SECONDS))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Forcing shutdown");
                        threadPoolExecutor.ShutdownNow();
                    }
                }
                catch (InterruptedException e)
                {
                    threadPoolExecutor.ShutdownNow();
                }

                long endTime = System.CurrentTimeMillis();
                long executionTime = endTime - startTime;
                Console.WriteLine("All tasks have finished execution in " + executionTime + "ms");
                clientExecutor.ShutdownNow();
            }
        }
    }
}