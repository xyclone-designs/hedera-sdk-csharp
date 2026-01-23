// SPDX-License-Identifier: Apache-2.0
using Java.Util.Concurrent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /// <summary>
    /// Account Info Flow object.
    /// </summary>
    public class AccountInfoFlow
    {
        private static PublicKey GetAccountPublicKey(Client client, AccountId accountId)
        {
            return RequirePublicKey(accountId, new AccountInfoQuery().SetAccountId(accountId).Execute(client).key);
        }

        private static CompletableFuture<PublicKey> GetAccountPublicKeyAsync(Client client, AccountId accountId)
        {
            return new AccountInfoQuery().SetAccountId(accountId).ExecuteAsync(client).ThenApply((accountInfo) =>
            {
                return RequirePublicKey(accountId, accountInfo.key);
            });
        }

        private static PublicKey RequirePublicKey(AccountId accountId, Key key)
        {
            if (key is PublicKey)
            {
                return k;
            }

            throw new NotSupportedException("Account " + accountId + " has a KeyList key, which is not supported");
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
        public static bool VerifyTransactionSignature(Client client, AccountId accountId, Transaction<TWildcardTodo> transaction)
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
        public static CompletableFuture<bool> VerifySignatureAsync(Client client, AccountId accountId, byte[] message, byte[] signature)
        {
            return GetAccountPublicKeyAsync(client, accountId).ThenApply((pubKey) => pubKey.Verify(message, signature));
        }

        /// <summary>
        /// Asynchronously determine if the signature is valid.
        /// </summary>
        /// <param name="client">the client</param>
        /// <param name="accountId">the account id</param>
        /// <param name="transaction">the signed transaction</param>
        /// <returns>is the signature valid</returns>
        public static CompletableFuture<bool> VerifyTransactionSignatureAsync(Client client, AccountId accountId, Transaction<TWildcardTodo> transaction)
        {
            return GetAccountPublicKeyAsync(client, accountId).ThenApply((pubKey) => pubKey.VerifyTransaction(transaction));
        }
    }
}