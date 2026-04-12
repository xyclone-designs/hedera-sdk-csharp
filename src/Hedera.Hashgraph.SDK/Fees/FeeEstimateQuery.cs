// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
		private static bool ShouldRetry(HttpStatusCode statusCode)
		{
			return ShouldRetry((int)statusCode);
		}
		private static bool ShouldRetry(Exception exception)
        {
            return exception is TimeoutException || (exception as HttpRequestException)?.InnerException is TimeoutException  || exception is HttpIOException;
        }
        private static bool IsSuccessfulResponse(int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }
		private static bool IsSuccessfulResponse(HttpStatusCode statusCode)
		{
			return IsSuccessfulResponse((int)statusCode);
		}

		public virtual FeeEstimateMode Mode { get; set; } = FeeEstimateMode.State;
		public virtual Proto.Services.Transaction? Transaction { get; set; }
        public virtual int MaxAttempts { get; set; } = 10;
        public virtual TimeSpan MaxBackoff
        {
            get;
            set
            {
				if (value.TotalMilliseconds < 500)
					throw new ArgumentException("maxBackoff must be at least 500 ms");

				field = value;
			}

        } = TimeSpan.FromSeconds(8);

		public virtual FeeEstimateQuery SetTransaction<T>(Transaction<T> transaction) where T : Transaction<T>
		{
			Transaction = transaction.MakeRequest();

			return this;
		}

		public virtual FeeEstimateResponse Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
        }
        public virtual FeeEstimateResponse Execute(Client client, TimeSpan timeout)
        {
            var requestPayload = GetRequestPayload();
            var url = BuildUrl(client, Mode);
            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    HTTP_CLIENT.Timeout = timeout;
					var response = HTTP_CLIENT.Send(BuildHttpRequest(url, requestPayload));
                    var result = HandleResponse(response, Mode, attempt);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (Exception error)
                {
                    HandleError(error, attempt);
                }
            }

			throw new HttpIOException(HttpRequestError.InvalidResponse, "Failed to fetch fee estimate after " + MaxAttempts + " attempts");
        }

        /// <include file="FeeEstimateQuery.cs.xml" path='docs/member[@name="M:HandleResponse(HttpResponseMessage,FeeEstimateMode,System.Int32)"]/*' />
        private FeeEstimateResponse? HandleResponse(HttpResponseMessage response, FeeEstimateMode resolvedMode, int attempt)
        {
            if (IsSuccessfulResponse(response.StatusCode))
            {
                return FeeEstimateResponse.FromJson(response.Content.ReadAsStringAsync().GetAwaiter().GetResult(), resolvedMode);
            }

            if (!ShouldRetry(response.StatusCode) || attempt >= MaxAttempts)
            {
                throw new InvalidOperationException("Failed to fetch fee estimate. HTTP status: " + response.StatusCode + " body: " + response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }

            WarnAndDelay(attempt, new Exception("HTTP status: " + response.StatusCode));

            return null;
        }

        /// <include file="FeeEstimateQuery.cs.xml" path='docs/member[@name="M:HandleError(System.Exception,System.Int32)"]/*' />
        private void HandleError(Exception error, int attempt)
        {
            if (!ShouldRetry(error) || attempt >= MaxAttempts)
            {
                LOGGER.Error("Error attempting to get fee estimate", error);
                
                throw new InvalidOperationException(null, error);
            }

            WarnAndDelay(attempt, error);
        }

        public virtual TaskCompletionSource<FeeEstimateResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.RequestTimeout);
        }

        public virtual TaskCompletionSource<FeeEstimateResponse> ExecuteAsync(Client client, TimeSpan timeout)
        {
            TaskCompletionSource<FeeEstimateResponse> returnFuture = new ();
            ExecuteAsync(client, timeout, Mode, returnFuture, 1);
            return returnFuture;
        }

        public virtual async void ExecuteAsync(Client client, TimeSpan timeout, FeeEstimateMode resolvedMode, TaskCompletionSource<FeeEstimateResponse> returnFuture, int attempt)
        {
            var requestPayload = GetRequestPayload();
            var url = BuildUrl(client, resolvedMode);

        	HttpResponseMessage httpresponsemessage;

            try
            {
                HTTP_CLIENT.Timeout = timeout;
				httpresponsemessage = await HTTP_CLIENT.SendAsync(BuildHttpRequest(url, requestPayload));
            }
            catch (Exception exception) 
            {
				HandleAsyncError(client, timeout, resolvedMode, returnFuture, attempt, exception);
			
                return;
			}

            HandleAsyncResponse(client, timeout, resolvedMode, returnFuture, attempt, httpresponsemessage);
		}

        /// <include file="FeeEstimateQuery.cs.xml" path='docs/member[@name="M:HandleAsyncError(Client,System.TimeSpan,FeeEstimateMode,TaskCompletionSource{FeeEstimateResponse},System.Int32,System.Exception)"]/*' />
        private void HandleAsyncError(Client client, TimeSpan timeout, FeeEstimateMode resolvedMode, TaskCompletionSource<FeeEstimateResponse> returnFuture, int attempt, Exception error)
        {
            if (attempt >= MaxAttempts || !ShouldRetry(error))
            {
                LOGGER.Error("Error attempting to get fee estimate", error);
                returnFuture.SetException(error);
                return;
            }

            WarnAndDelay(attempt, error);
            ExecuteAsync(client, timeout, resolvedMode, returnFuture, attempt + 1);
        }

        /// <include file="FeeEstimateQuery.cs.xml" path='docs/member[@name="M:HandleAsyncResponse(Client,System.TimeSpan,FeeEstimateMode,TaskCompletionSource{FeeEstimateResponse},System.Int32,HttpResponseMessage)"]/*' />
        private async void HandleAsyncResponse(Client client, TimeSpan timeout, FeeEstimateMode resolvedMode, TaskCompletionSource<FeeEstimateResponse> returnFuture, int attempt, HttpResponseMessage response)
        {
            if (IsSuccessfulResponse(response.StatusCode))
            {
                returnFuture.SetResult(FeeEstimateResponse.FromJson(await response.Content.ReadAsStringAsync(), resolvedMode));
                return;
            }

            if (attempt >= MaxAttempts || !ShouldRetry(response.StatusCode))
            {
                LOGGER.Error("Failed to fetch fee estimate.HTTP status: {} body: {}", response.StatusCode, await response.Content.ReadAsStringAsync());
                returnFuture.SetException(new Exception("Failed to fetch fee estimate, status " + response.StatusCode));
                return;
            }

            WarnAndDelay(attempt, new Exception("Transient HTTP status: " + response.StatusCode));
            ExecuteAsync(client, timeout, resolvedMode, returnFuture, attempt + 1);
        }

        public virtual HttpRequestMessage BuildRequest(Client client, FeeEstimateMode resolvedMode)
        {
            string url = BuildUrl(client, resolvedMode);

            return BuildHttpRequest(url, GetRequestPayload());
        }

        private byte[] GetRequestPayload()
        {
            if (Transaction == null)
				throw new InvalidOperationException("transaction must be set before executing fee estimate");

			return Transaction.ToByteArray();
        }

        private string BuildUrl(Client client, FeeEstimateMode resolvedMode)
        {
            // Keep Mode casing consistent with JS SDK (uppercase)
            return client.MirrorRestBaseUrl + "/network/fees?Mode=" + resolvedMode.ToString();
        }

        private HttpRequestMessage BuildHttpRequest(string url, byte[] payload)
        {
			HttpRequestMessage httprequestmessage = new ()
            {
                Method = HttpMethod.Post,
                Content = new ByteArrayContent(payload),
                RequestUri = new Uri(url),
            };

            httprequestmessage.Headers.Add("Content-Type", "application/protobuf");

            return httprequestmessage;
        }

        private void WarnAndDelay(int attempt, Exception error)
        {
            var delay = Math.Min(500 * Math.Pow(2, attempt), MaxBackoff.TotalMilliseconds);
            LOGGER.Warn("Error fetching fee estimate during attempt #{}. Waiting {} ms before next attempt: {}", attempt, delay, error.Message);
            try
            {
                Thread.Sleep((int)delay);
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
            }
        }
    }
}
