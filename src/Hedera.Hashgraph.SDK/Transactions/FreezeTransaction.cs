// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="FreezeTransaction.cs.xml" path='docs/member[@name="T:FreezeTransaction"]/*' />
    public sealed class FreezeTransaction : Transaction<FreezeTransaction>
    {
		/// <include file="FreezeTransaction.cs.xml" path='docs/member[@name="M:FreezeTransaction.#ctor"]/*' />
		public FreezeTransaction() { }
		internal FreezeTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		internal FreezeTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
		{
			InitFromTransactionBody();
		}

		public DateTimeOffset StartTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		} = DateTimeOffset.UnixEpoch;
		public FileId? FileId
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="FreezeTransaction.cs.xml" path='docs/member[@name="M:FreezeTransaction.CopyArray"]/*' />
		public byte[] FileHash
		{
			get => field.CopyArray();
			set
			{
				RequireNotFrozen();
				field = value.CopyArray();
			}
		} = [];
		/// <include file="FreezeTransaction.cs.xml" path='docs/member[@name="M:FreezeTransaction.RequireNotFrozen"]/*' />
		public FreezeType FreezeType
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		} = FreezeType.UnknownFreezeType;

		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.Freeze;

			FreezeType = (FreezeType)body.FreezeType;

			if (body.UpdateFile != null)
			{
				FileId = FileId.FromProtobuf(body.UpdateFile);
			}

			FileHash = body.FileHash.ToByteArray();

			if (body.StartTime != null)
			{
				StartTime = body.StartTime.ToDateTimeOffset();
			}
		}

		public Proto.Services.FreezeTransactionBody ToProtobuf()
		{
			var builder = new Proto.Services.FreezeTransactionBody
			{
				FreezeType = (Proto.Services.FreezeType)FreezeType,
				FileHash = ByteString.CopyFrom(FileHash)
			};

			if (FileId != null)
			{
				builder.UpdateFile = FileId.ToProtobuf();
			}

			if (StartTime != null)
			{
				builder.StartTime = StartTime.ToProtoTimestamp();
			}

			return builder;
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.FreezeService.FreezeServiceClient.freezeAsync);

			return Proto.Services.FreezeService.Descriptor.FindMethodByName(methodname);
		}
		public override void ValidateChecksums(Client client)
		{ }
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
		{
			bodyBuilder.Freeze = ToProtobuf();
		}
		public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
		{
			scheduled.Freeze = ToProtobuf();
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
