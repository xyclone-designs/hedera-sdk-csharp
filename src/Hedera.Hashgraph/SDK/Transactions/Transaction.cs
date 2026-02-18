// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.LiveHashes;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.System;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Topic;

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Transactions
{
	public sealed class Transaction
	{
		/// <summary>
		/// Create the correct transaction from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>the new transaction</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static Transaction<T> FromBytes<T>(byte[] bytes) where T : Transaction<T>
		{
			var list = Proto.TransactionList.Parser.ParseFrom(bytes);
			var txsMap = new LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>>();

			Proto.TransactionBody.DataOneofCase dataCase;

			if (list.TransactionList_.Count != 0)
				dataCase = ProcessTransactionList([.. list.TransactionList_], txsMap);
			else dataCase = ProcessSingleTransaction(bytes, txsMap);

			return CreateTransactionFromDataCase<T>(dataCase, txsMap);
		}

		/// <summary>
		/// Generate a hash from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>the hash</returns>
		public static byte[] GenerateHash(byte[] bytes)
		{
			var digest = new Sha384Digest();
			var hash = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(bytes, 0, bytes.Length);
			digest.DoFinal(hash, 0);
			return hash;
		}

		/// <summary>
		/// Add a transaction to the transaction map
		/// </summary>
		private static void AddTransactionToMap(Proto.Transaction transaction, Proto.TransactionBody txBody, LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txsMap)
		{
			var account = txBody.NodeAccountID is not null ? AccountId.FromProtobuf(txBody.NodeAccountID) : DUMMY_ACCOUNT_ID;
			var transactionId = txBody.TransactionID is not null ? TransactionId.FromProtobuf(txBody.TransactionID) : DUMMY_TRANSACTION_ID;
			var linked = txsMap.ContainsKey(transactionId) ? txsMap[transactionId] : new LinkedDictionary<AccountId, Proto.Transaction>();

			linked.Add(account, transaction);
			txsMap.Add(transactionId, linked);
		}
		/// <summary>
		/// Creates the appropriate transaction type based on the data case.
		/// </summary>
		private static Transaction<T> CreateTransactionFromDataCase<T>(Proto.TransactionBody.DataOneofCase dataCase, LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) where T : Transaction<T>
		{
			return dataCase switch
			{
				Proto.TransactionBody.DataOneofCase.ContractCall => new ContractExecuteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ContractCreateInstance => new ContractCreateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ContractUpdateInstance => new ContractUpdateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ContractDeleteInstance => new ContractDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.EthereumTransaction => new EthereumTransaction(txs),
				Proto.TransactionBody.DataOneofCase.CryptoAddLiveHash => new LiveHashAddTransaction(txs),
				Proto.TransactionBody.DataOneofCase.CryptoCreateAccount => new AccountCreateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.CryptoDelete => new AccountDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.CryptoDeleteLiveHash => new LiveHashDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.CryptoTransfer => new TransferTransaction(txs),
				Proto.TransactionBody.DataOneofCase.CryptoUpdateAccount => new AccountUpdateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.FileAppend => new FileAppendTransaction(txs),
				Proto.TransactionBody.DataOneofCase.FileCreate => new FileCreateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.FileDelete => new FileDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.FileUpdate => new FileUpdateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.NodeCreate => new NodeCreateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.NodeUpdate => new NodeUpdateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.NodeDelete => new NodeDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.SystemDelete => new SystemDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.SystemUndelete => new SystemUndeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.Freeze => new FreezeTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ConsensusCreateTopic => new TopicCreateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ConsensusUpdateTopic => new TopicUpdateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ConsensusDeleteTopic => new TopicDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ConsensusSubmitMessage => new TopicMessageSubmitTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenAssociate => new TokenAssociateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenBurn => new TokenBurnTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenCreation => new TokenCreateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenDeletion => new TokenDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenDissociate => new TokenDissociateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenFreeze => new TokenFreezeTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenGrantKyc => new TokenGrantKycTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenMint => new TokenMintTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenRevokeKyc => new TokenRevokeKycTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenUnfreeze => new TokenUnfreezeTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenUpdate => new TokenUpdateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenUpdateNfts => new TokenUpdateNftsTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenWipe => new TokenWipeTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenFeeScheduleUpdate => new TokenFeeScheduleUpdateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ScheduleCreate => new ScheduleCreateTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ScheduleDelete => new ScheduleDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.ScheduleSign => new ScheduleSignTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenPause => new TokenPauseTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenUnpause => new TokenUnpauseTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenReject => new TokenRejectTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenAirdrop => new TokenAirdropTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenCancelAirdrop => new TokenCancelAirdropTransaction(txs),
				Proto.TransactionBody.DataOneofCase.TokenClaimAirdrop => new TokenClaimAirdropTransaction(txs),
				Proto.TransactionBody.DataOneofCase.CryptoApproveAllowance => new AccountAllowanceApproveTransaction(txs),
				Proto.TransactionBody.DataOneofCase.CryptoDeleteAllowance => new AccountAllowanceDeleteTransaction(txs),
				Proto.TransactionBody.DataOneofCase.AtomicBatch => new BatchTransaction(txs),
				Proto.TransactionBody.DataOneofCase.LambdaSstore => new LambdaSStoreTransaction(txs),

				_ => throw new ArgumentException("parsed transaction body has no data")
			};
		}
		/// <summary>
		/// Create the correct transaction from a scheduled transaction.
		/// </summary>
		/// <param name="scheduled">the scheduled transaction</param>
		/// <returns>the new transaction</returns>
		public static Transaction<T> FromScheduledTransaction<T>(Proto.SchedulableTransactionBody scheduled) where T : Transaction<T>
		{
			Proto.TransactionBody proto = new()
			{
				Memo = scheduled.Memo,
				TransactionFee = scheduled.TransactionFee,
			};

			proto.MaxCustomFees.AddRange(scheduled.MaxCustomFees);

			switch (scheduled.DataCase)
			{
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractCall:
					proto.ContractCall = scheduled.ContractCall;
					return new ContractExecuteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractCreateInstance:
					proto.ContractCreateInstance = scheduled.ContractCreateInstance;
					return new ContractCreateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractUpdateInstance:
					proto.ContractUpdateInstance = scheduled.ContractUpdateInstance;
					return new ContractUpdateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractDeleteInstance:
					proto.ContractDeleteInstance = scheduled.ContractDeleteInstance;
					return new ContractDeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoApproveAllowance:
					proto.CryptoApproveAllowance = scheduled.CryptoApproveAllowance;
					return new AccountAllowanceApproveTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoDeleteAllowance:
					proto.CryptoDeleteAllowance = scheduled.CryptoDeleteAllowance;
					return new AccountAllowanceDeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoCreateAccount:
					proto.CryptoCreateAccount = scheduled.CryptoCreateAccount;
					return new AccountCreateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoDelete:
					proto.CryptoDelete = scheduled.CryptoDelete;
					return new AccountDeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoTransfer:
					proto.CryptoTransfer = scheduled.CryptoTransfer;
					return new TransferTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoUpdateAccount:
					proto.CryptoUpdateAccount = scheduled.CryptoUpdateAccount;
					return new AccountUpdateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileAppend:
					proto.FileAppend = scheduled.FileAppend;
					return new FileAppendTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileCreate:
					proto.FileCreate = scheduled.FileCreate;
					return new FileCreateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileDelete:
					proto.FileDelete = scheduled.FileDelete;
					return new FileDeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileUpdate:
					proto.FileUpdate = scheduled.FileUpdate;
					return new FileUpdateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeCreate:
					proto.NodeCreate = scheduled.NodeCreate;
					return new NodeCreateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeUpdate:
					proto.NodeUpdate = scheduled.NodeUpdate;
					return new NodeUpdateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeDelete:
					proto.NodeDelete = scheduled.NodeDelete;
					return new NodeDeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.SystemDelete:
					proto.SystemDelete = scheduled.SystemDelete;
					return new SystemDeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.SystemUndelete:
					proto.SystemUndelete = scheduled.SystemUndelete;
					return new SystemUndeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.Freeze:
					proto.Freeze = scheduled.Freeze;
					return new FreezeTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusCreateTopic:
					proto.ConsensusCreateTopic = scheduled.ConsensusCreateTopic;
					return new TopicCreateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusUpdateTopic:
					proto.ConsensusUpdateTopic = scheduled.ConsensusUpdateTopic;
					return new TopicUpdateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusDeleteTopic:
					proto.ConsensusDeleteTopic = scheduled.ConsensusDeleteTopic;
					return new TopicDeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusSubmitMessage:
					proto.ConsensusSubmitMessage = scheduled.ConsensusSubmitMessage;
					return new TopicMessageSubmitTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenCreation:
					proto.TokenCreation = scheduled.TokenCreation;
					return new TokenCreateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenFreeze:
					proto.TokenFreeze = scheduled.TokenFreeze;
					return new TokenFreezeTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUnfreeze:
					proto.TokenUnfreeze = scheduled.TokenUnfreeze;
					return new TokenUnfreezeTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenGrantKyc:
					proto.TokenGrantKyc = scheduled.TokenGrantKyc;
					return new TokenGrantKycTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenRevokeKyc:
					proto.TokenRevokeKyc = scheduled.TokenRevokeKyc;
					return new TokenRevokeKycTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenDeletion:
					proto.TokenDeletion = scheduled.TokenDeletion;
					return new TokenDeleteTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUpdate:
					proto.TokenUpdate = scheduled.TokenUpdate;
					return new TokenUpdateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUpdateNfts:
					proto.TokenUpdateNfts = scheduled.TokenUpdateNfts;
					return new TokenUpdateNftsTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenMint:
					proto.TokenMint = scheduled.TokenMint;
					return new TokenMintTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenBurn:
					proto.TokenBurn = scheduled.TokenBurn;
					return new TokenBurnTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenWipe:
					proto.TokenWipe = scheduled.TokenWipe;
					return new TokenWipeTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenAssociate:
					proto.TokenAssociate = scheduled.TokenAssociate;
					return new TokenAssociateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenDissociate:
					proto.TokenDissociate = scheduled.TokenDissociate;
					return new TokenDissociateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenFeeScheduleUpdate:
					proto.TokenFeeScheduleUpdate = scheduled.TokenFeeScheduleUpdate;
					return new TokenFeeScheduleUpdateTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenPause:
					proto.TokenPause = scheduled.TokenPause;
					return new TokenPauseTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUnpause:
					proto.TokenUnpause = scheduled.TokenUnpause;
					return new TokenUnpauseTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenReject:
					proto.TokenReject = scheduled.TokenReject;
					return new TokenRejectTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenAirdrop:
					proto.TokenAirdrop = scheduled.TokenAirdrop;
					return new TokenAirdropTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenCancelAirdrop:
					proto.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					return new TokenCancelAirdropTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenClaimAirdrop:
					proto.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					return new TokenClaimAirdropTransaction(proto) as Transaction<T>;
				case Proto.SchedulableTransactionBody.DataOneofCase.ScheduleDelete:
					proto.ScheduleDelete = scheduled.ScheduleDelete;
					return new ScheduleDeleteTransaction(proto) as Transaction<T>;

				default: throw new InvalidOperationException("schedulable transaction did not have a transaction set");
			}
		}
		/// <summary>
		/// Parses the transaction body from a signed transaction bytestring.
		/// </summary>
		/// <param name="signedTransactionBuilder">The signed transaction builder</param>
		/// <returns>The parsed transaction body, or null if parsing fails</returns>
		private static Proto.TransactionBody ParseTransactionBody(ByteString signedTransactionBuilder)
		{
			try
			{
				return Proto.TransactionBody.Parser.ParseFrom(signedTransactionBuilder);
			}
			catch (InvalidProtocolBufferException e)
			{
				throw new Exception("Failed to parse transaction body", e);
			}
		}
		/// <summary>
		/// Parses the transaction body from a signed transaction builder.
		/// </summary>
		/// <param name="signedTransactionBuilder">The signed transaction builder</param>
		/// <returns>The parsed transaction body, or null if parsing fails</returns>
		private static Proto.TransactionBody ParseTransactionBody(Proto.SignedTransaction signedTransactionBuilder)
		{
			try
			{
				return Proto.TransactionBody.Parser.ParseFrom(signedTransactionBuilder.BodyBytes);
			}
			catch (InvalidProtocolBufferException e)
			{
				throw new Exception("Failed to parse transaction body", e);
			}
		}
		/// <summary>
		/// Prepare a single transaction by ensuring it has SignedTransactionBytes
		/// </summary>
		private static Proto.Transaction PrepareSingleTransaction(Proto.Transaction transaction)
		{
			if (transaction.SignedTransactionBytes.Length == 0)
			{
				transaction.SignedTransactionBytes = new Proto.SignedTransaction
				{
					BodyBytes = transaction.BodyBytes,
					SigMap = transaction.SigMap,

				}.ToByteString();

				return transaction;
			}

			return transaction;
		}
		/// <summary>
		/// Process a single transaction
		/// </summary>
		private static Proto.TransactionBody.DataOneofCase ProcessSingleTransaction(byte[] bytes, LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txsMap)
		{
			var transaction = Proto.Transaction.Parser.ParseFrom(bytes);
			var builtTransaction = PrepareSingleTransaction(transaction);
			var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(builtTransaction.SignedTransactionBytes);
			var txBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);

			AddTransactionToMap(builtTransaction, txBody, txsMap);

			return txBody.DataCase;
		}
		/// <summary>
		/// Process a list of transactions with integrity verification
		/// </summary>
		private static Proto.TransactionBody.DataOneofCase ProcessTransactionList(List<Proto.Transaction> transactionList, LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txsMap)
		{
			if (transactionList.Count == 0)
			{
				return Proto.TransactionBody.DataOneofCase.None;
			}

			var firstTransaction = transactionList[0];
			var firstSignedTransaction = Proto.SignedTransaction.Parser.ParseFrom(firstTransaction.SignedTransactionBytes);
			var firstTxBody = Proto.TransactionBody.Parser.ParseFrom(firstSignedTransaction.BodyBytes);
			var dataCase = firstTxBody.DataCase;

			foreach (Proto.Transaction transaction in transactionList)
			{
				var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(transaction.SignedTransactionBytes);
				var txBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);

				AddTransactionToMap(transaction, txBody, txsMap);
			}

			return dataCase;
		}
		private static bool PublicKeyIsInSigPairList(ByteString publicKeyBytes, IList<Proto.SignaturePair> sigPairList)
		{
			foreach (var pair in sigPairList)
			{
				if (pair.PubKeyPrefix.Equals(publicKeyBytes))
				{
					return true;
				}
			}

			return false;
		}
		private static void RequireProtoMatches(object? protoA, object? protoB, HashSet<string> ignoreSet, string thisFieldName)
		{
			var aIsNull = protoA == null;
			var bIsNull = protoB == null;

			if (aIsNull != bIsNull)
				ThrowProtoMatchException(thisFieldName, aIsNull ? "null" : "not null", bIsNull ? "null" : "not null");

			if (aIsNull) return;

			var protoAClass = protoA.GetType();
			var protoBClass = protoB.GetType();

			if (!protoAClass.Equals(protoBClass))
				ThrowProtoMatchException(thisFieldName, "of class " + protoAClass, "of class " + protoBClass);

			if (protoA is bool || protoA is int || protoA is long || protoA is float || protoA is Double || protoA is string || protoA is ByteString)
			{
				// System.out.println("values A = " + protoA.toString() + ", B = " + protoB.toString());
				if (!protoA.Equals(protoB))
				{
					ThrowProtoMatchException(thisFieldName, protoA.ToString(), protoB.ToString());
				}
			}

			foreach (var method in protoAClass.GetMethods())
			{
				if (method.GetParameters().Length != 0)
				{
					continue;
				}

				int methodModifiers = method.GetModifiers();
				if ((!Modifier.IsPublic(methodModifiers)) || Modifier.IsStatic(methodModifiers))
				{
					continue;
				}

				var methodName = method.Name;
				if (!methodName.StartsWith("get"))
				{
					continue;
				}

				var isList = methodName.EndsWith("List") && typeof(IList).IsAssignableFrom(method.ReturnType);
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
						bool hasA = (bool)hasMethod.Invoke(protoA, null)!;
						bool hasB = (bool)hasMethod.Invoke(protoB, null)!;

						if (!hasA.Equals(hasB))
							ThrowProtoMatchException(methodFieldName, hasA ? "present" : "not present", hasB ? "present" : "not present");

						if (!hasA)
							continue;
					}
					catch (MissingMethodException) { }
					catch (ArgumentException)
					{
						throw;
					}
					catch (Exception error)
					{
						throw new ArgumentException("fromBytes() failed due to error", error);
					}
				}

				try
				{
					var retvalA = method.Invoke(protoA, null);
					var retvalB = method.Invoke(protoB, null);

					if (isList)
					{
						var listA = (IList<T>?)retvalA;
						var listB = (IList<T>?)retvalB;

						if (listA?.Count != listB?.Count)
						{
							ThrowProtoMatchException(methodFieldName, "of size " + listA?.Count, "of size " + listB?.Count);
						}

						for (int i = 0; i < listA?.Count; i++)
						{
							// System.out.println("comparing " + thisFieldName + "." + methodFieldName + "[" + i + "]");
							RequireProtoMatches(listA?[i], listB?[i], ignoreSet, methodFieldName + "[" + i + "]");
						}
					}
					else
					{

						// System.out.println("comparing " + thisFieldName + "." + methodFieldName);
						RequireProtoMatches(retvalA, retvalB, ignoreSet, methodFieldName);
					}
				}
				catch (ArgumentException) { throw; }
				catch (Exception error)
				{
					throw new ArgumentException("fromBytes() failed due to error", error);
				}
			}
		}
		private static void ThrowProtoMatchException(string fieldName, string aWas, string bWas)
		{
			throw new ArgumentException("fromBytes() failed because " + fieldName + " fields in TransactionBody protobuf messages in the TransactionList did not match: A was " + aWas + ", B was " + bWas);
		}
	}

	/// <summary>
	/// Base class for all transactions that may be built and submitted to Hedera.
	/// </summary>
	/// <param name="<T>">The type of the transaction. Used to enable chaining.</param>
	public abstract partial class Transaction<T> : Executable<T, Proto.Transaction, Proto.TransactionResponse, TransactionResponse> where T : Transaction<T>
    {
		/// <summary>
		/// Default auto renew duration for accounts, contracts, topics, and files (entities)
		/// </summary>
		protected static readonly Duration DEFAULT_AUTO_RENEW_PERIOD = Duration.FromTimeSpan(TimeSpan.FromDays(90));
        /// <summary>
        /// Dummy account ID used to assist in deserializing incomplete Transactions.
        /// </summary>
        protected static readonly AccountId DUMMY_ACCOUNT_ID = new (0, 0, 0);
        /// <summary>
        /// Dummy transaction ID used to assist in deserializing incomplete Transactions.
        /// </summary>
        protected static readonly TransactionId DUMMY_TRANSACTION_ID = TransactionId.WithValidStart(DUMMY_ACCOUNT_ID, Timestamp.FromDateTimeOffset(DateTimeOffset.UnixEpoch));
        /// <summary>
        /// Default transaction duration
        /// </summary>
        private static readonly Duration DEFAULT_TRANSACTION_VALID_DURATION = Duration.FromTimeSpan(TimeSpan.FromSeconds(120));
        private static readonly string ATOMIC_BATCH_NODE_ACCOUNT_ID = "0.0.0";

        /// <summary>
        /// An SDK [Transaction] is composed of multiple, raw protobuf transactions. These should be functionally identical,
        /// except pointing to different nodes. When retrying a transaction after a network error or retry-able status
        /// response, we try a different transaction and thus a different node.
        /// </summary>
        protected List<Proto.Transaction> OuterTransactions = [];
		/// <summary>
		/// An SDK [Transaction] is composed of multiple, raw protobuf transactions. These should be functionally identical,
		/// except pointing to different nodes. When retrying a transaction after a network error or retry-able status
		/// response, we try a different transaction and thus a different node.
		/// </summary>
		public List<Proto.SignedTransaction> InnerSignedTransactions = [];
        /// <summary>
        /// A set of signatures corresponding to every unique public key used to sign the transaction.
        /// </summary>
        protected List<Proto.SignatureMap> SigPairLists = [];
        /// <summary>
        /// List of IDs for the transaction based on the operator because the transaction ID includes the operator's account
        /// </summary>
        protected LockableList<TransactionId> TransactionIds = [];
        /// <summary>
        /// publicKeys and signers are parallel Array. If the signer associated with a public key is null, that means that
        /// the private key associated with that public key has already contributed a signature to sigPairListBuilders, but
        /// the signer is not available (likely because this came from fromBytes())
        /// </summary>
        public  IList<PublicKey> PublicKeys = [];
        /// <summary>
        /// publicKeys and signers are parallel Array. If the signer associated with a public key is null, that means that
        /// the private key associated with that public key has already contributed a signature to sigPairListBuilders, but
        /// the signer is not available (likely because this came from fromBytes())
        /// </summary>
        protected List<Func<byte[], byte[]>?> Signers = [];
        /// <summary>
        /// The maximum transaction fee the client is willing to pay
        /// </summary>
        protected Hbar DefaultMaxTransactionFee = new (2);
        /// <summary>
        /// Should the transaction id be regenerated
        /// </summary>
		/// 
        protected bool? regenerateTransactionId = null;
        private string Memo = "";
        protected IList<CustomFeeLimit> customFeeLimits = [];

		/// <summary>
		/// Constructor.
		/// </summary>
		protected Transaction()
        {
            TransactionValidDuration = DEFAULT_TRANSACTION_VALID_DURATION;
            SourceTransactionBody = new Proto.TransactionBody();
        }
		/// <summary>
		/// This constructor is used to construct from a scheduled transaction body
		/// </summary>
		/// <param name="txBody"></param>
		internal Transaction(Proto.TransactionBody txBody)
		{
			TransactionValidDuration = DEFAULT_TRANSACTION_VALID_DURATION;
			MaxTransactionFee = Hbar.FromTinybars((long)txBody.TransactionFee);
			TransactionMemo = txBody.Memo;
			SourceTransactionBody = txBody;
		}
		/// <summary>
		/// This constructor is used to construct via fromBytes
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal Transaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs)
        {
			LinkedDictionary<AccountId, Proto.Transaction> transactionMap = txs.First().Value;

            if (transactionMap.Count != 0 && transactionMap.Keys.First().Equals(DUMMY_ACCOUNT_ID) && BatchKey != null)
            {
                // If the first account ID is a dummy account ID, then only the source TransactionBody needs to be copied.
                var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(transactionMap.Values.First().SignedTransactionBytes);
                SourceTransactionBody = ParseTransactionBody(signedTransaction.BodyBytes);
            }
            else
            {
                var txCount = txs.Keys.Count;
                var nodeCount = txs.Values.First().Count;

                NodeAccountIds.EnsureCapacity(nodeCount);
                
				SigPairLists = new List<Proto.SignatureMap>(nodeCount * txCount);
                OuterTransactions = new List<Proto.Transaction>(nodeCount * txCount);
                InnerSignedTransactions = new List<Proto.SignedTransaction>(nodeCount * txCount);

                TransactionIds.EnsureCapacity(txCount);
                foreach (var transactionEntry in txs)
                {
                    if (!transactionEntry.Key.Equals(DUMMY_TRANSACTION_ID))
                    {
                        TransactionIds.Add(transactionEntry.Key);
                    }

                    foreach (var nodeEntry in transactionEntry.Value)
                    {
                        if (NodeAccountIds.Count != nodeCount)
							NodeAccountIds.Add(nodeEntry.Key);

						var transaction = Proto.SignedTransaction.Parser.ParseFrom(nodeEntry.Value.SignedTransactionBytes);
                        OuterTransactions.Add(nodeEntry.Value);
                        SigPairLists.Add(transaction.SigMap);
                        InnerSignedTransactions.Add(transaction);
                        if (PublicKeys.Count == 0)
                        {
                            foreach (var sigPair in transaction.SigMap.SigPair)
                            {
                                PublicKeys.Add(PublicKey.FromBytes(sigPair.PubKeyPrefix.ToByteArray()));
                                Signers.Add(null);
                            }
                        }
                    }
                }

                NodeAccountIds.Remove(new AccountId(0, 0, 0));

                // Verify that transaction bodies match
                for (int i = 0; i < txCount; i++)
                {
                    Proto.TransactionBody? firstTxBody = null;
                    for (int j = 0; j < nodeCount; j++)
                    {
                        int k = i * nodeCount + j;
                        var txBody = ParseTransactionBody(InnerSignedTransactions[k].BodyBytes);
                        if (firstTxBody == null)
							firstTxBody = txBody;
						else RequireProtoMatches(firstTxBody, txBody, ["NodeAccountID"], "TransactionBody");
					}
                }

                SourceTransactionBody = ParseTransactionBody(InnerSignedTransactions[0].BodyBytes);
            }

            TransactionValidDuration = Utils.DurationConverter.FromProtobuf(SourceTransactionBody.TransactionValidDuration);
            MaxTransactionFee = Hbar.FromTinybars(SourceTransactionBody.TransactionFee);
            TransactionMemo = SourceTransactionBody.Memo;
            
			customFeeLimits = [.. SourceTransactionBody.MaxCustomFees.Select(_ => CustomFeeLimit.FromProtobuf(_))];
            BatchKey = Key.FromProtobufKey(SourceTransactionBody.BatchKey);

			// The presence of signatures implies the Transaction should be frozen.
			if (PublicKeys.Count != 0)
				FrozenBodyBuilder = SourceTransactionBody;

		}
        
        

        /// <summary>
        /// The maximum transaction fee the operator (paying account) is willing to pay.
        /// </summary>
        public Hbar? MaxTransactionFee
        {
            get;
            set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Extract the memo for the transaction.
		/// </summary>
		/// <returns>the memo for the transaction</returns>
		public string TransactionMemo
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Sets the duration that this transaction is valid for.
		/// <p>
		/// This is defaulted by the SDK to 120 seconds (or two minutes).
		/// </summary>
		public Duration TransactionValidDuration
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Transaction constructors end their work by setting sourceTransactionBody. The expectation is that the Transaction
		/// subclass constructor will pick up where the Transaction superclass constructor left off, and will unpack the data
		/// in the transaction body.
		/// </summary>
		public Proto.TransactionBody SourceTransactionBody { get; internal set; }
		/// <summary>
		/// The builder that gets re-used to build each outer transaction. freezeWith() will create the frozenBodyBuilder.
		/// The presence of frozenBodyBuilder indicates that this transaction is frozen.
		/// </summary>
		public Proto.TransactionBody? FrozenBodyBuilder { get; internal set; }
		/// <summary>
		/// The key that will sign the batch of which this Transaction is a part of.
		/// </summary>
		public Key? BatchKey 
		{ 
			get;
			set 
			{ 
				RequireNotFrozen(); 
				field = value; 
			} 
		}
		/// <summary>
		/// Should the transaction id be regenerated.
		/// </summary>
		/// <returns>should the transaction id be regenerated</returns>
		public bool ShouldRegenerateTransactionId { get; set; }
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
		public TransactionId TransactionId
		{
			get
			{
				if (TransactionIds.Count == 0 || !IsFrozen())
				{
					throw new InvalidOperationException("No transaction ID generated yet. Try freezing the transaction or manually setting the transaction ID.");
				}

				return TransactionIds.SetLocked(true).GetCurrent();
			}
			set
			{
				RequireNotFrozen();
				TransactionIds.SetList([value]).SetLocked(true);
			}
		}

		public override TransactionId TransactionIdInternal
		{
			get => TransactionIds.GetCurrent();
		}

		/// <summary>
		/// Called in {@link #freezeWith(Client)} just before the transaction body is built. The intent is for the derived
		/// class to assign their data variant to the transaction body.
		/// </summary>
		public abstract void OnFreeze(Proto.TransactionBody bodyBuilder);
		/// <summary>
		/// Called in {@link #schedule()} when converting transaction into a scheduled version.
		/// </summary>
		public abstract void OnScheduled(Proto.SchedulableTransactionBody scheduled);
		public abstract void ValidateChecksums(Client client);

		protected override bool IsBatchedAndNotBatchTransaction()
		{
			return BatchKey != null && this is not BatchTransaction;
		}

		/// <summary>
		/// Converts transaction into a scheduled version
		/// </summary>
		/// <param name="bodyBuilder">the transaction's body builder</param>
		/// <returns>the scheduled transaction</returns>
		protected virtual ScheduleCreateTransaction DoSchedule(Proto.TransactionBody bodyBuilder)
		{
			Proto.SchedulableTransactionBody proto = new ()
			{
				TransactionFee = bodyBuilder.TransactionFee,
				Memo = bodyBuilder.Memo,
			};
			
			proto.MaxCustomFees.AddRange(bodyBuilder.MaxCustomFees);

			OnScheduled(proto);

			var scheduled = new ScheduleCreateTransaction
			{
				ScheduledTransactionBody = proto
			};

			if (TransactionIds.Count > 0)
				scheduled.TransactionId = TransactionIds[0];

			return scheduled;
		}
		protected virtual IDictionary<AccountId, IDictionary<PublicKey, byte[]>> GetSignaturesAtOffset(int offset)
		{
			var map = new Dictionary<AccountId, IDictionary<PublicKey, byte[]>>(NodeAccountIds.Count);

			for (int i = 0; i < NodeAccountIds.Count; i++)
			{
				var sigMap = SigPairLists[i + offset];
				var nodeAccountId = NodeAccountIds[i];
				var keyMap = map.TryGetValue(nodeAccountId, out IDictionary<PublicKey, byte[]>? value) 
					? value 
					: new Dictionary<PublicKey, byte[]>();
				
				map.Add(nodeAccountId, keyMap);

				foreach (var sigPair in sigMap.SigPair)
				{
					keyMap.Add(PublicKey.FromBytes(sigPair.PubKeyPrefix.ToByteArray()), sigPair.Ed25519.ToByteArray());
				}
			}

			return map;
		}
		/// <summary>
		/// Checks if a public key is already added to the transaction
		/// </summary>
		/// <param name="key">the public key</param>
		/// <returns>if the public key is already added</returns>
		protected virtual bool KeyAlreadySigned(PublicKey key)
		{
			return PublicKeys.Contains(key);
		}
		/// <summary>
		/// Throw an exception if the transaction is frozen.
		/// </summary>
		internal virtual void RequireNotFrozen()
		{
			if (IsFrozen())
			{
				throw new InvalidOperationException("transaction is immutable; it has at least one signature or has been explicitly frozen");
			}
		}
		/// <summary>
		/// Throw an exception if there is not exactly one node id set.
		/// </summary>
		internal virtual void RequireOneNodeAccountId()
		{
			if (NodeAccountIds.Count != 1)
				throw new InvalidOperationException("transaction did not have exactly one node ID set");
		}
		protected virtual Proto.TransactionBody SpawnBodyBuilder(Client? client)
		{
			var builder = new Proto.TransactionBody
			{
				Memo = Memo,
				TransactionFee = (ulong)(MaxTransactionFee ?? client?.DefaultMaxTransactionFee ?? DefaultMaxTransactionFee).ToTinybars(),
				TransactionValidDuration = Utils.DurationConverter.ToProtobuf(TransactionValidDuration),
			};

			if (BatchKey is not null)
				builder.BatchKey = BatchKey.ToProtobufKey();

			builder.MaxCustomFees.AddRange(customFeeLimits.Select(_ => _.ToProtobuf()));

			return builder;
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
				Freeze();

			if (KeyAlreadySigned(publicKey))
			{
				// noinspection unchecked
				return (T)this;
			}

			TransactionIds.SetLocked(true);
			NodeAccountIds.SetLocked(true);
			for (int i = 0; i < OuterTransactions.Count; i++)
				OuterTransactions[i] = null;

			PublicKeys.Add(publicKey);
			Signers.Add(null);
			SigPairLists[0].SigPair.Add(publicKey.ToSignaturePairProtobuf(signature));

			// noinspection unchecked
			return (T)this;
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
			if (InnerSignedTransactions.Count == 0)
			{
				// noinspection unchecked
				return (T)this;
			}

			TransactionIds.SetLocked(true);

			for (int index = 0; index < InnerSignedTransactions.Count; index++)
				if (ProcessedSignatureForTransaction(index, publicKey, signature, transactionID, nodeID))
					UpdateTransactionState(publicKey);


			// noinspection unchecked
			return (T)this;
		}
		/// <summary>
		/// Build all the transactions.
		/// </summary>
		public virtual void BuildAllTransactions()
        {
            TransactionIds.SetLocked(true);
            NodeAccountIds.SetLocked(true);

            for (var i = 0; i < InnerSignedTransactions.Count; ++i)
				BuildTransaction(i);
		}
        /// <summary>
        /// Will build the specific transaction at {@code index} This function is only ever called after the transaction is
        /// frozen.
        /// </summary>
        /// <param name="index">the index of the transaction to be built</param>
        public virtual void BuildTransaction(int index)
        {
            // Check if transaction is already built.
            // Every time a signer is added via sign() or signWith(), all outerTransactions are nullified.
            if (OuterTransactions[index] != null && OuterTransactions[index].SignedTransactionBytes.Length != 0)
				return;

			SignTransaction(index);

            OuterTransactions[index] = new Proto.Transaction
			{
				SigMap = SigPairLists[index],
				SignedTransactionBytes = InnerSignedTransactions[index].ToByteString(),
			};
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

			if (TransactionIds.Count == 0)
			{
				if (client != null)
				{
					var @operator = client.Operator_;

					if (@operator != null)
					{
						// Set a default transaction ID, generated from the operator account ID
						TransactionIds.SetList([TransactionId.Generate(@operator.AccountId)]);
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

			if (NodeAccountIds.Count == 0)
			{
				if (client == null)
					throw new InvalidOperationException("`client` must be provided or both `nodeId` and `transactionId` must be set");

				try
				{
					if (BatchKey == null)
						NodeAccountIds.SetList(client.Network_.GetNodeAccountIdsForExecute());
					else 
						NodeAccountIds.SetList([AccountId.FromString(ATOMIC_BATCH_NODE_ACCOUNT_ID)]);
				}
				catch (ThreadInterruptedException e)
				{
					throw new Exception(string.Empty, e);
				}
			}

			FrozenBodyBuilder = SpawnBodyBuilder(client);
			FrozenBodyBuilder.TransactionID = TransactionIds[0].ToProtobuf();

			int requiredChunks = GetRequiredChunks();
			
			OnFreeze(FrozenBodyBuilder);
			GenerateTransactionIds(TransactionIds[0], requiredChunks);
			WipeTransactionLists(requiredChunks);

			regenerateTransactionId = regenerateTransactionId != null ? regenerateTransactionId : client?.DefaultRegenerateTransactionId;

			// noinspection unchecked
			return (T)this;
		}
		/// <summary>
		/// There must be at least one chunk.
		/// </summary>
		/// <returns>there is 1 required chunk</returns>
		public virtual int GetRequiredChunks()
		{
			return 1;
		}
		/// <summary>
		/// Extract list of account id and public keys.
		/// </summary>
		/// <returns>the list of account id and public keys</returns>
		public virtual IDictionary<AccountId, IDictionary<PublicKey, byte[]>> GetSignatures()
		{
			if (!IsFrozen())
				throw new InvalidOperationException("Transaction must be frozen in order to have signatures.");

			if (PublicKeys.Count == 0)
				return new Dictionary<AccountId, IDictionary<PublicKey, byte[]>>();

			BuildAllTransactions();

			return GetSignaturesAtOffset(0);
		}
		/// <summary>
		/// Returns a list of SignableNodeTransactionBodyBytes objects for each signed transaction in the transaction list.
		/// The NodeID represents the node that this transaction is signed for.
		/// The TransactionID is useful for signing chunked transactions like FileAppendTransaction,
		/// since they can have multiple transaction ids.
		/// </summary>
		/// <returns>List of SignableNodeTransactionBodyBytes</returns>
		/// <exception cref="RuntimeException">if transaction is not frozen or protobuf parsing fails</exception>
		public virtual List<SignableNodeTransactionBodyBytes> GetSignableNodeBodyBytesList()
		{
			if (!IsFrozen())
				throw new Exception("Transaction is not frozen");

			List<SignableNodeTransactionBodyBytes> signableNodeTransactionBodyBytesList = new();

			for (int i = 0; i < InnerSignedTransactions.Count; i++)
			{
				Proto.SignedTransaction signableNodeTransactionBodyBytes = InnerSignedTransactions[i];
				Proto.TransactionBody body = ParseTransactionBody(signableNodeTransactionBodyBytes.BodyBytes);
				
				AccountId nodeID = AccountId.FromProtobuf(body.NodeAccountID);
				TransactionId transactionID = TransactionId.FromProtobuf(body.TransactionID);

				signableNodeTransactionBodyBytesList.Add(new SignableNodeTransactionBodyBytes(nodeID, transactionID, signableNodeTransactionBodyBytes.BodyBytes.ToByteArray()));
			}

			return signableNodeTransactionBodyBytesList;
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

			return MakeRequest().CalculateSize();
		}
		/// <summary>
		/// This method retrieves the transaction body size
		/// </summary>
		/// <returns></returns>
		public virtual int GetTransactionBodySize()
		{
			if (!IsFrozen())
				throw new InvalidOperationException("transaction must have been frozen before getting it's body size, try calling `freeze`");

			return FrozenBodyBuilder?.CalculateSize() ?? 0;
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

			TransactionIds.SetLocked(true);
			NodeAccountIds.SetLocked(true);

			var index = TransactionIds.Index * NodeAccountIds.Count + NodeAccountIds.Index;

			BuildTransaction(index);

			return GenerateHash(OuterTransactions[index].SignedTransactionBytes.ToByteArray());
		}
		/// <summary>
		/// Extract the list of account id and hash records.
		/// </summary>
		/// <returns>the list of account id and hash records</returns>
		public virtual IDictionary<AccountId, byte[]> GetTransactionHashPerNode()
		{
			if (!IsFrozen())
			{
				throw new InvalidOperationException("transaction must have been frozen before calculating the hash will be stable, try calling `freeze`");
			}

			BuildAllTransactions();
			var hashes = new Dictionary<AccountId, byte[]>();
			for (var i = 0; i < OuterTransactions.Count; i++)
			{
				hashes.Add(NodeAccountIds[i], GenerateHash(OuterTransactions[i].SignedTransactionBytes.ToByteArray()));
			}

			return hashes;
		}
		/// <summary>
		/// Generate transaction id's.
		/// </summary>
		/// <param name="initialTransactionId">the initial transaction id</param>
		/// <param name="count">the number of id's to generate.</param>
		public virtual void GenerateTransactionIds(TransactionId initialTransactionId, int count)
		{
			var locked = TransactionIds.IsLocked;

			TransactionIds.SetLocked(false);

			if (count == 1)
			{
				TransactionIds.SetList([initialTransactionId]);
				return;
			}

			var nextTransactionId = initialTransactionId.ToProtobuf();
			TransactionIds.EnsureCapacity(count);
			TransactionIds.Clear();
			for (int i = 0; i < count; i++)
			{
				TransactionIds.Add(TransactionId.FromProtobuf(nextTransactionId));

				// add 1 ns to the validStart to make cascading transaction IDs
				var nextValidStart = nextTransactionId.TransactionValidStart;
				nextValidStart.Nanos = nextValidStart.Nanos + 1;
				nextTransactionId.TransactionValidStart = nextValidStart;
			}

			TransactionIds.SetLocked(locked);
		}
		/// <summary>
		/// Check if transaction is frozen.
		/// </summary>
		/// <returns>is the transaction frozen</returns>
		public virtual bool IsFrozen()
		{
			return FrozenBodyBuilder != null;
		}
		public virtual TransactionResponse MapResponse(Proto.TransactionResponse transactionResponse, AccountId nodeId, Proto.Transaction request)
		{
			var transactionId = TransactionIdInternal;
			var hash = GenerateHash(request.SignedTransactionBytes.ToByteArray());

			// advance is needed for chunked transactions
			TransactionIds.Advance();

			return new TransactionResponse(nodeId, transactionId, hash, null, this);
		}
		public virtual ResponseStatus MapResponseStatus(Proto.TransactionResponse transactionResponse)
		{
			return (ResponseStatus)transactionResponse.NodeTransactionPrecheckCode;
		}
		public virtual Transaction<T> RegenerateTransactionId(Client client)
		{
			ArgumentNullException.ThrowIfNull(client.OperatorAccountId);
			TransactionIds.SetLocked(false);
			var newTransactionID = TransactionId.Generate(client.OperatorAccountId);
			TransactionIds[TransactionIds.Index] = newTransactionID;
			TransactionIds.SetLocked(true);
			return this;
		}
		/// <summary>
		/// Extract the scheduled transaction.
		/// </summary>
		/// <returns>the scheduled transaction</returns>
		public virtual ScheduleCreateTransaction Schedule()
		{
			RequireNotFrozen();
			if (NodeAccountIds.Count != 0)
			{
				throw new InvalidOperationException("The underlying transaction for a scheduled transaction cannot have node account IDs set");
			}

			var bodyBuilder = SpawnBodyBuilder(null);
			OnFreeze(bodyBuilder);
			return DoSchedule(bodyBuilder);
		}
		/// <summary>
		/// Will sign the specific transaction at {@code index} This function is only ever called after the transaction is
		/// frozen.
		/// </summary>
		/// <param name="index">the index of the transaction to sign</param>
		public virtual void SignTransaction(int index)
        {
            var bodyBytes = InnerSignedTransactions[index].BodyBytes.ToByteArray();
            var thisSigPairList = SigPairLists[index].SigPair;
            for (var i = 0; i < PublicKeys.Count; i++)
            {
                if (Signers[i] == null)
                {
                    continue;
                }

                if (PublicKeyIsInSigPairList(ByteString.CopyFrom(PublicKeys[i].ToBytesRaw()), thisSigPairList))
                {
                    continue;
                }

                var signatureBytes = Signers[i].Invoke(bodyBytes);
                SigPairLists[index].SigPair.Add(PublicKeys[i].ToSignaturePairProtobuf(signatureBytes));
            }
        }
		/// <summary>
		/// Sign the transaction with the configured client.
		/// </summary>
		/// <param name="client">the configured client</param>
		/// <returns>the signed transaction</returns>
		public virtual T SignWithOperator(Client client)
		{
			if (client.Operator_ == null)
				throw new InvalidOperationException("`client` must have an `operator` to sign with the operator");

			if (!IsFrozen())
				FreezeWith(client);

			return SignWith(client.Operator_.PublicKey, client.Operator_.TransactionSigner);
		}
		/// <summary>
		/// Sign the transaction.
		/// </summary>
		/// <param name="publicKey">the public key</param>
		/// <param name="transactionSigner">the key list</param>
		/// <returns>{@code this}</returns>
		public virtual T SignWith(PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
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

			for (int i = 0; i < OuterTransactions.Count; i++)
			{
				OuterTransactions[i] = null;
			}

			PublicKeys.Add(publicKey);
			Signers.Add(transactionSigner);

			// noinspection unchecked
			return (T)this;
		}
		/// <summary>
		/// Extract a byte array representation.
		/// </summary>
		/// <returns>the byte array representation</returns>
		public virtual byte[] ToBytes()
		{
			var list = new Proto.TransactionList();

			// If no nodes have been selected yet,
			// the new TransactionBody can be used to build a Transaction protobuf object.
			if (NodeAccountIds.Count == 0)
			{
				var bodyBuilder = SpawnBodyBuilder(null);
				if (TransactionIds.Count != 0)
				{
					bodyBuilder.TransactionID = TransactionIds[0].ToProtobuf();
				}

				OnFreeze(bodyBuilder);

				list.TransactionList_.Add(new Proto.Transaction
				{
					SignedTransactionBytes = new Proto.SignedTransaction
					{
						BodyBytes = bodyBuilder.ToByteString()

					}.ToByteString()
				});
			}
			else
			{

				// Generate the SignedTransaction protobuf objects if the Transaction's not frozen.
				if (!IsFrozen())
				{
					FrozenBodyBuilder = SpawnBodyBuilder(null);
					if (TransactionIds.Count != 0)
					{
						FrozenBodyBuilder.TransactionID = TransactionIds[0].ToProtobuf();
					}

					OnFreeze(FrozenBodyBuilder);
					int requiredChunks = GetRequiredChunks();
					if (TransactionIds.Count != 0)
					{
						GenerateTransactionIds(TransactionIds[0], requiredChunks);
					}

					WipeTransactionLists(requiredChunks);
				}


				// Build all the Transaction protobuf objects and add them to the TransactionList protobuf object.
				BuildAllTransactions();

				foreach (var transaction in OuterTransactions)
					list.TransactionList_.Add(transaction);
			}

			return list.ToByteArray();
		}
		/// <summary>
		/// Wipe / reset the transaction list.
		/// </summary>
		/// <param name="requiredChunks">the number of required chunks</param>
		public virtual void WipeTransactionLists(int requiredChunks)
		{
			if (TransactionIds.Count != 0)
				FrozenBodyBuilder.TransactionID = TransactionIdInternal.ToProtobuf();

			OuterTransactions = new List<Proto.Transaction>(NodeAccountIds.Count);
			SigPairLists = new List<Proto.SignatureMap>(NodeAccountIds.Count);
			InnerSignedTransactions = new List<Proto.SignedTransaction>(NodeAccountIds.Count);

			foreach (AccountId nodeId in NodeAccountIds)
			{
				FrozenBodyBuilder.NodeAccountID = nodeId.ToProtobuf();

				SigPairLists.Add(new Proto.SignatureMap());
				OuterTransactions.Add(null);
				InnerSignedTransactions.Add(new Proto.SignedTransaction
				{
					BodyBytes = FrozenBodyBuilder.ToByteString()
				});
			}
		}

		public override Method<Proto.Transaction, Proto.TransactionResponse> GetMethod()
		{
			MethodDescriptor methoddescriptor = GetMethodDescriptor();

			IMessage input = (IMessage)Activator.CreateInstance(methoddescriptor.InputType.ClrType)!;
			IMessage output = (IMessage)Activator.CreateInstance(methoddescriptor.OutputType.ClrType)!;

			return new Method<Proto.Transaction, Proto.TransactionResponse>(
				type: MethodType.Unary,
				name: methoddescriptor.Name,
				serviceName: methoddescriptor.Service.FullName,
				requestMarshaller: Marshallers.Create(r => r.ToByteArray(), data => Proto.Transaction.Parser.ParseFrom(data)),
				responseMarshaller: Marshallers.Create(r => r.ToByteArray(), data => Proto.TransactionResponse.Parser.ParseFrom(data)));
		}

		public override ExecutionState GetExecutionState(ResponseStatus status, Proto.TransactionResponse response)
		{
			if (status == ResponseStatus.TransactionExpired)
			{
				if (regenerateTransactionId ?? false || TransactionIds.IsLocked)
					return ExecutionState.RequestError;
				else
				{
					var firstTransactionId = TransactionIds[0];
					var accountId = firstTransactionId.AccountId;

					GenerateTransactionIds(TransactionId.Generate(accountId), TransactionIds.Count);
					WipeTransactionLists(TransactionIds.Count);
					
					return ExecutionState.Retry;
				}
			}

			return base.GetExecutionState(status, response);
		}
		public override void OnExecute(Client client)
		{
			if (!IsFrozen())
				FreezeWith(client);

			var accountId = TransactionIds[0].AccountId;

			if (client.AutoValidateChecksums)
				try
				{
					accountId.ValidateChecksum(client);
					ValidateChecksums(client);
				}
				catch (BadEntityIdException exc)
				{
					throw new ArgumentException(exc.Message);
				}

			var operatorId = client.OperatorAccountId;

			if (operatorId != null && operatorId.Equals(accountId))
			{
				// on execute, sign each transaction with the operator, if present
				// and we are signing a transaction that used the default transaction ID
				SignWithOperator(client);
			}
		}
		public override Task OnExecuteAsync(Client client)
		{
			OnExecute(client);

			return Task.CompletedTask;
		}
		public override Proto.Transaction MakeRequest()
        {
            var index = NodeAccountIds.Index + (TransactionIds.Index * NodeAccountIds.Count);

            BuildTransaction(index);
            return OuterTransactions[index];
        }
		/// <summary>
		/// Set the account IDs of the nodes that this transaction will be submitted to.
		/// <p>
		/// Providing an explicit node account ID interferes with client-side load balancing of the network. By default, the
		/// SDK will pre-generate a transaction for 1/3 of the nodes on the network. If a node is down, busy, or otherwise
		/// reports a fatal error, the SDK will try again with a different node.
		/// </summary>
		/// <param name="NodeAccountIds">The list of node AccountIds to be set</param>
		/// <returns>{@code this}</returns>
		public virtual T SetNodeAccountIds(IList<AccountId> nodeaccountids)
		{
			RequireNotFrozen();

			NodeAccountIds = [.. nodeaccountids];
		}
		public override string ToString()
		{

			// NOTE: regex is for removing the instance address from the default debug output
			Proto.TransactionBody body = SpawnBodyBuilder(null);
			
			if (TransactionIds.Count != 0)
				body.TransactionID = TransactionIds[0].ToProtobuf();

			if (NodeAccountIds.Count != 0)
				body.NodeAccountID = NodeAccountIds[0].ToProtobuf();

			OnFreeze(body);

			return Regex.Replace(body.ToString(), "@[A-Za-z0-9]+", string.Empty);
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
			ArgumentNullException.ThrowIfNull(batchKey);
			this.BatchKey = batchKey;
			SignWithOperator(client);

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
			return SignWith(privateKey.GetPublicKey(), privateKey.Sign);
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
			Proto.SignatureMap sigMapBuilder = SigPairLists[index];

			// Check if the signature is already in the signature map
			if (IsSignatureAlreadyPresent(sigMapBuilder, publicKey))
				return false;

			// Add the signature to the signature map
			Proto.SignaturePair newSigPair = publicKey.ToSignaturePairProtobuf(signature);
			sigMapBuilder.SigPair.Add(newSigPair);

			return true;
		}
		/// <summary>
		/// Checks if a signature for the given public key already exists.
		/// </summary>
		/// <param name="sigMapBuilder">The signature map builder</param>
		/// <param name="publicKey">The public key to check</param>
		/// <returns>true if signature already exists, false otherwise</returns>
		private bool IsSignatureAlreadyPresent(Proto.SignatureMap sigMapBuilder, PublicKey publicKey)
		{
			foreach (Proto.SignaturePair sig in sigMapBuilder.SigPair)
				if (Equals(sig.PubKeyPrefix.ToByteArray(), publicKey.ToBytesRaw()))
					return true;

			return false;
		}
		/// <summary>
		/// Checks if the transaction body matches the target transaction ID and node ID.
		/// </summary>
		/// <param name="body">The transaction body to check</param>
		/// <param name="targetTransactionID">The target transaction ID to match against</param>
		/// <param name="targetNodeID">The target node ID to match against</param>
		/// <returns>true if both the transaction ID and node ID match the targets, false otherwise</returns>
		private bool MatchesTargetTransactionAndNode(Proto.TransactionBody body, TransactionId targetTransactionID, AccountId targetNodeID)
        {
            TransactionId bodyTxID = TransactionId.FromProtobuf(body.TransactionID);
            AccountId bodyNodeID = AccountId.FromProtobuf(body.NodeAccountID);

            return bodyTxID.ToString().Equals(targetTransactionID.ToString()) && bodyNodeID.ToString().Equals(targetNodeID.ToString());
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
			Proto.SignedTransaction temp = InnerSignedTransactions[index];
			Proto.TransactionBody body = ParseTransactionBody(temp);

			if (body == null)
				return false;

			if (!MatchesTargetTransactionAndNode(body, transactionID, nodeID))
				return false;

			return AddSignatureIfNotExists(index, publicKey, signature);
		}
		/// <summary>
		/// Updates the transaction state after adding a signature.
		/// </summary>
		/// <param name="publicKey">The public key that was added</param>
		private void UpdateTransactionState(PublicKey publicKey)
        {
            PublicKeys.Add(publicKey);
            Signers.Add(null);
        }
    }
}