// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType"]/*' />
    public enum RequestType
    {
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_2"]/*' />
		None = Proto.Services.HederaFunctionality.None,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_3"]/*' />
		CryptoTransfer = Proto.Services.HederaFunctionality.CryptoTransfer,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_4"]/*' />
		CryptoUpdate = Proto.Services.HederaFunctionality.CryptoUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_5"]/*' />
		CryptoDelete = Proto.Services.HederaFunctionality.CryptoDelete,
        /// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_6"]/*' />
        [Obsolete]
        CryptoAddLiveHash = Proto.Services.HederaFunctionality.CryptoAddLiveHash,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_7"]/*' />
		[Obsolete]
		CryptoDeleteLiveHash = Proto.Services.HederaFunctionality.CryptoDeleteLiveHash,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_8"]/*' />
		ContractCall = Proto.Services.HederaFunctionality.ContractCall,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_9"]/*' />
		ContractCreate = Proto.Services.HederaFunctionality.ContractCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_10"]/*' />
		ContractUpdate = Proto.Services.HederaFunctionality.ContractUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_11"]/*' />
		FileCreate = Proto.Services.HederaFunctionality.FileCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_12"]/*' />
		FileAppend = Proto.Services.HederaFunctionality.FileAppend,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_13"]/*' />
		FileUpdate = Proto.Services.HederaFunctionality.FileUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_14"]/*' />
		FileDelete = Proto.Services.HederaFunctionality.FileDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_15"]/*' />
		CryptoGetAccountBalance = Proto.Services.HederaFunctionality.CryptoGetAccountBalance,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_16"]/*' />
		CryptoGetAccountRecords = Proto.Services.HederaFunctionality.CryptoGetAccountRecords,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_17"]/*' />
		CryptoGetInfo = Proto.Services.HederaFunctionality.CryptoGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_18"]/*' />
		// /**
		ContractCallLocal = Proto.Services.HederaFunctionality.ContractCallLocal,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_19"]/*' />
		ContractGetInfo = Proto.Services.HederaFunctionality.ContractGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_20"]/*' />
		ContractGetBytecode = Proto.Services.HederaFunctionality.ContractGetBytecode,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_21"]/*' />
		GetBySolidityId = Proto.Services.HederaFunctionality.GetBySolidityId,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_22"]/*' />
		GetByKey = Proto.Services.HederaFunctionality.GetByKey,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_23"]/*' />
		[Obsolete]
		CryptoGetLiveHash = Proto.Services.HederaFunctionality.CryptoGetLiveHash,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_24"]/*' />
		[Obsolete]
		CryptoGetStakers = Proto.Services.HederaFunctionality.CryptoGetStakers,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_25"]/*' />
		FileGetContents = Proto.Services.HederaFunctionality.FileGetContents,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_26"]/*' />
		FileGetInfo = Proto.Services.HederaFunctionality.FileGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_27"]/*' />
		TransactionGetRecord = Proto.Services.HederaFunctionality.TransactionGetRecord,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_28"]/*' />
		[Obsolete]
		ContractGetRecords = Proto.Services.HederaFunctionality.ContractGetRecords,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_29"]/*' />
		CryptoCreate = Proto.Services.HederaFunctionality.CryptoCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_30"]/*' />
		SystemDelete = Proto.Services.HederaFunctionality.SystemDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_31"]/*' />
		SystemUndelete = Proto.Services.HederaFunctionality.SystemUndelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_32"]/*' />
		ContractDelete = Proto.Services.HederaFunctionality.ContractDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_33"]/*' />
		Freeze = Proto.Services.HederaFunctionality.Freeze,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.services(node)"]/*' />
		CreateTransactionRecord = Proto.Services.HederaFunctionality.CreateTransactionRecord,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.services(node)_2"]/*' />
		CryptoAccountAutoRenew = Proto.Services.HederaFunctionality.CryptoAccountAutoRenew,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.services(node)_3"]/*' />
		ContractAutoRenew = Proto.Services.HederaFunctionality.ContractAutoRenew,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HCS)"]/*' />
		GetVersionInfo = Proto.Services.HederaFunctionality.GetVersionInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HCS)_2"]/*' />
		TransactionGetReceipt = Proto.Services.HederaFunctionality.TransactionGetReceipt,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)"]/*' />
		ConsensusCreateTopic = Proto.Services.HederaFunctionality.ConsensusCreateTopic,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_2"]/*' />
		ConsensusUpdateTopic = Proto.Services.HederaFunctionality.ConsensusUpdateTopic,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_3"]/*' />
		ConsensusDeleteTopic = Proto.Services.HederaFunctionality.ConsensusDeleteTopic,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HTS)"]/*' />
		ConsensusGetTopicInfo = Proto.Services.HederaFunctionality.ConsensusGetTopicInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HTS)_2"]/*' />
		ConsensusSubmitMessage = Proto.Services.HederaFunctionality.ConsensusSubmitMessage,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Service(HTS)_3"]/*' />
		// /**
		UncheckedSubmit = Proto.Services.HederaFunctionality.UncheckedSubmit,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_4"]/*' />
		TokenCreate = Proto.Services.HederaFunctionality.TokenCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_34"]/*' />
		TokenGetInfo = Proto.Services.HederaFunctionality.TokenGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_35"]/*' />
		TokenFreezeAccount = Proto.Services.HederaFunctionality.TokenFreezeAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_36"]/*' />
		TokenUnfreezeAccount = Proto.Services.HederaFunctionality.TokenUnfreezeAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_37"]/*' />
		TokenGrantKycToAccount = Proto.Services.HederaFunctionality.TokenGrantKycToAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_38"]/*' />
		TokenRevokeKycFromAccount = Proto.Services.HederaFunctionality.TokenRevokeKycFromAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_39"]/*' />
		TokenDelete = Proto.Services.HederaFunctionality.TokenDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_40"]/*' />
		TokenUpdate = Proto.Services.HederaFunctionality.TokenUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_41"]/*' />
		TokenMint = Proto.Services.HederaFunctionality.TokenMint,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_42"]/*' />
		TokenBurn = Proto.Services.HederaFunctionality.TokenBurn,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_43"]/*' />
		TokenAccountWipe = Proto.Services.HederaFunctionality.TokenAccountWipe,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_44"]/*' />
		TokenAssociateToAccount = Proto.Services.HederaFunctionality.TokenAssociateToAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_5"]/*' />
		TokenDissociateFromAccount = Proto.Services.HederaFunctionality.TokenDissociateFromAccount,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_6"]/*' />
		ScheduleCreate = Proto.Services.HederaFunctionality.ScheduleCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_7"]/*' />
		ScheduleDelete = Proto.Services.HederaFunctionality.ScheduleDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_8"]/*' />
		ScheduleSign = Proto.Services.HederaFunctionality.ScheduleSign,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_9"]/*' />
		ScheduleGetInfo = Proto.Services.HederaFunctionality.ScheduleGetInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_10"]/*' />
		[Obsolete]
		TokenGetAccountNftInfos = Proto.Services.HederaFunctionality.TokenGetAccountNftInfos,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.metadata(information)_11"]/*' />
		[Obsolete]
		TokenGetNftInfo = Proto.Services.HederaFunctionality.TokenGetNftInfo,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.time(s)"]/*' />
		TokenGetNftInfos = Proto.Services.HederaFunctionality.TokenGetNftInfos,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.time(s)_2"]/*' />
		TokenFeeScheduleUpdate = Proto.Services.HederaFunctionality.TokenFeeScheduleUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_45"]/*' />
		[Obsolete]
		NetworkGetExecutionTime = Proto.Services.HederaFunctionality.NetworkGetExecutionTime,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Delete(unapprove)"]/*' />
		TokenPause = Proto.Services.HederaFunctionality.TokenPause,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Delete(unapprove)_2"]/*' />
		TokenUnpause = Proto.Services.HederaFunctionality.TokenUnpause,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Delete(unapprove)_3"]/*' />
		CryptoApproveAllowance = Proto.Services.HederaFunctionality.CryptoApproveAllowance,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Ethereum(EVM)"]/*' />
		CryptoDeleteAllowance = Proto.Services.HederaFunctionality.CryptoDeleteAllowance,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.Ethereum(EVM)_2"]/*' />
		GetAccountDetails = Proto.Services.HederaFunctionality.GetAccountDetails,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_46"]/*' />
		EthereumTransaction = Proto.Services.HederaFunctionality.EthereumTransaction,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_47"]/*' />
		NodeStakeUpdate = Proto.Services.HederaFunctionality.NodeStakeUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_48"]/*' />
		UtilPrng = Proto.Services.HederaFunctionality.UtilPrng,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_49"]/*' />
		[Obsolete]
		TransactionGetFastRecord = Proto.Services.HederaFunctionality.TransactionGetFastRecord,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_50"]/*' />
		TokenUpdateNfts = Proto.Services.HederaFunctionality.TokenUpdateNfts,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_51"]/*' />
		NodeCreate = Proto.Services.HederaFunctionality.NodeCreate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_52"]/*' />
		NodeUpdate = Proto.Services.HederaFunctionality.NodeUpdate,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.sender(s)"]/*' />
		NodeDelete = Proto.Services.HederaFunctionality.NodeDelete,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.sender(s)_2"]/*' />
		// /**
		TokenReject = Proto.Services.HederaFunctionality.TokenReject,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="M:RequestType.sender(s)_3"]/*' />
		TokenAirdrop = Proto.Services.HederaFunctionality.TokenAirdrop,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_53"]/*' />
		// /**
		TokenCancelAirdrop = Proto.Services.HederaFunctionality.TokenCancelAirdrop,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_54"]/*' />
		TokenClaimAirdrop = Proto.Services.HederaFunctionality.TokenClaimAirdrop,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_55"]/*' />
		StateSignatureTransaction = Proto.Services.HederaFunctionality.StateSignatureTransaction,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_56"]/*' />
		HintsKeyPublication = Proto.Services.HederaFunctionality.HintsKeyPublication,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_57"]/*' />
		HintsPreprocessingVote = Proto.Services.HederaFunctionality.HintsPreprocessingVote,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_58"]/*' />
		HintsPartialSignature = Proto.Services.HederaFunctionality.HintsPartialSignature,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_59"]/*' />
		HistoryAssemblySignature = Proto.Services.HederaFunctionality.HistoryAssemblySignature,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_60"]/*' />
		HistoryProofKeyPublication = Proto.Services.HederaFunctionality.HistoryProofKeyPublication,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_61"]/*' />
		HistoryProofVote = Proto.Services.HederaFunctionality.HistoryProofVote,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_62"]/*' />
		CrsPublication = Proto.Services.HederaFunctionality.CrsPublication,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_63"]/*' />
		AtomicBatch = Proto.Services.HederaFunctionality.AtomicBatch,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_64"]/*' />
		LambdaSstore = Proto.Services.HederaFunctionality.LambdaSstore,
		/// <include file="RequestType.cs.xml" path='docs/member[@name="T:RequestType_65"]/*' />
		HookDispatch = Proto.Services.HederaFunctionality.HookDispatch,
	}
}
