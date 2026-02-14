using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Utils
{
    internal class ActionHelper
    {
        public static async void Action<TSuccess>(Task<TSuccess> task, Action<TSuccess?, Exception?> callback)
        {
			TSuccess? tsuccess = default;
			Exception? exception = null;

			try 
			{
				tsuccess = await task;
			}
			catch (Exception _exception) 
			{
				exception = _exception; 
			}

			callback.Invoke(tsuccess, exception);
		}
        public static async void TwoActions<TSuccess>(Task<TSuccess> task, Action<TSuccess> onSuccess, Action<Exception> onException)
        {
			TSuccess tsuccess;
			
			try 
			{
				tsuccess = await task;
			}
			catch (Exception exception) 
			{
				onException.Invoke(exception);
				return;
			}

			onSuccess.Invoke(tsuccess);
		}
	}
}
