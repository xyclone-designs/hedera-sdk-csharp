// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="T:ContractCreateTransaction"]/*' />
    public sealed class ContractCreateTransaction : Transaction<ContractCreateTransaction>
    {        
        /// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.#ctor"]/*' />
        public ContractCreateTransaction()
        {
            AutoRenewPeriod = Transaction.DEFAULT_AUTO_RENEW_PERIOD;
            DefaultMaxTransactionFee = new Hbar(20);
        }
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal ContractCreateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal ContractCreateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private List<HookCreationDetails> _HookCreationDetails = [];

		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen"]/*' />
		public FileId? BytecodeFileId
		{
			get;
			set
			{
				RequireNotFrozen();
				Bytecode = null;
				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.CopyArray"]/*' />
		public byte[]? Bytecode
		{
			get => field?.CopyArray();
			set
			{
				RequireNotFrozen();
				BytecodeFileId = null;
				field = value?.CopyArray();
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_2"]/*' />
		public Key? AdminKey
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_3"]/*' />
		public long Gas
		{
			get;
			set
			{
				RequireNotFrozen();

				if (value < 0)
					throw new ArgumentException("Gas must be non-negative", nameof(value));

				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_4"]/*' />
		public Hbar InitialBalance
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		} = new Hbar(0);
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_5"]/*' />
		[Obsolete]
		public AccountId? ProxyAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_6"]/*' />
		public int MaxAutomaticTokenAssociations
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_7"]/*' />
		public TimeSpan? AutoRenewPeriod
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.Copy"]/*' />
		public ByteString ConstructorParameters
		{
			set => field = value.Copy();
			get => field ?? ByteString.Empty;
		}
		public ContractFunctionParameters ConstructorParameters_Function
		{
			set => ConstructorParameters_Bytes = value.ToBytes(null).ToByteArray();
		}
		public byte[] ConstructorParameters_Bytes
		{
			set
			{
				RequireNotFrozen();
				ConstructorParameters = ByteString.CopyFrom(value);
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_8"]/*' />
		public string ContractMemo
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		} = string.Empty;
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_9"]/*' />
		public AccountId? StakedAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				StakedNodeId = null;
				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_10"]/*' />
		public long? StakedNodeId
		{
			get;
			set
			{
				RequireNotFrozen();
				StakedAccountId = null;
				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.RequireNotFrozen_11"]/*' />
		public bool DeclineStakingReward
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.ThrowIfNull(value)"]/*' />
		public AccountId? AutoRenewAccountId
		{
			get;
			set
			{
				ArgumentNullException.ThrowIfNull(value);
				RequireNotFrozen();
				field = value;
			}
		}

		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.ToProtobuf"]/*' />
		public ListGuarded<HookCreationDetails> HookCreationDetails_
		{
			init; get => field ??= new ListGuarded<HookCreationDetails>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}

		/// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.ToProtobuf_2"]/*' />
		public Proto.Services.ContractCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ContractCreateTransactionBody();
            
            if (BytecodeFileId != null)
				builder.FileID = BytecodeFileId.ToProtobuf();

			if (Bytecode != null)
				builder.Initcode = ByteString.CopyFrom(Bytecode);

			if (ProxyAccountId != null)
				builder.ProxyAccountID = ProxyAccountId.ToProtobuf();

			if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

			builder.MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations;

            if (AutoRenewPeriod != null)
				builder.AutoRenewPeriod = AutoRenewPeriod.Value.ToProtoDuration();

			builder.Gas = Gas;
            builder.InitialBalance = InitialBalance.ToTinybars();
            builder.ConstructorParameters = ConstructorParameters;
            builder.Memo = ContractMemo;
            builder.DeclineReward = DeclineStakingReward;

            if (StakedAccountId != null)
				builder.StakedAccountId = StakedAccountId.ToProtobuf();
			else if (StakedNodeId != null)
				builder.StakedNodeId = StakedNodeId.Value;

			if (AutoRenewAccountId != null)
				builder.AutoRenewAccountId = AutoRenewAccountId.ToProtobuf();

			foreach (HookCreationDetails hookDetails in HookCreationDetails_)
				builder.HookCreationDetails.Add(hookDetails.ToProtobuf());

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            BytecodeFileId?.ValidateChecksum(client);
            ProxyAccountId?.ValidateChecksum(client);
            StakedAccountId?.ValidateChecksum(client);
            AutoRenewAccountId?.ValidateChecksum(client);
        }

        /// <include file="ContractCreateTransaction.cs.xml" path='docs/member[@name="M:ContractCreateTransaction.InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractCreateInstance;

			BytecodeFileId = FileId.FromProtobuf(body.FileID);
			Bytecode = body.Initcode.ToByteArray();
			ProxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
			AdminKey = Key.FromProtobufKey(body.AdminKey);

			MaxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
			AutoRenewPeriod = body.AutoRenewPeriod.ToTimeSpan();

			Gas = body.Gas;
            InitialBalance = Hbar.FromTinybars(body.InitialBalance);
            ConstructorParameters = body.ConstructorParameters;
            ContractMemo = body.Memo;
            DeclineStakingReward = body.DeclineReward;

			StakedNodeId = body.StakedNodeId;
			StakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
			AutoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccountId);

            // Initialize hook creation details
            HookCreationDetails_.Clear();

            foreach (var protoHookDetails in body.HookCreationDetails)
				HookCreationDetails_.Add(HookCreationDetails.FromProtobuf(protoHookDetails));
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.createContract);

			return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractCreateInstance = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractCreateInstance = ToProtobuf();
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
