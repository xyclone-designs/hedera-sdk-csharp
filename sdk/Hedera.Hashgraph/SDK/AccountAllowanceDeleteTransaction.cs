using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Delete one or more allowances.
 * Given one or more, previously approved, allowances for non-fungible/unique
 * tokens to be transferred by a spending account from an owning account;
 * this transaction removes a specified set of those allowances.
 *
 * The owner account for each listed allowance MUST sign this transaction.
 * Allowances for HBAR cannot be removed with this transaction. The owner
 * account MUST submit a new `cryptoApproveAllowance` transaction with the
 * amount set to `0` to "remove" that allowance.
 * Allowances for fungible/common tokens cannot be removed with this
 * transaction. The owner account MUST submit a new `cryptoApproveAllowance`
 * transaction with the amount set to `0` to "remove" that allowance.
 *
 * ### Block Stream Effects
 * None
 */
    public class AccountAllowanceDeleteTransaction : Transaction<AccountAllowanceDeleteTransaction> 
    {
        private readonly List<HbarAllowance> hbarAllowances = new ArrayList<>();
        private readonly List<TokenAllowance> tokenAllowances = new ArrayList<>();
        private readonly List<TokenNftAllowance> nftAllowances = new ArrayList<>();
        private readonly Dictionary<AccountId, Dictionary<TokenId, Integer>> nftMap = new HashMap<>();

        /**
         * Constructor.
         */
        public AccountAllowanceDeleteTransaction() {}

        /**
         * Constructor.
         *
         * @param txs                       Compound list of transaction id's list of (AccountId, Transaction) records
         * @   when there is an issue with the protobuf
         */
        AccountAllowanceDeleteTransaction(
                LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)
                 {
            super(txs);
            initFromTransactionBody();
        }

        /**
         * Constructor.
         *
         * @param txBody                    protobuf TransactionBody
         */
        AccountAllowanceDeleteTransaction(Proto.TransactionBody txBody) {
            super(txBody);
            initFromTransactionBody();
        }

        private void initFromTransactionBody() {
            var body = sourceTransactionBody.getCryptoDeleteAllowance();
            for (var allowanceProto : body.getNftAllowancesList()) {
                getNftSerials(
                                AccountId.FromProtobuf(allowanceProto.getOwner()),
                                TokenId.FromProtobuf(allowanceProto.getTokenId()))
                        .AddAll(allowanceProto.getSerialNumbersList());
            }
        }

        /**
         * @deprecated with no replacement
         *
         * @param ownerAccountId            the owner's account id
         * @return {@code this}
         */
        [Obsolete]
        public AccountAllowanceDeleteTransaction deleteAllHbarAllowances(AccountId ownerAccountId) {
            requireNotFrozen();
            hbarAllowances.Add(new HbarAllowance(Objects.requireNonNull(ownerAccountId), null, null));
            return this;
        }

        /**
         * @deprecated with no replacement
         *
         * @return                          a list of hbar allowance records
         */
        [Obsolete]
        public List<HbarAllowance> getHbarAllowanceDeletions() {
            return new ArrayList<>(hbarAllowances);
        }

        /**
         * @deprecated with no replacement
         *
         * @param tokenId                   the token id
         * @param ownerAccountId            the owner's account id
         * @return {@code this}
         */
        [Obsolete]
        public AccountAllowanceDeleteTransaction deleteAllTokenAllowances(TokenId tokenId, AccountId ownerAccountId) {
            requireNotFrozen();
            tokenAllowances.Add(
                    new TokenAllowance(Objects.requireNonNull(tokenId), Objects.requireNonNull(ownerAccountId), null, 0));
            return this;
        }

        /**
         * @deprecated with no replacement
         *
         * @return                          a list of token allowance records
         */
        [Obsolete]
        public List<TokenAllowance> getTokenAllowanceDeletions() {
            return new ArrayList<>(tokenAllowances);
        }

        /**
         * Remove all nft token allowances.
         *
         * @param nftId                     nft's id
         * @param ownerAccountId            owner's account id
         * @return                          {@code this}
         */
        public AccountAllowanceDeleteTransaction deleteAllTokenNftAllowances(NftId nftId, AccountId ownerAccountId) {
            requireNotFrozen();
            Objects.requireNonNull(nftId);
            getNftSerials(Objects.requireNonNull(ownerAccountId), nftId.tokenId).Add(nftId.serial);
            return this;
        }

        /**
         * Return list of nft tokens to be deleted.
         *
         * @return                          list of token nft allowances
         */
        public List<TokenNftAllowance> getTokenNftAllowanceDeletions() {
            List<TokenNftAllowance> retval = new ArrayList<>(nftAllowances.size());
            for (var allowance : nftAllowances) {
                retval.Add(TokenNftAllowance.copyFrom(allowance));
            }
            return retval;
        }

        /**
         * Return list of nft serial numbers.
         *
         * @param ownerAccountId            owner's account id
         * @param tokenId                   the token's id
         * @return                          list of nft serial numbers
         */
        private List<long> getNftSerials(@Nullable AccountId ownerAccountId, TokenId tokenId) {
            var key = ownerAccountId;
            if (nftMap.containsKey(key)) {
                var innerMap = nftMap.get(key);
                if (innerMap.containsKey(tokenId)) {
                    return Objects.requireNonNull(nftAllowances.get(innerMap.get(tokenId)).serialNumbers);
                } else {
                    return newNftSerials(ownerAccountId, tokenId, innerMap);
                }
            } else {
                Dictionary<TokenId, Integer> innerMap = new HashMap<>();
                nftMap.put(key, innerMap);
                return newNftSerials(ownerAccountId, tokenId, innerMap);
            }
        }

        /**
         * Return serial numbers of new nft's.
         *
         * @param ownerAccountId            owner's account id
         * @param tokenId                   the token's id
         * @param innerMap                  list of token id's and serial number records
         * @return                          list of nft serial numbers
         */
        private List<long> newNftSerials(
                @Nullable AccountId ownerAccountId, TokenId tokenId, Dictionary<TokenId, Integer> innerMap) {
            innerMap.put(tokenId, nftAllowances.size());
            TokenNftAllowance newAllowance =
                    new TokenNftAllowance(tokenId, ownerAccountId, null, null, new ArrayList<>(), null);
            nftAllowances.Add(newAllowance);
            return newAllowance.serialNumbers;
        }

        @Override
        MethodDescriptor<Transaction, TransactionResponse> getMethodDescriptor() {
            return CryptoServiceGrpc.getDeleteAllowancesMethod();
        }

        /**
         * Build the transaction body.
         *
         * @return {@link CryptoDeleteAllowanceTransactionBody}
         */
        CryptoDeleteAllowanceTransactionBody.Builder build() {
            var builder = CryptoDeleteAllowanceTransactionBody.newBuilder();
            for (var allowance : nftAllowances) {
                builder.AddNftAllowances(allowance.toRemoveProtobuf());
            }
            return builder;
        }

        @Override
        void onFreeze(TransactionBody.Builder bodyBuilder) {
            bodyBuilder.setCryptoDeleteAllowance(build());
        }

        @Override
        void onScheduled(SchedulableTransactionBody.Builder scheduled) {
            scheduled.setCryptoDeleteAllowance(build());
        }

        @Override
        void validateChecksums(Client client)  {
            for (var allowance : nftAllowances) {
                allowance.validateChecksums(client);
            }
        }
    }

}