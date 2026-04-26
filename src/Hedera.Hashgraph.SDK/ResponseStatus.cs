// SPDX-License-Identifier: Apache-2.0

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus"]/*' />
    public enum ResponseStatus
    {
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_2"]/*' />
        Ok = Proto.Services.ResponseCodeEnum.Ok,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_3"]/*' />
        InvalidTransaction = Proto.Services.ResponseCodeEnum.InvalidTransaction,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_4"]/*' />
        PayerAccountNotFound = Proto.Services.ResponseCodeEnum.PayerAccountNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_5"]/*' />
        InvalidNodeAccount = Proto.Services.ResponseCodeEnum.InvalidNodeAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_6"]/*' />
        TransactionExpired = Proto.Services.ResponseCodeEnum.TransactionExpired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_7"]/*' />
        InvalidTransactionStart = Proto.Services.ResponseCodeEnum.InvalidTransactionStart,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_8"]/*' />
        InvalidTransactionDuration = Proto.Services.ResponseCodeEnum.InvalidTransactionDuration,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_9"]/*' />
        InvalidSignature = Proto.Services.ResponseCodeEnum.InvalidSignature,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.seconds(receipt)"]/*' />
        MemoTooLong = Proto.Services.ResponseCodeEnum.MemoTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.seconds(receipt)_2"]/*' />
        InsufficientTxFee = Proto.Services.ResponseCodeEnum.InsufficientTxFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.seconds(receipt)_3"]/*' />
        InsufficientPayerBalance = Proto.Services.ResponseCodeEnum.InsufficientPayerBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_10"]/*' />
        DuplicateTransaction = Proto.Services.ResponseCodeEnum.DuplicateTransaction,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_11"]/*' />
        Busy = Proto.Services.ResponseCodeEnum.Busy,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_12"]/*' />
        NotSupported = Proto.Services.ResponseCodeEnum.NotSupported,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_13"]/*' />
        InvalidFileId = Proto.Services.ResponseCodeEnum.InvalidFileId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_14"]/*' />
        InvalidAccountId = Proto.Services.ResponseCodeEnum.InvalidAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_15"]/*' />
        InvalidContractId = Proto.Services.ResponseCodeEnum.InvalidContractId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_16"]/*' />
        InvalidTransactionId = Proto.Services.ResponseCodeEnum.InvalidTransactionId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_17"]/*' />
        ReceiptNotFound = Proto.Services.ResponseCodeEnum.ReceiptNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_18"]/*' />
        RecordNotFound = Proto.Services.ResponseCodeEnum.RecordNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_19"]/*' />
        InvalidSolidityId = Proto.Services.ResponseCodeEnum.InvalidSolidityId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_20"]/*' />
        Unknown = Proto.Services.ResponseCodeEnum.Unknown,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_21"]/*' />
        Success = Proto.Services.ResponseCodeEnum.Success,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_22"]/*' />
        FailInvalid = Proto.Services.ResponseCodeEnum.FailInvalid,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_23"]/*' />
        FailFee = Proto.Services.ResponseCodeEnum.FailFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_24"]/*' />
        FailBalance = Proto.Services.ResponseCodeEnum.FailBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_25"]/*' />
        KeyRequired = Proto.Services.ResponseCodeEnum.KeyRequired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_26"]/*' />
        BadEncoding = Proto.Services.ResponseCodeEnum.BadEncoding,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.execution(query)"]/*' />
        InsufficientAccountBalance = Proto.Services.ResponseCodeEnum.InsufficientAccountBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.execution(query)_2"]/*' />
        InvalidSolidityAddress = Proto.Services.ResponseCodeEnum.InvalidSolidityAddress,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.execution(query)_3"]/*' />
        InsufficientGas = Proto.Services.ResponseCodeEnum.InsufficientGas,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.execution(query)_4"]/*' />
        ContractSizeLimitExceeded = Proto.Services.ResponseCodeEnum.ContractSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ve(amount)"]/*' />
        LocalCallModificationException = Proto.Services.ResponseCodeEnum.LocalCallModificationException,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ve(amount)_2"]/*' />
        ContractRevertExecuted = Proto.Services.ResponseCodeEnum.ContractRevertExecuted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ve(amount)_3"]/*' />
        ContractExecutionException = Proto.Services.ResponseCodeEnum.ContractExecutionException,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_27"]/*' />
        InvalidReceivingNodeAccount = Proto.Services.ResponseCodeEnum.InvalidReceivingNodeAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_28"]/*' />
        MissingQueryHeader = Proto.Services.ResponseCodeEnum.MissingQueryHeader,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_29"]/*' />
        AccountUpdateFailed = Proto.Services.ResponseCodeEnum.AccountUpdateFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_30"]/*' />
        InvalidKeyEncoding = Proto.Services.ResponseCodeEnum.InvalidKeyEncoding,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_31"]/*' />
        NullSolidityAddress = Proto.Services.ResponseCodeEnum.NullSolidityAddress,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_32"]/*' />
        ContractUpdateFailed = Proto.Services.ResponseCodeEnum.ContractUpdateFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_33"]/*' />
        InvalidQueryHeader = Proto.Services.ResponseCodeEnum.InvalidQueryHeader,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_34"]/*' />
        InvalidFeeSubmitted = Proto.Services.ResponseCodeEnum.InvalidFeeSubmitted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_35"]/*' />
        InvalidPayerSignature = Proto.Services.ResponseCodeEnum.InvalidPayerSignature,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_36"]/*' />
        KeyNotProvided = Proto.Services.ResponseCodeEnum.KeyNotProvided,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_37"]/*' />
        InvalidExpirationTime = Proto.Services.ResponseCodeEnum.InvalidExpirationTime,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_38"]/*' />
        NoWaclKey = Proto.Services.ResponseCodeEnum.NoWaclKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(base ed25519,Keylist,or)"]/*' />
        FileContentEmpty = Proto.Services.ResponseCodeEnum.FileContentEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(base ed25519,Keylist,or)_2"]/*' />
        InvalidAccountAmounts = Proto.Services.ResponseCodeEnum.InvalidAccountAmounts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(base ed25519,Keylist,or)_3"]/*' />
        EmptyTransactionBody = Proto.Services.ResponseCodeEnum.EmptyTransactionBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(base ed25519,Keylist,or)_4"]/*' />
        InvalidTransactionBody = Proto.Services.ResponseCodeEnum.InvalidTransactionBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(Keylist,or)"]/*' />
        InvalidSignatureTypeMismatchingKey = Proto.Services.ResponseCodeEnum.InvalidSignatureTypeMismatchingKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_39"]/*' />
        InvalidSignatureCountMismatchingKey = Proto.Services.ResponseCodeEnum.InvalidSignatureCountMismatchingKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_40"]/*' />
        EmptyLiveHashBody = Proto.Services.ResponseCodeEnum.EmptyLiveHashBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_41"]/*' />
        EmptyLiveHash = Proto.Services.ResponseCodeEnum.EmptyLiveHash,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_42"]/*' />
        EmptyLiveHashKeys = Proto.Services.ResponseCodeEnum.EmptyLiveHashKeys,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_43"]/*' />
        InvalidLiveHashSize = Proto.Services.ResponseCodeEnum.InvalidLiveHashSize,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_44"]/*' />
        EmptyQueryBody = Proto.Services.ResponseCodeEnum.EmptyQueryBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_45"]/*' />
        EmptyLiveHashQuery = Proto.Services.ResponseCodeEnum.EmptyLiveHashQuery,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_46"]/*' />
        LiveHashNotFound = Proto.Services.ResponseCodeEnum.LiveHashNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_47"]/*' />
        AccountIdDoesNotExist = Proto.Services.ResponseCodeEnum.AccountIdDoesNotExist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_48"]/*' />
        LiveHashAlreadyExists = Proto.Services.ResponseCodeEnum.LiveHashAlreadyExists,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_49"]/*' />
        InvalidFileWacl = Proto.Services.ResponseCodeEnum.InvalidFileWacl,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_50"]/*' />
        SerializationFailed = Proto.Services.ResponseCodeEnum.SerializationFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_51"]/*' />
        TransactionOversize = Proto.Services.ResponseCodeEnum.TransactionOversize,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_52"]/*' />
        TransactionTooManyLayers = Proto.Services.ResponseCodeEnum.TransactionTooManyLayers,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_53"]/*' />
        ContractDeleted = Proto.Services.ResponseCodeEnum.ContractDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_54"]/*' />
        PlatformNotActive = Proto.Services.ResponseCodeEnum.PlatformNotActive,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_55"]/*' />
        KeyPrefixMismatch = Proto.Services.ResponseCodeEnum.KeyPrefixMismatch,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_56"]/*' />
        PlatformTransactionNotCreated = Proto.Services.ResponseCodeEnum.PlatformTransactionNotCreated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_57"]/*' />
        InvalidRenewalPeriod = Proto.Services.ResponseCodeEnum.InvalidRenewalPeriod,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_58"]/*' />
        InvalidPayerAccountId = Proto.Services.ResponseCodeEnum.InvalidPayerAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_59"]/*' />
        AccountDeleted = Proto.Services.ResponseCodeEnum.AccountDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_60"]/*' />
        FileDeleted = Proto.Services.ResponseCodeEnum.FileDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_61"]/*' />
        AccountRepeatedInAccountAmounts = Proto.Services.ResponseCodeEnum.AccountRepeatedInAccountAmounts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_62"]/*' />
        SettingNegativeAccountBalance = Proto.Services.ResponseCodeEnum.SettingNegativeAccountBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.modify(update or delete a immutable smart,i.e. one created without a /// admin)"]/*' />
        ObtainerRequired = Proto.Services.ResponseCodeEnum.ObtainerRequired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.modify(update or delete a immutable smart,i.e. one created without a /// admin)_2"]/*' />
        ObtainerSameContractId = Proto.Services.ResponseCodeEnum.ObtainerSameContractId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.modify(update or delete a immutable smart,i.e. one created without a /// admin)_3"]/*' />
        ObtainerDoesNotExist = Proto.Services.ResponseCodeEnum.ObtainerDoesNotExist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_63"]/*' />
        ModifyingImmutableContract = Proto.Services.ResponseCodeEnum.ModifyingImmutableContract,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_64"]/*' />
        FileSystemException = Proto.Services.ResponseCodeEnum.FileSystemException,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_65"]/*' />
        AutorenewDurationNotInRange = Proto.Services.ResponseCodeEnum.AutorenewDurationNotInRange,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_66"]/*' />
        ErrorDecodingBytestring = Proto.Services.ResponseCodeEnum.ErrorDecodingBytestring,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_67"]/*' />
        ContractFileEmpty = Proto.Services.ResponseCodeEnum.ContractFileEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_68"]/*' />
        ContractBytecodeEmpty = Proto.Services.ResponseCodeEnum.ContractBytecodeEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_69"]/*' />
        InvalidInitialBalance = Proto.Services.ResponseCodeEnum.InvalidInitialBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_70"]/*' />
        [Obsolete]
        InvalidReceiveRecordThreshold = Proto.Services.ResponseCodeEnum.InvalidReceiveRecordThreshold,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_71"]/*' />
		[Obsolete]
		InvalidSendRecordThreshold = Proto.Services.ResponseCodeEnum.InvalidSendRecordThreshold,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.accounts(both from and)"]/*' />
        AccountIsNotGenesisAccount = Proto.Services.ResponseCodeEnum.AccountIsNotGenesisAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.accounts(both from and)_2"]/*' />
        PayerAccountUnauthorized = Proto.Services.ResponseCodeEnum.PayerAccountUnauthorized,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.accounts(both from and)_3"]/*' />
        InvalidFreezeTransactionBody = Proto.Services.ResponseCodeEnum.InvalidFreezeTransactionBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.accounts(both from and)_4"]/*' />
        FreezeTransactionBodyNotFound = Proto.Services.ResponseCodeEnum.FreezeTransactionBodyNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(account 0.0.)"]/*' />
        TransferListSizeLimitExceeded = Proto.Services.ResponseCodeEnum.TransferListSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(account 0.0.)_2"]/*' />
        ResultSizeLimitExceeded = Proto.Services.ResponseCodeEnum.ResultSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_72"]/*' />
        NotSpecialAccount = Proto.Services.ResponseCodeEnum.NotSpecialAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_73"]/*' />
        ContractNegativeGas = Proto.Services.ResponseCodeEnum.ContractNegativeGas,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_74"]/*' />
        ContractNegativeValue = Proto.Services.ResponseCodeEnum.ContractNegativeValue,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_75"]/*' />
        InvalidFeeFile = Proto.Services.ResponseCodeEnum.InvalidFeeFile,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_76"]/*' />
        InvalidExchangeRateFile = Proto.Services.ResponseCodeEnum.InvalidExchangeRateFile,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_77"]/*' />
        InsufficientLocalCallGas = Proto.Services.ResponseCodeEnum.InsufficientLocalCallGas,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_78"]/*' />
        EntityNotAllowedToDelete = Proto.Services.ResponseCodeEnum.EntityNotAllowedToDelete,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.valid(append or update is)"]/*' />
        AuthorizationFailed = Proto.Services.ResponseCodeEnum.AuthorizationFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.valid(append or update is)_2"]/*' />
        FileUploadedProtoInvalid = Proto.Services.ResponseCodeEnum.FileUploadedProtoInvalid,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_79"]/*' />
        FileUploadedProtoNotSavedToDisk = Proto.Services.ResponseCodeEnum.FileUploadedProtoNotSavedToDisk,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_80"]/*' />
        FeeScheduleFilePartUploaded = Proto.Services.ResponseCodeEnum.FeeScheduleFilePartUploaded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_81"]/*' />
        ExchangeRateChangeLimitExceeded = Proto.Services.ResponseCodeEnum.ExchangeRateChangeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_82"]/*' />
        MaxContractStorageExceeded = Proto.Services.ResponseCodeEnum.MaxContractStorageExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_83"]/*' />
        TotalLedgerBalanceInvalid = Proto.Services.ResponseCodeEnum.TotalLedgerBalanceInvalid,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_84"]/*' />
        ExpirationReductionNotAllowed = Proto.Services.ResponseCodeEnum.ExpirationReductionNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)"]/*' />
        MaxGasLimitExceeded = Proto.Services.ResponseCodeEnum.MaxGasLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_2"]/*' />
        MaxFileSizeExceeded = Proto.Services.ResponseCodeEnum.MaxFileSizeExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_3"]/*' />
        ReceiverSigRequired = Proto.Services.ResponseCodeEnum.ReceiverSigRequired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_4"]/*' />
        InvalidTopicId = Proto.Services.ResponseCodeEnum.InvalidTopicId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.authorized(ie - a deleteTopic for a topic with no)"]/*' />
        InvalidAdminKey = Proto.Services.ResponseCodeEnum.InvalidAdminKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.authorized(ie - a deleteTopic for a topic with no)_2"]/*' />
        InvalidSubmitKey = Proto.Services.ResponseCodeEnum.InvalidSubmitKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_85"]/*' />
        Unauthorized = Proto.Services.ResponseCodeEnum.Unauthorized,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_86"]/*' />
        InvalidTopicMessage = Proto.Services.ResponseCodeEnum.InvalidTopicMessage,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.total(chunks)"]/*' />
        InvalidAutorenewAccount = Proto.Services.ResponseCodeEnum.InvalidAutorenewAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.total(chunks)_2"]/*' />
        AutorenewAccountNotAllowed = Proto.Services.ResponseCodeEnum.AutorenewAccountNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.total(chunks)_3"]/*' />
        TopicExpired = Proto.Services.ResponseCodeEnum.TopicExpired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_87"]/*' />
        InvalidChunkNumber = Proto.Services.ResponseCodeEnum.InvalidChunkNumber,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_88"]/*' />
        InvalidChunkTransactionId = Proto.Services.ResponseCodeEnum.InvalidChunkTransactionId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_89"]/*' />
        AccountFrozenForToken = Proto.Services.ResponseCodeEnum.AccountFrozenForToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_90"]/*' />
        TokensPerAccountLimitExceeded = Proto.Services.ResponseCodeEnum.TokensPerAccountLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_91"]/*' />
        InvalidTokenId = Proto.Services.ResponseCodeEnum.InvalidTokenId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_92"]/*' />
        InvalidTokenDecimals = Proto.Services.ResponseCodeEnum.InvalidTokenDecimals,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_93"]/*' />
        InvalidTokenInitialSupply = Proto.Services.ResponseCodeEnum.InvalidTokenInitialSupply,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_94"]/*' />
        InvalidTreasuryAccountForToken = Proto.Services.ResponseCodeEnum.InvalidTreasuryAccountForToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_95"]/*' />
        InvalidTokenSymbol = Proto.Services.ResponseCodeEnum.InvalidTokenSymbol,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_96"]/*' />
        TokenHasNoFreezeKey = Proto.Services.ResponseCodeEnum.TokenHasNoFreezeKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_97"]/*' />
        TransfersNotZeroSumForToken = Proto.Services.ResponseCodeEnum.TransfersNotZeroSumForToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_98"]/*' />
        MissingTokenSymbol = Proto.Services.ResponseCodeEnum.MissingTokenSymbol,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_99"]/*' />
        TokenSymbolTooLong = Proto.Services.ResponseCodeEnum.TokenSymbolTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_100"]/*' />
        AccountKycNotGrantedForToken = Proto.Services.ResponseCodeEnum.AccountKycNotGrantedForToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_101"]/*' />
        TokenHasNoKycKey = Proto.Services.ResponseCodeEnum.TokenHasNoKycKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_102"]/*' />
        InsufficientTokenBalance = Proto.Services.ResponseCodeEnum.InsufficientTokenBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_103"]/*' />
        TokenWasDeleted = Proto.Services.ResponseCodeEnum.TokenWasDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_104"]/*' />
        TokenHasNoSupplyKey = Proto.Services.ResponseCodeEnum.TokenHasNoSupplyKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_105"]/*' />
        TokenHasNoWipeKey = Proto.Services.ResponseCodeEnum.TokenHasNoWipeKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_106"]/*' />
        InvalidTokenMintAmount = Proto.Services.ResponseCodeEnum.InvalidTokenMintAmount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_107"]/*' />
        InvalidTokenBurnAmount = Proto.Services.ResponseCodeEnum.InvalidTokenBurnAmount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_108"]/*' />
        TokenNotAssociatedToAccount = Proto.Services.ResponseCodeEnum.TokenNotAssociatedToAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_109"]/*' />
        CannotWipeTokenTreasuryAccount = Proto.Services.ResponseCodeEnum.CannotWipeTokenTreasuryAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_110"]/*' />
        InvalidKycKey = Proto.Services.ResponseCodeEnum.InvalidKycKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_111"]/*' />
        InvalidWipeKey = Proto.Services.ResponseCodeEnum.InvalidWipeKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_112"]/*' />
        InvalidFreezeKey = Proto.Services.ResponseCodeEnum.InvalidFreezeKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_113"]/*' />
        InvalidSupplyKey = Proto.Services.ResponseCodeEnum.InvalidSupplyKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_114"]/*' />
        MissingTokenName = Proto.Services.ResponseCodeEnum.MissingTokenName,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_115"]/*' />
        TokenNameTooLong = Proto.Services.ResponseCodeEnum.TokenNameTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_116"]/*' />
        InvalidWipingAmount = Proto.Services.ResponseCodeEnum.InvalidWipingAmount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_117"]/*' />
        TokenIsImmutable = Proto.Services.ResponseCodeEnum.TokenIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.transfers(both from and)"]/*' />
        TokenAlreadyAssociatedToAccount = Proto.Services.ResponseCodeEnum.TokenAlreadyAssociatedToAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.transfers(both from and)_2"]/*' />
        TransactionRequiresZeroTokenBalances = Proto.Services.ResponseCodeEnum.TransactionRequiresZeroTokenBalances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.transfers(both from and)_3"]/*' />
        AccountIsTreasury = Proto.Services.ResponseCodeEnum.AccountIsTreasury,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.transfers(both from and)_4"]/*' />
        TokenIdRepeatedInTokenList = Proto.Services.ResponseCodeEnum.TokenIdRepeatedInTokenList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_118"]/*' />
        TokenTransferListSizeLimitExceeded = Proto.Services.ResponseCodeEnum.TokenTransferListSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_119"]/*' />
        EmptyTokenTransferBody = Proto.Services.ResponseCodeEnum.EmptyTokenTransferBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_120"]/*' />
        EmptyTokenTransferAccountAmounts = Proto.Services.ResponseCodeEnum.EmptyTokenTransferAccountAmounts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_121"]/*' />
        InvalidScheduleId = Proto.Services.ResponseCodeEnum.InvalidScheduleId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_122"]/*' />
        ScheduleIsImmutable = Proto.Services.ResponseCodeEnum.ScheduleIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_123"]/*' />
        InvalidSchedulePayerId = Proto.Services.ResponseCodeEnum.InvalidSchedulePayerId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_124"]/*' />
        InvalidScheduleAccountId = Proto.Services.ResponseCodeEnum.InvalidScheduleAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_125"]/*' />
        NoNewValidSignatures = Proto.Services.ResponseCodeEnum.NoNewValidSignatures,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ScheduleCreate(that,all fields other than)"]/*' />
        UnresolvableRequiredSigners = Proto.Services.ResponseCodeEnum.UnresolvableRequiredSigners,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ScheduleCreate(that,all fields other than)_2"]/*' />
        ScheduledTransactionNotInWhitelist = Proto.Services.ResponseCodeEnum.ScheduledTransactionNotInWhitelist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ScheduleCreate(that,all fields other than)_3"]/*' />
        SomeSignaturesWereInvalid = Proto.Services.ResponseCodeEnum.SomeSignaturesWereInvalid,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ScheduleCreate(that,all fields other than)_4"]/*' />
        TransactionIdFieldNotAllowed = Proto.Services.ResponseCodeEnum.TransactionIdFieldNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_126"]/*' />
        IdenticalScheduleAlreadyCreated = Proto.Services.ResponseCodeEnum.IdenticalScheduleAlreadyCreated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_127"]/*' />
        InvalidZeroByteInString = Proto.Services.ResponseCodeEnum.InvalidZeroByteInString,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_128"]/*' />
        ScheduleAlreadyDeleted = Proto.Services.ResponseCodeEnum.ScheduleAlreadyDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_129"]/*' />
        ScheduleAlreadyExecuted = Proto.Services.ResponseCodeEnum.ScheduleAlreadyExecuted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_130"]/*' />
        MessageSizeTooLarge = Proto.Services.ResponseCodeEnum.MessageSizeTooLarge,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_131"]/*' />
        OperationRepeatedInBucketGroups = Proto.Services.ResponseCodeEnum.OperationRepeatedInBucketGroups,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_132"]/*' />
        BucketCapacityOverflow = Proto.Services.ResponseCodeEnum.BucketCapacityOverflow,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_133"]/*' />
        NodeCapacityNotSufficientForOperation = Proto.Services.ResponseCodeEnum.NodeCapacityNotSufficientForOperation,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_134"]/*' />
        BucketHasNoThrottleGroups = Proto.Services.ResponseCodeEnum.BucketHasNoThrottleGroups,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_135"]/*' />
        ThrottleGroupHasZeroOpsPerSec = Proto.Services.ResponseCodeEnum.ThrottleGroupHasZeroOpsPerSec,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_136"]/*' />
        SuccessButMissingExpectedOperation = Proto.Services.ResponseCodeEnum.SuccessButMissingExpectedOperation,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_137"]/*' />
        UnparseableThrottleDefinitions = Proto.Services.ResponseCodeEnum.UnparseableThrottleDefinitions,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_138"]/*' />
        InvalidThrottleDefinitions = Proto.Services.ResponseCodeEnum.InvalidThrottleDefinitions,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_139"]/*' />
        AccountExpiredAndPendingRemoval = Proto.Services.ResponseCodeEnum.AccountExpiredAndPendingRemoval,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_140"]/*' />
        InvalidTokenMaxSupply = Proto.Services.ResponseCodeEnum.InvalidTokenMaxSupply,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_141"]/*' />
        InvalidTokenNftSerialNumber = Proto.Services.ResponseCodeEnum.InvalidTokenNftSerialNumber,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_142"]/*' />
        InvalidNftId = Proto.Services.ResponseCodeEnum.InvalidNftId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_143"]/*' />
        MetadataTooLong = Proto.Services.ResponseCodeEnum.MetadataTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_144"]/*' />
        BatchSizeLimitExceeded = Proto.Services.ResponseCodeEnum.BatchSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_145"]/*' />
        InvalidQueryRange = Proto.Services.ResponseCodeEnum.InvalidQueryRange,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_146"]/*' />
        FractionDividesByZero = Proto.Services.ResponseCodeEnum.FractionDividesByZero,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_147"]/*' />
		[Obsolete]
		InsufficientPayerBalanceForCustomFee = Proto.Services.ResponseCodeEnum.InsufficientPayerBalanceForCustomFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_148"]/*' />
        CustomFeesListTooLong = Proto.Services.ResponseCodeEnum.CustomFeesListTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_149"]/*' />
        InvalidCustomFeeCollector = Proto.Services.ResponseCodeEnum.InvalidCustomFeeCollector,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_150"]/*' />
        InvalidTokenIdInCustomFees = Proto.Services.ResponseCodeEnum.InvalidTokenIdInCustomFees,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_151"]/*' />
        TokenNotAssociatedToFeeCollector = Proto.Services.ResponseCodeEnum.TokenNotAssociatedToFeeCollector,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_152"]/*' />
        TokenMaxSupplyReached = Proto.Services.ResponseCodeEnum.TokenMaxSupplyReached,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_153"]/*' />
        SenderDoesNotOwnNftSerialNo = Proto.Services.ResponseCodeEnum.SenderDoesNotOwnNftSerialNo,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_154"]/*' />
        CustomFeeNotFullySpecified = Proto.Services.ResponseCodeEnum.CustomFeeNotFullySpecified,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_155"]/*' />
        CustomFeeMustBePositive = Proto.Services.ResponseCodeEnum.CustomFeeMustBePositive,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_156"]/*' />
        TokenHasNoFeeScheduleKey = Proto.Services.ResponseCodeEnum.TokenHasNoFeeScheduleKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_157"]/*' />
        CustomFeeOutsideNumericRange = Proto.Services.ResponseCodeEnum.CustomFeeOutsideNumericRange,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_158"]/*' />
        RoyaltyFractionCannotExceedOne = Proto.Services.ResponseCodeEnum.RoyaltyFractionCannotExceedOne,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_159"]/*' />
        FractionalFeeMaxAmountLessThanMinAmount = Proto.Services.ResponseCodeEnum.FractionalFeeMaxAmountLessThanMinAmount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_160"]/*' />
        CustomScheduleAlreadyHasNoFees = Proto.Services.ResponseCodeEnum.CustomScheduleAlreadyHasNoFees,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_161"]/*' />
        CustomFeeDenominationMustBeFungibleCommon = Proto.Services.ResponseCodeEnum.CustomFeeDenominationMustBeFungibleCommon,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_162"]/*' />
        CustomFractionalFeeOnlyAllowedForFungibleCommon = Proto.Services.ResponseCodeEnum.CustomFractionalFeeOnlyAllowedForFungibleCommon,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_163"]/*' />
        InvalidCustomFeeScheduleKey = Proto.Services.ResponseCodeEnum.InvalidCustomFeeScheduleKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_164"]/*' />
        InvalidTokenMintMetadata = Proto.Services.ResponseCodeEnum.InvalidTokenMintMetadata,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_165"]/*' />
        InvalidTokenBurnMetadata = Proto.Services.ResponseCodeEnum.InvalidTokenBurnMetadata,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_166"]/*' />
        CurrentTreasuryStillOwnsNfts = Proto.Services.ResponseCodeEnum.CurrentTreasuryStillOwnsNfts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_167"]/*' />
        AccountStillOwnsNfts = Proto.Services.ResponseCodeEnum.AccountStillOwnsNfts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_168"]/*' />
        TreasuryMustOwnBurnedNft = Proto.Services.ResponseCodeEnum.TreasuryMustOwnBurnedNft,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_169"]/*' />
        AccountDoesNotOwnWipedNft = Proto.Services.ResponseCodeEnum.AccountDoesNotOwnWipedNft,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_170"]/*' />
        AccountAmountTransfersOnlyAllowedForFungibleCommon = Proto.Services.ResponseCodeEnum.AccountAmountTransfersOnlyAllowedForFungibleCommon,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_171"]/*' />
        MaxNftsInPriceRegimeHaveBeenMinted = Proto.Services.ResponseCodeEnum.MaxNftsInPriceRegimeHaveBeenMinted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_172"]/*' />
        PayerAccountDeleted = Proto.Services.ResponseCodeEnum.PayerAccountDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_173"]/*' />
        CustomFeeChargingExceededMaxRecursionDepth = Proto.Services.ResponseCodeEnum.CustomFeeChargingExceededMaxRecursionDepth,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_174"]/*' />
        CustomFeeChargingExceededMaxAccountAmounts = Proto.Services.ResponseCodeEnum.CustomFeeChargingExceededMaxAccountAmounts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_175"]/*' />
        InsufficientSenderAccountBalanceForCustomFee = Proto.Services.ResponseCodeEnum.InsufficientSenderAccountBalanceForCustomFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_176"]/*' />
        SerialNumberLimitReached = Proto.Services.ResponseCodeEnum.SerialNumberLimitReached,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_177"]/*' />
        CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique = Proto.Services.ResponseCodeEnum.CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_178"]/*' />
        NoRemainingAutomaticAssociations = Proto.Services.ResponseCodeEnum.NoRemainingAutomaticAssociations,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_179"]/*' />
        ExistingAutomaticAssociationsExceedGivenLimit = Proto.Services.ResponseCodeEnum.ExistingAutomaticAssociationsExceedGivenLimit,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_180"]/*' />
        RequestedNumAutomaticAssociationsExceedsAssociationLimit = Proto.Services.ResponseCodeEnum.RequestedNumAutomaticAssociationsExceedsAssociationLimit,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_181"]/*' />
        TokenIsPaused = Proto.Services.ResponseCodeEnum.TokenIsPaused,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_182"]/*' />
        TokenHasNoPauseKey = Proto.Services.ResponseCodeEnum.TokenHasNoPauseKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_183"]/*' />
        InvalidPauseKey = Proto.Services.ResponseCodeEnum.InvalidPauseKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_184"]/*' />
        FreezeUpdateFileDoesNotExist = Proto.Services.ResponseCodeEnum.FreezeUpdateFileDoesNotExist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_185"]/*' />
        FreezeUpdateFileHashDoesNotMatch = Proto.Services.ResponseCodeEnum.FreezeUpdateFileHashDoesNotMatch,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.the(consensus)"]/*' />
        NoUpgradeHasBeenPrepared = Proto.Services.ResponseCodeEnum.NoUpgradeHasBeenPrepared,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.the(consensus)_2"]/*' />
        NoFreezeIsScheduled = Proto.Services.ResponseCodeEnum.NoFreezeIsScheduled,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.the(consensus)_3"]/*' />
        UpdateFileHashChangedSincePrepareUpgrade = Proto.Services.ResponseCodeEnum.UpdateFileHashChangedSincePrepareUpgrade,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_186"]/*' />
        FreezeStartTimeMustBeFuture = Proto.Services.ResponseCodeEnum.FreezeStartTimeMustBeFuture,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_187"]/*' />
        PreparedUpdateFileIsImmutable = Proto.Services.ResponseCodeEnum.PreparedUpdateFileIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_188"]/*' />
        FreezeAlreadyScheduled = Proto.Services.ResponseCodeEnum.FreezeAlreadyScheduled,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_189"]/*' />
        FreezeUpgradeInProgress = Proto.Services.ResponseCodeEnum.FreezeUpgradeInProgress,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_190"]/*' />
        UpdateFileIdDoesNotMatchPrepared = Proto.Services.ResponseCodeEnum.UpdateFileIdDoesNotMatchPrepared,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_191"]/*' />
        UpdateFileHashDoesNotMatchPrepared = Proto.Services.ResponseCodeEnum.UpdateFileHashDoesNotMatchPrepared,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_5"]/*' />
        ConsensusGasExhausted = Proto.Services.ResponseCodeEnum.ConsensusGasExhausted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_6"]/*' />
        RevertedSuccess = Proto.Services.ResponseCodeEnum.RevertedSuccess,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_7"]/*' />
        MaxStorageInPriceRegimeHasBeenUsed = Proto.Services.ResponseCodeEnum.MaxStorageInPriceRegimeHasBeenUsed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_192"]/*' />
        InvalidAliasKey = Proto.Services.ResponseCodeEnum.InvalidAliasKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_193"]/*' />
        UnexpectedTokenDecimals = Proto.Services.ResponseCodeEnum.UnexpectedTokenDecimals,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_194"]/*' />
		[Obsolete]
		InvalidProxyAccountId = Proto.Services.ResponseCodeEnum.InvalidProxyAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_195"]/*' />
        InvalidTransferAccountId = Proto.Services.ResponseCodeEnum.InvalidTransferAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_196"]/*' />
        InvalidFeeCollectorAccountId = Proto.Services.ResponseCodeEnum.InvalidFeeCollectorAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_197"]/*' />
        AliasIsImmutable = Proto.Services.ResponseCodeEnum.AliasIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_198"]/*' />
        SpenderAccountSameAsOwner = Proto.Services.ResponseCodeEnum.SpenderAccountSameAsOwner,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_199"]/*' />
        AmountExceedsTokenMaxSupply = Proto.Services.ResponseCodeEnum.AmountExceedsTokenMaxSupply,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_200"]/*' />
        NegativeAllowanceAmount = Proto.Services.ResponseCodeEnum.NegativeAllowanceAmount,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_201"]/*' />
		[Obsolete]
		CannotApproveForAllFungibleCommon = Proto.Services.ResponseCodeEnum.CannotApproveForAllFungibleCommon,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_202"]/*' />
        SpenderDoesNotHaveAllowance = Proto.Services.ResponseCodeEnum.SpenderDoesNotHaveAllowance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_203"]/*' />
        AmountExceedsAllowance = Proto.Services.ResponseCodeEnum.AmountExceedsAllowance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_204"]/*' />
        MaxAllowancesExceeded = Proto.Services.ResponseCodeEnum.MaxAllowancesExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_205"]/*' />
        EmptyAllowances = Proto.Services.ResponseCodeEnum.EmptyAllowances,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_206"]/*' />
		[Obsolete]
		SpenderAccountRepeatedInAllowances = Proto.Services.ResponseCodeEnum.SpenderAccountRepeatedInAllowances,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_207"]/*' />
		[Obsolete]
		RepeatedSerialNumsInNftAllowances = Proto.Services.ResponseCodeEnum.RepeatedSerialNumsInNftAllowances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_208"]/*' />
        FungibleTokenInNftAllowances = Proto.Services.ResponseCodeEnum.FungibleTokenInNftAllowances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_209"]/*' />
        NftInFungibleTokenAllowances = Proto.Services.ResponseCodeEnum.NftInFungibleTokenAllowances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_210"]/*' />
        InvalidAllowanceOwnerId = Proto.Services.ResponseCodeEnum.InvalidAllowanceOwnerId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_211"]/*' />
        InvalidAllowanceSpenderId = Proto.Services.ResponseCodeEnum.InvalidAllowanceSpenderId,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_212"]/*' />
		[Obsolete]
		RepeatedAllowancesToDelete = Proto.Services.ResponseCodeEnum.RepeatedAllowancesToDelete,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_213"]/*' />
        InvalidDelegatingSpender = Proto.Services.ResponseCodeEnum.InvalidDelegatingSpender,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_214"]/*' />
        DelegatingSpenderCannotGrantApproveForAll = Proto.Services.ResponseCodeEnum.DelegatingSpenderCannotGrantApproveForAll,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_215"]/*' />
        DelegatingSpenderDoesNotHaveApproveForAll = Proto.Services.ResponseCodeEnum.DelegatingSpenderDoesNotHaveApproveForAll,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_216"]/*' />
        ScheduleExpirationTimeTooFarInFuture = Proto.Services.ResponseCodeEnum.ScheduleExpirationTimeTooFarInFuture,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_217"]/*' />
        ScheduleExpirationTimeMustBeHigherThanConsensusTime = Proto.Services.ResponseCodeEnum.ScheduleExpirationTimeMustBeHigherThanConsensusTime,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_218"]/*' />
        ScheduleFutureThrottleExceeded = Proto.Services.ResponseCodeEnum.ScheduleFutureThrottleExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_219"]/*' />
        ScheduleFutureGasLimitExceeded = Proto.Services.ResponseCodeEnum.ScheduleFutureGasLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_220"]/*' />
        InvalidEthereumTransaction = Proto.Services.ResponseCodeEnum.InvalidEthereumTransaction,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_221"]/*' />
        WrongChainId = Proto.Services.ResponseCodeEnum.WrongChainId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_222"]/*' />
        WrongNonce = Proto.Services.ResponseCodeEnum.WrongNonce,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_223"]/*' />
        AccessListUnsupported = Proto.Services.ResponseCodeEnum.AccessListUnsupported,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_224"]/*' />
        SchedulePendingExpiration = Proto.Services.ResponseCodeEnum.SchedulePendingExpiration,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_225"]/*' />
        ContractIsTokenTreasury = Proto.Services.ResponseCodeEnum.ContractIsTokenTreasury,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_226"]/*' />
        ContractHasNonZeroTokenBalances = Proto.Services.ResponseCodeEnum.ContractHasNonZeroTokenBalances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_227"]/*' />
        ContractExpiredAndPendingRemoval = Proto.Services.ResponseCodeEnum.ContractExpiredAndPendingRemoval,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_228"]/*' />
        ContractHasNoAutoRenewAccount = Proto.Services.ResponseCodeEnum.ContractHasNoAutoRenewAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_229"]/*' />
        PermanentRemovalRequiresSystemInitiation = Proto.Services.ResponseCodeEnum.PermanentRemovalRequiresSystemInitiation,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_230"]/*' />
        ProxyAccountIdFieldIsDeprecated = Proto.Services.ResponseCodeEnum.ProxyAccountIdFieldIsDeprecated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_231"]/*' />
        SelfStakingIsNotAllowed = Proto.Services.ResponseCodeEnum.SelfStakingIsNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_232"]/*' />
        InvalidStakingId = Proto.Services.ResponseCodeEnum.InvalidStakingId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)"]/*' />
        StakingNotEnabled = Proto.Services.ResponseCodeEnum.StakingNotEnabled,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_2"]/*' />
        InvalidPrngRange = Proto.Services.ResponseCodeEnum.InvalidPrngRange,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_3"]/*' />
        MaxEntitiesInPriceRegimeHaveBeenCreated = Proto.Services.ResponseCodeEnum.MaxEntitiesInPriceRegimeHaveBeenCreated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_4"]/*' />
        InvalidFullPrefixSignatureForPrecompile = Proto.Services.ResponseCodeEnum.InvalidFullPrefixSignatureForPrecompile,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_5"]/*' />
        InsufficientBalancesForStorageRent = Proto.Services.ResponseCodeEnum.InsufficientBalancesForStorageRent,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_6"]/*' />
        MaxChildRecordsExceeded = Proto.Services.ResponseCodeEnum.MaxChildRecordsExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_233"]/*' />
        InsufficientBalancesForRenewalFees = Proto.Services.ResponseCodeEnum.InsufficientBalancesForRenewalFees,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_8"]/*' />
        TransactionHasUnknownFields = Proto.Services.ResponseCodeEnum.TransactionHasUnknownFields,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_9"]/*' />
        AccountIsImmutable = Proto.Services.ResponseCodeEnum.AccountIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_10"]/*' />
        AliasAlreadyAssigned = Proto.Services.ResponseCodeEnum.AliasAlreadyAssigned,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_234"]/*' />
        InvalidMetadataKey = Proto.Services.ResponseCodeEnum.InvalidMetadataKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_235"]/*' />
        TokenHasNoMetadataKey = Proto.Services.ResponseCodeEnum.TokenHasNoMetadataKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_236"]/*' />
        MissingTokenMetadata = Proto.Services.ResponseCodeEnum.MissingTokenMetadata,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_237"]/*' />
        MissingSerialNumbers = Proto.Services.ResponseCodeEnum.MissingSerialNumbers,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_238"]/*' />
        TokenHasNoAdminKey = Proto.Services.ResponseCodeEnum.TokenHasNoAdminKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.name(DNS)"]/*' />
        NodeDeleted = Proto.Services.ResponseCodeEnum.NodeDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.name(DNS)_2"]/*' />
        InvalidNodeId = Proto.Services.ResponseCodeEnum.InvalidNodeId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_239"]/*' />
        InvalidGossipEndpoint = Proto.Services.ResponseCodeEnum.InvalidGossipEndpoint,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.name(DNS)_3"]/*' />
        InvalidNodeAccountId = Proto.Services.ResponseCodeEnum.InvalidNodeAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.name(DNS)_4"]/*' />
        InvalidNodeDescription = Proto.Services.ResponseCodeEnum.InvalidNodeDescription,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_240"]/*' />
        InvalidServiceEndpoint = Proto.Services.ResponseCodeEnum.InvalidServiceEndpoint,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_241"]/*' />
        InvalidGossipCaCertificate = Proto.Services.ResponseCodeEnum.InvalidGossipCaCertificate,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_242"]/*' />
        InvalidGrpcCertificate = Proto.Services.ResponseCodeEnum.InvalidGrpcCertificate,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_243"]/*' />
        InvalidMaxAutoAssociations = Proto.Services.ResponseCodeEnum.InvalidMaxAutoAssociations,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_244"]/*' />
        MaxNodesCreated = Proto.Services.ResponseCodeEnum.MaxNodesCreated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_245"]/*' />
        IpFqdnCannotBeSetForSameEndpoint = Proto.Services.ResponseCodeEnum.IpFqdnCannotBeSetForSameEndpoint,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_246"]/*' />
        GossipEndpointCannotHaveFqdn = Proto.Services.ResponseCodeEnum.GossipEndpointCannotHaveFqdn,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_247"]/*' />
        FqdnSizeTooLarge = Proto.Services.ResponseCodeEnum.FqdnSizeTooLarge,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_248"]/*' />
        InvalidEndpoint = Proto.Services.ResponseCodeEnum.InvalidEndpoint,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_249"]/*' />
        GossipEndpointsExceededLimit = Proto.Services.ResponseCodeEnum.GossipEndpointsExceededLimit,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_250"]/*' />
        TokenReferenceRepeated = Proto.Services.ResponseCodeEnum.TokenReferenceRepeated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_251"]/*' />
        InvalidOwnerId = Proto.Services.ResponseCodeEnum.InvalidOwnerId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_252"]/*' />
        TokenReferenceListSizeLimitExceeded = Proto.Services.ResponseCodeEnum.TokenReferenceListSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_253"]/*' />
        ServiceEndpointsExceededLimit = Proto.Services.ResponseCodeEnum.ServiceEndpointsExceededLimit,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_254"]/*' />
        InvalidIpv4Address = Proto.Services.ResponseCodeEnum.InvalidIpv4Address,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_255"]/*' />
        EmptyTokenReferenceList = Proto.Services.ResponseCodeEnum.EmptyTokenReferenceList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_256"]/*' />
        UpdateNodeAccountNotAllowed = Proto.Services.ResponseCodeEnum.UpdateNodeAccountNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_257"]/*' />
        TokenHasNoMetadataOrSupplyKey = Proto.Services.ResponseCodeEnum.TokenHasNoMetadataOrSupplyKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_258"]/*' />
        EmptyPendingAirdropIdList = Proto.Services.ResponseCodeEnum.EmptyPendingAirdropIdList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.airdrop(s)"]/*' />
        PendingAirdropIdRepeated = Proto.Services.ResponseCodeEnum.PendingAirdropIdRepeated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.airdrop(s)_2"]/*' />
        PendingAirdropIdListTooLong = Proto.Services.ResponseCodeEnum.PendingAirdropIdListTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.airdrop(s)_3"]/*' />
        PendingNftAirdropAlreadyExists = Proto.Services.ResponseCodeEnum.PendingNftAirdropAlreadyExists,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_259"]/*' />
        AccountHasPendingAirdrops = Proto.Services.ResponseCodeEnum.AccountHasPendingAirdrops,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_260"]/*' />
        ThrottledAtConsensus = Proto.Services.ResponseCodeEnum.ThrottledAtConsensus,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_261"]/*' />
        InvalidPendingAirdropId = Proto.Services.ResponseCodeEnum.InvalidPendingAirdropId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_262"]/*' />
        TokenAirdropWithFallbackRoyalty = Proto.Services.ResponseCodeEnum.TokenAirdropWithFallbackRoyalty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_263"]/*' />
        InvalidTokenInPendingAirdrop = Proto.Services.ResponseCodeEnum.InvalidTokenInPendingAirdrop,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_264"]/*' />
        ScheduleExpiryIsBusy = Proto.Services.ResponseCodeEnum.ScheduleExpiryIsBusy,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.times(is /// approximately four million times with typical network configuration.)"]/*' />
        InvalidGrpcCertificateHash = Proto.Services.ResponseCodeEnum.InvalidGrpcCertificateHash,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.times(is /// approximately four million times with typical network configuration.)_2"]/*' />
        MissingExpiryTime = Proto.Services.ResponseCodeEnum.MissingExpiryTime,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.times(is /// approximately four million times with typical network configuration.)_3"]/*' />
        NoSchedulingAllowedAfterScheduledRecursion = Proto.Services.ResponseCodeEnum.NoSchedulingAllowedAfterScheduledRecursion,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_265"]/*' />
        RecursiveSchedulingLimitReached = Proto.Services.ResponseCodeEnum.RecursiveSchedulingLimitReached,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_266"]/*' />
        WaitingForLedgerId = Proto.Services.ResponseCodeEnum.WaitingForLedgerId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_267"]/*' />
        MaxEntriesForFeeExemptKeyListExceeded = Proto.Services.ResponseCodeEnum.MaxEntriesForFeeExemptKeyListExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_268"]/*' />
        FeeExemptKeyListContainsDuplicatedKeys = Proto.Services.ResponseCodeEnum.FeeExemptKeyListContainsDuplicatedKeys,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_269"]/*' />
        InvalidKeyInFeeExemptKeyList = Proto.Services.ResponseCodeEnum.InvalidKeyInFeeExemptKeyList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_270"]/*' />
        InvalidFeeScheduleKey = Proto.Services.ResponseCodeEnum.InvalidFeeScheduleKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_271"]/*' />
        FeeScheduleKeyCannotBeUpdated = Proto.Services.ResponseCodeEnum.FeeScheduleKeyCannotBeUpdated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_272"]/*' />
        FeeScheduleKeyNotSet = Proto.Services.ResponseCodeEnum.FeeScheduleKeyNotSet,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_273"]/*' />
        MaxCustomFeeLimitExceeded = Proto.Services.ResponseCodeEnum.MaxCustomFeeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_274"]/*' />
        NoValidMaxCustomFee = Proto.Services.ResponseCodeEnum.NoValidMaxCustomFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_275"]/*' />
        InvalidMaxCustomFees = Proto.Services.ResponseCodeEnum.InvalidMaxCustomFees,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_276"]/*' />
        DuplicateDenominationInMaxCustomFeeList = Proto.Services.ResponseCodeEnum.DuplicateDenominationInMaxCustomFeeList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_277"]/*' />
        DuplicateAccountIdInMaxCustomFeeList = Proto.Services.ResponseCodeEnum.DuplicateAccountIdInMaxCustomFeeList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_278"]/*' />
        MaxCustomFeesIsNotSupported = Proto.Services.ResponseCodeEnum.MaxCustomFeesIsNotSupported,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_279"]/*' />
        BatchListEmpty = Proto.Services.ResponseCodeEnum.BatchListEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_280"]/*' />
        BatchListContainsDuplicates = Proto.Services.ResponseCodeEnum.BatchListContainsDuplicates,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_281"]/*' />
        BatchTransactionInBlacklist = Proto.Services.ResponseCodeEnum.BatchTransactionInBlacklist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_282"]/*' />
        InnerTransactionFailed = Proto.Services.ResponseCodeEnum.InnerTransactionFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_283"]/*' />
        MissingBatchKey = Proto.Services.ResponseCodeEnum.MissingBatchKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_284"]/*' />
        BatchKeySetOnNonInnerTransaction = Proto.Services.ResponseCodeEnum.BatchKeySetOnNonInnerTransaction,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_285"]/*' />
        InvalidBatchKey = Proto.Services.ResponseCodeEnum.InvalidBatchKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_286"]/*' />
        ScheduleExpiryNotConfigurable = Proto.Services.ResponseCodeEnum.ScheduleExpiryNotConfigurable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_287"]/*' />
        CreatingSystemEntities = Proto.Services.ResponseCodeEnum.CreatingSystemEntities,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_288"]/*' />
        ThrottleGroupLcmOverflow = Proto.Services.ResponseCodeEnum.ThrottleGroupLcmOverflow,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_289"]/*' />
        AirdropContainsMultipleSendersForAToken = Proto.Services.ResponseCodeEnum.AirdropContainsMultipleSendersForAToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_290"]/*' />
        GrpcWebProxyNotSupported = Proto.Services.ResponseCodeEnum.GrpcWebProxyNotSupported,
        HookIdInUse = Proto.Services.ResponseCodeEnum.HookIdInUse,
        HookIdRepeatedInCreationDetails = Proto.Services.ResponseCodeEnum.HookIdRepeatedInCreationDetails,
        HookNotFound = Proto.Services.ResponseCodeEnum.HookNotFound,
    }
}
