// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.LiveHashes;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <summary>
    /// Current information about an account, including the balance.
    /// </summary>
    public sealed class AccountInfo
    {
        /// <summary>
        /// The account ID for which this information applies.
        /// </summary>
        public readonly AccountId AccountId;
        /// <summary>
        /// The Contract Account ID comprising both the contract instance and the cryptocurrency account owned by the
        /// contract instance, in the format used by Solidity.
        /// </summary>
        public readonly string ContractAccountId;
        /// <summary>
        /// If true, then this account has been deleted, it will disappear when it expires, and all transactions for it will
        /// fail except the transaction to extend its expiration date.
        /// </summary>
        public readonly bool IsDeleted;
        /// <summary>
        /// The Account ID of the account to which this is proxy staked. If proxyAccountID is null, or is an invalid account,
        /// or is an account that isn't a node, then this account is automatically proxy staked to a node chosen by the
        /// network, but without earning payments. If the proxyAccountID account refuses to accept proxy staking , or if it
        /// is not currently running a node, then it will behave as if proxyAccountID was null.
        /// </summary>
        public readonly AccountId? ProxyAccountId;
        /// <summary>
        /// The total proxy staked to this account.
        /// </summary>
        public readonly Hbar ProxyReceived;
        /// <summary>
        /// The key for the account, which must sign in order to transfer out, or to modify the account in any way other than
        /// extending its expiration date.
        /// </summary>
        public readonly Key Key;
        /// <summary>
        /// The current balance of account.
        /// </summary>
        public readonly Hbar Balance;
        /// <summary>
        /// The threshold amount for which an account record is created (and this account charged for them) for any
        /// send/withdraw transaction.
        /// </summary>
        public readonly Hbar SendRecordThreshold;
        /// <summary>
        /// The threshold amount for which an account record is created (and this account charged for them) for any
        /// transaction above this amount.
        /// </summary>
        public readonly Hbar ReceiveRecordThreshold;
        /// <summary>
        /// If true, no transaction can transfer to this account unless signed by this account's key.
        /// </summary>
        public readonly bool IsReceiverSignatureRequired;
        /// <summary>
        /// The time at which this account is set to expire.
        /// </summary>
        public readonly Timestamp ExpirationTime;
        /// <summary>
        /// The duration for expiration time will extend every this many seconds. If there are insufficient funds, then it
        /// extends as long as possible. If it is empty when it expires, then it is deleted.
        /// </summary>
        public readonly Duration AutoRenewPeriod;
        /// <summary>
        /// All the livehashes attached to the account (each of which is a hash along with the keys that authorized it and
        /// can delete it)
        /// </summary>
        public readonly IList<LiveHash> LiveHashes;
        public readonly Dictionary<TokenId, TokenRelationship> TokenRelationships;
        /// <summary>
        /// The memo associated with the account
        /// </summary>
        public readonly string AccountMemo;
        /// <summary>
        /// The number of NFTs owned by this account
        /// </summary>
        public readonly long OwnedNfts;
        /// <summary>
        /// The maximum number of tokens that an Account can be implicitly associated with.
        /// </summary>
        public readonly int MaxAutomaticTokenAssociations;
        /// <summary>
        /// The public key which aliases to this account.
        /// </summary>
        public readonly PublicKey? AliasKey;
        /// <summary>
        /// The ledger ID the response was returned from; please see <a
        /// href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the
        /// network-specific IDs.
        /// </summary>
        public readonly LedgerId LedgerId;
        /// <summary>
        /// The ethereum transaction nonce associated with this account.
        /// </summary>
        public readonly long EthereumNonce;
        /// <summary>
        /// List of Hbar allowances
        /// </summary>
        public readonly IList<HbarAllowance> HbarAllowances;
        /// <summary>
        /// List of token allowances
        /// </summary>
        public readonly IList<TokenAllowance> TokenAllowances;
        /// <summary>
        /// List of NFT allowances
        /// </summary>
        public readonly IList<TokenNftAllowance> TokenNftAllowances;
        /// <summary>
        /// Staking metadata for this account.
        /// </summary>
        public readonly StakingInfo? StakingInfo;
        
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
        private AccountInfo(AccountId accountId, string contractAccountId, bool isDeleted, AccountId? proxyAccountId, long proxyReceived, Key key, long balance, long sendRecordThreshold, long receiveRecordThreshold, bool receiverSignatureRequired, Timestamp expirationTime, Duration autoRenewPeriod, IList<LiveHash> liveHashes, Dictionary<TokenId, TokenRelationship> tokenRelationships, string accountMemo, long ownedNfts, int maxAutomaticTokenAssociations, PublicKey? aliasKey, LedgerId ledgerId, long ethereumNonce, StakingInfo? stakingInfo)
        {
            AccountId = accountId;
            ContractAccountId = contractAccountId;
            IsDeleted = isDeleted;
            ProxyAccountId = proxyAccountId;
            ProxyReceived = Hbar.FromTinybars(proxyReceived);
            Key = key;
            Balance = Hbar.FromTinybars(balance);
            SendRecordThreshold = Hbar.FromTinybars(sendRecordThreshold);
            ReceiveRecordThreshold = Hbar.FromTinybars(receiveRecordThreshold);
            IsReceiverSignatureRequired = receiverSignatureRequired;
            ExpirationTime = expirationTime;
            AutoRenewPeriod = autoRenewPeriod;
            LiveHashes = liveHashes;
            TokenRelationships = new Dictionary<TokenId, TokenRelationship>(tokenRelationships);
            AccountMemo = accountMemo;
            OwnedNfts = ownedNfts;
            MaxAutomaticTokenAssociations = maxAutomaticTokenAssociations;
            AliasKey = aliasKey;
            LedgerId = ledgerId;
            EthereumNonce = ethereumNonce;
            HbarAllowances = [];
            TokenAllowances = [];
            TokenNftAllowances = [];
            StakingInfo = stakingInfo;
        }
	
        /// <summary>
		/// Retrieve the account info from a protobuf byte array.
		/// </summary>
		/// <param name="bytes">a byte array representing the protobuf</param>
		/// <returns>the account info object</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static AccountInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.CryptoGetInfoResponse.Types.AccountInfo.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Retrieve the account info from a protobuf.
		/// </summary>
		/// <param name="accountInfo">the account info protobuf</param>
		/// <returns>the account info object</returns>
		public static AccountInfo FromProtobuf(Proto.CryptoGetInfoResponse.Types.AccountInfo accountInfo)
        {
            return new AccountInfo(
				AccountId.FromProtobuf(accountInfo.AccountID), 
                accountInfo.ContractAccountID, 
                accountInfo.Deleted,
				accountInfo.ProxyAccountID.AccountNum > 0 ? AccountId.FromProtobuf(accountInfo.ProxyAccountID) : null, 
                accountInfo.ProxyReceived,
                Key.FromProtobufKey(accountInfo.Key), 
                (long)accountInfo.Balance,
                (long)accountInfo.GenerateSendRecordThreshold, 
                (long)accountInfo.GenerateReceiveRecordThreshold, 
                accountInfo.ReceiverSigRequired, 
                Utils.TimestampConverter.FromProtobuf(accountInfo.ExpirationTime),
                Utils.DurationConverter.FromProtobuf(accountInfo.AutoRenewPeriod),
				[.. accountInfo.LiveHashes.Select(_ => LiveHash.FromProtobuf(_))],
				accountInfo.TokenRelationships.ToDictionary(_ => TokenId.FromProtobuf(_.TokenId), _ => TokenRelationship.FromProtobuf(_)), 
                accountInfo.Memo,
                accountInfo.OwnedNfts, 
                accountInfo.MaxAutomaticTokenAssociations,
				PublicKey.FromAliasBytes(accountInfo.Alias), 
                LedgerId.FromByteString(accountInfo.LedgerId), 
                accountInfo.EthereumNonce, 
                StakingInfo.FromProtobuf(accountInfo.StakingInfo));
        }

		/// <summary>
		/// Extract a byte array representation.
		/// </summary>
		/// <returns>a byte array representation</returns>
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Convert an account info object into a protobuf.
		/// </summary>
		/// <returns>the protobuf object</returns>
		public Proto.CryptoGetInfoResponse.Types.AccountInfo ToProtobuf()
        {
			Proto.CryptoGetInfoResponse.Types.AccountInfo proto = new ()
            {
				AccountID = AccountId.ToProtobuf(),
				Deleted = IsDeleted,
				ProxyReceived = ProxyReceived.ToTinybars(),
				Key = Key.ToProtobufKey(),
				Balance = (ulong)Balance.ToTinybars(),
				GenerateSendRecordThreshold = (ulong)SendRecordThreshold.ToTinybars(),
				GenerateReceiveRecordThreshold = (ulong)ReceiveRecordThreshold.ToTinybars(),
				ReceiverSigRequired = IsReceiverSignatureRequired,
				ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime),
				AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(AutoRenewPeriod),
				Memo = AccountMemo,
				OwnedNfts = OwnedNfts,
				MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations,
				LedgerId = LedgerId.ToByteString(),
				EthereumNonce = EthereumNonce,
			};

            proto.LiveHashes.AddRange(LiveHashes.Select(_ => _.ToProtobuf()));
            
            if (ContractAccountId != null)
				proto.ContractAccountID = ContractAccountId;

			if (ProxyAccountId != null)
				proto.ProxyAccountID = ProxyAccountId.ToProtobuf();

            if (AliasKey != null)
				proto.Alias = AliasKey.ToProtobufKey().ToByteString();

            if (StakingInfo != null)
				proto.StakingInfo = StakingInfo.ToProtobuf();

            return proto;
        }
    }
}