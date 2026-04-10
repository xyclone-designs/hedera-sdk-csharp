// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Account
{
    /*
     * Create a new Hedera™ account.
     *
     * If the auto_renew_account field is set, the key of the referenced account
     * MUST sign this transaction.
     * Current limitations REQUIRE that `shardID` and `realmID` both MUST be `0`.
     * This is expected to change in the future.
     *
     * ### Block Stream Effects
     * The newly created account SHALL be included in State Changes.
     */
    public sealed class AccountCreateTransaction : Transaction<AccountCreateTransaction>
    {
        private List<HookCreationDetails> _HookCreationDetails = [];

		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:AccountCreateTransaction"]/*' />
		public AccountCreateTransaction()
        {
            DefaultMaxTransactionFee = Hbar.From(5);
        }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:AccountCreateTransaction(Proto.TransactionBody)"]/*' />
		internal AccountCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:AccountCreateTransaction(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal AccountCreateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private static EvmAddress ExtractAlias(Key key)
		{
			if (key is PrivateKeyECDSA privatekeyecdsa)
			{
				return privatekeyecdsa.GetPublicKey().ToEvmAddress();
			}
			else if (key is PublicKeyECDSA publickeyecdsa)
			{
				return publickeyecdsa.ToEvmAddress();
			}
			else
			{
				throw new BadKeyException("Private key is not ECDSA");
			}
		}

		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:SetKeyWithAlias(Key)"]/*' />
		public AccountCreateTransaction SetKeyWithAlias(Key key)
        {
            RequireNotFrozen();
			Key = key;
            Alias = ExtractAlias(key);
            return this;
        }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:SetKeyWithoutAlias(Key)"]/*' />
		public AccountCreateTransaction SetKeyWithoutAlias(Key key)
		{
			RequireNotFrozen();
			Key = key;
			return this;
		}
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:SetKeyWithAlias(Key,Key)"]/*' />
		public AccountCreateTransaction SetKeyWithAlias(Key key, Key ecdsaKey)
        {
            RequireNotFrozen();
            Key = key;
            Alias = ExtractAlias(ecdsaKey);
            return this;
        }

        /// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen"]/*' />
        public Hbar InitialBalance { get; set { RequireNotFrozen(); field = value; } } = new(0);
        /// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_2"]/*' />
        public bool ReceiverSigRequired { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_3"]/*' />
		public Key? Key { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_4"]/*' />
		public AccountId? ProxyAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_5"]/*' />
		public TimeSpan AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } } = Transaction.DEFAULT_AUTO_RENEW_PERIOD;
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_6"]/*' />
		public int MaxAutomaticTokenAssociations { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_7"]/*' />
		public string AccountMemo { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_8"]/*' />
		public AccountId? StakedAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_9"]/*' />
		public long? StakedNodeId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_10"]/*' />
		public bool DeclineStakingReward { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_11"]/*' />
		public EvmAddress? Alias { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="T:Unknown"]/*' />
		public string Alias_String 
        { 
            set 
            {
				if ((value.StartsWith("0x") && value.Length == 42) || value.Length == 40)
					Alias = EvmAddress.FromString(value);
				else throw new ArgumentException("evmAddress must be an a valid EVM address with \"0x\" prefix");
			}
        }

        /// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:ToProtobuf"]/*' />
        public ListGuarded<HookCreationDetails> HookCreationDetails
		{
			init; get => field ??= new ListGuarded<HookCreationDetails>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}


		/// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:ToProtobuf_2"]/*' />
		public Proto.CryptoCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.CryptoCreateTransactionBody
            {
				InitialBalance = (ulong)InitialBalance.ToTinybars(),
				ReceiverSigRequired = ReceiverSigRequired,
				AutoRenewPeriod = AutoRenewPeriod.ToProtoDuration(),
				Memo = AccountMemo,
				MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations,
				DeclineReward = DeclineStakingReward,
			};

            if (ProxyAccountId != null)
            {
                builder.ProxyAccountID = ProxyAccountId.ToProtobuf();
            }

            if (Key != null)
            {
                builder.Key = Key.ToProtobufKey();
            }

            if (Alias != null)
            {
                builder.Alias = ByteString.CopyFrom(Alias.ToBytes());
            }

            if (StakedAccountId != null)
            {
                builder.StakedAccountId = StakedAccountId.ToProtobuf();
            }
            else if (StakedNodeId != null)
            {
                builder.StakedNodeId = StakedNodeId.Value;
            }

            foreach (HookCreationDetails hookDetails in HookCreationDetails)
            {
                builder.HookCreationDetails.Add(hookDetails.ToProtobuf());
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			ProxyAccountId?.ValidateChecksum(client);
			StakedAccountId?.ValidateChecksum(client);
		}

        /// <include file="AccountCreateTransaction.cs.xml" path='docs/member[@name="M:InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoCreateAccount;

            if (body.ProxyAccountID is not null)
            {
                ProxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
            }

            if (body.Key is not null)
            {
                Key = Key.FromProtobufKey(body.Key);
            }

            if (body.AutoRenewPeriod is not null)
            {
                AutoRenewPeriod = body.AutoRenewPeriod.ToTimeSpan();
            }

            InitialBalance = Hbar.FromTinybars((long)body.InitialBalance);
            AccountMemo = body.Memo;
            ReceiverSigRequired = body.ReceiverSigRequired;
            MaxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
            DeclineStakingReward = body.DeclineReward;
            if (body.StakedAccountId is not null)
            {
                StakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
            }

			StakedNodeId = body.StakedNodeId;
            Alias = EvmAddress.FromAliasBytes(body.Alias);

			// Initialize hook creation details
			_HookCreationDetails = [.. body.HookCreationDetails.Select(_ => Hook.HookCreationDetails.FromProtobuf(_))];
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.createAccount);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoCreateAccount = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoCreateAccount = ToProtobuf();
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