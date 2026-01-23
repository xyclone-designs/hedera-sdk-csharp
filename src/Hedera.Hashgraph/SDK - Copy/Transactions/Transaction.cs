// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK;
using Java.Lang.Reflect;
using Java.Time;
using Java;
using Java.Util.Concurrent;
using Java.Util.Function;
using Java.Util.Stream;
using Javax.Annotation;
using Org.Bouncycastle.Crypto.Digests;
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
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Base class for all transactions that may be built and submitted to Hedera.
    /// </summary>
    /// <param name="<T>">The type of the transaction. Used to enable chaining.</param>
    public abstract class Transaction<T> : Executable<T, Proto.Transaction, Proto.TransactionResponse, TransactionResponse> where T : Transaction<T>
    {
        /// <summary>
        /// Default auto renew duration for accounts, contracts, topics, and files (entities)
        /// </summary>
        static readonly Duration DEFAULT_AUTO_RENEW_PERIOD = Duration.OfDays(90);
        /// <summary>
        /// Dummy account ID used to assist in deserializing incomplete Transactions.
        /// </summary>
        protected static readonly AccountId DUMMY_ACCOUNT_ID = new AccountId(0, 0, 0);
        /// <summary>
        /// Dummy transaction ID used to assist in deserializing incomplete Transactions.
        /// </summary>
        protected static readonly TransactionId DUMMY_TRANSACTION_ID = TransactionId.WithValidStart(DUMMY_ACCOUNT_ID, Timestamp.EPOCH);
        /// <summary>
        /// Default transaction duration
        /// </summary>
        private static readonly Duration DEFAULT_TRANSACTION_VALID_DURATION = Duration.OfSeconds(120);
        private static readonly string ATOMIC_BATCH_NODE_ACCOUNT_ID = "0.0.0";
        /// <summary>
        /// Transaction constructors end their work by setting sourceTransactionBody. The expectation is that the Transaction
        /// subclass constructor will pick up where the Transaction superclass constructor left off, and will unpack the data
        /// in the transaction body.
        /// </summary>
        protected readonly TransactionBody sourceTransactionBody;
        /// <summary>
        /// The builder that gets re-used to build each outer transaction. freezeWith() will create the frozenBodyBuilder.
        /// The presence of frozenBodyBuilder indicates that this transaction is frozen.
        /// </summary>
        protected TransactionBody.Builder frozenBodyBuilder = null;
        /// <summary>
        /// An SDK [Transaction] is composed of multiple, raw protobuf transactions. These should be functionally identical,
        /// except pointing to different nodes. When retrying a transaction after a network error or retry-able status
        /// response, we try a different transaction and thus a different node.
        /// </summary>
        protected List<Proto.Transaction> outerTransactions = Collections.EmptyList();
        /// <summary>
        /// An SDK [Transaction] is composed of multiple, raw protobuf transactions. These should be functionally identical,
        /// except pointing to different nodes. When retrying a transaction after a network error or retry-able status
        /// response, we try a different transaction and thus a different node.
        /// </summary>
        protected List<Proto.SignedTransaction.Builder> innerSignedTransactions = Collections.EmptyList();
        /// <summary>
        /// A set of signatures corresponding to every unique public key used to sign the transaction.
        /// </summary>
        protected List<SignatureMap.Builder> sigPairLists = Collections.EmptyList();
        /// <summary>
        /// List of IDs for the transaction based on the operator because the transaction ID includes the operator's account
        /// </summary>
        protected LockableList<TransactionId> transactionIds = new LockableList();
        /// <summary>
        /// publicKeys and signers are parallel Array. If the signer associated with a public key is null, that means that
        /// the private key associated with that public key has already contributed a signature to sigPairListBuilders, but
        /// the signer is not available (likely because this came from fromBytes())
        /// </summary>
        protected IList<PublicKey> publicKeys = new ();
        /// <summary>
        /// publicKeys and signers are parallel Array. If the signer associated with a public key is null, that means that
        /// the private key associated with that public key has already contributed a signature to sigPairListBuilders, but
        /// the signer is not available (likely because this came from fromBytes())
        /// </summary>
        protected List<Function<byte[], byte[]>> signers = new ();
        /// <summary>
        /// The maximum transaction fee the client is willing to pay
        /// </summary>
        protected Hbar defaultMaxTransactionFee = new Hbar(2);
        /// <summary>
        /// Should the transaction id be regenerated
        /// </summary>
        protected bool regenerateTransactionId = null;
        private Duration transactionValidDuration;
        private Hbar maxTransactionFee = null;
        private string memo = "";
        IList<CustomFeeLimit> customFeeLimits = new ();
        private Key batchKey = null;
		/// <summary>
		/// Constructor.
		/// </summary>
		protected Transaction()
        {
            SetTransactionValidDuration(DEFAULT_TRANSACTION_VALID_DURATION);
            sourceTransactionBody = TransactionBody.GetDefaultInstance();
        }

        // This constructor is used to construct from a scheduled transaction body
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        protected Transaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)
        {
            LinkedHashMap<AccountId, Proto.Transaction> transactionMap = txs.Values().Iterator().Next();
            if (!transactionMap.IsEmpty() && transactionMap.KeySet().Iterator().Next().Equals(DUMMY_ACCOUNT_ID) && batchKey != null)
            {

                // If the first account ID is a dummy account ID, then only the source TransactionBody needs to be copied.
                var signedTransaction = SignedTransaction.ParseFrom(transactionMap.Values().Iterator().Next().GetSignedTransactionBytes());
                sourceTransactionBody = ParseTransactionBody(signedTransaction.GetBodyBytes());
            }
            else
            {
                var txCount = txs.KeySet().Count;
                var nodeCount = txs.Values().Iterator().Next().Count;
                nodeAccountIds.EnsureCapacity(nodeCount);
                sigPairLists = new List(nodeCount * txCount);
                outerTransactions = new List(nodeCount * txCount);
                innerSignedTransactions = new List(nodeCount * txCount);
                transactionIds.EnsureCapacity(txCount);
                foreach (var transactionEntry in txs.EntrySet())
                {
                    if (!transactionEntry.GetKey().Equals(DUMMY_TRANSACTION_ID))
                    {
                        transactionIds.Add(transactionEntry.GetKey());
                    }

                    foreach (var nodeEntry in transactionEntry.GetValue().EntrySet())
                    {
                        if (nodeAccountIds.Count != nodeCount)
                        {
                            nodeAccountIds.Add(nodeEntry.GetKey());
                        }

                        var transaction = SignedTransaction.ParseFrom(nodeEntry.GetValue().GetSignedTransactionBytes());
                        outerTransactions.Add(nodeEntry.GetValue());
                        sigPairLists.Add(transaction.GetSigMap().ToBuilder());
                        innerSignedTransactions.Add(transaction.ToBuilder());
                        if (publicKeys.IsEmpty())
                        {
                            foreach (var sigPair in transaction.GetSigMap().GetSigPairList())
                            {
                                publicKeys.Add(PublicKey.FromBytes(sigPair.GetPubKeyPrefix().ToByteArray()));
                                signers.Add(null);
                            }
                        }
                    }
                }

                nodeAccountIds.Remove(new AccountId(0, 0, 0));

                // Verify that transaction bodies match
                for (int i = 0; i < txCount; i++)
                {
                    TransactionBody firstTxBody = null;
                    for (int j = 0; j < nodeCount; j++)
                    {
                        int k = i * nodeCount + j;
                        var txBody = ParseTransactionBody(innerSignedTransactions[k].GetBodyBytes());
                        if (firstTxBody == null)
                        {
                            firstTxBody = txBody;
                        }
                        else
                        {
                            RequireProtoMatches(firstTxBody, txBody, new HashSet(List.Of("NodeAccountID")), "TransactionBody");
                        }
                    }
                }

                sourceTransactionBody = ParseTransactionBody(innerSignedTransactions[0].GetBodyBytes());
            }

            SetTransactionValidDuration(Utils.DurationConverter.FromProtobuf(sourceTransactionBody.GetTransactionValidDuration()));
            SetMaxTransactionFee(Hbar.FromTinybars(sourceTransactionBody.GetTransactionFee()));
            SetTransactionMemo(sourceTransactionBody.GetMemo());
            customFeeLimits = sourceTransactionBody.GetMaxCustomFeesList().Stream().Map(CustomFeeLimit.FromProtobuf()).ToList();
            batchKey = Key.FromProtobufKey(sourceTransactionBody.GetBatchKey());

            // The presence of signatures implies the Transaction should be frozen.
            if (!publicKeys.IsEmpty())
            {
                frozenBodyBuilder = sourceTransactionBody.ToBuilder();
            }
        }
        protected Transaction(Proto.TransactionBody txBody)
        {
            SetTransactionValidDuration(DEFAULT_TRANSACTION_VALID_DURATION);
            SetMaxTransactionFee(Hbar.FromTinybars(txBody.GetTransactionFee()));
            SetTransactionMemo(txBody.GetMemo());
            sourceTransactionBody = txBody;
        }

        // This constructor is used to construct via fromBytes
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        protected Transaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)
        {
            LinkedHashMap<AccountId, Proto.Transaction> transactionMap = txs.Values().Iterator().Next();
            if (!transactionMap.IsEmpty() && transactionMap.KeySet().Iterator().Next().Equals(DUMMY_ACCOUNT_ID) && batchKey != null)
            {

                // If the first account ID is a dummy account ID, then only the source TransactionBody needs to be copied.
                var signedTransaction = SignedTransaction.ParseFrom(transactionMap.Values().Iterator().Next().GetSignedTransactionBytes());
                sourceTransactionBody = ParseTransactionBody(signedTransaction.GetBodyBytes());
            }
            else
            {
                var txCount = txs.KeySet().Count;
                var nodeCount = txs.Values().Iterator().Next().Count;
                nodeAccountIds.EnsureCapacity(nodeCount);
                sigPairLists = new List(nodeCount * txCount);
                outerTransactions = new List(nodeCount * txCount);
                innerSignedTransactions = new List(nodeCount * txCount);
                transactionIds.EnsureCapacity(txCount);
                foreach (var transactionEntry in txs.EntrySet())
                {
                    if (!transactionEntry.GetKey().Equals(DUMMY_TRANSACTION_ID))
                    {
                        transactionIds.Add(transactionEntry.GetKey());
                    }

                    foreach (var nodeEntry in transactionEntry.GetValue().EntrySet())
                    {
                        if (nodeAccountIds.Count != nodeCount)
                        {
                            nodeAccountIds.Add(nodeEntry.GetKey());
                        }

                        var transaction = SignedTransaction.ParseFrom(nodeEntry.GetValue().GetSignedTransactionBytes());
                        outerTransactions.Add(nodeEntry.GetValue());
                        sigPairLists.Add(transaction.GetSigMap().ToBuilder());
                        innerSignedTransactions.Add(transaction.ToBuilder());
                        if (publicKeys.IsEmpty())
                        {
                            foreach (var sigPair in transaction.GetSigMap().GetSigPairList())
                            {
                                publicKeys.Add(PublicKey.FromBytes(sigPair.GetPubKeyPrefix().ToByteArray()));
                                signers.Add(null);
                            }
                        }
                    }
                }

                nodeAccountIds.Remove(new AccountId(0, 0, 0));

                // Verify that transaction bodies match
                for (int i = 0; i < txCount; i++)
                {
                    TransactionBody firstTxBody = null;
                    for (int j = 0; j < nodeCount; j++)
                    {
                        int k = i * nodeCount + j;
                        var txBody = ParseTransactionBody(innerSignedTransactions[k].GetBodyBytes());
                        if (firstTxBody == null)
                        {
                            firstTxBody = txBody;
                        }
                        else
                        {
                            RequireProtoMatches(firstTxBody, txBody, new HashSet(List.Of("NodeAccountID")), "TransactionBody");
                        }
                    }
                }

                sourceTransactionBody = ParseTransactionBody(innerSignedTransactions[0].GetBodyBytes());
            }

            SetTransactionValidDuration(Utils.DurationConverter.FromProtobuf(sourceTransactionBody.GetTransactionValidDuration()));
            SetMaxTransactionFee(Hbar.FromTinybars(sourceTransactionBody.GetTransactionFee()));
            SetTransactionMemo(sourceTransactionBody.GetMemo());
            customFeeLimits = sourceTransactionBody.GetMaxCustomFeesList().Stream().Map(CustomFeeLimit.FromProtobuf()).ToList();
            batchKey = Key.FromProtobufKey(sourceTransactionBody.GetBatchKey());

            // The presence of signatures implies the Transaction should be frozen.
            if (!publicKeys.IsEmpty())
            {
                frozenBodyBuilder = sourceTransactionBody.ToBuilder();
            }
        }

        /// <summary>
        /// Create the correct transaction from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>the new transaction</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static Transaction<TWildcardTodo> FromBytes(byte[] bytes)
        {
            var list = TransactionList.Parser.ParseFrom(bytes);
            var txsMap = new LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>>();
            TransactionBody.DataCase dataCase;
            if (!list.GetTransactionListList().IsEmpty())
            {
                dataCase = ProcessTransactionList(list.GetTransactionListList(), txsMap);
            }
            else
            {
                dataCase = ProcessSingleTransaction(bytes, txsMap);
            }

            return CreateTransactionFromDataCase(dataCase, txsMap);
        }

        /// <summary>
        /// Process a single transaction
        /// </summary>
        private static TransactionBody.DataCase ProcessSingleTransaction(byte[] bytes, LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txsMap)
        {
            var transaction = Proto.Transaction.Parser.ParseFrom(bytes);
            var builtTransaction = PrepareSingleTransaction(transaction);
            var signedTransaction = SignedTransaction.ParseFrom(builtTransaction.GetSignedTransactionBytes());
            var txBody = TransactionBody.ParseFrom(signedTransaction.GetBodyBytes());
            AddTransactionToMap(builtTransaction, txBody, txsMap);
            return txBody.GetDataCase();
        }

        /// <summary>
        /// Prepare a single transaction by ensuring it has SignedTransactionBytes
        /// </summary>
        private static Proto.Transaction PrepareSingleTransaction(Proto.Transaction transaction)
        {
            if (transaction.GetSignedTransactionBytes().IsEmpty())
            {
                var txBuilder = transaction.ToBuilder();
                var bodyBytes = txBuilder.GetBodyBytes();
                var sigMap = txBuilder.GetSigMap();
                txBuilder.SetSignedTransactionBytes(SignedTransaction.NewBuilder().SetBodyBytes(bodyBytes).SetSigMap(sigMap).Build().ToByteString()).ClearBodyBytes().ClearSigMap();
                return txBuilder.Build();
            }

            return transaction;
        }

        /// <summary>
        /// Process a list of transactions with integrity verification
        /// </summary>
        private static TransactionBody.DataCase ProcessTransactionList(List<Proto.Transaction> transactionList, LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txsMap)
        {
            if (transactionList.IsEmpty())
            {
                return TransactionBody.DataCase.DATA_NOT_SET;
            }

            var firstTransaction = transactionList[0];
            var firstSignedTransaction = SignedTransaction.ParseFrom(firstTransaction.GetSignedTransactionBytes());
            var firstTxBody = TransactionBody.ParseFrom(firstSignedTransaction.GetBodyBytes());
            var dataCase = firstTxBody.GetDataCase();
            foreach (Proto.Transaction transaction in transactionList)
            {
                var signedTransaction = SignedTransaction.ParseFrom(transaction.GetSignedTransactionBytes());
                var txBody = TransactionBody.ParseFrom(signedTransaction.GetBodyBytes());
                AddTransactionToMap(transaction, txBody, txsMap);
            }

            return dataCase;
        }

        /// <summary>
        /// Add a transaction to the transaction map
        /// </summary>
        private static void AddTransactionToMap(Proto.Transaction transaction, TransactionBody txBody, LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txsMap)
        {
            var account = txBody.HasNodeAccountID() ? AccountId.FromProtobuf(txBody.GetNodeAccountID()) : DUMMY_ACCOUNT_ID;
            var transactionId = txBody.HasTransactionID() ? TransactionId.FromProtobuf(txBody.GetTransactionID()) : DUMMY_TRANSACTION_ID;
            var linked = txsMap.ContainsKey(transactionId) ? Objects.RequireNonNull(txsMap[transactionId]) : new LinkedHashMap<AccountId, Proto.Transaction>();
            linked.Put(account, transaction);
            txsMap.Put(transactionId, linked);
        }

        /// <summary>
        /// Creates the appropriate transaction type based on the data case.
        /// </summary>
        private static Transaction<TWildcardTodo> CreateTransactionFromDataCase(TransactionBody.DataCase dataCase, LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)
        {
            return dataCase switch
            {
                CONTRACTCALL => new ContractExecuteTransaction(txs),
                CONTRACTCREATEINSTANCE => new ContractCreateTransaction(txs),
                CONTRACTUPDATEINSTANCE => new ContractUpdateTransaction(txs),
                CONTRACTDELETEINSTANCE => new ContractDeleteTransaction(txs),
                ETHEREUMTRANSACTION => new EthereumTransaction(txs),
                CRYPTOADDLIVEHASH => new LiveHashAddTransaction(txs),
                CRYPTOCREATEACCOUNT => new AccountCreateTransaction(txs),
                CRYPTODELETE => new AccountDeleteTransaction(txs),
                CRYPTODELETELIVEHASH => new LiveHashDeleteTransaction(txs),
                CRYPTOTRANSFER => new TransferTransaction(txs),
                CRYPTOUPDATEACCOUNT => new AccountUpdateTransaction(txs),
                FILEAPPEND => new FileAppendTransaction(txs),
                FILECREATE => new FileCreateTransaction(txs),
                FILEDELETE => new FileDeleteTransaction(txs),
                FILEUPDATE => new FileUpdateTransaction(txs),
                NODECREATE => new NodeCreateTransaction(txs),
                NODEUPDATE => new NodeUpdateTransaction(txs),
                NODEDELETE => new NodeDeleteTransaction(txs),
                SYSTEMDELETE => new SystemDeleteTransaction(txs),
                SYSTEMUNDELETE => new SystemUndeleteTransaction(txs),
                FREEZE => new FreezeTransaction(txs),
                CONSENSUSCREATETOPIC => new TopicCreateTransaction(txs),
                CONSENSUSUPDATETOPIC => new TopicUpdateTransaction(txs),
                CONSENSUSDELETETOPIC => new TopicDeleteTransaction(txs),
                CONSENSUSSUBMITMESSAGE => new TopicMessageSubmitTransaction(txs),
                TOKENASSOCIATE => new TokenAssociateTransaction(txs),
                TOKENBURN => new TokenBurnTransaction(txs),
                TOKENCREATION => new TokenCreateTransaction(txs),
                TOKENDELETION => new TokenDeleteTransaction(txs),
                TOKENDISSOCIATE => new TokenDissociateTransaction(txs),
                TOKENFREEZE => new TokenFreezeTransaction(txs),
                TOKENGRANTKYC => new TokenGrantKycTransaction(txs),
                TOKENMINT => new TokenMintTransaction(txs),
                TOKENREVOKEKYC => new TokenRevokeKycTransaction(txs),
                TOKENUNFREEZE => new TokenUnfreezeTransaction(txs),
                TOKENUPDATE => new TokenUpdateTransaction(txs),
                TOKEN_UPDATE_NFTS => new TokenUpdateNftsTransaction(txs),
                TOKENWIPE => new TokenWipeTransaction(txs),
                TOKEN_FEE_SCHEDULE_UPDATE => new TokenFeeScheduleUpdateTransaction(txs),
                SCHEDULECREATE => new ScheduleCreateTransaction(txs),
                SCHEDULEDELETE => new ScheduleDeleteTransaction(txs),
                SCHEDULESIGN => new ScheduleSignTransaction(txs),
                TOKEN_PAUSE => new TokenPauseTransaction(txs),
                TOKEN_UNPAUSE => new TokenUnpauseTransaction(txs),
                TOKENREJECT => new TokenRejectTransaction(txs),
                TOKENAIRDROP => new TokenAirdropTransaction(txs),
                TOKENCANCELAIRDROP => new TokenCancelAirdropTransaction(txs),
                TOKENCLAIMAIRDROP => new TokenClaimAirdropTransaction(txs),
                CRYPTOAPPROVEALLOWANCE => new AccountAllowanceApproveTransaction(txs),
                CRYPTODELETEALLOWANCE => new AccountAllowanceDeleteTransaction(txs),
                ATOMIC_BATCH => new BatchTransaction(txs),
                LAMBDA_SSTORE => new LambdaSStoreTransaction(txs),
                _ => new ArgumentException("parsed transaction body has no data")};
        }

        /// <summary>
        /// Create the correct transaction from a scheduled transaction.
        /// </summary>
        /// <param name="scheduled">the scheduled transaction</param>
        /// <returns>the new transaction</returns>
        public static Transaction<TWildcardTodo> FromScheduledTransaction(Proto.SchedulableTransactionBody scheduled)
        {
            var body = TransactionBody.NewBuilder().SetMemo(scheduled.GetMemo()).SetTransactionFee(scheduled.GetTransactionFee()).AddAllMaxCustomFees(scheduled.GetMaxCustomFeesList());
            return scheduled.GetDataCase() switch
            {
                CONTRACTCALL => new ContractExecuteTransaction(body.SetContractCall(scheduled.GetContractCall()).Build()),
                CONTRACTCREATEINSTANCE => new ContractCreateTransaction(body.SetContractCreateInstance(scheduled.GetContractCreateInstance()).Build()),
                CONTRACTUPDATEINSTANCE => new ContractUpdateTransaction(body.SetContractUpdateInstance(scheduled.GetContractUpdateInstance()).Build()),
                CONTRACTDELETEINSTANCE => new ContractDeleteTransaction(body.SetContractDeleteInstance(scheduled.GetContractDeleteInstance()).Build()),
                CRYPTOAPPROVEALLOWANCE => new AccountAllowanceApproveTransaction(body.SetCryptoApproveAllowance(scheduled.GetCryptoApproveAllowance()).Build()),
                CRYPTODELETEALLOWANCE => new AccountAllowanceDeleteTransaction(body.SetCryptoDeleteAllowance(scheduled.GetCryptoDeleteAllowance()).Build()),
                CRYPTOCREATEACCOUNT => new AccountCreateTransaction(body.SetCryptoCreateAccount(scheduled.GetCryptoCreateAccount()).Build()),
                CRYPTODELETE => new AccountDeleteTransaction(body.SetCryptoDelete(scheduled.GetCryptoDelete()).Build()),
                CRYPTOTRANSFER => new TransferTransaction(body.SetCryptoTransfer(scheduled.GetCryptoTransfer()).Build()),
                CRYPTOUPDATEACCOUNT => new AccountUpdateTransaction(body.SetCryptoUpdateAccount(scheduled.GetCryptoUpdateAccount()).Build()),
                FILEAPPEND => new FileAppendTransaction(body.SetFileAppend(scheduled.GetFileAppend()).Build()),
                FILECREATE => new FileCreateTransaction(body.SetFileCreate(scheduled.GetFileCreate()).Build()),
                FILEDELETE => new FileDeleteTransaction(body.SetFileDelete(scheduled.GetFileDelete()).Build()),
                FILEUPDATE => new FileUpdateTransaction(body.SetFileUpdate(scheduled.GetFileUpdate()).Build()),
                NODECREATE => new NodeCreateTransaction(body.SetNodeCreate(scheduled.GetNodeCreate()).Build()),
                NODEUPDATE => new NodeUpdateTransaction(body.SetNodeUpdate(scheduled.GetNodeUpdate()).Build()),
                NODEDELETE => new NodeDeleteTransaction(body.SetNodeDelete(scheduled.GetNodeDelete()).Build()),
                SYSTEMDELETE => new SystemDeleteTransaction(body.SetSystemDelete(scheduled.GetSystemDelete()).Build()),
                SYSTEMUNDELETE => new SystemUndeleteTransaction(body.SetSystemUndelete(scheduled.GetSystemUndelete()).Build()),
                FREEZE => new FreezeTransaction(body.SetFreeze(scheduled.GetFreeze()).Build()),
                CONSENSUSCREATETOPIC => new TopicCreateTransaction(body.SetConsensusCreateTopic(scheduled.GetConsensusCreateTopic()).Build()),
                CONSENSUSUPDATETOPIC => new TopicUpdateTransaction(body.SetConsensusUpdateTopic(scheduled.GetConsensusUpdateTopic()).Build()),
                CONSENSUSDELETETOPIC => new TopicDeleteTransaction(body.SetConsensusDeleteTopic(scheduled.GetConsensusDeleteTopic()).Build()),
                CONSENSUSSUBMITMESSAGE => new TopicMessageSubmitTransaction(body.SetConsensusSubmitMessage(scheduled.GetConsensusSubmitMessage()).Build()),
                TOKENCREATION => new TokenCreateTransaction(body.SetTokenCreation(scheduled.GetTokenCreation()).Build()),
                TOKENFREEZE => new TokenFreezeTransaction(body.SetTokenFreeze(scheduled.GetTokenFreeze()).Build()),
                TOKENUNFREEZE => new TokenUnfreezeTransaction(body.SetTokenUnfreeze(scheduled.GetTokenUnfreeze()).Build()),
                TOKENGRANTKYC => new TokenGrantKycTransaction(body.SetTokenGrantKyc(scheduled.GetTokenGrantKyc()).Build()),
                TOKENREVOKEKYC => new TokenRevokeKycTransaction(body.SetTokenRevokeKyc(scheduled.GetTokenRevokeKyc()).Build()),
                TOKENDELETION => new TokenDeleteTransaction(body.SetTokenDeletion(scheduled.GetTokenDeletion()).Build()),
                TOKENUPDATE => new TokenUpdateTransaction(body.SetTokenUpdate(scheduled.GetTokenUpdate()).Build()),
                TOKEN_UPDATE_NFTS => new TokenUpdateNftsTransaction(body.SetTokenUpdateNfts(scheduled.GetTokenUpdateNfts()).Build()),
                TOKENMINT => new TokenMintTransaction(body.SetTokenMint(scheduled.GetTokenMint()).Build()),
                TOKENBURN => new TokenBurnTransaction(body.SetTokenBurn(scheduled.GetTokenBurn()).Build()),
                TOKENWIPE => new TokenWipeTransaction(body.SetTokenWipe(scheduled.GetTokenWipe()).Build()),
                TOKENASSOCIATE => new TokenAssociateTransaction(body.SetTokenAssociate(scheduled.GetTokenAssociate()).Build()),
                TOKENDISSOCIATE => new TokenDissociateTransaction(body.SetTokenDissociate(scheduled.GetTokenDissociate()).Build()),
                TOKEN_FEE_SCHEDULE_UPDATE => new TokenFeeScheduleUpdateTransaction(body.SetTokenFeeScheduleUpdate(scheduled.GetTokenFeeScheduleUpdate()).Build()),
                TOKEN_PAUSE => new TokenPauseTransaction(body.SetTokenPause(scheduled.GetTokenPause()).Build()),
                TOKEN_UNPAUSE => new TokenUnpauseTransaction(body.SetTokenUnpause(scheduled.GetTokenUnpause()).Build()),
                TOKENREJECT => new TokenRejectTransaction(body.SetTokenReject(scheduled.GetTokenReject()).Build()),
                TOKENAIRDROP => new TokenAirdropTransaction(body.SetTokenAirdrop(scheduled.GetTokenAirdrop()).Build()),
                TOKENCANCELAIRDROP => new TokenCancelAirdropTransaction(body.SetTokenCancelAirdrop(scheduled.GetTokenCancelAirdrop()).Build()),
                TOKENCLAIMAIRDROP => new TokenClaimAirdropTransaction(body.SetTokenCancelAirdrop(scheduled.GetTokenCancelAirdrop()).Build()),
                SCHEDULEDELETE => new ScheduleDeleteTransaction(body.SetScheduleDelete(scheduled.GetScheduleDelete()).Build()),
                _ => new InvalidOperationException("schedulable transaction did not have a transaction set")};
        }

        private static void ThrowProtoMatchException(string fieldName, string aWas, string bWas)
        {
            throw new ArgumentException("fromBytes() failed because " + fieldName + " fields in TransactionBody protobuf messages in the TransactionList did not match: A was " + aWas + ", B was " + bWas);
        }

        private static void RequireProtoMatches(object protoA, object protoB, HashSet<string> ignoreSet, string thisFieldName)
        {
            var aIsNull = protoA == null;
            var bIsNull = protoB == null;
            if (aIsNull != bIsNull)
            {
                ThrowProtoMatchException(thisFieldName, aIsNull ? "null" : "not null", bIsNull ? "null" : "not null");
            }

            if (aIsNull)
            {
                return;
            }

            var protoAClass = protoA.GetType();
            var protoBClass = protoB.GetType();
            if (!protoAClass.Equals(protoBClass))
            {
                ThrowProtoMatchException(thisFieldName, "of class " + protoAClass, "of class " + protoBClass);
            }

            if (protoA is bool || protoA is int || protoA is long || protoA is float || protoA is Double || protoA is string || protoA is ByteString)
            {

                // System.out.println("values A = " + protoA.toString() + ", B = " + protoB.toString());
                if (!protoA.Equals(protoB))
                {
                    ThrowProtoMatchException(thisFieldName, protoA.ToString(), protoB.ToString());
                }
            }

            foreach (var method in protoAClass.GetDeclaredMethods())
            {
                if (method.GetParameterCount() != 0)
                {
                    continue;
                }

                int methodModifiers = method.GetModifiers();
                if ((!Modifier.IsPublic(methodModifiers)) || Modifier.IsStatic(methodModifiers))
                {
                    continue;
                }

                var methodName = method.GetName();
                if (!methodName.StartsWith("get"))
                {
                    continue;
                }

                var isList = methodName.EndsWith("List") && typeof(IList).IsAssignableFrom(method.GetReturnType());
                var methodFieldName = methodName.Substring(3, methodName.Length - (isList ? 4 : 0));
                if (ignoreSet.Contains(methodFieldName) || methodFieldName.Equals("DefaultInstance"))
                {
                    continue;
                }

                if (!isList)
                {
                    try
                    {
                        var hasMethod = protoAClass.GetMethod("has" + methodFieldName);
                        var hasA = (bool)hasMethod.Invoke(protoA);
                        var hasB = (bool)hasMethod.Invoke(protoB);
                        if (!hasA.Equals(hasB))
                        {
                            ThrowProtoMatchException(methodFieldName, hasA ? "present" : "not present", hasB ? "present" : "not present");
                        }

                        if (!hasA)
                        {
                            continue;
                        }
                    }
                    catch (NoSuchMethodException ignored)
                    {
                    }
                    catch (ArgumentException error)
                    {
                        throw error;
                    }
                    catch (Throwable error)
                    {
                        throw new ArgumentException("fromBytes() failed due to error", error);
                    }
                }

                try
                {
                    var retvalA = method.Invoke(protoA);
                    var retvalB = method.Invoke(protoB);
                    if (isList)
                    {
                        var listA = (IList<TWildcardTodo>)retvalA;
                        var listB = (IList<TWildcardTodo>)retvalB;
                        if (listA.Count != listB.Count)
                        {
                            ThrowProtoMatchException(methodFieldName, "of size " + listA.Count, "of size " + listB.Count);
                        }

                        for (int i = 0; i < listA.Count; i++)
                        {

                            // System.out.println("comparing " + thisFieldName + "." + methodFieldName + "[" + i + "]");
                            RequireProtoMatches(listA[i], listB[i], ignoreSet, methodFieldName + "[" + i + "]");
                        }
                    }
                    else
                    {

                        // System.out.println("comparing " + thisFieldName + "." + methodFieldName);
                        RequireProtoMatches(retvalA, retvalB, ignoreSet, methodFieldName);
                    }
                }
                catch (ArgumentException error)
                {
                    throw error;
                }
                catch (Throwable error)
                {
                    throw new ArgumentException("fromBytes() failed due to error", error);
                }
            }
        }

        /// <summary>
        /// Generate a hash from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>the hash</returns>
        static byte[] Hash(byte[] bytes)
        {
            var digest = new SHA384Digest();
            var hash = new byte[digest.GetDigestSize()];
            digest.Update(bytes, 0, bytes.Length);
            digest.DoFinal(hash, 0);
            return hash;
        }

        private static bool PublicKeyIsInSigPairList(ByteString publicKeyBytes, IList<SignaturePair> sigPairList)
        {
            foreach (var pair in sigPairList)
            {
                if (pair.GetPubKeyPrefix().Equals(publicKeyBytes))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts transaction into a scheduled version
        /// </summary>
        /// <param name="bodyBuilder">the transaction's body builder</param>
        /// <returns>the scheduled transaction</returns>
        protected virtual ScheduleCreateTransaction DoSchedule(TransactionBody.Builder bodyBuilder)
        {
            var schedulable = SchedulableTransactionBody.NewBuilder().SetTransactionFee(bodyBuilder.GetTransactionFee()).SetMemo(bodyBuilder.GetMemo()).AddAllMaxCustomFees(bodyBuilder.GetMaxCustomFeesList());
            OnScheduled(schedulable);
            var scheduled = new ScheduleCreateTransaction().SetScheduledTransactionBody(schedulable.Build());
            if (!transactionIds.IsEmpty())
            {
                scheduled.SetTransactionId(transactionIds[0]);
            }

            return scheduled;
        }

        protected virtual bool IsBatchedAndNotBatchTransaction()
        {
            return batchKey != null && !(this is BatchTransaction);
        }

        /// <summary>
        /// Extract the scheduled transaction.
        /// </summary>
        /// <returns>the scheduled transaction</returns>
        public virtual ScheduleCreateTransaction Schedule()
        {
            RequireNotFrozen();
            if (!nodeAccountIds.IsEmpty())
            {
                throw new InvalidOperationException("The underlying transaction for a scheduled transaction cannot have node account IDs set");
            }

            var bodyBuilder = SpawnBodyBuilder(null);
            OnFreeze(bodyBuilder);
            return DoSchedule(bodyBuilder);
        }

        /// <summary>
        /// Set the account IDs of the nodes that this transaction will be submitted to.
        /// <p>
        /// Providing an explicit node account ID interferes with client-side load balancing of the network. By default, the
        /// SDK will pre-generate a transaction for 1/3 of the nodes on the network. If a node is down, busy, or otherwise
        /// reports a fatal error, the SDK will try again with a different node.
        /// </summary>
        /// <param name="nodeAccountIds">The list of node AccountIds to be set</param>
        /// <returns>{@code this}</returns>
        public override T SetNodeAccountIds(IList<AccountId> nodeAccountIds)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(nodeAccountIds);
            return base.SetNodeAccountIds(nodeAccountIds);
        }

        /// <summary>
        /// Extract the valid transaction duration.
        /// </summary>
        /// <returns>the transaction valid duration</returns>
        public Duration GetTransactionValidDuration()
        {
            return transactionValidDuration;
        }

        /// <summary>
        /// Sets the duration that this transaction is valid for.
        /// <p>
        /// This is defaulted by the SDK to 120 seconds (or two minutes).
        /// </summary>
        /// <param name="validDuration">The duration to be set</param>
        /// <returns>{@code this}</returns>
        public T SetTransactionValidDuration(Duration validDuration)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(validDuration);
            transactionValidDuration = validDuration;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Extract the maximum transaction fee.
        /// </summary>
        /// <returns>the maximum transaction fee</returns>
        public Hbar GetMaxTransactionFee()
        {
            return maxTransactionFee;
        }

        /// <summary>
        /// Set the maximum transaction fee the operator (paying account) is willing to pay.
        /// </summary>
        /// <param name="maxTransactionFee">the maximum transaction fee, in tinybars.</param>
        /// <returns>{@code this}</returns>
        public T SetMaxTransactionFee(Hbar maxTransactionFee)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(maxTransactionFee);
            maxTransactionFee = maxTransactionFee;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Extract the default maximum transaction fee.
        /// </summary>
        /// <returns>the default maximum transaction fee</returns>
        public Hbar GetDefaultMaxTransactionFee()
        {
            return defaultMaxTransactionFee;
        }

        /// <summary>
        /// Extract the memo for the transaction.
        /// </summary>
        /// <returns>the memo for the transaction</returns>
        public string GetTransactionMemo()
        {
            return memo;
        }

        /// <summary>
        /// Set a note or description that should be recorded in the transaction record (maximum length of 100 characters).
        /// </summary>
        /// <param name="memo">any notes or descriptions for this transaction.</param>
        /// <returns>{@code this}</returns>
        public T SetTransactionMemo(string memo)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(memo);
            memo = memo;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// batchify method is used to mark a transaction as part of a batch transaction or make it so-called inner transaction.
        /// The Transaction will be frozen and signed by the operator of the client.
        /// </summary>
        /// <param name="client">sdk client</param>
        /// <param name="batchKey">batch key</param>
        /// <returns>{@code this}</returns>
        public T Batchify(Client client, Key batchKey)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(batchKey);
            batchKey = batchKey;
            SignWithOperator(client);

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Set the key that will sign the batch of which this Transaction is a part of.
        /// </summary>
        public T SetBatchKey(Key batchKey)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(batchKey);
            batchKey = batchKey;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Get the key that will sign the batch of which this Transaction is a part of.
        /// </summary>
        public virtual Key GetBatchKey()
        {
            return batchKey;
        }

        /// <summary>
        /// Extract a byte array representation.
        /// </summary>
        /// <returns>the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            var list = TransactionList.NewBuilder();

            // If no nodes have been selected yet,
            // the new TransactionBody can be used to build a Transaction protobuf object.
            if (nodeAccountIds.IsEmpty())
            {
                var bodyBuilder = SpawnBodyBuilder(null);
                if (!transactionIds.IsEmpty())
                {
                    bodyBuilder.SetTransactionID(transactionIds[0].ToProtobuf());
                }

                OnFreeze(bodyBuilder);
                var signedTransaction = SignedTransaction.NewBuilder().SetBodyBytes(bodyBuilder.Build().ToByteString()).Build();
                var transaction = Proto.Transaction.NewBuilder().SetSignedTransactionBytes(signedTransaction.ToByteString()).Build();
                list.AddTransactionList(transaction);
            }
            else
            {

                // Generate the SignedTransaction protobuf objects if the Transaction's not frozen.
                if (!IsFrozen())
                {
                    frozenBodyBuilder = SpawnBodyBuilder(null);
                    if (!transactionIds.IsEmpty())
                    {
                        frozenBodyBuilder.SetTransactionID(transactionIds[0].ToProtobuf());
                    }

                    OnFreeze(frozenBodyBuilder);
                    int requiredChunks = GetRequiredChunks();
                    if (!transactionIds.IsEmpty())
                    {
                        GenerateTransactionIds(transactionIds[0], requiredChunks);
                    }

                    WipeTransactionLists(requiredChunks);
                }


                // Build all the Transaction protobuf objects and add them to the TransactionList protobuf object.
                BuildAllTransactions();
                foreach (var transaction in outerTransactions)
                {
                    list.AddTransactionList(transaction);
                }
            }

            return list.Build().ToByteArray();
        }

        /// <summary>
        /// Extract a byte array of the transaction hash.
        /// </summary>
        /// <returns>the transaction hash</returns>
        public virtual byte[] GetTransactionHash()
        {
            if (!IsFrozen())
            {
                throw new InvalidOperationException("transaction must have been frozen before calculating the hash will be stable, try calling `freeze`");
            }

            transactionIds.SetLocked(true);
            nodeAccountIds.SetLocked(true);
            var index = transactionIds.GetIndex() * nodeAccountIds.Count + nodeAccountIds.GetIndex();
            BuildTransaction(index);
            return Hash(outerTransactions[index].GetSignedTransactionBytes().ToByteArray());
        }

        /// <summary>
        /// Extract the list of account id and hash records.
        /// </summary>
        /// <returns>the list of account id and hash records</returns>
        public virtual Map<AccountId, byte[]> GetTransactionHashPerNode()
        {
            if (!IsFrozen())
            {
                throw new InvalidOperationException("transaction must have been frozen before calculating the hash will be stable, try calling `freeze`");
            }

            BuildAllTransactions();
            var hashes = new HashMap<AccountId, byte[]>();
            for (var i = 0; i < outerTransactions.Count; i++)
            {
                hashes.Put(nodeAccountIds[i], Hash(outerTransactions[i].GetSignedTransactionBytes().ToByteArray()));
            }

            return hashes;
        }

        override TransactionId GetTransactionIdInternal()
        {
            return transactionIds.GetCurrent();
        }

        /// <summary>
        /// Extract the transaction id.
        /// </summary>
        /// <returns>the transaction id</returns>
        public TransactionId GetTransactionId()
        {
            if (transactionIds.IsEmpty() || !IsFrozen())
            {
                throw new InvalidOperationException("No transaction ID generated yet. Try freezing the transaction or manually setting the transaction ID.");
            }

            return transactionIds.SetLocked(true).GetCurrent();
        }

        /// <summary>
        /// Set the ID for this transaction.
        /// <p>
        /// The transaction ID includes the operator's account ( the account paying the transaction fee). If two transactions
        /// have the same transaction ID, they won't both have an effect. One will complete normally and the other will fail
        /// with a duplicate transaction status.
        /// <p>
        /// Normally, you should not use this method. Just before a transaction is executed, a transaction ID will be
        /// generated from the operator on the client.
        /// </summary>
        /// <param name="transactionId">The TransactionId to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@seeTransactionId</remarks>
        public T SetTransactionId(TransactionId transactionId)
        {
            RequireNotFrozen();
            transactionIds.SetList(Collections.SingletonList(transactionId)).SetLocked(true);

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Should the transaction id be regenerated.
        /// </summary>
        /// <returns>should the transaction id be regenerated</returns>
        public bool GetRegenerateTransactionId()
        {
            return regenerateTransactionId;
        }

        /// <summary>
        /// Regenerate the transaction id.
        /// </summary>
        /// <param name="regenerateTransactionId">should the transaction id be regenerated</param>
        /// <returns>{@code this}</returns>
        public T SetRegenerateTransactionId(bool regenerateTransactionId)
        {
            regenerateTransactionId = regenerateTransactionId;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Sign the transaction.
        /// </summary>
        /// <param name="privateKey">the private key</param>
        /// <returns>the signed transaction</returns>
        public T Sign(PrivateKey privateKey)
        {
            return SignWith(privateKey.GetPublicKey(), privateKey.Sign());
        }

        /// <summary>
        /// Sign the transaction.
        /// </summary>
        /// <param name="publicKey">the public key</param>
        /// <param name="transactionSigner">the key list</param>
        /// <returns>{@code this}</returns>
        public virtual T SignWith(PublicKey publicKey, UnaryOperator<byte[]> transactionSigner)
        {
            if (!IsFrozen())
            {
                throw new InvalidOperationException("Signing requires transaction to be frozen");
            }

            if (KeyAlreadySigned(publicKey))
            {

                // noinspection unchecked
                return (T)this;
            }

            for (int i = 0; i < outerTransactions.Count; i++)
            {
                outerTransactions[i] = null;
            }

            publicKeys.Add(publicKey);
            signers.Add(transactionSigner);

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Sign the transaction with the configured client.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <returns>the signed transaction</returns>
        public virtual T SignWithOperator(Client client)
        {
            var operator = client.GetOperator();
            if (@operator == null)
            {
                throw new InvalidOperationException("`client` must have an `operator` to sign with the operator");
            }

            if (!IsFrozen())
            {
                FreezeWith(client);
            }

            return SignWith(@operator.publicKey, @operator.transactionSigner);
        }

        /// <summary>
        /// Checks if a public key is already added to the transaction
        /// </summary>
        /// <param name="key">the public key</param>
        /// <returns>if the public key is already added</returns>
        protected virtual bool KeyAlreadySigned(PublicKey key)
        {
            return publicKeys.Contains(key);
        }

        /// <summary>
        /// Add a signature to the transaction.
        /// </summary>
        /// <param name="publicKey">the public key</param>
        /// <param name="signature">the signature</param>
        /// <returns>{@code this}</returns>
        public virtual T AddSignature(PublicKey publicKey, byte[] signature)
        {
            RequireOneNodeAccountId();
            if (!IsFrozen())
            {
                Freeze();
            }

            if (KeyAlreadySigned(publicKey))
            {

                // noinspection unchecked
                return (T)this;
            }

            transactionIds.SetLocked(true);
            nodeAccountIds.SetLocked(true);
            for (int i = 0; i < outerTransactions.Count; i++)
            {
                outerTransactions[i] = null;
            }

            publicKeys.Add(publicKey);
            signers.Add(null);
            sigPairLists[0].AddSigPair(publicKey.ToSignaturePairProtobuf(signature));

            // noinspection unchecked
            return (T)this;
        }

        protected virtual Map<AccountId, Map<PublicKey, byte[]>> GetSignaturesAtOffset(int offset)
        {
            var map = new HashMap<AccountId, Map<PublicKey, byte[]>>(nodeAccountIds.Count);
            for (int i = 0; i < nodeAccountIds.Count; i++)
            {
                var sigMap = sigPairLists[i + offset];
                var nodeAccountId = nodeAccountIds[i];
                var keyMap = map.ContainsKey(nodeAccountId) ? Objects.RequireNonNull(map[nodeAccountId]) : new HashMap<PublicKey, byte[]>(sigMap.GetSigPairCount());
                map.Put(nodeAccountId, keyMap);
                foreach (var sigPair in sigMap.GetSigPairList())
                {
                    keyMap.Put(PublicKey.FromBytes(sigPair.GetPubKeyPrefix().ToByteArray()), sigPair.GetEd25519().ToByteArray());
                }
            }

            return map;
        }

        /// <summary>
        /// Extract list of account id and public keys.
        /// </summary>
        /// <returns>the list of account id and public keys</returns>
        public virtual Map<AccountId, Map<PublicKey, byte[]>> GetSignatures()
        {
            if (!IsFrozen())
            {
                throw new InvalidOperationException("Transaction must be frozen in order to have signatures.");
            }

            if (publicKeys.IsEmpty())
            {
                return Collections.EmptyMap();
            }

            BuildAllTransactions();
            return GetSignaturesAtOffset(0);
        }

        /// <summary>
        /// Check if transaction is frozen.
        /// </summary>
        /// <returns>is the transaction frozen</returns>
        protected virtual bool IsFrozen()
        {
            return frozenBodyBuilder != null;
        }

        /// <summary>
        /// Throw an exception if the transaction is frozen.
        /// </summary>
        protected virtual void RequireNotFrozen()
        {
            if (IsFrozen())
            {
                throw new InvalidOperationException("transaction is immutable; it has at least one signature or has been explicitly frozen");
            }
        }

        /// <summary>
        /// Throw an exception if there is not exactly one node id set.
        /// </summary>
        protected virtual void RequireOneNodeAccountId()
        {
            if (nodeAccountIds.Count != 1)
            {
                throw new InvalidOperationException("transaction did not have exactly one node ID set");
            }
        }

        protected virtual TransactionBody.Builder SpawnBodyBuilder(Client client)
        {
            var clientDefaultFee = client != null ? client.GetDefaultMaxTransactionFee() : null;
            var defaultFee = clientDefaultFee != null ? clientDefaultFee : defaultMaxTransactionFee;
            var feeHbars = maxTransactionFee != null ? maxTransactionFee : defaultFee;
            var builder = TransactionBody.NewBuilder().SetTransactionFee(feeHbars.ToTinybars()).SetTransactionValidDuration(Utils.DurationConverter.ToProtobuf(transactionValidDuration).ToBuilder()).AddAllMaxCustomFees(customFeeLimits.Stream().Map(CustomFeeLimit.ToProtobuf()).Collect(Collectors.ToList())).SetMemo(memo);
            if (batchKey != null)
            {
                builder.SetBatchKey(batchKey.ToProtobufKey());
            }

            return builder;
        }

        /// <summary>
        /// Freeze this transaction from further modification to prepare for signing or serialization.
        /// </summary>
        /// <returns>{@code this}</returns>
        public virtual T Freeze()
        {
            return FreezeWith(null);
        }

        /// <summary>
        /// Freeze this transaction from further modification to prepare for signing or serialization.
        /// <p>
        /// Will use the `Client`, if available, to generate a default Transaction ID and select 1/3 nodes to prepare this
        /// transaction for.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <returns>{@code this}</returns>
        public virtual T FreezeWith(Client client)
        {
            if (IsFrozen())
            {

                // noinspection unchecked
                return (T)this;
            }

            if (transactionIds.IsEmpty())
            {
                if (client != null)
                {
                    var operator = client.GetOperator();
                    if (@operator != null)
                    {

                        // Set a default transaction ID, generated from the operator account ID
                        transactionIds.SetList(Collections.SingletonList(TransactionId.Generate(@operator.accountId)));
                    }
                    else
                    {

                        // no client means there must be an explicitly set node ID and transaction ID
                        throw new InvalidOperationException("`client` must have an `operator` or `transactionId` must be set");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Transaction ID must be set, or operator must be provided via freezeWith()");
                }
            }

            if (nodeAccountIds.IsEmpty())
            {
                if (client == null)
                {
                    throw new InvalidOperationException("`client` must be provided or both `nodeId` and `transactionId` must be set");
                }

                try
                {
                    if (batchKey == null)
                    {
                        nodeAccountIds.SetList(client.network.GetNodeAccountIdsForExecute());
                    }
                    else
                    {
                        nodeAccountIds.SetList(Collections.SingletonList(AccountId.FromString(ATOMIC_BATCH_NODE_ACCOUNT_ID)));
                    }
                }
                catch (InterruptedException e)
                {
                    throw new Exception(e);
                }
            }

            frozenBodyBuilder = SpawnBodyBuilder(client).SetTransactionID(transactionIds[0].ToProtobuf());
            OnFreeze(frozenBodyBuilder);
            int requiredChunks = GetRequiredChunks();
            GenerateTransactionIds(transactionIds[0], requiredChunks);
            WipeTransactionLists(requiredChunks);
            var clientDefaultRegenerateTransactionId = client != null ? client.GetDefaultRegenerateTransactionId() : null;
            regenerateTransactionId = regenerateTransactionId != null ? regenerateTransactionId : clientDefaultRegenerateTransactionId;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// There must be at least one chunk.
        /// </summary>
        /// <returns>there is 1 required chunk</returns>
        virtual int GetRequiredChunks()
        {
            return 1;
        }

        /// <summary>
        /// Generate transaction id's.
        /// </summary>
        /// <param name="initialTransactionId">the initial transaction id</param>
        /// <param name="count">the number of id's to generate.</param>
        virtual void GenerateTransactionIds(TransactionId initialTransactionId, int count)
        {
            var locked = transactionIds.IsLocked();
            transactionIds.SetLocked(false);
            if (count == 1)
            {
                transactionIds.SetList(Collections.SingletonList(initialTransactionId));
                return;
            }

            var nextTransactionId = initialTransactionId.ToProtobuf().ToBuilder();
            transactionIds.EnsureCapacity(count);
            transactionIds.Clear();
            for (int i = 0; i < count; i++)
            {
                transactionIds.Add(TransactionId.FromProtobuf(nextTransactionId.Build()));

                // add 1 ns to the validStart to make cascading transaction IDs
                var nextValidStart = nextTransactionId.GetTransactionValidStart().ToBuilder();
                nextValidStart.SetNanos(nextValidStart.GetNanos() + 1);
                nextTransactionId.SetTransactionValidStart(nextValidStart);
            }

            transactionIds.SetLocked(locked);
        }

        /// <summary>
        /// Wipe / reset the transaction list.
        /// </summary>
        /// <param name="requiredChunks">the number of required chunks</param>
        virtual void WipeTransactionLists(int requiredChunks)
        {
            if (!transactionIds.IsEmpty())
            {
                Objects.RequireNonNull(frozenBodyBuilder).SetTransactionID(GetTransactionIdInternal().ToProtobuf());
            }

            outerTransactions = new List(nodeAccountIds.Count);
            sigPairLists = new List(nodeAccountIds.Count);
            innerSignedTransactions = new List(nodeAccountIds.Count);
            foreach (AccountId nodeId in nodeAccountIds)
            {
                sigPairLists.Add(SignatureMap.NewBuilder());
                innerSignedTransactions.Add(SignedTransaction.NewBuilder().SetBodyBytes(Objects.RequireNonNull(frozenBodyBuilder).SetNodeAccountID(nodeId.ToProtobuf()).Build().ToByteString()));
                outerTransactions.Add(null);
            }
        }

        /// <summary>
        /// Build all the transactions.
        /// </summary>
        virtual void BuildAllTransactions()
        {
            transactionIds.SetLocked(true);
            nodeAccountIds.SetLocked(true);
            for (var i = 0; i < innerSignedTransactions.Count; ++i)
            {
                BuildTransaction(i);
            }
        }

        /// <summary>
        /// Will build the specific transaction at {@code index} This function is only ever called after the transaction is
        /// frozen.
        /// </summary>
        /// <param name="index">the index of the transaction to be built</param>
        virtual void BuildTransaction(int index)
        {

            // Check if transaction is already built.
            // Every time a signer is added via sign() or signWith(), all outerTransactions are nullified.
            if (outerTransactions[index] != null && !outerTransactions[index].GetSignedTransactionBytes().IsEmpty())
            {
                return;
            }

            SignTransaction(index);
            outerTransactions[index] = Proto.Transaction.NewBuilder().SetSignedTransactionBytes(innerSignedTransactions[index].SetSigMap(sigPairLists[index]).Build().ToByteString()).Build();
        }

        /// <summary>
        /// Will sign the specific transaction at {@code index} This function is only ever called after the transaction is
        /// frozen.
        /// </summary>
        /// <param name="index">the index of the transaction to sign</param>
        virtual void SignTransaction(int index)
        {
            var bodyBytes = innerSignedTransactions[index].GetBodyBytes().ToByteArray();
            var thisSigPairList = sigPairLists[index].GetSigPairList();
            for (var i = 0; i < publicKeys.Count; i++)
            {
                if (signers[i] == null)
                {
                    continue;
                }

                if (PublicKeyIsInSigPairList(ByteString.CopyFrom(publicKeys[i].ToBytesRaw()), thisSigPairList))
                {
                    continue;
                }

                var signatureBytes = signers[i].Apply(bodyBytes);
                sigPairLists[index].AddSigPair(publicKeys[i].ToSignaturePairProtobuf(signatureBytes));
            }
        }

        /// <summary>
        /// Called in {@link #freezeWith(Client)} just before the transaction body is built. The intent is for the derived
        /// class to assign their data variant to the transaction body.
        /// </summary>
        abstract void OnFreeze(TransactionBody.Builder bodyBuilder);
        /// <summary>
        /// Called in {@link #schedule()} when converting transaction into a scheduled version.
        /// </summary>
        abstract void OnScheduled(SchedulableTransactionBody.Builder scheduled);
        override Proto.Transaction MakeRequest()
        {
            var index = nodeAccountIds.GetIndex() + (transactionIds.GetIndex() * nodeAccountIds.Count);
            BuildTransaction(index);
            return outerTransactions[index];
        }

        override TransactionResponse MapResponse(Proto.TransactionResponse transactionResponse, AccountId nodeId, Proto.Transaction request)
        {
            var transactionId = Objects.RequireNonNull(GetTransactionIdInternal());
            var hash = Hash(request.GetSignedTransactionBytes().ToByteArray());

            // advance is needed for chunked transactions
            transactionIds.Advance();
            return new TransactionResponse(nodeId, transactionId, hash, null, this);
        }

        override Status MapResponseStatus(Proto.TransactionResponse transactionResponse)
        {
            return Status.ValueOf(transactionResponse.GetNodeTransactionPrecheckCode());
        }

        abstract void ValidateChecksums(Client client);
        /// <summary>
        /// Prepare the transactions to be executed.
        /// </summary>
        /// <param name="client">the configured client</param>
        virtual void OnExecute(Client client)
        {
            if (!IsFrozen())
            {
                FreezeWith(client);
            }

            var accountId = Objects.RequireNonNull(Objects.RequireNonNull(transactionIds[0]).accountId);
            if (client.IsAutoValidateChecksumsEnabled())
            {
                try
                {
                    accountId.ValidateChecksum(client);
                    ValidateChecksums(client);
                }
                catch (BadEntityIdException exc)
                {
                    throw new ArgumentException(exc.GetMessage());
                }
            }

            var operatorId = client.GetOperatorAccountId();
            if (operatorId != null && operatorId.Equals(accountId))
            {

                // on execute, sign each transaction with the operator, if present
                // and we are signing a transaction that used the default transaction ID
                SignWithOperator(client);
            }
        }

        override CompletableFuture<Void> OnExecuteAsync(Client client)
        {
            OnExecute(client);
            return CompletableFuture.CompletedFuture(null);
        }

        override ExecutionState GetExecutionState(Status status, Proto.TransactionResponse response)
        {
            if (status == Status.TRANSACTION_EXPIRED)
            {
                if ((regenerateTransactionId != null && !regenerateTransactionId) || transactionIds.IsLocked())
                {
                    return ExecutionState.REQUEST_ERROR;
                }
                else
                {
                    var firstTransactionId = Objects.RequireNonNull(transactionIds[0]);
                    var accountId = Objects.RequireNonNull(firstTransactionId.accountId);
                    GenerateTransactionIds(TransactionId.Generate(accountId), transactionIds.Count);
                    WipeTransactionLists(transactionIds.Count);
                    return ExecutionState.RETRY;
                }
            }

            return base.GetExecutionState(status, response);
        }

        virtual Transaction RegenerateTransactionId(Client client)
        {
            Objects.RequireNonNull(client.GetOperatorAccountId());
            transactionIds.SetLocked(false);
            var newTransactionID = TransactionId.Generate(client.GetOperatorAccountId());
            transactionIds[transactionIds.GetIndex()] = newTransactionID;
            transactionIds.SetLocked(true);
            return this;
        }

        public override string ToString()
        {

            // NOTE: regex is for removing the instance address from the default debug output
            TransactionBody.Builder body = SpawnBodyBuilder(null);
            if (!transactionIds.IsEmpty())
            {
                body.SetTransactionID(transactionIds[0].ToProtobuf());
            }

            if (!nodeAccountIds.IsEmpty())
            {
                body.SetNodeAccountID(nodeAccountIds[0].ToProtobuf());
            }

            OnFreeze(body);
            return body.BuildPartial().ToString().ReplaceAll("@[A-Za-z0-9]+", "");
        }

        /// <summary>
        /// This method retrieves the size of the transaction
        /// </summary>
        /// <returns></returns>
        public virtual int GetTransactionSize()
        {
            if (!IsFrozen())
            {
                throw new InvalidOperationException("transaction must have been frozen before getting it's size, try calling `freeze`");
            }

            return MakeRequest().GetSerializedSize();
        }

        /// <summary>
        /// This method retrieves the transaction body size
        /// </summary>
        /// <returns></returns>
        public virtual int GetTransactionBodySize()
        {
            if (!IsFrozen())
            {
                throw new InvalidOperationException("transaction must have been frozen before getting it's body size, try calling `freeze`");
            }

            if (frozenBodyBuilder != null)
            {
                return frozenBodyBuilder.Build().GetSerializedSize();
            }

            return 0;
        }

        public class SignableNodeTransactionBodyBytes
        {
            private AccountId nodeID;
            private TransactionId transactionID;
            private byte[] body;
            public SignableNodeTransactionBodyBytes(AccountId nodeID, TransactionId transactionID, byte[] body)
            {
                nodeID = nodeID;
                transactionID = transactionID;
                body = body;
            }

            public virtual AccountId GetNodeID()
            {
                return nodeID;
            }

            public virtual TransactionId GetTransactionID()
            {
                return transactionID;
            }

            public virtual byte[] GetBody()
            {
                return body;
            }
        }

        /// <summary>
        /// Returns a list of SignableNodeTransactionBodyBytes objects for each signed transaction in the transaction list.
        /// The NodeID represents the node that this transaction is signed for.
        /// The TransactionID is useful for signing chunked transactions like FileAppendTransaction,
        /// since they can have multiple transaction ids.
        /// </summary>
        /// <returns>List of SignableNodeTransactionBodyBytes</returns>
        /// <exception cref="RuntimeException">if transaction is not frozen or protobuf parsing fails</exception>
        public virtual IList<SignableNodeTransactionBodyBytes> GetSignableNodeBodyBytesList()
        {
            if (!IsFrozen())
            {
                throw new Exception("Transaction is not frozen");
            }

            IList<SignableNodeTransactionBodyBytes> signableNodeTransactionBodyBytesList = new ();
            for (int i = 0; i < innerSignedTransactions.Count; i++)
            {
                SignedTransaction signableNodeTransactionBodyBytes = innerSignedTransactions[i].Build();
                TransactionBody body = ParseTransactionBody(signableNodeTransactionBodyBytes.GetBodyBytes());
                AccountId nodeID = AccountId.FromProtobuf(body.GetNodeAccountID());
                TransactionId transactionID = TransactionId.FromProtobuf(body.GetTransactionID());
                signableNodeTransactionBodyBytesList.Add(new SignableNodeTransactionBodyBytes(nodeID, transactionID, signableNodeTransactionBodyBytes.GetBodyBytes().ToByteArray()));
            }

            return signableNodeTransactionBodyBytesList;
        }

        /// <summary>
        /// Adds a signature to the transaction for a specific transaction id and node id.
        /// This is useful for signing chunked transactions like FileAppendTransaction,
        /// since they can have multiple transaction ids.
        /// </summary>
        /// <param name="publicKey">The public key to add signature for</param>
        /// <param name="signature">The signature bytes</param>
        /// <param name="transactionID">The specific transaction ID to match</param>
        /// <param name="nodeID">The specific node ID to match</param>
        /// <returns>The child transaction (this)</returns>
        /// <exception cref="RuntimeException">if unmarshaling fails or invalid signed transaction</exception>
        public virtual T AddSignature(PublicKey publicKey, byte[] signature, TransactionId transactionID, AccountId nodeID)
        {
            if (innerSignedTransactions.IsEmpty())
            {

                // noinspection unchecked
                return (T)this;
            }

            transactionIds.SetLocked(true);
            for (int index = 0; index < innerSignedTransactions.Count; index++)
            {
                if (ProcessedSignatureForTransaction(index, publicKey, signature, transactionID, nodeID))
                {
                    UpdateTransactionState(publicKey);
                }
            }


            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Processes signature addition for a single transaction at the given index.
        /// </summary>
        /// <param name="index">The index of the transaction to process</param>
        /// <param name="publicKey">The public key to add signature for</param>
        /// <param name="signature">The signature bytes</param>
        /// <param name="transactionID">The specific transaction ID to match</param>
        /// <param name="nodeID">The specific node ID to match</param>
        /// <returns>true if signature was added, false otherwise</returns>
        private bool ProcessedSignatureForTransaction(int index, PublicKey publicKey, byte[] signature, TransactionId transactionID, AccountId nodeID)
        {
            SignedTransaction.Builder temp = innerSignedTransactions[index];
            TransactionBody body = ParseTransactionBody(temp);
            if (body == null)
            {
                return false;
            }

            if (!MatchesTargetTransactionAndNode(body, transactionID, nodeID))
            {
                return false;
            }

            return AddSignatureIfNotExists(index, publicKey, signature);
        }

        /// <summary>
        /// Parses the transaction body from a signed transaction builder.
        /// </summary>
        /// <param name="signedTransactionBuilder">The signed transaction builder</param>
        /// <returns>The parsed transaction body, or null if parsing fails</returns>
        private static TransactionBody ParseTransactionBody(SignedTransaction.Builder signedTransactionBuilder)
        {
            try
            {
                return TransactionBody.ParseFrom(signedTransactionBuilder.GetBodyBytes());
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new Exception("Failed to parse transaction body", e);
            }
        }

        private static TransactionBody ParseTransactionBody(ByteString signedTransactionBuilder)
        {
            try
            {
                return TransactionBody.ParseFrom(signedTransactionBuilder);
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new Exception("Failed to parse transaction body", e);
            }
        }

        /// <summary>
        /// Checks if the transaction body matches the target transaction ID and node ID.
        /// </summary>
        /// <param name="body">The transaction body to check</param>
        /// <param name="targetTransactionID">The target transaction ID to match against</param>
        /// <param name="targetNodeID">The target node ID to match against</param>
        /// <returns>true if both the transaction ID and node ID match the targets, false otherwise</returns>
        private bool MatchesTargetTransactionAndNode(TransactionBody body, TransactionId targetTransactionID, AccountId targetNodeID)
        {
            TransactionId bodyTxID = TransactionId.FromProtobuf(body.GetTransactionID());
            AccountId bodyNodeID = AccountId.FromProtobuf(body.GetNodeAccountID());
            return bodyTxID.ToString().Equals(targetTransactionID.ToString()) && bodyNodeID.ToString().Equals(targetNodeID.ToString());
        }

        /// <summary>
        /// Adds signature if it doesn't already exist for the given public key.
        /// </summary>
        /// <param name="index">The transaction index</param>
        /// <param name="publicKey">The public key</param>
        /// <param name="signature">The signature bytes</param>
        /// <returns>true if signature was added, false if it already existed</returns>
        private bool AddSignatureIfNotExists(int index, PublicKey publicKey, byte[] signature)
        {
            SignatureMap.Builder sigMapBuilder = sigPairLists[index];

            // Check if the signature is already in the signature map
            if (IsSignatureAlreadyPresent(sigMapBuilder, publicKey))
            {
                return false;
            }


            // Add the signature to the signature map
            SignaturePair newSigPair = publicKey.ToSignaturePairProtobuf(signature);
            sigMapBuilder.AddSigPair(newSigPair);
            return true;
        }

        /// <summary>
        /// Checks if a signature for the given public key already exists.
        /// </summary>
        /// <param name="sigMapBuilder">The signature map builder</param>
        /// <param name="publicKey">The public key to check</param>
        /// <returns>true if signature already exists, false otherwise</returns>
        private bool IsSignatureAlreadyPresent(SignatureMap.Builder sigMapBuilder, PublicKey publicKey)
        {
            foreach (SignaturePair sig in sigMapBuilder.GetSigPairList())
            {
                if (Equals(sig.GetPubKeyPrefix().ToByteArray(), publicKey.ToBytesRaw()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the transaction state after adding a signature.
        /// </summary>
        /// <param name="publicKey">The public key that was added</param>
        private void UpdateTransactionState(PublicKey publicKey)
        {
            publicKeys.Add(publicKey);
            signers.Add(null);
        }
    }
}