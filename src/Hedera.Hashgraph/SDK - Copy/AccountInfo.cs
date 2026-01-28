// SPDX-License-Identifier: Apache-2.0
using Java.Util.Stream.Collectors;
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Time;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Current information about an account, including the balance.
    /// </summary>
    public sealed class AccountInfo
    {
        /// <summary>
        /// The account ID for which this information applies.
        /// </summary>
        public readonly AccountId accountId;
        /// <summary>
        /// The Contract Account ID comprising both the contract instance and the cryptocurrency account owned by the
        /// contract instance, in the format used by Solidity.
        /// </summary>
        public readonly string contractAccountId;
        /// <summary>
        /// If true, then this account has been deleted, it will disappear when it expires, and all transactions for it will
        /// fail except the transaction to extend its expiration date.
        /// </summary>
        public readonly bool isDeleted;
        /// <summary>
        /// The Account ID of the account to which this is proxy staked. If proxyAccountID is null, or is an invalid account,
        /// or is an account that isn't a node, then this account is automatically proxy staked to a node chosen by the
        /// network, but without earning payments. If the proxyAccountID account refuses to accept proxy staking , or if it
        /// is not currently running a node, then it will behave as if proxyAccountID was null.
        /// </summary>
        public readonly AccountId proxyAccountId;
        /// <summary>
        /// The total proxy staked to this account.
        /// </summary>
        public readonly Hbar proxyReceived;
        /// <summary>
        /// The key for the account, which must sign in order to transfer out, or to modify the account in any way other than
        /// extending its expiration date.
        /// </summary>
        public readonly Key key;
        /// <summary>
        /// The current balance of account.
        /// </summary>
        public readonly Hbar balance;
        /// <summary>
        /// The threshold amount for which an account record is created (and this account charged for them) for any
        /// send/withdraw transaction.
        /// </summary>
        public readonly Hbar sendRecordThreshold;
        /// <summary>
        /// The threshold amount for which an account record is created (and this account charged for them) for any
        /// transaction above this amount.
        /// </summary>
        public readonly Hbar receiveRecordThreshold;
        /// <summary>
        /// If true, no transaction can transfer to this account unless signed by this account's key.
        /// </summary>
        public readonly bool isReceiverSignatureRequired;
        /// <summary>
        /// The time at which this account is set to expire.
        /// </summary>
        public readonly Timestamp expirationTime;
        /// <summary>
        /// The duration for expiration time will extend every this many seconds. If there are insufficient funds, then it
        /// extends as long as possible. If it is empty when it expires, then it is deleted.
        /// </summary>
        public readonly Duration autoRenewPeriod;
        /// <summary>
        /// All the livehashes attached to the account (each of which is a hash along with the keys that authorized it and
        /// can delete it)
        /// </summary>
        public readonly IList<LiveHash> liveHashes;
        public readonly Dictionary<TokenId, TokenRelationship> tokenRelationships;
        /// <summary>
        /// The memo associated with the account
        /// </summary>
        public readonly string accountMemo;
        /// <summary>
        /// The number of NFTs owned by this account
        /// </summary>
        public readonly long ownedNfts;
        /// <summary>
        /// The maximum number of tokens that an Account can be implicitly associated with.
        /// </summary>
        public readonly int maxAutomaticTokenAssociations;
        /// <summary>
        /// The public key which aliases to this account.
        /// </summary>
        public readonly PublicKey aliasKey;
        /// <summary>
        /// The ledger ID the response was returned from; please see <a
        /// href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the
        /// network-specific IDs.
        /// </summary>
        public readonly LedgerId ledgerId;
        /// <summary>
        /// The ethereum transaction nonce associated with this account.
        /// </summary>
        public readonly long ethereumNonce;
        /// <summary>
        /// List of Hbar allowances
        /// </summary>
        public readonly IList<HbarAllowance> hbarAllowances;
        /// <summary>
        /// List of token allowances
        /// </summary>
        public readonly IList<TokenAllowance> tokenAllowances;
        /// <summary>
        /// List of NFT allowances
        /// </summary>
        public readonly IList<TokenNftAllowance> tokenNftAllowances;
        /// <summary>
        /// Staking metadata for this account.
        /// </summary>
        public readonly StakingInfo stakingInfo;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="contractAccountId">the contracts account id</param>
        /// <param name="isDeleted">is it deleted</param>
        /// <param name="proxyAccountId">the proxy account's id</param>
        /// <param name="proxyReceived">amount of proxy received</param>
        /// <param name="key">signing key</param>
        /// <param name="balance">account balance</param>
        /// <param name="sendRecordThreshold">@depreciated no replacement</param>
        /// <param name="receiveRecordThreshold">@depreciated no replacement</param>
        /// <param name="receiverSignatureRequired">is the receiver's signature required</param>
        /// <param name="expirationTime">the expiration time</param>
        /// <param name="autoRenewPeriod">the auto renew period</param>
        /// <param name="liveHashes">the live hashes</param>
        /// <param name="tokenRelationships">list of token id and token relationship records</param>
        /// <param name="accountMemo">the account memo</param>
        /// <param name="ownedNfts">number of nft's</param>
        /// <param name="maxAutomaticTokenAssociations">max number of token associations</param>
        /// <param name="aliasKey">public alias key</param>
        /// <param name="ledgerId">the ledger id</param>
        private AccountInfo(AccountId accountId, string contractAccountId, bool isDeleted, AccountId proxyAccountId, long proxyReceived, Key key, long balance, long sendRecordThreshold, long receiveRecordThreshold, bool receiverSignatureRequired, Timestamp expirationTime, Duration autoRenewPeriod, IList<LiveHash> liveHashes, Dictionary<TokenId, TokenRelationship> tokenRelationships, string accountMemo, long ownedNfts, int maxAutomaticTokenAssociations, PublicKey aliasKey, LedgerId ledgerId, long ethereumNonce, StakingInfo stakingInfo)
        {
            accountId = accountId;
            contractAccountId = contractAccountId;
            isDeleted = isDeleted;
            proxyAccountId = proxyAccountId;
            proxyReceived = Hbar.FromTinybars(proxyReceived);
            key = key;
            balance = Hbar.FromTinybars(balance);
            sendRecordThreshold = Hbar.FromTinybars(sendRecordThreshold);
            receiveRecordThreshold = Hbar.FromTinybars(receiveRecordThreshold);
            isReceiverSignatureRequired = receiverSignatureRequired;
            expirationTime = expirationTime;
            autoRenewPeriod = autoRenewPeriod;
            liveHashes = liveHashes;
            tokenRelationships = Collections.UnmodifiableMap(tokenRelationships);
            accountMemo = accountMemo;
            ownedNfts = ownedNfts;
            maxAutomaticTokenAssociations = maxAutomaticTokenAssociations;
            aliasKey = aliasKey;
            ledgerId = ledgerId;
            ethereumNonce = ethereumNonce;
            hbarAllowances = Collections.EmptyList();
            tokenAllowances = Collections.EmptyList();
            tokenNftAllowances = Collections.EmptyList();
            stakingInfo = stakingInfo;
        }

        /// <summary>
        /// Retrieve the account info from a protobuf.
        /// </summary>
        /// <param name="accountInfo">the account info protobuf</param>
        /// <returns>the account info object</returns>
        static AccountInfo FromProtobuf(CryptoGetInfoResponse.AccountInfo accountInfo)
        {
            var accountId = AccountId.FromProtobuf(accountInfo.GetAccountID());
            var proxyAccountId = accountInfo.GetProxyAccountID().GetAccountNum() > 0 ? AccountId.FromProtobuf(accountInfo.GetProxyAccountID()) : null;
            var liveHashes = Array.Stream(accountInfo.GetLiveHashesList().ToArray()).Map((liveHash) => LiveHash.FromProtobuf((Proto.LiveHash)liveHash)).Collect(ToList());
            Dictionary<TokenId, TokenRelationship> relationships = [];
            foreach (Proto.TokenRelationship relationship in accountInfo.GetTokenRelationshipsList())
            {
                TokenId tokenId = TokenId.FromProtobuf(relationship.GetTokenId());
                relationships.Add(tokenId, TokenRelationship.FromProtobuf(relationship));
            }

            var aliasKey = PublicKey.FromAliasBytes(accountInfo.GetAlias());
            return new AccountInfo(accountId, accountInfo.GetContractAccountID(), accountInfo.GetDeleted(), proxyAccountId, accountInfo.GetProxyReceived(), Key.FromProtobufKey(accountInfo.GetKey()), accountInfo.GetBalance(), accountInfo.GetGenerateSendRecordThreshold(), accountInfo.GetGenerateReceiveRecordThreshold(), accountInfo.GetReceiverSigRequired(), Utils.TimestampConverter.FromProtobuf(accountInfo.GetExpirationTime()), Utils.DurationConverter.FromProtobuf(accountInfo.GetAutoRenewPeriod()), liveHashes, relationships, accountInfo.GetMemo(), accountInfo.GetOwnedNfts(), accountInfo.GetMaxAutomaticTokenAssociations(), aliasKey, LedgerId.FromByteString(accountInfo.GetLedgerId()), accountInfo.GetEthereumNonce(), accountInfo.HasStakingInfo() ? StakingInfo.FromProtobuf(accountInfo.GetStakingInfo()) : null);
        }

        /// <summary>
        /// Retrieve the account info from a protobuf byte array.
        /// </summary>
        /// <param name="bytes">a byte array representing the protobuf</param>
        /// <returns>the account info object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static AccountInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(CryptoGetInfoResponse.AccountInfo.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Convert an account info object into a protobuf.
        /// </summary>
        /// <returns>the protobuf object</returns>
        CryptoGetInfoResponse.AccountInfo ToProtobuf()
        {
            var hashes = Array.Stream(liveHashes.ToArray()).Map((liveHash) => ((LiveHash)liveHash).ToProtobuf()).Collect(ToList());
            var accountInfoBuilder = CryptoGetInfoResponse.AccountInfo.NewBuilder().SetAccountID(accountId.ToProtobuf()).SetDeleted(isDeleted).SetProxyReceived(proxyReceived.ToTinybars()).SetKey(key.ToProtobufKey()).SetBalance(balance.ToTinybars()).SetGenerateSendRecordThreshold(sendRecordThreshold.ToTinybars()).SetGenerateReceiveRecordThreshold(receiveRecordThreshold.ToTinybars()).SetReceiverSigRequired(isReceiverSignatureRequired).SetExpirationTime(Utils.TimestampConverter.ToProtobuf(expirationTime)).SetAutoRenewPeriod(Utils.DurationConverter.ToProtobuf(autoRenewPeriod)).AddAllLiveHashes(hashes).SetMemo(accountMemo).SetOwnedNfts(ownedNfts).SetMaxAutomaticTokenAssociations(maxAutomaticTokenAssociations).SetLedgerId(ledgerId.ToByteString()).SetEthereumNonce(ethereumNonce);
            if (contractAccountId != null)
            {
                accountInfoBuilder.SetContractAccountID(contractAccountId);
            }

            if (proxyAccountId != null)
            {
                accountInfoBuilder.SetProxyAccountID(proxyAccountId.ToProtobuf());
            }

            if (aliasKey != null)
            {
                accountInfoBuilder.SetAlias(aliasKey.ToProtobufKey().ToByteString());
            }

            if (stakingInfo != null)
            {
                accountInfoBuilder.SetStakingInfo(stakingInfo.ToProtobuf());
            }

            return accountInfoBuilder.Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("accountId", accountId).Add("contractAccountId", contractAccountId).Add("deleted", isDeleted).Add("proxyAccountId", proxyAccountId).Add("proxyReceived", proxyReceived).Add("key", key).Add("balance", balance).Add("sendRecordThreshold", sendRecordThreshold).Add("receiveRecordThreshold", receiveRecordThreshold).Add("receiverSignatureRequired", isReceiverSignatureRequired).Add("expirationTime", expirationTime).Add("autoRenewPeriod", autoRenewPeriod).Add("liveHashes", liveHashes).Add("tokenRelationships", tokenRelationships).Add("accountMemo", accountMemo).Add("ownedNfts", ownedNfts).Add("maxAutomaticTokenAssociations", maxAutomaticTokenAssociations).Add("aliasKey", aliasKey).Add("ledgerId", ledgerId).Add("ethereumNonce", ethereumNonce).Add("stakingInfo", stakingInfo).ToString();
        }

        /// <summary>
        /// Extract a byte array representation.
        /// </summary>
        /// <returns>a byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}