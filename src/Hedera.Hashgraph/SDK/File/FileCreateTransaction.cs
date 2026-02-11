// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Hedera.Hashgraph.SDK.File
{
    /// <summary>
    /// Create a new file.
    /// 
    /// If successful, the new file SHALL contain the (possibly empty) content
    /// provided in the `contents` field.<br/>
    /// When the current consensus time exceeds the `expirationTime` value, the
    /// network SHALL expire the file, and MAY archive the state entry.
    /// 
    /// #### Signature Requirements
    /// The HFS manages file authorization in a manner that can be confusing.
    /// The core element of file authorization is the `keys` field,
    /// which is a `KeyList`; a list of individual `Key` messages, each of which
    /// may represent a simple or complex key.<br/>
    /// The file service transactions treat this list differently.<br/>
    /// A `fileCreate`, `fileAppend`, or `fileUpdate` MUST have a valid signature
    /// from _each_ key in the list.<br/>
    /// A `fileDelete` MUST have a valid signature from _at least one_ key in
    /// the list. This is different, and allows a file "owned" by many entities
    /// to be deleted by any one of those entities. A deleted file cannot be
    /// restored, so it is important to consider this when assigning keys for
    /// a file.<br/>
    /// If any of the keys in a `KeyList` are complex, the full requirements of
    /// each complex key must be met to count as a "valid signature" for that key.
    /// A complex key structure (i.e. a `ThresholdKey`, or `KeyList`, possibly
    /// including additional `ThresholdKey` or `KeyList` descendants) may be
    /// assigned as the sole entry in a file `keys` field to ensure all transactions
    /// have the same signature requirements.
    /// 
    /// If the `keys` field is an empty `KeyList`, then the file SHALL be immutable
    /// and the only transaction permitted to modify that file SHALL be a
    /// `fileUpdate` transaction with _only_ the `expirationTime` set.
    /// 
    /// #### Shard and Realm
    /// The current API ignores shardID and realmID. All files are created in
    /// shard 0 and realm 0. Future versions of the API may support multiple
    /// realms and multiple shards.
    /// 
    /// ### Block Stream Effects
    /// After the file is created, the FileID for it SHALL be returned in the
    /// transaction receipt, and SHALL be recorded in the transaction record.
    /// </summary>
    public sealed class FileCreateTransaction : Transaction<FileCreateTransaction>
    {
        private KeyList? keys = null;

        /// <summary>
        /// Constructor.
        /// </summary
        public FileCreateTransaction()
        {
            ExpirationTime = Timestamp.FromDateTime(DateTime.UtcNow.Add(DEFAULT_AUTO_RENEW_PERIOD.ToTimeSpan()));
            DefaultMaxTransactionFee = new Hbar(5);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		public FileCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public FileCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// <p>Set the instant at which this file will expire, after which its contents will no longer be
		/// available.
		/// 
		/// <p>Defaults to 1/4 of a Julian year from the instant {@link #FileCreateTransaction()}
		/// was invoked.
		/// 
		/// <p>May be extended using {@link FileUpdateTransaction#setExpirationTime(Timestamp)}.
		/// </summary>
		public Timestamp? ExpirationTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// <p>Set the instant at which this file will expire, after which its contents will no longer be
		/// available.
		/// 
		/// <p>Defaults to 1/4 of a Julian year from the instant {@link #FileCreateTransaction()}
		/// was invoked.
		/// 
		/// <p>May be extended using {@link FileUpdateTransaction#setExpirationTime(Timestamp)}.
		/// </summary>
		public Duration? ExpirationTimeDuration
		{
			get;
			set
			{
				RequireNotFrozen();
                ExpirationTime = null;
				field = value;
			}
		}
		/// <summary>
		/// <p>Encode the given {@link String} as UTF-8 and set it as the file's contents.
		/// 
		/// <p>This may be omitted to create an empty file.
		/// 
		/// <p>The string can later be recovered from {@link FileContentsQuery#execute(Client)}
		/// via {@link String#String(byte[], java.nio.charset.Charset)} using
		/// {@link java.nio.charset.StandardCharsets#UTF_8}.
		/// 
		/// <p>Note that total size for a given transaction is limited to 6KiB (as of March 2020) by the
		/// network; if you exceed this you may receive a {@link PrecheckStatusException}
		/// with {@link Status#TRANSACTION_OVERSIZE}.
		/// 
		/// <p>In this case, you can use {@link FileAppendTransaction}, which automatically breaks the contents
		/// into chunks for you, to append contents of arbitrary size.
		/// </summary>
		/// <summary>
		/// <p>Set the keys which must sign any transactions modifying this file. Required.
		/// 
		/// <p>All keys must sign to modify the file's contents or keys. No key is required
		/// to sign for extending the expiration time (except the one for the operator account
		/// paying for the transaction). Only one key must sign to delete the file, however.
		/// 
		/// <p>To require more than one key to sign to delete a file, add them to a
		/// {@link KeyList} and pass that here.
		/// 
		/// <p>The network currently requires a file to have at least one key (or key list or threshold key)
		/// but this requirement may be lifted in the future.
		/// </summary>
		public IList<Key> Keys
        {
            get => keys?.AsReadOnly() ?? [];
            set => keys = KeyList.Of(null, [.. value]);
        }

        /// <summary>
        /// Create the byte string.
        /// </summary>
        /// <returns>                         byte string representation</returns>
        public byte[] Contents 
        {
            get => field.CopyArray();
            set => field = value.CopyArray(); 
        } = [];
		/// <summary>
		/// <p>Encode the given {@link String} as UTF-8 and set it as the file's contents.
		/// 
		/// <p>This may be omitted to create an empty file.
		/// 
		/// <p>The string can later be recovered from {@link FileContentsQuery#execute(Client)}
		/// via {@link String#String(byte[], java.nio.charset.Charset)} using
		/// {@link java.nio.charset.StandardCharsets#UTF_8}.
		/// 
		/// <p>Note that total size for a given transaction is limited to 6KiB (as of March 2020) by the
		/// network; if you exceed this you may receive a {@link PrecheckStatusException}
		/// with {@link Status#TRANSACTION_OVERSIZE}.
		/// 
		/// <p>In this case, you can use {@link FileAppendTransaction}, which automatically breaks the contents
		/// into chunks for you, to append contents of arbitrary size.
		/// </summary>
		public string Contents_String
		{
			get => Encoding.UTF8.GetString(Contents);
            set
			{
				RequireNotFrozen();
				Contents = Encoding.UTF8.GetBytes(value);
			}
		}
		/// <summary>
		/// <p>Set the given byte array as the file's contents.
		/// 
		/// <p>This may be omitted to create an empty file.
		/// 
		/// <p>Note that total size for a given transaction is limited to 6KiB (as of March 2020) by the
		/// network; if you exceed this you may receive a {@link PrecheckStatusException}
		/// with {@link Status#TRANSACTION_OVERSIZE}.
		/// 
		/// <p>In this case, you can use {@link FileAppendTransaction}, which automatically breaks the contents
		/// into chunks for you, to append contents of arbitrary size.
		/// </summary>
		public ByteString Contents_ByteString
		{
			get => ByteString.CopyFrom(Contents);
            set
            {
                RequireNotFrozen();
				Contents = value.ToByteArray();
			}
		}
		/// <summary>
		/// Assign a memo to the file (100 bytes max).
		/// </summary>
		public string FileMemo
		{
			get;
			set
			{
				RequireNotFrozen();
                field = value;
			}
		}

		/// <summary>
		/// Initialize from transaction body.
		/// </summary>
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.FileCreate;

			if (body.ExpirationTime is not null)
				ExpirationTime = Utils.TimestampConverter.FromProtobuf(body.ExpirationTime);

			if (body.Keys is not null)
				keys = KeyList.FromProtobuf(body.Keys, null);

			Contents = body.Contents.ToByteArray();
			FileMemo = body.Memo;
		}

		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.FileCreateTransactionBody builder}</returns>
		public Proto.FileCreateTransactionBody ToProtobuf()
		{
			var builder = new Proto.FileCreateTransactionBody();

			if (ExpirationTime != null)
			{
				builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime);
			}

			if (ExpirationTimeDuration != null)
			{
				builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTimeDuration);
			}

			if (keys != null)
			{
				builder.Keys = keys.ToProtobuf();
			}

			builder.Contents = ByteString.CopyFrom(Contents);
			builder.Memo = FileMemo;

			return builder;
		}

		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return FileServiceGrpc.CreateFileMethod;
        }

		public override void ValidateChecksums(Client client) { }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileCreate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.FileCreate = ToProtobuf();
        }
    }
}