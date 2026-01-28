// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
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

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// A transaction body for all five freeze transactions.
    /// 
    /// Combining five different transactions into a single message, this
    /// transaction body MUST support options to schedule a freeze, abort a
    /// scheduled freeze, prepare a software upgrade, prepare a telemetry
    /// upgrade, or initiate a software upgrade.
    /// 
    /// For a scheduled freeze, at the scheduled time, according to
    /// network consensus time
    ///   - A freeze (`FREEZE_ONLY`) causes the network nodes to stop creating
    ///     events or accepting transactions, and enter a persistent
    ///     maintenance state.
    ///   - A freeze upgrade (`FREEZE_UPGRADE`) causes the network nodes to stop
    ///     creating events or accepting transactions, and upgrade the node software
    ///     from a previously prepared upgrade package. The network nodes then
    ///     restart and rejoin the network after upgrading.
    /// 
    /// For other freeze types, immediately upon processing the freeze transaction
    ///   - A Freeze Abort (`FREEZE_ABORT`) cancels any pending scheduled freeze.
    ///   - A prepare upgrade (`PREPARE_UPGRADE`) begins to extract the contents of
    ///     the specified upgrade file to the local filesystem.
    ///   - A telemetry upgrade (`TELEMETRY_UPGRADE`) causes the network nodes to
    ///     extract a telemetry upgrade package to the local filesystem and signal
    ///     other software on the machine to upgrade, without impacting the node or
    ///     network processing.
    /// 
    /// ### Block Stream Effects
    /// Unknown
    /// </summary>
    public sealed class FreezeTransaction : Transaction<FreezeTransaction>
    {
        private int endHour = 0;
        private int endMinute = 0;
        private Timestamp startTime = null;
        private FileId fileId = null;
        private byte[] fileHash = new[]
        {
        };
        private FreezeType freezeType = FreezeType.UNKNOWN_FREEZE_TYPE;
        /// <summary>
        /// Constructor.
        /// </summary>
        public FreezeTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        FreezeTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        FreezeTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the start time.
        /// </summary>
        /// <returns>                         the start time</returns>
        public Timestamp GetStartTime()
        {
            return startTime != null ? startTime : Timestamp.EPOCH;
        }

        /// <summary>
        /// A start time for the freeze.
        /// <p>
        /// If this field is REQUIRED for the specified `freeze_type`, then
        /// when the network consensus time reaches this instant<ol>
        ///   <li>The network SHALL stop accepting transactions.</li>
        ///   <li>The network SHALL gossip a freeze state.</li>
        ///   <li>The nodes SHALL, in coordinated order, disconnect and
        ///       shut down.</li>
        ///   <li>The nodes SHALL halt or perform a software upgrade, depending
        ///       on `freeze_type`.</li>
        ///   <li>If the `freeze_type` is `FREEZE_UPGRADE`, the nodes SHALL
        ///       restart and rejoin the network upon completion of the
        ///       software upgrade.</li>
        /// </ol>
        /// <blockquote>
        /// If the `freeze_type` is `TELEMETRY_UPGRADE`, the start time is required,
        /// but the network SHALL NOT stop, halt, or interrupt transaction
        /// processing. The required field is an historical anomaly and SHOULD
        /// change in a future release.</blockquote>
        /// </summary>
        /// <param name="startTime">the start time</param>
        /// <returns>{@code this}</returns>
        public FreezeTransaction SetStartTime(Timestamp startTime)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(startTime);
            startTime = startTime;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="hour">The hour to be set</param>
        /// <param name="minute">The minute to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedUse {@link #setStartTime(Timestamp)} instead.</remarks>
        public FreezeTransaction SetStartTime(int hour, int minute)
        {
            return SetStartTime(Timestamp.OfEpochMilli(((long)hour * 60 * 60 + (long)minute * 60) * 1000));
        }

        /// <summary>
        /// </summary>
        /// <returns>the end time</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public Timestamp GetEndTime()
        {
            return Timestamp.From(OffsetTime.Of(endHour, endMinute, 0, 0, ZoneOffset.UTC));
        }

        /// <summary>
        /// Sets the end time (in UTC).
        /// </summary>
        /// <param name="hour">The hour to be set</param>
        /// <param name="minute">The minute to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public FreezeTransaction SetEndTime(int hour, int minute)
        {
            RequireNotFrozen();
            endHour = hour;
            endMinute = minute;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>the fileId</returns>
        /// <remarks>@deprecatedUse {@link #getFileId()} instead.</remarks>
        public FileId GetUpdateFileId()
        {
            return fileId;
        }

        /// <summary>
        /// </summary>
        /// <param name="updateFileId">the new fileId</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedUse {@link #setFileId(FileId)} instead.</remarks>
        public FreezeTransaction SetUpdateFileId(FileId updateFileId)
        {
            return SetFileId(updateFileId);
        }

        /// <summary>
        /// </summary>
        /// <returns>the fileHash</returns>
        /// <remarks>@deprecatedUse {@link #getFileHash()} instead.</remarks>
        public byte[] GetUpdateFileHash()
        {
            return Array.CopyOf(fileHash, fileHash.Length);
        }

        /// <summary>
        /// </summary>
        /// <param name="updateFileHash">fileHash to set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedUse {@link #setFileHash(byte[])} instead.</remarks>
        public FreezeTransaction SetUpdateFileHash(byte[] updateFileHash)
        {
            return SetFileHash(updateFileHash);
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
        /// Assign the file id.
        /// </summary>
        /// <param name="fileId">the file id</param>
        /// <returns>{@code this}</returns>
        public FreezeTransaction SetFileId(FileId fileId)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(fileId);
            fileId = fileId;
            return this;
        }

        /// <summary>
        /// The expected hash of the contents of the update file (used to verify the update)
        /// </summary>
        /// <returns>                         the file's hash</returns>
        public byte[] GetFileHash()
        {
            return Array.CopyOf(fileHash, fileHash.Length);
        }

        /// <summary>
        /// A SHA384 hash of file content.<br/>
        /// This is a hash of the file identified by `update_file`.
        /// <p>
        /// This MUST be set if `update_file` is set, and MUST match the
        /// SHA384 hash of the contents of that file.
        /// </summary>
        /// <param name="fileHash">the fileHash to set</param>
        /// <returns>{@code this}</returns>
        public FreezeTransaction SetFileHash(byte[] fileHash)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(fileHash);
            fileHash = Array.CopyOf(fileHash, fileHash.Length);
            return this;
        }

        /// <summary>
        /// Extract the freeze type.
        /// </summary>
        /// <returns>                         the freeze type</returns>
        public FreezeType GetFreezeType()
        {
            return freezeType;
        }

        /// <summary>
        /// The type of freeze.
        /// <p>
        /// This REQUIRED field effectively selects between five quite different
        /// transactions in the same transaction body. Depending on this value
        /// the service may schedule a freeze, prepare upgrades, perform upgrades,
        /// or even abort a previously scheduled freeze.
        /// 
        /// {@link FreezeTransaction}
        /// </summary>
        /// <param name="freezeType">the freeze type</param>
        /// <returns>{@code this}</returns>
        public FreezeTransaction SetFreezeType(FreezeType freezeType)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(freezeType);
            freezeType = freezeType;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return FreezeServiceGrpc.GetFreezeMethod();
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.Freeze();
            freezeType = FreezeType.ValueOf(body.GetFreezeType());
            if (body.HasUpdateFile())
            {
                fileId = FileId.FromProtobuf(body.GetUpdateFile());
            }

            fileHash = body.GetFileHash().ToByteArray();
            if (body.HasStartTime())
            {
                startTime = Utils.TimestampConverter.FromProtobuf(body.GetStartTime());
            }
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.FreezeTransactionBody builder }</returns>
        FreezeTransactionBody.Builder Build()
        {
            var builder = FreezeTransactionBody.NewBuilder();
            builder.FreezeType(freezeType.code);
            if (fileId != null)
            {
                builder.UpdateFile(fileId.ToProtobuf());
            }

            builder.FileHash(ByteString.CopyFrom(fileHash));
            if (startTime != null)
            {
                builder.StartTime(Utils.TimestampConverter.ToProtobuf(startTime));
            }

            return builder;
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SetFreeze(Build());
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.SetFreeze(Build());
        }
    }
}