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
        public virtual void HandleTestExecutionException(ExtensionContext context, Throwable throwable)
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
            Throwable lastThrowable = throwable;
            while (attempt < maxAttempts)
            {
                System.@out.Println("Retrying test " + context.GetDisplayName() + " after failure.");
                try
                {

                    // Wait with exponential backoff
                    Thread.Sleep(currentDelay);

                    // Execute the test again
                    context.GetRequiredTestMethod().Invoke(context.GetRequiredTestInstance());
                    System.@out.Println("Test " + context.GetDisplayName() + " passed on retry attempt" + attempt + 1);
                    return;
                }
                catch (Throwable t)
                {
                    lastThrowable = t;
                    attempt++;

                    // Calculate next delay with exponential backoff
                    currentDelay = Math.Min(currentDelay * 2, maxDelay);
                }
            }


            // If we get here, we've exhausted our retries
            System.@out.Println("Test " + context.GetDisplayName() + " reached max attempts.");
            throw lastThrowable;
        }
    }
}