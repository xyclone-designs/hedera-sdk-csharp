using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Queries
{
	/**
	 * Base class for all queries that can be submitted to Hedera.
	 *
	 * @param <O> The output type of the query.
	 * @param <T> The type of the query itself. Used to enable chaining.
	 */
	public abstract partial class Query<O, T> : Executable<T, Proto.Query, Proto.Response, O> where T : Query<O, T>
	{
		private readonly Proto.Query _ProtoQuery = new ();
		private readonly Proto.QueryHeader _ProtoQueryHeader = new ();

		/**
		 * Constructor.
		 */
		protected Query()
		{
			_ProtoQuery = new Proto.Query { };
			_ProtoQueryHeader = new Proto.QueryHeader { };
		}

		/**
		 * Retrieve the op from the configured client.
		 *
		 * @param client                    the configured client
		 * @return                          the op
		 */
		private static Client.Operator GetOperatorFromClient(Client client)
		{
			return
				client.Oper8r ??
				throw new InvalidOperationException("`client` must have an `op` or an explicit payment transaction must be provided");
		}
		/**
		 * Create a payment transaction.
		 *
		 * @param paymentTransactionId      the transaction id
		 * @param nodeId                    the node id
		 * @param op                  the op
		 * @param paymentAmount             the amount
		 * @return                          the new payment transaction
		 */
		private static Proto.Transaction MakePaymentTransaction(TransactionId paymentTransactionId, AccountId nodeId, Client.Operator operator_, Hbar paymentAmount)
		{
			return new TransferTransaction
			{
				TransactionId = paymentTransactionId,
				NodeAccountIds = [nodeId],
				MaxTransactionFee = new Hbar(1),

			}.SignWith(operator_.PublicKey, operator_.TransactionSigner).MakeRequest();
		}

		private Client.Operator? PaymentOperator { get; set; }

		public override TransactionId TransactionIdInternal
		{
			// this is only called on an error about either the payment transaction or missing a payment transaction
			// as we make sure the latter can't happen, this will never be null
			get => PaymentTransactionId;
		}

		/**
		 * The transaction ID
		 */
		public TransactionId? PaymentTransactionId { get; set; }
		/**
		 * List of payment transactions
		 */
		public List<Proto.Transaction>? PaymentTransactions { get; internal set; }

		/**
		 * Set an explicit payment amount for this query.
		 * <p>
		 * The client will submit exactly this amount for the payment of this query. Hedera
		 * will not return any remainder.
		 *
		 * @param queryPayment The explicit payment amount to set
		 * @return {@code this}
		 */
		public Hbar? QueryPayment { set; private get; }
		/**
		 * Set the maximum payment allowable for this query.
		 * <p>
		 * When a query is executed without an explicit {@link Query#setQueryPayment(Hbar)} call,
		 * the client will first request the cost
		 * of the given query from the node it will be submitted to and attach a payment for that amount
		 * from the op account on the client.
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
		public Hbar? MaxQueryPayment { set; private get; }
		public Hbar? ChosenQueryPayment { set; private get; }
		/**
		 * Does this query require a payment?
		 *
		 * default to true as nearly all queries require a payment
		 */
		public virtual bool IsPaymentRequired { get; } = true;

		private void InitWithNodeIds(Client client)
		{
			if (client.AutoValidateChecksums)
			{
				try
				{
					ValidateChecksums(client);
				}
				catch (BadEntityIdException exc)
				{
					throw new ArgumentException(exc.Message);
				}
			}

			if (NodeAccountIds.Count == 0)
			{
				// Get a list of node AccountId's if the user has not set them manually.
				try
				{
					NodeAccountIds.SetList(client.Network.NodeAccountIdsForExecute);
				}
				catch (ThreadInterruptedException e)
				{
					throw new RuntimeWrappedException(e);
				}
			}
		}
		/**
		 * Retrieve the transaction at the given index.
		 *
		 * @param index                     the index
		 * @return                          the transaction
		 */
		private Proto.Transaction GetPaymentTransaction(int index)
		{
			PaymentTransactionId = TransactionId.Generate(PaymentOperator.AccountId);

			Proto.Transaction newPaymentTx = MakePaymentTransaction(PaymentTransactionId, NodeAccountIds.ElementAt(index), PaymentOperator, ChosenQueryPayment);

			PaymentTransactions[index] = newPaymentTx;

			return newPaymentTx;
		}

		/**
		 * Validate the checksums.
		 */
		public abstract void ValidateChecksums(Client client);
		/**
		 * The derived class should access its request header and return.
		 */
		public abstract Proto.QueryHeader MapRequestHeader(Proto.Query request);
		/**
		 * The derived class should access its response header and return.
		 */
		public abstract Proto.ResponseHeader MapResponseHeader(Proto.Response response);
		/**
		 * Called in {@link #makeRequest} just before the query is built. The intent is for the derived
		 * class to assign their data variant to the query.
		 */
		public abstract void OnMakeRequest(Proto.Query query, Proto.QueryHeader header);

		/**
		 * Fetch the expected cost.
		 *
		 * @param client                    the client
		 * @return                          the cost in hbar
		 * @         when the transaction times out
		 * @  when the precheck fails
		 */
		public virtual Hbar GetCost(Client client)
		{
			return GetCost(client, client.RequestTimeout);
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
		public virtual Hbar GetCost(Client client, Duration timeout)
		{
			InitWithNodeIds(client);

			return new QueryCostQuery
			{
				NodeAccountIds = [.. NodeAccountIds ?? []]

			}.Execute(client, timeout);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @return                          Future result of the cost in hbar
		 */
		public virtual Task<Hbar> GetCostAsync(Client client)
		{
			return GetCostAsync(client, client.RequestTimeout);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param timeout The timeout after which the execution attempt will be cancelled.
		 * @return                          Future result of the cost in hbar
		 */
		public virtual Task<Hbar> GetCostAsync(Client client, Duration timeout)
		{
			InitWithNodeIds(client);

			return new QueryCostQuery
			{
				NodeAccountIds = [.. NodeAccountIds ?? []]

			}.ExecuteAsync(client, timeout);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param timeout The timeout after which the execution attempt will be cancelled.
		 * @param callback a BiConsumer which handles the result or error.
		 */
		public virtual void GetCostAsync(Client client, Duration timeout, Action<Hbar, Exception> callback)
		{
			ActionHelper.BiConsumer(GetCostAsync(client, timeout), callback);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param callback a BiConsumer which handles the result or error.
		 */
		public virtual void GetCostAsync(Client client, Action<Hbar, Exception> callback)
		{
			ActionHelper.BiConsumer(GetCostAsync(client), callback);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param timeout The timeout after which the execution attempt will be cancelled.
		 * @param onSuccess a Consumer which consumes the result on success.
		 * @param onFailure a Consumer which consumes the error on failure.
		 */
		public virtual void GetCostAsync(Client client, Duration timeout, Action<Hbar> onSuccess, Action<Exception> onFailure)
		{
			ActionHelper.TwoActions(GetCostAsync(client, timeout), onSuccess, onFailure);
		}
		/**
		 * Fetch the expected cost asynchronously.
		 *
		 * @param client                    the client
		 * @param onSuccess a Consumer which consumes the result on success.
		 * @param onFailure a Consumer which consumes the error on failure.
		 */
		public virtual void GetCostAsync(Client client, Action<Hbar> onSuccess, Action<Exception> onFailure)
		{
			ActionHelper.TwoActions(GetCostAsync(client), onSuccess, onFailure);
		}

		public override Proto.Query MakeRequest()
		{
			// If payment is required, set the next payment transaction on the query
			if (IsPaymentRequired && PaymentTransactions != null)
			{
				_ProtoQueryHeader.Payment = GetPaymentTransaction(NodeAccountIds.Index);
			}

			// Delegate to the derived class to apply the header because the common header struct is
			// within the nested type

			_ProtoQueryHeader.ResponseType = Proto.ResponseType.AnswerOnly;

			OnMakeRequest(_ProtoQuery, _ProtoQueryHeader);

			return _ProtoQuery;
		}
		public override void OnExecute(Client client)
		{
			var grpcCostQuery = new GrpcCostQuery(client, this);

			if (grpcCostQuery.NotRequired)
				return;

			if (grpcCostQuery.Cost == null)
			{
				grpcCostQuery.Cost = GetCost(client);

				if (grpcCostQuery.ShouldError())
					throw grpcCostQuery.MapError();
			}

			grpcCostQuery.Finish();
		}
		public override Task OnExecuteAsync(Client client)
		{
			var grpcCostQuery = new GrpcCostQuery(client, this);

			if (grpcCostQuery.NotRequired)
				return Task.CompletedTask;

			// Task.Run replaces CompletableFuture.supplyAsync to offload work to a thread pool
			return Task.Run(async () =>
			{
				if (grpcCostQuery.Cost == null)
				{
					// Awaiting GetCostAsync replaces.thenCompose() for flattened async calls
					var cost = await GetCostAsync(client);
					grpcCostQuery.Cost = cost;

					if (grpcCostQuery.ShouldError())
					{
						// Throwing an exception is the idiomatic way to handle failed futures in C# tasks
						throw grpcCostQuery.MapError();
					}
				}
			}).ContinueWith(_ => grpcCostQuery.Finish());
		}
		public override ResponseStatus MapResponseStatus(Proto.Response response)
		{
			var preCheckCode = MapResponseHeader(response).NodeTransactionPrecheckCode;

			return (ResponseStatus)preCheckCode;
		}
		public override string ToString()
		{
			Proto.Query query = MakeRequest();
			StringBuilder builder = new (Regex.Replace(query.ToString(), @"^# Proto\.Query.*", "", RegexOptions.Multiline));
			Proto.QueryHeader queryHeader = MapRequestHeader(query);

			if (queryHeader.Payment is not null)
			{
				builder.Append('\n');

				try
				{
					// the replaceAll() is for removing the class name from Transaction Body

					string transactionbody = Regex.Replace(
						replacement: string.Empty, 
						options: RegexOptions.Multiline,
						pattern: @"(?m)^# Proto.TransactionBuilder.*", 
						input: Proto.TransactionBody.Parser
							.ParseFrom(queryHeader.Payment.BodyBytes)
							.ToString());

					builder.Append(transactionbody);
				}
				catch (InvalidProtocolBufferException e)
				{
					throw new RuntimeWrappedException(e);
				}
			}

			return builder.ToString();
		}
	}
}