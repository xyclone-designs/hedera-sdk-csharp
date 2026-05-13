// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="T:ContractUpdateTransaction"]' />
    public sealed class ContractUpdateTransaction : Transaction<ContractUpdateTransaction>
    {
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.#ctor"]' />
		public ContractUpdateTransaction() { }
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.#ctor(Proto.Services.TransactionBody)"]' />
		internal ContractUpdateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]' />
		internal ContractUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen"]' />
		public ContractId? ContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_2"]' />
		public DateTimeOffset? ExpirationTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
                if (field == null && ExpirationTimeDuration is not null)
                    ExpirationTimeDuration = null;
            }
		}
		public TimeSpan? ExpirationTimeDuration
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
                if (field == null && ExpirationTime is not null)
                    ExpirationTime = null;
            }
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="T:ContractUpdateTransaction_2"]' />
		public Key? AdminKey
		{
			get;
			set;
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_3"]' />
		public AccountId? ProxyAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_4"]' />
		public int? MaxAutomaticTokenAssociations
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_5"]' />
		public TimeSpan? AutoRenewPeriod
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_6"]' />
		public FileId? BytecodeFileId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_7"]' />
		public string? ContractMemo
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_8"]' />
		public AccountId? StakedAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
                StakedNodeId = null;
			}
		}
		public long? StakedNodeId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
                StakedAccountId = null;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_9"]' />
		public bool? DeclineStakingReward
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_10"]' />
		public AccountId? AutoRenewAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="T:ContractUpdateTransaction_3"]' />
		public ListGuarded<HookCreationDetails> HookCreationDetails_
		{
			init => field = GenerateListGuarded(value);
			get => field ??= GenerateListGuarded<HookCreationDetails>();
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.InitFromTransactionBody"]' />
		public ListGuarded<long> HookIdsToDelete
		{
            init => field = GenerateListGuarded(value);
            get => field ??= GenerateListGuarded<long>();
        }

		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.InitFromTransactionBody_2"]' />
		void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractUpdateInstance;

            ContractId = body.ContractId is null? null : ContractId.FromProtobuf(body.ContractId);
            ProxyAccountId = body.ProxyAccountId is null ? null: AccountId.FromProtobuf(body.ProxyAccountId);
            ExpirationTime = body.ExpirationTime.ToDateTimeOffset();

            if (body.AdminKey is not null)
				AdminKey = Key.FromProtobufKey(body.AdminKey);

			MaxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
            AutoRenewPeriod = body.AutoRenewPeriod.ToTimeSpan();
            ContractMemo = body.MemoWrapper;
            DeclineStakingReward = body.DeclineReward;
            StakedAccountId = body.StakedAccountId is null ? null : AccountId.FromProtobuf(body.StakedAccountId);
            StakedNodeId = body.StakedNodeId;

            AutoRenewAccountId = body.AutoRenewAccountId is null ? null : AccountId.FromProtobuf(body.AutoRenewAccountId);

			HookCreationDetails_.ClearAndSet(body.HookCreationDetails.Select(_ => HookCreationDetails.FromProtobuf(_)));
			HookIdsToDelete.ClearAndSet(body.HookIdsToDelete);
        }

		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.ToProtobuf"]' />
		public Proto.Services.ContractUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ContractUpdateTransactionBody { };

            if (ContractId != null)
				builder.ContractId = ContractId.ToProtobuf();

            if (ProxyAccountId != null)
				builder.ProxyAccountId = ProxyAccountId.ToProtobuf();

            if (ExpirationTime != null)
				builder.ExpirationTime = ExpirationTime.Value.ToProtoTimestamp();

            if (ExpirationTimeDuration != null)
				builder.ExpirationTime = ExpirationTimeDuration.Value.ToProtoTimestamp();

            if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

            if (MaxAutomaticTokenAssociations != null)
				builder.MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations;

			if (AutoRenewPeriod != null)
				builder.AutoRenewPeriod = AutoRenewPeriod.Value.ToProtoDuration();

            if (ContractMemo != null)
				builder.MemoWrapper = ContractMemo;

            if (StakedAccountId != null)
				builder.StakedAccountId = StakedAccountId.ToProtobuf();

            if (StakedNodeId != null)
				builder.StakedNodeId = StakedNodeId.Value;

            if (DeclineStakingReward != null)
				builder.DeclineReward = DeclineStakingReward.Value;

			if (HookIdsToDelete.Count != 0)
				builder.HookIdsToDelete.AddRange(HookIdsToDelete);

			if (AutoRenewAccountId != null)
			{
				if (AutoRenewAccountId.ToString().Equals("0.0.0"))
					builder.AutoRenewAccountId = new Proto.Services.AccountID { };
				else
					builder.AutoRenewAccountId = AutoRenewAccountId.ToProtobuf();
			}

			builder.HookCreationDetails.AddRange(HookCreationDetails_.Select(_ => _.ToProtobuf()));

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            ContractId?.ValidateChecksum(client);
            ProxyAccountId?.ValidateChecksum(client);
            StakedAccountId?.ValidateChecksum(client);
            AutoRenewAccountId?.ValidateChecksum(client);
        }
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractUpdateInstance = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractUpdateInstance = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.updateContract);

			return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
    }
}
