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
		/// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="M:FileDeleteTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal FileDeleteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="FileDeleteTransaction.cs.xml" path='docs/member[@name="M:FileDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal FileDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
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
        public Proto.Services.FileDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.FileDeleteTransactionBody();

            if (FileId != null)
				builder.FileID = FileId.ToProtobuf();

			return builder;
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.FileService.FileServiceClient.deleteFile);

			return Proto.Services.FileService.Descriptor.FindMethodByName(methodname);
		}

		public override void ValidateChecksums(Client client)
        {
			FileId?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.FileDelete = ToProtobuf();
        }

        public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
