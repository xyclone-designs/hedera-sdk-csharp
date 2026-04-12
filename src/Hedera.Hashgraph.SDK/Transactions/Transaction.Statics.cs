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
using Hedera.Hashgraph.SDK.Consensus;

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
	public static partial class Transaction
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
			var list = Proto.Services.TransactionList.Parser.ParseFrom(bytes);
			var txsMap = new DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>>();

			Proto.Services.TransactionBody.DataOneofCase dataCase;

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

		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:AddTransactionToMap(Proto.Services.Transaction,Proto.Services.TransactionBody,DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal static void AddTransactionToMap(Proto.Services.Transaction transaction, Proto.Services.TransactionBody txBody, DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txsMap)
		{
			var account = txBody.NodeAccountID is not null ? AccountId.FromProtobuf(txBody.NodeAccountID) : DUMMY_ACCOUNT_ID;
			var transactionId = txBody.TransactionID is not null ? TransactionId.FromProtobuf(txBody.TransactionID) : DUMMY_TRANSACTION_ID;
			var linked = txsMap.ContainsKey(transactionId) ? txsMap[transactionId] : new DictionaryLinked<AccountId, Proto.Services.Transaction>();

			linked.Add(account, transaction);
			txsMap.Add(transactionId, linked);
		}

		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:CreateTransactionFromDataCase``1(Proto.Services.TransactionBody.DataOneofCase,DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal static T CreateTransactionFromDataCase<T>(Proto.Services.TransactionBody.DataOneofCase dataCase, DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) where T : Transaction<T>
		{
			return dataCase switch
			{
				Proto.Services.TransactionBody.DataOneofCase.ContractCall => new ContractExecuteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ContractCreateInstance => new ContractCreateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ContractUpdateInstance => new ContractUpdateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ContractDeleteInstance => new ContractDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.EthereumTransaction => new EthereumTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.CryptoAddLiveHash => new LiveHashAddTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.CryptoCreateAccount => new AccountCreateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.CryptoDelete => new AccountDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.CryptoDeleteLiveHash => new LiveHashDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.CryptoTransfer => new TransferTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.CryptoUpdateAccount => new AccountUpdateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.FileAppend => new FileAppendTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.FileCreate => new FileCreateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.FileDelete => new FileDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.FileUpdate => new FileUpdateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.NodeCreate => new NodeCreateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.NodeUpdate => new NodeUpdateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.NodeDelete => new NodeDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.SystemDelete => new SystemDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.SystemUndelete => new SystemUndeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.Freeze => new FreezeTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ConsensusCreateTopic => new TopicCreateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ConsensusUpdateTopic => new TopicUpdateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ConsensusDeleteTopic => new TopicDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ConsensusSubmitMessage => new TopicMessageSubmitTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenAssociate => new TokenAssociateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenBurn => new TokenBurnTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenCreation => new TokenCreateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenDeletion => new TokenDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenDissociate => new TokenDissociateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenFreeze => new TokenFreezeTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenGrantKyc => new TokenGrantKycTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenMint => new TokenMintTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenRevokeKyc => new TokenRevokeKycTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenUnfreeze => new TokenUnfreezeTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenUpdate => new TokenUpdateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenUpdateNfts => new TokenUpdateNftsTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenWipe => new TokenWipeTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenFeeScheduleUpdate => new TokenFeeScheduleUpdateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ScheduleCreate => new ScheduleCreateTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ScheduleDelete => new ScheduleDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.ScheduleSign => new ScheduleSignTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenPause => new TokenPauseTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenUnpause => new TokenUnpauseTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenReject => new TokenRejectTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenAirdrop => new TokenAirdropTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenCancelAirdrop => new TokenCancelAirdropTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.TokenClaimAirdrop => new TokenClaimAirdropTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.CryptoApproveAllowance => new AccountAllowanceApproveTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.CryptoDeleteAllowance => new AccountAllowanceDeleteTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.AtomicBatch => new BatchTransaction(txs) as T,
				Proto.Services.TransactionBody.DataOneofCase.HookStore => new HookStoreTransaction(txs) as T,

				_ => throw new ArgumentException("parsed transaction body has no data")

			} ?? throw new ArgumentException("transaction body has no counterpart");
		}
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:ProcessSingleTransaction(System.Byte[],DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal static Proto.Services.TransactionBody.DataOneofCase ProcessSingleTransaction(byte[] bytes, DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txsMap)
		{
			var transaction = Proto.Services.Transaction.Parser.ParseFrom(bytes);
			var builtTransaction = PrepareSingleTransaction(transaction);
			var signedTransaction = Proto.Services.SignedTransaction.Parser.ParseFrom(builtTransaction.SignedTransactionBytes);
			var txBody = Proto.Services.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);

			AddTransactionToMap(builtTransaction, txBody, txsMap);

			return txBody.DataCase;
		}
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:ProcessTransactionList(System.Collections.Generic.List{Proto.Services.Transaction},DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal static Proto.Services.TransactionBody.DataOneofCase ProcessTransactionList(List<Proto.Services.Transaction> transactionList, DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txsMap)
		{
			if (transactionList.Count == 0)
			{
				return Proto.Services.TransactionBody.DataOneofCase.None;
			}

			var firstTransaction = transactionList[0];
			var firstSignedTransaction = Proto.Services.SignedTransaction.Parser.ParseFrom(firstTransaction.SignedTransactionBytes);
			var firstTxBody = Proto.Services.TransactionBody.Parser.ParseFrom(firstSignedTransaction.BodyBytes);
			var dataCase = firstTxBody.DataCase;

			foreach (Proto.Services.Transaction transaction in transactionList)
			{
				var signedTransaction = Proto.Services.SignedTransaction.Parser.ParseFrom(transaction.SignedTransactionBytes);
				var txBody = Proto.Services.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);

				AddTransactionToMap(transaction, txBody, txsMap);
			}

			return dataCase;
		}

		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:FromScheduledTransaction``1(Proto.Services.SchedulableTransactionBody)"]/*' />
		public static T FromScheduledTransaction<T>(Proto.Services.SchedulableTransactionBody scheduled) where T : Transaction<T>
		{
			T? transaction = null;

			Proto.Services.TransactionBody proto = new()
			{
				Memo = scheduled.Memo,
				TransactionFee = scheduled.TransactionFee,
			};

			Proto.Services.MaxCustomFees.AddRange(scheduled.MaxCustomFees);
			
			switch (scheduled.DataCase)
			{
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ContractCall:
					Proto.Services.ContractCall = scheduled.ContractCall;
					transaction = new ContractExecuteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ContractCreateInstance:
					Proto.Services.ContractCreateInstance = scheduled.ContractCreateInstance;
					transaction = new ContractCreateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ContractUpdateInstance:
					Proto.Services.ContractUpdateInstance = scheduled.ContractUpdateInstance;
					transaction = new ContractUpdateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ContractDeleteInstance:
					Proto.Services.ContractDeleteInstance = scheduled.ContractDeleteInstance;
					transaction = new ContractDeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoApproveAllowance:
					Proto.Services.CryptoApproveAllowance = scheduled.CryptoApproveAllowance;
					transaction = new AccountAllowanceApproveTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoDeleteAllowance:
					Proto.Services.CryptoDeleteAllowance = scheduled.CryptoDeleteAllowance;
					transaction = new AccountAllowanceDeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoCreateAccount:
					Proto.Services.CryptoCreateAccount = scheduled.CryptoCreateAccount;
					transaction = new AccountCreateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoDelete:
					Proto.Services.CryptoDelete = scheduled.CryptoDelete;
					transaction = new AccountDeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoTransfer:
					Proto.Services.CryptoTransfer = scheduled.CryptoTransfer;
					transaction = new TransferTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoUpdateAccount:
					Proto.Services.CryptoUpdateAccount = scheduled.CryptoUpdateAccount;
					transaction = new AccountUpdateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.FileAppend:
					Proto.Services.FileAppend = scheduled.FileAppend;
					transaction = new FileAppendTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.FileCreate:
					Proto.Services.FileCreate = scheduled.FileCreate;
					transaction = new FileCreateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.FileDelete:
					Proto.Services.FileDelete = scheduled.FileDelete;
					transaction = new FileDeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.FileUpdate:
					Proto.Services.FileUpdate = scheduled.FileUpdate;
					transaction = new FileUpdateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.NodeCreate:
					Proto.Services.NodeCreate = scheduled.NodeCreate;
					transaction = new NodeCreateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.NodeUpdate:
					Proto.Services.NodeUpdate = scheduled.NodeUpdate;
					transaction = new NodeUpdateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.NodeDelete:
					Proto.Services.NodeDelete = scheduled.NodeDelete;
					transaction = new NodeDeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.SystemDelete:
					Proto.Services.SystemDelete = scheduled.SystemDelete;
					transaction = new SystemDeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.SystemUndelete:
					Proto.Services.SystemUndelete = scheduled.SystemUndelete;
					transaction = new SystemUndeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.Freeze:
					Proto.Services.Freeze = scheduled.Freeze;
					transaction = new FreezeTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ConsensusCreateTopic:
					Proto.Services.ConsensusCreateTopic = scheduled.ConsensusCreateTopic;
					transaction = new TopicCreateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ConsensusUpdateTopic:
					Proto.Services.ConsensusUpdateTopic = scheduled.ConsensusUpdateTopic;
					transaction = new TopicUpdateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ConsensusDeleteTopic:
					Proto.Services.ConsensusDeleteTopic = scheduled.ConsensusDeleteTopic;
					transaction = new TopicDeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ConsensusSubmitMessage:
					Proto.Services.ConsensusSubmitMessage = scheduled.ConsensusSubmitMessage;
					transaction = new TopicMessageSubmitTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenCreation:
					Proto.Services.TokenCreation = scheduled.TokenCreation;
					transaction = new TokenCreateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenFreeze:
					Proto.Services.TokenFreeze = scheduled.TokenFreeze;
					transaction = new TokenFreezeTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenUnfreeze:
					Proto.Services.TokenUnfreeze = scheduled.TokenUnfreeze;
					transaction = new TokenUnfreezeTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenGrantKyc:
					Proto.Services.TokenGrantKyc = scheduled.TokenGrantKyc;
					transaction = new TokenGrantKycTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenRevokeKyc:
					Proto.Services.TokenRevokeKyc = scheduled.TokenRevokeKyc;
					transaction = new TokenRevokeKycTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenDeletion:
					Proto.Services.TokenDeletion = scheduled.TokenDeletion;
					transaction = new TokenDeleteTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenUpdate:
					Proto.Services.TokenUpdate = scheduled.TokenUpdate;
					transaction = new TokenUpdateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenUpdateNfts:
					Proto.Services.TokenUpdateNfts = scheduled.TokenUpdateNfts;
					transaction = new TokenUpdateNftsTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenMint:
					Proto.Services.TokenMint = scheduled.TokenMint;
					transaction = new TokenMintTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenBurn:
					Proto.Services.TokenBurn = scheduled.TokenBurn;
					transaction = new TokenBurnTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenWipe:
					Proto.Services.TokenWipe = scheduled.TokenWipe;
					transaction = new TokenWipeTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenAssociate:
					Proto.Services.TokenAssociate = scheduled.TokenAssociate;
					transaction = new TokenAssociateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenDissociate:
					Proto.Services.TokenDissociate = scheduled.TokenDissociate;
					transaction = new TokenDissociateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenFeeScheduleUpdate:
					Proto.Services.TokenFeeScheduleUpdate = scheduled.TokenFeeScheduleUpdate;
					transaction = new TokenFeeScheduleUpdateTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenPause:
					Proto.Services.TokenPause = scheduled.TokenPause;
					transaction = new TokenPauseTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenUnpause:
					Proto.Services.TokenUnpause = scheduled.TokenUnpause;
					transaction = new TokenUnpauseTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenReject:
					Proto.Services.TokenReject = scheduled.TokenReject;
					transaction = new TokenRejectTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenAirdrop:
					Proto.Services.TokenAirdrop = scheduled.TokenAirdrop;
					transaction = new TokenAirdropTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenCancelAirdrop:
					Proto.Services.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					transaction = new TokenCancelAirdropTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenClaimAirdrop:
					Proto.Services.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					transaction = new TokenClaimAirdropTransaction(proto) as T;
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ScheduleDelete:
					Proto.Services.ScheduleDelete = scheduled.ScheduleDelete;
					transaction = new ScheduleDeleteTransaction(proto) as T;
					break;

				default: break;
			}

			return transaction ?? throw new InvalidOperationException("schedulable transaction did not have a transaction set");
		}
		public static object FromScheduledTransaction(Proto.Services.SchedulableTransactionBody scheduled) 
		{
			object? transaction = null;

			Proto.Services.TransactionBody proto = new()
			{
				Memo = scheduled.Memo,
				TransactionFee = scheduled.TransactionFee,
			};

			Proto.Services.MaxCustomFees.AddRange(scheduled.MaxCustomFees);

			switch (scheduled.DataCase)
			{
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ContractCall:
					Proto.Services.ContractCall = scheduled.ContractCall;
					transaction = new ContractExecuteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ContractCreateInstance:
					Proto.Services.ContractCreateInstance = scheduled.ContractCreateInstance;
					transaction = new ContractCreateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ContractUpdateInstance:
					Proto.Services.ContractUpdateInstance = scheduled.ContractUpdateInstance;
					transaction = new ContractUpdateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ContractDeleteInstance:
					Proto.Services.ContractDeleteInstance = scheduled.ContractDeleteInstance;
					transaction = new ContractDeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoApproveAllowance:
					Proto.Services.CryptoApproveAllowance = scheduled.CryptoApproveAllowance;
					transaction = new AccountAllowanceApproveTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoDeleteAllowance:
					Proto.Services.CryptoDeleteAllowance = scheduled.CryptoDeleteAllowance;
					transaction = new AccountAllowanceDeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoCreateAccount:
					Proto.Services.CryptoCreateAccount = scheduled.CryptoCreateAccount;
					transaction = new AccountCreateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoDelete:
					Proto.Services.CryptoDelete = scheduled.CryptoDelete;
					transaction = new AccountDeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoTransfer:
					Proto.Services.CryptoTransfer = scheduled.CryptoTransfer;
					transaction = new TransferTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.CryptoUpdateAccount:
					Proto.Services.CryptoUpdateAccount = scheduled.CryptoUpdateAccount;
					transaction = new AccountUpdateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.FileAppend:
					Proto.Services.FileAppend = scheduled.FileAppend;
					transaction = new FileAppendTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.FileCreate:
					Proto.Services.FileCreate = scheduled.FileCreate;
					transaction = new FileCreateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.FileDelete:
					Proto.Services.FileDelete = scheduled.FileDelete;
					transaction = new FileDeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.FileUpdate:
					Proto.Services.FileUpdate = scheduled.FileUpdate;
					transaction = new FileUpdateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.NodeCreate:
					Proto.Services.NodeCreate = scheduled.NodeCreate;
					transaction = new NodeCreateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.NodeUpdate:
					Proto.Services.NodeUpdate = scheduled.NodeUpdate;
					transaction = new NodeUpdateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.NodeDelete:
					Proto.Services.NodeDelete = scheduled.NodeDelete;
					transaction = new NodeDeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.SystemDelete:
					Proto.Services.SystemDelete = scheduled.SystemDelete;
					transaction = new SystemDeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.SystemUndelete:
					Proto.Services.SystemUndelete = scheduled.SystemUndelete;
					transaction = new SystemUndeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.Freeze:
					Proto.Services.Freeze = scheduled.Freeze;
					transaction = new FreezeTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ConsensusCreateTopic:
					Proto.Services.ConsensusCreateTopic = scheduled.ConsensusCreateTopic;
					transaction = new TopicCreateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ConsensusUpdateTopic:
					Proto.Services.ConsensusUpdateTopic = scheduled.ConsensusUpdateTopic;
					transaction = new TopicUpdateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ConsensusDeleteTopic:
					Proto.Services.ConsensusDeleteTopic = scheduled.ConsensusDeleteTopic;
					transaction = new TopicDeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ConsensusSubmitMessage:
					Proto.Services.ConsensusSubmitMessage = scheduled.ConsensusSubmitMessage;
					transaction = new TopicMessageSubmitTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenCreation:
					Proto.Services.TokenCreation = scheduled.TokenCreation;
					transaction = new TokenCreateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenFreeze:
					Proto.Services.TokenFreeze = scheduled.TokenFreeze;
					transaction = new TokenFreezeTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenUnfreeze:
					Proto.Services.TokenUnfreeze = scheduled.TokenUnfreeze;
					transaction = new TokenUnfreezeTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenGrantKyc:
					Proto.Services.TokenGrantKyc = scheduled.TokenGrantKyc;
					transaction = new TokenGrantKycTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenRevokeKyc:
					Proto.Services.TokenRevokeKyc = scheduled.TokenRevokeKyc;
					transaction = new TokenRevokeKycTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenDeletion:
					Proto.Services.TokenDeletion = scheduled.TokenDeletion;
					transaction = new TokenDeleteTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenUpdate:
					Proto.Services.TokenUpdate = scheduled.TokenUpdate;
					transaction = new TokenUpdateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenUpdateNfts:
					Proto.Services.TokenUpdateNfts = scheduled.TokenUpdateNfts;
					transaction = new TokenUpdateNftsTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenMint:
					Proto.Services.TokenMint = scheduled.TokenMint;
					transaction = new TokenMintTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenBurn:
					Proto.Services.TokenBurn = scheduled.TokenBurn;
					transaction = new TokenBurnTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenWipe:
					Proto.Services.TokenWipe = scheduled.TokenWipe;
					transaction = new TokenWipeTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenAssociate:
					Proto.Services.TokenAssociate = scheduled.TokenAssociate;
					transaction = new TokenAssociateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenDissociate:
					Proto.Services.TokenDissociate = scheduled.TokenDissociate;
					transaction = new TokenDissociateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenFeeScheduleUpdate:
					Proto.Services.TokenFeeScheduleUpdate = scheduled.TokenFeeScheduleUpdate;
					transaction = new TokenFeeScheduleUpdateTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenPause:
					Proto.Services.TokenPause = scheduled.TokenPause;
					transaction = new TokenPauseTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenUnpause:
					Proto.Services.TokenUnpause = scheduled.TokenUnpause;
					transaction = new TokenUnpauseTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenReject:
					Proto.Services.TokenReject = scheduled.TokenReject;
					transaction = new TokenRejectTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenAirdrop:
					Proto.Services.TokenAirdrop = scheduled.TokenAirdrop;
					transaction = new TokenAirdropTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenCancelAirdrop:
					Proto.Services.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					transaction = new TokenCancelAirdropTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.TokenClaimAirdrop:
					Proto.Services.TokenCancelAirdrop = scheduled.TokenCancelAirdrop;
					transaction = new TokenClaimAirdropTransaction(proto);
					break;
				case Proto.Services.SchedulableTransactionBody.DataOneofCase.ScheduleDelete:
					Proto.Services.ScheduleDelete = scheduled.ScheduleDelete;
					transaction = new ScheduleDeleteTransaction(proto);
					break;

				default: break;
			}

			return transaction ?? throw new InvalidOperationException("schedulable transaction did not have a transaction set");
		}
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:ParseTransactionBody(ByteString)"]/*' />
		internal static Proto.Services.TransactionBody ParseTransactionBody(ByteString signedTransactionBuilder)
		{
			try
			{
				return Proto.Services.TransactionBody.Parser.ParseFrom(signedTransactionBuilder);
			}
			catch (InvalidProtocolBufferException e)
			{
				throw new Exception("Failed to parse transaction body", e);
			}
		}
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:ParseTransactionBody(Proto.Services.SignedTransaction)"]/*' />
		internal static Proto.Services.TransactionBody ParseTransactionBody(Proto.Services.SignedTransaction signedTransactionBuilder)
		{
			try
			{
				return Proto.Services.TransactionBody.Parser.ParseFrom(signedTransactionBuilder.BodyBytes);
			}
			catch (InvalidProtocolBufferException e)
			{
				throw new Exception("Failed to parse transaction body", e);
			}
		}
		/// <include file="Transaction.Statics.cs.xml" path='docs/member[@name="M:PrepareSingleTransaction(Proto.Services.Transaction)"]/*' />
		internal static Proto.Services.Transaction PrepareSingleTransaction(Proto.Services.Transaction transaction)
		{
			if (transaction.SignedTransactionBytes.Length == 0)
			{
				transaction.SignedTransactionBytes = new Proto.Services.SignedTransaction
				{
					BodyBytes = transaction.BodyBytes,
					SigMap = transaction.SigMap,

				}.ToByteString();

				return transaction;
			}

			return transaction;
		}

		internal static bool PublicKeyIsInSigPairList(ByteString publicKeyBytes, IEnumerable<Proto.Services.SignaturePair> sigPairList)
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
