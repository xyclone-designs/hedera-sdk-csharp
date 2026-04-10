using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Utils
{
    internal class ActionHelper
    {
        public static async void Action(Task task, Action<Exception> onException)
        {
			try 
			{
				await task;
			}
			catch (Exception exception) 
			{
				onException.Invoke(exception);
			}
		}
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

		public static async void TwoActions(Task task, Action onSuccess, Action<Exception> onException)
		{
			try
			{
				await task;
			}
			catch (Exception exception)
			{
				onException.Invoke(exception);
				return;
			}

			onSuccess.Invoke();
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
