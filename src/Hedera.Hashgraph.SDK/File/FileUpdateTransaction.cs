// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.File
{
    /// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="T:FileUpdateTransaction"]/*' />
    public sealed class FileUpdateTransaction : Transaction<FileUpdateTransaction>
    {
		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.#ctor"]/*' />
		public FileUpdateTransaction() { }
		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal FileUpdateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal FileUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.RequireNotFrozen"]/*' />
		public FileId? FileId
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.RequireNotFrozen_2"]/*' />
		public string? FileMemo
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.RequireNotFrozen_3"]/*' />
		public KeyList? Keys
		{
			private get;
			set
			{
				RequireNotFrozen();
				field = value is null ? null : KeyList.Of(null, value);
			}
		}
		public IReadOnlyList<Key>? Keys_Read { get => Keys?.AsReadOnly(); }

		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.RequireNotFrozen_4"]/*' />
		public ByteString? Contents
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = ByteString.CopyFrom(value?.ToByteArray());
			}
		}
		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.RequireNotFrozen_5"]/*' />
		public byte[]? Contents_Bytes
		{
			set
			{
				RequireNotFrozen();
				Contents = ByteString.CopyFrom(value);
			}
		}
		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.RequireNotFrozen_6"]/*' />
		public string? Contents_String
		{
			set
			{
				RequireNotFrozen();
				Contents = ByteString.CopyFromUtf8(value);
			}
		}

		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.RequireNotFrozen_7"]/*' />
		public DateTimeOffset? ExpirationTime
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
				ExpirationTimeDuration = null;
			}
		}
		public TimeSpan? ExpirationTimeDuration
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
				ExpirationTime = null;
			}
		}

		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.ToProtobuf"]/*' />
		public Proto.Services.FileUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.FileUpdateTransactionBody
			{
				Contents = Contents
			};

            if (FileId != null)
				builder.FileId = FileId.ToProtobuf();

			if (Keys != null)
				builder.Keys = Keys.ToProtobuf();

			if (ExpirationTime != null)
				builder.ExpirationTime = ExpirationTime.Value.ToProtoTimestamp();

            if (ExpirationTimeDuration != null)
				builder.ExpirationTime = ExpirationTimeDuration.Value.ToProtoTimestamp();

            if (FileMemo != null)
				builder.Memo = FileMemo;

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			FileId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileUpdate = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.FileUpdate = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.FileService.FileServiceClient.updateFile);

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

		/// <include file="FileUpdateTransaction.cs.xml" path='docs/member[@name="M:FileUpdateTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.FileUpdate;

			if (body.FileId is not null)
				FileId = FileId.FromProtobuf(body.FileId);

			if (body.Keys is not null)
				Keys = KeyList.FromProtobuf(body.Keys, null);

			if (body.ExpirationTime is not null)
				ExpirationTime = body.ExpirationTime.ToDateTimeOffset();

			if (body.Memo is not null)
				FileMemo = body.Memo;

			Contents = body.Contents;
		}
	}
}
