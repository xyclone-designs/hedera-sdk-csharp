// SPDX-License-Identifier: Apache-2.0

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Returned in {@link TransactionReceipt} = Proto.ResponseCodeEnum.}, {@link PrecheckStatusException}
    /// and {@link ReceiptStatusException}.
    /// <p>
    /// The success variant is {@link #SUCCESS} which is what a {@link TransactionReceipt} will contain for a
    /// successful transaction.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The transaction passed the precheck validations.
        /// </summary>
        Ok = Proto.ResponseCodeEnum.Ok,
        /// <summary>
        /// For any error not handled by specific error codes listed below.
        /// </summary>
        InvalidTransaction = Proto.ResponseCodeEnum.InvalidTransaction,
        /// <summary>
        /// Payer account does not exist.
        /// </summary>
        PayerAccountNotFound = Proto.ResponseCodeEnum.PayerAccountNotFound,
        /// <summary>
        /// Node Account provided does not match the node account of the node the transaction was submitted
        /// to.
        /// </summary>
        InvalidNodeAccount = Proto.ResponseCodeEnum.InvalidNodeAccount,
        /// <summary>
        /// Pre-Check error when TransactionValidStart + transactionValidDuration is less than current
        /// consensus time.
        /// </summary>
        TransactionExpired = Proto.ResponseCodeEnum.TransactionExpired,
        /// <summary>
        /// Transaction start time is greater than current consensus time
        /// </summary>
        InvalidTransactionStart = Proto.ResponseCodeEnum.InvalidTransactionStart,
        /// <summary>
        /// The given transactionValidDuration was either non-Positive = Proto.ResponseCodeEnum.Positive, or greater than the maximum
        /// valid duration of 180 secs.
        /// </summary>
        InvalidTransactionDuration = Proto.ResponseCodeEnum.InvalidTransactionDuration,
        /// <summary>
        /// The transaction signature is not valid
        /// </summary>
        InvalidSignature = Proto.ResponseCodeEnum.InvalidSignature,
        /// <summary>
        /// Transaction memo size exceeded 100 bytes
        /// </summary>
        MemoTooLong = Proto.ResponseCodeEnum.MemoTooLong,
        /// <summary>
        /// The fee provided in the transaction is insufficient for this type of transaction
        /// </summary>
        InsufficientTxFee = Proto.ResponseCodeEnum.InsufficientTxFee,
        /// <summary>
        /// The payer account has insufficient cryptocurrency to pay the transaction fee
        /// </summary>
        InsufficientPayerBalance = Proto.ResponseCodeEnum.InsufficientPayerBalance,
        /// <summary>
        /// This transaction ID is a duplicate of one that was submitted to this node or reached consensus
        /// in the last 180 seconds (receipt period)
        /// </summary>
        DuplicateTransaction = Proto.ResponseCodeEnum.DuplicateTransaction,
        /// <summary>
        /// If API is throttled out
        /// </summary>
        Busy = Proto.ResponseCodeEnum.Busy,
        /// <summary>
        /// The API is not currently supported
        /// </summary>
        NotSupported = Proto.ResponseCodeEnum.NotSupported,
        /// <summary>
        /// The file id is invalid or does not exist
        /// </summary>
        InvalidFileId = Proto.ResponseCodeEnum.InvalidFileId,
        /// <summary>
        /// The account id is invalid or does not exist
        /// </summary>
        InvalidAccountId = Proto.ResponseCodeEnum.InvalidAccountId,
        /// <summary>
        /// The contract id is invalid or does not exist
        /// </summary>
        InvalidContractId = Proto.ResponseCodeEnum.InvalidContractId,
        /// <summary>
        /// Transaction id is not valid
        /// </summary>
        InvalidTransactionId = Proto.ResponseCodeEnum.InvalidTransactionId,
        /// <summary>
        /// Receipt for given transaction id does not exist
        /// </summary>
        ReceiptNotFound = Proto.ResponseCodeEnum.ReceiptNotFound,
        /// <summary>
        /// Record for given transaction id does not exist
        /// </summary>
        RecordNotFound = Proto.ResponseCodeEnum.RecordNotFound,
        /// <summary>
        /// The solidity id is invalid or entity with this solidity id does not exist
        /// </summary>
        InvalidSolidityId = Proto.ResponseCodeEnum.InvalidSolidityId,
        /// <summary>
        /// The responding node has submitted the transaction to the network. Its final status is still
        /// unknown.
        /// </summary>
        Unknown = Proto.ResponseCodeEnum.Unknown,
        /// <summary>
        /// The transaction succeeded
        /// </summary>
        Success = Proto.ResponseCodeEnum.Success,
        /// <summary>
        /// There was a system error and the transaction failed because of invalid request parameters.
        /// </summary>
        FailInvalid = Proto.ResponseCodeEnum.FailInvalid,
        /// <summary>
        /// There was a system error while performing fee Calculation = Proto.ResponseCodeEnum.Calculation, reserved for future.
        /// </summary>
        FailFee = Proto.ResponseCodeEnum.FailFee,
        /// <summary>
        /// There was a system error while performing balance Checks = Proto.ResponseCodeEnum.Checks, reserved for future.
        /// </summary>
        FailBalance = Proto.ResponseCodeEnum.FailBalance,
        /// <summary>
        /// Key not provided in the transaction body
        /// </summary>
        KeyRequired = Proto.ResponseCodeEnum.KeyRequired,
        /// <summary>
        /// Unsupported algorithm/encoding used for keys in the transaction
        /// </summary>
        BadEncoding = Proto.ResponseCodeEnum.BadEncoding,
        /// <summary>
        /// When the account balance is not sufficient for the transfer
        /// </summary>
        InsufficientAccountBalance = Proto.ResponseCodeEnum.InsufficientAccountBalance,
        /// <summary>
        /// During an update transaction when the system is not able to find the Users Solidity address
        /// </summary>
        InvalidSolidityAddress = Proto.ResponseCodeEnum.InvalidSolidityAddress,
        /// <summary>
        /// Not enough gas was supplied to execute transaction
        /// </summary>
        InsufficientGas = Proto.ResponseCodeEnum.InsufficientGas,
        /// <summary>
        /// contract byte code size is over the limit
        /// </summary>
        ContractSizeLimitExceeded = Proto.ResponseCodeEnum.ContractSizeLimitExceeded,
        /// <summary>
        /// local execution (query) is requested for a function which changes state
        /// </summary>
        LocalCallModificationException = Proto.ResponseCodeEnum.LocalCallModificationException,
        /// <summary>
        /// Contract REVERT OPCODE executed
        /// </summary>
        ContractRevertExecuted = Proto.ResponseCodeEnum.ContractRevertExecuted,
        /// <summary>
        /// For any contract execution related error not handled by specific error codes listed above.
        /// </summary>
        ContractExecutionException = Proto.ResponseCodeEnum.ContractExecutionException,
        /// <summary>
        /// In Query Validation = Proto.ResponseCodeEnum.Validation, account with +ve(amount) value should be Receiving node Account = Proto.ResponseCodeEnum.Account, the
        /// receiver account should be only one account in the list
        /// </summary>
        InvalidReceivingNodeAccount = Proto.ResponseCodeEnum.InvalidReceivingNodeAccount,
        /// <summary>
        /// Header is missing in Query request
        /// </summary>
        MissingQueryHeader = Proto.ResponseCodeEnum.MissingQueryHeader,
        /// <summary>
        /// The update of the account failed
        /// </summary>
        AccountUpdateFailed = Proto.ResponseCodeEnum.AccountUpdateFailed,
        /// <summary>
        /// Provided key encoding was not supported by the system
        /// </summary>
        InvalidKeyEncoding = Proto.ResponseCodeEnum.InvalidKeyEncoding,
        /// <summary>
        /// null solidity address
        /// </summary>
        NullSolidityAddress = Proto.ResponseCodeEnum.NullSolidityAddress,
        /// <summary>
        /// update of the contract failed
        /// </summary>
        ContractUpdateFailed = Proto.ResponseCodeEnum.ContractUpdateFailed,
        /// <summary>
        /// the query header is invalid
        /// </summary>
        InvalidQueryHeader = Proto.ResponseCodeEnum.InvalidQueryHeader,
        /// <summary>
        /// Invalid fee submitted
        /// </summary>
        InvalidFeeSubmitted = Proto.ResponseCodeEnum.InvalidFeeSubmitted,
        /// <summary>
        /// Payer signature is invalid
        /// </summary>
        InvalidPayerSignature = Proto.ResponseCodeEnum.InvalidPayerSignature,
        /// <summary>
        /// The keys were not provided in the request.
        /// </summary>
        KeyNotProvided = Proto.ResponseCodeEnum.KeyNotProvided,
        /// <summary>
        /// Expiration time provided in the transaction was invalid.
        /// </summary>
        InvalidExpirationTime = Proto.ResponseCodeEnum.InvalidExpirationTime,
        /// <summary>
        /// WriteAccess Control Keys are not provided for the file
        /// </summary>
        NoWaclKey = Proto.ResponseCodeEnum.NoWaclKey,
        /// <summary>
        /// The contents of file are provided as empty.
        /// </summary>
        FileContentEmpty = Proto.ResponseCodeEnum.FileContentEmpty,
        /// <summary>
        /// The crypto transfer credit and debit do not sum equal to 0
        /// </summary>
        InvalidAccountAmounts = Proto.ResponseCodeEnum.InvalidAccountAmounts,
        /// <summary>
        /// Transaction body provided is empty
        /// </summary>
        EmptyTransactionBody = Proto.ResponseCodeEnum.EmptyTransactionBody,
        /// <summary>
        /// Invalid transaction body provided
        /// </summary>
        InvalidTransactionBody = Proto.ResponseCodeEnum.InvalidTransactionBody,
        /// <summary>
        /// the type of key (base ed25519 Key = Proto.ResponseCodeEnum.Key, Keylist = Proto.ResponseCodeEnum.Keylist, or ThresholdKey) does not match the type of
        /// signature (base ed25519 Signature = Proto.ResponseCodeEnum.Signature, Signaturelist = Proto.ResponseCodeEnum.Signaturelist, or ThresholdKeySignature)
        /// </summary>
        InvalidSignatureTypeMismatchingKey = Proto.ResponseCodeEnum.InvalidSignatureTypeMismatchingKey,
        /// <summary>
        /// the number of key (Keylist = Proto.ResponseCodeEnum.Keylist, or ThresholdKey) does not match that of signature (Signaturelist = Proto.ResponseCodeEnum.Signaturelist,
        /// or ThresholdKeySignature). e.g. if a keyList has 3 base Keys = Proto.ResponseCodeEnum.Keys, then the corresponding
        /// signatureList should also have 3 base signatures.
        /// </summary>
        InvalidSignatureCountMismatchingKey = Proto.ResponseCodeEnum.InvalidSignatureCountMismatchingKey,
        /// <summary>
        /// the livehash body is empty
        /// </summary>
        EmptyLiveHashBody = Proto.ResponseCodeEnum.EmptyLiveHashBody,
        /// <summary>
        /// the livehash data is missing
        /// </summary>
        EmptyLiveHash = Proto.ResponseCodeEnum.EmptyLiveHash,
        /// <summary>
        /// the keys for a livehash are missing
        /// </summary>
        EmptyLiveHashKeys = Proto.ResponseCodeEnum.EmptyLiveHashKeys,
        /// <summary>
        /// the livehash data is not the output of a SHA-384 digest
        /// </summary>
        InvalidLiveHashSize = Proto.ResponseCodeEnum.InvalidLiveHashSize,
        /// <summary>
        /// the query body is empty
        /// </summary>
        EmptyQueryBody = Proto.ResponseCodeEnum.EmptyQueryBody,
        /// <summary>
        /// the crypto livehash query is empty
        /// </summary>
        EmptyLiveHashQuery = Proto.ResponseCodeEnum.EmptyLiveHashQuery,
        /// <summary>
        /// the livehash is not present
        /// </summary>
        LiveHashNotFound = Proto.ResponseCodeEnum.LiveHashNotFound,
        /// <summary>
        /// the account id passed has not yet been created.
        /// </summary>
        AccountIdDoesNotExist = Proto.ResponseCodeEnum.AccountIdDoesNotExist,
        /// <summary>
        /// the livehash already exists for a given account
        /// </summary>
        LiveHashAlreadyExists = Proto.ResponseCodeEnum.LiveHashAlreadyExists,
        /// <summary>
        /// File WACL keys are invalid
        /// </summary>
        InvalidFileWacl = Proto.ResponseCodeEnum.InvalidFileWacl,
        /// <summary>
        /// Serialization failure
        /// </summary>
        SerializationFailed = Proto.ResponseCodeEnum.SerializationFailed,
        /// <summary>
        /// The size of the Transaction is greater than transactionMaxBytes
        /// </summary>
        TransactionOversize = Proto.ResponseCodeEnum.TransactionOversize,
        /// <summary>
        /// The Transaction has more than 50 levels
        /// </summary>
        TransactionTooManyLayers = Proto.ResponseCodeEnum.TransactionTooManyLayers,
        /// <summary>
        /// Contract is marked as deleted
        /// </summary>
        ContractDeleted = Proto.ResponseCodeEnum.ContractDeleted,
        /// <summary>
        /// the platform node is either disconnected or lagging behind.
        /// </summary>
        PlatformNotActive = Proto.ResponseCodeEnum.PlatformNotActive,
        /// <summary>
        /// one public key matches more than one prefixes on the signature map
        /// </summary>
        KeyPrefixMismatch = Proto.ResponseCodeEnum.KeyPrefixMismatch,
        /// <summary>
        /// transaction not created by platform due to large backlog
        /// </summary>
        PlatformTransactionNotCreated = Proto.ResponseCodeEnum.PlatformTransactionNotCreated,
        /// <summary>
        /// auto renewal period is not a positive number of seconds
        /// </summary>
        InvalidRenewalPeriod = Proto.ResponseCodeEnum.InvalidRenewalPeriod,
        /// <summary>
        /// the response code when a smart contract id is passed for a crypto API request
        /// </summary>
        InvalidPayerAccountId = Proto.ResponseCodeEnum.InvalidPayerAccountId,
        /// <summary>
        /// the account has been marked as deleted
        /// </summary>
        AccountDeleted = Proto.ResponseCodeEnum.AccountDeleted,
        /// <summary>
        /// the file has been marked as deleted
        /// </summary>
        FileDeleted = Proto.ResponseCodeEnum.FileDeleted,
        /// <summary>
        /// same accounts repeated in the transfer account list
        /// </summary>
        AccountRepeatedInAccountAmounts = Proto.ResponseCodeEnum.AccountRepeatedInAccountAmounts,
        /// <summary>
        /// attempting to set negative balance value for crypto account
        /// </summary>
        SettingNegativeAccountBalance = Proto.ResponseCodeEnum.SettingNegativeAccountBalance,
        /// <summary>
        /// when deleting smart contract that has crypto balance either transfer account or transfer smart
        /// contract is required
        /// </summary>
        ObtainerRequired = Proto.ResponseCodeEnum.ObtainerRequired,
        /// <summary>
        /// when deleting smart contract that has crypto balance you can not use the same contract id as
        /// transferContractId as the one being deleted
        /// </summary>
        ObtainerSameContractId = Proto.ResponseCodeEnum.ObtainerSameContractId,
        /// <summary>
        /// transferAccountId or transferContractId specified for contract delete does not exist
        /// </summary>
        ObtainerDoesNotExist = Proto.ResponseCodeEnum.ObtainerDoesNotExist,
        /// <summary>
        /// attempting to modify (update or delete a immutable smart Contract = Proto.ResponseCodeEnum.Contract, i.e. one created without a
        /// admin key)
        /// </summary>
        ModifyingImmutableContract = Proto.ResponseCodeEnum.ModifyingImmutableContract,
        /// <summary>
        /// Unexpected exception thrown by file system functions
        /// </summary>
        FileSystemException = Proto.ResponseCodeEnum.FileSystemException,
        /// <summary>
        /// the duration is not a subset of [MinimumAutorenewDuration = Proto.ResponseCodeEnum.MinimumAutorenewDuration,MAXIMUMAUTORENEWDURATION]
        /// </summary>
        AutorenewDurationNotInRange = Proto.ResponseCodeEnum.AutorenewDurationNotInRange,
        /// <summary>
        /// Decoding the smart contract binary to a byte array failed. Check that the input is a valid hex
        /// string.
        /// </summary>
        ErrorDecodingBytestring = Proto.ResponseCodeEnum.ErrorDecodingBytestring,
        /// <summary>
        /// File to create a smart contract was of length zero
        /// </summary>
        ContractFileEmpty = Proto.ResponseCodeEnum.ContractFileEmpty,
        /// <summary>
        /// Bytecode for smart contract is of length zero
        /// </summary>
        ContractBytecodeEmpty = Proto.ResponseCodeEnum.ContractBytecodeEmpty,
        /// <summary>
        /// Attempt to set negative initial balance
        /// </summary>
        InvalidInitialBalance = Proto.ResponseCodeEnum.InvalidInitialBalance,
        /// <summary>
        /// Attempt to set negative receive record threshold
        /// </summary>
        [Obsolete]
        InvalidReceiveRecordThreshold = Proto.ResponseCodeEnum.InvalidReceiveRecordThreshold,
		/// <summary>
		/// Attempt to set negative send record threshold
		/// </summary>
		[Obsolete]
		InvalidSendRecordThreshold = Proto.ResponseCodeEnum.InvalidSendRecordThreshold,
        /// <summary>
        /// Special Account Operations should be performed by only Genesis Account = Proto.ResponseCodeEnum.Account, return this code if it
        /// is not Genesis Account
        /// </summary>
        AccountIsNotGenesisAccount = Proto.ResponseCodeEnum.AccountIsNotGenesisAccount,
        /// <summary>
        /// The fee payer account doesn't have permission to submit such Transaction
        /// </summary>
        PayerAccountUnauthorized = Proto.ResponseCodeEnum.PayerAccountUnauthorized,
        /// <summary>
        /// FreezeTransactionBody is invalid
        /// </summary>
        InvalidFreezeTransactionBody = Proto.ResponseCodeEnum.InvalidFreezeTransactionBody,
        /// <summary>
        /// FreezeTransactionBody does not exist
        /// </summary>
        FreezeTransactionBodyNotFound = Proto.ResponseCodeEnum.FreezeTransactionBodyNotFound,
        /// <summary>
        /// Exceeded the number of accounts (both from and to) allowed for crypto transfer list
        /// </summary>
        TransferListSizeLimitExceeded = Proto.ResponseCodeEnum.TransferListSizeLimitExceeded,
        /// <summary>
        /// Smart contract result size greater than specified maxResultSize
        /// </summary>
        ResultSizeLimitExceeded = Proto.ResponseCodeEnum.ResultSizeLimitExceeded,
        /// <summary>
        /// The payer account is not a special account(account 0.0.55)
        /// </summary>
        NotSpecialAccount = Proto.ResponseCodeEnum.NotSpecialAccount,
        /// <summary>
        /// Negative gas was offered in smart contract call
        /// </summary>
        ContractNegativeGas = Proto.ResponseCodeEnum.ContractNegativeGas,
        /// <summary>
        /// Negative value / initial balance was specified in a smart contract call / create
        /// </summary>
        ContractNegativeValue = Proto.ResponseCodeEnum.ContractNegativeValue,
        /// <summary>
        /// Failed to update fee file
        /// </summary>
        InvalidFeeFile = Proto.ResponseCodeEnum.InvalidFeeFile,
        /// <summary>
        /// Failed to update exchange rate file
        /// </summary>
        InvalidExchangeRateFile = Proto.ResponseCodeEnum.InvalidExchangeRateFile,
        /// <summary>
        /// Payment tendered for contract local call cannot cover both the fee and the gas
        /// </summary>
        InsufficientLocalCallGas = Proto.ResponseCodeEnum.InsufficientLocalCallGas,
        /// <summary>
        /// Entities with Entity ID below 1000 are not allowed to be deleted
        /// </summary>
        EntityNotAllowedToDelete = Proto.ResponseCodeEnum.EntityNotAllowedToDelete,
        /// <summary>
        /// Violating one of these rules: 1) treasury account can update all entities below 0.0.1000 = Proto.ResponseCodeEnum.1000, 2)
        /// account 0.0.50 can update all entities from 0.0.51 - 0.0.80 = Proto.ResponseCodeEnum.80, 3) Network Function Master Account
        /// A/c 0.0.50 - Update all Network Function accounts and perform all the Network Functions listed
        /// Below = Proto.ResponseCodeEnum.Below, 4) Network Function Accounts: i) A/c 0.0.55 - Update Address Book files (0.0.101/102) = Proto.ResponseCodeEnum.),
        /// ii) A/c 0.0.56 - Update Fee schedule (0.0.111) = Proto.ResponseCodeEnum.), iii) A/c 0.0.57 - Update Exchange Rate
        /// (0.0.112).
        /// </summary>
        AuthorizationFailed = Proto.ResponseCodeEnum.AuthorizationFailed,
        /// <summary>
        /// Fee Schedule Proto uploaded but not valid (append or update is required)
        /// </summary>
        FileUploadedProtoInvalid = Proto.ResponseCodeEnum.FileUploadedProtoInvalid,
        /// <summary>
        /// Fee Schedule Proto uploaded but not valid (append or update is required)
        /// </summary>
        FileUploadedProtoNotSavedToDisk = Proto.ResponseCodeEnum.FileUploadedProtoNotSavedToDisk,
        /// <summary>
        /// Fee Schedule Proto File Part uploaded
        /// </summary>
        FeeScheduleFilePartUploaded = Proto.ResponseCodeEnum.FeeScheduleFilePartUploaded,
        /// <summary>
        /// The change on Exchange Rate exceeds ExchangeRateAllowedPercentage
        /// </summary>
        ExchangeRateChangeLimitExceeded = Proto.ResponseCodeEnum.ExchangeRateChangeLimitExceeded,
        /// <summary>
        /// Contract permanent storage exceeded the currently allowable limit
        /// </summary>
        MaxContractStorageExceeded = Proto.ResponseCodeEnum.MaxContractStorageExceeded,
        /// <summary>
        /// Transfer Account should not be same as Account to be deleted
        /// </summary>
        TotalLedgerBalanceInvalid = Proto.ResponseCodeEnum.TotalLedgerBalanceInvalid,
        /// <summary>
        /// The expiration date/time on a smart contract may not be reduced
        /// </summary>
        ExpirationReductionNotAllowed = Proto.ResponseCodeEnum.ExpirationReductionNotAllowed,
        /// <summary>
        /// Gas exceeded currently allowable gas limit per transaction
        /// </summary>
        MaxGasLimitExceeded = Proto.ResponseCodeEnum.MaxGasLimitExceeded,
        /// <summary>
        /// File size exceeded the currently allowable limit
        /// </summary>
        MaxFileSizeExceeded = Proto.ResponseCodeEnum.MaxFileSizeExceeded,
        /// <summary>
        /// When a valid signature is not provided for operations on account with receiverSigRequired=true
        /// </summary>
        ReceiverSigRequired = Proto.ResponseCodeEnum.ReceiverSigRequired,
        /// <summary>
        /// The Topic ID specified is not in the system.
        /// </summary>
        InvalidTopicId = Proto.ResponseCodeEnum.InvalidTopicId,
        /// <summary>
        /// A provided admin key was invalid. Verify the bytes for an Ed25519 public key are exactly 32 bytes; and the bytes for a compressed ECDSA(secp256k1) key are exactly 33 Bytes = Proto.ResponseCodeEnum.Bytes, with the first byte either 0x02 or 0x03..
        /// </summary>
        InvalidAdminKey = Proto.ResponseCodeEnum.InvalidAdminKey,
        /// <summary>
        /// A provided submit key was invalid.
        /// </summary>
        InvalidSubmitKey = Proto.ResponseCodeEnum.InvalidSubmitKey,
        /// <summary>
        /// An attempted operation was not authorized (ie - a deleteTopic for a topic with no adminKey).
        /// </summary>
        Unauthorized = Proto.ResponseCodeEnum.Unauthorized,
        /// <summary>
        /// A ConsensusService message is empty.
        /// </summary>
        InvalidTopicMessage = Proto.ResponseCodeEnum.InvalidTopicMessage,
        /// <summary>
        /// The autoRenewAccount specified is not a Valid = Proto.ResponseCodeEnum.Valid, active account.
        /// </summary>
        InvalidAutorenewAccount = Proto.ResponseCodeEnum.InvalidAutorenewAccount,
        /// <summary>
        /// An adminKey was not specified on the Topic = Proto.ResponseCodeEnum.Topic, so there must not be an autoRenewAccount.
        /// </summary>
        AutorenewAccountNotAllowed = Proto.ResponseCodeEnum.AutorenewAccountNotAllowed,
        /// <summary>
        /// The topic has Expired = Proto.ResponseCodeEnum.Expired, was not automatically Renewed = Proto.ResponseCodeEnum.Renewed, and is in a 7 day grace period before the
        /// topic will be deleted unrecoverably. This error response code will not be returned until
        /// autoRenew functionality is supported by HAPI.
        /// </summary>
        TopicExpired = Proto.ResponseCodeEnum.TopicExpired,
        /// <summary>
        /// chunk number must be from 1 to total (chunks) inclusive.
        /// </summary>
        InvalidChunkNumber = Proto.ResponseCodeEnum.InvalidChunkNumber,
        /// <summary>
        /// For every Chunk = Proto.ResponseCodeEnum.Chunk, the payer account that is part of initialTransactionID must match the Payer Account of this transaction. The entire initialTransactionID should match the transactionID of the first Chunk = Proto.ResponseCodeEnum.Chunk, but this is not checked or enforced by Hedera except when the chunk number is 1.
        /// </summary>
        InvalidChunkTransactionId = Proto.ResponseCodeEnum.InvalidChunkTransactionId,
        /// <summary>
        /// Account is frozen and cannot transact with the token
        /// </summary>
        AccountFrozenForToken = Proto.ResponseCodeEnum.AccountFrozenForToken,
        /// <summary>
        /// An involved account already has more than tokens.maxPerAccount associations with non-deleted tokens.
        /// </summary>
        TokensPerAccountLimitExceeded = Proto.ResponseCodeEnum.TokensPerAccountLimitExceeded,
        /// <summary>
        /// The token is invalid or does not exist
        /// </summary>
        InvalidTokenId = Proto.ResponseCodeEnum.InvalidTokenId,
        /// <summary>
        /// Invalid token decimals
        /// </summary>
        InvalidTokenDecimals = Proto.ResponseCodeEnum.InvalidTokenDecimals,
        /// <summary>
        /// Invalid token initial supply
        /// </summary>
        InvalidTokenInitialSupply = Proto.ResponseCodeEnum.InvalidTokenInitialSupply,
        /// <summary>
        /// Treasury Account does not exist or is deleted
        /// </summary>
        InvalidTreasuryAccountForToken = Proto.ResponseCodeEnum.InvalidTreasuryAccountForToken,
        /// <summary>
        /// Token Symbol is not UTF-8 capitalized alphabetical string
        /// </summary>
        InvalidTokenSymbol = Proto.ResponseCodeEnum.InvalidTokenSymbol,
        /// <summary>
        /// Freeze key is not set on token
        /// </summary>
        TokenHasNoFreezeKey = Proto.ResponseCodeEnum.TokenHasNoFreezeKey,
        /// <summary>
        /// Amounts in transfer list are not net zero
        /// </summary>
        TransfersNotZeroSumForToken = Proto.ResponseCodeEnum.TransfersNotZeroSumForToken,
        /// <summary>
        /// A token symbol was not provided
        /// </summary>
        MissingTokenSymbol = Proto.ResponseCodeEnum.MissingTokenSymbol,
        /// <summary>
        /// The provided token symbol was too long
        /// </summary>
        TokenSymbolTooLong = Proto.ResponseCodeEnum.TokenSymbolTooLong,
        /// <summary>
        /// KYC must be granted and account does not have KYC granted
        /// </summary>
        AccountKycNotGrantedForToken = Proto.ResponseCodeEnum.AccountKycNotGrantedForToken,
        /// <summary>
        /// KYC key is not set on token
        /// </summary>
        TokenHasNoKycKey = Proto.ResponseCodeEnum.TokenHasNoKycKey,
        /// <summary>
        /// Token balance is not sufficient for the transaction
        /// </summary>
        InsufficientTokenBalance = Proto.ResponseCodeEnum.InsufficientTokenBalance,
        /// <summary>
        /// Token transactions cannot be executed on deleted token
        /// </summary>
        TokenWasDeleted = Proto.ResponseCodeEnum.TokenWasDeleted,
        /// <summary>
        /// Supply key is not set on token
        /// </summary>
        TokenHasNoSupplyKey = Proto.ResponseCodeEnum.TokenHasNoSupplyKey,
        /// <summary>
        /// Wipe key is not set on token
        /// </summary>
        TokenHasNoWipeKey = Proto.ResponseCodeEnum.TokenHasNoWipeKey,
        /// <summary>
        /// The requested token mint amount would cause an invalid total supply
        /// </summary>
        InvalidTokenMintAmount = Proto.ResponseCodeEnum.InvalidTokenMintAmount,
        /// <summary>
        /// The requested token burn amount would cause an invalid total supply
        /// </summary>
        InvalidTokenBurnAmount = Proto.ResponseCodeEnum.InvalidTokenBurnAmount,
        /// <summary>
        /// A required token-account relationship is missing
        /// </summary>
        TokenNotAssociatedToAccount = Proto.ResponseCodeEnum.TokenNotAssociatedToAccount,
        /// <summary>
        /// The target of a wipe operation was the token treasury account
        /// </summary>
        CannotWipeTokenTreasuryAccount = Proto.ResponseCodeEnum.CannotWipeTokenTreasuryAccount,
        /// <summary>
        /// The provided KYC key was invalid.
        /// </summary>
        InvalidKycKey = Proto.ResponseCodeEnum.InvalidKycKey,
        /// <summary>
        /// The provided wipe key was invalid.
        /// </summary>
        InvalidWipeKey = Proto.ResponseCodeEnum.InvalidWipeKey,
        /// <summary>
        /// The provided freeze key was invalid.
        /// </summary>
        InvalidFreezeKey = Proto.ResponseCodeEnum.InvalidFreezeKey,
        /// <summary>
        /// The provided supply key was invalid.
        /// </summary>
        InvalidSupplyKey = Proto.ResponseCodeEnum.InvalidSupplyKey,
        /// <summary>
        /// Token Name is not provided
        /// </summary>
        MissingTokenName = Proto.ResponseCodeEnum.MissingTokenName,
        /// <summary>
        /// Token Name is too long
        /// </summary>
        TokenNameTooLong = Proto.ResponseCodeEnum.TokenNameTooLong,
        /// <summary>
        /// The provided wipe amount must not be Negative = Proto.ResponseCodeEnum.Negative, zero or bigger than the token holder balance
        /// </summary>
        InvalidWipingAmount = Proto.ResponseCodeEnum.InvalidWipingAmount,
        /// <summary>
        /// Token does not have Admin key Set = Proto.ResponseCodeEnum.Set, thus update/delete transactions cannot be performed
        /// </summary>
        TokenIsImmutable = Proto.ResponseCodeEnum.TokenIsImmutable,
        /// <summary>
        /// An associateToken operation specified a token already associated to the account
        /// </summary>
        TokenAlreadyAssociatedToAccount = Proto.ResponseCodeEnum.TokenAlreadyAssociatedToAccount,
        /// <summary>
        /// An attempted operation is invalid until all token balances for the target account are zero
        /// </summary>
        TransactionRequiresZeroTokenBalances = Proto.ResponseCodeEnum.TransactionRequiresZeroTokenBalances,
        /// <summary>
        /// An attempted operation is invalid because the account is a treasury
        /// </summary>
        AccountIsTreasury = Proto.ResponseCodeEnum.AccountIsTreasury,
        /// <summary>
        /// Same TokenIDs present in the token list
        /// </summary>
        TokenIdRepeatedInTokenList = Proto.ResponseCodeEnum.TokenIdRepeatedInTokenList,
        /// <summary>
        /// Exceeded the number of token transfers (both from and to) allowed for token transfer list
        /// </summary>
        TokenTransferListSizeLimitExceeded = Proto.ResponseCodeEnum.TokenTransferListSizeLimitExceeded,
        /// <summary>
        /// TokenTransfersTransactionBody has no TokenTransferList
        /// </summary>
        EmptyTokenTransferBody = Proto.ResponseCodeEnum.EmptyTokenTransferBody,
        /// <summary>
        /// TokenTransfersTransactionBody has a TokenTransferList with no AccountAmounts
        /// </summary>
        EmptyTokenTransferAccountAmounts = Proto.ResponseCodeEnum.EmptyTokenTransferAccountAmounts,
        /// <summary>
        /// The Scheduled entity does not exist; or has now Expired = Proto.ResponseCodeEnum.Expired, been Deleted = Proto.ResponseCodeEnum.Deleted, or been executed
        /// </summary>
        InvalidScheduleId = Proto.ResponseCodeEnum.InvalidScheduleId,
        /// <summary>
        /// The Scheduled entity cannot be modified. Admin key not set
        /// </summary>
        ScheduleIsImmutable = Proto.ResponseCodeEnum.ScheduleIsImmutable,
        /// <summary>
        /// The provided Scheduled Payer does not exist
        /// </summary>
        InvalidSchedulePayerId = Proto.ResponseCodeEnum.InvalidSchedulePayerId,
        /// <summary>
        /// The Schedule Create Transaction TransactionID account does not exist
        /// </summary>
        InvalidScheduleAccountId = Proto.ResponseCodeEnum.InvalidScheduleAccountId,
        /// <summary>
        /// The provided sig map did not contain any new valid signatures from required signers of the scheduled transaction
        /// </summary>
        NoNewValidSignatures = Proto.ResponseCodeEnum.NoNewValidSignatures,
        /// <summary>
        /// The required signers for a scheduled transaction cannot be Resolved = Proto.ResponseCodeEnum.Resolved, for example because they do not exist or have been deleted
        /// </summary>
        UnresolvableRequiredSigners = Proto.ResponseCodeEnum.UnresolvableRequiredSigners,
        /// <summary>
        /// Only whitelisted transaction types may be scheduled
        /// </summary>
        ScheduledTransactionNotInWhitelist = Proto.ResponseCodeEnum.ScheduledTransactionNotInWhitelist,
        /// <summary>
        /// At least one of the signatures in the provided sig map did not represent a valid signature for any required signer
        /// </summary>
        SomeSignaturesWereInvalid = Proto.ResponseCodeEnum.SomeSignaturesWereInvalid,
        /// <summary>
        /// The scheduled field in the TransactionID may not be set to true
        /// </summary>
        TransactionIdFieldNotAllowed = Proto.ResponseCodeEnum.TransactionIdFieldNotAllowed,
        /// <summary>
        /// A schedule already exists with the same identifying fields of an attempted ScheduleCreate (that Is = Proto.ResponseCodeEnum.Is, all fields other than scheduledPayerAccountID)
        /// </summary>
        IdenticalScheduleAlreadyCreated = Proto.ResponseCodeEnum.IdenticalScheduleAlreadyCreated,
        /// <summary>
        /// A string field in the transaction has a UTF-8 encoding with the prohibited zero byte
        /// </summary>
        InvalidZeroByteInString = Proto.ResponseCodeEnum.InvalidZeroByteInString,
        /// <summary>
        /// A schedule being signed or deleted has already been deleted
        /// </summary>
        ScheduleAlreadyDeleted = Proto.ResponseCodeEnum.ScheduleAlreadyDeleted,
        /// <summary>
        /// A schedule being signed or deleted has already been executed
        /// </summary>
        ScheduleAlreadyExecuted = Proto.ResponseCodeEnum.ScheduleAlreadyExecuted,
        /// <summary>
        /// ConsensusSubmitMessage request's message size is larger than allowed.
        /// </summary>
        MessageSizeTooLarge = Proto.ResponseCodeEnum.MessageSizeTooLarge,
        /// <summary>
        /// An operation was assigned to more than one throttle group in a given bucket
        /// </summary>
        OperationRepeatedInBucketGroups = Proto.ResponseCodeEnum.OperationRepeatedInBucketGroups,
        /// <summary>
        /// The capacity needed to satisfy all opsPerSec groups in a bucket overflowed a signed 8-byte integral type
        /// </summary>
        BucketCapacityOverflow = Proto.ResponseCodeEnum.BucketCapacityOverflow,
        /// <summary>
        /// Given the network size in the address Book = Proto.ResponseCodeEnum.Book, the node-level capacity for an operation would never be enough to accept a single request; usually means a bucket burstPeriod should be increased
        /// </summary>
        NodeCapacityNotSufficientForOperation = Proto.ResponseCodeEnum.NodeCapacityNotSufficientForOperation,
        /// <summary>
        /// A bucket was defined without any throttle groups
        /// </summary>
        BucketHasNoThrottleGroups = Proto.ResponseCodeEnum.BucketHasNoThrottleGroups,
        /// <summary>
        /// A throttle group was granted zero opsPerSec
        /// </summary>
        ThrottleGroupHasZeroOpsPerSec = Proto.ResponseCodeEnum.ThrottleGroupHasZeroOpsPerSec,
        /// <summary>
        /// The throttle definitions file was Updated = Proto.ResponseCodeEnum.Updated, but some supported operations were not assigned a bucket
        /// </summary>
        SuccessButMissingExpectedOperation = Proto.ResponseCodeEnum.SuccessButMissingExpectedOperation,
        /// <summary>
        /// The new contents for the throttle definitions system file were not valid protobuf
        /// </summary>
        UnparseableThrottleDefinitions = Proto.ResponseCodeEnum.UnparseableThrottleDefinitions,
        /// <summary>
        /// The new throttle definitions system file were Invalid = Proto.ResponseCodeEnum.Invalid, and no more specific error could be divined
        /// </summary>
        InvalidThrottleDefinitions = Proto.ResponseCodeEnum.InvalidThrottleDefinitions,
        /// <summary>
        /// The transaction references an account which has passed its expiration without renewal funds Available = Proto.ResponseCodeEnum.Available, and currently remains in the ledger only because of the grace period given to expired entities
        /// </summary>
        AccountExpiredAndPendingRemoval = Proto.ResponseCodeEnum.AccountExpiredAndPendingRemoval,
        /// <summary>
        /// Invalid token max supply
        /// </summary>
        InvalidTokenMaxSupply = Proto.ResponseCodeEnum.InvalidTokenMaxSupply,
        /// <summary>
        /// Invalid token nft serial number
        /// </summary>
        InvalidTokenNftSerialNumber = Proto.ResponseCodeEnum.InvalidTokenNftSerialNumber,
        /// <summary>
        /// Invalid nft id
        /// </summary>
        InvalidNftId = Proto.ResponseCodeEnum.InvalidNftId,
        /// <summary>
        /// Nft metadata is too long
        /// </summary>
        MetadataTooLong = Proto.ResponseCodeEnum.MetadataTooLong,
        /// <summary>
        /// Repeated operations count exceeds the limit
        /// </summary>
        BatchSizeLimitExceeded = Proto.ResponseCodeEnum.BatchSizeLimitExceeded,
        /// <summary>
        /// The range of data to be gathered is out of the set boundaries
        /// </summary>
        InvalidQueryRange = Proto.ResponseCodeEnum.InvalidQueryRange,
        /// <summary>
        /// A custom fractional fee set a denominator of zero
        /// </summary>
        FractionDividesByZero = Proto.ResponseCodeEnum.FractionDividesByZero,
		/// <summary>
		/// The transaction payer could not afford a custom fee
		/// </summary>
		[Obsolete]
		InsufficientPayerBalanceForCustomFee = Proto.ResponseCodeEnum.InsufficientPayerBalanceForCustomFee,
        /// <summary>
        /// More than 10 custom fees were specified
        /// </summary>
        CustomFeesListTooLong = Proto.ResponseCodeEnum.CustomFeesListTooLong,
        /// <summary>
        /// Any of the feeCollector accounts for customFees is invalid
        /// </summary>
        InvalidCustomFeeCollector = Proto.ResponseCodeEnum.InvalidCustomFeeCollector,
        /// <summary>
        /// Any of the token Ids in customFees is invalid
        /// </summary>
        InvalidTokenIdInCustomFees = Proto.ResponseCodeEnum.InvalidTokenIdInCustomFees,
        /// <summary>
        /// Any of the token Ids in customFees are not associated to feeCollector
        /// </summary>
        TokenNotAssociatedToFeeCollector = Proto.ResponseCodeEnum.TokenNotAssociatedToFeeCollector,
        /// <summary>
        /// A token cannot have more units minted due to its configured supply ceiling
        /// </summary>
        TokenMaxSupplyReached = Proto.ResponseCodeEnum.TokenMaxSupplyReached,
        /// <summary>
        /// The transaction attempted to move an NFT serial number from an account other than its owner
        /// </summary>
        SenderDoesNotOwnNftSerialNo = Proto.ResponseCodeEnum.SenderDoesNotOwnNftSerialNo,
        /// <summary>
        /// A custom fee schedule entry did not specify either a fixed or fractional fee
        /// </summary>
        CustomFeeNotFullySpecified = Proto.ResponseCodeEnum.CustomFeeNotFullySpecified,
        /// <summary>
        /// Only positive fees may be assessed at this time
        /// </summary>
        CustomFeeMustBePositive = Proto.ResponseCodeEnum.CustomFeeMustBePositive,
        /// <summary>
        /// Fee schedule key is not set on token
        /// </summary>
        TokenHasNoFeeScheduleKey = Proto.ResponseCodeEnum.TokenHasNoFeeScheduleKey,
        /// <summary>
        /// A fractional custom fee exceeded the range of a 64-bit signed integer
        /// </summary>
        CustomFeeOutsideNumericRange = Proto.ResponseCodeEnum.CustomFeeOutsideNumericRange,
        /// <summary>
        /// A royalty cannot exceed the total fungible value exchanged for an NFT
        /// </summary>
        RoyaltyFractionCannotExceedOne = Proto.ResponseCodeEnum.RoyaltyFractionCannotExceedOne,
        /// <summary>
        /// Each fractional custom fee must have its MaximumAmount = Proto.ResponseCodeEnum.MaximumAmount, if Specified = Proto.ResponseCodeEnum.Specified, at least its minimumAmount
        /// </summary>
        FractionalFeeMaxAmountLessThanMinAmount = Proto.ResponseCodeEnum.FractionalFeeMaxAmountLessThanMinAmount,
        /// <summary>
        /// A fee schedule update tried to clear the custom fees from a token whose fee schedule was already empty
        /// </summary>
        CustomScheduleAlreadyHasNoFees = Proto.ResponseCodeEnum.CustomScheduleAlreadyHasNoFees,
        /// <summary>
        /// Only tokens of type FUNGIBLECOMMON can be used to as fee schedule denominations
        /// </summary>
        CustomFeeDenominationMustBeFungibleCommon = Proto.ResponseCodeEnum.CustomFeeDenominationMustBeFungibleCommon,
        /// <summary>
        /// Only tokens of type FUNGIBLECOMMON can have fractional fees
        /// </summary>
        CustomFractionalFeeOnlyAllowedForFungibleCommon = Proto.ResponseCodeEnum.CustomFractionalFeeOnlyAllowedForFungibleCommon,
        /// <summary>
        /// The provided custom fee schedule key was invalid
        /// </summary>
        InvalidCustomFeeScheduleKey = Proto.ResponseCodeEnum.InvalidCustomFeeScheduleKey,
        /// <summary>
        /// The requested token mint metadata was invalid
        /// </summary>
        InvalidTokenMintMetadata = Proto.ResponseCodeEnum.InvalidTokenMintMetadata,
        /// <summary>
        /// The requested token burn metadata was invalid
        /// </summary>
        InvalidTokenBurnMetadata = Proto.ResponseCodeEnum.InvalidTokenBurnMetadata,
        /// <summary>
        /// The treasury for a unique token cannot be changed until it owns no NFTs
        /// </summary>
        CurrentTreasuryStillOwnsNfts = Proto.ResponseCodeEnum.CurrentTreasuryStillOwnsNfts,
        /// <summary>
        /// An account cannot be dissociated from a unique token if it owns NFTs for the token
        /// </summary>
        AccountStillOwnsNfts = Proto.ResponseCodeEnum.AccountStillOwnsNfts,
        /// <summary>
        /// A NFT can only be burned when owned by the unique token's treasury
        /// </summary>
        TreasuryMustOwnBurnedNft = Proto.ResponseCodeEnum.TreasuryMustOwnBurnedNft,
        /// <summary>
        /// An account did not own the NFT to be wiped
        /// </summary>
        AccountDoesNotOwnWipedNft = Proto.ResponseCodeEnum.AccountDoesNotOwnWipedNft,
        /// <summary>
        /// An AccountAmount token transfers list referenced a token type other than FUNGIBLECOMMON
        /// </summary>
        AccountAmountTransfersOnlyAllowedForFungibleCommon = Proto.ResponseCodeEnum.AccountAmountTransfersOnlyAllowedForFungibleCommon,
        /// <summary>
        /// All the NFTs allowed in the current price regime have already been minted
        /// </summary>
        MaxNftsInPriceRegimeHaveBeenMinted = Proto.ResponseCodeEnum.MaxNftsInPriceRegimeHaveBeenMinted,
        /// <summary>
        /// The payer account has been marked as deleted
        /// </summary>
        PayerAccountDeleted = Proto.ResponseCodeEnum.PayerAccountDeleted,
        /// <summary>
        /// The reference chain of custom fees for a transferred token exceeded the maximum length of 2
        /// </summary>
        CustomFeeChargingExceededMaxRecursionDepth = Proto.ResponseCodeEnum.CustomFeeChargingExceededMaxRecursionDepth,
        /// <summary>
        /// More than 20 balance adjustments were to satisfy a CryptoTransfer and its implied custom fee payments
        /// </summary>
        CustomFeeChargingExceededMaxAccountAmounts = Proto.ResponseCodeEnum.CustomFeeChargingExceededMaxAccountAmounts,
        /// <summary>
        /// The sender account in the token transfer transaction could not afford a custom fee
        /// </summary>
        InsufficientSenderAccountBalanceForCustomFee = Proto.ResponseCodeEnum.InsufficientSenderAccountBalanceForCustomFee,
        /// <summary>
        /// Currently no more than 4 = Proto.ResponseCodeEnum.4,294 = Proto.ResponseCodeEnum.294,967 = Proto.ResponseCodeEnum.967,295 NFTs may be minted for a given unique token type
        /// </summary>
        SerialNumberLimitReached = Proto.ResponseCodeEnum.SerialNumberLimitReached,
        /// <summary>
        /// Only tokens of type NONFUNGIBLEUNIQUE can have royalty fees
        /// </summary>
        CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique = Proto.ResponseCodeEnum.CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique,
        /// <summary>
        /// The account has reached the limit on the automatic associations count.
        /// </summary>
        NoRemainingAutomaticAssociations = Proto.ResponseCodeEnum.NoRemainingAutomaticAssociations,
        /// <summary>
        /// Already existing automatic associations are more than the new maximum automatic associations.
        /// </summary>
        ExistingAutomaticAssociationsExceedGivenLimit = Proto.ResponseCodeEnum.ExistingAutomaticAssociationsExceedGivenLimit,
        /// <summary>
        /// Cannot set the number of automatic associations for an account more than the maximum allowed
        /// token associations tokens.maxPerAccount.
        /// </summary>
        RequestedNumAutomaticAssociationsExceedsAssociationLimit = Proto.ResponseCodeEnum.RequestedNumAutomaticAssociationsExceedsAssociationLimit,
        /// <summary>
        /// Token is paused. This Token cannot be a part of any kind of Transaction until unpaused.
        /// </summary>
        TokenIsPaused = Proto.ResponseCodeEnum.TokenIsPaused,
        /// <summary>
        /// Pause key is not set on token
        /// </summary>
        TokenHasNoPauseKey = Proto.ResponseCodeEnum.TokenHasNoPauseKey,
        /// <summary>
        /// The provided pause key was invalid
        /// </summary>
        InvalidPauseKey = Proto.ResponseCodeEnum.InvalidPauseKey,
        /// <summary>
        /// The update file in a freeze transaction body must exist.
        /// </summary>
        FreezeUpdateFileDoesNotExist = Proto.ResponseCodeEnum.FreezeUpdateFileDoesNotExist,
        /// <summary>
        /// The hash of the update file in a freeze transaction body must match the in-memory hash.
        /// </summary>
        FreezeUpdateFileHashDoesNotMatch = Proto.ResponseCodeEnum.FreezeUpdateFileHashDoesNotMatch,
        /// <summary>
        /// A FREEZEUPGRADE transaction was handled with no previous update prepared.
        /// </summary>
        NoUpgradeHasBeenPrepared = Proto.ResponseCodeEnum.NoUpgradeHasBeenPrepared,
        /// <summary>
        /// A FREEZEABORT transaction was handled with no scheduled freeze.
        /// </summary>
        NoFreezeIsScheduled = Proto.ResponseCodeEnum.NoFreezeIsScheduled,
        /// <summary>
        /// The update file hash when handling a FREEZEUPGRADE transaction differs from the file
        /// hash at the time of handling the PREPAREUPGRADE transaction.
        /// </summary>
        UpdateFileHashChangedSincePrepareUpgrade = Proto.ResponseCodeEnum.UpdateFileHashChangedSincePrepareUpgrade,
        /// <summary>
        /// The given freeze start time was in the (consensus) past.
        /// </summary>
        FreezeStartTimeMustBeFuture = Proto.ResponseCodeEnum.FreezeStartTimeMustBeFuture,
        /// <summary>
        /// The prepared update file cannot be updated or appended until either the upgrade has
        /// been Completed = Proto.ResponseCodeEnum.Completed, or a FREEZEABORT has been handled.
        /// </summary>
        PreparedUpdateFileIsImmutable = Proto.ResponseCodeEnum.PreparedUpdateFileIsImmutable,
        /// <summary>
        /// Once a freeze is Scheduled = Proto.ResponseCodeEnum.Scheduled, it must be aborted before any other type of freeze can
        /// can be performed.
        /// </summary>
        FreezeAlreadyScheduled = Proto.ResponseCodeEnum.FreezeAlreadyScheduled,
        /// <summary>
        /// If an NMT upgrade has been Prepared = Proto.ResponseCodeEnum.Prepared, the following operation must be a FREEZEUPGRADE.
        /// (To issue a FreezeOnly = Proto.ResponseCodeEnum.FreezeOnly, submit a FREEZEABORT first.)
        /// </summary>
        FreezeUpgradeInProgress = Proto.ResponseCodeEnum.FreezeUpgradeInProgress,
        /// <summary>
        /// If an NMT upgrade has been Prepared = Proto.ResponseCodeEnum.Prepared, the subsequent FREEZEUPGRADE transaction must
        /// confirm the id of the file to be used in the upgrade.
        /// </summary>
        UpdateFileIdDoesNotMatchPrepared = Proto.ResponseCodeEnum.UpdateFileIdDoesNotMatchPrepared,
        /// <summary>
        /// If an NMT upgrade has been Prepared = Proto.ResponseCodeEnum.Prepared, the subsequent FREEZEUPGRADE transaction must
        /// confirm the hash of the file to be used in the upgrade.
        /// </summary>
        UpdateFileHashDoesNotMatchPrepared = Proto.ResponseCodeEnum.UpdateFileHashDoesNotMatchPrepared,
        /// <summary>
        /// Consensus throttle did not allow execution of this transaction. System is throttled at
        /// consensus level.
        /// </summary>
        ConsensusGasExhausted = Proto.ResponseCodeEnum.ConsensusGasExhausted,
        /// <summary>
        /// A precompiled contract Succeeded = Proto.ResponseCodeEnum.Succeeded, but was later reverted.
        /// </summary>
        RevertedSuccess = Proto.ResponseCodeEnum.RevertedSuccess,
        /// <summary>
        /// All contract storage allocated to the current price regime has been consumed.
        /// </summary>
        MaxStorageInPriceRegimeHasBeenUsed = Proto.ResponseCodeEnum.MaxStorageInPriceRegimeHasBeenUsed,
        /// <summary>
        /// An alias used in a CryptoTransfer transaction is not the serialization of a primitive Key
        /// message--that Is = Proto.ResponseCodeEnum.Is, a Key with a single Ed25519 or ECDSA(secp256k1) public key and no
        /// unknown protobuf fields.
        /// </summary>
        InvalidAliasKey = Proto.ResponseCodeEnum.InvalidAliasKey,
        /// <summary>
        /// A fungible token transfer expected a different number of decimals than the involved
        /// type actually has.
        /// </summary>
        UnexpectedTokenDecimals = Proto.ResponseCodeEnum.UnexpectedTokenDecimals,
		/// <summary>
		/// The proxy account id is invalid or does not exist.
		/// </summary>
		[Obsolete]
		InvalidProxyAccountId = Proto.ResponseCodeEnum.InvalidProxyAccountId,
        /// <summary>
        /// The transfer account id in CryptoDelete transaction is invalid or does not exist.
        /// </summary>
        InvalidTransferAccountId = Proto.ResponseCodeEnum.InvalidTransferAccountId,
        /// <summary>
        /// The fee collector account id in TokenFeeScheduleUpdate is invalid or does not exist.
        /// </summary>
        InvalidFeeCollectorAccountId = Proto.ResponseCodeEnum.InvalidFeeCollectorAccountId,
        /// <summary>
        /// The alias already set on an account cannot be updated using CryptoUpdate transaction.
        /// </summary>
        AliasIsImmutable = Proto.ResponseCodeEnum.AliasIsImmutable,
        /// <summary>
        /// An approved allowance specifies a spender account that is the same as the hbar/token
        /// owner account.
        /// </summary>
        SpenderAccountSameAsOwner = Proto.ResponseCodeEnum.SpenderAccountSameAsOwner,
        /// <summary>
        /// The establishment or adjustment of an approved allowance cause the token allowance
        /// to exceed the token maximum supply.
        /// </summary>
        AmountExceedsTokenMaxSupply = Proto.ResponseCodeEnum.AmountExceedsTokenMaxSupply,
        /// <summary>
        /// The specified amount for an approved allowance cannot be negative.
        /// </summary>
        NegativeAllowanceAmount = Proto.ResponseCodeEnum.NegativeAllowanceAmount,
		/// <summary>
		/// The approveForAll flag cannot be set for a fungible token.
		/// </summary>
		[Obsolete]
		CannotApproveForAllFungibleCommon = Proto.ResponseCodeEnum.CannotApproveForAllFungibleCommon,
        /// <summary>
        /// The spender does not have an existing approved allowance with the hbar/token owner.
        /// </summary>
        SpenderDoesNotHaveAllowance = Proto.ResponseCodeEnum.SpenderDoesNotHaveAllowance,
        /// <summary>
        /// The transfer amount exceeds the current approved allowance for the spender account.
        /// </summary>
        AmountExceedsAllowance = Proto.ResponseCodeEnum.AmountExceedsAllowance,
        /// <summary>
        /// The payer account of an approveAllowances or adjustAllowance transaction is attempting
        /// to go beyond the maximum allowed number of allowances.
        /// </summary>
        MaxAllowancesExceeded = Proto.ResponseCodeEnum.MaxAllowancesExceeded,
        /// <summary>
        /// No allowances have been specified in the approval transaction.
        /// </summary>
        EmptyAllowances = Proto.ResponseCodeEnum.EmptyAllowances,
		/// <summary>
		/// Spender is repeated more than once in Crypto or Token or NFT allowance lists in a single
		/// CryptoApproveAllowance transaction.
		/// </summary>
		[Obsolete]
		SpenderAccountRepeatedInAllowances = Proto.ResponseCodeEnum.SpenderAccountRepeatedInAllowances,
		/// <summary>
		/// Serial numbers are repeated in nft allowance for a single spender account
		/// </summary>
		[Obsolete]
		RepeatedSerialNumsInNftAllowances = Proto.ResponseCodeEnum.RepeatedSerialNumsInNftAllowances,
        /// <summary>
        /// Fungible common token used in NFT allowances
        /// </summary>
        FungibleTokenInNftAllowances = Proto.ResponseCodeEnum.FungibleTokenInNftAllowances,
        /// <summary>
        /// Non fungible token used in fungible token allowances
        /// </summary>
        NftInFungibleTokenAllowances = Proto.ResponseCodeEnum.NftInFungibleTokenAllowances,
        /// <summary>
        /// The account id specified as the owner is invalid or does not exist.
        /// </summary>
        InvalidAllowanceOwnerId = Proto.ResponseCodeEnum.InvalidAllowanceOwnerId,
        /// <summary>
        /// The account id specified as the spender is invalid or does not exist.
        /// </summary>
        InvalidAllowanceSpenderId = Proto.ResponseCodeEnum.InvalidAllowanceSpenderId,
		/// <summary>
		/// [Deprecated] If the CryptoDeleteAllowance transaction has repeated crypto or token or Nft allowances to delete.
		/// </summary>
		[Obsolete]
		RepeatedAllowancesToDelete = Proto.ResponseCodeEnum.RepeatedAllowancesToDelete,
        /// <summary>
        /// If the account Id specified as the delegating spender is invalid or does not exist.
        /// </summary>
        InvalidDelegatingSpender = Proto.ResponseCodeEnum.InvalidDelegatingSpender,
        /// <summary>
        /// The delegating Spender cannot grant approveForAll allowance on a NFT token type for another spender.
        /// </summary>
        DelegatingSpenderCannotGrantApproveForAll = Proto.ResponseCodeEnum.DelegatingSpenderCannotGrantApproveForAll,
        /// <summary>
        /// The delegating Spender cannot grant allowance on a NFT serial for another spender as it doesnt not have approveForAll
        /// granted on token-owner.
        /// </summary>
        DelegatingSpenderDoesNotHaveApproveForAll = Proto.ResponseCodeEnum.DelegatingSpenderDoesNotHaveApproveForAll,
        /// <summary>
        /// The scheduled transaction could not be created because it's expirationTime was too far in the future.
        /// </summary>
        ScheduleExpirationTimeTooFarInFuture = Proto.ResponseCodeEnum.ScheduleExpirationTimeTooFarInFuture,
        /// <summary>
        /// The scheduled transaction could not be created because it's expirationTime was less than or equal to the consensus time.
        /// </summary>
        ScheduleExpirationTimeMustBeHigherThanConsensusTime = Proto.ResponseCodeEnum.ScheduleExpirationTimeMustBeHigherThanConsensusTime,
        /// <summary>
        /// The scheduled transaction could not be created because it would cause throttles to be violated on the specified expirationTime.
        /// </summary>
        ScheduleFutureThrottleExceeded = Proto.ResponseCodeEnum.ScheduleFutureThrottleExceeded,
        /// <summary>
        /// The scheduled transaction could not be created because it would cause the gas limit to be violated on the specified expirationTime.
        /// </summary>
        ScheduleFutureGasLimitExceeded = Proto.ResponseCodeEnum.ScheduleFutureGasLimitExceeded,
        /// <summary>
        /// The ethereum transaction either failed parsing or failed signature Validation = Proto.ResponseCodeEnum.Validation, or some other EthereumTransaction error not covered by another response code.
        /// </summary>
        InvalidEthereumTransaction = Proto.ResponseCodeEnum.InvalidEthereumTransaction,
        /// <summary>
        /// EthereumTransaction was signed against a chainId that this network does not support.
        /// </summary>
        WrongChainId = Proto.ResponseCodeEnum.WrongChainId,
        /// <summary>
        /// This transaction specified an ethereumNonce that is not the current ethereumNonce of the account.
        /// </summary>
        WrongNonce = Proto.ResponseCodeEnum.WrongNonce,
        /// <summary>
        /// The ethereum transaction specified an access List = Proto.ResponseCodeEnum.List, which the network does not support.
        /// </summary>
        AccessListUnsupported = Proto.ResponseCodeEnum.AccessListUnsupported,
        /// <summary>
        /// A schedule being signed or deleted has passed it's expiration date and is pending execution if needed and then expiration.
        /// </summary>
        SchedulePendingExpiration = Proto.ResponseCodeEnum.SchedulePendingExpiration,
        /// <summary>
        /// A selfdestruct or ContractDelete targeted a contract that is a token treasury.
        /// </summary>
        ContractIsTokenTreasury = Proto.ResponseCodeEnum.ContractIsTokenTreasury,
        /// <summary>
        /// A selfdestruct or ContractDelete targeted a contract with non-zero token balances.
        /// </summary>
        ContractHasNonZeroTokenBalances = Proto.ResponseCodeEnum.ContractHasNonZeroTokenBalances,
        /// <summary>
        /// A contract referenced by a transaction is "detached"; that Is = Proto.ResponseCodeEnum.Is, expired and lacking any
        /// hbar funds for auto-renewal payment---but still within its post-expiry grace period.
        /// </summary>
        ContractExpiredAndPendingRemoval = Proto.ResponseCodeEnum.ContractExpiredAndPendingRemoval,
        /// <summary>
        /// A ContractUpdate requested removal of a contract's auto-renew Account = Proto.ResponseCodeEnum.Account, but that contract has
        /// no auto-renew account.
        /// </summary>
        ContractHasNoAutoRenewAccount = Proto.ResponseCodeEnum.ContractHasNoAutoRenewAccount,
        /// <summary>
        /// A delete transaction submitted via HAPI set permanentRemoval=true
        /// </summary>
        PermanentRemovalRequiresSystemInitiation = Proto.ResponseCodeEnum.PermanentRemovalRequiresSystemInitiation,
        /// <summary>
        /// A CryptoCreate or ContractCreate used the deprecated proxyAccountID field.
        /// </summary>
        ProxyAccountIdFieldIsDeprecated = Proto.ResponseCodeEnum.ProxyAccountIdFieldIsDeprecated,
        /// <summary>
        /// An account set the stakedAccountId to itself in CryptoUpdate or ContractUpdate transactions.
        /// </summary>
        SelfStakingIsNotAllowed = Proto.ResponseCodeEnum.SelfStakingIsNotAllowed,
        /// <summary>
        /// The staking account id or staking node id given is invalid or does not exist.
        /// </summary>
        InvalidStakingId = Proto.ResponseCodeEnum.InvalidStakingId,
        /// <summary>
        /// Native Staking = Proto.ResponseCodeEnum.Staking, while Implemented = Proto.ResponseCodeEnum.Implemented, has not yet enabled by the council.
        /// </summary>
        StakingNotEnabled = Proto.ResponseCodeEnum.StakingNotEnabled,
        /// <summary>
        /// The range provided in UtilPrng transaction is negative.
        /// </summary>
        InvalidPrngRange = Proto.ResponseCodeEnum.InvalidPrngRange,
        /// <summary>
        /// The maximum number of entities allowed in the current price regime have been created.
        /// </summary>
        MaxEntitiesInPriceRegimeHaveBeenCreated = Proto.ResponseCodeEnum.MaxEntitiesInPriceRegimeHaveBeenCreated,
        /// <summary>
        /// The full prefix signature for precompile is not valid
        /// </summary>
        InvalidFullPrefixSignatureForPrecompile = Proto.ResponseCodeEnum.InvalidFullPrefixSignatureForPrecompile,
        /// <summary>
        /// The combined balances of a contract and its auto-renew account (if any) did not cover
        /// the rent charged for net new storage used in a transaction.
        /// </summary>
        InsufficientBalancesForStorageRent = Proto.ResponseCodeEnum.InsufficientBalancesForStorageRent,
        /// <summary>
        /// A contract transaction tried to use more than the allowed number of child Records = Proto.ResponseCodeEnum.Records, via
        /// either system contract records or internal contract creations.
        /// </summary>
        MaxChildRecordsExceeded = Proto.ResponseCodeEnum.MaxChildRecordsExceeded,
        /// <summary>
        /// The combined balances of a contract and its auto-renew account (if any) or balance of an account did not cover
        /// the auto-renewal fees in a transaction.
        /// </summary>
        InsufficientBalancesForRenewalFees = Proto.ResponseCodeEnum.InsufficientBalancesForRenewalFees,
        /// <summary>
        /// A transaction's protobuf message includes unknown fields; could mean that a client
        /// expects not-yet-released functionality to be available.
        /// </summary>
        TransactionHasUnknownFields = Proto.ResponseCodeEnum.TransactionHasUnknownFields,
        /// <summary>
        /// The account cannot be modified. Account's key is not set
        /// </summary>
        AccountIsImmutable = Proto.ResponseCodeEnum.AccountIsImmutable,
        /// <summary>
        /// An alias that is assigned to an account or contract cannot be assigned to another account or contract.
        /// </summary>
        AliasAlreadyAssigned = Proto.ResponseCodeEnum.AliasAlreadyAssigned,
        /// <summary>
        /// A provided metadata key was invalid. Verification Includes = Proto.ResponseCodeEnum.Includes, for Example = Proto.ResponseCodeEnum.Example, checking the size of Ed25519 and ECDSA(secp256k1) public keys.
        /// </summary>
        InvalidMetadataKey = Proto.ResponseCodeEnum.InvalidMetadataKey,
        /// <summary>
        /// Metadata key is not set on token
        /// </summary>
        TokenHasNoMetadataKey = Proto.ResponseCodeEnum.TokenHasNoMetadataKey,
        /// <summary>
        /// Token Metadata is not provided
        /// </summary>
        MissingTokenMetadata = Proto.ResponseCodeEnum.MissingTokenMetadata,
        /// <summary>
        /// NFT serial numbers are missing in the TokenUpdateNftsTransactionBody
        /// </summary>
        MissingSerialNumbers = Proto.ResponseCodeEnum.MissingSerialNumbers,
        /// <summary>
        /// Admin key is not set on token
        /// </summary>
        TokenHasNoAdminKey = Proto.ResponseCodeEnum.TokenHasNoAdminKey,
        /// <summary>
        /// A transaction failed because the consensus node identified is
        /// deleted from the address book.
        /// </summary>
        NodeDeleted = Proto.ResponseCodeEnum.NodeDeleted,
        /// <summary>
        /// A transaction failed because the consensus node identified is not valid or
        /// does not exist in state.
        /// </summary>
        InvalidNodeId = Proto.ResponseCodeEnum.InvalidNodeId,
        /// <summary>
        /// A transaction failed because one or more entries in the list of
        /// service endpoints for the `gossipEndpoint` field is invalid.<br/>
        /// The most common cause for this response is a service endpoint that has
        /// the domain name (DNS) set rather than address and port.
        /// </summary>
        InvalidGossipEndpoint = Proto.ResponseCodeEnum.InvalidGossipEndpoint,
        /// <summary>
        /// A transaction failed because the node account identifier provided
        /// does not exist or is not valid.<br/>
        /// One common source of this error is providing a node account identifier
        /// using the "alias" form rather than "numeric" form.
        /// It is also used for atomic batch transaction for child transaction if the node account id is not 0.0.0.
        /// </summary>
        InvalidNodeAccountId = Proto.ResponseCodeEnum.InvalidNodeAccountId,
        /// <summary>
        /// A transaction failed because the description field cannot be encoded
        /// as UTF-8 or is more than 100 bytes when encoded.
        /// </summary>
        InvalidNodeDescription = Proto.ResponseCodeEnum.InvalidNodeDescription,
        /// <summary>
        /// A transaction failed because one or more entries in the list of
        /// service endpoints for the `serviceEndpoint` field is invalid.<br/>
        /// The most common cause for this response is a service endpoint that has
        /// the domain name (DNS) set rather than address and port.
        /// </summary>
        InvalidServiceEndpoint = Proto.ResponseCodeEnum.InvalidServiceEndpoint,
        /// <summary>
        /// A transaction failed because the TLS certificate provided for the
        /// node is missing or invalid.
        /// <p>
        /// #### Probable Causes
        /// The certificate MUST be a TLS certificate of a type permitted for gossip
        /// signatures.<br/>
        /// The value presented MUST be a UTF-8 NFKD encoding of the TLS
        /// certificate.<br/>
        /// The certificate encoded MUST be in PEM format.<br/>
        /// The `gossipCaCertificate` field is REQUIRED and MUST NOT be empty.
        /// </summary>
        InvalidGossipCaCertificate = Proto.ResponseCodeEnum.InvalidGossipCaCertificate,
        /// <summary>
        /// A transaction failed because the hash provided for the gRPC certificate
        /// is present but invalid.
        /// <p>
        /// #### Probable Causes
        /// The `grpcCertificateHash` MUST be a SHA-384 hash.<br/>
        /// The input hashed MUST be a UTF-8 NFKD encoding of the actual TLS
        /// certificate.<br/>
        /// The certificate to be encoded MUST be in PEM format.
        /// </summary>
        InvalidGrpcCertificate = Proto.ResponseCodeEnum.InvalidGrpcCertificate,
        /// <summary>
        /// The maximum automatic associations value is not valid.<br/>
        /// The most common cause for this error is a value less than `-1`.
        /// </summary>
        InvalidMaxAutoAssociations = Proto.ResponseCodeEnum.InvalidMaxAutoAssociations,
        /// <summary>
        /// The maximum number of nodes allowed in the address book have been created.
        /// </summary>
        MaxNodesCreated = Proto.ResponseCodeEnum.MaxNodesCreated,
        /// <summary>
        /// In Serviceendpoint = Proto.ResponseCodeEnum.Serviceendpoint, domainName and ipAddressV4 are mutually exclusive
        /// </summary>
        IpFqdnCannotBeSetForSameEndpoint = Proto.ResponseCodeEnum.IpFqdnCannotBeSetForSameEndpoint,
        /// <summary>
        /// Fully qualified domain name is not allowed in gossipEndpoint
        /// </summary>
        GossipEndpointCannotHaveFqdn = Proto.ResponseCodeEnum.GossipEndpointCannotHaveFqdn,
        /// <summary>
        /// In Serviceendpoint = Proto.ResponseCodeEnum.Serviceendpoint, domainName size too large
        /// </summary>
        FqdnSizeTooLarge = Proto.ResponseCodeEnum.FqdnSizeTooLarge,
        /// <summary>
        /// ServiceEndpoint is invalid
        /// </summary>
        InvalidEndpoint = Proto.ResponseCodeEnum.InvalidEndpoint,
        /// <summary>
        /// The number of gossip endpoints exceeds the limit
        /// </summary>
        GossipEndpointsExceededLimit = Proto.ResponseCodeEnum.GossipEndpointsExceededLimit,
        /// <summary>
        /// The transaction attempted to use duplicate `TokenReference`.<br/>
        /// This affects `TokenReject` attempting to reject same token reference more than once.
        /// </summary>
        TokenReferenceRepeated = Proto.ResponseCodeEnum.TokenReferenceRepeated,
        /// <summary>
        /// The account id specified as the owner in `TokenReject` is invalid or does not exist.
        /// </summary>
        InvalidOwnerId = Proto.ResponseCodeEnum.InvalidOwnerId,
        /// <summary>
        /// The transaction attempted to use more than the allowed number of `TokenReference`.
        /// </summary>
        TokenReferenceListSizeLimitExceeded = Proto.ResponseCodeEnum.TokenReferenceListSizeLimitExceeded,
        /// <summary>
        /// The number of service endpoints exceeds the limit
        /// </summary>
        ServiceEndpointsExceededLimit = Proto.ResponseCodeEnum.ServiceEndpointsExceededLimit,
        /// <summary>
        /// The IPv4 address is invalid
        /// </summary>
        InvalidIpv4Address = Proto.ResponseCodeEnum.InvalidIpv4Address,
        /// <summary>
        /// The transaction attempted to use empty `TokenReference` list.
        /// </summary>
        EmptyTokenReferenceList = Proto.ResponseCodeEnum.EmptyTokenReferenceList,
        /// <summary>
        /// The node account is not allowed to be updated
        /// </summary>
        UpdateNodeAccountNotAllowed = Proto.ResponseCodeEnum.UpdateNodeAccountNotAllowed,
        /// <summary>
        /// The token has no metadata or supply key
        /// </summary>
        TokenHasNoMetadataOrSupplyKey = Proto.ResponseCodeEnum.TokenHasNoMetadataOrSupplyKey,
        /// <summary>
        /// The list of `PendingAirdropId`s is empty and MUST NOT be empty.
        /// </summary>
        EmptyPendingAirdropIdList = Proto.ResponseCodeEnum.EmptyPendingAirdropIdList,
        /// <summary>
        /// A `PendingAirdropId` is repeated in a `claim` or `cancel` transaction.
        /// </summary>
        PendingAirdropIdRepeated = Proto.ResponseCodeEnum.PendingAirdropIdRepeated,
        /// <summary>
        /// The number of `PendingAirdropId` values in the list exceeds the maximum
        /// allowable number.
        /// </summary>
        PendingAirdropIdListTooLong = Proto.ResponseCodeEnum.PendingAirdropIdListTooLong,
        /// <summary>
        /// A pending airdrop already exists for the specified NFT.
        /// </summary>
        PendingNftAirdropAlreadyExists = Proto.ResponseCodeEnum.PendingNftAirdropAlreadyExists,
        /// <summary>
        /// The identified account is sender for one or more pending airdrop(s)
        /// and cannot be deleted.
        /// <p>
        /// The requester SHOULD cancel all pending airdrops before resending
        /// this transaction.
        /// </summary>
        AccountHasPendingAirdrops = Proto.ResponseCodeEnum.AccountHasPendingAirdrops,
        /// <summary>
        /// Consensus throttle did not allow execution of this transaction.<br/>
        /// The transaction should be retried after a modest delay.
        /// </summary>
        ThrottledAtConsensus = Proto.ResponseCodeEnum.ThrottledAtConsensus,
        /// <summary>
        /// The provided pending airdrop id is invalid.<br/>
        /// This pending airdrop MAY already be claimed or cancelled.
        /// <p>
        /// The client SHOULD query a mirror node to determine the current status of
        /// the pending airdrop.
        /// </summary>
        InvalidPendingAirdropId = Proto.ResponseCodeEnum.InvalidPendingAirdropId,
        /// <summary>
        /// The token to be airdropped has a fallback royalty fee and cannot be
        /// sent or claimed via an airdrop transaction.
        /// </summary>
        TokenAirdropWithFallbackRoyalty = Proto.ResponseCodeEnum.TokenAirdropWithFallbackRoyalty,
        /// <summary>
        /// This airdrop claim is for a pending airdrop with an invalid token.<br/>
        /// The token might be Deleted = Proto.ResponseCodeEnum.Deleted, or the sender may not have enough tokens
        /// to fulfill the offer.
        /// <p>
        /// The client SHOULD query mirror node to determine the status of the
        /// pending airdrop and whether the sender can fulfill the offer.
        /// </summary>
        InvalidTokenInPendingAirdrop = Proto.ResponseCodeEnum.InvalidTokenInPendingAirdrop,
        /// <summary>
        /// A scheduled transaction configured to wait for expiry to execute was given
        /// an expiry time at which there is already too many transactions scheduled to
        /// expire; its creation must be retried with a different expiry.
        /// </summary>
        ScheduleExpiryIsBusy = Proto.ResponseCodeEnum.ScheduleExpiryIsBusy,
        /// <summary>
        /// The provided gRPC certificate hash is invalid.
        /// </summary>
        InvalidGrpcCertificateHash = Proto.ResponseCodeEnum.InvalidGrpcCertificateHash,
        /// <summary>
        /// A scheduled transaction configured to wait for expiry to execute was not
        /// given an explicit expiration time.
        /// </summary>
        MissingExpiryTime = Proto.ResponseCodeEnum.MissingExpiryTime,
        /// <summary>
        /// A contract operation attempted to schedule another transaction after it
        /// had already scheduled a recursive contract call.
        /// </summary>
        NoSchedulingAllowedAfterScheduledRecursion = Proto.ResponseCodeEnum.NoSchedulingAllowedAfterScheduledRecursion,
        /// <summary>
        /// A contract can schedule recursive calls a finite number of times (this is
        /// approximately four million times with typical network configuration.)
        /// </summary>
        RecursiveSchedulingLimitReached = Proto.ResponseCodeEnum.RecursiveSchedulingLimitReached,
        /// <summary>
        /// The target network is waiting for the ledger ID to be Set = Proto.ResponseCodeEnum.Set, which is a
        /// side effect of finishing the network's TSS construction.
        /// </summary>
        WaitingForLedgerId = Proto.ResponseCodeEnum.WaitingForLedgerId,
        /// <summary>
        /// The provided fee exempt key list size exceeded the limit.
        /// </summary>
        MaxEntriesForFeeExemptKeyListExceeded = Proto.ResponseCodeEnum.MaxEntriesForFeeExemptKeyListExceeded,
        /// <summary>
        /// The provided fee exempt key list contains duplicated keys.
        /// </summary>
        FeeExemptKeyListContainsDuplicatedKeys = Proto.ResponseCodeEnum.FeeExemptKeyListContainsDuplicatedKeys,
        /// <summary>
        /// The provided fee exempt key list contains an invalid key.
        /// </summary>
        InvalidKeyInFeeExemptKeyList = Proto.ResponseCodeEnum.InvalidKeyInFeeExemptKeyList,
        /// <summary>
        /// The provided fee schedule key contains an invalid key.
        /// </summary>
        InvalidFeeScheduleKey = Proto.ResponseCodeEnum.InvalidFeeScheduleKey,
        /// <summary>
        /// If a fee schedule key is not set when we create a topic
        /// we cannot add it on update.
        /// </summary>
        FeeScheduleKeyCannotBeUpdated = Proto.ResponseCodeEnum.FeeScheduleKeyCannotBeUpdated,
        /// <summary>
        /// If the topic's custom fees are updated the topic SHOULD have a
        /// fee schedule key
        /// </summary>
        FeeScheduleKeyNotSet = Proto.ResponseCodeEnum.FeeScheduleKeyNotSet,
        /// <summary>
        /// The fee amount is exceeding the amount that the payer
        /// is willing to pay.
        /// </summary>
        MaxCustomFeeLimitExceeded = Proto.ResponseCodeEnum.MaxCustomFeeLimitExceeded,
        /// <summary>
        /// There are no corresponding custom fees.
        /// </summary>
        NoValidMaxCustomFee = Proto.ResponseCodeEnum.NoValidMaxCustomFee,
        /// <summary>
        /// The provided list contains invalid max custom fee.
        /// </summary>
        InvalidMaxCustomFees = Proto.ResponseCodeEnum.InvalidMaxCustomFees,
        /// <summary>
        /// The provided max custom fee list contains fees with
        /// duplicate denominations.
        /// </summary>
        DuplicateDenominationInMaxCustomFeeList = Proto.ResponseCodeEnum.DuplicateDenominationInMaxCustomFeeList,
        /// <summary>
        /// The provided max custom fee list contains fees with
        /// duplicate account id.
        /// </summary>
        DuplicateAccountIdInMaxCustomFeeList = Proto.ResponseCodeEnum.DuplicateAccountIdInMaxCustomFeeList,
        /// <summary>
        /// Max custom fees list is not supported for this operation.
        /// </summary>
        MaxCustomFeesIsNotSupported = Proto.ResponseCodeEnum.MaxCustomFeesIsNotSupported,
        /// <summary>
        /// The list of batch transactions is empty
        /// </summary>
        BatchListEmpty = Proto.ResponseCodeEnum.BatchListEmpty,
        /// <summary>
        /// The list of batch transactions contains duplicated transactions
        /// </summary>
        BatchListContainsDuplicates = Proto.ResponseCodeEnum.BatchListContainsDuplicates,
        /// <summary>
        /// The list of batch transactions contains a transaction type that is
        /// in the AtomicBatch blacklist as configured in the network.
        /// </summary>
        BatchTransactionInBlacklist = Proto.ResponseCodeEnum.BatchTransactionInBlacklist,
        /// <summary>
        /// The inner transaction of a batch transaction failed
        /// </summary>
        InnerTransactionFailed = Proto.ResponseCodeEnum.InnerTransactionFailed,
        /// <summary>
        /// The inner transaction of a batch transaction is missing a batch key
        /// </summary>
        MissingBatchKey = Proto.ResponseCodeEnum.MissingBatchKey,
        /// <summary>
        /// The batch key is set for a non batch transaction
        /// </summary>
        BatchKeySetOnNonInnerTransaction = Proto.ResponseCodeEnum.BatchKeySetOnNonInnerTransaction,
        /// <summary>
        /// The batch key is not valid
        /// </summary>
        InvalidBatchKey = Proto.ResponseCodeEnum.InvalidBatchKey,
        /// <summary>
        /// The provided schedule expiry time is not configurable.
        /// </summary>
        ScheduleExpiryNotConfigurable = Proto.ResponseCodeEnum.ScheduleExpiryNotConfigurable,
        /// <summary>
        /// The network just started at genesis and is creating system entities.
        /// </summary>
        CreatingSystemEntities = Proto.ResponseCodeEnum.CreatingSystemEntities,
        /// <summary>
        /// The least common multiple of the throttle group's milliOpsPerSec is
        /// too large and it's overflowing.
        /// </summary>
        ThrottleGroupLcmOverflow = Proto.ResponseCodeEnum.ThrottleGroupLcmOverflow,
        /// <summary>
        /// Token airdrop transactions can not contain multiple senders for a single token.
        /// </summary>
        AirdropContainsMultipleSendersForAToken = Proto.ResponseCodeEnum.AirdropContainsMultipleSendersForAToken,
        /// <summary>
        /// The GRPC proxy endpoint is set in the NodeCreate or NodeUpdate Transaction = Proto.ResponseCodeEnum.Transaction,
        /// which the network does not support.
        /// </summary>
        GrpcWebProxyNotSupported = Proto.ResponseCodeEnum.GrpcWebProxyNotSupported,
        /// <summary>
        /// An NFT transfers list referenced a token type other than NONFUNGIBLEUNIQUE.
        /// </summary>
        NftTransfersOnlyAllowedForNonFungibleUnique = Proto.ResponseCodeEnum.NftTransfersOnlyAllowedForNonFungibleUnique,
        /// <summary>
        /// A HAPI client cannot set the SignedTransaction#useSerializedTxMessageHashAlgorithm field.
        /// </summary>
        InvalidSerializedTxMessageHashAlgorithm = Proto.ResponseCodeEnum.InvalidSerializedTxMessageHashAlgorithm,
        /// <summary>
        /// An EVM hook execution was throttled due to high network gas utilization.
        /// </summary>
        EvmHookGasThrottled = Proto.ResponseCodeEnum.EvmHookGasThrottled,
        /// <summary>
        /// A user tried to create a hook with an id already in use.
        /// </summary>
        HookIdInUse = Proto.ResponseCodeEnum.HookIdInUse,
        /// <summary>
        /// A transaction tried to execute a hook that did not match the specified
        /// type or was malformed in some other way.
        /// </summary>
        BadHookRequest = Proto.ResponseCodeEnum.BadHookRequest,
        /// <summary>
        /// A CryptoTransfer relying on a ACCOUNTALLOWANCE hook was rejected.
        /// </summary>
        RejectedByAccountAllowanceHook = Proto.ResponseCodeEnum.RejectedByAccountAllowanceHook,
        /// <summary>
        /// A hook id was not found.
        /// </summary>
        HookNotFound = Proto.ResponseCodeEnum.HookNotFound,
        /// <summary>
        /// A lambda mapping Slot = Proto.ResponseCodeEnum.Slot, storage Key = Proto.ResponseCodeEnum.Key, or storage value exceeded 32 bytes.
        /// </summary>
        LambdaStorageUpdateBytesTooLong = Proto.ResponseCodeEnum.LambdaStorageUpdateBytesTooLong,
        /// <summary>
        /// A lambda mapping Slot = Proto.ResponseCodeEnum.Slot, storage Key = Proto.ResponseCodeEnum.Key, or storage value failed to use the
        /// minimal representation (i.e. = Proto.ResponseCodeEnum.., no leading zeros).
        /// </summary>
        LambdaStorageUpdateBytesMustUseMinimalRepresentation = Proto.ResponseCodeEnum.LambdaStorageUpdateBytesMustUseMinimalRepresentation,
        /// <summary>
        /// A hook id was invalid.
        /// </summary>
        InvalidHookId = Proto.ResponseCodeEnum.InvalidHookId,
        /// <summary>
        /// A lambda storage update had no contents.
        /// </summary>
        EmptyLambdaStorageUpdate = Proto.ResponseCodeEnum.EmptyLambdaStorageUpdate,
        /// <summary>
        /// A user repeated the same hook id in a creation details list.
        /// </summary>
        HookIdRepeatedInCreationDetails = Proto.ResponseCodeEnum.HookIdRepeatedInCreationDetails,
        /// <summary>
        /// Hooks are not not enabled on the target Hiero network.
        /// </summary>
        HooksNotEnabled = Proto.ResponseCodeEnum.HooksNotEnabled,
        /// <summary>
        /// The target hook is not a lambda.
        /// </summary>
        HookIsNotALambda = Proto.ResponseCodeEnum.HookIsNotALambda,
        /// <summary>
        /// A hook was deleted.
        /// </summary>
        HookDeleted = Proto.ResponseCodeEnum.HookDeleted,
        /// <summary>
        /// The LambdaSStore tried to update too many storage slots in a single transaction.
        /// </summary>
        TooManyLambdaStorageUpdates = Proto.ResponseCodeEnum.TooManyLambdaStorageUpdates,
        /// <summary>
        /// A lambda mapping Slot = Proto.ResponseCodeEnum.Slot, storage Key = Proto.ResponseCodeEnum.Key, or storage value failed to use the
        /// minimal representation (i.e. = Proto.ResponseCodeEnum.., no leading zeros).
        /// </summary>
        HookCreationBytesMustUseMinimalRepresentation = Proto.ResponseCodeEnum.HookCreationBytesMustUseMinimalRepresentation,
        /// <summary>
        /// A lambda mapping Slot = Proto.ResponseCodeEnum.Slot, storage Key = Proto.ResponseCodeEnum.Key, or storage value exceeded 32 bytes.
        /// </summary>
        HookCreationBytesTooLong = Proto.ResponseCodeEnum.HookCreationBytesTooLong,
        /// <summary>
        /// A hook creation spec was not found.
        /// </summary>
        InvalidHookCreationSpec = Proto.ResponseCodeEnum.InvalidHookCreationSpec,
        /// <summary>
        /// A hook extension point was empty.
        /// </summary>
        HookExtensionEmpty = Proto.ResponseCodeEnum.HookExtensionEmpty,
        /// <summary>
        /// A hook admin key was invalid.
        /// </summary>
        InvalidHookAdminKey = Proto.ResponseCodeEnum.InvalidHookAdminKey,
        /// <summary>
        /// The hook deletion requires the hook to have zero storage slots.
        /// </summary>
        HookDeletionRequiresZeroStorageSlots = Proto.ResponseCodeEnum.HookDeletionRequiresZeroStorageSlots,
        /// <summary>
        /// Cannot set both a hook call and an approval on the same AccountAmount or NftTransfer message.
        /// </summary>
        CannotSetHooksAndApproval = Proto.ResponseCodeEnum.CannotSetHooksAndApproval,
        /// <summary>
        /// The attempted operation is invalid until all the target entity's hooks have been deleted.
        /// </summary>
        TransactionRequiresZeroHooks = Proto.ResponseCodeEnum.TransactionRequiresZeroHooks,
        /// <summary>
        /// The HookCall set in the transaction is invalid
        /// </summary>
        InvalidHookCall = Proto.ResponseCodeEnum.InvalidHookCall,
        /// <summary>
        /// Hooks are not supported to be used in TokenAirdrop transactions
        /// </summary>
    }
}