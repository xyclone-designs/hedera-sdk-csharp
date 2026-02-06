using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Networking;

namespace Hedera.Hashgraph.SDK.Queries
{
	public abstract partial class Query<O, T> 
	{
		private class GrpcCostQuery
		{
			public Hbar MaxCost { get; }
			public bool NotRequired { get; }
			public Query<O, T> Parent { get; }

			public Hbar? Cost { get; set; }
			private Client.Operator? Operator { get; }

			public GrpcCostQuery(Client client, Query<O, T> parent)
			{
				Parent = parent;
				Parent.InitWithNodeIds(client);

				Cost = Parent.QueryPayment;
				MaxCost = Parent.MaxQueryPayment ?? client.DefaultMaxQueryPayment;
				NotRequired = Parent.PaymentTransactions != null || !parent.IsPaymentRequired;

				if (!NotRequired)
					Operator = Query<O, T>.GetOperatorFromClient(client);
			}

			public void Finish()
			{
				Parent.ChosenQueryPayment = Cost;
				Parent.PaymentOperator = Operator;
				Parent.PaymentTransactions = Parent;
			}
			public bool ShouldError()
			{
				// Check if this is below our configured maximum query payment
				return Cost?.CompareTo(MaxCost) > 0;
			}

			public MaxQueryPaymentExceededException MapError()
			{
				return new MaxQueryPaymentExceededException(Parent.GetType(), Cost, MaxCost);
			}
		}
	}
}