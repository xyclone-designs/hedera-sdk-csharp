
using System;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Enum for the request types.
 */
	public enum RequestType
	{
		/**
		 * FUTURE - Uncomment when https://github.com/hashgraph/pbj/issues/339 is fixed;
		 * currently the PBJ-generated unit tests fail when using reserved ordinals
		 * reserved 96, 97, 98, 99;
		 * Unused - The first value is unused because this default value is
		 * ambiguous with an "unset" value and therefore should not be used.
		 */
		None = Proto.HederaFunctionality.None,

		/**
		 * Transfer tokens among accounts.
		 */
		CryptoTransfer = Proto.HederaFunctionality.CryptoTransfer,

		/**
		 * Update an account.
		 */
		CryptoUpdate = Proto.HederaFunctionality.CryptoUpdate,

		/**
		 * Delete an account.
		 */
		CryptoDelete = Proto.HederaFunctionality.CryptoDelete,

		/**
		 * Add a livehash to an account
		 */
		[Obsolete]
		CryptoAddLiveHash = Proto.HederaFunctionality.CryptoAddLiveHash,

		/**
		 * Delete a livehash from an account
		 */
		[Obsolete]
		CryptoDeleteLiveHash = Proto.HederaFunctionality.CryptoDeleteLiveHash,

		/**
		 * Execute a smart contract call.
		 */
		ContractCall = Proto.HederaFunctionality.ContractCall,

		/**
		 * Create a smart contract.
		 */
		ContractCreate = Proto.HederaFunctionality.ContractCreate,

		/**
		 * Update a smart contract.
		 */
		ContractUpdate = Proto.HederaFunctionality.ContractUpdate,

		/**
		 * Create a "file" stored in the ledger.
		 */
		FileCreate = Proto.HederaFunctionality.FileCreate,

		/**
		 * Append data to a "file" stored in the ledger.
		 */
		FileAppend = Proto.HederaFunctionality.FileAppend,

		/**
		 * Update a "file" stored in the ledger.
		 */
		FileUpdate = Proto.HederaFunctionality.FileUpdate,

		/**
		 * Delete a "file" stored in the ledger.
		 */
		FileDelete = Proto.HederaFunctionality.FileDelete,

		/**
		 * Get the balance for an account.
		 */
		CryptoGetAccountBalance = Proto.HederaFunctionality.CryptoGetAccountBalance,

		/**
		 * Get a full account record.
		 */
		CryptoGetAccountRecords = Proto.HederaFunctionality.CryptoGetAccountRecords,

		/**
		 * Get information about a token.
		 */
		CryptoGetInfo = Proto.HederaFunctionality.CryptoGetInfo,

		/**
		 * Execute a local smart contract call.<br/>
		 * Used by contracts to call other contracts.
		 */
		ContractCallLocal = Proto.HederaFunctionality.ContractCallLocal,

		/**
		 * Get information about a smart contract.
		 */
		ContractGetInfo = Proto.HederaFunctionality.ContractGetInfo,

		/**
		 * Get the compiled bytecode that implements a smart contract.
		 */
		ContractGetBytecode = Proto.HederaFunctionality.ContractGetBytecode,

		/**
		 * Get a smart contract record by reference to the solidity ID.
		 */
		GetBySolidityID = Proto.HederaFunctionality.GetBySolidityId,

		/**
		 * Get a smart contract by reference to the contract key.
		 */
		GetByKey = Proto.HederaFunctionality.GetByKey,

		/**
		 * Get the live hash for an account
		 */
		[Obsolete]
		CryptoGetLiveHash = Proto.HederaFunctionality.CryptoGetLiveHash,

		/**
		 * Get the accounts proxy staking to a given account.
		 */
		[Obsolete]
		CryptoGetStakers = Proto.HederaFunctionality.CryptoGetStakers,

		/**
		 * Get the contents of a "file" stored in the ledger.
		 */
		FileGetContents = Proto.HederaFunctionality.FileGetContents,

		/**
		 * Get the metadata for a "file" stored in the ledger.
		 */
		FileGetInfo = Proto.HederaFunctionality.FileGetInfo,

		/**
		 * Get transaction record(s) for a specified transaction ID.
		 */
		TransactionGetRecord = Proto.HederaFunctionality.TransactionGetRecord,

		/**
		 * Get all transaction records for a specified contract ID in
		 * the past 24 hours.<br/>
		 * deprecated since version 0.9.0
		 */
		[Obsolete]
		ContractGetRecords = Proto.HederaFunctionality.ContractGetRecords,

		/**
		 * Create a new account
		 */
		CryptoCreate = Proto.HederaFunctionality.CryptoCreate,

		/**
		 * Delete a "system" "file" stored in the ledger.<br/>
		 * "System" files are files with special purpose and ID values within a
		 * specific range.<br/>
		 * These files require additional controls and can only be deleted when
		 * authorized by accounts with elevated privilege.
		 */
		SystemDelete = Proto.HederaFunctionality.SystemDelete,

		/**
		 * Undo the delete of a "system" "file" stored in the ledger.<br/>
		 * "System" files are files with special purpose and ID values within a
		 * specific range.<br/>
		 * These files require additional controls and can only be deleted when
		 * authorized by accounts with elevated privilege. This operation allows
		 * such files to be restored, within a reasonable timeframe, if
		 * deleted improperly.
		 */
		SystemUndelete = Proto.HederaFunctionality.SystemUndelete,

		/**
		 * Delete a smart contract
		 */
		ContractDelete = Proto.HederaFunctionality.ContractDelete,

		/**
		 * Stop all processing and "freeze" the entire network.<br/>
		 * This is generally sent immediately prior to upgrading the network.<br/>
		 * After processing this transactions all nodes enter a quiescent state.
		 */
		Freeze = Proto.HederaFunctionality.Freeze,

		/**
		 * Create a Transaction Record.<br/>
		 * This appears to be purely internal and unused.
		 */
		CreateTransactionRecord = Proto.HederaFunctionality.CreateTransactionRecord,

		/**
		 * Auto-renew an account.<br/>
		 * This is used for internal fee calculations.
		 */
		CryptoAccountAutoRenew = Proto.HederaFunctionality.CryptoAccountAutoRenew,

		/**
		 * Auto-renew a smart contract.<br/>
		 * This is used for internal fee calculations.
		 */
		ContractAutoRenew = Proto.HederaFunctionality.ContractAutoRenew,

		/**
		 * Get version information for the ledger.<br/>
		 * This returns a the version of the software currently running the network
		 * for both the protocol buffers and the network services (node).
		 */
		GetVersionInfo = Proto.HederaFunctionality.GetVersionInfo,

		/**
		 * Get a receipt for a specified transaction ID.
		 */
		TransactionGetReceipt = Proto.HederaFunctionality.TransactionGetReceipt,

		/**
		 * Create a topic for the Hedera Consensus Service (HCS).
		 */
		ConsensusCreateTopic = Proto.HederaFunctionality.ConsensusCreateTopic,

		/**
		 * Update an HCS topic.
		 */
		ConsensusUpdateTopic = Proto.HederaFunctionality.ConsensusUpdateTopic,

		/**
		 * Delete an HCS topic.
		 */
		ConsensusDeleteTopic = Proto.HederaFunctionality.ConsensusDeleteTopic,

		/**
		 * Get metadata (information) for an HCS topic.
		 */
		ConsensusGetTopicInfo = Proto.HederaFunctionality.ConsensusGetTopicInfo,

		/**
		 * Publish a message to an HCS topic.
		 */
		ConsensusSubmitMessage = Proto.HederaFunctionality.ConsensusSubmitMessage,

		/**
		 * Submit a transaction, bypassing intake checking.
		 * Only enabled in local-mode.
		 */
		UncheckedSubmit = Proto.HederaFunctionality.UncheckedSubmit,

		/**
		 * Create a token for the Hedera Token Service (HTS).
		 */
		TokenCreate = Proto.HederaFunctionality.TokenCreate,

		/**
		 * Get metadata (information) for an HTS token.
		 */
		TokenGetInfo = Proto.HederaFunctionality.TokenGetInfo,

		/**
		 * Freeze a specific account with respect to a specific HTS token.
		 * <p>
		 * Once this transaction completes that account CANNOT send or receive
		 * the specified token.
		 */
		TokenFreezeAccount = Proto.HederaFunctionality.TokenFreezeAccount,

		/**
		 * Remove a "freeze" from an account with respect to a specific HTS token.
		 */
		TokenUnfreezeAccount = Proto.HederaFunctionality.TokenUnfreezeAccount,

		/**
		 * Grant KYC status to an account for a specific HTS token.
		 */
		TokenGrantKycToAccount = Proto.HederaFunctionality.TokenGrantKycToAccount,

		/**
		 * Revoke KYC status from an account for a specific HTS token.
		 */
		TokenRevokeKycFromAccount = Proto.HederaFunctionality.TokenRevokeKycFromAccount,

		/**
		 * Delete a specific HTS token.
		 */
		TokenDelete = Proto.HederaFunctionality.TokenDelete,

		/**
		 * Update a specific HTS token.
		 */
		TokenUpdate = Proto.HederaFunctionality.TokenUpdate,

		/**
		 * Mint HTS token amounts to the treasury account for that token.
		 */
		TokenMint = Proto.HederaFunctionality.TokenMint,

		/**
		 * Burn HTS token amounts from the treasury account for that token.
		 */
		TokenBurn = Proto.HederaFunctionality.TokenBurn,

		/**
		 * Wipe all amounts for a specific HTS token from a specified account.
		 */
		TokenAccountWipe = Proto.HederaFunctionality.TokenAccountWipe,

		/**
		 * Associate a specific HTS token to an account.
		 */
		TokenAssociateToAccount = Proto.HederaFunctionality.TokenAssociateToAccount,

		/**
		 * Dissociate a specific HTS token from an account.
		 */
		TokenDissociateFromAccount = Proto.HederaFunctionality.TokenDissociateFromAccount,

		/**
		 * Create a scheduled transaction
		 */
		ScheduleCreate = Proto.HederaFunctionality.ScheduleCreate,

		/**
		 * Delete a scheduled transaction
		 */
		ScheduleDelete = Proto.HederaFunctionality.ScheduleDelete,

		/**
		 * Sign a scheduled transaction
		 */
		ScheduleSign = Proto.HederaFunctionality.ScheduleSign,

		/**
		 * Get metadata (information) for a scheduled transaction
		 */
		ScheduleGetInfo = Proto.HederaFunctionality.ScheduleGetInfo,

		/**
		 * Get NFT metadata (information) for a range of NFTs associated to a
		 * specific non-fungible/unique HTS token and owned by a specific account.
		 */
		[Obsolete]
		TokenGetAccountNftInfos = Proto.HederaFunctionality.TokenGetAccountNftInfos,

		/**
		 * Get metadata (information) for a specific NFT identified by token and
		 * serial number.
		 */
		[Obsolete]
		TokenGetNftInfo = Proto.HederaFunctionality.TokenGetNftInfo,

		/**
		 * Get NFT metadata (information) for a range of NFTs associated to a
		 * specific non-fungible/unique HTS token.
		 */
		TokenGetNftInfos = Proto.HederaFunctionality.TokenGetNftInfos,

		/**
		 * Update a token's custom fee schedule.
		 * <p>
		 * If a transaction of this type is not signed by the token
		 * `fee_schedule_key` it SHALL fail with INVALID_SIGNATURE, or
		 * TOKEN_HAS_NO_FEE_SCHEDULE_KEY if there is no `fee_schedule_key` set.
		 */
		TokenFeeScheduleUpdate = Proto.HederaFunctionality.TokenFeeScheduleUpdate,

		/**
		 * Get execution time(s) for one or more "recent" TransactionIDs.
		 */
		[Obsolete]
		NetworkGetExecutionTime = Proto.HederaFunctionality.NetworkGetExecutionTime,

		/**
		 * Pause a specific HTS token
		 */
		TokenPause = Proto.HederaFunctionality.TokenPause,

		/**
		 * Unpause a paused HTS token.
		 */
		TokenUnpause = Proto.HederaFunctionality.TokenUnpause,

		/**
		 * Approve an allowance for a spender relative to the owner account, which
		 * MUST sign the transaction.
		 */
		CryptoApproveAllowance = Proto.HederaFunctionality.CryptoApproveAllowance,

		/**
		 * Delete (unapprove) an allowance previously approved
		 * for the owner account.
		 */
		CryptoDeleteAllowance = Proto.HederaFunctionality.CryptoDeleteAllowance,

		/**
		 * Get all the information about an account, including balance
		 * and allowances.<br/>
		 * This does not get a list of account records.
		 */
		GetAccountDetails = Proto.HederaFunctionality.GetAccountDetails,

		/**
		 * Perform an Ethereum (EVM) transaction.<br/>
		 * CallData may be inline if small, or in a "file" if large.
		 */
		EthereumTransaction = Proto.HederaFunctionality.EthereumTransaction,

		/**
		 * Used to indicate when the network has updated the staking information
		 * at the end of a staking period and to indicate a new staking period
		 * has started.
		 */
		NodeStakeUpdate = Proto.HederaFunctionality.NodeStakeUpdate,

		/**
		 * Generate and return a pseudorandom number based on network state.
		 */
		UtilPrng = Proto.HederaFunctionality.UtilPrng,

		/**
		 * Get a record for a "recent" transaction.
		 */
		[Obsolete]
		TransactionGetFastRecord = Proto.HederaFunctionality.TransactionGetFastRecord,

		/**
		 * Update the metadata of one or more NFT's of a specific token type.
		 */
		TokenUpdateNfts = Proto.HederaFunctionality.TokenUpdateNfts,

		/**
		 * Create a node
		 */
		NodeCreate = Proto.HederaFunctionality.NodeCreate,

		/**
		 * Update a node
		 */
		NodeUpdate = Proto.HederaFunctionality.NodeUpdate,

		/**
		 * Delete a node
		 */
		NodeDelete = Proto.HederaFunctionality.NodeDelete,

		/**
		 * Transfer one or more token balances held by the requesting account
		 * to the treasury for each token type.
		 */
		TokenReject = Proto.HederaFunctionality.TokenReject,

		/**
		 * Airdrop one or more tokens to one or more accounts.
		 */
		TokenAirdrop = Proto.HederaFunctionality.TokenAirdrop,

		/**
		 * Remove one or more pending airdrops from state on behalf of
		 * the sender(s) for each airdrop.
		 */
		TokenCancelAirdrop = Proto.HederaFunctionality.TokenCancelAirdrop,

		/**
		 * Claim one or more pending airdrops
		 */
		TokenClaimAirdrop = Proto.HederaFunctionality.TokenClaimAirdrop,

		/**
		 * Submit a signature of a state root hash gossiped to other nodes
		 */
		StateSignatureTransaction = Proto.HederaFunctionality.StateSignatureTransaction,

		/**
		 * Publish a hinTS key to the network.
		 */
		HintsKeyPublication = Proto.HederaFunctionality.HintsKeyPublication,

		/**
		 * Vote for a particular preprocessing output of a hinTS construction.
		 */
		HintsPreprocessingVote = Proto.HederaFunctionality.HintsPreprocessingVote,

		/**
		 * Sign a partial signature for the active hinTS construction.
		 */
		HintsPartialSignature = Proto.HederaFunctionality.HintsPartialSignature,

		/**
		 * Sign a particular history assembly.
		 */
		HistoryAssemblySignature = Proto.HederaFunctionality.HistoryAssemblySignature,

		/**
		 * Publish a roster history proof key to the network.
		 */
		HistoryProofKeyPublication = Proto.HederaFunctionality.HistoryProofKeyPublication,

		/**
		 * Vote for a particular history proof.
		 */
		HistoryProofVote = Proto.HederaFunctionality.HistoryProofVote,

		/**
		 * Publish a random CRS to the network.
		 */
		CrsPublication = Proto.HederaFunctionality.CrsPublication,

		/**
		 * Submit a batch of transactions to run atomically
		 */
		AtomicBatch = Proto.HederaFunctionality.AtomicBatch,

		/**
		 * Update one or more storage slots in an lambda EVM hook.
		 */
		LambdaSStore = Proto.HederaFunctionality.LambdaSstore,

		/**
		 * (Internal-only) Dispatch a hook action.
		 */
		HookDispatch = Proto.HederaFunctionality.HookDispatch,
	}
}