using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	internal class ConsumerHelper
	{
		internal static void BiConsumer<T>(Task<T> future, Action<T, Exception?> consumer)
		{
			future.ContinueWith(async _ => consumer.Invoke(await _, _.Exception));
		}

		internal static void TwoConsumers<T>(Task<T> future, Action<T> onSuccess, Action<Exception> onFailure)
		{
			future.ContinueWith(async _ => 
			{
				if (_.Exception != null)
				{
					onFailure.Invoke(_.Exception);
				}
				else
				{
					onSuccess.Invoke(await _);
				}
			});
		}
	}

}