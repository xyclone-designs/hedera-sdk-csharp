// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="T:TokenUpdateTransaction"]/*' />
    public class TokenUpdateTransaction : Transaction<TokenUpdateTransaction>
    {
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.#ctor"]/*' />
        public TokenUpdateTransaction() { }
		/// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenUpdateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_2"]/*' />
        public virtual string? TokenName { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_3"]/*' />
        public virtual string? TokenSymbol { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
		/// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_4"]/*' />
		public virtual AccountId? TreasuryAccountId { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_5"]/*' />
        public virtual Key? AdminKey { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_6"]/*' />
        public virtual Key? KycKey { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_7"]/*' />
        public virtual Key? FreezeKey { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_8"]/*' />
        public virtual Key? WipeKey { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_9"]/*' />
        public virtual Key? SupplyKey { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_10"]/*' />
        public virtual Key? FeeScheduleKey { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_11"]/*' />
        public virtual Key? PauseKey { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_12"]/*' />
        public virtual Key? MetadataKey { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_13"]/*' />
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
		/// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_14"]/*' />
		public virtual AccountId? AutoRenewAccountId { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_15"]/*' />
        public virtual TimeSpan? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_16"]/*' />
		public virtual string? TokenMemo { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_17"]/*' />
        public virtual byte[]? TokenMetadata { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.RequireNotFrozen_18"]/*' />
        public virtual TokenKeyValidation TokenKeyVerificationMode { get; set { RequireNotFrozen(); field = value; } } = TokenKeyValidation.FullValidation;

		/// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenUpdate;

			TokenName = body.Name;
			TokenSymbol = body.Symbol;
			TokenKeyVerificationMode = (TokenKeyValidation)body.KeyVerificationMode;

			if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

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

			if (body.Memo is not null)
				TokenMemo = body.Memo;

			if (body.Metadata is not null)
				TokenMetadata = body.Metadata.ToByteArray();


		}

        /// <include file="TokenUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenUpdateTransactionBody
            {
				Name = TokenName,
				Symbol = TokenSymbol,
				KeyVerificationMode = (Proto.Services.TokenKeyValidation)TokenKeyVerificationMode,
			};

			if (TokenId != null)
                builder.Token = TokenId.ToProtobuf();

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

			if (TokenMemo != null)
				builder.Memo = TokenMemo;

			if (TokenMetadata != null)
				builder.Metadata = ByteString.CopyFrom(TokenMetadata);
           
            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            TokenId?.ValidateChecksum(client);
            TreasuryAccountId?.ValidateChecksum(client);
            AutoRenewAccountId?.ValidateChecksum(client);
        }
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUpdate = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUpdate = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.updateToken);

			return Proto.Services.TokenService.Descriptor.FindMethodByName(methodname);
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
