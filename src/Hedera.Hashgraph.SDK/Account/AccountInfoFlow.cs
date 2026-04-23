// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <include file="AccountInfoFlow.cs.xml" path='docs/member[@name="T:AccountInfoFlow"]/*' />
    public class AccountInfoFlow
    {
		private static PublicKey RequirePublicKey(AccountId accountId, Key key)
		{
			return key as PublicKey ?? throw new NotSupportedException("Account " + accountId + " has a KeyList key, which is not supported");
		}
		private static PublicKey GetAccountPublicKey(Client client, AccountId accountId)
        {
            AccountInfo accountinfo = new AccountInfoQuery
            {
                AccountId = accountId

            }.Execute(client);

			return RequirePublicKey(accountId, accountinfo.Key);
        }
        private static async Task<PublicKey> GetAccountPublicKeyAsync(Client client, AccountId accountId)
        {
			AccountInfo accountinfo = await new AccountInfoQuery
			{
				AccountId = accountId

			}.ExecuteAsync(client);

			return RequirePublicKey(accountId, accountinfo.Key);
        }

        /// <include file="AccountInfoFlow.cs.xml" path='docs/member[@name="M:AccountInfoFlow.VerifySignature(Client,AccountId,System.Byte[],System.Byte[])"]/*' />
        public static bool VerifySignature(Client client, AccountId accountId, byte[] message, byte[] signature)
        {
            return GetAccountPublicKey(client, accountId).Verify(message, signature);
        }
        /// <include file="AccountInfoFlow.cs.xml" path='docs/member[@name="M:AccountInfoFlow.VerifyTransactionSignature``1(Client,AccountId,Transaction{``0})"]/*' />
        public static bool VerifyTransactionSignature<T>(Client client, AccountId accountId, Transaction<T> transaction) where T : Transaction<T>
		{
            return GetAccountPublicKey(client, accountId).VerifyTransaction(transaction);
        }
        /// <include file="AccountInfoFlow.cs.xml" path='docs/member[@name="M:AccountInfoFlow.VerifySignatureAsync(Client,AccountId,System.Byte[],System.Byte[])"]/*' />
        public static async Task<bool> VerifySignatureAsync(Client client, AccountId accountId, byte[] message, byte[] signature)
        {
            PublicKey publickey = await GetAccountPublicKeyAsync(client, accountId);

			return publickey.Verify(message, signature);
        }
        /// <include file="AccountInfoFlow.cs.xml" path='docs/member[@name="M:AccountInfoFlow.VerifyTransactionSignatureAsync``1(Client,AccountId,Transaction{``0})"]/*' />
        public static async Task<bool> VerifyTransactionSignatureAsync<T>(Client client, AccountId accountId, Transaction<T> transaction) where T : Transaction<T>
        {
			PublicKey publickey = await GetAccountPublicKeyAsync(client, accountId);

			return publickey.VerifyTransaction(transaction);
        }
    }
}