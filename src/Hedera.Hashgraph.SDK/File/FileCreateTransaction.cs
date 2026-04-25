// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Hedera.Hashgraph.SDK.File
{
    /// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="T:FileCreateTransaction"]/*' />
    public sealed class FileCreateTransaction : Transaction<FileCreateTransaction>
    {
        /// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.#ctor"]/*' />
        public FileCreateTransaction()
        {
            ExpirationTime = DateTimeOffset.UtcNow.Add(Transaction.DEFAULT_AUTO_RENEW_PERIOD);
            DefaultMaxTransactionFee = new Hbar(5);
        }
		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal FileCreateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		public FileCreateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.RequireNotFrozen"]/*' />
		public DateTimeOffset? ExpirationTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				ExpirationTimeDuration = null;
			}
		}
		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.RequireNotFrozen_2"]/*' />
		public TimeSpan? ExpirationTimeDuration
		{
			get;
			set
			{
				RequireNotFrozen();
                ExpirationTime = null;
				field = value;
			}
		}
		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.Of(null,value)"]/*' />
		public KeyList? Keys
        {
			private get;
            set => field = value is null ? null : KeyList.Of(null, value);
        }
		public IReadOnlyList<Key>? Keys_Read { get => Keys?.AsReadOnly(); }

		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.CopyArray"]/*' />
		public byte[] Contents 
        {
            get => field.CopyArray();
            set => field = value.CopyArray(); 
        } = [];
		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.GetString(Contents)"]/*' />
		public string Contents_String
		{
			get => Encoding.UTF8.GetString(Contents);
            set
			{
				RequireNotFrozen();
				Contents = Encoding.UTF8.GetBytes(value);
			}
		}
		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.CopyFrom(Contents)"]/*' />
		public ByteString Contents_ByteString
		{
			get => ByteString.CopyFrom(Contents);
            set
            {
                RequireNotFrozen();
				Contents = value.ToByteArray();
			}
		}
		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.RequireNotFrozen_3"]/*' />
		public string FileMemo
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		} = string.Empty;

		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.FileCreate;

			if (body.ExpirationTime is not null)
				ExpirationTime = body.ExpirationTime.ToDateTimeOffset();

			if (body.Keys is not null)
				Keys = KeyList.FromProtobuf(body.Keys, null);

			Contents = body.Contents.ToByteArray();
			FileMemo = body.Memo;
		}

		/// <include file="FileCreateTransaction.cs.xml" path='docs/member[@name="M:FileCreateTransaction.ToProtobuf"]/*' />
		public Proto.Services.FileCreateTransactionBody ToProtobuf()
		{
			var builder = new Proto.Services.FileCreateTransactionBody();

			if (ExpirationTime != null)
			{
				builder.ExpirationTime = ExpirationTime.Value.ToProtoTimestamp();
			}

			if (ExpirationTimeDuration != null)
			{
				builder.ExpirationTime = ExpirationTimeDuration.Value.ToProtoTimestamp();
			}

			if (Keys != null)
			{
				builder.Keys = Keys.ToProtobuf();
			}

			builder.Contents = ByteString.CopyFrom(Contents);
			builder.Memo = FileMemo;

			return builder;
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.FileService.FileServiceClient.createFile);

			return Proto.Services.FileService.Descriptor.FindMethodByName(methodname);
		}

		public override void ValidateChecksums(Client client) { }
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileCreate = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.FileCreate = ToProtobuf();
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
