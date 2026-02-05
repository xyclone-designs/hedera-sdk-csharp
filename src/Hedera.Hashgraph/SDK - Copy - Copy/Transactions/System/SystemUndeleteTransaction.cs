// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            ArgumentNullException.ThrowIfNull(fileId);
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
            ArgumentNullException.ThrowIfNull(contractId);
            RequireNotFrozen();
            contractId = contractId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.SystemUndelete;
            if (body.FileID is not null)
            {
                fileId = FileId.FromProtobuf(body.FileID);
            }

            if (body.ContractID is not null)
            {
                contractId = ContractId.FromProtobuf(body.ContractID);
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.SystemUndeleteTransactionBody}</returns>
        public Proto.SystemUndeleteTransactionBody Build()
        {
            var builder = new Proto.SystemUndeleteTransactionBody();
            if (fileId != null)
            {
                builder.FileID = fileId.ToProtobuf();
            }

            if (contractId != null)
            {
                builder.ContractID = contractId.ToProtobuf();
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
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
        public override Task OnExecuteAsync(Client client)
        {
            int modesEnabled = (fileId != null ? 1 : 0) + (contractId != null ? 1 : 0);
            if (modesEnabled != 1)
            {
                throw new InvalidOperationException("SystemDeleteTransaction must have exactly 1 of the following fields set: contractId, fileId");
            }
        }
        public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
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
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SystemUndelete = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.SystemUndelete = Build();
        }
    }
}