// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Time;
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
    /// Delete a file or contract bytecode as an administrative transaction.
    /// > Note
    /// >> A system delete/undelete for a `contractID` is not supported and
    /// >> SHALL return `INVALID_FILE_ID` or `MISSING_ENTITY_ID`.
    /// 
    /// This transaction MAY be reversed by the `systemUndelete` transaction.
    /// A file deleted via `fileDelete`, however SHALL be irrecoverable.<br/>
    /// This transaction MUST specify an expiration timestamp (with seconds
    /// precision). The file SHALL be permanently removed from state when
    /// network consensus time exceeds the specified expiration time.<br/>
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
    public sealed class SystemDeleteTransaction : Transaction<SystemDeleteTransaction>
    {
        private FileId fileId = null;
        private ContractId contractId = null;
        private Timestamp expirationTime = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public SystemDeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        SystemDeleteTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        SystemDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// The identified file MUST NOT be deleted.<br/>
        /// The identified file MUST NOT be a "system" file.<br/>
        /// This field is REQUIRED.
        /// 
        /// Mutually exclusive with {@link #setContractId(ContractId)}.
        /// </summary>
        /// <param name="fileId">The FileId to be set</param>
        /// <returns>{@code this}</returns>
        public SystemDeleteTransaction SetFileId(FileId fileId)
        {
            Objects.RequireNonNull(fileId);
            RequireNotFrozen();
            fileId = fileId;
            contractId = null; // Reset contractId
            return this;
        }

        /// <summary>
        /// Extract the contract id.
        /// </summary>
        /// <returns>                         the contract id</returns>
        public ContractId GetContractId()
        {
            return contractId;
        }

        /// <summary>
        /// A contract identifier.
        /// <p>
        /// The identified contract MUST exist in network state.<br/>
        /// The identified contract bytecode MUST NOT be deleted.<br/>
        /// <p>
        /// Mutually exclusive with {@link #setFileId(FileId)}.
        /// </summary>
        /// <param name="contractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public SystemDeleteTransaction SetContractId(ContractId contractId)
        {
            Objects.RequireNonNull(contractId);
            RequireNotFrozen();
            contractId = contractId;
            fileId = null; // Reset fileId
            return this;
        }

        /// <summary>
        /// Extract the expiration time.
        /// </summary>
        /// <returns>                         the expiration time</returns>
        public Timestamp GetExpirationTime()
        {
            return expirationTime;
        }

        /// <summary>
        /// A timestamp indicating when the file will be removed from state.
        /// <p>
        /// This value SHALL be expressed in seconds since the `epoch`. The `epoch`
        /// SHALL be the UNIX epoch with 0 at `1970-01-01T00:00:00.000Z`.<br/>
        /// This field is REQUIRED.
        /// </summary>
        /// <param name="expirationTime">The Timestamp to be set as expiration time</param>
        /// <returns>{@code this}</returns>
        public SystemDeleteTransaction SetExpirationTime(Timestamp expirationTime)
        {
            Objects.RequireNonNull(expirationTime);
            RequireNotFrozen();
            expirationTime = expirationTime;
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.SystemDeleteTransactionBody}</returns>
        SystemDeleteTransactionBody.Builder Build()
        {
            var builder = SystemDeleteTransactionBody.NewBuilder();
            if (fileId != null)
            {
                builder.SetFileID(fileId.ToProtobuf());
            }

            if (contractId != null)
            {
                builder.SetContractID(contractId.ToProtobuf());
            }

            if (expirationTime != null)
            {
                builder.SetExpirationTime(Utils.TimestampConverter.ToSecondsProtobuf(expirationTime));
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetSystemDelete();
            if (body.HasFileID())
            {
                fileId = FileId.FromProtobuf(body.GetFileID());
            }

            if (body.HasContractID())
            {
                contractId = ContractId.FromProtobuf(body.GetContractID());
            }

            if (body.HasExpirationTime())
            {
                expirationTime = Utils.TimestampConverter.FromProtobuf(body.GetExpirationTime());
            }
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

        override CompletableFuture<Void> OnExecuteAsync(Client client)
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
                return FileServiceGrpc.GetSystemDeleteMethod();
            }
            else
            {
                return SmartContractServiceGrpc.GetSystemDeleteMethod();
            }
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetSystemDelete(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetSystemDelete(Build());
        }
    }
}