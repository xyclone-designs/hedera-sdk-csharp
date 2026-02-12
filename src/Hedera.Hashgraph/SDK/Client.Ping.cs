// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	public sealed partial class Client
    {
		/// <summary>
		/// Send a ping to the given node.
		/// </summary>
		/// <param name="nodeAccountId">Account ID of the node to ping</param>
		/// <exception cref="TimeoutException">when the transaction times out</exception>
		/// <exception cref="PrecheckStatusException">when the precheck fails</exception>
		public void Ping(AccountId nodeAccountId)
        {
            Ping(nodeAccountId, RequestTimeout);
        }
        /// <summary>
        /// Send a ping to the given node.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public void Ping(AccountId nodeAccountId, Duration timeout)
        {
            new AccountBalanceQuery
			{
				AccountId = nodeAccountId,
				NodeAccountIds = [nodeAccountId],

			}.Execute(this, timeout);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public Task PingAsync(AccountId nodeAccountId)
        {
            return PingAsync(nodeAccountId, RequestTimeout);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public async Task PingAsync(AccountId nodeAccountId, Duration timeout)
        {
			await new AccountBalanceQuery()
			{
				NodeAccountIds = [nodeAccountId],

			}.ExecuteAsync(this, timeout);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAsync(AccountId nodeAccountId, Action<Exception> callback)
        {
            ActionHelper.Action(PingAsync(nodeAccountId), callback);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAsync(AccountId nodeAccountId, Duration timeout, Action<Exception> callback)
        {
            ActionHelper.Action(PingAsync(nodeAccountId, timeout), callback);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAsync(AccountId nodeAccountId, Action onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(PingAsync(nodeAccountId), onSuccess, onFailure);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAsync(AccountId nodeAccountId, Duration timeout, Action onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(PingAsync(nodeAccountId, timeout), onSuccess, onFailure);
        }
        /// <summary>
        /// Sends pings to all nodes in the client's Network. Combines well with setMaxAttempts(1) to remove all dead nodes
        /// from the Network.
        /// </summary>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public void PingAll()
        {
            lock (this)
            {
                PingAll(RequestTimeout);
            }
        }
        /// <summary>
        /// Sends pings to all nodes in the client's Network. Combines well with setMaxAttempts(1) to remove all dead nodes
        /// from the Network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public void PingAll(Duration timeoutPerPing)
        {
            lock (this)
            {
                foreach (var nodeAccountId in Network_.GetNetwork().Values)
                {
                    Ping(nodeAccountId, timeoutPerPing);
                }
            }
        }
        /// <summary>
        /// Sends pings to all nodes in the client's Network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the Network.
        /// </summary>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public Task PingAllAsync()
        {
            lock (this)
            {
                return PingAllAsync(RequestTimeout);
            }
        }
        /// <summary>
        /// Sends pings to all nodes in the client's Network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the Network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public Task PingAllAsync(Duration timeoutPerPing)
        {
            lock (this)
            {
                var _Network = Network_.GetNetwork();

                var list = new List<Task>(_Network.Count);
                foreach (var nodeAccountId in _Network.Values)
                {
                    list.Add(PingAsync(nodeAccountId, timeoutPerPing));
                }

                return Task.WhenAll(list);
            }
        }
        /// <summary>
        /// Sends pings to all nodes in the client's Network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the Network.
        /// </summary>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAllAsync(Action<Exception> callback)
        {
            ActionHelper.Action(PingAllAsync(), callback);
        }
		/// <summary>
		/// Sends pings to all nodes in the client's Network asynchronously. Combines well with setMaxAttempts(1) to remove
		/// all dead nodes from the Network.
		/// </summary>
		/// <param name="onSuccess">a Action which consumes the result on success.</param>
		/// <param name="onFailure">a Action which consumes the error on failure.</param>
		public void PingAllAsync(Action onSuccess, Action<Exception> onFailure)
		{
			ActionHelper.TwoActions(PingAllAsync(), onSuccess, onFailure);
		}
		/// <summary>
		/// Sends pings to all nodes in the client's Network asynchronously. Combines well with setMaxAttempts(1) to remove
		/// all dead nodes from the Network.
		/// </summary>
		/// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
		/// <param name="callback">a Action which handles the result or error.</param>
		public void PingAllAsync(Duration timeoutPerPing, Action<Exception> callback)
        {
            ActionHelper.Action(PingAllAsync(timeoutPerPing), callback);
        }
        /// <summary>
        /// Sends pings to all nodes in the client's Network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the Network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAllAsync(Duration timeoutPerPing, Action onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(PingAllAsync(timeoutPerPing), onSuccess, onFailure);
        }
    }
}