// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Hedera.Hashgraph.SDK.Transactions.File
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
        private Timestamp expirationTime = null;
        private Duration expirationTimeDuration = null;
        private KeyList keys = null;
        private byte[] contents = [];

        private string fileMemo = "";
        /// <summary>
        /// Constructor.
        /// </summary>
        public FileCreateTransaction()
        {
            SetExpirationTime(Timestamp..Now().Plus(DEFAULT_AUTO_RENEW_PERIOD));
            defaultMaxTransactionFee = new Hbar(5);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        FileCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        FileCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the time.
        /// </summary>
        /// <returns>                         expiration time</returns>
        public Timestamp GetExpirationTime()
        {
            return expirationTime;
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
        /// <param name="expirationTime">the {@link Timestamp} at which this file should expire.</param>
        /// <returns>{@code this}</returns>
        public FileCreateTransaction SetExpirationTime(Timestamp expirationTime)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(expirationTime);
            expirationTime = expirationTime;
            return this;
        }

        public FileCreateTransaction SetExpirationTime(Duration expirationTime)
        {
            ArgumentNullException.ThrowIfNull(expirationTime);
            RequireNotFrozen();
            expirationTime = null;
            expirationTimeDuration = expirationTime;
            return this;
        }

        /// <summary>
        /// Extract the of keys.
        /// </summary>
        /// <returns>                         list of keys</returns>
        public Collection<Key> GetKeys()
        {
            return keys != null ? Collections.UnmodifiableCollection(keys) : null;
        }

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
        /// <param name="keys">The Key or Keys to be set</param>
        /// <returns>{@code this}</returns>
        public FileCreateTransaction SetKeys(params Key[] keys)
        {
            RequireNotFrozen();
            keys = KeyList.Of(keys);
            return this;
        }

        /// <summary>
        /// Create the byte string.
        /// </summary>
        /// <returns>                         byte string representation</returns>
        public ByteString GetContents()
        {
            return ByteString.CopyFrom(contents);
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
        /// <param name="bytes">the contents of the file.</param>
        /// <returns>{@code this}</returns>
        public FileCreateTransaction SetContents(byte[] bytes)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(bytes);
            contents = bytes.CopyArray();
            return this;
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
        /// <param name="text">the contents of the file.</param>
        /// <returns>{@code this}</returns>
        public FileCreateTransaction SetContents(string text)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(text);
            contents = Encoding.UTF8.GetBytes(text);
            return this;
        }

        /// <summary>
        /// Extract the file's memo field.
        /// </summary>
        /// <returns>                         the file's memo field</returns>
        public string GetFileMemo()
        {
            return fileMemo;
        }

        /// <summary>
        /// Assign a memo to the file (100 bytes max).
        /// </summary>
        /// <param name="memo">memo string</param>
        /// <returns>{@code this}</returns>
        public FileCreateTransaction SetFileMemo(string memo)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(memo);
            fileMemo = memo;
            return this;
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return FileServiceGrpc.CreateFileMethod;
        }

        public override void ValidateChecksums(Client client) { }

        /// <summary>
        /// Initialize from transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.FileCreate;

            if (body.ExpirationTime is not null)
            {
                expirationTime = Utils.TimestampConverter.FromProtobuf(body.ExpirationTime);
            }

            if (body.Keys is not null)
            {
                keys = KeyList.FromProtobuf(body.Keys, null);
            }

            contents = body.Contents.ToByteArray();
            fileMemo = body.Memo;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.FileCreateTransactionBody builder}</returns>
        Proto.FileCreateTransactionBody Build()
        {
            var builder = new Proto.FileCreateTransactionBody();

            if (expirationTime != null)
            {
                builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(expirationTime);
            }

            if (expirationTimeDuration != null)
            {
                builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(expirationTimeDuration);
            }

            if (keys != null)
            {
                builder.Keys = keys.ToProtobuf();
            }

            builder.Contents = ByteString.CopyFrom(contents);
            builder.Memo = fileMemo;

            return builder;
        }

        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileCreate = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.FileCreate = Build();
        }
    }
}