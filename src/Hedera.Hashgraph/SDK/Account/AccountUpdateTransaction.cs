// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="T:AccountUpdateTransaction"]/*' />
    public sealed class AccountUpdateTransaction : Transaction<AccountUpdateTransaction>
    {
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.#ctor"]/*' />
		public AccountUpdateTransaction() { }
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal AccountUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal AccountUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen"]/*' />
        public AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_2"]/*' />
		public Key? Key { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_3"]/*' />
        [Obsolete]
		public Key? AliasKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_4"]/*' />
		public AccountId? ProxyAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_5"]/*' />
		public DateTimeOffset? ExpirationTime { get; set { RequireNotFrozen(); field = value; } }
		public TimeSpan? ExpirationTimeDuration { get; set { RequireNotFrozen(); field = value; ExpirationTime = null; } }
        /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_6"]/*' />
        public TimeSpan? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_7"]/*' />
		public bool? ReceiverSigRequired { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_8"]/*' />
		public int? MaxAutomaticTokenAssociations { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_9"]/*' />
		public string? AccountMemo { get; set { RequireNotFrozen(); field = value; } } 
        /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_10"]/*' />
        public AccountId? StakedAccountId { get; set { RequireNotFrozen(); field = value; StakedNodeId = null; ; } }
        /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_11"]/*' />
        public long? StakedNodeId { get; set { RequireNotFrozen(); field = value; StakedAccountId = null; } }
        /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.RequireNotFrozen_12"]/*' />
        public bool? DeclineStakingReward { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="T:AccountUpdateTransaction_2"]/*' />
		public ListGuarded<HookCreationDetails> HookCreationDetails
		{
			init; get => field ??= new ListGuarded<HookCreationDetails>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}
		/// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.InitFromTransactionBody"]/*' />
		public ListGuarded<long> HookIdsToDelete
        {
            init; get => field ??= new ListGuarded<long>
            {
                OnRequireNotFrozen = RequireNotFrozen
            };
        }
       

        /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.InitFromTransactionBody_2"]/*' />
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoUpdateAccount;

            if (body.AccountIDToUpdate is not null)
            {
                AccountId = AccountId.FromProtobuf(body.AccountIDToUpdate);
            }

            if (body.ProxyAccountID is not null)
            {
                ProxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
            }

            if (body.Key is not null)
            {
                Key = Key.FromProtobufKey(body.Key);
            }

            if (body.ExpirationTime is not null)
            {
                ExpirationTime = body.ExpirationTime.ToDateTimeOffset();
            }

            if (body.AutoRenewPeriod is not null)
            {
                AutoRenewPeriod = body.AutoRenewPeriod.ToTimeSpan();
            }

            if (body.ReceiverSigRequiredWrapper is not null)
            {
                ReceiverSigRequired = body.ReceiverSigRequiredWrapper.Value;
            }

            if (body.Memo is not null)
            {
                AccountMemo = body.Memo;
            }

            if (body.MaxAutomaticTokenAssociations is not null)
            {
                MaxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
            }

            if (body.DeclineReward is not null)
            {
                DeclineStakingReward = body.DeclineReward.Value;
            }

            if (body.StakedAccountId is not null)
            {
                StakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
            }

			StakedNodeId = body.StakedNodeId;

			// Initialize hook create/delete details
			HookCreationDetails.Clear();
			HookCreationDetails.AddRange(body.HookCreationDetails.Select(_ => Hook.HookCreationDetails.FromProtobuf(_)));

			HookIdsToDelete.Clear();
            HookIdsToDelete.AddRange(body.HookIdsToDelete);
        }

        /// <include file="AccountUpdateTransaction.cs.xml" path='docs/member[@name="M:AccountUpdateTransaction.ToProtobuf"]/*' />
        public Proto.CryptoUpdateTransactionBody ToProtobuf()
        {
			Proto.CryptoUpdateTransactionBody proto = new ()
            {
				ReceiverSigRequiredWrapper = ReceiverSigRequired
			};

            if (AccountId != null)
            {
                proto.AccountIDToUpdate = AccountId.ToProtobuf();
            }

            if (ProxyAccountId != null)
            {
                proto.ProxyAccountID = ProxyAccountId.ToProtobuf();
            }

            if (Key != null)
            {
                proto.Key = Key.ToProtobufKey();
            }

            if (ExpirationTime != null)
            {
                proto.ExpirationTime = ExpirationTime.Value.ToProtoTimestamp();
            }

            if (ExpirationTimeDuration != null)
            {
                proto.ExpirationTime = ExpirationTimeDuration.Value.ToProtoTimestamp();
            }

            if (AutoRenewPeriod != null)
            {
                proto.AutoRenewPeriod = AutoRenewPeriod.Value.ToProtoDuration();
            }

            if (AccountMemo != null)
            {
                proto.Memo = AccountMemo;
            }

            if (MaxAutomaticTokenAssociations != null)
            {
                proto.MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations;
            }

            if (StakedAccountId != null)
            {
                proto.StakedAccountId = StakedAccountId.ToProtobuf();
            }
            else if (StakedNodeId != null)
            {
                proto.StakedNodeId = StakedNodeId.Value;
            }

            if (DeclineStakingReward != null)
            {
                proto.DeclineReward = DeclineStakingReward;
            }

            foreach (HookCreationDetails hookDetails in HookCreationDetails.Read)
            {
                proto.HookCreationDetails.Add(hookDetails.ToProtobuf());
            }

            if (HookIdsToDelete.Read.Count != 0)
            {
                proto.HookIdsToDelete.AddRange(HookIdsToDelete.Read);
            }

            return proto;
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.updateAccount);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}
		public override void ValidateChecksums(Client client)
		{
			AccountId?.ValidateChecksum(client);
			ProxyAccountId?.ValidateChecksum(client);
			StakedAccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoUpdateAccount = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoUpdateAccount = ToProtobuf();
        }

        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}