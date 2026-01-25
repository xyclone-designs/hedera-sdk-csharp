// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Start a new smart contract instance. After the instance is created, the ContractID for it is in the receipt.
    /// <p>
    /// The instance will exist for autoRenewPeriod seconds. When that is reached, it will renew itself for another
    /// autoRenewPeriod seconds by charging its associated cryptocurrency account (which it creates here). If it has
    /// insufficient cryptocurrency to extend that long, it will extend as long as it can. If its balance is zero, the
    /// instance will be deleted.
    /// <p>
    /// A smart contract instance normally enforces rules, so "the code is law". For example, an ERC-20 contract prevents a
    /// transfer from being undone without a signature by the recipient of the transfer. This is always enforced if the
    /// contract instance was created with the adminKeys being null. But for some uses, it might be desirable to create
    /// something like an ERC-20 contract that has a specific group of trusted individuals who can act as a "supreme court"
    /// with the ability to override the normal operation, when a sufficient number of them agree to do so. If adminKeys is
    /// not null, then they can sign a transaction that can change the state of the smart contract in arbitrary ways, such as
    /// to reverse a transaction that violates some standard of behavior that is not covered by the code itself. The admin
    /// keys can also be used to change the autoRenewPeriod, and change the adminKeys field itself. The API currently does
    /// not implement this ability. But it does allow the adminKeys field to be set and queried, and will in the future
    /// implement such admin abilities for any instance that has a non-null adminKeys.
    /// <p>
    /// If this constructor stores information, it is charged gas to store it. There is a fee in hbars to maintain that
    /// storage until the expiration time, and that fee is added as part of the transaction fee.
    /// <p>
    /// An entity (account, file, or smart contract instance) must be created in a particular realm. If the realmID is left
    /// null, then a new realm will be created with the given admin key. If a new realm has a null adminKey, then anyone can
    /// create/modify/delete entities in that realm. But if an admin key is given, then any transaction to
    /// create/modify/delete an entity in that realm must be signed by that key, though anyone can still call functions on
    /// smart contract instances that exist in that realm. A realm ceases to exist when everything within it has expired and
    /// no longer exists.
    /// <p>
    /// The current API ignores shardID, realmID, and newRealmAdminKey, and creates everything in shard 0 and realm 0, with a
    /// null key. Future versions of the API will support multiple realms and multiple shards.
    /// <p>
    /// The optional memo field can contain a string whose length is up to 100 bytes. That is the size after Unicode NFD then
    /// UTF-8 conversion. This field can be used to describe the smart contract. It could also be used for other purposes.
    /// One recommended purpose is to hold a hexadecimal string that is the SHA-384 hash of a PDF file containing a
    /// human-readable legal contract. Then, if the admin keys are the public keys of human arbitrators, they can use that
    /// legal document to guide their decisions during a binding arbitration tribunal, convened to consider any changes to
    /// the smart contract in the future. The memo field can only be changed using the admin keys. If there are no admin
    /// keys, then it cannot be changed after the smart contract is created.
    /// </summary>
    // Re-use the WithExecute interface that was generated for Executable
    public class ContractCreateFlow
    {
        static readonly int FILE_CREATE_MAX_BYTES = 2048;
        private string bytecode = "";
        private int maxChunks = null;
        private Key adminKey = null;
        private long gas = 0;
        private Hbar initialBalance = Hbar.ZERO;
        private AccountId proxyAccountId = null;
        private int maxAutomaticTokenAssociations = 0;
        private Duration autoRenewPeriod = null;
        private AccountId autoRenewAccountId = null;
        private byte[] constructorParameters = new[]
        {
        };
        private string contractMemo = null;
        private IList<AccountId> nodeAccountIds = null;
        private string createBytecode = "";
        private string appendBytecode = "";
        private AccountId stakedAccountId = null;
        private long stakedNodeId = null;
        private bool declineStakingReward = false;
        private Client freezeWithClient = null;
        private PrivateKey signPrivateKey = null;
        private PublicKey signPublicKey = null;
        private Func<byte[], byte[]> transactionSigner = null;
        /// <summary>
        /// Constructor
        /// </summary>
        public ContractCreateFlow()
        {
        }

        /// <summary>
        /// Extract the hex-encoded bytecode of the contract.
        /// </summary>
        /// <returns>the hex-encoded bytecode of the contract.</returns>
        public virtual string GetBytecode()
        {
            return bytecode;
        }

        /// <summary>
        /// Sets the bytecode of the contract in hex.
        /// </summary>
        /// <param name="bytecode">the string to assign</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetBytecode(string bytecode)
        {
            Objects.RequireNonNull(bytecode);
            bytecode = bytecode;
            return this;
        }

        /// <summary>
        /// Sets the bytecode of the contract in raw bytes.
        /// </summary>
        /// <param name="bytecode">the byte array</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetBytecode(byte[] bytecode)
        {
            Objects.RequireNonNull(bytecode);
            bytecode = Hex.ToHexString(bytecode);
            return this;
        }

        /// <summary>
        /// Sets the bytecode of the contract in raw bytes.
        /// </summary>
        /// <param name="bytecode">the byte string</param>
        /// <returns>the contract in raw bytes</returns>
        public virtual ContractCreateFlow SetBytecode(ByteString bytecode)
        {
            Objects.RequireNonNull(bytecode);
            return SetBytecode(bytecode.ToByteArray());
        }

        /// <summary>
        /// Get the maximum number of chunks
        /// </summary>
        /// <returns>the maxChunks</returns>
        public virtual int GetMaxChunks()
        {
            return maxChunks;
        }

        /// <summary>
        /// Set the maximal number of chunks
        /// </summary>
        /// <param name="maxChunks">the maximum number of chunks</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetMaxChunks(int maxChunks)
        {
            maxChunks = maxChunks;
            return this;
        }

        /// <summary>
        /// Extract the admin key.
        /// </summary>
        /// <returns>the admin key</returns>
        public virtual Key GetAdminKey()
        {
            return adminKey;
        }

        /// <summary>
        /// Sets the state of the instance and its fields can be modified arbitrarily if this key signs a transaction to
        /// modify it. If this is null, then such modifications are not possible, and there is no administrator that can
        /// override the normal operation of this smart contract instance. Note that if it is created with no admin keys,
        /// then there is no administrator to authorize changing the admin keys, so there can never be any admin keys for
        /// that instance.
        /// </summary>
        /// <param name="adminKey">The Key to be set</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetAdminKey(Key adminKey)
        {
            Objects.RequireNonNull(adminKey);
            adminKey = adminKey;
            return this;
        }

        /// <summary>
        /// Extract the gas.
        /// </summary>
        /// <returns>the gas</returns>
        public virtual long GetGas()
        {
            return gas;
        }

        /// <summary>
        /// Sets the gas to run the constructor.
        /// </summary>
        /// <param name="gas">The long to be set as gas</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetGas(long gas)
        {
            gas = gas;
            return this;
        }

        /// <summary>
        /// Extract the initial balance in hbar.
        /// </summary>
        /// <returns>the initial balance in hbar</returns>
        public virtual Hbar GetInitialBalance()
        {
            return initialBalance;
        }

        /// <summary>
        /// Sets the initial number of hbars to put into the cryptocurrency account associated with and owned by the smart
        /// contract.
        /// </summary>
        /// <param name="initialBalance">The Hbar to be set as the initial balance</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetInitialBalance(Hbar initialBalance)
        {
            Objects.RequireNonNull(initialBalance);
            initialBalance = initialBalance;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>the proxy account id</returns>
        /// <remarks>
        /// @deprecatedwith no replacement
        /// <p>
        /// Extract the proxy account id.
        /// </remarks>
        public virtual AccountId GetProxyAccountId()
        {
            return proxyAccountId;
        }

        /// <summary>
        /// </summary>
        /// <param name="proxyAccountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>
        /// @deprecatedwith no replacement
        /// <p>
        /// Sets the ID of the account to which this account is proxy staked.
        /// <p>
        /// If proxyAccountID is null, or is an invalid account, or is an account that isn't a node, then this account is
        /// automatically proxy staked to a node chosen by the network, but without earning payments.
        /// <p>
        /// If the proxyAccountID account refuses to accept proxy staking , or if it is not currently running a node, then it
        /// will behave as if  proxyAccountID was null.
        /// </remarks>
        public virtual ContractCreateFlow SetProxyAccountId(AccountId proxyAccountId)
        {
            Objects.RequireNonNull(proxyAccountId);
            proxyAccountId = proxyAccountId;
            return this;
        }

        /// <summary>
        /// The maximum number of tokens that an Account can be implicitly associated with. Defaults to 0 and up to a maximum
        /// value of 1000.
        /// </summary>
        /// <returns>The maxAutomaticTokenAssociations.</returns>
        public virtual int GetMaxAutomaticTokenAssociations()
        {
            return maxAutomaticTokenAssociations;
        }

        /// <summary>
        /// The maximum number of tokens that an Account can be implicitly associated with. Defaults to 0 and up to a maximum
        /// value of 1000.
        /// </summary>
        /// <param name="maxAutomaticTokenAssociations">The maxAutomaticTokenAssociations to set</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetMaxAutomaticTokenAssociations(int maxAutomaticTokenAssociations)
        {
            maxAutomaticTokenAssociations = maxAutomaticTokenAssociations;
            return this;
        }

        /// <summary>
        /// Extract the auto renew period.
        /// </summary>
        /// <returns>the auto renew period</returns>
        public virtual Duration GetAutoRenewPeriod()
        {
            return autoRenewPeriod;
        }

        /// <summary>
        /// Sets the period that the instance will charge its account every this many seconds to renew.
        /// </summary>
        /// <param name="autoRenewPeriod">The Duration to be set for auto renewal</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetAutoRenewPeriod(Duration autoRenewPeriod)
        {
            Objects.RequireNonNull(autoRenewPeriod);
            autoRenewPeriod = autoRenewPeriod;
            return this;
        }

        /// <summary>
        /// Get the account ID which will be charged for renewing this account
        /// </summary>
        /// <returns>the auto-renewal account id</returns>
        public virtual AccountId GetAutoRenewAccountId()
        {
            return autoRenewAccountId;
        }

        /// <summary>
        /// Set the account ID which will be charged for renewing this account
        /// </summary>
        /// <param name="autoRenewAccountId">the autoRenewAccountId to set</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetAutoRenewAccountId(AccountId autoRenewAccountId)
        {
            Objects.RequireNonNull(autoRenewAccountId);
            autoRenewAccountId = autoRenewAccountId;
            return this;
        }

        /// <summary>
        /// Extract the byte string representation.
        /// </summary>
        /// <returns>the byte string representation</returns>
        public virtual ByteString GetConstructorParameters()
        {
            return ByteString.CopyFrom(constructorParameters);
        }

        /// <summary>
        /// Sets the constructor parameters as their raw bytes.
        /// <p>
        /// Use this instead of {@link #setConstructorParameters(ContractFunctionParameters)} if you have already pre-encoded
        /// a solidity function call.
        /// </summary>
        /// <param name="constructorParameters">The constructor parameters</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetConstructorParameters(byte[] constructorParameters)
        {
            constructorParameters = Array.CopyOf(constructorParameters, constructorParameters.Length);
            return this;
        }

        /// <summary>
        /// Sets the parameters to pass to the constructor.
        /// </summary>
        /// <param name="constructorParameters">The contructor parameters</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetConstructorParameters(ContractFunctionParameters constructorParameters)
        {
            Objects.RequireNonNull(constructorParameters);
            return SetConstructorParameters(constructorParameters.ToBytes(null).ToByteArray());
        }

        /// <summary>
        /// Extract the contract memo.
        /// </summary>
        /// <returns>the contract memo</returns>
        public virtual string GetContractMemo()
        {
            return contractMemo;
        }

        /// <summary>
        /// Sets the memo to be associated with this contract.
        /// </summary>
        /// <param name="memo">The String to be set as the memo</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetContractMemo(string memo)
        {
            Objects.RequireNonNull(memo);
            contractMemo = memo;
            return this;
        }

        /// <summary>
        /// ID of the account to which this contract will stake
        /// </summary>
        /// <returns>ID of the account to which this contract will stake.</returns>
        public virtual AccountId GetStakedAccountId()
        {
            return stakedAccountId;
        }

        /// <summary>
        /// Set the account to which this contract will stake
        /// </summary>
        /// <param name="stakedAccountId">ID of the account to which this contract will stake.</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetStakedAccountId(AccountId stakedAccountId)
        {
            stakedAccountId = stakedAccountId;
            stakedNodeId = null;
            return this;
        }

        /// <summary>
        /// The node to which this contract will stake
        /// </summary>
        /// <returns>ID of the node this contract will be staked to.</returns>
        public virtual long GetStakedNodeId()
        {
            return stakedNodeId;
        }

        /// <summary>
        /// Set the node to which this contract will stake
        /// </summary>
        /// <param name="stakedNodeId">ID of the node this contract will be staked to.</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetStakedNodeId(long stakedNodeId)
        {
            stakedNodeId = stakedNodeId;
            stakedAccountId = null;
            return this;
        }

        /// <summary>
        /// If true, the contract declines receiving a staking reward. The default value is false.
        /// </summary>
        /// <returns>If true, the contract declines receiving a staking reward. The default value is false.</returns>
        public virtual bool GetDeclineStakingReward()
        {
            return declineStakingReward;
        }

        /// <summary>
        /// If true, the contract declines receiving a staking reward. The default value is false.
        /// </summary>
        /// <param name="declineStakingReward">- If true, the contract declines receiving a staking reward. The default value is
        ///                             false.</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetDeclineStakingReward(bool declineStakingReward)
        {
            declineStakingReward = declineStakingReward;
            return this;
        }

        /// <summary>
        /// Extract the list of node account id's.
        /// </summary>
        /// <returns>the list of node account id's</returns>
        public virtual IList<AccountId> GetNodeAccountIds()
        {
            return nodeAccountIds != null ? Collections.UnmodifiableList(nodeAccountIds) : null;
        }

        /// <summary>
        /// Set the account IDs of the nodes that this transaction will be submitted to.
        /// <p>
        /// Providing an explicit node account ID interferes with client-side load balancing of the network. By default, the
        /// SDK will pre-generate a transaction for 1/3 of the nodes on the network. If a node is down, busy, or otherwise
        /// reports a fatal error, the SDK will try again with a different node.
        /// </summary>
        /// <param name="nodeAccountIds">The list of node AccountIds to be set</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SetNodeAccountIds(IList<AccountId> nodeAccountIds)
        {
            Objects.RequireNonNull(nodeAccountIds);
            nodeAccountIds = new List(nodeAccountIds);
            return this;
        }

        /// <summary>
        /// Set the client that this transaction will be frozen with.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow FreezeWith(Client client)
        {
            freezeWithClient = client;
            return this;
        }

        /// <summary>
        /// Set the private key that this transaction will be signed with.
        /// </summary>
        /// <param name="privateKey">the private key used for signing</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow Sign(PrivateKey privateKey)
        {
            signPrivateKey = privateKey;
            signPublicKey = null;
            transactionSigner = null;
            return this;
        }

        /// <summary>
        /// Set the public key and key list that this transaction will be signed with.
        /// </summary>
        /// <param name="publicKey">the public key</param>
        /// <param name="transactionSigner">the key list</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SignWith(PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
        {
            signPublicKey = publicKey;
            transactionSigner = transactionSigner;
            signPrivateKey = null;
            return this;
        }

        /// <summary>
        /// Set the operator that this transaction will be signed with.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>{@code this}</returns>
        public virtual ContractCreateFlow SignWithOperator(Client client)
        {
            var operator = Objects.RequireNonNull(client.GetOperator());
            signPublicKey = @operator.publicKey;
            transactionSigner = @operator.transactionSigner;
            signPrivateKey = null;
            return this;
        }

        private void SplitBytecode()
        {
            if (bytecode.Length > FILE_CREATE_MAX_BYTES)
            {
                createBytecode = bytecode.Substring(0, FILE_CREATE_MAX_BYTES);
                appendBytecode = bytecode.Substring(FILE_CREATE_MAX_BYTES);
            }
            else
            {
                createBytecode = bytecode;
                appendBytecode = "";
            }
        }

        private FileCreateTransaction CreateFileCreateTransaction(Client client)
        {
            var fileCreateTx = new FileCreateTransaction().SetKeys(Objects.RequireNonNull(client.GetOperatorPublicKey())).SetContents(createBytecode);
            if (nodeAccountIds != null)
            {
                fileCreateTx.SetNodeAccountIds(nodeAccountIds);
            }

            return fileCreateTx;
        }

        private FileAppendTransaction CreateFileAppendTransaction(FileId fileId)
        {
            var fileAppendTx = new FileAppendTransaction().SetFileId(fileId).SetContents(appendBytecode);
            if (maxChunks != null)
            {
                fileAppendTx.SetMaxChunks(maxChunks);
            }

            if (nodeAccountIds != null)
            {
                fileAppendTx.SetNodeAccountIds(nodeAccountIds);
            }

            return fileAppendTx;
        }

        private ContractCreateTransaction CreateContractCreateTransaction(FileId fileId)
        {
            var contractCreateTx = new ContractCreateTransaction().SetBytecodeFileId(fileId).SetConstructorParameters(constructorParameters).SetGas(gas).SetInitialBalance(initialBalance).SetMaxAutomaticTokenAssociations(maxAutomaticTokenAssociations).SetDeclineStakingReward(declineStakingReward);
            if (adminKey != null)
            {
                contractCreateTx.SetAdminKey(adminKey);
            }

            if (proxyAccountId != null)
            {
                contractCreateTx.SetProxyAccountId(proxyAccountId);
            }

            if (autoRenewPeriod != null)
            {
                contractCreateTx.SetAutoRenewPeriod(autoRenewPeriod);
            }

            if (autoRenewAccountId != null)
            {
                contractCreateTx.SetAutoRenewAccountId(autoRenewAccountId);
            }

            if (contractMemo != null)
            {
                contractCreateTx.SetContractMemo(contractMemo);
            }

            if (nodeAccountIds != null)
            {
                contractCreateTx.SetNodeAccountIds(nodeAccountIds);
            }

            if (stakedAccountId != null)
            {
                contractCreateTx.SetStakedAccountId(stakedAccountId);
            }
            else if (stakedNodeId != null)
            {
                contractCreateTx.SetStakedNodeId(stakedNodeId);
            }

            if (freezeWithClient != null)
            {
                contractCreateTx.FreezeWith(freezeWithClient);
            }

            if (signPrivateKey != null)
            {
                contractCreateTx.Sign(signPrivateKey);
            }
            else if (signPublicKey != null && transactionSigner != null)
            {
                contractCreateTx.SignWith(signPublicKey, transactionSigner);
            }

            return contractCreateTx;
        }

        /// <summary>
        /// Create a new transaction receipt query.
        /// </summary>
        /// <param name="response">the transaction response</param>
        /// <returns>the receipt query</returns>
        virtual TransactionReceiptQuery CreateTransactionReceiptQuery(TransactionResponse response)
        {
            return new TransactionReceiptQuery().SetNodeAccountIds(Collections.SingletonList(response.nodeId)).SetTransactionId(response.transactionId);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>the response</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public virtual TransactionResponse Execute(Client client)
        {
            return Execute(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <returns>the response</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public virtual TransactionResponse Execute(Client client, Duration timeoutPerTransaction)
        {
            try
            {
                SplitBytecode();
                var fileId = CreateFileCreateTransaction(client).Execute(client, timeoutPerTransaction).GetReceipt(client, timeoutPerTransaction).fileId;
                Objects.RequireNonNull(fileId);
                if (!appendBytecode.IsEmpty())
                {
                    CreateFileAppendTransaction(fileId).Execute(client, timeoutPerTransaction);
                }

                var response = CreateContractCreateTransaction(fileId).Execute(client, timeoutPerTransaction);
                response.GetReceipt(client, timeoutPerTransaction);
                new FileDeleteTransaction().SetFileId(fileId).Execute(client, timeoutPerTransaction);
                return response;
            }
            catch (ReceiptStatusException e)
            {
                throw new Exception(string.Empty, e);
            }
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>the response</returns>
        public virtual Task<TransactionResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <returns>the response</returns>
        public virtual Task<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerTransaction)
        {
            SplitBytecode();
            return CreateFileCreateTransaction(client).ExecuteAsync(client, timeoutPerTransaction).ThenCompose((fileCreateResponse) => CreateTransactionReceiptQuery(fileCreateResponse).ExecuteAsync(client, timeoutPerTransaction).ThenApply((receipt) => receipt.fileId)).ThenCompose((fileId) =>
            {
                Task appendFuture = appendBytecode.IsEmpty() ? Task.FromResult(null) : CreateFileAppendTransaction(fileId).ExecuteAsync(client, timeoutPerTransaction).ThenApply((ignored) => null);
                return appendFuture.ThenCompose((ignored) => CreateContractCreateTransaction(fileId).ExecuteAsync(client, timeoutPerTransaction).ThenApply((contractCreateResponse) =>
                {
                    CreateTransactionReceiptQuery(contractCreateResponse).ExecuteAsync(client, timeoutPerTransaction).ThenRun(() =>
                    {
                        new FileDeleteTransaction().SetFileId(fileId).ExecuteAsync(client, timeoutPerTransaction);
                    });
                    return contractCreateResponse;
                }));
            });
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse, Exception> callback)
        {
            ActionHelper.Action(ExecuteAsync(client), callback);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, Action<TransactionResponse, Exception> callback)
        {
            ActionHelper.Action(ExecuteAsync(client, timeoutPerTransaction), callback);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAsync(client, timeoutPerTransaction), onSuccess, onFailure);
        }
    }
}