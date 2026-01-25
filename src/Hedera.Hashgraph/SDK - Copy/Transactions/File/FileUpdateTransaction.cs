// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Nio.Charset;
using Java.Time;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;

namespace Hedera.Hashgraph.SDK.Transactions.File
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
        private FileId fileId = null;
        private KeyList keys = null;
        private Timestamp expirationTime = null;
        private Duration expirationTimeDuration = null;
        private byte[] contents = new[]
        {
        };
        private string fileMemo = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public FileUpdateTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        FileUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        FileUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the file id.
        /// </summary>
        /// <returns>                         the file id</returns>
        public FileId GetFileId()
        {
            return fileId;
        }

        /// <summary>
        /// Set the ID of the file to update; required.
        /// </summary>
        /// <param name="fileId">the ID of the file to update.</param>
        /// <returns>{@code this}</returns>
        public FileUpdateTransaction SetFileId(FileId fileId)
        {
            Objects.RequireNonNull(fileId);
            RequireNotFrozen();
            fileId = fileId;
            return this;
        }

        /// <summary>
        /// Get the keys which must sign any transactions modifying this file.
        /// </summary>
        /// <returns>                        the list of keys</returns>
        public Collection<Key> GetKeys()
        {
            return keys != null ? Collections.UnmodifiableCollection(keys) : null;
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
        /// <param name="keys">The Key or Keys to be set</param>
        /// <returns>{@code this}</returns>
        public FileUpdateTransaction SetKeys(params Key[] keys)
        {
            RequireNotFrozen();
            keys = KeyList.Of(keys);
            return this;
        }

        /// <summary>
        /// Extract the expiration time.
        /// </summary>
        /// <returns>                         the expiration time</returns>
        public Timestamp GetExpirationTime()
        {
            return expirationTime;
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
        /// <param name="expirationTime">the new {@link Timestamp} at which the transaction will expire.</param>
        /// <returns>{@code this}</returns>
        public FileUpdateTransaction SetExpirationTime(Timestamp expirationTime)
        {
            Objects.RequireNonNull(expirationTime);
            RequireNotFrozen();
            expirationTime = expirationTime;
            return this;
        }

        public FileUpdateTransaction SetExpirationTime(Duration expirationTime)
        {
            Objects.RequireNonNull(expirationTime);
            RequireNotFrozen();
            expirationTime = null;
            expirationTimeDuration = expirationTime;
            return this;
        }

        /// <summary>
        /// Extract the files contents as a byte string.
        /// </summary>
        /// <returns>                         the files contents as a byte string</returns>
        public ByteString GetContents()
        {
            return ByteString.CopyFrom(contents);
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
        public FileUpdateTransaction SetContents(byte[] bytes)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(bytes);
            contents = Array.CopyOf(bytes, bytes.Length);
            return this;
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
        public FileUpdateTransaction SetContents(string text)
        {
            Objects.RequireNonNull(text);
            RequireNotFrozen();
            contents = text.GetBytes(StandardCharsets.UTF_8);
            return this;
        }

        /// <summary>
        /// Extract the file's memo up to 100 bytes.
        /// </summary>
        /// <returns>                         the file's memo up to 100 bytes</returns>
        public string GetFileMemo()
        {
            return fileMemo;
        }

        /// <summary>
        /// A short description of this file.
        /// <p>
        /// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
        /// (default 100) bytes when encoded as UTF-8.
        /// </summary>
        /// <param name="memo">the file's memo</param>
        /// <returns>{@code this}</returns>
        public FileUpdateTransaction SetFileMemo(string memo)
        {
            Objects.RequireNonNull(memo);
            RequireNotFrozen();
            fileMemo = memo;
            return this;
        }

        /// <summary>
        /// Remove the file memo.
        /// </summary>
        /// <returns>{@code this}</returns>
        public FileUpdateTransaction ClearMemo()
        {
            RequireNotFrozen();
            fileMemo = "";
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetFileUpdate();
            if (body.HasFileID())
            {
                fileId = FileId.FromProtobuf(body.GetFileID());
            }

            if (body.HasKeys())
            {
                keys = KeyList.FromProtobuf(body.GetKeys(), null);
            }

            if (body.HasExpirationTime())
            {
                expirationTime = Utils.TimestampConverter.FromProtobuf(body.GetExpirationTime());
            }

            if (body.HasMemo())
            {
                fileMemo = body.GetMemo().GetValue();
            }

            contents = body.GetContents().ToByteArray();
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.FileUpdateTransactionBody builder }</returns>
        FileUpdateTransactionBody.Builder Build()
        {
            var builder = FileUpdateTransactionBody.NewBuilder();
            if (fileId != null)
            {
                builder.SetFileID(fileId.ToProtobuf());
            }

            if (keys != null)
            {
                builder.SetKeys(keys.ToProtobuf());
            }

            if (expirationTime != null)
            {
                builder.SetExpirationTime(Utils.TimestampConverter.ToProtobuf(expirationTime));
            }

            if (expirationTimeDuration != null)
            {
                builder.SetExpirationTime(Utils.TimestampConverter.ToProtobuf(expirationTimeDuration));
            }

            builder.SetContents(ByteString.CopyFrom(contents));
            if (fileMemo != null)
            {
                builder.SetMemo(StringValue.Of(fileMemo));
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (fileId != null)
            {
                fileId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return FileServiceGrpc.GetUpdateFileMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetFileUpdate(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetFileUpdate(Build());
        }
    }
}