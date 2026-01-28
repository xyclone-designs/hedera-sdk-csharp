// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Wipe (administratively burn) tokens held by a non-treasury account.<br/>
    /// On success, the requested tokens will be removed from the identified account
    /// and the token supply will be reduced by the amount "wiped".
    /// 
    /// This transaction MUST be signed by the token `wipe_key`.<br/>
    /// The identified token MUST exist, MUST NOT be deleted,
    /// and MUST NOT be paused.<br/>
    /// The identified token MUST have a valid `Key` set for the `wipe_key` field,
    /// and that key MUST NOT be an empty `KeyList`.<br/>
    /// The identified account MUST exist, MUST NOT be deleted, MUST be
    /// associated to the identified token, MUST NOT be frozen for the identified
    /// token, MUST NOT be the token `treasury`, and MUST hold a balance for the
    /// token or the specific serial numbers provided.<br/>
    /// This transaction SHOULD provide a value for `amount` or `serialNumbers`,
    /// but MUST NOT set both fields.
    /// 
    /// ### Block Stream Effects
    /// The new total supply for the wiped token type SHALL be recorded.
    /// </summary>
    public class TokenWipeTransaction : Transaction<TokenWipeTransaction>
    {
        private TokenId tokenId = null;
        private AccountId accountId = null;
        private long amount = 0;
        private IList<long> serials = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenWipeTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenWipeTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenWipeTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the token id.
        /// </summary>
        /// <returns>                         the token id</returns>
        public virtual TokenId GetTokenId()
        {
            return tokenId;
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// The identified token MUST exist, MUST NOT be paused, MUST NOT be
        /// deleted, and MUST NOT be expired.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenWipeTransaction SetTokenId(TokenId tokenId)
        {
            ArgumentNullException.ThrowIfNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        public virtual AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// An account identifier.<br/>
        /// This identifies the account from which tokens will be wiped.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// The identified account MUST NOT be deleted or expired.<br/>
        /// If the identified token `kyc_key` is set to a valid key, the
        /// identified account MUST have "KYC" granted.<br/>
        /// The identified account MUST NOT be the `treasury` account for the
        /// identified token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenWipeTransaction SetAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            RequireNotFrozen();
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the amount.
        /// </summary>
        /// <returns>                         the amount</returns>
        public virtual long GetAmount()
        {
            return amount;
        }

        /// <summary>
        /// An amount of fungible/common tokens to wipe.
        /// <p>
        /// If the identified token is a non-fungible/unique token type,
        /// this value MUST be exactly zero(`0`).<br/>
        /// If the identified token type is fungible/common:
        /// <ul>
        ///   <li>This value SHALL be specified in units of the smallest
        ///       denomination possible for the identified token
        ///       (<tt>10<sup>-decimals</sup></tt> whole tokens).</li>
        ///   <li>This value MUST be strictly less than `Long.MAX_VALUE`.</li>
        ///   <li>This value MUST be less than or equal to the current total
        ///       supply for the identified token.</li>
        ///   <li>This value MUST be less than or equal to the current balance
        ///       held by the identified account.</li>
        ///   <li>This value MAY be zero(`0`).</li>
        /// </ul>
        /// </summary>
        /// <param name="amount">the amount</param>
        /// <returns>{@code this}</returns>
        public virtual TokenWipeTransaction SetAmount(long amount)
        {
            RequireNotFrozen();
            amount = amount;
            return this;
        }

        /// <summary>
        /// Extract the list of serial numbers.
        /// </summary>
        /// <returns>                         the list of serial numbers</returns>
        public virtual IList<long> GetSerials()
        {
            return [.. serials];
        }

        /// <summary>
        /// A list of serial numbers to wipe.<br/>
        /// The non-fungible/unique tokens with these serial numbers will be
        /// destroyed and cannot be recovered or reused.
        /// <p>
        /// If the identified token type is a fungible/common type, this
        /// list MUST be empty.<br/>
        /// If the identified token type is non-fungible/unique:
        /// <ul>
        ///   <li>This list MUST contain at least one entry if the identified token
        ///       type is non-fungible/unique.>/li>
        ///   <li>This list MUST NOT contain more entries than the current total
        ///       supply for the identified token.</li>
        ///   <li>Every entry in this list MUST be a valid serial number for the
        ///       identified token (i.e. "collection").</li>
        ///   <li>Every entry in this list MUST be owned by the
        ///       identified account</li>
        ///   <li></li>
        /// </ul>
        /// This list MUST NOT contain more entries than the network configuration
        /// value for batch size limit, typically ten(`10`).
        /// </summary>
        /// <param name="serials">the list of serial numbers</param>
        /// <returns>{@code this}</returns>
        public virtual TokenWipeTransaction SetSerials(IList<long> serials)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(serials);
            serials = [.. serials];
            return this;
        }

        /// <summary>
        /// Add a serial number to the list of serial numbers.
        /// </summary>
        /// <param name="serial">the serial number to add</param>
        /// <returns>{@code this}</returns>
        public virtual TokenWipeTransaction AddSerial(long serial)
        {
            RequireNotFrozen();
            serials.Add(serial);
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenWipe;

            if (body.Token is not null)
            {
                tokenId = TokenId.FromProtobuf(body.Token);
            }

            if (body.Account is not null)
            {
                accountId = AccountId.FromProtobuf(body.Account);
            }

            amount = body.Amount;
            serials = body.SerialNumbers;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenWipeAccountTransactionBody}</returns>
        public virtual Proto.TokenWipeAccountTransactionBody Build()
        {
            var builder = new Proto.TokenWipeAccountTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            if (accountId != null)
            {
                builder.Account = accountId.ToProtobuf();
            }

            builder.Amount = amount;

            foreach (var serial in serials)
            {
                builder.SerialNumbers.Add(serial);
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }

            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetWipeTokenAccountMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenWipe = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenWipe = Build();
        }
    }
}