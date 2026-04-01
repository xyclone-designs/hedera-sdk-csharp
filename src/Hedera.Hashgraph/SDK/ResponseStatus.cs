// SPDX-License-Identifier: Apache-2.0

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus"]/*' />
    public enum ResponseStatus
    {
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_2"]/*' />
        Ok = Proto.ResponseCodeEnum.Ok,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_3"]/*' />
        InvalidTransaction = Proto.ResponseCodeEnum.InvalidTransaction,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_4"]/*' />
        PayerAccountNotFound = Proto.ResponseCodeEnum.PayerAccountNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_5"]/*' />
        InvalidNodeAccount = Proto.ResponseCodeEnum.InvalidNodeAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_6"]/*' />
        TransactionExpired = Proto.ResponseCodeEnum.TransactionExpired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_7"]/*' />
        InvalidTransactionStart = Proto.ResponseCodeEnum.InvalidTransactionStart,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_8"]/*' />
        InvalidTransactionDuration = Proto.ResponseCodeEnum.InvalidTransactionDuration,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_9"]/*' />
        InvalidSignature = Proto.ResponseCodeEnum.InvalidSignature,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.seconds(receipt)"]/*' />
        MemoTooLong = Proto.ResponseCodeEnum.MemoTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.seconds(receipt)_2"]/*' />
        InsufficientTxFee = Proto.ResponseCodeEnum.InsufficientTxFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.seconds(receipt)_3"]/*' />
        InsufficientPayerBalance = Proto.ResponseCodeEnum.InsufficientPayerBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_10"]/*' />
        DuplicateTransaction = Proto.ResponseCodeEnum.DuplicateTransaction,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_11"]/*' />
        Busy = Proto.ResponseCodeEnum.Busy,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_12"]/*' />
        NotSupported = Proto.ResponseCodeEnum.NotSupported,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_13"]/*' />
        InvalidFileId = Proto.ResponseCodeEnum.InvalidFileId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_14"]/*' />
        InvalidAccountId = Proto.ResponseCodeEnum.InvalidAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_15"]/*' />
        InvalidContractId = Proto.ResponseCodeEnum.InvalidContractId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_16"]/*' />
        InvalidTransactionId = Proto.ResponseCodeEnum.InvalidTransactionId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_17"]/*' />
        ReceiptNotFound = Proto.ResponseCodeEnum.ReceiptNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_18"]/*' />
        RecordNotFound = Proto.ResponseCodeEnum.RecordNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_19"]/*' />
        InvalidSolidityId = Proto.ResponseCodeEnum.InvalidSolidityId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_20"]/*' />
        Unknown = Proto.ResponseCodeEnum.Unknown,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_21"]/*' />
        Success = Proto.ResponseCodeEnum.Success,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_22"]/*' />
        FailInvalid = Proto.ResponseCodeEnum.FailInvalid,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_23"]/*' />
        FailFee = Proto.ResponseCodeEnum.FailFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_24"]/*' />
        FailBalance = Proto.ResponseCodeEnum.FailBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_25"]/*' />
        KeyRequired = Proto.ResponseCodeEnum.KeyRequired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_26"]/*' />
        BadEncoding = Proto.ResponseCodeEnum.BadEncoding,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.execution(query)"]/*' />
        InsufficientAccountBalance = Proto.ResponseCodeEnum.InsufficientAccountBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.execution(query)_2"]/*' />
        InvalidSolidityAddress = Proto.ResponseCodeEnum.InvalidSolidityAddress,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.execution(query)_3"]/*' />
        InsufficientGas = Proto.ResponseCodeEnum.InsufficientGas,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.execution(query)_4"]/*' />
        ContractSizeLimitExceeded = Proto.ResponseCodeEnum.ContractSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ve(amount)"]/*' />
        LocalCallModificationException = Proto.ResponseCodeEnum.LocalCallModificationException,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ve(amount)_2"]/*' />
        ContractRevertExecuted = Proto.ResponseCodeEnum.ContractRevertExecuted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ve(amount)_3"]/*' />
        ContractExecutionException = Proto.ResponseCodeEnum.ContractExecutionException,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_27"]/*' />
        InvalidReceivingNodeAccount = Proto.ResponseCodeEnum.InvalidReceivingNodeAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_28"]/*' />
        MissingQueryHeader = Proto.ResponseCodeEnum.MissingQueryHeader,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_29"]/*' />
        AccountUpdateFailed = Proto.ResponseCodeEnum.AccountUpdateFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_30"]/*' />
        InvalidKeyEncoding = Proto.ResponseCodeEnum.InvalidKeyEncoding,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_31"]/*' />
        NullSolidityAddress = Proto.ResponseCodeEnum.NullSolidityAddress,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_32"]/*' />
        ContractUpdateFailed = Proto.ResponseCodeEnum.ContractUpdateFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_33"]/*' />
        InvalidQueryHeader = Proto.ResponseCodeEnum.InvalidQueryHeader,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_34"]/*' />
        InvalidFeeSubmitted = Proto.ResponseCodeEnum.InvalidFeeSubmitted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_35"]/*' />
        InvalidPayerSignature = Proto.ResponseCodeEnum.InvalidPayerSignature,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_36"]/*' />
        KeyNotProvided = Proto.ResponseCodeEnum.KeyNotProvided,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_37"]/*' />
        InvalidExpirationTime = Proto.ResponseCodeEnum.InvalidExpirationTime,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_38"]/*' />
        NoWaclKey = Proto.ResponseCodeEnum.NoWaclKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(base ed25519,Keylist,or)"]/*' />
        FileContentEmpty = Proto.ResponseCodeEnum.FileContentEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(base ed25519,Keylist,or)_2"]/*' />
        InvalidAccountAmounts = Proto.ResponseCodeEnum.InvalidAccountAmounts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(base ed25519,Keylist,or)_3"]/*' />
        EmptyTransactionBody = Proto.ResponseCodeEnum.EmptyTransactionBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(base ed25519,Keylist,or)_4"]/*' />
        InvalidTransactionBody = Proto.ResponseCodeEnum.InvalidTransactionBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.key(Keylist,or)"]/*' />
        InvalidSignatureTypeMismatchingKey = Proto.ResponseCodeEnum.InvalidSignatureTypeMismatchingKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_39"]/*' />
        InvalidSignatureCountMismatchingKey = Proto.ResponseCodeEnum.InvalidSignatureCountMismatchingKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_40"]/*' />
        EmptyLiveHashBody = Proto.ResponseCodeEnum.EmptyLiveHashBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_41"]/*' />
        EmptyLiveHash = Proto.ResponseCodeEnum.EmptyLiveHash,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_42"]/*' />
        EmptyLiveHashKeys = Proto.ResponseCodeEnum.EmptyLiveHashKeys,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_43"]/*' />
        InvalidLiveHashSize = Proto.ResponseCodeEnum.InvalidLiveHashSize,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_44"]/*' />
        EmptyQueryBody = Proto.ResponseCodeEnum.EmptyQueryBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_45"]/*' />
        EmptyLiveHashQuery = Proto.ResponseCodeEnum.EmptyLiveHashQuery,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_46"]/*' />
        LiveHashNotFound = Proto.ResponseCodeEnum.LiveHashNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_47"]/*' />
        AccountIdDoesNotExist = Proto.ResponseCodeEnum.AccountIdDoesNotExist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_48"]/*' />
        LiveHashAlreadyExists = Proto.ResponseCodeEnum.LiveHashAlreadyExists,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_49"]/*' />
        InvalidFileWacl = Proto.ResponseCodeEnum.InvalidFileWacl,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_50"]/*' />
        SerializationFailed = Proto.ResponseCodeEnum.SerializationFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_51"]/*' />
        TransactionOversize = Proto.ResponseCodeEnum.TransactionOversize,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_52"]/*' />
        TransactionTooManyLayers = Proto.ResponseCodeEnum.TransactionTooManyLayers,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_53"]/*' />
        ContractDeleted = Proto.ResponseCodeEnum.ContractDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_54"]/*' />
        PlatformNotActive = Proto.ResponseCodeEnum.PlatformNotActive,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_55"]/*' />
        KeyPrefixMismatch = Proto.ResponseCodeEnum.KeyPrefixMismatch,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_56"]/*' />
        PlatformTransactionNotCreated = Proto.ResponseCodeEnum.PlatformTransactionNotCreated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_57"]/*' />
        InvalidRenewalPeriod = Proto.ResponseCodeEnum.InvalidRenewalPeriod,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_58"]/*' />
        InvalidPayerAccountId = Proto.ResponseCodeEnum.InvalidPayerAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_59"]/*' />
        AccountDeleted = Proto.ResponseCodeEnum.AccountDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_60"]/*' />
        FileDeleted = Proto.ResponseCodeEnum.FileDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_61"]/*' />
        AccountRepeatedInAccountAmounts = Proto.ResponseCodeEnum.AccountRepeatedInAccountAmounts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_62"]/*' />
        SettingNegativeAccountBalance = Proto.ResponseCodeEnum.SettingNegativeAccountBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.modify(update or delete a immutable smart,i.e. one created without a /// admin)"]/*' />
        ObtainerRequired = Proto.ResponseCodeEnum.ObtainerRequired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.modify(update or delete a immutable smart,i.e. one created without a /// admin)_2"]/*' />
        ObtainerSameContractId = Proto.ResponseCodeEnum.ObtainerSameContractId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.modify(update or delete a immutable smart,i.e. one created without a /// admin)_3"]/*' />
        ObtainerDoesNotExist = Proto.ResponseCodeEnum.ObtainerDoesNotExist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_63"]/*' />
        ModifyingImmutableContract = Proto.ResponseCodeEnum.ModifyingImmutableContract,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_64"]/*' />
        FileSystemException = Proto.ResponseCodeEnum.FileSystemException,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_65"]/*' />
        AutorenewDurationNotInRange = Proto.ResponseCodeEnum.AutorenewDurationNotInRange,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_66"]/*' />
        ErrorDecodingBytestring = Proto.ResponseCodeEnum.ErrorDecodingBytestring,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_67"]/*' />
        ContractFileEmpty = Proto.ResponseCodeEnum.ContractFileEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_68"]/*' />
        ContractBytecodeEmpty = Proto.ResponseCodeEnum.ContractBytecodeEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_69"]/*' />
        InvalidInitialBalance = Proto.ResponseCodeEnum.InvalidInitialBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_70"]/*' />
        [Obsolete]
        InvalidReceiveRecordThreshold = Proto.ResponseCodeEnum.InvalidReceiveRecordThreshold,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_71"]/*' />
		[Obsolete]
		InvalidSendRecordThreshold = Proto.ResponseCodeEnum.InvalidSendRecordThreshold,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.accounts(both from and)"]/*' />
        AccountIsNotGenesisAccount = Proto.ResponseCodeEnum.AccountIsNotGenesisAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.accounts(both from and)_2"]/*' />
        PayerAccountUnauthorized = Proto.ResponseCodeEnum.PayerAccountUnauthorized,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.accounts(both from and)_3"]/*' />
        InvalidFreezeTransactionBody = Proto.ResponseCodeEnum.InvalidFreezeTransactionBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.accounts(both from and)_4"]/*' />
        FreezeTransactionBodyNotFound = Proto.ResponseCodeEnum.FreezeTransactionBodyNotFound,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(account 0.0.)"]/*' />
        TransferListSizeLimitExceeded = Proto.ResponseCodeEnum.TransferListSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(account 0.0.)_2"]/*' />
        ResultSizeLimitExceeded = Proto.ResponseCodeEnum.ResultSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_72"]/*' />
        NotSpecialAccount = Proto.ResponseCodeEnum.NotSpecialAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_73"]/*' />
        ContractNegativeGas = Proto.ResponseCodeEnum.ContractNegativeGas,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_74"]/*' />
        ContractNegativeValue = Proto.ResponseCodeEnum.ContractNegativeValue,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_75"]/*' />
        InvalidFeeFile = Proto.ResponseCodeEnum.InvalidFeeFile,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_76"]/*' />
        InvalidExchangeRateFile = Proto.ResponseCodeEnum.InvalidExchangeRateFile,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_77"]/*' />
        InsufficientLocalCallGas = Proto.ResponseCodeEnum.InsufficientLocalCallGas,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_78"]/*' />
        EntityNotAllowedToDelete = Proto.ResponseCodeEnum.EntityNotAllowedToDelete,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.valid(append or update is)"]/*' />
        AuthorizationFailed = Proto.ResponseCodeEnum.AuthorizationFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.valid(append or update is)_2"]/*' />
        FileUploadedProtoInvalid = Proto.ResponseCodeEnum.FileUploadedProtoInvalid,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_79"]/*' />
        FileUploadedProtoNotSavedToDisk = Proto.ResponseCodeEnum.FileUploadedProtoNotSavedToDisk,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_80"]/*' />
        FeeScheduleFilePartUploaded = Proto.ResponseCodeEnum.FeeScheduleFilePartUploaded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_81"]/*' />
        ExchangeRateChangeLimitExceeded = Proto.ResponseCodeEnum.ExchangeRateChangeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_82"]/*' />
        MaxContractStorageExceeded = Proto.ResponseCodeEnum.MaxContractStorageExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_83"]/*' />
        TotalLedgerBalanceInvalid = Proto.ResponseCodeEnum.TotalLedgerBalanceInvalid,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_84"]/*' />
        ExpirationReductionNotAllowed = Proto.ResponseCodeEnum.ExpirationReductionNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)"]/*' />
        MaxGasLimitExceeded = Proto.ResponseCodeEnum.MaxGasLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_2"]/*' />
        MaxFileSizeExceeded = Proto.ResponseCodeEnum.MaxFileSizeExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_3"]/*' />
        ReceiverSigRequired = Proto.ResponseCodeEnum.ReceiverSigRequired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_4"]/*' />
        InvalidTopicId = Proto.ResponseCodeEnum.InvalidTopicId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.authorized(ie - a deleteTopic for a topic with no)"]/*' />
        InvalidAdminKey = Proto.ResponseCodeEnum.InvalidAdminKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.authorized(ie - a deleteTopic for a topic with no)_2"]/*' />
        InvalidSubmitKey = Proto.ResponseCodeEnum.InvalidSubmitKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_85"]/*' />
        Unauthorized = Proto.ResponseCodeEnum.Unauthorized,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_86"]/*' />
        InvalidTopicMessage = Proto.ResponseCodeEnum.InvalidTopicMessage,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.total(chunks)"]/*' />
        InvalidAutorenewAccount = Proto.ResponseCodeEnum.InvalidAutorenewAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.total(chunks)_2"]/*' />
        AutorenewAccountNotAllowed = Proto.ResponseCodeEnum.AutorenewAccountNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.total(chunks)_3"]/*' />
        TopicExpired = Proto.ResponseCodeEnum.TopicExpired,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_87"]/*' />
        InvalidChunkNumber = Proto.ResponseCodeEnum.InvalidChunkNumber,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_88"]/*' />
        InvalidChunkTransactionId = Proto.ResponseCodeEnum.InvalidChunkTransactionId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_89"]/*' />
        AccountFrozenForToken = Proto.ResponseCodeEnum.AccountFrozenForToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_90"]/*' />
        TokensPerAccountLimitExceeded = Proto.ResponseCodeEnum.TokensPerAccountLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_91"]/*' />
        InvalidTokenId = Proto.ResponseCodeEnum.InvalidTokenId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_92"]/*' />
        InvalidTokenDecimals = Proto.ResponseCodeEnum.InvalidTokenDecimals,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_93"]/*' />
        InvalidTokenInitialSupply = Proto.ResponseCodeEnum.InvalidTokenInitialSupply,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_94"]/*' />
        InvalidTreasuryAccountForToken = Proto.ResponseCodeEnum.InvalidTreasuryAccountForToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_95"]/*' />
        InvalidTokenSymbol = Proto.ResponseCodeEnum.InvalidTokenSymbol,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_96"]/*' />
        TokenHasNoFreezeKey = Proto.ResponseCodeEnum.TokenHasNoFreezeKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_97"]/*' />
        TransfersNotZeroSumForToken = Proto.ResponseCodeEnum.TransfersNotZeroSumForToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_98"]/*' />
        MissingTokenSymbol = Proto.ResponseCodeEnum.MissingTokenSymbol,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_99"]/*' />
        TokenSymbolTooLong = Proto.ResponseCodeEnum.TokenSymbolTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_100"]/*' />
        AccountKycNotGrantedForToken = Proto.ResponseCodeEnum.AccountKycNotGrantedForToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_101"]/*' />
        TokenHasNoKycKey = Proto.ResponseCodeEnum.TokenHasNoKycKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_102"]/*' />
        InsufficientTokenBalance = Proto.ResponseCodeEnum.InsufficientTokenBalance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_103"]/*' />
        TokenWasDeleted = Proto.ResponseCodeEnum.TokenWasDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_104"]/*' />
        TokenHasNoSupplyKey = Proto.ResponseCodeEnum.TokenHasNoSupplyKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_105"]/*' />
        TokenHasNoWipeKey = Proto.ResponseCodeEnum.TokenHasNoWipeKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_106"]/*' />
        InvalidTokenMintAmount = Proto.ResponseCodeEnum.InvalidTokenMintAmount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_107"]/*' />
        InvalidTokenBurnAmount = Proto.ResponseCodeEnum.InvalidTokenBurnAmount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_108"]/*' />
        TokenNotAssociatedToAccount = Proto.ResponseCodeEnum.TokenNotAssociatedToAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_109"]/*' />
        CannotWipeTokenTreasuryAccount = Proto.ResponseCodeEnum.CannotWipeTokenTreasuryAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_110"]/*' />
        InvalidKycKey = Proto.ResponseCodeEnum.InvalidKycKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_111"]/*' />
        InvalidWipeKey = Proto.ResponseCodeEnum.InvalidWipeKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_112"]/*' />
        InvalidFreezeKey = Proto.ResponseCodeEnum.InvalidFreezeKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_113"]/*' />
        InvalidSupplyKey = Proto.ResponseCodeEnum.InvalidSupplyKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_114"]/*' />
        MissingTokenName = Proto.ResponseCodeEnum.MissingTokenName,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_115"]/*' />
        TokenNameTooLong = Proto.ResponseCodeEnum.TokenNameTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_116"]/*' />
        InvalidWipingAmount = Proto.ResponseCodeEnum.InvalidWipingAmount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_117"]/*' />
        TokenIsImmutable = Proto.ResponseCodeEnum.TokenIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.transfers(both from and)"]/*' />
        TokenAlreadyAssociatedToAccount = Proto.ResponseCodeEnum.TokenAlreadyAssociatedToAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.transfers(both from and)_2"]/*' />
        TransactionRequiresZeroTokenBalances = Proto.ResponseCodeEnum.TransactionRequiresZeroTokenBalances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.transfers(both from and)_3"]/*' />
        AccountIsTreasury = Proto.ResponseCodeEnum.AccountIsTreasury,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.transfers(both from and)_4"]/*' />
        TokenIdRepeatedInTokenList = Proto.ResponseCodeEnum.TokenIdRepeatedInTokenList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_118"]/*' />
        TokenTransferListSizeLimitExceeded = Proto.ResponseCodeEnum.TokenTransferListSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_119"]/*' />
        EmptyTokenTransferBody = Proto.ResponseCodeEnum.EmptyTokenTransferBody,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_120"]/*' />
        EmptyTokenTransferAccountAmounts = Proto.ResponseCodeEnum.EmptyTokenTransferAccountAmounts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_121"]/*' />
        InvalidScheduleId = Proto.ResponseCodeEnum.InvalidScheduleId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_122"]/*' />
        ScheduleIsImmutable = Proto.ResponseCodeEnum.ScheduleIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_123"]/*' />
        InvalidSchedulePayerId = Proto.ResponseCodeEnum.InvalidSchedulePayerId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_124"]/*' />
        InvalidScheduleAccountId = Proto.ResponseCodeEnum.InvalidScheduleAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_125"]/*' />
        NoNewValidSignatures = Proto.ResponseCodeEnum.NoNewValidSignatures,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ScheduleCreate(that,all fields other than)"]/*' />
        UnresolvableRequiredSigners = Proto.ResponseCodeEnum.UnresolvableRequiredSigners,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ScheduleCreate(that,all fields other than)_2"]/*' />
        ScheduledTransactionNotInWhitelist = Proto.ResponseCodeEnum.ScheduledTransactionNotInWhitelist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ScheduleCreate(that,all fields other than)_3"]/*' />
        SomeSignaturesWereInvalid = Proto.ResponseCodeEnum.SomeSignaturesWereInvalid,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ScheduleCreate(that,all fields other than)_4"]/*' />
        TransactionIdFieldNotAllowed = Proto.ResponseCodeEnum.TransactionIdFieldNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_126"]/*' />
        IdenticalScheduleAlreadyCreated = Proto.ResponseCodeEnum.IdenticalScheduleAlreadyCreated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_127"]/*' />
        InvalidZeroByteInString = Proto.ResponseCodeEnum.InvalidZeroByteInString,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_128"]/*' />
        ScheduleAlreadyDeleted = Proto.ResponseCodeEnum.ScheduleAlreadyDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_129"]/*' />
        ScheduleAlreadyExecuted = Proto.ResponseCodeEnum.ScheduleAlreadyExecuted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_130"]/*' />
        MessageSizeTooLarge = Proto.ResponseCodeEnum.MessageSizeTooLarge,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_131"]/*' />
        OperationRepeatedInBucketGroups = Proto.ResponseCodeEnum.OperationRepeatedInBucketGroups,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_132"]/*' />
        BucketCapacityOverflow = Proto.ResponseCodeEnum.BucketCapacityOverflow,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_133"]/*' />
        NodeCapacityNotSufficientForOperation = Proto.ResponseCodeEnum.NodeCapacityNotSufficientForOperation,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_134"]/*' />
        BucketHasNoThrottleGroups = Proto.ResponseCodeEnum.BucketHasNoThrottleGroups,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_135"]/*' />
        ThrottleGroupHasZeroOpsPerSec = Proto.ResponseCodeEnum.ThrottleGroupHasZeroOpsPerSec,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_136"]/*' />
        SuccessButMissingExpectedOperation = Proto.ResponseCodeEnum.SuccessButMissingExpectedOperation,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_137"]/*' />
        UnparseableThrottleDefinitions = Proto.ResponseCodeEnum.UnparseableThrottleDefinitions,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_138"]/*' />
        InvalidThrottleDefinitions = Proto.ResponseCodeEnum.InvalidThrottleDefinitions,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_139"]/*' />
        AccountExpiredAndPendingRemoval = Proto.ResponseCodeEnum.AccountExpiredAndPendingRemoval,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_140"]/*' />
        InvalidTokenMaxSupply = Proto.ResponseCodeEnum.InvalidTokenMaxSupply,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_141"]/*' />
        InvalidTokenNftSerialNumber = Proto.ResponseCodeEnum.InvalidTokenNftSerialNumber,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_142"]/*' />
        InvalidNftId = Proto.ResponseCodeEnum.InvalidNftId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_143"]/*' />
        MetadataTooLong = Proto.ResponseCodeEnum.MetadataTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_144"]/*' />
        BatchSizeLimitExceeded = Proto.ResponseCodeEnum.BatchSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_145"]/*' />
        InvalidQueryRange = Proto.ResponseCodeEnum.InvalidQueryRange,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_146"]/*' />
        FractionDividesByZero = Proto.ResponseCodeEnum.FractionDividesByZero,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_147"]/*' />
		[Obsolete]
		InsufficientPayerBalanceForCustomFee = Proto.ResponseCodeEnum.InsufficientPayerBalanceForCustomFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_148"]/*' />
        CustomFeesListTooLong = Proto.ResponseCodeEnum.CustomFeesListTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_149"]/*' />
        InvalidCustomFeeCollector = Proto.ResponseCodeEnum.InvalidCustomFeeCollector,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_150"]/*' />
        InvalidTokenIdInCustomFees = Proto.ResponseCodeEnum.InvalidTokenIdInCustomFees,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_151"]/*' />
        TokenNotAssociatedToFeeCollector = Proto.ResponseCodeEnum.TokenNotAssociatedToFeeCollector,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_152"]/*' />
        TokenMaxSupplyReached = Proto.ResponseCodeEnum.TokenMaxSupplyReached,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_153"]/*' />
        SenderDoesNotOwnNftSerialNo = Proto.ResponseCodeEnum.SenderDoesNotOwnNftSerialNo,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_154"]/*' />
        CustomFeeNotFullySpecified = Proto.ResponseCodeEnum.CustomFeeNotFullySpecified,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_155"]/*' />
        CustomFeeMustBePositive = Proto.ResponseCodeEnum.CustomFeeMustBePositive,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_156"]/*' />
        TokenHasNoFeeScheduleKey = Proto.ResponseCodeEnum.TokenHasNoFeeScheduleKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_157"]/*' />
        CustomFeeOutsideNumericRange = Proto.ResponseCodeEnum.CustomFeeOutsideNumericRange,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_158"]/*' />
        RoyaltyFractionCannotExceedOne = Proto.ResponseCodeEnum.RoyaltyFractionCannotExceedOne,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_159"]/*' />
        FractionalFeeMaxAmountLessThanMinAmount = Proto.ResponseCodeEnum.FractionalFeeMaxAmountLessThanMinAmount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_160"]/*' />
        CustomScheduleAlreadyHasNoFees = Proto.ResponseCodeEnum.CustomScheduleAlreadyHasNoFees,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_161"]/*' />
        CustomFeeDenominationMustBeFungibleCommon = Proto.ResponseCodeEnum.CustomFeeDenominationMustBeFungibleCommon,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_162"]/*' />
        CustomFractionalFeeOnlyAllowedForFungibleCommon = Proto.ResponseCodeEnum.CustomFractionalFeeOnlyAllowedForFungibleCommon,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_163"]/*' />
        InvalidCustomFeeScheduleKey = Proto.ResponseCodeEnum.InvalidCustomFeeScheduleKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_164"]/*' />
        InvalidTokenMintMetadata = Proto.ResponseCodeEnum.InvalidTokenMintMetadata,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_165"]/*' />
        InvalidTokenBurnMetadata = Proto.ResponseCodeEnum.InvalidTokenBurnMetadata,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_166"]/*' />
        CurrentTreasuryStillOwnsNfts = Proto.ResponseCodeEnum.CurrentTreasuryStillOwnsNfts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_167"]/*' />
        AccountStillOwnsNfts = Proto.ResponseCodeEnum.AccountStillOwnsNfts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_168"]/*' />
        TreasuryMustOwnBurnedNft = Proto.ResponseCodeEnum.TreasuryMustOwnBurnedNft,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_169"]/*' />
        AccountDoesNotOwnWipedNft = Proto.ResponseCodeEnum.AccountDoesNotOwnWipedNft,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_170"]/*' />
        AccountAmountTransfersOnlyAllowedForFungibleCommon = Proto.ResponseCodeEnum.AccountAmountTransfersOnlyAllowedForFungibleCommon,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_171"]/*' />
        MaxNftsInPriceRegimeHaveBeenMinted = Proto.ResponseCodeEnum.MaxNftsInPriceRegimeHaveBeenMinted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_172"]/*' />
        PayerAccountDeleted = Proto.ResponseCodeEnum.PayerAccountDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_173"]/*' />
        CustomFeeChargingExceededMaxRecursionDepth = Proto.ResponseCodeEnum.CustomFeeChargingExceededMaxRecursionDepth,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_174"]/*' />
        CustomFeeChargingExceededMaxAccountAmounts = Proto.ResponseCodeEnum.CustomFeeChargingExceededMaxAccountAmounts,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_175"]/*' />
        InsufficientSenderAccountBalanceForCustomFee = Proto.ResponseCodeEnum.InsufficientSenderAccountBalanceForCustomFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_176"]/*' />
        SerialNumberLimitReached = Proto.ResponseCodeEnum.SerialNumberLimitReached,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_177"]/*' />
        CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique = Proto.ResponseCodeEnum.CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_178"]/*' />
        NoRemainingAutomaticAssociations = Proto.ResponseCodeEnum.NoRemainingAutomaticAssociations,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_179"]/*' />
        ExistingAutomaticAssociationsExceedGivenLimit = Proto.ResponseCodeEnum.ExistingAutomaticAssociationsExceedGivenLimit,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_180"]/*' />
        RequestedNumAutomaticAssociationsExceedsAssociationLimit = Proto.ResponseCodeEnum.RequestedNumAutomaticAssociationsExceedsAssociationLimit,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_181"]/*' />
        TokenIsPaused = Proto.ResponseCodeEnum.TokenIsPaused,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_182"]/*' />
        TokenHasNoPauseKey = Proto.ResponseCodeEnum.TokenHasNoPauseKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_183"]/*' />
        InvalidPauseKey = Proto.ResponseCodeEnum.InvalidPauseKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_184"]/*' />
        FreezeUpdateFileDoesNotExist = Proto.ResponseCodeEnum.FreezeUpdateFileDoesNotExist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_185"]/*' />
        FreezeUpdateFileHashDoesNotMatch = Proto.ResponseCodeEnum.FreezeUpdateFileHashDoesNotMatch,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.the(consensus)"]/*' />
        NoUpgradeHasBeenPrepared = Proto.ResponseCodeEnum.NoUpgradeHasBeenPrepared,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.the(consensus)_2"]/*' />
        NoFreezeIsScheduled = Proto.ResponseCodeEnum.NoFreezeIsScheduled,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.the(consensus)_3"]/*' />
        UpdateFileHashChangedSincePrepareUpgrade = Proto.ResponseCodeEnum.UpdateFileHashChangedSincePrepareUpgrade,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_186"]/*' />
        FreezeStartTimeMustBeFuture = Proto.ResponseCodeEnum.FreezeStartTimeMustBeFuture,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_187"]/*' />
        PreparedUpdateFileIsImmutable = Proto.ResponseCodeEnum.PreparedUpdateFileIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_188"]/*' />
        FreezeAlreadyScheduled = Proto.ResponseCodeEnum.FreezeAlreadyScheduled,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_189"]/*' />
        FreezeUpgradeInProgress = Proto.ResponseCodeEnum.FreezeUpgradeInProgress,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_190"]/*' />
        UpdateFileIdDoesNotMatchPrepared = Proto.ResponseCodeEnum.UpdateFileIdDoesNotMatchPrepared,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_191"]/*' />
        UpdateFileHashDoesNotMatchPrepared = Proto.ResponseCodeEnum.UpdateFileHashDoesNotMatchPrepared,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_5"]/*' />
        ConsensusGasExhausted = Proto.ResponseCodeEnum.ConsensusGasExhausted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_6"]/*' />
        RevertedSuccess = Proto.ResponseCodeEnum.RevertedSuccess,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_7"]/*' />
        MaxStorageInPriceRegimeHasBeenUsed = Proto.ResponseCodeEnum.MaxStorageInPriceRegimeHasBeenUsed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_192"]/*' />
        InvalidAliasKey = Proto.ResponseCodeEnum.InvalidAliasKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_193"]/*' />
        UnexpectedTokenDecimals = Proto.ResponseCodeEnum.UnexpectedTokenDecimals,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_194"]/*' />
		[Obsolete]
		InvalidProxyAccountId = Proto.ResponseCodeEnum.InvalidProxyAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_195"]/*' />
        InvalidTransferAccountId = Proto.ResponseCodeEnum.InvalidTransferAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_196"]/*' />
        InvalidFeeCollectorAccountId = Proto.ResponseCodeEnum.InvalidFeeCollectorAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_197"]/*' />
        AliasIsImmutable = Proto.ResponseCodeEnum.AliasIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_198"]/*' />
        SpenderAccountSameAsOwner = Proto.ResponseCodeEnum.SpenderAccountSameAsOwner,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_199"]/*' />
        AmountExceedsTokenMaxSupply = Proto.ResponseCodeEnum.AmountExceedsTokenMaxSupply,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_200"]/*' />
        NegativeAllowanceAmount = Proto.ResponseCodeEnum.NegativeAllowanceAmount,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_201"]/*' />
		[Obsolete]
		CannotApproveForAllFungibleCommon = Proto.ResponseCodeEnum.CannotApproveForAllFungibleCommon,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_202"]/*' />
        SpenderDoesNotHaveAllowance = Proto.ResponseCodeEnum.SpenderDoesNotHaveAllowance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_203"]/*' />
        AmountExceedsAllowance = Proto.ResponseCodeEnum.AmountExceedsAllowance,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_204"]/*' />
        MaxAllowancesExceeded = Proto.ResponseCodeEnum.MaxAllowancesExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_205"]/*' />
        EmptyAllowances = Proto.ResponseCodeEnum.EmptyAllowances,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_206"]/*' />
		[Obsolete]
		SpenderAccountRepeatedInAllowances = Proto.ResponseCodeEnum.SpenderAccountRepeatedInAllowances,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_207"]/*' />
		[Obsolete]
		RepeatedSerialNumsInNftAllowances = Proto.ResponseCodeEnum.RepeatedSerialNumsInNftAllowances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_208"]/*' />
        FungibleTokenInNftAllowances = Proto.ResponseCodeEnum.FungibleTokenInNftAllowances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_209"]/*' />
        NftInFungibleTokenAllowances = Proto.ResponseCodeEnum.NftInFungibleTokenAllowances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_210"]/*' />
        InvalidAllowanceOwnerId = Proto.ResponseCodeEnum.InvalidAllowanceOwnerId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_211"]/*' />
        InvalidAllowanceSpenderId = Proto.ResponseCodeEnum.InvalidAllowanceSpenderId,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_212"]/*' />
		[Obsolete]
		RepeatedAllowancesToDelete = Proto.ResponseCodeEnum.RepeatedAllowancesToDelete,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_213"]/*' />
        InvalidDelegatingSpender = Proto.ResponseCodeEnum.InvalidDelegatingSpender,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_214"]/*' />
        DelegatingSpenderCannotGrantApproveForAll = Proto.ResponseCodeEnum.DelegatingSpenderCannotGrantApproveForAll,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_215"]/*' />
        DelegatingSpenderDoesNotHaveApproveForAll = Proto.ResponseCodeEnum.DelegatingSpenderDoesNotHaveApproveForAll,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_216"]/*' />
        ScheduleExpirationTimeTooFarInFuture = Proto.ResponseCodeEnum.ScheduleExpirationTimeTooFarInFuture,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_217"]/*' />
        ScheduleExpirationTimeMustBeHigherThanConsensusTime = Proto.ResponseCodeEnum.ScheduleExpirationTimeMustBeHigherThanConsensusTime,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_218"]/*' />
        ScheduleFutureThrottleExceeded = Proto.ResponseCodeEnum.ScheduleFutureThrottleExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_219"]/*' />
        ScheduleFutureGasLimitExceeded = Proto.ResponseCodeEnum.ScheduleFutureGasLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_220"]/*' />
        InvalidEthereumTransaction = Proto.ResponseCodeEnum.InvalidEthereumTransaction,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_221"]/*' />
        WrongChainId = Proto.ResponseCodeEnum.WrongChainId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_222"]/*' />
        WrongNonce = Proto.ResponseCodeEnum.WrongNonce,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_223"]/*' />
        AccessListUnsupported = Proto.ResponseCodeEnum.AccessListUnsupported,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_224"]/*' />
        SchedulePendingExpiration = Proto.ResponseCodeEnum.SchedulePendingExpiration,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_225"]/*' />
        ContractIsTokenTreasury = Proto.ResponseCodeEnum.ContractIsTokenTreasury,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_226"]/*' />
        ContractHasNonZeroTokenBalances = Proto.ResponseCodeEnum.ContractHasNonZeroTokenBalances,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_227"]/*' />
        ContractExpiredAndPendingRemoval = Proto.ResponseCodeEnum.ContractExpiredAndPendingRemoval,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_228"]/*' />
        ContractHasNoAutoRenewAccount = Proto.ResponseCodeEnum.ContractHasNoAutoRenewAccount,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_229"]/*' />
        PermanentRemovalRequiresSystemInitiation = Proto.ResponseCodeEnum.PermanentRemovalRequiresSystemInitiation,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_230"]/*' />
        ProxyAccountIdFieldIsDeprecated = Proto.ResponseCodeEnum.ProxyAccountIdFieldIsDeprecated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_231"]/*' />
        SelfStakingIsNotAllowed = Proto.ResponseCodeEnum.SelfStakingIsNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_232"]/*' />
        InvalidStakingId = Proto.ResponseCodeEnum.InvalidStakingId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)"]/*' />
        StakingNotEnabled = Proto.ResponseCodeEnum.StakingNotEnabled,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_2"]/*' />
        InvalidPrngRange = Proto.ResponseCodeEnum.InvalidPrngRange,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_3"]/*' />
        MaxEntitiesInPriceRegimeHaveBeenCreated = Proto.ResponseCodeEnum.MaxEntitiesInPriceRegimeHaveBeenCreated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_4"]/*' />
        InvalidFullPrefixSignatureForPrecompile = Proto.ResponseCodeEnum.InvalidFullPrefixSignatureForPrecompile,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_5"]/*' />
        InsufficientBalancesForStorageRent = Proto.ResponseCodeEnum.InsufficientBalancesForStorageRent,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.account(if)_6"]/*' />
        MaxChildRecordsExceeded = Proto.ResponseCodeEnum.MaxChildRecordsExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_233"]/*' />
        InsufficientBalancesForRenewalFees = Proto.ResponseCodeEnum.InsufficientBalancesForRenewalFees,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_8"]/*' />
        TransactionHasUnknownFields = Proto.ResponseCodeEnum.TransactionHasUnknownFields,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_9"]/*' />
        AccountIsImmutable = Proto.ResponseCodeEnum.AccountIsImmutable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.ECDSA(secp256k1)_10"]/*' />
        AliasAlreadyAssigned = Proto.ResponseCodeEnum.AliasAlreadyAssigned,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_234"]/*' />
        InvalidMetadataKey = Proto.ResponseCodeEnum.InvalidMetadataKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_235"]/*' />
        TokenHasNoMetadataKey = Proto.ResponseCodeEnum.TokenHasNoMetadataKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_236"]/*' />
        MissingTokenMetadata = Proto.ResponseCodeEnum.MissingTokenMetadata,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_237"]/*' />
        MissingSerialNumbers = Proto.ResponseCodeEnum.MissingSerialNumbers,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_238"]/*' />
        TokenHasNoAdminKey = Proto.ResponseCodeEnum.TokenHasNoAdminKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.name(DNS)"]/*' />
        NodeDeleted = Proto.ResponseCodeEnum.NodeDeleted,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.name(DNS)_2"]/*' />
        InvalidNodeId = Proto.ResponseCodeEnum.InvalidNodeId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_239"]/*' />
        InvalidGossipEndpoint = Proto.ResponseCodeEnum.InvalidGossipEndpoint,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.name(DNS)_3"]/*' />
        InvalidNodeAccountId = Proto.ResponseCodeEnum.InvalidNodeAccountId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.name(DNS)_4"]/*' />
        InvalidNodeDescription = Proto.ResponseCodeEnum.InvalidNodeDescription,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_240"]/*' />
        InvalidServiceEndpoint = Proto.ResponseCodeEnum.InvalidServiceEndpoint,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_241"]/*' />
        InvalidGossipCaCertificate = Proto.ResponseCodeEnum.InvalidGossipCaCertificate,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_242"]/*' />
        InvalidGrpcCertificate = Proto.ResponseCodeEnum.InvalidGrpcCertificate,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_243"]/*' />
        InvalidMaxAutoAssociations = Proto.ResponseCodeEnum.InvalidMaxAutoAssociations,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_244"]/*' />
        MaxNodesCreated = Proto.ResponseCodeEnum.MaxNodesCreated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_245"]/*' />
        IpFqdnCannotBeSetForSameEndpoint = Proto.ResponseCodeEnum.IpFqdnCannotBeSetForSameEndpoint,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_246"]/*' />
        GossipEndpointCannotHaveFqdn = Proto.ResponseCodeEnum.GossipEndpointCannotHaveFqdn,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_247"]/*' />
        FqdnSizeTooLarge = Proto.ResponseCodeEnum.FqdnSizeTooLarge,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_248"]/*' />
        InvalidEndpoint = Proto.ResponseCodeEnum.InvalidEndpoint,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_249"]/*' />
        GossipEndpointsExceededLimit = Proto.ResponseCodeEnum.GossipEndpointsExceededLimit,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_250"]/*' />
        TokenReferenceRepeated = Proto.ResponseCodeEnum.TokenReferenceRepeated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_251"]/*' />
        InvalidOwnerId = Proto.ResponseCodeEnum.InvalidOwnerId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_252"]/*' />
        TokenReferenceListSizeLimitExceeded = Proto.ResponseCodeEnum.TokenReferenceListSizeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_253"]/*' />
        ServiceEndpointsExceededLimit = Proto.ResponseCodeEnum.ServiceEndpointsExceededLimit,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_254"]/*' />
        InvalidIpv4Address = Proto.ResponseCodeEnum.InvalidIpv4Address,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_255"]/*' />
        EmptyTokenReferenceList = Proto.ResponseCodeEnum.EmptyTokenReferenceList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_256"]/*' />
        UpdateNodeAccountNotAllowed = Proto.ResponseCodeEnum.UpdateNodeAccountNotAllowed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_257"]/*' />
        TokenHasNoMetadataOrSupplyKey = Proto.ResponseCodeEnum.TokenHasNoMetadataOrSupplyKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_258"]/*' />
        EmptyPendingAirdropIdList = Proto.ResponseCodeEnum.EmptyPendingAirdropIdList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.airdrop(s)"]/*' />
        PendingAirdropIdRepeated = Proto.ResponseCodeEnum.PendingAirdropIdRepeated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.airdrop(s)_2"]/*' />
        PendingAirdropIdListTooLong = Proto.ResponseCodeEnum.PendingAirdropIdListTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.airdrop(s)_3"]/*' />
        PendingNftAirdropAlreadyExists = Proto.ResponseCodeEnum.PendingNftAirdropAlreadyExists,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_259"]/*' />
        AccountHasPendingAirdrops = Proto.ResponseCodeEnum.AccountHasPendingAirdrops,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_260"]/*' />
        ThrottledAtConsensus = Proto.ResponseCodeEnum.ThrottledAtConsensus,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_261"]/*' />
        InvalidPendingAirdropId = Proto.ResponseCodeEnum.InvalidPendingAirdropId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_262"]/*' />
        TokenAirdropWithFallbackRoyalty = Proto.ResponseCodeEnum.TokenAirdropWithFallbackRoyalty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_263"]/*' />
        InvalidTokenInPendingAirdrop = Proto.ResponseCodeEnum.InvalidTokenInPendingAirdrop,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_264"]/*' />
        ScheduleExpiryIsBusy = Proto.ResponseCodeEnum.ScheduleExpiryIsBusy,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.times(is /// approximately four million times with typical network configuration.)"]/*' />
        InvalidGrpcCertificateHash = Proto.ResponseCodeEnum.InvalidGrpcCertificateHash,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.times(is /// approximately four million times with typical network configuration.)_2"]/*' />
        MissingExpiryTime = Proto.ResponseCodeEnum.MissingExpiryTime,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.times(is /// approximately four million times with typical network configuration.)_3"]/*' />
        NoSchedulingAllowedAfterScheduledRecursion = Proto.ResponseCodeEnum.NoSchedulingAllowedAfterScheduledRecursion,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_265"]/*' />
        RecursiveSchedulingLimitReached = Proto.ResponseCodeEnum.RecursiveSchedulingLimitReached,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_266"]/*' />
        WaitingForLedgerId = Proto.ResponseCodeEnum.WaitingForLedgerId,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_267"]/*' />
        MaxEntriesForFeeExemptKeyListExceeded = Proto.ResponseCodeEnum.MaxEntriesForFeeExemptKeyListExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_268"]/*' />
        FeeExemptKeyListContainsDuplicatedKeys = Proto.ResponseCodeEnum.FeeExemptKeyListContainsDuplicatedKeys,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_269"]/*' />
        InvalidKeyInFeeExemptKeyList = Proto.ResponseCodeEnum.InvalidKeyInFeeExemptKeyList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_270"]/*' />
        InvalidFeeScheduleKey = Proto.ResponseCodeEnum.InvalidFeeScheduleKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_271"]/*' />
        FeeScheduleKeyCannotBeUpdated = Proto.ResponseCodeEnum.FeeScheduleKeyCannotBeUpdated,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_272"]/*' />
        FeeScheduleKeyNotSet = Proto.ResponseCodeEnum.FeeScheduleKeyNotSet,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_273"]/*' />
        MaxCustomFeeLimitExceeded = Proto.ResponseCodeEnum.MaxCustomFeeLimitExceeded,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_274"]/*' />
        NoValidMaxCustomFee = Proto.ResponseCodeEnum.NoValidMaxCustomFee,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_275"]/*' />
        InvalidMaxCustomFees = Proto.ResponseCodeEnum.InvalidMaxCustomFees,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_276"]/*' />
        DuplicateDenominationInMaxCustomFeeList = Proto.ResponseCodeEnum.DuplicateDenominationInMaxCustomFeeList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_277"]/*' />
        DuplicateAccountIdInMaxCustomFeeList = Proto.ResponseCodeEnum.DuplicateAccountIdInMaxCustomFeeList,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_278"]/*' />
        MaxCustomFeesIsNotSupported = Proto.ResponseCodeEnum.MaxCustomFeesIsNotSupported,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_279"]/*' />
        BatchListEmpty = Proto.ResponseCodeEnum.BatchListEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_280"]/*' />
        BatchListContainsDuplicates = Proto.ResponseCodeEnum.BatchListContainsDuplicates,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_281"]/*' />
        BatchTransactionInBlacklist = Proto.ResponseCodeEnum.BatchTransactionInBlacklist,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_282"]/*' />
        InnerTransactionFailed = Proto.ResponseCodeEnum.InnerTransactionFailed,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_283"]/*' />
        MissingBatchKey = Proto.ResponseCodeEnum.MissingBatchKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_284"]/*' />
        BatchKeySetOnNonInnerTransaction = Proto.ResponseCodeEnum.BatchKeySetOnNonInnerTransaction,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_285"]/*' />
        InvalidBatchKey = Proto.ResponseCodeEnum.InvalidBatchKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_286"]/*' />
        ScheduleExpiryNotConfigurable = Proto.ResponseCodeEnum.ScheduleExpiryNotConfigurable,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_287"]/*' />
        CreatingSystemEntities = Proto.ResponseCodeEnum.CreatingSystemEntities,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_288"]/*' />
        ThrottleGroupLcmOverflow = Proto.ResponseCodeEnum.ThrottleGroupLcmOverflow,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_289"]/*' />
        AirdropContainsMultipleSendersForAToken = Proto.ResponseCodeEnum.AirdropContainsMultipleSendersForAToken,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_290"]/*' />
        GrpcWebProxyNotSupported = Proto.ResponseCodeEnum.GrpcWebProxyNotSupported,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_291"]/*' />
        NftTransfersOnlyAllowedForNonFungibleUnique = Proto.ResponseCodeEnum.NftTransfersOnlyAllowedForNonFungibleUnique,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_292"]/*' />
        InvalidSerializedTxMessageHashAlgorithm = Proto.ResponseCodeEnum.InvalidSerializedTxMessageHashAlgorithm,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_293"]/*' />
        EvmHookGasThrottled = Proto.ResponseCodeEnum.EvmHookGasThrottled,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_294"]/*' />
        HookIdInUse = Proto.ResponseCodeEnum.HookIdInUse,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_295"]/*' />
        BadHookRequest = Proto.ResponseCodeEnum.BadHookRequest,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.representation(i.e.,no leading)"]/*' />
        RejectedByAccountAllowanceHook = Proto.ResponseCodeEnum.RejectedByAccountAllowanceHook,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.representation(i.e.,no leading)_2"]/*' />
        HookNotFound = Proto.ResponseCodeEnum.HookNotFound,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.representation(i.e.,no leading)_3"]/*' />
		LambdaStorageUpdateBytesTooLong = Proto.ResponseCodeEnum.LambdaStorageUpdateBytesTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_296"]/*' />
        LambdaStorageUpdateBytesMustUseMinimalRepresentation = Proto.ResponseCodeEnum.LambdaStorageUpdateBytesMustUseMinimalRepresentation,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_297"]/*' />
        InvalidHookId = Proto.ResponseCodeEnum.InvalidHookId,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_298"]/*' />
		EmptyLambdaStorageUpdate = Proto.ResponseCodeEnum.EmptyLambdaStorageUpdate,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_299"]/*' />
        HookIdRepeatedInCreationDetails = Proto.ResponseCodeEnum.HookIdRepeatedInCreationDetails,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_300"]/*' />
        HooksNotEnabled = Proto.ResponseCodeEnum.HooksNotEnabled,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.representation(i.e.,no leading)_4"]/*' />
        HookIsNotALambda = Proto.ResponseCodeEnum.HookIsNotALambda,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.representation(i.e.,no leading)_5"]/*' />
        HookDeleted = Proto.ResponseCodeEnum.HookDeleted,
		/// <include file="ResponseStatus.cs.xml" path='docs/member[@name="M:ResponseStatus.representation(i.e.,no leading)_6"]/*' />
		TooManyLambdaStorageUpdates = Proto.ResponseCodeEnum.TooManyLambdaStorageUpdates,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_301"]/*' />
        HookCreationBytesMustUseMinimalRepresentation = Proto.ResponseCodeEnum.HookCreationBytesMustUseMinimalRepresentation,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_302"]/*' />
        HookCreationBytesTooLong = Proto.ResponseCodeEnum.HookCreationBytesTooLong,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_303"]/*' />
        InvalidHookCreationSpec = Proto.ResponseCodeEnum.InvalidHookCreationSpec,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_304"]/*' />
        HookExtensionEmpty = Proto.ResponseCodeEnum.HookExtensionEmpty,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_305"]/*' />
        InvalidHookAdminKey = Proto.ResponseCodeEnum.InvalidHookAdminKey,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_306"]/*' />
        HookDeletionRequiresZeroStorageSlots = Proto.ResponseCodeEnum.HookDeletionRequiresZeroStorageSlots,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_307"]/*' />
        CannotSetHooksAndApproval = Proto.ResponseCodeEnum.CannotSetHooksAndApproval,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_308"]/*' />
        TransactionRequiresZeroHooks = Proto.ResponseCodeEnum.TransactionRequiresZeroHooks,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_309"]/*' />
        InvalidHookCall = Proto.ResponseCodeEnum.InvalidHookCall,
        /// <include file="ResponseStatus.cs.xml" path='docs/member[@name="T:ResponseStatus_310"]/*' />
        HooksAreNotSupportedInAirdrops = Proto.ResponseCodeEnum.HooksAreNotSupportedInAirdrops
    }
}