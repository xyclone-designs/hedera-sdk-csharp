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

namespace Hedera.Hashgraph.SDK.Schedule
{
    /// <summary>
    /// Create a new Schedule.
    /// 
    /// #### Requirements
    /// This transaction SHALL create a new _schedule_ entity in network state.<br/>
    /// The schedule created SHALL contain the `scheduledTransactionBody` to be
    /// executed.<br/>
    /// If successful the receipt SHALL contain a `scheduleID` with the full
    /// identifier of the schedule created.<br/>
    /// When a schedule _executes_ successfully, the receipt SHALL include a
    /// `scheduledTransactionID` with the `TransactionID` of the transaction that
    /// executed.<br/>
    /// When a scheduled transaction is executed the network SHALL charge the
    /// regular _service_ fee for the transaction to the `payerAccountID` for
    /// that schedule, but SHALL NOT charge node or network fees.<br/>
    /// If the `payerAccountID` field is not set, the effective `payerAccountID`
    /// SHALL be the `payer` for this create transaction.<br/>
    /// If an `adminKey` is not specified, or is an empty `KeyList`, the schedule
    /// created SHALL be immutable.<br/>
    /// An immutable schedule MAY be signed, and MAY execute, but SHALL NOT be
    /// deleted.<br/>
    /// If two schedules have the same values for all fields except `payerAccountID`
    /// then those two schedules SHALL be deemed "identical".<br/>
    /// If a `scheduleCreate` requests a new schedule that is identical to an
    /// existing schedule, the transaction SHALL fail and SHALL return a status
    /// code of `IDENTICAL_SCHEDULE_ALREADY_CREATED` in the receipt.<br/>
    /// The receipt for a duplicate schedule SHALL include the `ScheduleID` of the
    /// existing schedule and the `TransactionID` of the earlier `scheduleCreate`
    /// so that the earlier schedule may be queried and/or referred to in a
    /// subsequent `scheduleSign`.
    /// 
    /// #### Signature Requirements
    /// A `scheduleSign` transaction SHALL be used to add additional signatures
    /// to an existing schedule.<br/>
    /// Each signature SHALL "activate" the corresponding cryptographic("primitive")
    /// key for that schedule.<br/>
    /// Signature requirements SHALL be met when the set of active keys includes
    /// all keys required by the scheduled transaction.<br/>
    /// A scheduled transaction for a "long term" schedule SHALL NOT execute if
    /// the signature requirements for that transaction are not met when the
    /// network consensus time reaches the schedule `expiration_time`.<br/>
    /// A "short term" schedule SHALL execute immediately once signature
    /// requirements are met. This MAY be immediately when created.
    /// 
    /// #### Long Term Schedules
    /// A "short term" schedule SHALL have the flag `wait_for_expiry` _unset_.<br/>
    /// A "long term" schedule SHALL have the flag  `wait_for_expiry` _set_.<br/>
    /// A "long term" schedule SHALL NOT be accepted if the network configuration
    /// `scheduling.longTermEnabled` is not enabled.<br/>
    /// A "long term" schedule SHALL execute when the current consensus time
    /// matches or exceeds the `expiration_time` for that schedule, if the
    /// signature requirements for the scheduled transaction
    /// are met at that instant.<br/>
    /// A "long term" schedule SHALL NOT execute before the current consensus time
    /// matches or exceeds the `expiration_time` for that schedule.<br/>
    /// A "long term" schedule SHALL expire, and be removed from state, after the
    /// network consensus time exceeds the schedule `expiration_time`.<br/>
    /// A short term schedule SHALL expire, and be removed from state,
    /// after the network consensus time exceeds the current network
    /// configuration for `ledger.scheduleTxExpiryTimeSecs`.
    /// 
    /// > Note
    /// >> Long term schedules are not (as of release 0.56.0) enabled. Any schedule
    /// >> created currently MUST NOT set the `wait_for_expiry` flag.<br/>
    /// >> When long term schedules are not enabled, schedules SHALL NOT be
    /// >> executed at expiration, and MUST meet signature requirements strictly
    /// >> before expiration to be executed.
    /// 
    /// ### Block Stream Effects
    /// If the scheduled transaction is executed immediately, the transaction
    /// record SHALL include a `scheduleRef` with the schedule identifier of the
    /// schedule created.
    /// </summary>
    public sealed class ScheduleCreateTransaction : Transaction<ScheduleCreateTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScheduleCreateTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public ScheduleCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// An optional timestamp for specifying when the transaction should be evaluated for execution and then expire.
		/// Defaults to 30 minutes after the transaction's consensus timestamp.
		/// <p>
		/// Note: This field is unused and forced to be unset until Long Term Scheduled Transactions are enabled - Transactions will always
		///       expire in 30 minutes if Long Term Scheduled Transactions are not enabled.
		/// </summary>
		public Timestamp? ExpirationTime 
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
				ExpirationTimeDuration = null;
			}
        }
		/// <summary>
		/// Overload: set the expiration time using a Duration value.
		/// </summary>
		public Duration? ExpirationTimeDuration
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				ExpirationTime = null;
			}
		}
		/// <summary>
		/// When set to true, the transaction will be evaluated for execution at expiration_time instead
		/// of when all required signatures are received.
		/// When set to false, the transaction will execute immediately after sufficient signatures are received
		/// to sign the contained transaction. During the initial ScheduleCreate transaction or via ScheduleSign transactions.
		/// Defaults to false.
		/// <p>
		/// Setting this to false does not necessarily mean that the transaction will never execute at expiration_time.
		///  <p>
		///  For Example - If the signature requirements for a Scheduled Transaction change via external means (e.g. CryptoUpdate)
		///  such that the Scheduled Transaction would be allowed to execute, it will do so autonomously at expiration_time, unless a
		///  ScheduleSign comes in to “poke” it and force it to go through immediately.
		/// <p>
		/// Note: This field is unused and forced to be unset until Long Term Scheduled Transactions are enabled. Before Long Term
		///       Scheduled Transactions are enabled, Scheduled Transactions will _never_ execute at expiration  - they will _only_
		///       execute during the initial ScheduleCreate transaction or via ScheduleSign transactions and will _always_
		///       expire at expiration_time.
		/// </summary>
		public bool WaitForExpiry { get; set; }
		/// <summary>
		/// An account identifier of a `payer` for the scheduled transaction.
		/// <p>
		/// This value MAY be unset. If unset, the `payer` for this `scheduleCreate`
		/// transaction SHALL be the `payer` for the scheduled transaction.<br/>
		/// If this is set, the identified account SHALL be charged the fees
		/// required for the scheduled transaction when it is executed.<br/>
		/// If the actual `payer` for the _scheduled_ transaction lacks
		/// sufficient HBAR balance to pay service fees for the scheduled
		/// transaction _when it executes_, the scheduled transaction
		/// SHALL fail with `INSUFFICIENT_PAYER_BALANCE`.<br/>
		/// </summary>
		public AccountId? PayerAccountId 
        {
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Assign the transaction body to schedule.
		/// </summary>
		public Proto.SchedulableTransactionBody? ScheduledTransactionBody
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// A `Key` required to delete this schedule.
		/// <p>
		/// If this is not set, or is an empty `KeyList`, this schedule SHALL be
		/// immutable and SHALL NOT be deleted.
		/// </summary>
		public Key? AdminKey
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// A short description of the schedule.
		/// <p>
		/// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
		/// (default 100) bytes when encoded as UTF-8.
		/// </summary>
		public string ScheduleMemo 
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        } = string.Empty;

		/// <summary>
		/// Build the correct transaction body.
		/// </summary>
		/// <returns>{@link Proto.ScheduleCreateTransactionBody builder }</returns>
		public Proto.ScheduleCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.ScheduleCreateTransactionBody();

            if (PayerAccountId != null)
				builder.PayerAccountID = PayerAccountId.ToProtobuf();

			if (ScheduledTransactionBody != null)
				builder.ScheduledTransactionBody = ScheduledTransactionBody;

			if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

			if (ExpirationTime != null)
				builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime);
			else if (ExpirationTimeDuration != null)
				builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTimeDuration);

			builder.Memo = ScheduleMemo;
            builder.WaitForExpiry = WaitForExpiry;

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ScheduleCreate;

			ScheduleMemo = body.Memo;
			AdminKey = Key.FromProtobufKey(body.AdminKey);
			ScheduledTransactionBody = body.ScheduledTransactionBody;
			PayerAccountId = AccountId.FromProtobuf(body.PayerAccountID);
			ExpirationTime = Utils.TimestampConverter.FromProtobuf(body.ExpirationTime);
        }

        public override void ValidateChecksums(Client client)
        {
            PayerAccountId?.ValidateChecksum(client);
        }
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ScheduleCreate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule ScheduleCreateTransaction");
        }
		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return ScheduleServiceGrpc.CreateScheduleMethod;
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