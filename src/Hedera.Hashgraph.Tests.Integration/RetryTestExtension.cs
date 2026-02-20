// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class RetryTestExtension : TestExecutionExceptionHandler
    {
        public virtual void HandleTestExecutionException(ExtensionContext context, Exception throwable)
        {
            RetryTest retryTest = context.GetRequiredTestMethod().GetAnnotation(typeof(RetryTest));
            if (retryTest == null)
            {
                throw throwable;
            }

            int maxAttempts = retryTest.MaxAttempts();
            long initialDelay = retryTest.InitialDelayMs();
            long maxDelay = retryTest.MaxDelayMs();
            long currentDelay = initialDelay;
            int attempt = 1;
            Exception lastException = throwable;
            while (attempt < maxAttempts)
            {
                Console.WriteLine("Retrying test " + context.GetDisplayName() + " after failure.");
                try
                {

                    // Wait with exponential backoff
                    Thread.Sleep(currentDelay);

                    // Execute the test again
                    context.GetRequiredTestMethod().Invoke(context.GetRequiredTestInstance());
                    Console.WriteLine("Test " + context.GetDisplayName() + " passed on retry attempt" + attempt + 1);
                    return;
                }
                catch (Exception t)
                {
                    lastException = t;
                    attempt++;

                    // Calculate next delay with exponential backoff
                    currentDelay = Math.Min(currentDelay * 2, maxDelay);
                }
            }


            // If we get here, we've exhausted our retries
            Console.WriteLine("Test " + context.GetDisplayName() + " reached max attempts.");
            throw lastException;
        }
    }
}