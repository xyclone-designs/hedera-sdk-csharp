// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using Java.Io;
using Java.Net;
using Java.Net.Http;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Javax.Annotation;
using Org.Slf4j;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Hedera.Hashgraph.SDK.Fees
{
    public class FeeEstimateQuery
    {
        private static readonly Logger LOGGER = LoggerFactory.GetLogger(typeof(FeeEstimateQuery));
        private static readonly HttpClient HTTP_CLIENT = new ();

		private static bool ShouldRetry(int statusCode)
		{
			// Retry on common transient HTTP statuses
			return statusCode == 408 || statusCode == 429 || (statusCode >= 500 && statusCode < 600);
		}
		private static bool ShouldRetry(Exception exception)
        {
            return exception is TimeoutException || (exception as HttpRequestException)?.InnerException is TimeoutException  || exception is HttpIOException;
        }
        private static bool IsSuccessfulResponse(int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }

        public virtual FeeEstimateMode Mode { get; set; } = FeeEstimateMode.State;
		public virtual Proto.Transaction? Transaction { get; set; }
        public virtual int MaxAttempts { get; set; } = 10;
        public virtual Duration MaxBackoff
        {
            get;
            set
            {
				if (value.ToTimeSpan().TotalMilliseconds < 500)
					throw new ArgumentException("maxBackoff must be at least 500 ms");

				field = value;
			}

        } = Duration.FromTimeSpan(TimeSpan.FromSeconds(8));

		public virtual FeeEstimateQuery SetTransaction<T>(Transaction<T> transaction) where T : Transaction<T>
		{
			Transaction = transaction.MakeRequest();

			return this;
		}

		public virtual FeeEstimateResponse Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
        }
        public virtual FeeEstimateResponse Execute(Client client, Duration timeout)
        {
            var requestPayload = GetRequestPayload();
            var url = BuildUrl(client, Mode);
            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    HTTP_CLIENT.Send()

                    var response = HTTP_CLIENT.Send(BuildHttpRequest(url, timeout, requestPayload), HttpResponse.BodyHandlers.OfString());
                    var result = HandleResponse(response, Mode, attempt);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                catch (Exception error)
                {
                    HandleError(error, attempt);
                }
            }

			throw new HttpIOException("Failed to fetch fee estimate after " + MaxAttempts + " attempts");
        }

        /// <summary>
        /// Handle the HTTP response and return the result or null if retry is needed.
        /// </summary>
        private FeeEstimateResponse HandleResponse(HttpResponseMessage response, FeeEstimateMode resolvedMode, int attempt)
        {
            if (IsSuccessfulResponse(response.StatusCode()))
            {
                return FeeEstimateResponse.FromJson(response.Body(), resolvedMode);
            }

            if (!ShouldRetry(response.StatusCode()) || attempt >= MaxAttempts)
            {
                throw new InvalidOperationException("Failed to fetch fee estimate. HTTP status: " + response.StatusCode() + " body: " + response.Body());
            }

            WarnAndDelay(attempt, new Exception("HTTP status: " + response.StatusCode()));
            return null;
        }

        /// <summary>
        /// Handle errors during execution.
        /// </summary>
        private void HandleError(Exception error, int attempt)
        {
            if (!ShouldRetry(error) || attempt >= MaxAttempts)
            {
                LOGGER.Error("Error attempting to get fee estimate", error);
                if (error is IOException)
                {
                    throw ioException;
                }

                if (error is InterruptedException)
                {
                    throw interruptedException;
                }

                throw new InvalidOperationException(error);
            }

            WarnAndDelay(attempt, error);
        }

        public virtual CompletableFuture<FeeEstimateResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.GetRequestTimeout());
        }

        public virtual CompletableFuture<FeeEstimateResponse> ExecuteAsync(Client client, Duration timeout)
        {
            var resolvedMode = mode != null ? mode : FeeEstimateMode.STATE;
            CompletableFuture<FeeEstimateResponse> returnFuture = new CompletableFuture();
            ExecuteAsync(client, timeout, resolvedMode, returnFuture, 1);
            return returnFuture;
        }

        virtual void ExecuteAsync(Client client, Duration timeout, FeeEstimateMode resolvedMode, CompletableFuture<FeeEstimateResponse> returnFuture, int attempt)
        {
            var requestPayload = GetRequestPayload();
            var url = BuildUrl(client, resolvedMode);
            HTTP_CLIENT.SendAsync(BuildHttpRequest(url, timeout, requestPayload), HttpResponse.BodyHandlers.OfString()).WhenComplete((response, error) =>
            {
                if (error != null)
                {
                    HandleAsyncError(client, timeout, resolvedMode, returnFuture, attempt, error);
                    return;
                }

                HandleAsyncResponse(client, timeout, resolvedMode, returnFuture, attempt, response);
            });
        }

        /// <summary>
        /// Handle async error response.
        /// </summary>
        private void HandleAsyncError(Client client, Duration timeout, FeeEstimateMode resolvedMode, CompletableFuture<FeeEstimateResponse> returnFuture, int attempt, Exception error)
        {
            if (attempt >= MaxAttempts || !ShouldRetry(error))
            {
                LOGGER.Error("Error attempting to get fee estimate", error);
                returnFuture.CompleteExceptionally(error);
                return;
            }

            WarnAndDelay(attempt, error);
            ExecuteAsync(client, timeout, resolvedMode, returnFuture, attempt + 1);
        }

        /// <summary>
        /// Handle async success response.
        /// </summary>
        private void HandleAsyncResponse(Client client, Duration timeout, FeeEstimateMode resolvedMode, CompletableFuture<FeeEstimateResponse> returnFuture, int attempt, HttpResponseMessage response)
        {
            if (IsSuccessfulResponse(response.StatusCode()))
            {
                returnFuture.Complete(FeeEstimateResponse.FromJson(response.Body(), resolvedMode));
                return;
            }

            if (attempt >= MaxAttempts || !ShouldRetry(response.StatusCode()))
            {
                LOGGER.Error("Failed to fetch fee estimate.HTTP status: {} body: {}", response.StatusCode(), response.Body());
                returnFuture.CompleteExceptionally(new Exception("Failed to fetch fee estimate, status " + response.StatusCode()));
                return;
            }

            WarnAndDelay(attempt, new Exception("Transient HTTP status: " + response.StatusCode()));
            ExecuteAsync(client, timeout, resolvedMode, returnFuture, attempt + 1);
        }

        virtual HttpRequestMessage BuildRequest(Client client, Duration timeout, FeeEstimateMode resolvedMode)
        {
            string url = BuildUrl(client, resolvedMode);
            return BuildHttpRequest(url, timeout, GetRequestPayload());
        }

        private byte[] GetRequestPayload()
        {
            if (transaction == null)
            {
                throw new InvalidOperationException("transaction must be set before executing fee estimate");
            }

            return transaction.ToByteArray();
        }

        private string BuildUrl(Client client, FeeEstimateMode resolvedMode)
        {

            // Keep mode casing consistent with JS SDK (uppercase)
            return client.GetMirrorRestBaseUrl() + "/network/fees?mode=" + resolvedMode.ToString();
        }

        private HttpRequestMessage BuildHttpRequest(string url, Duration timeout, byte[] payload)
        {
            return HttpRequest.NewBuilder().Uri(URI.Create(url)).Timeout(timeout).Header("Content-Type", "application/protobuf").POST(HttpRequest.BodyPublishers.OfByteArray(payload)).Build();
        }

        private void WarnAndDelay(int attempt, Exception error)
        {
            var delay = Math.Min(500 * (long)Math.Pow(2, attempt), maxBackoff.ToMillis());
            LOGGER.Warn("Error fetching fee estimate during attempt #{}. Waiting {} ms before next attempt: {}", attempt, delay, error.GetMessage());
            try
            {
                Thread.Sleep(delay);
            }
            catch (InterruptedException e)
            {
                Thread.CurrentThread().Interrupt();
            }
        }
    }
}