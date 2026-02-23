using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.SDK.Queries
{
	public abstract partial class Query<O, T> 
	{
		private class QueryCostQuery : Query<Hbar, QueryCostQuery> 
		{
			public override bool IsPaymentRequired => false;

			public override void ValidateChecksums(Client client)  { }
			public override void OnMakeRequest(Proto.Query query, Proto.QueryHeader header) 
			{
				header.ResponseType = Proto.ResponseType.CostAnswer;

				// COST_ANSWER requires a payment to pass validation but doesn't actually process it
				// yes, this transaction is completely invalid
				// that is okay
				// now go back to sleep
				// without this, an error of MISSING_QUERY_HEADER is returned
				header.Payment = new TransferTransaction
				{
					NodeAccountIds = [ new AccountId(0, 0, 0) ],
					TransactionId = TransactionId.WithValidStart(new AccountId(0, 0, 0), Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(0)))

				}.Freeze().MakeRequest();;
			}

			public override Hbar MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
			{
				return Hbar.FromTinybars((long)MapResponseHeader(response).Cost);
			}

			public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
			{
				return new Proto.ResponseHeader();
			}
			public override Proto.QueryHeader MapRequestHeader(Proto.Query request) 
			{
				return new Proto.QueryHeader();
			}
            public override MethodDescriptor GetMethodDescriptor()
            {
                throw new NotImplementedException();
            }
        }
	}
}