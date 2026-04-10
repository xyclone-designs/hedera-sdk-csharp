// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType"]/*' />
    public enum RequestType
    {
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_2"]/*' />
		None = Proto.HederaFunctionality.None,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_3"]/*' />
		CryptoTransfer = Proto.HederaFunctionality.CryptoTransfer,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_4"]/*' />
		CryptoUpdate = Proto.HederaFunctionality.CryptoUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_5"]/*' />
		CryptoDelete = Proto.HederaFunctionality.CryptoDelete,
        /// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_6"]/*' />
        [Obsolete]
        CryptoAddLiveHash = Proto.HederaFunctionality.CryptoAddLiveHash,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_7"]/*' />
		[Obsolete]
		CryptoDeleteLiveHash = Proto.HederaFunctionality.CryptoDeleteLiveHash,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_8"]/*' />
		ContractCall = Proto.HederaFunctionality.ContractCall,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_9"]/*' />
		ContractCreate = Proto.HederaFunctionality.ContractCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_10"]/*' />
		ContractUpdate = Proto.HederaFunctionality.ContractUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_11"]/*' />
		FileCreate = Proto.HederaFunctionality.FileCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_12"]/*' />
		FileAppend = Proto.HederaFunctionality.FileAppend,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_13"]/*' />
		FileUpdate = Proto.HederaFunctionality.FileUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_14"]/*' />
		FileDelete = Proto.HederaFunctionality.FileDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_15"]/*' />
		CryptoGetAccountBalance = Proto.HederaFunctionality.CryptoGetAccountBalance,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_16"]/*' />
		CryptoGetAccountRecords = Proto.HederaFunctionality.CryptoGetAccountRecords,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_17"]/*' />
		CryptoGetInfo = Proto.HederaFunctionality.CryptoGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_18"]/*' />
		// /**
		ContractCallLocal = Proto.HederaFunctionality.ContractCallLocal,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_19"]/*' />
		ContractGetInfo = Proto.HederaFunctionality.ContractGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_20"]/*' />
		ContractGetBytecode = Proto.HederaFunctionality.ContractGetBytecode,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_21"]/*' />
		GetBySolidityId = Proto.HederaFunctionality.GetBySolidityId,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_22"]/*' />
		GetByKey = Proto.HederaFunctionality.GetByKey,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_23"]/*' />
		[Obsolete]
		CryptoGetLiveHash = Proto.HederaFunctionality.CryptoGetLiveHash,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_24"]/*' />
		[Obsolete]
		CryptoGetStakers = Proto.HederaFunctionality.CryptoGetStakers,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_25"]/*' />
		FileGetContents = Proto.HederaFunctionality.FileGetContents,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_26"]/*' />
		FileGetInfo = Proto.HederaFunctionality.FileGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_27"]/*' />
		TransactionGetRecord = Proto.HederaFunctionality.TransactionGetRecord,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_28"]/*' />
		[Obsolete]
		ContractGetRecords = Proto.HederaFunctionality.ContractGetRecords,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_29"]/*' />
		CryptoCreate = Proto.HederaFunctionality.CryptoCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_30"]/*' />
		SystemDelete = Proto.HederaFunctionality.SystemDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_31"]/*' />
		SystemUndelete = Proto.HederaFunctionality.SystemUndelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_32"]/*' />
		ContractDelete = Proto.HederaFunctionality.ContractDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_33"]/*' />
		Freeze = Proto.HederaFunctionality.Freeze,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.services(node)"]/*' />
		CreateTransactionRecord = Proto.HederaFunctionality.CreateTransactionRecord,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.services(node)_2"]/*' />
		CryptoAccountAutoRenew = Proto.HederaFunctionality.CryptoAccountAutoRenew,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.services(node)_3"]/*' />
		ContractAutoRenew = Proto.HederaFunctionality.ContractAutoRenew,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HCS)"]/*' />
		GetVersionInfo = Proto.HederaFunctionality.GetVersionInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HCS)_2"]/*' />
		TransactionGetReceipt = Proto.HederaFunctionality.TransactionGetReceipt,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)"]/*' />
		ConsensusCreateTopic = Proto.HederaFunctionality.ConsensusCreateTopic,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_2"]/*' />
		ConsensusUpdateTopic = Proto.HederaFunctionality.ConsensusUpdateTopic,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_3"]/*' />
		ConsensusDeleteTopic = Proto.HederaFunctionality.ConsensusDeleteTopic,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HTS)"]/*' />
		ConsensusGetTopicInfo = Proto.HederaFunctionality.ConsensusGetTopicInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HTS)_2"]/*' />
		ConsensusSubmitMessage = Proto.HederaFunctionality.ConsensusSubmitMessage,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HTS)_3"]/*' />
		// /**
		UncheckedSubmit = Proto.HederaFunctionality.UncheckedSubmit,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_4"]/*' />
		TokenCreate = Proto.HederaFunctionality.TokenCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_34"]/*' />
		TokenGetInfo = Proto.HederaFunctionality.TokenGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_35"]/*' />
		TokenFreezeAccount = Proto.HederaFunctionality.TokenFreezeAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_36"]/*' />
		TokenUnfreezeAccount = Proto.HederaFunctionality.TokenUnfreezeAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_37"]/*' />
		TokenGrantKycToAccount = Proto.HederaFunctionality.TokenGrantKycToAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_38"]/*' />
		TokenRevokeKycFromAccount = Proto.HederaFunctionality.TokenRevokeKycFromAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_39"]/*' />
		TokenDelete = Proto.HederaFunctionality.TokenDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_40"]/*' />
		TokenUpdate = Proto.HederaFunctionality.TokenUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_41"]/*' />
		TokenMint = Proto.HederaFunctionality.TokenMint,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_42"]/*' />
		TokenBurn = Proto.HederaFunctionality.TokenBurn,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_43"]/*' />
		TokenAccountWipe = Proto.HederaFunctionality.TokenAccountWipe,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_44"]/*' />
		TokenAssociateToAccount = Proto.HederaFunctionality.TokenAssociateToAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_5"]/*' />
		TokenDissociateFromAccount = Proto.HederaFunctionality.TokenDissociateFromAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_6"]/*' />
		ScheduleCreate = Proto.HederaFunctionality.ScheduleCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_7"]/*' />
		ScheduleDelete = Proto.HederaFunctionality.ScheduleDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_8"]/*' />
		ScheduleSign = Proto.HederaFunctionality.ScheduleSign,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_9"]/*' />
		ScheduleGetInfo = Proto.HederaFunctionality.ScheduleGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_10"]/*' />
		[Obsolete]
		TokenGetAccountNftInfos = Proto.HederaFunctionality.TokenGetAccountNftInfos,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_11"]/*' />
		[Obsolete]
		TokenGetNftInfo = Proto.HederaFunctionality.TokenGetNftInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.time(s)"]/*' />
		TokenGetNftInfos = Proto.HederaFunctionality.TokenGetNftInfos,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.time(s)_2"]/*' />
		TokenFeeScheduleUpdate = Proto.HederaFunctionality.TokenFeeScheduleUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_45"]/*' />
		[Obsolete]
		NetworkGetExecutionTime = Proto.HederaFunctionality.NetworkGetExecutionTime,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Delete(unapprove)"]/*' />
		TokenPause = Proto.HederaFunctionality.TokenPause,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Delete(unapprove)_2"]/*' />
		TokenUnpause = Proto.HederaFunctionality.TokenUnpause,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Delete(unapprove)_3"]/*' />
		CryptoApproveAllowance = Proto.HederaFunctionality.CryptoApproveAllowance,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Ethereum(EVM)"]/*' />
		CryptoDeleteAllowance = Proto.HederaFunctionality.CryptoDeleteAllowance,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Ethereum(EVM)_2"]/*' />
		GetAccountDetails = Proto.HederaFunctionality.GetAccountDetails,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_46"]/*' />
		EthereumTransaction = Proto.HederaFunctionality.EthereumTransaction,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_47"]/*' />
		NodeStakeUpdate = Proto.HederaFunctionality.NodeStakeUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_48"]/*' />
		UtilPrng = Proto.HederaFunctionality.UtilPrng,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_49"]/*' />
		[Obsolete]
		TransactionGetFastRecord = Proto.HederaFunctionality.TransactionGetFastRecord,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_50"]/*' />
		TokenUpdateNfts = Proto.HederaFunctionality.TokenUpdateNfts,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_51"]/*' />
		NodeCreate = Proto.HederaFunctionality.NodeCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_52"]/*' />
		NodeUpdate = Proto.HederaFunctionality.NodeUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.sender(s)"]/*' />
		NodeDelete = Proto.HederaFunctionality.NodeDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.sender(s)_2"]/*' />
		// /**
		TokenReject = Proto.HederaFunctionality.TokenReject,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.sender(s)_3"]/*' />
		TokenAirdrop = Proto.HederaFunctionality.TokenAirdrop,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_53"]/*' />
		// /**
		TokenCancelAirdrop = Proto.HederaFunctionality.TokenCancelAirdrop,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_54"]/*' />
		TokenClaimAirdrop = Proto.HederaFunctionality.TokenClaimAirdrop,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_55"]/*' />
		StateSignatureTransaction = Proto.HederaFunctionality.StateSignatureTransaction,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_56"]/*' />
		HintsKeyPublication = Proto.HederaFunctionality.HintsKeyPublication,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_57"]/*' />
		HintsPreprocessingVote = Proto.HederaFunctionality.HintsPreprocessingVote,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_58"]/*' />
		HintsPartialSignature = Proto.HederaFunctionality.HintsPartialSignature,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_59"]/*' />
		HistoryAssemblySignature = Proto.HederaFunctionality.HistoryAssemblySignature,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_60"]/*' />
		HistoryProofKeyPublication = Proto.HederaFunctionality.HistoryProofKeyPublication,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_61"]/*' />
		HistoryProofVote = Proto.HederaFunctionality.HistoryProofVote,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_62"]/*' />
		CrsPublication = Proto.HederaFunctionality.CrsPublication,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_63"]/*' />
		AtomicBatch = Proto.HederaFunctionality.AtomicBatch,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_64"]/*' />
		LambdaSstore = Proto.HederaFunctionality.LambdaSstore,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_65"]/*' />
		HookDispatch = Proto.HederaFunctionality.HookDispatch,
	}
}