// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
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
    /// A transaction body for an `appendContent` transaction.<br/>
    /// This transaction body provides a mechanism to append content to a "file" in
    /// network state. Hedera transactions are limited in size, but there are many
    /// uses for in-state byte arrays (e.g. smart contract bytecode) which require
    /// more than may fit within a single transaction. The `appendFile` transaction
    /// exists to support these requirements. The typical pattern is to create a
    /// file, append more data until the full content is stored, verify the file is
    /// correct, then update the file entry with any final metadata changes (e.g.
    /// adding threshold keys and removing the initial upload key).
    /// 
    /// Each append transaction MUST remain within the total transaction size limit
    /// for the network (typically 6144 bytes).<br/>
    /// The total size of a file MUST remain within the maximum file size limit for
    /// the network (typically 1048576 bytes).
    /// 
    /// #### Signature Requirements
    /// Append transactions MUST have signatures from _all_ keys in the `KeyList`
    /// assigned to the `keys` field of the file.<br/>
    /// See the [File Service](#FileService) specification for a detailed
    /// explanation of the signature requirements for all file transactions.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public sealed class FileAppendTransaction : ChunkedTransaction<FileAppendTransaction>
    {
        static int DEFAULT_CHUNK_SIZE = 4096;
        private FileId fileId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public FileAppendTransaction() : base()
        {
            defaultMaxTransactionFee = new Hbar(5);
            SetChunkSize(2048);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        FileAppendTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        FileAppendTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// A file identifier.<br/>
        /// This identifies the file to which the `contents` will be appended.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// The identified file MUST exist.<br/>
        /// The identified file MUST NOT be larger than the current maximum file
        /// size limit.<br/>
        /// The identified file MUST NOT be deleted.<br/>
        /// The identified file MUST NOT be immutable.
        /// </summary>
        /// <param name="fileId">the ID of the file to append to.</param>
        /// <returns>{@code this}</returns>
        public FileAppendTransaction SetFileId(FileId fileId)
        {
            Objects.RequireNonNull(fileId);
            RequireNotFrozen();
            fileId = fileId;
            return this;
        }

        /// <summary>
        /// Extract the byte string representing the file.
        /// </summary>
        /// <returns>                         the byte string representing the file</returns>
        public ByteString GetContents()
        {
            return GetData();
        }

        /// <summary>
        /// An array of bytes to append.<br/>
        /// <p>
        /// This content SHALL be appended to the identified file if this
        /// transaction succeeds.<br/>
        /// This field is REQUIRED.<br/>
        /// This field MUST NOT be empty.
        /// </summary>
        /// <param name="contents">the contents to append to the file.</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@see#setContents(String) for an overload which takes String.</remarks>
        public FileAppendTransaction SetContents(byte[] contents)
        {
            return SetData(contents);
        }

        /// <summary>
        /// <p>Set the contents to append to the file as identified by {@link #setFileId(FileId)}.
        /// </summary>
        /// <param name="contents">the contents to append to the file.</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@see#setContents(String) for an overload which takes String.</remarks>
        public FileAppendTransaction SetContents(ByteString contents)
        {
            return SetData(contents);
        }

        /// <summary>
        /// <p>Encode the given {@link String} as UTF-8 and append it to file as identified by
        /// {@link #setFileId(FileId)}.
        /// 
        /// <p>If the whole file is UTF-8 encoded, the string can later be recovered from
        /// {@link FileContentsQuery#execute(Client)} via
        /// {@link String#String(byte[], java.nio.charset.Charset)} using
        /// {@link java.nio.charset.StandardCharsets#UTF_8}.
        /// </summary>
        /// <param name="text">The String to be set as the contents of the file</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@see#setContents(byte[]) for appending arbitrary data.</remarks>
        public FileAppendTransaction SetContents(string text)
        {
            return SetData(text);
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
            return FileServiceGrpc.GetAppendContentMethod();
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetFileAppend();
            if (body.HasFileID())
            {
                fileId = FileId.FromProtobuf(body.GetFileID());
            }

            if (!innerSignedTransactions.IsEmpty())
            {
                try
                {
                    for (var i = 0; i < innerSignedTransactions.Count; i += nodeAccountIds.IsEmpty() ? 1 : nodeAccountIds.Count)
                    {
                        data = data.Concat(TransactionBody.ParseFrom(innerSignedTransactions[i].GetBodyBytes()).GetFileAppend().GetContents());
                    }
                }
                catch (InvalidProtocolBufferException exc)
                {
                    throw new ArgumentException(exc.GetMessage());
                }
            }
            else
            {
                data = body.GetContents();
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.FileAppendTransactionBody builder}</returns>
        FileAppendTransactionBody.Builder Build()
        {
            var builder = FileAppendTransactionBody.NewBuilder();
            if (fileId != null)
            {
                builder.SetFileID(fileId.ToProtobuf());
            }

            builder.SetContents(data);
            return builder;
        }

        override void OnFreezeChunk(TransactionBody.Builder body, TransactionID initialTransactionId, int startIndex, int endIndex, int chunk, int total)
        {
            body.SetFileAppend(Build().SetContents(data.Substring(startIndex, endIndex)));
        }

        override bool ShouldGetReceipt()
        {
            return true;
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetFileAppend(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetFileAppend(Build().SetContents(data));
        }
    }
}