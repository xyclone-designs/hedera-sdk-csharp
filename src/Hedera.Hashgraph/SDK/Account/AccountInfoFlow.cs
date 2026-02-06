// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <summary>
    /// Account Info Flow object.
    /// </summary>
    public class AccountInfoFlow
    {
        private static PublicKey GetAccountPublicKey(Client client, AccountId accountId)
        {
            AccountInfo accountinfo = new AccountInfoQuery
            {
                AccountId = accountId

            }.Execute(client);

			return RequirePublicKey(accountId, accountinfo.key);
        }

        private static async Task<PublicKey> GetAccountPublicKeyAsync(Client client, AccountId accountId)
        {
			AccountInfo accountinfo = await new AccountInfoQuery
			{
				AccountId = accountId

			}.ExecuteAsync(client);

			return RequirePublicKey(accountId, accountinfo.key);
        }

        private static PublicKey RequirePublicKey(AccountId accountId, Key key)
        {
            return key as PublicKey ?? throw new NotSupportedException("Account " + accountId + " has a KeyList key, which is not supported");
        }

        /// <summary>
        /// Is the signature valid.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="accountId">the account id</param>
        /// <param name="message">the message</param>
        /// <param name="signature">the signature</param>
        /// <returns>is the signature valid</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public static bool VerifySignature(Client client, AccountId accountId, byte[] message, byte[] signature)
        {
            return GetAccountPublicKey(client, accountId).Verify(message, signature);
        }

        /// <summary>
        /// Is the transaction signature valid.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="accountId">the account id</param>
        /// <param name="transaction">the signed transaction</param>
        /// <returns>is the transaction signature valid</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public static bool VerifyTransactionSignature<T>(Client client, AccountId accountId, Transaction<T> transaction) where T : Transaction<T>
		{
            return GetAccountPublicKey(client, accountId).VerifyTransaction(transaction);
        }

        /// <summary>
        /// Asynchronously determine if the signature is valid.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="accountId">the account id</param>
        /// <param name="message">the message</param>
        /// <param name="signature">the signature</param>
        /// <returns>is the signature valid</returns>
        public static async Task<bool> VerifySignatureAsync(Client client, AccountId accountId, byte[] message, byte[] signature)
        {
            PublicKey publickey = await GetAccountPublicKeyAsync(client, accountId);

			return publickey.Verify(message, signature);
        }

        /// <summary>
        /// Asynchronously determine if the signature is valid.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="accountId">the account id</param>
        /// <param name="transaction">the signed transaction</param>
        /// <returns>is the signature valid</returns>
        public static async Task<bool> VerifyTransactionSignatureAsync<T>(Client client, AccountId accountId, Transaction<T> transaction) where T : Transaction<T>
        {
			PublicKey publickey = await GetAccountPublicKeyAsync(client, accountId);

			return publickey.VerifyTransaction(transaction);
        }
    }
}