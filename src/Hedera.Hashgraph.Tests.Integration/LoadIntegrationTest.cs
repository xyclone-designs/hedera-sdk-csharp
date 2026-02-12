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
        virtual void LoadTest()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var operatorPrivateKey = PrivateKey.FromString(System.GetProperty("OPERATOR_KEY"));
                var operatorId = AccountId.FromString(System.GetProperty("OPERATOR_ID"));
                int nThreads = 10;
                var clientExecutor = Executors.NewFixedThreadPool(16);
                var threadPoolExecutor = (ThreadPoolExecutor)Executors.NewFixedThreadPool(nThreads);
                long startTime = System.CurrentTimeMillis();
                System.@out.Println("Finished executing tasks:");
                for (int i = 0; i < nThreads; i++)
                {
                    int finalI = i;
                    threadPoolExecutor.Submit(() =>
                    {
                        try
                        {
                            using (var client = Client.ForNetwork(testEnv.client.GetNetwork(), clientExecutor))
                            {
                                client.SetOperator(operatorId, operatorPrivateKey);
                                client.SetMaxAttempts(10);
                                new AccountCreateTransaction().SetKeyWithoutAlias(PrivateKey.GenerateED25519()).Execute(client).GetReceipt(client);
                                System.@out.Println(finalI);
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
                        System.@out.Println();
                        System.@out.Println("Forcing shutdown");
                        threadPoolExecutor.ShutdownNow();
                    }
                }
                catch (InterruptedException e)
                {
                    threadPoolExecutor.ShutdownNow();
                }

                long endTime = System.CurrentTimeMillis();
                long executionTime = endTime - startTime;
                System.@out.Println("All tasks have finished execution in " + executionTime + "ms");
                clientExecutor.ShutdownNow();
            }
        }
    }
}