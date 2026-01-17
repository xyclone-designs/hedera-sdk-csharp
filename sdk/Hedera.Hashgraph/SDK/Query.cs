using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Base class for all queries that can be submitted to Hedera.
	 *
	 * @param <O> The output type of the query.
	 * @param <T> The type of the query itself. Used to enable chaining.
	 */
	public abstract class Query<O, T> where T : Query<O, T>
	{

		private readonly Proto.Query.Builder builder;

		private readonly QueryHeader.Builder headerBuilder;

		/**
		 * The transaction ID
		 */
		protected TransactionId? PaymentTransactionId  { get; private set; }
		/**
		 * List of payment transactions
		 */
		protected List<Transaction>? PaymentTransactions  { get; private set; }

		@Nullable
		private Client.Operator paymentOperator = null;

		public Hbar? QueryPayment { set; private get; }
		public Hbar? MaxQueryPayment { set; private get; }
		public Hbar? ChosenQueryPayment { set; private get; }

		/**
		 * Constructor.
		 */
		Query()
		{
			builder = Proto.Query.newBuilder();
			headerBuilder = QueryHeader.newBuilder();
		}

		/**
		 * Create a payment transaction.
		 *
		 * @param paymentTransactionId      the transaction id
		 * @param nodeId                    the node id
		 * @param operator                  the operator
		 * @param paymentAmount             the amount
		 * @return                          the new payment transaction
		 */
		private static Transaction makePaymentTransaction(TransactionId paymentTransactionId, AccountId nodeId, Client.Operator operator, Hbar paymentAmount)
		{
			return new TransferTransaction()
					.setTransactionId(paymentTransactionId)
					.setNodeAccountIds(Collections.singletonList(nodeId))
					.setMaxTransactionFee(new Hbar(1)) // 1 Hbar
					.AddHbarTransfer(operator.accountId, paymentAmount.negated())
					.AddHbarTransfer(nodeId, paymentAmount)
					.freeze()
					.signWith(operator.publicKey, operator.transactionSigner)
					.makeRequest();
		}

		/**
		 * Set an explicit payment amount for this query.
		 * <p>
		 * The client will submit exactly this amount for the payment of this query. Hedera
		 * will not return any remainder.
		 *
		 * @param queryPayment The explicit payment amount to set
		 * @return {@code this}
		 */
		public T setQueryPayment(Hbar queryPayment)
		{
			this.queryPayment = queryPayment;

			// noinspection unchecked
			return (T)this;
		}
		/**
		 * Set the maximum payment allowable for this query.
		 * <p>
		 * When a query is executed without an explicit {@link Query#setQueryPayment(Hbar)} call,
		 * the client will first request the cost
		 * of the given query from the node it will be submitted to and attach a payment for that amount
		 * from the operator account on the client.
		 * <p>
		 * If the returned value is greater than this value, a
		 * {@link MaxQueryPaymentExceededException} will be thrown from
		 * {@link Query#execute(Client)} or returned in the second callback of
		 * {@link Query#executeAsync(Client, Consumer, Consumer)}.
		 * <p>
		 * Set to 0 to disable automatic implicit payments.
		 *
		 * @param maxQueryPayment The maximum payment amount to set
		 * @return {@code this}
		 */
		public T setMaxQueryPayment(Hbar maxQueryPayment)
		{
			this.maxQueryPayment = maxQueryPayment;

			// noinspection unchecked
			return (T)this;
		}

		/**
		 * Fetch the expected cost.
		 *
		 * @param client                    the client
		 * @return                          the cost in hbar
		 * @         when the transaction times out
		 * @  when the precheck fails
		 */
		public Hbar GetCost(Client client) 
		{
			return GetCost(client, client.getRequestTimeout());
		}
		/**
		* Fetch the expected cost.
		*
		* @param client                    the client
		* @param timeout The timeout after which the execution attempt will be cancelled.
		* @return                          the cost in hbar
		* @         when the transaction times out
		* @  when the precheck fails
		*/
		public Hbar GetCost(Client client, Duration timeout)  
		{
			initWithNodeIds(client);
			
			return GetCostExecutable()
			.setNodeAccountIds(Objects.requireNonNull(getNodeAccountIds()))
			.execute(client, timeout);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @return                          Future result of the cost in hbar
		 */
		public Task<Hbar> GetCostAsync(Client client)
	{
		return GetCostAsync(client, client.getRequestTimeout());
	}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param timeout The timeout after which the execution attempt will be cancelled.
		 * @return                          Future result of the cost in hbar
		 */
		public Task<Hbar> GetCostAsync(Client client, Duration timeout)
		{
			initWithNodeIds(client);
			return GetCostExecutable()
					.setNodeAccountIds(Objects.requireNonNull(getNodeAccountIds()))
					.executeAsync(client, timeout);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param timeout The timeout after which the execution attempt will be cancelled.
		 * @param callback a BiConsumer which handles the result or error.
		 */
		public void GetCostAsync(Client client, Duration timeout, BiConsumer<Hbar, Throwable> callback)
		{
			ConsumerHelper.biConsumer(GetCostAsync(client, timeout), callback);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param callback a BiConsumer which handles the result or error.
		 */
		public void GetCostAsync(Client client, BiConsumer<Hbar, Throwable> callback)
		{
			ConsumerHelper.biConsumer(GetCostAsync(client), callback);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param timeout The timeout after which the execution attempt will be cancelled.
		 * @param onSuccess a Consumer which consumes the result on success.
		 * @param onFailure a Consumer which consumes the error on failure.
		 */
		public void GetCostAsync(Client client, Duration timeout, Consumer<Hbar> onSuccess, Consumer<Throwable> onFailure)
		{
			ConsumerHelper.twoConsumers(GetCostAsync(client, timeout), onSuccess, onFailure);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param onSuccess a Consumer which consumes the result on success.
		 * @param onFailure a Consumer which consumes the error on failure.
		 */
		public void GetCostAsync(Client client, Consumer<Hbar> onSuccess, Consumer<Throwable> onFailure)
		{
			ConsumerHelper.twoConsumers(GetCostAsync(client), onSuccess, onFailure);
		}

		/**
		 * Does this query require a payment?
		 *
		 * @return                          does this query require a payment
		 */
		bool isPaymentRequired()
		{
			// nearly all queries require a payment
			return true;
		}

		/**
		 * Called in {@link #makeRequest} just before the query is built. The intent is for the derived
		 * class to assign their data variant to the query.
		 */
		abstract void onMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header);

		/**
		 * The derived class should access its response header and return.
		 */
		abstract ResponseHeader mapResponseHeader(Response response);

		/**
		 * The derived class should access its request header and return.
		 */
		abstract QueryHeader mapRequestHeader(Proto.Query request);

		/**
		 * Crate the new Query.
		 *
		 * @return                          the new Query
		 */
		private Query<Hbar, QueryCostQuery> GetCostExecutable()
		{
			return new QueryCostQuery();
		}

		/**
		 * Validate the checksums.
		 */
		abstract void validateChecksums(Client client) ;

		/**
		 * Retrieve the operator from the configured client.
		 *
		 * @param client                    the configured client
		 * @return                          the operator
		 */
		Client.Operator getOperatorFromClient(Client client)
		{
			var operator = client.getOperator();

			if (operator == null) {
				throw new IllegalStateException(
						"`client` must have an `operator` or an explicit payment transaction must be provided");
			}

			return operator;
		}

	
		public override void OnExecute(Client client) 
		{
			var grpcCostQuery = new GrpcCostQuery(client);

			if (grpcCostQuery.isNotRequired())
			{
				return;
			}

			if (grpcCostQuery.GetCost() == null)
			{
				grpcCostQuery.setCost(GetCost(client));

				if (grpcCostQuery.shouldError())
				{
					throw grpcCostQuery.mapError();
				}
			}

			grpcCostQuery.finish();
		}
		/**
		 * <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
		 * because it uses features introduced in API level 31 (Android 12).</p>*
		 */
		public override Task OnExecuteAsync(Client client) 
		{
			var grpcCostQuery = new GrpcCostQuery(client);

			if (grpcCostQuery.isNotRequired())
				return Task.CompletedTask;

			return Task.Run(() => { })

			return Task.supplyAsync(
							()-> {
				if (grpcCostQuery.GetCost() == null)
				{
					// No payment was specified so we need to go ask
					// This is a query in its own right so we use a nested future here
					return GetCostAsync(client).thenCompose(cost-> {
						grpcCostQuery.setCost(cost);

						if (grpcCostQuery.shouldError())
						{
							return Task.failedFuture(grpcCostQuery.mapError());
						}

						return Task.completedFuture(null);
					});
				}

				return Task.completedFuture(null);
			},
								client.executor)
						.thenCompose(x->x)
						.thenAccept((paymentAmount)-> {
				grpcCostQuery.finish();
			});
		}

		public override async Task OnExecuteAsync(Client client)
		{
			var grpcCostQuery = new GrpcCostQuery(client);

			if (grpcCostQuery.isNotRequired())
				return Task.CompletedTask;

			// Task.Run replaces CompletableFuture.supplyAsync to offload work to a thread pool
			return Task.Run(async () =>
			{
				if (grpcCostQuery.GetCost() == null)
				{
					// Awaiting GetCostAsync replaces .thenCompose() for flattened async calls
					var cost = await GetCostAsync(client);
					grpcCostQuery.SetCost(cost);

					if (grpcCostQuery.ShouldError())
					{
						// Throwing an exception is the idiomatic way to handle failed futures in C# tasks
						throw grpcCostQuery.MapError();
					}
				}
			});

			grpcCostQuery.Finish();
		}

		private void initWithNodeIds(Client client)
	{
		if (client.isAutoValidateChecksumsEnabled())
		{
			try
			{
				validateChecksums(client);
			}
			catch (BadEntityIdException exc)
			{
				throw new ArgumentException(exc.getMessage());
			}
		}

		if (nodeAccountIds.size() == 0)
		{
			// Get a list of node AccountId's if the user has not set them manually.
			try
			{
				nodeAccountIds.setList(client.network.getNodeAccountIdsForExecute());
			}
			catch (InterruptedException e)
			{
				throw new RuntimeException(e);
			}
		}
	}

		/**
		 * Retrieve the transaction at the given index.
		 *
		 * @param index                     the index
		 * @return                          the transaction
		 */
		Transaction getPaymentTransaction(int index)
		{
			paymentTransactionId = TransactionId.generate(Objects.requireNonNull(paymentOperator).accountId);

			var newPaymentTx = makePaymentTransaction(
					paymentTransactionId,
					nodeAccountIds.get(index),
					paymentOperator,
					Objects.requireNonNull(chosenQueryPayment));
			paymentTransactions.set(index, newPaymentTx);
			return newPaymentTx;
		}

		public override Proto.Query makeRequest() 
		{
			// If payment is required, set the next payment transaction on the query
			if (isPaymentRequired() && paymentTransactions != null) {
				headerBuilder.setPayment(getPaymentTransaction(nodeAccountIds.getIndex()));
			}

			// Delegate to the derived class to apply the header because the common header struct is
			// within the nested type
			onMakeRequest(
					builder, headerBuilder.setResponseType(ResponseType.ANSWER_ONLY).build());

	return builder.build();
		}

		public override Status mapResponseStatus(Response response) {
			var preCheckCode = mapResponseHeader(response).getNodeTransactionPrecheckCode();

	return Status.valueOf(preCheckCode);
		}

		public override TransactionId? getTransactionIdInternal() {
			// this is only called on an error about either the payment transaction or missing a payment transaction
			// as we make sure the latter can't happen, this will never be null
			return paymentTransactionId;
		}

		/**
		 * Extract the transaction id.
		 *
		 * @return                          the transaction id
		 */
		public TransactionId? getPaymentTransactionId()
	{
		return paymentTransactionId;
	}

		/**
	 * Assign the transaction id.
	 *
	 * @param paymentTransactionId      the transaction id
	 * @return {@code this}
	 */
		public T? setPaymentTransactionId(TransactionId paymentTransactionId)
	{
		this.paymentTransactionId = paymentTransactionId;

		// noinspection unchecked
		return (T)this;
	}

		public override string ToString()
		{
			var request = makeRequest();

			StringBuilder builder = new (request.toString().replaceAll("(?m)^# Proto.Query.*", ""));

			var queryHeader = mapRequestHeader(request);
			if (queryHeader.hasPayment())
			{
				builder.append("\n");

				try
				{
					// the replaceAll() is for removing the class name from Transaction Body
					builder.append(
							TransactionBody.Parser.ParseFrom(queryHeader.getPayment().getBodyBytes())
									.toString()
									.replaceAll("(?m)^# Proto.TransactionBuilder.*", ""));
				}
				catch (InvalidProtocolBufferException e)
				{
					throw new RuntimeException(e);
				}
			}

			return builder.toString();
		}

		private class GrpcCostQuery
		{
			private readonly Hbar maxCost;
			private readonly bool notRequired;

			private Client.Operator operator;

			private Hbar cost;

			GrpcCostQuery(Client client)
			{
				Query.this.initWithNodeIds(client);

				cost = Query.this.queryPayment;
				notRequired = (Query.this.paymentTransactions != null) || !Query.this.isPaymentRequired();
				maxCost = MoreObjects.firstNonNull(Query.this.maxQueryPayment, client.defaultMaxQueryPayment);

				if (!notRequired)
				{
						operator = Query.this.getOperatorFromClient(client);
				}
			}

			public Client.Operator getOperator()
			{
				return operator;
			}

			public Hbar GetCost()
			{
				return cost;
			}

			public bool isNotRequired()
			{
				return notRequired;
			}

			GrpcCostQuery setCost(Hbar cost)
			{
				this.cost = cost;
				return this;
			}

			bool shouldError()
			{
				// Check if this is below our configured maximum query payment
				return cost.compareTo(maxCost) > 0;
			}

			MaxQueryPaymentExceededException mapError()
			{
				return new MaxQueryPaymentExceededException(Query.this, cost, maxCost);
			}

			void finish()
			{
				Query.this.chosenQueryPayment = cost;
				Query.this.paymentOperator = operator;
				Query.this.paymentTransactions = new ArrayList<>(Query.this.nodeAccountIds.size());

				for (int i = 0; i < Query.this.nodeAccountIds.size(); i++)
				{
					Query.this.paymentTransactions.Add(null);
				}
			}
		}

		private class QueryCostQuery : Query<Hbar, QueryCostQuery> 
		{
			public override void ValidateChecksums(Client client)  { }
			public override void OnMakeRequest(Proto.Query queryBuilder, QueryHeader header) 
			{
				headerBuilder.setResponseType(ResponseType.COST_ANSWER);

				// COST_ANSWER requires a payment to pass validation but doesn't actually process it
				// yes, this transaction is completely invalid
				// that is okay
				// now go back to sleep
				// without this, an error of MISSING_QUERY_HEADER is returned
				headerBuilder.setPayment(new TransferTransaction()
						.setNodeAccountIds(Collections.singletonList(new AccountId(0, 0, 0)))
						.setTransactionId(TransactionId.withValidStart(new AccountId(0, 0, 0), DateTimeOffset.ofEpochSecond(0)))
						.freeze()
						.makeRequest());

				Query.this.onMakeRequest(queryBuilder, headerBuilder.build());
			}
			public override ResponseHeader MapResponseHeader(Response response) 
			{
				return Query.this.mapResponseHeader(response);
			}
			public override QueryHeader MapRequestHeader(Proto.Query request) 
			{
				return Query.this.mapRequestHeader(request);
			}
			public override Hbar MapResponse(Response response, AccountId nodeId, Proto.Query request) 
			{
				return Hbar.FromTinybars(mapResponseHeader(response).GetCost());
			}
			public override MethodDescriptor<Proto.Query, Response> getMethodDescriptor() 
			{
				return Query.this.getMethodDescriptor();
			}
			public override bool IsPaymentRequired()
			{
				// combo breaker
				return false;
			}
		}
	}
}