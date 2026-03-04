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
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.LiveHashes;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Systems;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Topic;

using Org.BouncyCastle.Crypto.Digests;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Transactions
{
	public static class Transaction
	{
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:FromDays(90)"]/*' />
		internal static readonly TimeSpan DEFAULT_AUTO_RENEW_PERIOD = TimeSpan.FromDays(90);
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="T:Unknown"]/*' />
		internal static readonly AccountId DUMMY_ACCOUNT_ID = new(0, 0, 0);
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:WithValidStart(DUMMY_ACCOUNT_ID,DateTimeOffset.)"]/*' />
		internal static readonly TransactionId DUMMY_TRANSACTION_ID = TransactionId.WithValidStart(DUMMY_ACCOUNT_ID, DateTimeOffset.UnixEpoch);
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:FromSeconds(120)"]/*' />
		internal static readonly TimeSpan DEFAULT_TRANSACTION_VALID_DURATION = TimeSpan.FromSeconds(120);
		internal static readonly string ATOMIC_BATCH_NODE_ACCOUNT_ID = "0.0.0";

		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:FromBytes``1(System.Byte[])"]/*' />
		public static T FromBytes<T>(byte[] bytes) where T : Transaction<T>
		{
			var list = Proto.TransactionList.Parser.ParseFrom(bytes);
			var txsMap = new DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>>();

			Proto.TransactionBody.DataOneofCase dataCase;

			if (list.TransactionList_.Count != 0)
				dataCase = ProcessTransactionList([.. list.TransactionList_], txsMap);
			else dataCase = ProcessSingleTransaction(bytes, txsMap);

			return CreateTransactionFromDataCase<T>(dataCase, txsMap);
		}

		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:GenerateHash(System.Byte[])"]/*' />
		public static byte[] GenerateHash(byte[] bytes)
		{
			var digest = new Sha384Digest();
			var hash = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(bytes, 0, bytes.Length);
			digest.DoFinal(hash, 0);
			return hash;
		}

		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:AddTransactionToMap(Proto.Transaction,Proto.TransactionBody,DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal static void AddTransactionToMap(Proto.Transaction transaction, Proto.TransactionBody txBody, DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txsMap)
		{
			var account = txBody.NodeAccountID is not null ? AccountId.FromProtobuf(txBody.NodeAccountID) : DUMMY_ACCOUNT_ID;
			var transactionId = txBody.TransactionID is not null ? TransactionId.FromProtobuf(txBody.TransactionID) : DUMMY_TRANSACTION_ID;
			var linked = txsMap.ContainsKey(transactionId) ? txsMap[transactionId] : new DictionaryLinked<AccountId, Proto.Transaction>();

			linked.Add(account, transaction);
			txsMap.Add(transactionId, linked);
		}

		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:CreateTransactionFromDataCase``1(Proto.TransactionBody.DataOneofCase,DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal static T CreateTransactionFromDataCase<T>(Proto.TransactionBody.DataOneofCase dataCase, DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) where T : Transaction<T>
		{
			return dataCase switch
			{
				Proto.TransactionBody.DataOneofCase.ContractCall => new ContractExecuteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ContractCreateInstance => new ContractCreateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ContractUpdateInstance => new ContractUpdateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ContractDeleteInstance => new ContractDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.EthereumTransaction => new EthereumTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.CryptoAddLiveHash => new LiveHashAddTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.CryptoCreateAccount => new AccountCreateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.CryptoDelete => new AccountDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.CryptoDeleteLiveHash => new LiveHashDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.CryptoTransfer => new TransferTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.CryptoUpdateAccount => new AccountUpdateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.FileAppend => new FileAppendTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.FileCreate => new FileCreateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.FileDelete => new FileDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.FileUpdate => new FileUpdateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.NodeCreate => new NodeCreateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.NodeUpdate => new NodeUpdateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.NodeDelete => new NodeDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.SystemDelete => new SystemDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.SystemUndelete => new SystemUndeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.Freeze => new FreezeTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ConsensusCreateTopic => new TopicCreateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ConsensusUpdateTopic => new TopicUpdateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ConsensusDeleteTopic => new TopicDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ConsensusSubmitMessage => new TopicMessageSubmitTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenAssociate => new TokenAssociateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenBurn => new TokenBurnTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenCreation => new TokenCreateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenDeletion => new TokenDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenDissociate => new TokenDissociateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenFreeze => new TokenFreezeTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenGrantKyc => new TokenGrantKycTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenMint => new TokenMintTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenRevokeKyc => new TokenRevokeKycTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenUnfreeze => new TokenUnfreezeTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenUpdate => new TokenUpdateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenUpdateNfts => new TokenUpdateNftsTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenWipe => new TokenWipeTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenFeeScheduleUpdate => new TokenFeeScheduleUpdateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ScheduleCreate => new ScheduleCreateTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ScheduleDelete => new ScheduleDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.ScheduleSign => new ScheduleSignTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenPause => new TokenPauseTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenUnpause => new TokenUnpauseTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenReject => new TokenRejectTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenAirdrop => new TokenAirdropTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenCancelAirdrop => new TokenCancelAirdropTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.TokenClaimAirdrop => new TokenClaimAirdropTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.CryptoApproveAllowance => new AccountAllowanceApproveTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.CryptoDeleteAllowance => new AccountAllowanceDeleteTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.AtomicBatch => new BatchTransaction(txs) as T,
				Proto.TransactionBody.DataOneofCase.HookStore => new HookStoreTransaction(txs) as T,

				_ => throw new ArgumentException("parsed transaction body has no data")

			} ?? throw new ArgumentException("transaction body has no counterpart");
		}
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:ProcessSingleTransaction(System.Byte[],DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal static Proto.TransactionBody.DataOneofCase ProcessSingleTransaction(byte[] bytes, DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txsMap)
		{
			var transaction = Proto.Transaction.Parser.ParseFrom(bytes);
			var builtTransaction = PrepareSingleTransaction(transaction);
			var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(builtTransaction.SignedTransactionBytes);
			var txBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);

			AddTransactionToMap(builtTransaction, txBody, txsMap);

			return txBody.DataCase;
		}
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:ProcessTransactionList(System.Collections.Generic.List{Proto.Transaction},DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal static Proto.TransactionBody.DataOneofCase ProcessTransactionList(List<Proto.Transaction> transactionList, DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txsMap)
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

		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:FromScheduledTransaction``1(Proto.SchedulableTransactionBody)"]/*' />
		public static T FromScheduledTransaction<T>(Proto.SchedulableTransactionBody scheduled) where T : Transaction<T>
		{
			T? transaction = null;

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
					transaction = new ContractExecuteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractCreateInstance:
					proto.ContractCreateInstance = scheduled.ContractCreateInstance;
					transaction = new ContractCreateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractUpdateInstance:
					proto.ContractUpdateInstance = scheduled.ContractUpdateInstance;
					transaction = new ContractUpdateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractDeleteInstance:
					proto.ContractDeleteInstance = scheduled.ContractDeleteInstance;
					transaction = new ContractDeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoApproveAllowance:
					proto.CryptoApproveAllowance = scheduled.CryptoApproveAllowance;
					transaction = new AccountAllowanceApproveTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoDeleteAllowance:
					proto.CryptoDeleteAllowance = scheduled.CryptoDeleteAllowance;
					transaction = new AccountAllowanceDeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoCreateAccount:
					proto.CryptoCreateAccount = scheduled.CryptoCreateAccount;
					transaction = new AccountCreateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoDelete:
					proto.CryptoDelete = scheduled.CryptoDelete;
					transaction = new AccountDeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoTransfer:
					proto.CryptoTransfer = scheduled.CryptoTransfer;
					transaction = new TransferTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoUpdateAccount:
					proto.CryptoUpdateAccount = scheduled.CryptoUpdateAccount;
					transaction = new AccountUpdateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileAppend:
					proto.FileAppend = scheduled.FileAppend;
					transaction = new FileAppendTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileCreate:
					proto.FileCreate = scheduled.FileCreate;
					transaction = new FileCreateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileDelete:
					proto.FileDelete = scheduled.FileDelete;
					transaction = new FileDeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileUpdate:
					proto.FileUpdate = scheduled.FileUpdate;
					transaction = new FileUpdateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeCreate:
					proto.NodeCreate = scheduled.NodeCreate;
					transaction = new NodeCreateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeUpdate:
					proto.NodeUpdate = scheduled.NodeUpdate;
					transaction = new NodeUpdateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeDelete:
					proto.NodeDelete = scheduled.NodeDelete;
					transaction = new NodeDeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.SystemDelete:
					proto.SystemDelete = scheduled.SystemDelete;
					transaction = new SystemDeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.SystemUndelete:
					proto.SystemUndelete = scheduled.SystemUndelete;
					transaction = new SystemUndeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.Freeze:
					proto.Freeze = scheduled.Freeze;
					transaction = new FreezeTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusCreateTopic:
					proto.ConsensusCreateTopic = scheduled.ConsensusCreateTopic;
					transaction = new TopicCreateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusUpdateTopic:
					proto.ConsensusUpdateTopic = scheduled.ConsensusUpdateTopic;
					transaction = new TopicUpdateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusDeleteTopic:
					proto.ConsensusDeleteTopic = scheduled.ConsensusDeleteTopic;
					transaction = new TopicDeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusSubmitMessage:
					proto.ConsensusSubmitMessage = scheduled.ConsensusSubmitMessage;
					transaction = new TopicMessageSubmitTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenCreation:
					proto.TokenCreation = scheduled.TokenCreation;
					transaction = new TokenCreateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenFreeze:
					proto.TokenFreeze = scheduled.TokenFreeze;
					transaction = new TokenFreezeTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUnfreeze:
					proto.TokenUnfreeze = scheduled.TokenUnfreeze;
					transaction = new TokenUnfreezeTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenGrantKyc:
					proto.TokenGrantKyc = scheduled.TokenGrantKyc;
					transaction = new TokenGrantKycTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenRevokeKyc:
					proto.TokenRevokeKyc = scheduled.TokenRevokeKyc;
					transaction = new TokenRevokeKycTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenDeletion:
					proto.TokenDeletion = scheduled.TokenDeletion;
					transaction = new TokenDeleteTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUpdate:
					proto.TokenUpdate = scheduled.TokenUpdate;
					transaction = new TokenUpdateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUpdateNfts:
					proto.TokenUpdateNfts = scheduled.TokenUpdateNfts;
					transaction = new TokenUpdateNftsTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenMint:
					proto.TokenMint = scheduled.TokenMint;
					transaction = new TokenMintTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenBurn:
					proto.TokenBurn = scheduled.TokenBurn;
					transaction = new TokenBurnTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenWipe:
					proto.TokenWipe = scheduled.TokenWipe;
					transaction = new TokenWipeTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenAssociate:
					proto.TokenAssociate = scheduled.TokenAssociate;
					transaction = new TokenAssociateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenDissociate:
					proto.TokenDissociate = scheduled.TokenDissociate;
					transaction = new TokenDissociateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenFeeScheduleUpdate:
					proto.TokenFeeScheduleUpdate = scheduled.TokenFeeScheduleUpdate;
					transaction = new TokenFeeScheduleUpdateTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenPause:
					proto.TokenPause = scheduled.TokenPause;
					transaction = new TokenPauseTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUnpause:
					proto.TokenUnpause = scheduled.TokenUnpause;
					transaction = new TokenUnpauseTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenReject:
					proto.TokenReject = scheduled.TokenReject;
					transaction = new TokenRejectTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenAirdrop:
					proto.TokenAirdrop = scheduled.TokenAirdrop;
					transaction = new TokenAirdropTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenCancelAirdrop:
					proto.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					transaction = new TokenCancelAirdropTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenClaimAirdrop:
					proto.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					transaction = new TokenClaimAirdropTransaction(proto) as T;
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ScheduleDelete:
					proto.ScheduleDelete = scheduled.ScheduleDelete;
					transaction = new ScheduleDeleteTransaction(proto) as T;
					break;

				default: break;
			}

			return transaction ?? throw new InvalidOperationException("schedulable transaction did not have a transaction set");
		}
		public static object FromScheduledTransaction(Proto.SchedulableTransactionBody scheduled) 
		{
			object? transaction = null;

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
					transaction = new ContractExecuteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractCreateInstance:
					proto.ContractCreateInstance = scheduled.ContractCreateInstance;
					transaction = new ContractCreateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractUpdateInstance:
					proto.ContractUpdateInstance = scheduled.ContractUpdateInstance;
					transaction = new ContractUpdateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ContractDeleteInstance:
					proto.ContractDeleteInstance = scheduled.ContractDeleteInstance;
					transaction = new ContractDeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoApproveAllowance:
					proto.CryptoApproveAllowance = scheduled.CryptoApproveAllowance;
					transaction = new AccountAllowanceApproveTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoDeleteAllowance:
					proto.CryptoDeleteAllowance = scheduled.CryptoDeleteAllowance;
					transaction = new AccountAllowanceDeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoCreateAccount:
					proto.CryptoCreateAccount = scheduled.CryptoCreateAccount;
					transaction = new AccountCreateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoDelete:
					proto.CryptoDelete = scheduled.CryptoDelete;
					transaction = new AccountDeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoTransfer:
					proto.CryptoTransfer = scheduled.CryptoTransfer;
					transaction = new TransferTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.CryptoUpdateAccount:
					proto.CryptoUpdateAccount = scheduled.CryptoUpdateAccount;
					transaction = new AccountUpdateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileAppend:
					proto.FileAppend = scheduled.FileAppend;
					transaction = new FileAppendTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileCreate:
					proto.FileCreate = scheduled.FileCreate;
					transaction = new FileCreateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileDelete:
					proto.FileDelete = scheduled.FileDelete;
					transaction = new FileDeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.FileUpdate:
					proto.FileUpdate = scheduled.FileUpdate;
					transaction = new FileUpdateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeCreate:
					proto.NodeCreate = scheduled.NodeCreate;
					transaction = new NodeCreateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeUpdate:
					proto.NodeUpdate = scheduled.NodeUpdate;
					transaction = new NodeUpdateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.NodeDelete:
					proto.NodeDelete = scheduled.NodeDelete;
					transaction = new NodeDeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.SystemDelete:
					proto.SystemDelete = scheduled.SystemDelete;
					transaction = new SystemDeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.SystemUndelete:
					proto.SystemUndelete = scheduled.SystemUndelete;
					transaction = new SystemUndeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.Freeze:
					proto.Freeze = scheduled.Freeze;
					transaction = new FreezeTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusCreateTopic:
					proto.ConsensusCreateTopic = scheduled.ConsensusCreateTopic;
					transaction = new TopicCreateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusUpdateTopic:
					proto.ConsensusUpdateTopic = scheduled.ConsensusUpdateTopic;
					transaction = new TopicUpdateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusDeleteTopic:
					proto.ConsensusDeleteTopic = scheduled.ConsensusDeleteTopic;
					transaction = new TopicDeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ConsensusSubmitMessage:
					proto.ConsensusSubmitMessage = scheduled.ConsensusSubmitMessage;
					transaction = new TopicMessageSubmitTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenCreation:
					proto.TokenCreation = scheduled.TokenCreation;
					transaction = new TokenCreateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenFreeze:
					proto.TokenFreeze = scheduled.TokenFreeze;
					transaction = new TokenFreezeTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUnfreeze:
					proto.TokenUnfreeze = scheduled.TokenUnfreeze;
					transaction = new TokenUnfreezeTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenGrantKyc:
					proto.TokenGrantKyc = scheduled.TokenGrantKyc;
					transaction = new TokenGrantKycTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenRevokeKyc:
					proto.TokenRevokeKyc = scheduled.TokenRevokeKyc;
					transaction = new TokenRevokeKycTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenDeletion:
					proto.TokenDeletion = scheduled.TokenDeletion;
					transaction = new TokenDeleteTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUpdate:
					proto.TokenUpdate = scheduled.TokenUpdate;
					transaction = new TokenUpdateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUpdateNfts:
					proto.TokenUpdateNfts = scheduled.TokenUpdateNfts;
					transaction = new TokenUpdateNftsTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenMint:
					proto.TokenMint = scheduled.TokenMint;
					transaction = new TokenMintTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenBurn:
					proto.TokenBurn = scheduled.TokenBurn;
					transaction = new TokenBurnTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenWipe:
					proto.TokenWipe = scheduled.TokenWipe;
					transaction = new TokenWipeTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenAssociate:
					proto.TokenAssociate = scheduled.TokenAssociate;
					transaction = new TokenAssociateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenDissociate:
					proto.TokenDissociate = scheduled.TokenDissociate;
					transaction = new TokenDissociateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenFeeScheduleUpdate:
					proto.TokenFeeScheduleUpdate = scheduled.TokenFeeScheduleUpdate;
					transaction = new TokenFeeScheduleUpdateTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenPause:
					proto.TokenPause = scheduled.TokenPause;
					transaction = new TokenPauseTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenUnpause:
					proto.TokenUnpause = scheduled.TokenUnpause;
					transaction = new TokenUnpauseTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenReject:
					proto.TokenReject = scheduled.TokenReject;
					transaction = new TokenRejectTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenAirdrop:
					proto.TokenAirdrop = scheduled.TokenAirdrop;
					transaction = new TokenAirdropTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenCancelAirdrop:
					proto.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					transaction = new TokenCancelAirdropTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.TokenClaimAirdrop:
					proto.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					transaction = new TokenClaimAirdropTransaction(proto);
					break;
				case Proto.SchedulableTransactionBody.DataOneofCase.ScheduleDelete:
					proto.ScheduleDelete = scheduled.ScheduleDelete;
					transaction = new ScheduleDeleteTransaction(proto);
					break;

				default: break;
			}

			return transaction ?? throw new InvalidOperationException("schedulable transaction did not have a transaction set");
		}
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:ParseTransactionBody(ByteString)"]/*' />
		internal static Proto.TransactionBody ParseTransactionBody(ByteString signedTransactionBuilder)
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
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:ParseTransactionBody(Proto.SignedTransaction)"]/*' />
		internal static Proto.TransactionBody ParseTransactionBody(Proto.SignedTransaction signedTransactionBuilder)
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
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:PrepareSingleTransaction(Proto.Transaction)"]/*' />
		internal static Proto.Transaction PrepareSingleTransaction(Proto.Transaction transaction)
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

		internal static bool PublicKeyIsInSigPairList(ByteString publicKeyBytes, IEnumerable<Proto.SignaturePair> sigPairList)
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
		internal static void RequireProtoMatches(object? protoA, object? protoB, HashSet<string> ignoreSet, string thisFieldName)
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

				if ((!method.IsPublic) || method.IsStatic)
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
						var listA = retvalA as IList;
						var listB = retvalB as IList;

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
		internal static void ThrowProtoMatchException(string fieldName, string aWas, string bWas)
		{
			throw new ArgumentException("fromBytes() failed because " + fieldName + " fields in TransactionBody protobuf messages in the TransactionList did not match: A was " + aWas + ", B was " + bWas);
		}
	}
}