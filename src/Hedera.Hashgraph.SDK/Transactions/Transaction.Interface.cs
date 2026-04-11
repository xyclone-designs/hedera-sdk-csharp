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
	public interface ITransaction
	{
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.OuterTransactions"]/*' />
		public List<Proto.Transaction> OuterTransactions { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.InnerSignedTransactions"]/*' />
		public List<Proto.SignedTransaction> InnerSignedTransactions { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.SigPairLists"]/*' />
		public List<Proto.SignatureMap> SigPairLists { get; }
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
		public Proto.TransactionBody SourceTransactionBody { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.FrozenBodyBuilder"]/*' />
		public Proto.TransactionBody? FrozenBodyBuilder { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.BatchKey"]/*' />
		public Key? BatchKey { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.ShouldRegenerateTransactionId"]/*' />
		public bool ShouldRegenerateTransactionId { get; }
		/// <include file="Transaction.Interface.cs.xml" path='docs/member[@name="P:.TransactionId"]/*' />
		public TransactionId TransactionId { get; }

		bool IsFrozen();
		string ToString();
		void ValidateChecksums(Client client);
		Proto.Transaction MakeRequest();

		public static ITransaction FromBytes(byte[] bytes)
		{
			var list = Proto.TransactionList.Parser.ParseFrom(bytes);
			var txsMap = new DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>>();

			Proto.TransactionBody.DataOneofCase dataCase;

			if (list.TransactionList_.Count != 0)
				dataCase = Transaction.ProcessTransactionList([.. list.TransactionList_], txsMap);
			else dataCase = Transaction.ProcessSingleTransaction(bytes, txsMap);

			return dataCase switch
			{
				Proto.TransactionBody.DataOneofCase.ContractCall => new ContractExecuteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ContractCreateInstance => new ContractCreateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ContractUpdateInstance => new ContractUpdateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ContractDeleteInstance => new ContractDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.EthereumTransaction => new EthereumTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.CryptoAddLiveHash => new LiveHashAddTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.CryptoCreateAccount => new AccountCreateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.CryptoDelete => new AccountDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.CryptoDeleteLiveHash => new LiveHashDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.CryptoTransfer => new TransferTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.CryptoUpdateAccount => new AccountUpdateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.FileAppend => new FileAppendTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.FileCreate => new FileCreateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.FileDelete => new FileDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.FileUpdate => new FileUpdateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.NodeCreate => new NodeCreateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.NodeUpdate => new NodeUpdateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.NodeDelete => new NodeDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.SystemDelete => new SystemDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.SystemUndelete => new SystemUndeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.Freeze => new FreezeTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ConsensusCreateTopic => new TopicCreateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ConsensusUpdateTopic => new TopicUpdateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ConsensusDeleteTopic => new TopicDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ConsensusSubmitMessage => new TopicMessageSubmitTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenAssociate => new TokenAssociateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenBurn => new TokenBurnTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenCreation => new TokenCreateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenDeletion => new TokenDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenDissociate => new TokenDissociateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenFreeze => new TokenFreezeTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenGrantKyc => new TokenGrantKycTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenMint => new TokenMintTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenRevokeKyc => new TokenRevokeKycTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenUnfreeze => new TokenUnfreezeTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenUpdate => new TokenUpdateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenUpdateNfts => new TokenUpdateNftsTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenWipe => new TokenWipeTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenFeeScheduleUpdate => new TokenFeeScheduleUpdateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ScheduleCreate => new ScheduleCreateTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ScheduleDelete => new ScheduleDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.ScheduleSign => new ScheduleSignTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenPause => new TokenPauseTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenUnpause => new TokenUnpauseTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenReject => new TokenRejectTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenAirdrop => new TokenAirdropTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenCancelAirdrop => new TokenCancelAirdropTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.TokenClaimAirdrop => new TokenClaimAirdropTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.CryptoApproveAllowance => new AccountAllowanceApproveTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.CryptoDeleteAllowance => new AccountAllowanceDeleteTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.AtomicBatch => new BatchTransaction(txsMap),
				Proto.TransactionBody.DataOneofCase.HookStore => new HookStoreTransaction(txsMap),

				_ => throw new ArgumentException("parsed transaction body has no data")
			};
		}
	}
}