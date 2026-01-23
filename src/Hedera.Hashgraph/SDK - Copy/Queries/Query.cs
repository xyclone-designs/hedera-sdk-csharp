// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Base class for all queries that can be submitted to Hedera.
    /// </summary>
    /// <param name="<O>">The output type of the query.</param>
    /// <param name="<T>">The type of the query itself. Used to enable chaining.</param>
    public abstract class Query<O, T> : Executable<T, Proto.Query, Response, O> where T : Query<O, T>
    {
        private readonly Proto.Query.Builder builder;
        private readonly QueryHeader.Builder headerBuilder;
        /// <summary>
        /// The transaction ID
        /// </summary>
        protected TransactionId paymentTransactionId = null;
        /// <summary>
        /// List of payment transactions
        /// </summary>
        protected IList<Transaction> paymentTransactions = null;
        private Client.Operator paymentOperator = null;
        private Hbar queryPayment = null;
        private Hbar maxQueryPayment = null;
        private Hbar chosenQueryPayment = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        Query()
        {
            builder = Proto.Query.NewBuilder();
            headerBuilder = QueryHeader.NewBuilder();
        }

        /// <summary>
        /// Create a payment transaction.
        /// </summary>
        /// <param name="paymentTransactionId">the transaction id</param>
        /// <param name="nodeId">the node id</param>
        /// <param name="operator">the operator</param>
        /// <param name="paymentAmount">the amount</param>
        /// <returns>                         the new payment transaction</returns>
        private static Transaction MakePaymentTransaction(TransactionId paymentTransactionId, AccountId nodeId, Client.Operator @operator, Hbar paymentAmount)
        {
            return new TransferTransaction().SetTransactionId(paymentTransactionId).SetNodeAccountIds(Collections.SingletonList(nodeId)).SetMaxTransactionFee(new Hbar(1)).AddHbarTransfer(@operator.accountId, paymentAmount.Negated()).AddHbarTransfer(nodeId, paymentAmount).Freeze().SignWith(@operator.publicKey, @operator.transactionSigner).MakeRequest();
        }

        /// <summary>
        /// Set an explicit payment amount for this query.
        /// <p>
        /// The client will submit exactly this amount for the payment of this query. Hedera
        /// will not return any remainder.
        /// </summary>
        /// <param name="queryPayment">The explicit payment amount to set</param>
        /// <returns>{@code this}</returns>
        public virtual T SetQueryPayment(Hbar queryPayment)
        {
            this.queryPayment = queryPayment;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Set the maximum payment allowable for this query.
        /// <p>
        /// When a query is executed without an explicit {@link Query#setQueryPayment(Hbar)} call,
        /// the client will first request the cost
        /// of the given query from the node it will be submitted to and attach a payment for that amount
        /// from the operator account on the client.
        /// <p>
        /// If the returned value is greater than this value, a
        /// {@link MaxQueryPaymentExceededException} will be thrown from
        /// {@link Query#execute(Client)} or returned in the second callback of
        /// {@link Query#executeAsync(Client, Consumer, Consumer)}.
        /// <p>
        /// Set to 0 to disable automatic implicit payments.
        /// </summary>
        /// <param name="maxQueryPayment">The maximum payment amount to set</param>
        /// <returns>{@code this}</returns>
        public virtual T SetMaxQueryPayment(Hbar maxQueryPayment)
        {
            this.maxQueryPayment = maxQueryPayment;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Fetch the expected cost.
        /// </summary>
        /// <param name="client">the client</param>
        /// <returns>                         the cost in hbar</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public virtual Hbar GetCost(Client client)
        {
            return GetCost(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Fetch the expected cost.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>                         the cost in hbar</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public virtual Hbar GetCost(Client client, Duration timeout)
        {
            InitWithNodeIds(client);
            return GetCostExecutable().SetNodeAccountIds(Objects.RequireNonNull(GetNodeAccountIds())).Execute(client, timeout);
        }

        /// <summary>
        /// Fetch the expected cost asynchronously.
        /// </summary>
        /// <param name="client">the client</param>
        /// <returns>                         Future result of the cost in hbar</returns>
        public virtual CompletableFuture<Hbar> GetCostAsync(Client client)
        {
            return GetCostAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Fetch the expected cost asynchronously.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>                         Future result of the cost in hbar</returns>
        public virtual CompletableFuture<Hbar> GetCostAsync(Client client, Duration timeout)
        {
            InitWithNodeIds(client);
            return GetCostExecutable().SetNodeAccountIds(Objects.RequireNonNull(GetNodeAccountIds())).ExecuteAsync(client, timeout);
        }

        /// <summary>
        /// Fetch the expected cost asynchronously.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a BiConsumer which handles the result or error.</param>
        public virtual void GetCostAsync(Client client, Duration timeout, BiConsumer<Hbar, Throwable> callback)
        {
            ConsumerHelper.BiConsumer(GetCostAsync(client, timeout), callback);
        }

        /// <summary>
        /// Fetch the expected cost asynchronously.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="callback">a BiConsumer which handles the result or error.</param>
        public virtual void GetCostAsync(Client client, BiConsumer<Hbar, Throwable> callback)
        {
            ConsumerHelper.BiConsumer(GetCostAsync(client), callback);
        }

        /// <summary>
        /// Fetch the expected cost asynchronously.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Consumer which consumes the result on success.</param>
        /// <param name="onFailure">a Consumer which consumes the error on failure.</param>
        public virtual void GetCostAsync(Client client, Duration timeout, Consumer<Hbar> onSuccess, Consumer<Throwable> onFailure)
        {
            ConsumerHelper.TwoConsumers(GetCostAsync(client, timeout), onSuccess, onFailure);
        }

        /// <summary>
        /// Fetch the expected cost asynchronously.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="onSuccess">a Consumer which consumes the result on success.</param>
        /// <param name="onFailure">a Consumer which consumes the error on failure.</param>
        public virtual void GetCostAsync(Client client, Consumer<Hbar> onSuccess, Consumer<Throwable> onFailure)
        {
            ConsumerHelper.TwoConsumers(GetCostAsync(client), onSuccess, onFailure);
        }

        /// <summary>
        /// Does this query require a payment?
        /// </summary>
        /// <returns>                         does this query require a payment</returns>
        virtual bool IsPaymentRequired()
        {

            // nearly all queries require a payment
            return true;
        }

        /// <summary>
        /// Called in {@link #makeRequest} just before the query is built. The intent is for the derived
        /// class to assign their data variant to the query.
        /// </summary>
        abstract void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header);
        /// <summary>
        /// The derived class should access its response header and return.
        /// </summary>
        abstract ResponseHeader MapResponseHeader(Response response);
        /// <summary>
        /// The derived class should access its request header and return.
        /// </summary>
        abstract QueryHeader MapRequestHeader(Proto.Query request);
        /// <summary>
        /// Crate the new Query.
        /// </summary>
        /// <returns>                         the new Query</returns>
        private Query<Hbar, QueryCostQuery> GetCostExecutable()
        {
            return new QueryCostQuery();
        }

        /// <summary>
        /// Validate the checksums.
        /// </summary>
        abstract void ValidateChecksums(Client client);
        /// <summary>
        /// Retrieve the operator from the configured client.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <returns>                         the operator</returns>
        virtual Client.Operator GetOperatorFromClient(Client client)
        {
            var operator = client.GetOperator();
            if (@operator == null)
            {
                throw new InvalidOperationException("`client` must have an `operator` or an explicit payment transaction must be provided");
            }

            return @operator;
        }

        override void OnExecute(Client client)
        {
            var grpcCostQuery = new GrpcCostQuery(client);
            if (grpcCostQuery.IsNotRequired())
            {
                return;
            }

            if (grpcCostQuery.GetCost() == null)
            {
                grpcCostQuery.SetCost(GetCost(client));
                if (grpcCostQuery.ShouldError())
                {
                    throw grpcCostQuery.MapError();
                }
            }

            grpcCostQuery.Finish();
        }

        /// <summary>
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
        /// because it uses features introduced in API level 31 (Android 12).</p>*
        /// </summary>
        override CompletableFuture<Void> OnExecuteAsync(Client client)
        {
            var grpcCostQuery = new GrpcCostQuery(client);
            if (grpcCostQuery.IsNotRequired())
            {
                return CompletableFuture.CompletedFuture(null);
            }

            return CompletableFuture.SupplyAsync(() =>
            {
                if (grpcCostQuery.GetCost() == null)
                {

                    // No payment was specified so we need to go ask
                    // This is a query in its own right so we use a nested future here
                    return GetCostAsync(client).ThenCompose((cost) =>
                    {
                        grpcCostQuery.SetCost(cost);
                        if (grpcCostQuery.ShouldError())
                        {
                            return CompletableFuture.FailedFuture(grpcCostQuery.MapError());
                        }

                        return CompletableFuture.CompletedFuture(null);
                    });
                }

                return CompletableFuture.CompletedFuture(null);
            }, client.executor).ThenCompose((x) => x).ThenAccept((paymentAmount) =>
            {
                grpcCostQuery.Finish();
            });
        }

        private void InitWithNodeIds(Client client)
        {
            if (client.IsAutoValidateChecksumsEnabled())
            {
                try
                {
                    ValidateChecksums(client);
                }
                catch (BadEntityIdException exc)
                {
                    throw new ArgumentException(exc.GetMessage());
                }
            }

            if (nodeAccountIds.Count == 0)
            {

                // Get a list of node AccountId's if the user has not set them manually.
                try
                {
                    nodeAccountIds.SetList(client.network.GetNodeAccountIdsForExecute());
                }
                catch (InterruptedException e)
                {
                    throw new Exception(e);
                }
            }
        }

        /// <summary>
        /// Retrieve the transaction at the given index.
        /// </summary>
        /// <param name="index">the index</param>
        /// <returns>                         the transaction</returns>
        virtual Transaction GetPaymentTransaction(int index)
        {
            paymentTransactionId = TransactionId.Generate(Objects.RequireNonNull(paymentOperator).accountId);
            var newPaymentTx = MakePaymentTransaction(paymentTransactionId, nodeAccountIds[index], paymentOperator, Objects.RequireNonNull(chosenQueryPayment));
            paymentTransactions[index] = newPaymentTx;
            return newPaymentTx;
        }

        override Proto.Query MakeRequest()
        {

            // If payment is required, set the next payment transaction on the query
            if (IsPaymentRequired() && paymentTransactions != null)
            {
                headerBuilder.SetPayment(GetPaymentTransaction(nodeAccountIds.GetIndex()));
            }


            // Delegate to the derived class to apply the header because the common header struct is
            // within the nested type
            OnMakeRequest(builder, headerBuilder.SetResponseType(ResponseType.ANSWER_ONLY).Build());
            return proto;
        }

        override Status MapResponseStatus(Response response)
        {
            var preCheckCode = MapResponseHeader(response).GetNodeTransactionPrecheckCode();
            return Status.ValueOf(preCheckCode);
        }

        override TransactionId GetTransactionIdInternal()
        {

            // this is only called on an error about either the payment transaction or missing a payment transaction
            // as we make sure the latter can't happen, this will never be null
            return paymentTransactionId;
        }

        /// <summary>
        /// Extract the transaction id.
        /// </summary>
        /// <returns>                         the transaction id</returns>
        public virtual TransactionId GetPaymentTransactionId()
        {
            return paymentTransactionId;
        }

        /// <summary>
        /// Assign the transaction id.
        /// </summary>
        /// <param name="paymentTransactionId">the transaction id</param>
        /// <returns>{@code this}</returns>
        public virtual T SetPaymentTransactionId(TransactionId paymentTransactionId)
        {
            this.paymentTransactionId = paymentTransactionId;

            // noinspection unchecked
            return (T)this;
        }

        public override string ToString()
        {
            var request = MakeRequest();
            StringBuilder builder = new StringBuilder(request.ToString().ReplaceAll("(?m)^# Proto.Query.*", ""));
            var queryHeader = MapRequestHeader(request);
            if (queryHeader.HasPayment())
            {
                builder.Append("\n");
                try
                {

                    // the replaceAll() is for removing the class name from Transaction Body
                    builder.Append(TransactionBody.ParseFrom(queryHeader.GetPayment().GetBodyBytes()).ToString().ReplaceAll("(?m)^# Proto.TransactionBuilder.*", ""));
                }
                catch (InvalidProtocolBufferException e)
                {
                    throw new Exception(e);
                }
            }

            return builder.ToString();
        }

        private class GrpcCostQuery
        {
            private readonly Hbar maxCost;
            private readonly bool notRequired;
            private Client.Operator operator;
            private Hbar cost;
            GrpcCostQuery(Client client)
            {
                this.InitWithNodeIds(client);
                cost = this.queryPayment;
                notRequired = (this.paymentTransactions != null) || !this.IsPaymentRequired();
                maxCost = MoreObjects.FirstNonNull(this.maxQueryPayment, client.defaultMaxQueryPayment);
                if (!notRequired)
                {
                    @operator = this.GetOperatorFromClient(client);
                }
            }

            public virtual Client.Operator GetOperator()
            {
                return @operator;
            }

            public virtual Hbar GetCost()
            {
                return cost;
            }

            public virtual bool IsNotRequired()
            {
                return notRequired;
            }

            virtual GrpcCostQuery SetCost(Hbar cost)
            {
                this.cost = cost;
                return this;
            }

            virtual bool ShouldError()
            {

                // Check if this is below our configured maximum query payment
                return cost.CompareTo(maxCost) > 0;
            }

            virtual MaxQueryPaymentExceededException MapError()
            {
                return new MaxQueryPaymentExceededException(this, cost, maxCost);
            }

            virtual void Finish()
            {
                this.chosenQueryPayment = cost;
                this.paymentOperator = @operator;
                this.paymentTransactions = new List(this.nodeAccountIds.Count);
                for (int i = 0; i < this.nodeAccountIds.Count; i++)
                {
                    this.paymentTransactions.Add(null);
                }
            }
        }

        private class QueryCostQuery : Query<Hbar, QueryCostQuery>
        {
            override void ValidateChecksums(Client client)
            {
            }

            override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
            {
                headerBuilder.SetResponseType(ResponseType.COST_ANSWER);

                // COST_ANSWER requires a payment to pass validation but doesn't actually process it
                // yes, this transaction is completely invalid
                // that is okay
                // now go back to sleep
                // without this, an error of MISSING_QUERY_HEADER is returned
                headerBuilder.SetPayment(new TransferTransaction().SetNodeAccountIds(Collections.SingletonList(new AccountId(0, 0, 0))).SetTransactionId(TransactionId.WithValidStart(new AccountId(0, 0, 0), Instant.OfEpochSecond(0))).Freeze().MakeRequest());
                this.OnMakeRequest(queryBuilder, headerBuilder.Build());
            }

            override ResponseHeader MapResponseHeader(Response response)
            {
                return this.MapResponseHeader(response);
            }

            override QueryHeader MapRequestHeader(Proto.Query request)
            {
                return this.MapRequestHeader(request);
            }

            override Hbar MapResponse(Response response, AccountId nodeId, Proto.Query request)
            {
                return Hbar.FromTinybars(MapResponseHeader(response).GetCost());
            }

            override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
            {
                return this.GetMethodDescriptor();
            }

            override bool IsPaymentRequired()
            {

                // combo breaker
                return false;
            }
        }
    }
}