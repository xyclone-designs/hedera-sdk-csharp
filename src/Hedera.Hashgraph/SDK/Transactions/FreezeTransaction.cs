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
		internal FreezeTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		internal FreezeTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
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

		public Proto.FreezeTransactionBody ToProtobuf()
		{
			var builder = new Proto.FreezeTransactionBody
			{
				FreezeType = (Proto.FreezeType)FreezeType,
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
			string methodname = nameof(Proto.FreezeService.FreezeServiceClient.freezeAsync);

			return Proto.FreezeService.Descriptor.FindMethodByName(methodname);
		}
		public override void ValidateChecksums(Client client)
		{ }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
		{
			bodyBuilder.Freeze = ToProtobuf();
		}
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			scheduled.Freeze = ToProtobuf();
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