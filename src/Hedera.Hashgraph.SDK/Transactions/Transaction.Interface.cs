// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Cryptography;
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
	public interface ITransaction
	{
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.OuterTransactions"]/*' />
		public List<Proto.Services.Transaction> OuterTransactions { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.InnerSignedTransactions"]/*' />
		public List<Proto.Services.SignedTransaction> InnerSignedTransactions { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.SigPairLists"]/*' />
		public List<Proto.Services.SignatureMap> SigPairLists { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.TransactionIds"]/*' />
		public ListGuarded<TransactionId> TransactionIds { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.PublicKeys"]/*' />
		public IList<PublicKey> PublicKeys { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="M:operator(paying)"]/*' />
		public List<Func<byte[], byte[]>?> Signers { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.MaxTransactionFee"]/*' />
		public Hbar? MaxTransactionFee { get; set; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.TransactionMemo"]/*' />
		public string TransactionMemo { get; set; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.TransactionValidDuration"]/*' />
		public TimeSpan TransactionValidDuration { get; set; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.SourceTransactionBody"]/*' />
		public Proto.Services.TransactionBody SourceTransactionBody { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.FrozenBodyBuilder"]/*' />
		public Proto.Services.TransactionBody? FrozenBodyBuilder { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.BatchKey"]/*' />
		public Key? BatchKey { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.ShouldRegenerateTransactionId"]/*' />
		public bool ShouldRegenerateTransactionId { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.TransactionId"]/*' />
		public TransactionId TransactionId { get; }

		bool IsFrozen();
		string ToString();
		void ValidateChecksums(Client client);
		Proto.Services.Transaction MakeRequest();

		public static ITransaction FromBytes(byte[] bytes)
		{
			var list = Proto.SDK.TransactionList.Parser.ParseFrom(bytes);
			var txsMap = new DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>>();

			Proto.Services.TransactionBody.DataOneofCase dataCase;

			if (list.TransactionList_.Count != 0)
				dataCase = Transaction.ProcessTransactionList([.. list.TransactionList_], txsMap);
			else dataCase = Transaction.ProcessSingleTransaction(bytes, txsMap);

			return dataCase switch
			{
				Proto.Services.TransactionBody.DataOneofCase.ContractCall => new ContractExecuteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ContractCreateInstance => new ContractCreateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ContractUpdateInstance => new ContractUpdateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ContractDeleteInstance => new ContractDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.EthereumTransaction => new EthereumTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.CryptoAddLiveHash => new LiveHashAddTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.CryptoCreateAccount => new AccountCreateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.CryptoDelete => new AccountDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.CryptoDeleteLiveHash => new LiveHashDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.CryptoTransfer => new TransferTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.CryptoUpdateAccount => new AccountUpdateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.FileAppend => new FileAppendTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.FileCreate => new FileCreateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.FileDelete => new FileDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.FileUpdate => new FileUpdateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.NodeCreate => new NodeCreateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.NodeUpdate => new NodeUpdateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.NodeDelete => new NodeDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.SystemDelete => new SystemDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.SystemUndelete => new SystemUndeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.Freeze => new FreezeTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ConsensusCreateTopic => new TopicCreateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ConsensusUpdateTopic => new TopicUpdateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ConsensusDeleteTopic => new TopicDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ConsensusSubmitMessage => new TopicMessageSubmitTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenAssociate => new TokenAssociateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenBurn => new TokenBurnTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenCreation => new TokenCreateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenDeletion => new TokenDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenDissociate => new TokenDissociateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenFreeze => new TokenFreezeTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenGrantKyc => new TokenGrantKycTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenMint => new TokenMintTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenRevokeKyc => new TokenRevokeKycTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenUnfreeze => new TokenUnfreezeTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenUpdate => new TokenUpdateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenUpdateNfts => new TokenUpdateNftsTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenWipe => new TokenWipeTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenFeeScheduleUpdate => new TokenFeeScheduleUpdateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ScheduleCreate => new ScheduleCreateTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ScheduleDelete => new ScheduleDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.ScheduleSign => new ScheduleSignTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenPause => new TokenPauseTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenUnpause => new TokenUnpauseTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenReject => new TokenRejectTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenAirdrop => new TokenAirdropTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenCancelAirdrop => new TokenCancelAirdropTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.TokenClaimAirdrop => new TokenClaimAirdropTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.CryptoApproveAllowance => new AccountAllowanceApproveTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.CryptoDeleteAllowance => new AccountAllowanceDeleteTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.AtomicBatch => new BatchTransaction(txsMap),
				Proto.Services.TransactionBody.DataOneofCase.HookStore => new HookStoreTransaction(txsMap),

				_ => throw new ArgumentException("parsed transaction body has no data")
			};
		}
	}
}
