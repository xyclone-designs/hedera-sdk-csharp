// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Utils;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// Current information on the smart contract instance, including its balance.
    /// </summary>
    public sealed class ContractInfo
    {
        /// <summary>
        /// ID of the contract instance, in the format used in transactions.
        /// </summary>
        public readonly ContractId contractId;
        /// <summary>
        /// ID of the cryptocurrency account owned by the contract instance,
        /// in the format used in transactions.
        /// </summary>
        public readonly AccountId accountId;
        /// <summary>
        /// ID of both the contract instance and the cryptocurrency account owned by the contract
        /// instance, in the format used by Solidity.
        /// </summary>
        public readonly string contractAccountId;
        /// <summary>
        /// The state of the instance and its fields can be modified arbitrarily if this key signs a
        /// transaction to modify it. If this is null, then such modifications are not possible,
        /// and there is no administrator that can override the normal operation of this smart
        /// contract instance. Note that if it is created with no admin keys, then there is no
        /// administrator to authorize changing the admin keys, so there can never be any admin keys
        /// for that instance.
        /// </summary>
        public readonly Key adminKey;
        /// <summary>
        /// The current time at which this contract instance (and its account) is set to expire.
        /// </summary>
        public readonly Timestamp expirationTime;
        /// <summary>
        /// The expiration time will extend every this many seconds. If there are insufficient funds,
        /// then it extends as long as possible. If the account is empty when it expires,
        /// then it is deleted.
        /// </summary>
        public readonly Duration autoRenewPeriod;
        /// <summary>
        /// ID of the an account to charge for auto-renewal of this contract. If not set, or set to
        /// an account with zero hbar balance, the contract's own hbar balance will be used to cover
        /// auto-renewal fees.
        /// </summary>
        public readonly AccountId autoRenewAccountId;
        /// <summary>
        /// Number of bytes of storage being used by this instance (which affects the cost to
        /// extend the expiration time).
        /// </summary>
        public readonly long storage;
        /// <summary>
        /// The memo associated with the contract (max 100 bytes).
        /// </summary>
        public readonly string contractMemo;
        /// <summary>
        /// The current balance of the contract.
        /// </summary>
        public readonly Hbar balance;
        /// <summary>
        /// Whether the contract has been deleted
        /// </summary>
        public readonly bool isDeleted;
        /// <summary>
        /// The tokens associated to the contract
        /// </summary>
        public readonly Dictionary<TokenId, TokenRelationship> tokenRelationships;
        /// <summary>
        /// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
        /// </summary>
        public readonly LedgerId ledgerId;
        /// <summary>
        /// Staking metadata for this account.
        /// </summary>
        public readonly StakingInfo stakingInfo;
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="contractId">the contract id</param>
        /// <param name="accountId">the account id</param>
        /// <param name="contractAccountId">the account id of the owner</param>
        /// <param name="adminKey">the key that can modify the contract</param>
        /// <param name="expirationTime">the time that contract will expire</param>
        /// <param name="autoRenewPeriod">seconds before contract is renewed (funds must be available)</param>
        /// <param name="autoRenewAccountId">account ID which will be charged for renewing this account</param>
        /// <param name="storage">number of bytes used by this contract</param>
        /// <param name="contractMemo">the memo field 100 bytes</param>
        /// <param name="balance">current balance</param>
        /// <param name="isDeleted">does it still exist</param>
        /// <param name="tokenRelationships">list of compound token id and relationship records</param>
        /// <param name="ledgerId">the ledger id</param>
        private ContractInfo(ContractId contractId, AccountId accountId, string contractAccountId, Key adminKey, Timestamp expirationTime, Duration autoRenewPeriod, AccountId autoRenewAccountId, long storage, string contractMemo, Hbar balance, bool isDeleted, Dictionary<TokenId, TokenRelationship> tokenRelationships, LedgerId ledgerId, StakingInfo stakingInfo)
        {
            this.contractId = contractId;
            this.accountId = accountId;
            this.contractAccountId = contractAccountId;
            this.adminKey = adminKey;
            this.expirationTime = expirationTime;
            this.autoRenewPeriod = autoRenewPeriod;
            this.autoRenewAccountId = autoRenewAccountId;
            this.storage = storage;
            this.contractMemo = contractMemo;
            this.balance = balance;
            this.isDeleted = isDeleted;
            this.tokenRelationships = tokenRelationships;
            this.ledgerId = ledgerId;
            this.stakingInfo = stakingInfo;
        }

        /// <summary>
        /// Extract the contract from the protobuf.
        /// </summary>
        /// <param name="contractInfo">the protobuf</param>
        /// <returns>                         the contract object</returns>
        public static ContractInfo FromProtobuf(Proto.ContractGetInfoResponse.Types.ContractInfo contractInfo)
        {
            return new ContractInfo(
                ContractId.FromProtobuf(contractInfo.ContractID), 
                AccountId.FromProtobuf(contractInfo.AccountID), 
                contractInfo.ContractAccountID,
				Key.FromProtobufKey(contractInfo.AdminKey), 
                TimestampConverter.FromProtobuf(contractInfo.ExpirationTime), 
                DurationConverter.FromProtobuf(contractInfo.AutoRenewPeriod), 
                AccountId.FromProtobuf(contractInfo.AutoRenewAccountId), 
                contractInfo.Storage, 
                contractInfo.Memo, 
                Hbar.FromTinybars((long)contractInfo.Balance), 
                contractInfo.Deleted,
				contractInfo.TokenRelationships.ToDictionary(
				    _ => TokenId.FromProtobuf(_.TokenId),
				    _ => TokenRelationship.FromProtobuf(_)), 
                LedgerId.FromByteString(contractInfo.LedgerId), 
                StakingInfo.FromProtobuf(contractInfo.StakingInfo));
        }

        /// <summary>
        /// Extract the contract from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the extracted contract</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static ContractInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.ContractGetInfoResponse.Types.ContractInfo.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Build the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        public Proto.ContractGetInfoResponse.Types.ContractInfo ToProtobuf()
        {
            Proto.ContractGetInfoResponse.Types.ContractInfo proto = new()
            {
				ContractID = contractId.ToProtobuf(),
				AccountID = accountId.ToProtobuf(),
				ContractAccountID = contractAccountId,
				ExpirationTime = TimestampConverter.ToProtobuf(expirationTime),
				AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(autoRenewPeriod),
				Storage = storage,
				Memo = contractMemo,
				Balance = (ulong)balance.ToTinybars(),
				LedgerId = ledgerId.ToByteString(),
			};
            
            if (adminKey != null)
                proto.AdminKey = adminKey.ToProtobufKey();

            if (stakingInfo != null)
                proto.StakingInfo = stakingInfo.ToProtobuf();

            if (autoRenewAccountId != null)
                proto.AutoRenewAccountId = autoRenewAccountId.ToProtobuf();

            return proto;
        }

        /// <summary>
        /// Create a byte array representation.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}