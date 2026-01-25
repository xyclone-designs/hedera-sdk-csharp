// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Java.Util.Concurrent;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;

namespace Hedera.Hashgraph.SDK.Transactions.System
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// @deprecated
    /// This transaction is obsolete, not supported, and SHALL fail with a
    /// pre-check result of `NOT_SUPPORTED`.
    /// 
    /// Recover a file or contract bytecode deleted from the Hedera File
    /// System (HFS) by a `systemDelete` transaction.
    /// > Note
    /// >> A system delete/undelete for a `contractID` is not supported and
    /// >> SHALL return `INVALID_FILE_ID` or `MISSING_ENTITY_ID`.
    /// 
    /// This transaction can _only_ recover a file removed with the `systemDelete`
    /// transaction. A file deleted via `fileDelete` SHALL be irrecoverable.<br/>
    /// This transaction MUST be signed by an Hedera administrative ("system")
    /// account.
    /// 
    /// ### What is a "system" file
    /// A "system" file is any file with a file number less than or equal to the
    /// current configuration value for `ledger.numReservedSystemEntities`,
    /// typically `750`.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </remarks>
    [Obsolete("Obsolete")]
    public sealed class SystemUndeleteTransaction : Transaction<SystemUndeleteTransaction>
    {
        private FileId fileId;
        private ContractId contractId;
        /// <summary>
        /// Constructor.
        /// </summary>
        public SystemUndeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        SystemUndeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        SystemUndeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the file id.
        /// </summary>
        /// <returns>                         the file id</returns>
        public FileId GetFileId()
        {
            return fileId;
        }

        /// <summary>
        /// A file identifier.
        /// <p>
        /// The identified file MUST exist in the HFS.<br/>
        /// The identified file MUST be deleted.<br/>
        /// The identified file deletion MUST be a result of a
        /// `systemDelete` transaction.<br/>
        /// The identified file MUST NOT be a "system" file.<br/>
        /// This field is REQUIRED.
        /// 
        /// Mutually exclusive with {@link #setContractId(ContractId)}.
        /// </summary>
        /// <param name="fileId">The FileId to be set</param>
        /// <returns>{@code this}</returns>
        public SystemUndeleteTransaction SetFileId(FileId fileId)
        {
            Objects.RequireNonNull(fileId);
            RequireNotFrozen();
            fileId = fileId;
            return this;
        }

        /// <summary>
        /// The contract ID instance to undelete, in the format used in transactions
        /// </summary>
        /// <returns>the contractId</returns>
        public ContractId GetContractId()
        {
            return contractId;
        }

        /// <summary>
        /// A contract identifier.
        /// <p>
        /// The identified contract MUST exist in network state.<br/>
        /// The identified contract bytecode MUST be deleted.<br/>
        /// The identified contract deletion MUST be a result of a
        /// `systemDelete` transaction.
        /// <p>
        /// </summary>
        /// <param name="contractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public SystemUndeleteTransaction SetContractId(ContractId contractId)
        {
            Objects.RequireNonNull(contractId);
            RequireNotFrozen();
            contractId = contractId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetSystemUndelete();
            if (body.HasFileID())
            {
                fileId = FileId.FromProtobuf(body.GetFileID());
            }

            if (body.HasContractID())
            {
                contractId = ContractId.FromProtobuf(body.GetContractID());
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.SystemUndeleteTransactionBody}</returns>
        SystemUndeleteTransactionBody.Builder Build()
        {
            var builder = SystemUndeleteTransactionBody.NewBuilder();
            if (fileId != null)
            {
                builder.SetFileID(fileId.ToProtobuf());
            }

            if (contractId != null)
            {
                builder.SetContractID(contractId.ToProtobuf());
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (fileId != null)
            {
                fileId.ValidateChecksum(client);
            }

            if (contractId != null)
            {
                contractId.ValidateChecksum(client);
            }
        }

        override Task OnExecuteAsync(Client client)
        {
            int modesEnabled = (fileId != null ? 1 : 0) + (contractId != null ? 1 : 0);
            if (modesEnabled != 1)
            {
                throw new InvalidOperationException("SystemDeleteTransaction must have exactly 1 of the following fields set: contractId, fileId");
            }

            return base.OnExecuteAsync(client);
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            if (fileId != null)
            {
                return FileServiceGrpc.GetSystemUndeleteMethod();
            }
            else
            {
                return SmartContractServiceGrpc.GetSystemUndeleteMethod();
            }
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetSystemUndelete(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetSystemUndelete(Build());
        }
    }
}