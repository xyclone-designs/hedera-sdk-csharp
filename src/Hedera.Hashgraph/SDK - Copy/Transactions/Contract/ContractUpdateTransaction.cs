// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Time;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Transactions.Contract
{
    /// <summary>
    /// Modify a smart contract instance to have the given parameter values.
    /// <p>
    /// Any null field is ignored (left unchanged).
    /// <p>
    /// If only the contractInstanceExpirationTime is being modified, then no signature is
    /// needed on this transaction other than for the account paying for the transaction itself.
    /// <p>
    /// But if any of the other fields are being modified, then it must be signed by the adminKey.
    /// <p>
    /// The use of adminKey is not currently supported in this API, but in the future will
    /// be implemented to allow these fields to be modified, and also to make modifications
    /// to the state of the instance.
    /// <p>
    /// If the contract is created with no admin key, then none of the fields can be
    /// changed that need an admin signature, and therefore no admin key can ever be added.
    /// So if there is no admin key, then things like the bytecode are immutable.
    /// But if there is an admin key, then they can be changed. For example, the
    /// admin key might be a threshold key, which requires 3 of 5 binding arbitration judges to
    /// agree before the bytecode can be changed. This can be used to add flexibility to the management
    /// of smart contract behavior. But this is optional. If the smart contract is created
    /// without an admin key, then such a key can never be added, and its bytecode will be immutable.
    /// </summary>
    public sealed class ContractUpdateTransaction : Transaction<ContractUpdateTransaction>
    {
        private ContractId contractId = null;
        private AccountId proxyAccountId = null;
        private FileId bytecodeFileId = null;
        private Timestamp expirationTime = null;
        private Duration expirationTimeDuration = null;
        private Key adminKey = null;
        private int maxAutomaticTokenAssociations = null;
        private Duration autoRenewPeriod = null;
        private string contractMemo = null;
        private AccountId stakedAccountId = null;
        private long stakedNodeId = null;
        private bool declineStakingReward = null;
        private AccountId autoRenewAccountId = null;
        private IList<long> hookIdsToDelete = new ();
        private IList<HookCreationDetails> hookCreationDetails = new ();
        /// <summary>
        /// Contract.
        /// </summary>
        public ContractUpdateTransaction()
        {
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) record</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        ContractUpdateTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        ContractUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the contract id.
        /// </summary>
        /// <returns>                         the contract id</returns>
        public ContractId GetContractId()
        {
            return contractId;
        }

        /// <summary>
        /// Sets the Contract ID instance to update.
        /// </summary>
        /// <param name="contractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetContractId(ContractId contractId)
        {
            Objects.RequireNonNull(contractId);
            RequireNotFrozen();
            contractId = contractId;
            return this;
        }

        /// <summary>
        /// Extract the contract expiration time.
        /// </summary>
        /// <returns>                         the contract expiration time</returns>
        public Timestamp GetExpirationTime()
        {
            return expirationTime;
        }

        /// <summary>
        /// If set, modify the time at which this contract will expire.<br/>
        /// An expired contract requires a rent payment to "renew" the contract.
        /// A transaction to update this field is how that rent payment is made.
        /// <p>
        /// This value MUST NOT be less than the current `expirationTime`
        /// of the contract. If this value is earlier than the current
        /// value, the transaction SHALL fail with response
        /// code `EXPIRATION_REDUCTION_NOT_ALLOWED`.
        /// </summary>
        /// <param name="expirationTime">The Timestamp to be set for expiration time</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetExpirationTime(Timestamp expirationTime)
        {
            Objects.RequireNonNull(expirationTime);
            RequireNotFrozen();
            expirationTime = expirationTime;
            expirationTimeDuration = null;
            return this;
        }

        public ContractUpdateTransaction SetExpirationTime(Duration expirationTime)
        {
            Objects.RequireNonNull(expirationTime);
            RequireNotFrozen();
            expirationTime = null;
            expirationTimeDuration = expirationTime;
            return this;
        }

        /// <summary>
        /// Extract the administrator key.
        /// </summary>
        /// <returns>                         the administrator key</returns>
        public Key GetAdminKey()
        {
            return adminKey;
        }

        /// <summary>
        /// If set, modify the key that authorizes updates to the contract.
        /// <p>
        /// If this field is set to a valid Key, this key and the previously set key
        /// MUST both sign this transaction.<br/>
        /// If this value is an empty `KeyList`, the prior key MUST sign this
        /// transaction, and the smart contract SHALL be immutable after this
        /// transaction completes, except for expiration and renewal.<br/>
        /// If this value is not an empty `KeyList`, but does not contain any
        /// cryptographic keys, or is otherwise malformed, this transaction SHALL
        /// fail with response code `INVALID_ADMIN_KEY`.
        /// </summary>
        /// <param name="adminKey">The Key to be set</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetAdminKey(Key adminKey)
        {
            Objects.RequireNonNull(adminKey);
            RequireNotFrozen();
            adminKey = adminKey;
            return this;
        }

        /// <summary>
        /// Extract the proxy account id.
        /// </summary>
        /// <returns>                         the proxy account id</returns>
        public AccountId GetProxyAccountId()
        {
            return proxyAccountId;
        }

        /// <summary>
        /// Sets the ID of the account to which this account is proxy staked.
        /// <p>
        /// If proxyAccountID is null, or is an invalid account, or is an account
        /// that isn't a node, then this account is automatically proxy staked to a
        /// node chosen by the network, but without earning payments.
        /// <p>
        /// If the proxyAccountID account refuses to accept proxy staking, or if it is
        /// not currently running a node, then it will behave as if proxyAccountID was null.
        /// </summary>
        /// <param name="proxyAccountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetProxyAccountId(AccountId proxyAccountId)
        {
            Objects.RequireNonNull(proxyAccountId);
            RequireNotFrozen();
            proxyAccountId = proxyAccountId;
            return this;
        }

        /// <summary>
        /// Extract the auto renew period.
        /// </summary>
        /// <returns>                         the auto renew period</returns>
        public int GetMaxAutomaticTokenAssociations()
        {
            return maxAutomaticTokenAssociations;
        }

        /// <summary>
        /// If set, modify the maximum number of tokens that can be auto-associated with the
        /// contract.
        /// <p>
        /// If this is set and less than or equal to `used_auto_associations`, or 0, then this contract
        /// MUST manually associate with a token before transacting in that token.<br/>
        /// This value MAY also be `-1` to indicate no limit.<br/>
        /// This value MUST NOT be less than `-1`.
        /// </summary>
        /// <param name="maxAutomaticTokenAssociations">The maximum automatic token associations</param>
        /// <returns> {@code this}</returns>
        public ContractUpdateTransaction SetMaxAutomaticTokenAssociations(int maxAutomaticTokenAssociations)
        {
            RequireNotFrozen();
            maxAutomaticTokenAssociations = maxAutomaticTokenAssociations;
            return this;
        }

        /// <summary>
        /// Extract the duration for the auto renew period.
        /// </summary>
        /// <returns>                         the duration for auto-renew</returns>
        public Duration GetAutoRenewPeriod()
        {
            return autoRenewPeriod;
        }

        /// <summary>
        /// If set, modify the duration added to expiration time by each
        /// auto-renewal to this value.
        /// </summary>
        /// <param name="autoRenewPeriod">The Duration to be set for auto-renewal</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetAutoRenewPeriod(Duration autoRenewPeriod)
        {
            Objects.RequireNonNull(autoRenewPeriod);
            RequireNotFrozen();
            autoRenewPeriod = autoRenewPeriod;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>the bytecodeFileId</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public FileId GetBytecodeFileId()
        {
            return bytecodeFileId;
        }

        /// <summary>
        /// </summary>
        /// <param name="bytecodeFileId">The FileId to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>
        /// @deprecatedwith no replacement
        /// 
        /// Sets the file ID of file containing the smart contract byte code.
        /// <p>
        /// A copy will be made and held by the contract instance, and have the same expiration
        /// time as the instance.
        /// </remarks>
        public ContractUpdateTransaction SetBytecodeFileId(FileId bytecodeFileId)
        {
            Objects.RequireNonNull(bytecodeFileId);
            RequireNotFrozen();
            bytecodeFileId = bytecodeFileId;
            return this;
        }

        /// <summary>
        /// Extract the contents of the memo.
        /// </summary>
        /// <returns>                         the contents of the memo</returns>
        public string GetContractMemo()
        {
            return contractMemo;
        }

        /// <summary>
        /// Sets the memo associated with the contract (max: 100 bytes).
        /// </summary>
        /// <param name="memo">The memo to be set</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetContractMemo(string memo)
        {
            Objects.RequireNonNull(memo);
            RequireNotFrozen();
            contractMemo = memo;
            return this;
        }

        /// <summary>
        /// Remove the memo contents.
        /// </summary>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction ClearMemo()
        {
            RequireNotFrozen();
            contractMemo = "";
            return this;
        }

        /// <summary>
        /// ID of the account to which this contract will stake
        /// </summary>
        /// <returns>ID of the account to which this contract will stake.</returns>
        public AccountId GetStakedAccountId()
        {
            return stakedAccountId;
        }

        /// <summary>
        /// An account identifier.<br/>
        /// A staked account acts as a proxy, and this contract effectively
        /// nominates the same node as the identified account.
        /// <p>
        /// If set, modify this smart contract such that it SHALL stake its HBAR
        /// to the same node as the identified account.<br/>
        /// If this field is set to a default AccountID value (`0.0.0`), any
        /// pre-existing `staked_account_id` value SHALL be removed on success.
        /// </summary>
        /// <param name="stakedAccountId">ID of the account to which this contract will stake.</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetStakedAccountId(AccountId stakedAccountId)
        {
            RequireNotFrozen();
            stakedAccountId = stakedAccountId;
            stakedNodeId = null;
            return this;
        }

        /// <summary>
        /// Clear the staked account ID
        /// </summary>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction ClearStakedAccountId()
        {
            RequireNotFrozen();
            stakedAccountId = new AccountId(0, 0, 0);
            stakedNodeId = null;
            return this;
        }

        /// <summary>
        /// The node to which this contract will stake
        /// </summary>
        /// <returns>ID of the node this contract will be staked to.</returns>
        public long GetStakedNodeId()
        {
            return stakedNodeId;
        }

        /// <summary>
        /// A node identifier.<br/>
        /// A staked node identifier indicates the consensus node that this
        /// account nominates for staking.
        /// <p>
        /// If set, modify this smart contract such that it SHALL stake its HBAR
        /// to this node.
        /// If set to the value `-1` any pre-existing `staked_node_id` value
        /// SHALL be removed on success.
        /// <p>
        /// <blockquote>Note: node IDs do fluctuate as node operators change.
        /// Most contracts are immutable, and a contract staking to an invalid
        /// node ID SHALL NOT participate in staking. Immutable contracts may
        /// find it more reliable to use a proxy account for staking (via
        /// `staked_account_id`) to enable updating the _effective_ staking node
        /// ID when necessary through updating the proxy account.</blockquote>
        /// </summary>
        /// <param name="stakedNodeId">ID of the node this contract will be staked to.</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetStakedNodeId(long stakedNodeId)
        {
            RequireNotFrozen();
            stakedNodeId = stakedNodeId;
            stakedAccountId = null;
            return this;
        }

        /// <summary>
        /// clear the staked node account ID
        /// </summary>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction ClearStakedNodeId()
        {
            RequireNotFrozen();
            stakedNodeId = -1;
            stakedAccountId = null;
            return this;
        }

        /// <summary>
        /// If true, the contract declines receiving a staking reward. The default value is false.
        /// </summary>
        /// <returns>If true, the contract declines receiving a staking reward. The default value is false.</returns>
        public bool GetDeclineStakingReward()
        {
            return declineStakingReward;
        }

        /// <summary>
        /// A flag indicating if staking rewards are declined.<br/>
        /// If set, modify the flag indicating if this contract declines to accept
        /// rewards for staking its HBAR to secure the network.
        /// <p>
        /// If set to true, this smart contract SHALL NOT receive any reward for
        /// staking its HBAR balance to help secure the network, regardless of
        /// staking configuration, but MAY stake HBAR to support the network
        /// without reward.
        /// </summary>
        /// <param name="declineStakingReward">- If true, the contract declines receiving a staking reward. The default value is false.</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetDeclineStakingReward(bool declineStakingReward)
        {
            RequireNotFrozen();
            declineStakingReward = declineStakingReward;
            return this;
        }

        /// <summary>
        /// Clear decline staking reward
        /// </summary>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction ClearDeclineStakingReward()
        {
            RequireNotFrozen();
            declineStakingReward = null;
            return this;
        }

        /// <summary>
        /// Get the auto renew accountId.
        /// </summary>
        /// <returns>                         the auto renew accountId</returns>
        public AccountId GetAutoRenewAccountId()
        {
            return autoRenewAccountId;
        }

        /// <summary>
        /// If set, modify the account, in the same shard and realm as this smart
        /// contract, that has agreed to allow the network to use its balance, when
        /// needed, to automatically extend this contract's expiration time.
        /// <p>
        /// If this field is set to a non-default value, that Account MUST sign this
        /// transaction.<br/>
        /// If this field is set to a default AccountID value (`0.0.0`), any
        /// pre-existing `auto_renew_account_id` value SHALL be removed on success.
        /// </summary>
        /// <param name="autoRenewAccountId">The AccountId to be set for auto-renewal</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetAutoRenewAccountId(AccountId autoRenewAccountId)
        {
            Objects.RequireNonNull(autoRenewAccountId);
            RequireNotFrozen();
            autoRenewAccountId = autoRenewAccountId;
            return this;
        }

        /// <summary>
        /// Clears the auto-renew account ID
        /// </summary>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction ClearAutoRenewAccountId()
        {
            autoRenewAccountId = new AccountId(0, 0, 0);
            return this;
        }

        /// <summary>
        /// Add a hook to be created for the contract.
        /// </summary>
        /// <param name="hookDetails">the hook creation details to add</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction AddHookToCreate(HookCreationDetails hookDetails)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(hookDetails, "hookDetails cannot be null");
            hookCreationDetails.Add(hookDetails);
            return this;
        }

        /// <summary>
        /// Set hooks to be created with the contract.
        /// </summary>
        /// <param name="hookDetails">list of hook creation details</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetHooksToCreate(IList<HookCreationDetails> hookDetails)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(hookDetails, "hookDetails cannot be null");
            hookCreationDetails = new List(hookDetails);
            return this;
        }

        /// <summary>
        /// Mark a hook for deletion from the contract.
        /// </summary>
        /// <param name="hookId">the hook id to delete</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction AddHookToDelete(long hookId)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(hookId, "hookId cannot be null");
            hookIdsToDelete.Add(hookId);
            return this;
        }

        /// <summary>
        /// Mark hooks for deletion from the contract.
        /// </summary>
        /// <param name="hookIds">list of hook ids to delete</param>
        /// <returns>{@code this}</returns>
        public ContractUpdateTransaction SetHooksToDelete(IList<long> hookIds)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(hookIds, "hookIds cannot be null");
            hookIdsToDelete = new List(hookIds);
            return this;
        }

        /// <summary>
        /// Get the list of hooks to be created.
        /// </summary>
        /// <returns>a copy of the hook creation details list</returns>
        public IList<HookCreationDetails> GetHooksToCreate()
        {
            return new List(hookCreationDetails);
        }

        /// <summary>
        /// Get the list of hook IDs to be deleted.
        /// </summary>
        /// <returns>a copy of the hook IDs list</returns>
        public IList<long> GetHooksToDelete()
        {
            return new List(hookIdsToDelete);
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetContractUpdateInstance();
            if (body.HasContractID())
            {
                contractId = ContractId.FromProtobuf(body.GetContractID());
            }

            if (body.HasProxyAccountID())
            {
                proxyAccountId = AccountId.FromProtobuf(body.GetProxyAccountID());
            }

            if (body.HasExpirationTime())
            {
                expirationTime = TimestampConverter.FromProtobuf(body.GetExpirationTime());
            }

            if (body.HasAdminKey())
            {
                adminKey = Key.FromProtobufKey(body.GetAdminKey());
            }

            if (body.HasMaxAutomaticTokenAssociations())
            {
                maxAutomaticTokenAssociations = body.GetMaxAutomaticTokenAssociations().GetValue();
            }

            if (body.HasAutoRenewPeriod())
            {
                autoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.GetAutoRenewPeriod());
            }

            if (body.HasMemoWrapper())
            {
                contractMemo = body.GetMemoWrapper().GetValue();
            }

            if (body.HasDeclineReward())
            {
                declineStakingReward = body.GetDeclineReward().GetValue();
            }

            if (body.HasStakedAccountId())
            {
                stakedAccountId = AccountId.FromProtobuf(body.GetStakedAccountId());
            }

            if (body.HasStakedNodeId())
            {
                stakedNodeId = body.GetStakedNodeId();
            }

            if (body.HasAutoRenewAccountId())
            {
                autoRenewAccountId = AccountId.FromProtobuf(body.GetAutoRenewAccountId());
            }

            hookCreationDetails.Clear();
            foreach (var protoHookDetails in body.GetHookCreationDetailsList())
            {
                hookCreationDetails.Add(HookCreationDetails.FromProtobuf(protoHookDetails));
            }

            hookIdsToDelete.Clear();
            hookIdsToDelete.AddAll(body.GetHookIdsToDeleteList());
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.ContractUpdateTransactionBody builder }</returns>
        ContractUpdateTransactionBody.Builder Build()
        {
            var builder = ContractUpdateTransactionBody.NewBuilder();
            if (contractId != null)
            {
                builder.SetContractID(contractId.ToProtobuf());
            }

            if (proxyAccountId != null)
            {
                builder.SetProxyAccountID(proxyAccountId.ToProtobuf());
            }

            if (expirationTime != null)
            {
                builder.SetExpirationTime(TimestampConverter.ToProtobuf(expirationTime));
            }

            if (expirationTimeDuration != null)
            {
                builder.SetExpirationTime(TimestampConverter.ToProtobuf(expirationTimeDuration));
            }

            if (adminKey != null)
            {
                builder.SetAdminKey(adminKey.ToProtobufKey());
            }

            if (maxAutomaticTokenAssociations != null)
            {
                builder.SetMaxAutomaticTokenAssociations(Int32Value.Of(maxAutomaticTokenAssociations));
            }

            if (autoRenewPeriod != null)
            {
                builder.SetAutoRenewPeriod(Utils.DurationConverter.ToProtobuf(autoRenewPeriod));
            }

            if (contractMemo != null)
            {
                builder.SetMemoWrapper(StringValue.Of(contractMemo));
            }

            if (stakedAccountId != null)
            {
                builder.SetStakedAccountId(stakedAccountId.ToProtobuf());
            }

            if (stakedNodeId != null)
            {
                builder.SetStakedNodeId(stakedNodeId);
            }

            if (declineStakingReward != null)
            {
                builder.SetDeclineReward(BoolValue.NewBuilder().SetValue(declineStakingReward).Build());
            }

            if (autoRenewAccountId != null)
            {
                if (autoRenewAccountId.ToString().Equals("0.0.0"))
                {
                    builder.SetAutoRenewAccountId(AccountID.GetDefaultInstance());
                }
                else
                {
                    builder.SetAutoRenewAccountId(autoRenewAccountId.ToProtobuf());
                }
            }

            foreach (HookCreationDetails hookDetails in hookCreationDetails)
            {
                builder.AddHookCreationDetails(hookDetails.ToProtobuf());
            }

            if (!hookIdsToDelete.IsEmpty())
            {
                builder.AddAllHookIdsToDelete(hookIdsToDelete);
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (contractId != null)
            {
                contractId.ValidateChecksum(client);
            }

            if (proxyAccountId != null)
            {
                proxyAccountId.ValidateChecksum(client);
            }

            if (stakedAccountId != null)
            {
                stakedAccountId.ValidateChecksum(client);
            }

            if (autoRenewAccountId != null)
            {
                autoRenewAccountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetUpdateContractMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetContractUpdateInstance(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetContractUpdateInstance(Build());
        }
    }
}