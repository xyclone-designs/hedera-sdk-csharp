// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Hedera.Hashgraph.SDK.File
{
    /// <summary>
    /// Update the metadata, and/or replace the content, of a file in the
    /// Hedera File Service (HFS).
    /// 
    /// Any field which is not set (i.e. is null) in this message, other than
    /// `fileID`, SHALL be ignored.<br/>
    /// If the `keys` list for the identified file is an empty `KeyList`, then
    /// this message MUST NOT set any field except `expirationTime`.
    /// 
    /// #### Signature Requirements
    /// Every `Key` in the `keys` list for the identified file MUST sign this
    /// transaction, if any field other than `expirationTime` is to be updated.<br/>
    /// If the `keys` list for the identified file is an empty `KeyList` (because
    /// this file was previously created or updated to have an empty `KeyList`),
    /// then the file is considered immutable and this message MUST NOT set any
    /// field except `expirationTime`.<br/>
    /// See the [File Service](#FileService) specification for a detailed
    /// explanation of the signature requirements for all file transactions.
    /// 
    /// ### Block Stream Effects
    /// None
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/file-storage/update-a-file">Hedera Documentation</a>
    /// </summary>
    public sealed class FileUpdateTransaction : Transaction<FileUpdateTransaction>
    {
		/// <summary>
		/// Constructor.
		/// </summary>
		public FileUpdateTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		public FileUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public FileUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// Set the ID of the file to update; required.
		/// </summary>
		public FileId? FileId
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// A short description of this file.
		/// <p>
		/// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
		/// (default 100) bytes when encoded as UTF-8.
		/// </summary>
		public string? FileMemo
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// The new list of keys that "own" this file.
		/// <p>
		/// If set, every key in this `KeyList` MUST sign this transaction.<br/>
		/// If set, every key in the _previous_ `KeyList` MUST _also_
		/// sign this transaction.<br/>
		/// If this value is an empty `KeyList`, then the file SHALL be immutable
		/// after completion of this transaction.
		/// </summary>
		public KeyList? Keys
		{
			get => field.CopyArray();
			set
			{
				RequireNotFrozen();
				field = value.CopyArray();
			}
		}

		/// <summary>
		/// Extract the files contents as a byte string.
		/// </summary>
		public ByteString? Contents
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = ByteString.CopyFrom(value?.ToByteArray());
			}
		}
		/// <summary>
		/// If set, replace contents of the file identified by {@link #setFileId(FileId)}
		/// with the given bytes.
		/// <p>
		/// If the contents of the file are longer than the given byte array, then the file will
		/// be truncated.
		/// <p>
		/// Note that total size for a given transaction is limited to 6KiB (as of March 2020) by the
		/// network; if you exceed this you may receive a {@link Status#TRANSACTION_OVERSIZE}.
		/// <p>
		/// In this case, you will need to keep the initial file contents under ~6KiB and
		/// then use {@link FileAppendTransaction}, which automatically breaks the contents
		/// into chunks for you, to append contents of arbitrary size.
		/// </summary>
		/// <param name="bytes">the bytes to replace the contents of the file with.</param>
		/// <returns>{@code this}</returns>
		/// <remarks>
		/// @see#setContents(String) for an overload which takes a String.
		/// @seeFileAppendTransaction if you merely want to add data to a file's existing contents.
		/// </remarks>
		public byte[]? Contents_Bytes
		{
			set
			{
				RequireNotFrozen();
				Contents = ByteString.CopyFrom(value);
			}
		}
		/// <summary>
		/// If set, encode the given {@link String} as UTF-8 and replace the contents of the file
		/// identified by {@link #setFileId(FileId)}.
		/// <p>
		/// If the contents of the file are longer than the UTF-8 encoding of the given string, then the
		/// file will be truncated.
		/// <p>
		/// The string can later be recovered from {@link FileContentsQuery#execute(Client)}
		/// via {@link String#String(byte[], java.nio.charset.Charset)} using
		/// {@link java.nio.charset.StandardCharsets#UTF_8}.
		/// <p>
		/// Note that total size for a given transaction is limited to 6KiB (as of March 2020) by the
		/// network; if you exceed this you may receive a  {@link Status#TRANSACTION_OVERSIZE}.
		/// <p>
		/// In this case, you will need to keep the initial file contents under ~6KiB and
		/// then use {@link FileAppendTransaction}, which automatically breaks the contents
		/// into chunks for you, to append contents of arbitrary size.
		/// </summary>
		/// <param name="text">the string to replace the contents of the file with.</param>
		/// <returns>{@code this}</returns>
		/// <remarks>
		/// @see#setContents(byte[]) for replacing the contents with arbitrary data.
		/// @seeFileAppendTransaction if you merely want to add data to a file's existing contents.
		/// </remarks>
		public string? Contents_String
		{
			set
			{
				RequireNotFrozen();
				Contents = ByteString.CopyFromUtf8(value);
			}
		}

		/// <summary>
		/// An expiration timestamp.
		/// <p>
		/// If set, this value MUST be strictly later than the existing
		/// `expirationTime` value, or else it will be ignored.<br/>
		/// If set, this value SHALL replace the existing `expirationTime`.<br/>
		/// If this field is the only field set, then this transaction SHALL NOT
		/// require any signature other than the `payer` for the transaction.<br/>
		/// When the network consensus time exceeds the then-current
		/// `expirationTime`, the network SHALL expire the file.
		/// </summary>
		public Timestamp? ExpirationTime
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		public Duration? ExpirationTimeDuration
		{
			get => field;
			set
			{
				RequireNotFrozen();
				field = value;
				ExpirationTime = null;
			}
		}

		/// <summary>
		/// Build the correct transaction body.
		/// </summary>
		/// <returns>{@link Proto.FileUpdateTransactionBody builder }</returns>
		public Proto.FileUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.FileUpdateTransactionBody
			{
				Contents = Contents
			};

            if (FileId != null)
				builder.FileID = FileId.ToProtobuf();

			if (Keys != null)
				builder.Keys = Keys.ToProtobuf();

			if (ExpirationTime != null)
				builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime);

            if (ExpirationTimeDuration != null)
				builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTimeDuration);

            if (FileMemo != null)
				builder.Memo = FileMemo;

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			FileId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileUpdate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.FileUpdate = ToProtobuf();
        }
		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return FileServiceGrpc.GetUpdateFileMethod();
		}

        public override void OnExecute(Client client)
        {
            throw new NotImplementedException();
        }
        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.FileUpdate;

			if (body.FileID is not null)
				FileId = FileId.FromProtobuf(body.FileID);

			if (body.Keys is not null)
				Keys = KeyList.FromProtobuf(body.Keys, null);

			if (body.ExpirationTime is not null)
				ExpirationTime = Utils.TimestampConverter.FromProtobuf(body.ExpirationTime);

			if (body.Memo is not null)
				FileMemo = body.Memo;

			Contents = body.Contents;
		}
	}
}