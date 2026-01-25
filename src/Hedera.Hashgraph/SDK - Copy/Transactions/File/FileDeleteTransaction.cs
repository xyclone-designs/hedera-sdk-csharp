// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
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

namespace Hedera.Hashgraph.SDK.Transactions.File
{
    /// <summary>
    /// <p>A transaction to delete a file on the Hedera network.
    /// 
    /// <p>When deleted, a file's contents are truncated to zero length and it can no longer be updated
    /// or appended to, or its expiration time extended. {@link FileContentsQuery} and {@link FileInfoQuery}
    /// will throw {@link PrecheckStatusException} with a status of {@link Status#FILE_DELETED}.
    /// 
    /// <p>Only one of the file's keys needs to sign to delete the file, unless the key you have is part
    /// of a {@link KeyList}.
    /// </summary>
    public sealed class FileDeleteTransaction : Transaction<FileDeleteTransaction>
    {
        private FileId fileId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public FileDeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        FileDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        FileDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// A file identifier.<br/>
        /// This identifies the file to delete.
        /// <p>
        /// The identified file MUST NOT be a "system" file.<br/>
        /// This field is REQUIRED.
        /// </summary>
        /// <param name="fileId">the ID of the file to delete.</param>
        /// <returns>{@code this}</returns>
        public FileDeleteTransaction SetFileId(FileId fileId)
        {
            Objects.RequireNonNull(fileId);
            RequireNotFrozen();
            fileId = fileId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetFileDelete();
            if (body.HasFileID())
            {
                fileId = FileId.FromProtobuf(body.GetFileID());
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.FileDeleteTransactionBody builder}</returns>
        FileDeleteTransactionBody.Builder Build()
        {
            var builder = FileDeleteTransactionBody.NewBuilder();
            if (fileId != null)
            {
                builder.SetFileID(fileId.ToProtobuf());
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (fileId != null)
            {
                fileId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return FileServiceGrpc.GetDeleteFileMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetFileDelete(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetFileDelete(Build());
        }
    }
}