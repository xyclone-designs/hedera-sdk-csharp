// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.File
{
    /// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="T:FileAppendTransaction"]/*' />
    public sealed class FileAppendTransaction : ChunkedTransaction<FileAppendTransaction>
    {
        public static readonly int DEFAULT_CHUNK_SIZE = 4096;

        /// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="M:FileAppendTransaction.#ctor"]/*' />
        public FileAppendTransaction() : base()
        {
            DefaultMaxTransactionFee = new Hbar(5);
            ChunkSize = 2048;
        }
		/// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="M:FileAppendTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal FileAppendTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="M:FileAppendTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal FileAppendTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="M:FileAppendTransaction.RequireNotFrozen"]/*' />
        public FileId? FileId
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }
		/// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="T:FileAppendTransaction_2"]/*' />
		public ByteString Contents
        {
            get => Data;
            set => Data = value;
		}
		/// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="M:FileAppendTransaction.CopyFrom(value)"]/*' />
		public byte[] Contents_Bytes
		{
			set => Data = ByteString.CopyFrom(value);
		}
		/// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="M:FileAppendTransaction.CopyFromUtf8(value)"]/*' />
		public string Contents_String
		{
			set => Data = ByteString.CopyFromUtf8(value);
		}

		/// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="M:FileAppendTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.FileAppend;
			if (body.FileId is not null)
			{
				FileId = FileId.FromProtobuf(body.FileId);
			}

			if (InnerSignedTransactions.Count > 0)
			{
				try
				{
					for (var i = 0; i < InnerSignedTransactions.Count; i += NodeAccountIds.Count == 0 ? 1 : NodeAccountIds.Count)
					{
						Data = Data.Concat(Proto.Services.TransactionBody.Parser.ParseFrom(InnerSignedTransactions[i].BodyBytes).FileAppend.Contents);
					}
				}
				catch (InvalidProtocolBufferException exc)
				{
					throw new ArgumentException(exc.Message);
				}
			}
			else
			{
				Data = body.Contents;
			}
		}

		/// <include file="FileAppendTransaction.cs.xml" path='docs/member[@name="M:FileAppendTransaction.ToProtobuf"]/*' />
		public Proto.Services.FileAppendTransactionBody ToProtobuf()
		{
			var builder = new Proto.Services.FileAppendTransactionBody
			{
				Contents = Data
			};

            if (FileId != null)
				builder.FileId = FileId.ToProtobuf();

            return builder;
        }

		public override bool ShouldGetReceipt()
        {
            return true;
        }
		public override void ValidateChecksums(Client client)
		{
			FileId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileAppend = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.FileAppend = ToProtobuf();
            scheduled.FileAppend.Contents = Data;
        }
        public override void OnFreezeChunk(Proto.Services.TransactionBody body, Proto.Services.TransactionID? initialTransactionId, int startIndex, int endIndex, int chunk, int total)
        {
			body.FileAppend = ToProtobuf();
			body.FileAppend.Contents = ByteString.CopyFrom(Data.Span[startIndex..endIndex]);
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.FileService.FileServiceClient.appendContent);

			return Proto.Services.FileService.Descriptor.FindMethodByName(methodname);
		}

		public override void OnExecute(Client client)
        {
            throw new NotImplementedException();
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
