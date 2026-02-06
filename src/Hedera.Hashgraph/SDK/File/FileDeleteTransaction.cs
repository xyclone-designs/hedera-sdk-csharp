// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.File
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
        /// <summary>
        /// Constructor.
        /// </summary>
        public FileDeleteTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		public FileDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public FileDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A file identifier.<br/>
        /// This identifies the file to delete.
        /// <p>
        /// The identified file MUST NOT be a "system" file.<br/>
        /// This field is REQUIRED.
        /// </summary>
        public FileId? FileId 
        {
            get;
            set { RequireNotFrozen(); field = value; } 
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.FileDelete;

            if (body.FileID is not null)
            {
                FileId = FileId.FromProtobuf(body.FileID);
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.FileDeleteTransactionBody builder}</returns>
        public Proto.FileDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.FileDeleteTransactionBody();

            if (FileId != null)
				builder.FileID = FileId.ToProtobuf();

			return builder;
        }

		public override MethodDescriptor<Proto.Transaction, TransactionResponse<FileDeleteTransaction>> GetMethodDescriptor()
		{
			return FileServiceGrpc.DeleteFileMethod;
		}

		public override void ValidateChecksums(Client client)
        {
			FileId?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.FileDelete = ToProtobuf();
        }
    }
}