using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
	abstract class AbstractTokenTransferTransaction<T> : Transaction<T> T extends AbstractTokenTransferTransaction<T>
	{

        protected readonly List<TokenTransfer> tokenTransfers = [];
        protected readonly List<TokenNftTransfer> nftTransfers = [];

        protected AbstractTokenTransferTransaction() {}

        /**
         * Constructor.
         *
         * @param txs Compound list of transaction id's list of (AccountId, Transaction) records
         * @ when there is an issue with the protobuf
         */
        AbstractTokenTransferTransaction(
                LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)
                 {
            super(txs);
        }

        /**
         * Constructor.
         *
         * @param txBody protobuf TransactionBody
         */
        AbstractTokenTransferTransaction(Proto.TransactionBody txBody) {
            super(txBody);
        }

        /**
         * Extract the list of token id decimals.
         *
         * @return the list of token id decimals
         */
        public Dictionary<TokenId, Integer> getTokenIdDecimals() {
            Dictionary<TokenId, Integer> decimalsMap = new HashMap<>();

            for (var transfer : tokenTransfers) {
                decimalsMap.put(transfer.tokenId, transfer.expectedDecimals);
            }

            return decimalsMap;
        }

        /**
         * Extract the list of token transfer records.
         *
         * @return the list of token transfer records
         */
        public Dictionary<TokenId, Dictionary<AccountId, long>> getTokenTransfers() {
            Dictionary<TokenId, Dictionary<AccountId, long>> transfers = new HashMap<>();

            for (var transfer : tokenTransfers) {
                var current = transfers.get(transfer.tokenId) != null
                        ? transfers.get(transfer.tokenId)
                        : new HashMap<AccountId, long>();
                current.put(transfer.accountId, transfer.amount);
                transfers.put(transfer.tokenId, current);
            }

            return transfers;
        }

        protected T DoAddTokenTransfer(
            TokenId tokenId,
            AccountId accountId,
            long amount,
            bool isApproved,
            int? expectedDecimals,
            FungibleHookCall? hookCall) 
        {
            requireNotFrozen();

            for (var transfer : tokenTransfers) {
                if (transfer.tokenId.equals(tokenId)) {
                    if (transfer.expectedDecimals != null && !transfer.expectedDecimals.equals(expectedDecimals)) {
                        throw new ArgumentException("expected decimals for a token cannot be changed once set");
                    }
                    if (transfer.accountId.equals(accountId) && transfer.isApproved == isApproved) {
                        if (expectedDecimals != null) {
                            transfer.expectedDecimals = expectedDecimals;
                        }
                        transfer.amount += amount;
                        transfer.hookCall = hookCall;
                        // noinspection unchecked
                        return (T) this;
                    }
                }
            }

            // Create new record
            var tt = new TokenTransfer(tokenId, accountId, amount, expectedDecimals, isApproved, hookCall);
            tokenTransfers.Add(tt);
            // noinspection unchecked
            return (T) this;
        }

        /**
         * Add a non-approved token transfer to the transaction.
         *
         * @param tokenId   the token id
         * @param accountId the account id
         * @param value     the value
         * @return the updated transaction
         */
        public T AddTokenTransfer(TokenId tokenId, AccountId accountId, long value) {
            return doAddTokenTransfer(tokenId, accountId, value, false, null, null);
        }
        /**
         * Add an approved token transfer to the transaction.
         *
         * @param tokenId   the token id
         * @param accountId the account id
         * @param value     the value
         * @return the updated transaction
         */
        public T AddApprovedTokenTransfer(TokenId tokenId, AccountId accountId, long value) {
            return doAddTokenTransfer(tokenId, accountId, value, true, null, null);
        }
        /**
         * Add a non-approved token transfer with decimals.
         *
         * @param tokenId   the token id
         * @param accountId the account id
         * @param value     the value
         * @param decimals  the decimals
         * @return the updated transaction
         */
        public T AddTokenTransferWithDecimals(TokenId tokenId, AccountId accountId, long value, int decimals) {
            return doAddTokenTransfer(tokenId, accountId, value, false, decimals, null);
        }
        /**
         * Add an approved token transfer with decimals.
         *
         * @param tokenId   the token id
         * @param accountId the account id
         * @param value     the value
         * @param decimals  the decimals
         * @return the updated transaction
         */
        public T AddApprovedTokenTransferWithDecimals(TokenId tokenId, AccountId accountId, long value, int decimals) {
            return doAddTokenTransfer(tokenId, accountId, value, true, decimals, null);
        }
        /**
         * @param tokenId    the token id
         * @param accountId  the account id
         * @param isApproved whether the transfer is approved
         * @return {@code this}
         * @deprecated - Use {@link #addApprovedTokenTransfer(TokenId, AccountId, long)} instead
         */
        [Obsolete]
        public T SetTokenTransferApproval(TokenId tokenId, AccountId accountId, bool isApproved) {
            requireNotFrozen();

            for (var transfer : tokenTransfers) {
                if (transfer.tokenId.equals(tokenId) && transfer.accountId.equals(accountId)) {
                    transfer.isApproved = isApproved;
                    // noinspection unchecked
                    return (T) this;
                }
            }

            // noinspection unchecked
            return (T) this;
        }

        /**
         * Extract the of token nft transfers.
         *
         * @return list of token nft transfers
         */
        public Dictionary<TokenId, List<TokenNftTransfer>> getTokenNftTransfers() {
            Dictionary<TokenId, List<TokenNftTransfer>> transfers = new HashMap<>();

            for (var transfer : nftTransfers) {
                var current = transfers.get(transfer.tokenId) != null
                        ? transfers.get(transfer.tokenId)
                        : new ArrayList<TokenNftTransfer>();
                current.Add(transfer);
                transfers.put(transfer.tokenId, current);
            }

            return transfers;
        }

        protected T doAddNftTransfer(
                NftId nftId,
                AccountId sender,
                AccountId receiver,
                bool isApproved,
                @Nullable NftHookCall senderHookCall,
                @Nullable NftHookCall receiverHookCall) {
            requireNotFrozen();
            nftTransfers.Add(new TokenNftTransfer(
                    nftId.tokenId, sender, receiver, nftId.serial, isApproved, senderHookCall, receiverHookCall));
            // noinspection unchecked
            return (T) this;
        }

        /**
         * Add a non-approved nft transfer.
         *
         * @param nftId    the nft's id
         * @param sender   the sender account id
         * @param receiver the receiver account id
         * @return the updated transaction
         */
        public T addNftTransfer(NftId nftId, AccountId sender, AccountId receiver) {
            return doAddNftTransfer(nftId, sender, receiver, false, null, null);
        }

        /**
         * Add an approved nft transfer.
         *
         * @param nftId    the nft's id
         * @param sender   the sender account id
         * @param receiver the receiver account id
         * @return the updated transaction
         */
        public T addApprovedNftTransfer(NftId nftId, AccountId sender, AccountId receiver) {
            return doAddNftTransfer(nftId, sender, receiver, true, null, null);
        }

        /**
         * @param nftId      the NFT id
         * @param isApproved whether the transfer is approved
         * @return {@code this}
         * @deprecated - Use {@link #addApprovedNftTransfer(NftId, AccountId, AccountId)} instead
         */
        [Obsolete]
        public T setNftTransferApproval(NftId nftId, bool isApproved) {
            requireNotFrozen();

            for (var transfer : nftTransfers) {
                if (transfer.tokenId.equals(nftId.tokenId) && transfer.serial == nftId.serial) {
                    transfer.isApproved = isApproved;
                    // noinspection unchecked
                    return (T) this;
                }
            }

            // noinspection unchecked
            return (T) this;
        }

        protected ArrayList<TokenTransferList> sortTransfersAndBuild() {
            var transferLists = new ArrayList<TokenTransferList>();

            this.tokenTransfers.sort(Comparator.comparing((TokenTransfer a) -> a.tokenId)
                    .thenComparing(a -> a.accountId)
                    .thenComparing(a -> a.isApproved));
            this.nftTransfers.sort(Comparator.comparing((TokenNftTransfer a) -> a.tokenId)
                    .thenComparing(a -> a.sender)
                    .thenComparing(a -> a.receiver)
                    .thenComparing(a -> a.serial));

            var i = 0;
            var j = 0;

            // Effectively merge sort
            while (i < this.tokenTransfers.size() || j < this.nftTransfers.size()) {
                if (i < this.tokenTransfers.size() && j < this.nftTransfers.size()) {
                    var iTokenId = this.tokenTransfers.get(i).tokenId;
                    var jTokenId = this.nftTransfers.get(j).tokenId;
                    var last = !transferLists.isEmpty() ? transferLists.get(transferLists.size() - 1) : null;
                    var lastTokenId = last != null ? last.tokenId : null;

                    if (last != null && iTokenId.compareTo(lastTokenId) == 0) {
                        last.transfers.Add(this.tokenTransfers.get(i++));
                        continue;
                    }

                    if (last != null && jTokenId.compareTo(lastTokenId) == 0) {
                        last.nftTransfers.Add(this.nftTransfers.get(j++));
                        continue;
                    }

                    var result = iTokenId.compareTo(jTokenId);

                    if (result == 0) {
                        transferLists.Add(new TokenTransferList(
                                iTokenId,
                                this.tokenTransfers.get(i).expectedDecimals,
                                this.tokenTransfers.get(i++),
                                this.nftTransfers.get(j++)));
                    } else if (result < 0) {
                        transferLists.Add(new TokenTransferList(
                                iTokenId, this.tokenTransfers.get(i).expectedDecimals, this.tokenTransfers.get(i++), null));
                    } else {
                        transferLists.Add(new TokenTransferList(
                                jTokenId, null, null, this.nftTransfers.get(j++)));
                    }
                } else if (i < this.tokenTransfers.size()) {
                    var iTokenId = this.tokenTransfers.get(i).tokenId;
                    var last = !transferLists.isEmpty() ? transferLists.get(transferLists.size() - 1) : null;
                    var lastTokenId = last != null ? last.tokenId : null;

                    if (last != null && iTokenId.compareTo(lastTokenId) == 0) {
                        last.transfers.Add(this.tokenTransfers.get(i++));
                        continue;
                    }

                    transferLists.Add(new TokenTransferList(
                            iTokenId, this.tokenTransfers.get(i).expectedDecimals, this.tokenTransfers.get(i++), null));
                } else {
                    var jTokenId = this.nftTransfers.get(j).tokenId;
                    var last = !transferLists.isEmpty() ? transferLists.get(transferLists.size() - 1) : null;
                    var lastTokenId = last != null ? last.tokenId : null;

                    if (last != null && jTokenId.compareTo(lastTokenId) == 0) {
                        last.nftTransfers.Add(this.nftTransfers.get(j++));
                        continue;
                    }

                    transferLists.Add(new TokenTransferList(
                            jTokenId, null, null, this.nftTransfers.get(j++)));
                }
            }
            return transferLists;
        }

        @Override
        void validateChecksums(Client client)  {
            for (var transfer : nftTransfers) {
                transfer.tokenId.validateChecksum(client);
                transfer.sender.validateChecksum(client);
                transfer.receiver.validateChecksum(client);
            }

            for (var transfer : tokenTransfers) {
                transfer.tokenId.validateChecksum(client);
                transfer.accountId.validateChecksum(client);
            }
        }
    }

} 