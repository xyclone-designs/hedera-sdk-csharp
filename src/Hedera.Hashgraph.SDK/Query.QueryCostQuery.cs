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
			public override void OnMakeRequest(Proto.Services.Query query, Proto.Services.QueryHeader header) 
			{
				header.ResponseType = Proto.Services.ResponseType.CostAnswer;

				// COST_ANSWER requires a payment to pass validation but doesn't actually process it
				// yes, this transaction is completely invalid
				// that is okay
				// now go back to sleep
				// without this, an error of MISSING_QUERY_HEADER is returned
				header.Payment = new TransferTransaction
				{
					NodeAccountIds = [ new AccountId(0, 0, 0) ],
					TransactionId = TransactionId.WithValidStart(new AccountId(0, 0, 0), DateTimeOffset.FromUnixTimeMilliseconds(0))

				}.Freeze().MakeRequest();;
			}

			public override Hbar MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
			{
				return Hbar.FromTinybars((long)MapResponseHeader(response).Cost);
			}

			public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
			{
				return new Proto.Services.ResponseHeader();
			}
			public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request) 
			{
				return new Proto.Services.QueryHeader();
			}
            public override MethodDescriptor GetMethodDescriptor()
            {
                throw new NotImplementedException();
            }
        }
	}
}
