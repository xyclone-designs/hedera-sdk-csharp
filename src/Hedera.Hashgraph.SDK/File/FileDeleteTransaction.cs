// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.File
{
    /// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="T:FileDeleteTransaction"]/*' />
    public sealed class FileDeleteTransaction : Transaction<FileDeleteTransaction>
    {
        /// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="M:FileDeleteTransaction.#ctor"]/*' />
        public FileDeleteTransaction() { }
		/// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="M:FileDeleteTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal FileDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="M:FileDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal FileDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="M:FileDeleteTransaction.RequireNotFrozen"]/*' />
        public FileId? FileId 
        {
            get;
            set { RequireNotFrozen(); field = value; } 
        }

        /// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="M:FileDeleteTransaction.InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.FileDelete;

            if (body.FileID is not null)
            {
                FileId = FileId.FromProtobuf(body.FileID);
            }
        }

        /// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="M:FileDeleteTransaction.ToProtobuf"]/*' />
        public Proto.FileDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.FileDeleteTransactionBody();

            if (FileId != null)
				builder.FileID = FileId.ToProtobuf();

			return builder;
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.FileService.FileServiceClient.deleteFile);

			return Proto.FileService.Descriptor.FindMethodByName(methodname);
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

        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}