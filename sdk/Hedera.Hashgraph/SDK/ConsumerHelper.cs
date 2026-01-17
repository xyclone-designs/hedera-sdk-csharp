namespace Hedera.Hashgraph.SDK
{
	class ConsumerHelper
	{
		static <T> void biConsumer(Task<T> future, BiConsumer<T, Throwable> consumer)
		{
			future.whenComplete(consumer);
		}

		static <T> void twoConsumers(Task<T> future, Consumer<T> onSuccess, Consumer<Throwable> onFailure)
		{
			future.whenComplete((output, error)-> {
				if (error != null)
				{
					onFailure.accept(error);
				}
				else
				{
					onSuccess.accept(output);
				}
			});
		}
	}

}