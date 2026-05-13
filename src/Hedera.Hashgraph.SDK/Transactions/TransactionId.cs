// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Exceptions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="TransactionId.cs.xml" path='docs/member[@name="T:TransactionId"]' />
    /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.#ctor(AccountId,DateTimeOffset)"]' />
    public sealed class TransactionId(AccountId? accountId, DateTimeOffset? validStart) : IComparable<TransactionId>
    {
		private static readonly long NANOSECONDS_PER_MILLISECOND = 1000000;
		private static readonly long TIMESTAMP_INCREMENT_NANOSECONDS = 1000;
		private static readonly long NANOSECONDS_TO_REMOVE = 10000000000;
		private static long monotonicTime = -1;

        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.Generate(AccountId)"]' />
        public static TransactionId Generate(AccountId accountId)
		{
			long currentTime;
			long lastTime;

			// Loop to ensure the generated timestamp is strictly increasing,
			// and it handles the case where the system clock appears to move backward
			// or if multiple threads attempt to generate a timestamp concurrently.
			do
			{

				// Get the current time in nanoseconds and remove a few seconds to allow for some time drift
				// between the client and the receiving node and prevented spurious INVALID_TRANSACTION_START.
				currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * NANOSECONDS_PER_MILLISECOND - NANOSECONDS_TO_REMOVE;

				// Get the last recorded timestamp.
				lastTime = Interlocked.Read(ref monotonicTime);

				// If the current time is less than or equal to the last recorded time,
				// adjust the timestamp to ensure it is strictly increasing.
				if (currentTime <= lastTime)
				{
					currentTime = lastTime + TIMESTAMP_INCREMENT_NANOSECONDS;
				}
			}
			while (Interlocked.CompareExchange(ref monotonicTime, lastTime, currentTime) != lastTime);

			return new TransactionId(accountId, DateTimeOffset.UtcNow.AddNanoseconds((int)(currentTime + Random.Shared.NextInt64(1000))));
		}
		/// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.WithValidStart(AccountId,DateTimeOffset)"]' />
		public static TransactionId WithValidStart(AccountId accountId, DateTimeOffset validStart)
        {
			return new TransactionId(accountId, validStart);
        }

        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.FromString(System.String)"]' />
        public static TransactionId FromString(string s)
        {
            var parts = s.Split("/", 2);
            
            int? nonce = (parts.Length == 2) ? int.Parse(parts[1]) : null;
            parts = parts[0].Split("?", 2);
            
            var scheduled = parts.Length == 2 && parts[1].Equals("scheduled");
            parts = parts[0].Split("@", 2);

            if (parts.Length != 2)
                throw new ArgumentException("expecting {account}@{seconds}.{nanos}[?scheduled][/nonce]");

            AccountId? accountId = AccountId.FromString(parts[0]);

            var validStartParts = parts[1].Split(".", 2);

            if (validStartParts.Length != 2)
                throw new ArgumentException("expecting {account}@{seconds}.{nanos}");

            DateTimeOffset validStart = DateTimeOffset
                .FromUnixTimeSeconds(long.Parse(validStartParts[0]))
				.AddNanoseconds(long.Parse(validStartParts[1]));

            return new TransactionId(accountId, validStart)
            {
				Scheduled = scheduled,
				Nonce = nonce
			};
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.FromBytes(System.Byte[])"]' />
        public static TransactionId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.TransactionID.Parser.ParseFrom(bytes));
        }
		/// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.FromProtobuf(Proto.Services.TransactionId)"]' />
		public static TransactionId FromProtobuf(Proto.Services.TransactionID transactionId)
		{
			return new TransactionId(AccountId.FromProtobuf(transactionId.AccountId), transactionId.TransactionValidStart.ToDateTimeOffset())
			{
				Scheduled = transactionId.Scheduled,
				Nonce = transactionId.Nonce != 0 ? transactionId.Nonce : null
			};
		}

		/// <include file="TransactionId.cs.xml" path='docs/member[@name="P:TransactionId.Nonce"]' />
		public int? Nonce { get; set; }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="P:TransactionId.Scheduled"]' />
        public bool Scheduled { get; set; } = false;
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="P:TransactionId.AccountId"]' />
        public AccountId? AccountId { get; } = accountId;
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="P:TransactionId.ValidStart"]' />
        public DateTimeOffset ValidStart { get; } = validStart ?? DateTimeOffset.UtcNow;

        private string ToStringPostfix()
		{
			return "@" + ValidStart.ToUnixTimeSeconds() + "." + ValidStart.Nanosecond + (Scheduled ? "?scheduled" : "") + (Nonce != null ? "/" + Nonce : "");
		}

		/// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetReceipt(Client)"]' />
		public TransactionReceipt GetReceipt(Client client)
        {
            return GetReceipt(client, client.RequestTimeout);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetReceipt(Client,System.TimeSpan)"]' />
        public TransactionReceipt GetReceipt(Client client, TimeSpan timeout)
        {
            var receipt = new TransactionReceiptQuery
            {
				PaymentTransactionId = this

			}.Execute(client, timeout);

            if (receipt.Status != ResponseStatus.Success)
				throw new ReceiptStatusException(this, receipt);

			return receipt;
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetReceiptAsync(Client)"]' />
        public Task<TransactionReceipt> GetReceiptAsync(Client client)
        {
            return GetReceiptAsync(client, client.RequestTimeout);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetReceiptAsync(Client,System.TimeSpan)"]' />
        public async Task<TransactionReceipt> GetReceiptAsync(Client client, TimeSpan timeout)
        {
            TransactionReceipt transactionreceipt = await new TransactionReceiptQuery
            {
                PaymentTransactionId = this

            }.ExecuteAsync(client, timeout);

			if (transactionreceipt.Status != ResponseStatus.Success)
				throw new ReceiptStatusException(this, transactionreceipt);

			return transactionreceipt;
		}
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetReceiptAsync(Client,System.Action{TransactionReceipt,System.Exception})"]' />
        public void GetReceiptAsync(Client client, Action<TransactionReceipt?, Exception?> callback)
        {
            Utils.ActionHelper.Action(GetReceiptAsync(client), callback);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetReceiptAsync(Client,System.TimeSpan,System.Action{TransactionReceipt,System.Exception})"]' />
        public void GetReceiptAsync(Client client, TimeSpan timeout, Action<TransactionReceipt?, Exception?> callback)
        {
            Utils.ActionHelper.Action(GetReceiptAsync(client, timeout), callback);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetReceiptAsync(Client,System.Action{TransactionReceipt},System.Action{System.Exception})"]' />
        public void GetReceiptAsync(Client client, Action<TransactionReceipt> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(GetReceiptAsync(client), onSuccess, onFailure);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetReceiptAsync(Client,System.TimeSpan,System.Action{TransactionReceipt},System.Action{System.Exception})"]' />
        public void GetReceiptAsync(Client client, TimeSpan timeout, Action<TransactionReceipt> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(GetReceiptAsync(client, timeout), onSuccess, onFailure);
        }

        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetRecord(Client)"]' />
        public TransactionRecord GetRecord(Client client)
        {
            return GetRecord(client, client.RequestTimeout);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetRecord(Client,System.TimeSpan)"]' />
        public TransactionRecord GetRecord(Client client, TimeSpan timeout)
        {
            GetReceipt(client, timeout);

            return new TransactionRecordQuery
            {
                PaymentTransactionId = this

			}.Execute(client, timeout);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetRecordAsync(Client)"]' />
        public Task<TransactionRecord> GetRecordAsync(Client client)
        {
            return GetRecordAsync(client, client.RequestTimeout);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetRecordAsync(Client,System.TimeSpan)"]' />
        public async Task<TransactionRecord> GetRecordAsync(Client client, TimeSpan timeout)
        {
            // note: we get the receipt first to ensure consensus has been reached
            TransactionReceipt _ = await GetReceiptAsync(client, timeout);
			
            return await new TransactionRecordQuery
			{
				TransactionId = this

			}.ExecuteAsync(client, timeout);
		}
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetRecordAsync(Client,System.Action{TransactionRecord,System.Exception})"]' />
        public void GetRecordAsync(Client client, Action<TransactionRecord?, Exception?> callback)
        {
            Utils.ActionHelper.Action(GetRecordAsync(client), callback);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetRecordAsync(Client,System.TimeSpan,System.Action{TransactionRecord,System.Exception})"]' />
        public void GetRecordAsync(Client client, TimeSpan timeout, Action<TransactionRecord?, Exception?> callback)
        {
            Utils.ActionHelper.Action(GetRecordAsync(client, timeout), callback);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetRecordAsync(Client,System.Action{TransactionRecord},System.Action{System.Exception})"]' />
        public void GetRecordAsync(Client client, Action<TransactionRecord> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(GetRecordAsync(client), onSuccess, onFailure);
        }
        /// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.GetRecordAsync(Client,System.TimeSpan,System.Action{TransactionRecord},System.Action{System.Exception})"]' />
        public void GetRecordAsync(Client client, TimeSpan timeout, Action<TransactionRecord> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(GetRecordAsync(client, timeout), onSuccess, onFailure);
        }

		public int CompareTo(TransactionId? o)
		{
            if (o is null)
                return 1;

			if (Scheduled != o.Scheduled)
                return Scheduled ? 1 : -1;

            int accountIdComparison = AccountId.CompareTo(o.AccountId);

            if (accountIdComparison != 0) return accountIdComparison;

            return ValidStart.CompareTo(o.ValidStart);
		}

		/// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.ToBytes"]' />
		public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.ToProtobuf"]' />
		public Proto.Services.TransactionID ToProtobuf()
		{
			return new Proto.Services.TransactionID
            {
                AccountId = AccountId.ToProtobuf(),
                Scheduled = Scheduled,
                Nonce = Nonce ?? 0,
                TransactionValidStart = ValidStart.ToProtoTimestamp()
            };
        }
		/// <include file="TransactionId.cs.xml" path='docs/member[@name="M:TransactionId.ToStringWithChecksum(Client)"]' />
		public string ToStringWithChecksum(Client client)
		{
            return "" + AccountId.ToStringWithChecksum(client) + ToStringPostfix();
        }

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
		public override string ToString()
		{
            return "" + AccountId + ToStringPostfix();
        }
		public override bool Equals(object? @object)
        {
            return 
                @object is TransactionId id && 
                id.AccountId.Equals(AccountId) && 
                id.ValidStart.Equals(ValidStart) && 
                Scheduled == id.Scheduled;
        }        
    }
}
