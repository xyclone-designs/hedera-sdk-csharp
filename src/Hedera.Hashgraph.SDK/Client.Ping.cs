// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Exceptions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	public sealed partial class Client
    {
		/// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:Ping(AccountId)"]/*' />
		public void Ping(AccountId nodeAccountId)
        {
            Ping(nodeAccountId, RequestTimeout);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:Ping(AccountId,System.TimeSpan)"]/*' />
        public void Ping(AccountId nodeAccountId, TimeSpan timeout)
        {
            new AccountBalanceQuery
			{
				AccountId = nodeAccountId,
				NodeAccountIds = [nodeAccountId],

			}.Execute(this, timeout);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAsync(AccountId)"]/*' />
        public Task PingAsync(AccountId nodeAccountId)
        {
            return PingAsync(nodeAccountId, RequestTimeout);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAsync(AccountId,System.TimeSpan)"]/*' />
        public async Task PingAsync(AccountId nodeAccountId, TimeSpan timeout)
        {
			await new AccountBalanceQuery()
			{
				NodeAccountIds = [nodeAccountId],

			}.ExecuteAsync(this, timeout);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAsync(AccountId,System.Action{System.Exception})"]/*' />
        public void PingAsync(AccountId nodeAccountId, Action<Exception> callback)
        {
            Utils.ActionHelper.Action(PingAsync(nodeAccountId), callback);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAsync(AccountId,System.TimeSpan,System.Action{System.Exception})"]/*' />
        public void PingAsync(AccountId nodeAccountId, TimeSpan timeout, Action<Exception> callback)
        {
            Utils.ActionHelper.Action(PingAsync(nodeAccountId, timeout), callback);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAsync(AccountId,System.Action,System.Action{System.Exception})"]/*' />
        public void PingAsync(AccountId nodeAccountId, Action onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(PingAsync(nodeAccountId), onSuccess, onFailure);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAsync(AccountId,System.TimeSpan,System.Action,System.Action{System.Exception})"]/*' />
        public void PingAsync(AccountId nodeAccountId, TimeSpan timeout, Action onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(PingAsync(nodeAccountId, timeout), onSuccess, onFailure);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAll"]/*' />
        public void PingAll()
        {
            lock (this)
            {
                PingAll(RequestTimeout);
            }
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAll(System.TimeSpan)"]/*' />
        public void PingAll(TimeSpan timeoutPerPing)
        {
            lock (this)
            {
                foreach (var nodeAccountId in Network_.GetNetwork().Values)
                {
                    Ping(nodeAccountId, timeoutPerPing);
                }
            }
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAllAsync"]/*' />
        public Task PingAllAsync()
        {
            lock (this)
            {
                return PingAllAsync(RequestTimeout);
            }
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAllAsync(System.TimeSpan)"]/*' />
        public Task PingAllAsync(TimeSpan timeoutPerPing)
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
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAllAsync(System.Action{System.Exception})"]/*' />
        public void PingAllAsync(Action<Exception> callback)
        {
            Utils.ActionHelper.Action(PingAllAsync(), callback);
        }
		/// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAllAsync(System.Action,System.Action{System.Exception})"]/*' />
		public void PingAllAsync(Action onSuccess, Action<Exception> onFailure)
		{
			Utils.ActionHelper.TwoActions(PingAllAsync(), onSuccess, onFailure);
		}
		/// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAllAsync(System.TimeSpan,System.Action{System.Exception})"]/*' />
		public void PingAllAsync(TimeSpan timeoutPerPing, Action<Exception> callback)
        {
            Utils.ActionHelper.Action(PingAllAsync(timeoutPerPing), callback);
        }
        /// <include file="Client.Ping.cs.xml" path='docs/member[@name="M:PingAllAsync(System.TimeSpan,System.Action,System.Action{System.Exception})"]/*' />
        public void PingAllAsync(TimeSpan timeoutPerPing, Action onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(PingAllAsync(timeoutPerPing), onSuccess, onFailure);
        }
    }
}