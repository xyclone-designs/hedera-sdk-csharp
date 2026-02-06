// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Collections.Generic;

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
		/// <summary>
		/// Constructor.
		/// </summary>
		public FreezeTransaction() { }
		public FreezeTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		public FreezeTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
		{
			InitFromTransactionBody();
		}

		public Timestamp StartTime
		{
			get => field ?? Timestamp.FromDateTimeOffset(DateTimeOffset.UnixEpoch);
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
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
		/// A SHA384 hash of file content.<br/>
		/// This is a hash of the file identified by `update_file`.
		/// <p>
		/// This MUST be set if `update_file` is set, and MUST match the
		/// SHA384 hash of the contents of that file.
		/// </summary>
		/// <param name="fileHash">the fileHash to set</param>
		/// <returns>{@code this}</returns>
		public byte[] FileHash
		{
			get => field.CopyArray();
			set
			{
				RequireNotFrozen();
				field = value.CopyArray();
			}
		} = [];
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
				StartTime = Utils.TimestampConverter.FromProtobuf(body.StartTime);
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
				builder.StartTime = Utils.TimestampConverter.ToProtobuf(StartTime);
			}

			return builder;
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
		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return FreezeServiceGrpc.GetFreezeMethod();
		}
	}
}