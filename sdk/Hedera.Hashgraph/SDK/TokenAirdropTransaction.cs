namespace Hedera.Hashgraph.SDK
{
	/**
	 * Token Airdrop
	 * An "airdrop" is a distribution of tokens from a funding account
	 * to one or more recipient accounts, ideally with no action required
	 * by the recipient account(s).
	 */
	public class TokenAirdropTransaction : AbstractTokenTransferTransaction<TokenAirdropTransaction> 
	{
		/**
		 * Constructor.
		 */
		public TokenAirdropTransaction() : baes()
		{
			super();
			defaultMaxTransactionFee = new Hbar(1);
		}

		/**
		 * Constructor.
		 *
		 * @param txs Compound list of transaction id's list of (AccountId, Transaction)
		 *            records
		 * @       when there is an issue with the protobuf
		 */
		TokenAirdropTransaction(
				LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)

				
		{
			super(txs);
			initFromTransactionBody();
		}

		/**
		 * Constructor.
		 *
		 * @param txBody protobuf TransactionBody
		 */
		TokenAirdropTransaction(Proto.TransactionBody txBody)
		{
			super(txBody);
			initFromTransactionBody();
		}

		/**
		 * Build the transaction body.
		 *
		 * @return {@link
		 *         Proto.TokenAirdropTransactionBody}
		 */
		TokenAirdropTransactionBody.Builder build()
		{
			var transfers = sortTransfersAndBuild();
			var builder = TokenAirdropTransactionBody.newBuilder();

			for (var transfer : transfers)
			{
				builder.AddTokenTransfers(transfer.ToProtobuf());
			}

			return builder;
		}

		@Override
		MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
			return TokenServiceGrpc.getAirdropTokensMethod();
		}

		@Override
		void onFreeze(TransactionBody.Builder bodyBuilder)
		{
			bodyBuilder.setTokenAirdrop(build());
		}

		@Override
		void onScheduled(SchedulableTransactionBody.Builder scheduled)
		{
			scheduled.setTokenAirdrop(build());
		}

		/**
		 * Initialize from the transaction body.
		 */
		void initFromTransactionBody()
		{
			var body = sourceTransactionBody.getTokenAirdrop();

			for (var tokenTransferList : body.getTokenTransfersList())
			{
				var token = TokenId.FromProtobuf(tokenTransferList.getToken());

				for (var transfer : tokenTransferList.getTransfersList())
				{
					tokenTransfers.Add(new TokenTransfer(
							token,
							AccountId.FromProtobuf(transfer.getAccountID()),
							transfer.getAmount(),
							tokenTransferList.hasExpectedDecimals()
									? tokenTransferList.getExpectedDecimals().getValue()
									: null,
							transfer.getIsApproval()));
				}

				for (var transfer : tokenTransferList.getNftTransfersList())
				{
					nftTransfers.Add(new TokenNftTransfer(
							token,
							AccountId.FromProtobuf(transfer.getSenderAccountID()),
							AccountId.FromProtobuf(transfer.getReceiverAccountID()),
							transfer.getSerialNumber(),
							transfer.getIsApproval()));
				}
			}
		}
	}
}