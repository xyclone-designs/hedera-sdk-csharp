using Org.BouncyCastle.Asn1.Ocsp;
using System.Diagnostics.Contracts;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Get the balance of a Hedera™ crypto-currency account. This returns only the balance, so it is a
 * smaller and faster reply than {@link AccountInfoQuery}.
 *
 * <p>This query is free.
 */
	public sealed class AccountBalanceQuery : Query<AccountBalance, AccountBalanceQuery> 
	{
		public AccountId? AccountId { get; set; }
		public ContractId? ContractId { get; set; }

		/**
		 * Constructor.
		 */
		public AccountBalanceQuery() { } 

		
		public override void ValidateChecksums(Client client) 
		{
			AccountId?.ValidateChecksum(client);
			ContractId?.ValidateChecksum(client);
		}

		public override bool IsPaymentRequired()
		{
			return false;
		}
		public override void OnMakeRequest(Proto.Query queryBuilder, QueryHeader header)
		{
			var builder = CryptoGetAccountBalanceQuery.newBuilder();
			if (accountId != null)
			{
				builder.setAccountID(accountId.ToProtobuf());
			}

			if (contractId != null)
			{
				builder.setContractID(contractId.ToProtobuf());
			}

			queryBuilder.setCryptogetAccountBalance(builder.setHeader(header));
		}

		public override AccountBalance MapResponse(Response response, AccountId nodeId, Proto.Query request)
		{
			return AccountBalance.FromProtobuf(response.getCryptogetAccountBalance());
		}

		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
		{
			return response.CryptogetAccountBalance.Header;
		}

		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.CryptogetAccountBalance.Header;
		}

		public override MethodDescriptor<Proto.Query, Response> getMethodDescriptor()
		{
			return CryptoServiceGrpc.getCryptoGetBalanceMethod();
		}
	}
}