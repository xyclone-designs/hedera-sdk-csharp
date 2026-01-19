namespace Hedera.Hashgraph.SDK
{
	/**
 * Account Info Flow object.
 */
public class AccountInfoFlow {

    private static PublicKey getAccountPublicKey(Client client, AccountId accountId)
            , TimeoutException {
        return requirePublicKey(
                accountId, new AccountInfoQuery().setAccountId(accountId).execute(client).key);
    }

    private static Task<PublicKey> getAccountPublicKeyAsync(Client client, AccountId accountId) {
        return new AccountInfoQuery()
                .setAccountId(accountId)
                .executeAsync(client)
                .thenApply(accountInfo -> {
                    return requirePublicKey(accountId, accountInfo.key);
                });
    }

    private static PublicKey requirePublicKey(AccountId accountId, Key key) {
        if (key is PublicKey k) {
            return k;
        }
        throw new UnsupportedOperationException("Account " + accountId + " has a KeyList key, which is not supported");
    }

    /**
     * Is the signature valid.
     *
     * @param client    the client
     * @param accountId the account id
     * @param message   the message
     * @param signature the signature
     * @return is the signature valid
     * @ when the precheck fails
     * @        when the transaction times out
     */
    public static bool verifySignature(Client client, AccountId accountId, byte[] message, byte[] signature)
            , TimeoutException {
        return getAccountPublicKey(client, accountId).verify(message, signature);
    }

    /**
     * Is the transaction signature valid.
     *
     * @param client      the client
     * @param accountId   the account id
     * @param transaction the signed transaction
     * @return is the transaction signature valid
     * @ when the precheck fails
     * @        when the transaction times out
     */
    public static bool verifyTransactionSignature(Client client, AccountId accountId, Transaction<?> transaction)
            , TimeoutException {
        return getAccountPublicKey(client, accountId).verifyTransaction(transaction);
    }

    /**
     * Asynchronously determine if the signature is valid.
     *
     * @param client    the client
     * @param accountId the account id
     * @param message   the message
     * @param signature the signature
     * @return is the signature valid
     */
    public static Task<Boolean> verifySignatureAsync(
            Client client, AccountId accountId, byte[] message, byte[] signature) {
        return getAccountPublicKeyAsync(client, accountId).thenApply(pubKey -> pubKey.verify(message, signature));
    }

    /**
     * Asynchronously determine if the signature is valid.
     *
     * @param client      the client
     * @param accountId   the account id
     * @param transaction the signed transaction
     * @return is the signature valid
     */
    public static Task<Boolean> verifyTransactionSignatureAsync(
            Client client, AccountId accountId, Transaction<?> transaction) {
        return getAccountPublicKeyAsync(client, accountId).thenApply(pubKey -> pubKey.verifyTransaction(transaction));
    }
}

}