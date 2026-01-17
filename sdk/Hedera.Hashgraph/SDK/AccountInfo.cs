using Google.Protobuf.WellKnownTypes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Current information about an account, including the balance.
 */
	public sealed class AccountInfo
	{
		/**
		 * The memo associated with the account
		 */
		public readonly string AccountMemo;
		/**
		 * The Contract Account ID comprising both the contract instance and the cryptocurrency account owned by the
		 * contract instance, in the format used by Solidity.
		 */
		public readonly string ContractAccountId;

		/**
		 * If true, then this account has been deleted, it will disappear when it expires, and all transactions for it will
		 * fail except the transaction to extend its expiration date.
		 */
		public readonly bool IsDeleted;
		/**
		 * If true, no transaction can transfer to this account unless signed by this account's key.
		 */
		public readonly bool IsReceiverSignatureRequired;

		/**
		 * The ledger ID the response was returned from; please see <a
		 * href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the
		 * network-specific IDs.
		 */
		public readonly LedgerId LedgerId;
		/**
		 * The account ID for which this information applies.
		 */
		public readonly AccountId AccountId;
		/**
		 * The public key which aliases to this account.
		 */
		public readonly PublicKey? AliasKey;
		/**
		 * The Account ID of the account to which this is proxy staked. If proxyAccountID is null, or is an invalid account,
		 * or is an account that isn't a node, then this account is automatically proxy staked to a node chosen by the
		 * network, but without earning payments. If the proxyAccountID account refuses to accept proxy staking , or if it
		 * is not currently running a node, then it will behave as if proxyAccountID was null.
		 */
		public readonly AccountId? ProxyAccountId;
		/**
		 * Staking metadata for this account.
		 */
		public readonly StakingInfo? StakingInfo;

		public readonly List<LiveHash> LiveHashes;
		public readonly ReadOnlyDictionary<TokenId, TokenRelationship> TokenRelationships;

		/**
		 * The key for the account, which must sign in order to transfer out, or to modify the account in any way other than
		 * extending its expiration date.
		 */
		public readonly Key Key;
		/**
		 * The total proxy staked to this account.
		 */
		public readonly Hbar ProxyReceived;
		/**
		 * The current balance of account.
		 */
		public readonly Hbar Balance;
		/**
		 * The threshold amount for which an account record is created (and this account charged for them) for any
		 * send/withdraw transaction.
		 */
		public readonly Hbar SendRecordThreshold;
		/**
		 * The threshold amount for which an account record is created (and this account charged for them) for any
		 * transaction above this amount.
		 */
		public readonly Hbar ReceiveRecordThreshold;
		/**
		 * The duration for expiration time will extend every this many seconds. If there are insufficient funds, then it
		 * extends as long as possible. If it is empty when it expires, then it is deleted.
		 */
		public readonly Duration AutoRenewPeriod;
		/**
		 * The time at which this account is set to expire.
		 */
		public readonly DateTimeOffset ExpirationTime;
		/**
		 * All the livehashes attached to the account (each of which is a hash along with the keys that authorized it and
		 * can delete it)
		 */
		
		/**
		 * The number of NFTs owned by this account
		 */
		public readonly long OwnedNfts;
		/**
		 * The ethereum transaction nonce associated with this account.
		 */
		public readonly long EthereumNonce;
		/**
		 * The maximum number of tokens that an Account can be implicitly associated with.
		 */
		public readonly int MaxAutomaticTokenAssociations;

		/**
		 * List of Hbar allowances
		 */
		[Obsolete]
		public readonly List<HbarAllowance> HbarAllowances;
		/**
		 * List of token allowances
		 */
		[Obsolete]
		public readonly List<TokenAllowance> TokenAllowances;
		/**
		 * List of NFT allowances
		 */
		[Obsolete]
		public readonly List<TokenNftAllowance> TokenNftAllowances;

		/**
		 * Constructor.
		 *
		 * @param accountId                     the account id
		 * @param contractAccountId             the contracts account id
		 * @param isDeleted                     is it deleted
		 * @param proxyAccountId                the proxy account's id
		 * @param proxyReceived                 amount of proxy received
		 * @param key                           signing key
		 * @param balance                       account balance
		 * @param sendRecordThreshold           @depreciated no replacement
		 * @param receiveRecordThreshold        @depreciated no replacement
		 * @param receiverSignatureRequired     is the receiver's signature required
		 * @param expirationTime                the expiration time
		 * @param autoRenewPeriod               the auto renew period
		 * @param liveHashes                    the live hashes
		 * @param tokenRelationships            list of token id and token relationship records
		 * @param accountMemo                   the account memo
		 * @param ownedNfts                     number of nft's
		 * @param maxAutomaticTokenAssociations max number of token associations
		 * @param aliasKey                      public alias key
		 * @param ledgerId                      the ledger id
		 */
		private AccountInfo(
			AccountId accountId,
			string contractAccountId,
			bool isDeleted,
			AccountId? proxyAccountId,
			long proxyReceived,
			Key key,
			long balance,
			long sendRecordThreshold,
			long receiveRecordThreshold,
			bool receiverSignatureRequired,
			DateTimeOffset expirationTime,
			Duration autoRenewPeriod,
			List<LiveHash> liveHashes,
			Dictionary<TokenId, TokenRelationship> tokenRelationships,
			string accountMemo,
			long ownedNfts,
			int maxAutomaticTokenAssociations,
			PublicKey? aliasKey,
			LedgerId ledgerId,
			long ethereumNonce,
			StakingInfo? stakingInfo)
		{
			AccountId = accountId;
			ContractAccountId = contractAccountId;
			IsDeleted = isDeleted;
			ProxyAccountId = proxyAccountId;
			Key = key;
			ProxyReceived = Hbar.FromTinybars(proxyReceived);
			Balance = Hbar.FromTinybars(balance);
			SendRecordThreshold = Hbar.FromTinybars(sendRecordThreshold);
			ReceiveRecordThreshold = Hbar.FromTinybars(receiveRecordThreshold);
			IsReceiverSignatureRequired = receiverSignatureRequired;
			ExpirationTime = expirationTime;
			AutoRenewPeriod = autoRenewPeriod;
			LiveHashes = liveHashes;
			TokenRelationships = new ReadOnlyDictionary<TokenId, TokenRelationship>(tokenRelationships);
			AccountMemo = accountMemo;
			OwnedNfts = ownedNfts;
			MaxAutomaticTokenAssociations = maxAutomaticTokenAssociations;
			AliasKey = aliasKey;
			LedgerId = ledgerId;
			EthereumNonce = ethereumNonce;
			StakingInfo = stakingInfo;
			HbarAllowances = [];
			TokenAllowances = [];
			TokenNftAllowances = [];
		}

		/**
		 * Retrieve the account info from a protobuf byte array.
		 *
		 * @param bytes a byte array representing the protobuf
		 * @return the account info object
		 * @ when there is an issue with the protobuf
		 */
		public static AccountInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(CryptoGetInfoResponse.Types.AccountInfo.Parser.ParseFrom(bytes));
		}
		/**
		 * Retrieve the account info from a protobuf.
		 *
		 * @param accountInfo the account info protobuf
		 * @return the account info object
		 */
		public static AccountInfo FromProtobuf(CryptoGetInfoResponse.Types.AccountInfo accountInfo)
		{
			var accountId = AccountId.FromProtobuf(accountInfo.getAccountID());

			var proxyAccountId = accountInfo.getProxyAccountID().getAccountNum() > 0
					? AccountId.FromProtobuf(accountInfo.getProxyAccountID())
					: null;

			var liveHashes = Arrays.stream(accountInfo.getLiveHashesList().toArray())
					.map((liveHash)->LiveHash.FromProtobuf((Proto.LiveHash)liveHash))
					.collect(toList());

			Dictionary<TokenId, TokenRelationship> relationships = new HashMap<>();

			for (Proto.TokenRelationship relationship : accountInfo.getTokenRelationshipsList())
			{
				TokenId tokenId = TokenId.FromProtobuf(relationship.getTokenId());
				relationships.put(tokenId, TokenRelationship.FromProtobuf(relationship));
			}

			@Nullable var aliasKey = PublicKey.FromAliasBytes(accountInfo.getAlias());

			return new AccountInfo(
					accountId,
					accountInfo.getContractAccountID(),
					accountInfo.getDeleted(),
					proxyAccountId,
					accountInfo.getProxyReceived(),
					Key.FromProtobufKey(accountInfo.getKey()),
					accountInfo.getBalance(),
					accountInfo.getGenerateSendRecordThreshold(),
					accountInfo.getGenerateReceiveRecordThreshold(),
					accountInfo.getReceiverSigRequired(),
					DateTimeOffsetConverter.FromProtobuf(accountInfo.getExpirationTime()),
					DurationConverter.FromProtobuf(accountInfo.getAutoRenewPeriod()),
					liveHashes,
					relationships,
					accountInfo.getMemo(),
					accountInfo.getOwnedNfts(),
					accountInfo.getMaxAutomaticTokenAssociations(),
					aliasKey,
					LedgerId.FromByteString(accountInfo.getLedgerId()),
					accountInfo.getEthereumNonce(),
					accountInfo.hasStakingInfo() ? StakingInfo.FromProtobuf(accountInfo.getStakingInfo()) : null);
		}

		/**
		 * Convert an account info object into a protobuf.
		 *
		 * @return the protobuf object
		 */
		public CryptoGetInfoResponse.Types.AccountInfo ToProtobuf()
		{
			var hashes = Arrays.stream(liveHashes.toArray())
					.map((liveHash)-> ((LiveHash)liveHash).ToProtobuf())
					.collect(toList());

			var accountInfoBuilder = CryptoGetInfoResponse.AccountInfo.newBuilder()
					.setAccountID(accountId.ToProtobuf())
					.setDeleted(isDeleted)
					.setProxyReceived(proxyReceived.toTinybars())
					.setKey(key.ToProtobufKey())
					.setBalance(balance.toTinybars())
					.setGenerateSendRecordThreshold(sendRecordThreshold.toTinybars())
					.setGenerateReceiveRecordThreshold(receiveRecordThreshold.toTinybars())
					.setReceiverSigRequired(isReceiverSignatureRequired)
					.setExpirationTime(DateTimeOffsetConverter.ToProtobuf(expirationTime))
					.setAutoRenewPeriod(DurationConverter.ToProtobuf(autoRenewPeriod))
					.AddAllLiveHashes(hashes)
					.setMemo(accountMemo)
					.setOwnedNfts(ownedNfts)
					.setMaxAutomaticTokenAssociations(maxAutomaticTokenAssociations)
					.setLedgerId(ledgerId.toByteString())
					.setEthereumNonce(ethereumNonce);

			if (contractAccountId != null)
			{
				accountInfoBuilder.setContractAccountID(contractAccountId);
			}

			if (proxyAccountId != null)
			{
				accountInfoBuilder.setProxyAccountID(proxyAccountId.ToProtobuf());
			}

			if (aliasKey != null)
			{
				accountInfoBuilder.setAlias(aliasKey.ToProtobufKey().toByteString());
			}

			if (stakingInfo != null)
			{
				accountInfoBuilder.setStakingInfo(stakingInfo.ToProtobuf());
			}

			return accountInfoBuilder.build();
		}


		/**
		 * Extract a byte array representation.
		 *
		 * @return a byte array representation
		 */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
	}

}