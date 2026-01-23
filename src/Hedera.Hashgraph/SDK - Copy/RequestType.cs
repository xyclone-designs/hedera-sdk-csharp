// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Enum for the request types.
    /// </summary>
    public enum RequestType
    {
		/// <summary>
		/// FUTURE - Uncomment when https://github.com/hashgraph/pbj/issues/339 is fixed;
		/// currently the PBJ-generated unit tests fail when using reserved ordinals
		/// reserved 96, 97, 98, 99;
		/// Unused - The first value is unused because this default value is
		/// ambiguous with an "unset" value and therefore should not be used.
		/// </summary>
		None = Proto.HederaFunctionality.None,
		/// <summary>
		/// Transfer tokens among accounts.
		/// </summary>
		CryptoTransfer = Proto.HederaFunctionality.CryptoTransfer,
		/// <summary>
		/// Update an account.
		/// </summary>
		CryptoUpdate = Proto.HederaFunctionality.CryptoUpdate,
		/// <summary>
		/// Delete an account.
		/// </summary>
		CryptoDelete = Proto.HederaFunctionality.CryptoDelete,
		/// <summary>
		/// Add a livehash to an account
		/// </summary>
		CryptoAddLiveHash = Proto.HederaFunctionality.CryptoAddLiveHash,
		/// <summary>
		/// Delete a livehash from an account
		/// </summary>
		CryptoDeleteLiveHash = Proto.HederaFunctionality.CryptoDeleteLiveHash,
		/// <summary>
		/// Execute a smart contract call.
		/// </summary>
		ContractCall = Proto.HederaFunctionality.ContractCall,
		/// <summary>
		/// Create a smart contract.
		/// </summary>
		ContractCreate = Proto.HederaFunctionality.ContractCreate,
		/// <summary>
		/// Update a smart contract.
		/// </summary>
		ContractUpdate = Proto.HederaFunctionality.ContractUpdate,
		/// <summary>
		/// Create a "file" stored in the ledger.
		/// </summary>
		FileCreate = Proto.HederaFunctionality.FileCreate,
		/// <summary>
		/// Append data to a "file" stored in the ledger.
		/// </summary>
		FileAppend = Proto.HederaFunctionality.FileAppend,
		/// <summary>
		/// Update a "file" stored in the ledger.
		/// </summary>
		FileUpdate = Proto.HederaFunctionality.FileUpdate,
		/// <summary>
		/// Delete a "file" stored in the ledger.
		/// </summary>
		FileDelete = Proto.HederaFunctionality.FileDelete,
		/// <summary>
		/// Get the balance for an account.
		/// </summary>
		CryptoGetAccountBalance = Proto.HederaFunctionality.CryptoGetAccountBalance,
		/// <summary>
		/// Get a full account record.
		/// </summary>
		CryptoGetAccountRecords = Proto.HederaFunctionality.CryptoGetAccountRecords,
		/// <summary>
		/// Get information about a token.
		/// </summary>
		CryptoGetInfo = Proto.HederaFunctionality.CryptoGetInfo,
		/// <summary>
		/// Execute a local smart contract call.<br/>
		/// Used by contracts to call other contracts.
		/// </summary>
		// /**
		ContractCallLocal = Proto.HederaFunctionality.ContractCallLocal,
		/// <summary>
		/// Get information about a smart contract.
		/// </summary>
		ContractGetInfo = Proto.HederaFunctionality.ContractGetInfo,
		/// <summary>
		/// Get the compiled bytecode that implements a smart contract.
		/// </summary>
		ContractGetBytecode = Proto.HederaFunctionality.ContractGetBytecode,
		/// <summary>
		/// Get a smart contract record by reference to the solidity ID.
		/// </summary>
		GetBySolidityId = Proto.HederaFunctionality.GetBySolidityId,
		/// <summary>
		/// Get a smart contract by reference to the contract key.
		/// </summary>
		GetByKey = Proto.HederaFunctionality.GetByKey,
		/// <summary>
		/// Get the live hash for an account
		/// </summary>
		CryptoGetLiveHash = Proto.HederaFunctionality.CryptoGetLiveHash,
		/// <summary>
		/// Get the accounts proxy staking to a given account.
		/// </summary>
		CryptoGetStakers = Proto.HederaFunctionality.CryptoGetStakers,
		/// <summary>
		/// Get the contents of a "file" stored in the ledger.
		/// </summary>
		FileGetContents = Proto.HederaFunctionality.FileGetContents,
		/// <summary>
		/// Get the metadata for a "file" stored in the ledger.
		/// </summary>
		FileGetInfo = Proto.HederaFunctionality.FileGetInfo,
		/// <summary>
		/// Get transaction record(s) for a specified transaction ID.
		/// </summary>
		TransactionGetRecord = Proto.HederaFunctionality.TransactionGetRecord,
		/// <summary>
		/// Get all transaction records for a specified contract ID in
		/// the past 24 hours.<br/>
		/// deprecated since version 0.9.0
		/// </summary>
		ContractGetRecords = Proto.HederaFunctionality.ContractGetRecords,
		/// <summary>
		/// Create a new account
		/// </summary>
		CryptoCreate = Proto.HederaFunctionality.CryptoCreate,
		/// <summary>
		/// Delete a "system" "file" stored in the ledger.<br/>
		/// "System" files are files with special purpose and ID values within a
		/// specific range.<br/>
		/// These files require additional controls and can only be deleted when
		/// authorized by accounts with elevated privilege.
		/// </summary>
		SystemDelete = Proto.HederaFunctionality.SystemDelete,
		/// <summary>
		/// Undo the delete of a "system" "file" stored in the ledger.<br/>
		/// "System" files are files with special purpose and ID values within a
		/// specific range.<br/>
		/// These files require additional controls and can only be deleted when
		/// authorized by accounts with elevated privilege. This operation allows
		/// such files to be restored, within a reasonable timeframe, if
		/// deleted improperly.
		/// </summary>
		SystemUndelete = Proto.HederaFunctionality.SystemUndelete,
		/// <summary>
		/// Delete a smart contract
		/// </summary>
		ContractDelete = Proto.HederaFunctionality.ContractDelete,
		/// <summary>
		/// Stop all processing and "freeze" the entire network.<br/>
		/// This is generally sent immediately prior to upgrading the network.<br/>
		/// After processing this transactions all nodes enter a quiescent state.
		/// </summary>
		Freeze = Proto.HederaFunctionality.Freeze,
		/// <summary>
		/// Create a Transaction Record.<br/>
		/// This appears to be purely internal and unused.
		/// </summary>
		CreateTransactionRecord = Proto.HederaFunctionality.CreateTransactionRecord,
		/// <summary>
		/// Auto-renew an account.<br/>
		/// This is used for internal fee calculations.
		/// </summary>
		CryptoAccountAutoRenew = Proto.HederaFunctionality.CryptoAccountAutoRenew,
		/// <summary>
		/// Auto-renew a smart contract.<br/>
		/// This is used for internal fee calculations.
		/// </summary>
		ContractAutoRenew = Proto.HederaFunctionality.ContractAutoRenew,
		/// <summary>
		/// Get version information for the ledger.<br/>
		/// This returns a the version of the software currently running the network
		/// for both the protocol buffers and the network services (node).
		/// </summary>
		GetVersionInfo = Proto.HederaFunctionality.GetVersionInfo,
		/// <summary>
		/// Get a receipt for a specified transaction ID.
		/// </summary>
		TransactionGetReceipt = Proto.HederaFunctionality.TransactionGetReceipt,
		/// <summary>
		/// Create a topic for the Hedera Consensus Service (HCS).
		/// </summary>
		ConsensusCreateTopic = Proto.HederaFunctionality.ConsensusCreateTopic,
		/// <summary>
		/// Update an HCS topic.
		/// </summary>
		ConsensusUpdateTopic = Proto.HederaFunctionality.ConsensusUpdateTopic,
		/// <summary>
		/// Delete an HCS topic.
		/// </summary>
		ConsensusDeleteTopic = Proto.HederaFunctionality.ConsensusDeleteTopic,
		/// <summary>
		/// Get metadata (information) for an HCS topic.
		/// </summary>
		ConsensusGetTopicInfo = Proto.HederaFunctionality.ConsensusGetTopicInfo,
		/// <summary>
		/// Publish a message to an HCS topic.
		/// </summary>
		ConsensusSubmitMessage = Proto.HederaFunctionality.ConsensusSubmitMessage,
		/// <summary>
		/// Submit a transaction, bypassing intake checking.
		/// Only enabled in local-mode.
		/// </summary>
		// /**
		UncheckedSubmit = Proto.HederaFunctionality.UncheckedSubmit,
		/// <summary>
		/// Create a token for the Hedera Token Service (HTS).
		/// </summary>
		TokenCreate = Proto.HederaFunctionality.TokenCreate,
		/// <summary>
		/// Get metadata (information) for an HTS token.
		/// </summary>
		TokenGetInfo = Proto.HederaFunctionality.TokenGetInfo,
		/// <summary>
		/// Freeze a specific account with respect to a specific HTS token.
		/// <p>
		/// Once this transaction completes that account CANNOT send or receive
		/// the specified token.
		/// </summary>
		TokenFreezeAccount = Proto.HederaFunctionality.TokenFreezeAccount,
		/// <summary>
		/// Remove a "freeze" from an account with respect to a specific HTS token.
		/// </summary>
		TokenUnfreezeAccount = Proto.HederaFunctionality.TokenUnfreezeAccount,
		/// <summary>
		/// Grant KYC status to an account for a specific HTS token.
		/// </summary>
		TokenGrantKycToAccount = Proto.HederaFunctionality.TokenGrantKycToAccount,
		/// <summary>
		/// Revoke KYC status from an account for a specific HTS token.
		/// </summary>
		TokenRevokeKycFromAccount = Proto.HederaFunctionality.TokenRevokeKycFromAccount,
		/// <summary>
		/// Delete a specific HTS token.
		/// </summary>
		TokenDelete = Proto.HederaFunctionality.TokenDelete,
		/// <summary>
		/// Update a specific HTS token.
		/// </summary>
		TokenUpdate = Proto.HederaFunctionality.TokenUpdate,
		/// <summary>
		/// Mint HTS token amounts to the treasury account for that token.
		/// </summary>
		TokenMint = Proto.HederaFunctionality.TokenMint,
		/// <summary>
		/// Burn HTS token amounts from the treasury account for that token.
		/// </summary>
		TokenBurn = Proto.HederaFunctionality.TokenBurn,
		/// <summary>
		/// Wipe all amounts for a specific HTS token from a specified account.
		/// </summary>
		TokenAccountWipe = Proto.HederaFunctionality.TokenAccountWipe,
		/// <summary>
		/// Associate a specific HTS token to an account.
		/// </summary>
		TokenAssociateToAccount = Proto.HederaFunctionality.TokenAssociateToAccount,
		/// <summary>
		/// Dissociate a specific HTS token from an account.
		/// </summary>
		TokenDissociateFromAccount = Proto.HederaFunctionality.TokenDissociateFromAccount,
		/// <summary>
		/// Create a scheduled transaction
		/// </summary>
		ScheduleCreate = Proto.HederaFunctionality.ScheduleCreate,
		/// <summary>
		/// Delete a scheduled transaction
		/// </summary>
		ScheduleDelete = Proto.HederaFunctionality.ScheduleDelete,
		/// <summary>
		/// Sign a scheduled transaction
		/// </summary>
		ScheduleSign = Proto.HederaFunctionality.ScheduleSign,
		/// <summary>
		/// Get metadata (information) for a scheduled transaction
		/// </summary>
		ScheduleGetInfo = Proto.HederaFunctionality.ScheduleGetInfo,
		/// <summary>
		/// Get NFT metadata (information) for a range of NFTs associated to a
		/// specific non-fungible/unique HTS token and owned by a specific account.
		/// </summary>
		TokenGetAccountNftInfos = Proto.HederaFunctionality.TokenGetAccountNftInfos,
		/// <summary>
		/// Get metadata (information) for a specific NFT identified by token and
		/// serial number.
		/// </summary>
		TokenGetNftInfo = Proto.HederaFunctionality.TokenGetNftInfo,
		/// <summary>
		/// Get NFT metadata (information) for a range of NFTs associated to a
		/// specific non-fungible/unique HTS token.
		/// </summary>
		TokenGetNftInfos = Proto.HederaFunctionality.TokenGetNftInfos,
		/// <summary>
		/// Update a token's custom fee schedule.
		/// <p>
		/// If a transaction of this type is not signed by the token
		/// `fee_schedule_key` it SHALL fail with INVALID_SIGNATURE, or
		/// TOKEN_HAS_NO_FEE_SCHEDULE_KEY if there is no `fee_schedule_key` set.
		/// </summary>
		TokenFeeScheduleUpdate = Proto.HederaFunctionality.TokenFeeScheduleUpdate,
		/// <summary>
		/// Get execution time(s) for one or more "recent" TransactionIDs.
		/// </summary>
		NetworkGetExecutionTime = Proto.HederaFunctionality.NetworkGetExecutionTime,
		/// <summary>
		/// Pause a specific HTS token
		/// </summary>
		TokenPause = Proto.HederaFunctionality.TokenPause,
		/// <summary>
		/// Unpause a paused HTS token.
		/// </summary>
		TokenUnpause = Proto.HederaFunctionality.TokenUnpause,
		/// <summary>
		/// Approve an allowance for a spender relative to the owner account, which
		/// MUST sign the transaction.
		/// </summary>
		// /**
		CryptoApproveAllowance = Proto.HederaFunctionality.CryptoApproveAllowance,
		/// <summary>
		/// Delete (unapprove) an allowance previously approved
		/// for the owner account.
		/// </summary>
		CryptoDeleteAllowance = Proto.HederaFunctionality.CryptoDeleteAllowance,
		/// <summary>
		/// Get all the information about an account, including balance
		/// and allowances.<br/>
		/// This does not get a list of account records.
		/// </summary>
		GetAccountDetails = Proto.HederaFunctionality.GetAccountDetails,
		/// <summary>
		/// Perform an Ethereum (EVM) transaction.<br/>
		/// CallData may be inline if small, or in a "file" if large.
		/// </summary>
		EthereumTransaction = Proto.HederaFunctionality.EthereumTransaction,
		/// <summary>
		/// Used to indicate when the network has updated the staking information
		/// at the end of a staking period and to indicate a new staking period
		/// has started.
		/// </summary>
		NodeStakeUpdate = Proto.HederaFunctionality.NodeStakeUpdate,
		/// <summary>
		/// Generate and return a pseudorandom number based on network state.
		/// </summary>
		UtilPrng = Proto.HederaFunctionality.UtilPrng,
		/// <summary>
		/// Get a record for a "recent" transaction.
		/// </summary>
		TransactionGetFastRecord = Proto.HederaFunctionality.TransactionGetFastRecord,
		/// <summary>
		/// Update the metadata of one or more NFT's of a specific token type.
		/// </summary>
		TokenUpdateNfts = Proto.HederaFunctionality.TokenUpdateNfts,
		/// <summary>
		/// Create a node
		/// </summary>
		NodeCreate = Proto.HederaFunctionality.NodeCreate,
		/// <summary>
		/// Update a node
		/// </summary>
		NodeUpdate = Proto.HederaFunctionality.NodeUpdate,
		/// <summary>
		/// Delete a node
		/// </summary>
		NodeDelete = Proto.HederaFunctionality.NodeDelete,
		/// <summary>
		/// Transfer one or more token balances held by the requesting account
		/// to the treasury for each token type.
		/// </summary>
		// /**
		TokenReject = Proto.HederaFunctionality.TokenReject,
		/// <summary>
		/// Airdrop one or more tokens to one or more accounts.
		/// </summary>
		TokenAirdrop = Proto.HederaFunctionality.TokenAirdrop,
		/// <summary>
		/// Remove one or more pending airdrops from state on behalf of
		/// the sender(s) for each airdrop.
		/// </summary>
		// /**
		TokenCancelAirdrop = Proto.HederaFunctionality.TokenCancelAirdrop,
		/// <summary>
		/// Claim one or more pending airdrops
		/// </summary>
		TokenClaimAirdrop = Proto.HederaFunctionality.TokenClaimAirdrop,
		/// <summary>
		/// Submit a signature of a state root hash gossiped to other nodes
		/// </summary>
		StateSignatureTransaction = Proto.HederaFunctionality.StateSignatureTransaction,
		/// <summary>
		/// Publish a hinTS key to the network.
		/// </summary>
		HintsKeyPublication = Proto.HederaFunctionality.HintsKeyPublication,
		/// <summary>
		/// Vote for a particular preprocessing output of a hinTS construction.
		/// </summary>
		HintsPreprocessingVote = Proto.HederaFunctionality.HintsPreprocessingVote,
		/// <summary>
		/// Sign a partial signature for the active hinTS construction.
		/// </summary>
		HintsPartialSignature = Proto.HederaFunctionality.HintsPartialSignature,
		/// <summary>
		/// Sign a particular history assembly.
		/// </summary>
		HistoryAssemblySignature = Proto.HederaFunctionality.HistoryAssemblySignature,
		/// <summary>
		/// Publish a roster history proof key to the network.
		/// </summary>
		HistoryProofKeyPublication = Proto.HederaFunctionality.HistoryProofKeyPublication,
		/// <summary>
		/// Vote for a particular history proof.
		/// </summary>
		HistoryProofVote = Proto.HederaFunctionality.HistoryProofVote,
		/// <summary>
		/// Publish a random CRS to the network.
		/// </summary>
		CrsPublication = Proto.HederaFunctionality.CrsPublication,
		/// <summary>
		/// Submit a batch of transactions to run atomically
		/// </summary>
		AtomicBatch = Proto.HederaFunctionality.AtomicBatch,
		/// <summary>
		/// Update one or more storage slots in an lambda EVM hook.
		/// </summary>
		LambdaSstore = Proto.HederaFunctionality.LambdaSstore,
		/// <summary>
		/// (Internal-only) Dispatch a hook action.
		/// </summary>
		HookDispatch = Proto.HederaFunctionality.HookDispatch,
	}
}