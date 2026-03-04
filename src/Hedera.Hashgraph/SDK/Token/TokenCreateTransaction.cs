// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="T:TokenCreateTransaction"]/*' />
    public class TokenCreateTransaction : Transaction<TokenCreateTransaction>
    {
        /// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.#ctor"]/*' />
        public TokenCreateTransaction()
        {
            AutoRenewPeriod = Transaction.DEFAULT_AUTO_RENEW_PERIOD;
            DefaultMaxTransactionFee = new Hbar(40);
        }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenCreateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen"]/*' />
        public virtual uint Decimals { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_2"]/*' />
        public virtual ulong InitialSupply { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_3"]/*' />
        public virtual bool FreezeDefault { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.DeepCloneList(field)"]/*' />
		public virtual IList<CustomFee> CustomFees
		{
			get => CustomFee.DeepCloneList(field);
			set => field = CustomFee.DeepCloneList(value);
		} = [];
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_4"]/*' />
		public virtual string? TokenName { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_5"]/*' />
		public virtual string? TokenSymbol { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_6"]/*' />
		public virtual AccountId? TreasuryAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_7"]/*' />
		public virtual Key? AdminKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_8"]/*' />
		public virtual Key? KycKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_9"]/*' />
		public virtual Key? FreezeKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_10"]/*' />
		public virtual Key? WipeKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_11"]/*' />
		public virtual Key? SupplyKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_12"]/*' />
		public virtual Key? FeeScheduleKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_13"]/*' />
		public virtual Key? PauseKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_14"]/*' />
		public virtual Key? MetadataKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_15"]/*' />
		public virtual DateTimeOffset? ExpirationTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				AutoRenewPeriod = null;
				ExpirationTimeDuration = null;

			}
		}
		public virtual TimeSpan? ExpirationTimeDuration
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				AutoRenewPeriod = null;
				ExpirationTime = null;

			}
		}
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_16"]/*' />
		public virtual AccountId? AutoRenewAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_17"]/*' />
		public virtual TimeSpan? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_18"]/*' />
		public virtual string? TokenMemo { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_19"]/*' />
		public virtual byte[]? TokenMetadata { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_20"]/*' />
		public virtual TokenType TokenType { get; set { RequireNotFrozen(); field = value; } } = TokenType.FungibleCommon;
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_21"]/*' />
		public virtual TokenSupplyType TokenSupplyType { get; set { RequireNotFrozen(); field = value; } } = TokenSupplyType.Infinite;
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.RequireNotFrozen_22"]/*' />
		public virtual long MaxSupply { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.TokenCreation;

			TokenName = body.Name;
			TokenSymbol = body.Symbol;
			Decimals = body.Decimals;
			InitialSupply = body.InitialSupply;
			FreezeDefault = body.FreezeDefault;
			TokenMemo = body.Memo;
			TokenType = (TokenType)body.TokenType;
			TokenSupplyType = (TokenSupplyType)body.SupplyType;
			MaxSupply = body.MaxSupply;
			TokenMetadata = body.Metadata.ToByteArray();

			if (body.Treasury is not null)
				TreasuryAccountId = AccountId.FromProtobuf(body.Treasury);

			if (body.AutoRenewAccount is not null)
				AutoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccount);

			if (body.AdminKey is not null)
				AdminKey = Key.FromProtobufKey(body.AdminKey);

			if (body.KycKey is not null)
				KycKey = Key.FromProtobufKey(body.KycKey);

			if (body.FreezeKey is not null)
				FreezeKey = Key.FromProtobufKey(body.FreezeKey);

			if (body.WipeKey is not null)
				WipeKey = Key.FromProtobufKey(body.WipeKey);

			if (body.SupplyKey is not null)
				SupplyKey = Key.FromProtobufKey(body.SupplyKey);

			if (body.FeeScheduleKey is not null)
				FeeScheduleKey = Key.FromProtobufKey(body.FeeScheduleKey);

			if (body.PauseKey is not null)
				PauseKey = Key.FromProtobufKey(body.PauseKey);

			if (body.MetadataKey is not null)
				MetadataKey = Key.FromProtobufKey(body.MetadataKey);

			if (body.Expiry is not null)
				ExpirationTime = body.Expiry.ToDateTimeOffset();

			if (body.AutoRenewPeriod is not null)
				AutoRenewPeriod = body.AutoRenewPeriod.ToTimeSpan();

			foreach (var fee in body.CustomFees)
				CustomFees.Add(CustomFee.FromProtobuf(fee));
		}
		/// <include file="TokenCreateTransaction.cs.xml" path='docs/member[@name="M:TokenCreateTransaction.ToProtobuf"]/*' />
		public virtual Proto.TokenCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenCreateTransactionBody
			{
				Name = TokenName,
				Symbol = TokenSymbol,
				Decimals = Decimals,
				InitialSupply = InitialSupply,
				FreezeDefault = FreezeDefault,
				Memo = TokenMemo,
				TokenType = (Proto.TokenType)TokenType,
				SupplyType = (Proto.TokenSupplyType)TokenSupplyType,
				MaxSupply = MaxSupply,
				Metadata = ByteString.CopyFrom(TokenMetadata),
			};

            if (TreasuryAccountId != null)
				builder.Treasury = TreasuryAccountId.ToProtobuf();

            if (AutoRenewAccountId != null)
				builder.AutoRenewAccount = AutoRenewAccountId.ToProtobuf();

			if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

            if (KycKey != null)
				builder.KycKey = KycKey.ToProtobufKey();

            if (FreezeKey != null)
				builder.FreezeKey = FreezeKey.ToProtobufKey();

            if (WipeKey != null)
				builder.WipeKey = WipeKey.ToProtobufKey();

            if (SupplyKey != null)
				builder.SupplyKey = SupplyKey.ToProtobufKey();

            if (FeeScheduleKey != null)
				builder.FeeScheduleKey = FeeScheduleKey.ToProtobufKey();

            if (PauseKey != null)
				builder.PauseKey = PauseKey.ToProtobufKey();

            if (MetadataKey != null)
				builder.MetadataKey = MetadataKey.ToProtobufKey();

            if (ExpirationTime != null)
				builder.Expiry = ExpirationTime.Value.ToProtoTimestamp();

            if (ExpirationTimeDuration != null)
				builder.Expiry = ExpirationTimeDuration.Value.ToProtoTimestamp();

            if (AutoRenewPeriod != null)
				builder.AutoRenewPeriod = AutoRenewPeriod.Value.ToProtoDuration();

            foreach (var fee in CustomFees)
				builder.CustomFees.Add(fee.ToProtobuf());

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            foreach (var fee in CustomFees)
				fee.ValidateChecksums(client);

			TreasuryAccountId?.ValidateChecksum(client);
			AutoRenewAccountId?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenCreation = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenCreation = ToProtobuf();
        }
		public override TokenCreateTransaction FreezeWith(Client? client)
		{
			if (AutoRenewAccountId == null && client?.OperatorAccountId != null && AutoRenewPeriod != null && AutoRenewPeriod.Value.Seconds == 0)
			{
				AutoRenewAccountId = TransactionIds != null && TransactionIds.Count != 0 && TransactionIds.Current != null ? TransactionIds.Current.AccountId : client.OperatorAccountId;
			}

			return base.FreezeWith(client);
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.createToken);

			return Proto.TokenService.Descriptor.FindMethodByName(methodname);
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