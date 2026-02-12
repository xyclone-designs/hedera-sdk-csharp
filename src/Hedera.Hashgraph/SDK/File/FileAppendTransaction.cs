// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.File
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
        public static readonly int DEFAULT_CHUNK_SIZE = 4096;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileAppendTransaction() : base()
        {
            DefaultMaxTransactionFee = new Hbar(5);
            ChunkSize = 2048;
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal FileAppendTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
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
        public FileId? FileId
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }
		/// <summary>
		/// <p>Set the contents to append to the file as identified by {@link #setFileId(FileId)}.
		/// </summary>
		/// <param name="contents">the contents to append to the file.</param>
		public ByteString Contents
        {
            get => Data;
            set => Data = value;
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
		public byte[] Contents_Bytes
		{
			set => Data = ByteString.CopyFrom(value);
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
		public string Contents_String
		{
			set => Data = ByteString.CopyFromUtf8(value);
		}

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.FileAppend;
			if (body.FileID is not null)
			{
				FileId = FileId.FromProtobuf(body.FileID);
			}

			if (InnerSignedTransactions.Count > 0)
			{
				try
				{
					for (var i = 0; i < InnerSignedTransactions.Count; i += NodeAccountIds.Count == 0 ? 1 : NodeAccountIds.Count)
					{
						Data = Data.Concat(Proto.TransactionBody.Parser.ParseFrom(InnerSignedTransactions[i].BodyBytes).FileAppend.Contents);
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

		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.FileAppendTransactionBody builder}</returns>
		public Proto.FileAppendTransactionBody ToProtobuf()
		{
			var builder = new Proto.FileAppendTransactionBody
			{
				Contents = Data
			};

            if (FileId != null)
				builder.FileID = FileId.ToProtobuf();

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
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.FileAppend = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.FileAppend = ToProtobuf();
            scheduled.FileAppend.Contents = Data;
        }
        public override void OnFreezeChunk(Proto.TransactionBody body, Proto.TransactionID? initialTransactionId, int startIndex, int endIndex, int chunk, int total)
        {
			body.FileAppend = ToProtobuf();
			body.FileAppend.Contents = ByteString.CopyFrom(Data.Span[startIndex..endIndex]);
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.FileService.FileServiceClient.appendContent);

			return Proto.FileService.Descriptor.FindMethodByName(methodname);
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
    }
}