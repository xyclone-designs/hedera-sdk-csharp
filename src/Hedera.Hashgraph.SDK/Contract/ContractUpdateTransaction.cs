// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="T:ContractUpdateTransaction"]/*' />
    public sealed class ContractUpdateTransaction : Transaction<ContractUpdateTransaction>
    {
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.#ctor"]/*' />
		public ContractUpdateTransaction() { }
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal ContractUpdateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal ContractUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private List<HookCreationDetails> _HookCreationDetails = [];
		private List<long> _HookIdsToDelete = [];

		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen"]/*' />
		public ContractId? ContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_2"]/*' />
		public DateTimeOffset? ExpirationTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
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
				ExpirationTime = null;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="T:ContractUpdateTransaction_2"]/*' />
		public Key? AdminKey
		{
			get;
			set;
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_3"]/*' />
		public AccountId? ProxyAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_4"]/*' />
		public int? MaxAutomaticTokenAssociations
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_5"]/*' />
		public TimeSpan? AutoRenewPeriod
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_6"]/*' />
		public FileId? BytecodeFileId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_7"]/*' />
		public string? ContractMemo
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_8"]/*' />
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
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_9"]/*' />
		public bool? DeclineStakingReward
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.RequireNotFrozen_10"]/*' />
		public AccountId? AutoRenewAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="T:ContractUpdateTransaction_3"]/*' />
		public ListGuarded<HookCreationDetails> HookCreationDetails_
		{
			init; get => field ??= new ListGuarded<HookCreationDetails>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}
		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.InitFromTransactionBody"]/*' />
		public ListGuarded<long> HookIdsToDelete
		{
			init; get => field ??= new ListGuarded<long>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}


		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.InitFromTransactionBody_2"]/*' />
		void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractUpdateInstance;

            ContractId = ContractId.FromProtobuf(body.ContractID);
            ProxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
            ExpirationTime = body.ExpirationTime.ToDateTimeOffset();

            if (body.AdminKey is not null)
				AdminKey = Key.FromProtobufKey(body.AdminKey);

			MaxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
            AutoRenewPeriod = body.AutoRenewPeriod.ToTimeSpan();
            ContractMemo = body.MemoWrapper;
            DeclineStakingReward = body.DeclineReward;
            StakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
            StakedNodeId = body.StakedNodeId;

            AutoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccountId);

			_HookCreationDetails.Clear();
			_HookCreationDetails.AddRange(body.HookCreationDetails.Select(_ => HookCreationDetails.FromProtobuf(_)));

			_HookIdsToDelete.Clear();
            _HookIdsToDelete.AddRange(body.HookIdsToDelete);
        }

		/// <include file="ContractUpdateTransaction.cs.xml" path='docs/member[@name="M:ContractUpdateTransaction.ToProtobuf"]/*' />
		public Proto.Services.ContractUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ContractUpdateTransactionBody { };

            if (ContractId != null)
				builder.ContractID = ContractId.ToProtobuf();

            if (ProxyAccountId != null)
				builder.ProxyAccountID = ProxyAccountId.ToProtobuf();

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

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
