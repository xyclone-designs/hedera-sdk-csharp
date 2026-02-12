// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Transactions.Contract;
using Hedera.Hashgraph.SDK.Transactions.File;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// Extract the hex-encoded bytecode of the contract.
        /// </summary>
        public virtual string Bytecode { get; set; } = string.Empty;
		/// <summary>
		/// Sets the bytecode of the contract in raw bytes.
		/// </summary>
		public virtual byte[] Bytecode_Bytes { set => Bytecode = Hex.ToHexString(value); }
		/// <summary>
		/// Sets the bytecode of the contract in raw bytes.
		/// </summary>
		public virtual ByteString Bytecode_ByteString { set => Bytecode_Bytes = value.ToByteArray(); }
        /// <summary>
        /// The maximum number of chunks
        /// </summary>
        public virtual int? MaxChunks { get; set; }
        /// <summary>
        /// Sets the state of the instance and its fields can be modified arbitrarily if this key signs a transaction to
        /// modify it. If this is null, then such modifications are not possible, and there is no administrator that can
        /// override the normal operation of this smart contract instance. Note that if it is created with no admin keys,
        /// then there is no administrator to authorize changing the admin keys, so there can never be any admin keys for
        /// that instance.
        /// </summary>
        public virtual Key? AdminKey { get; set; }
		/// <summary>
		/// Sets the gas to run the constructor.
		/// </summary>
		public virtual long Gas { get; set; }
		/// <summary>
		/// Sets the initial number of hbars to put into the cryptocurrency account associated with and owned by the smart
		/// contract.
		/// </summary>
		public virtual Hbar InitialBalance { get; set; } = Hbar.ZERO;
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
		public virtual AccountId? ProxyAccountId { get; set; }
		/// <summary>
		/// The maximum number of tokens that an Account can be implicitly associated with. Defaults to 0 and up to a maximum
		/// value of 1000.
		/// </summary>
		public virtual int MaxAutomaticTokenAssociations { get; set; }
		/// <summary>
		/// Sets the period that the instance will charge its account every this many seconds to renew.
		/// </summary>
		public virtual Duration? AutoRenewPeriod { get; set; }
		/// <summary>
		/// Set the account ID which will be charged for renewing this account
		/// </summary>
		public virtual AccountId? AutoRenewAccountId { get; set; }
		/// <summary>
		/// Extract the byte string representation.
		/// </summary>
		/// <returns>the byte string representation</returns>
		public virtual byte[] ConstructorParameters { get; set; } = [];
		/// <summary>
		/// Extract the byte string representation.
		/// </summary>
		/// <returns>the byte string representation</returns>
		public virtual ByteString ConstructorParameters_ByteString
		{
			get => ByteString.CopyFrom(ConstructorParameters);
		}
		/// <summary>
		/// Sets the memo to be associated with this contract.
		/// </summary>
		public virtual string? ContractMemo { get; set; }
		/// <summary>
		/// Set the account IDs of the nodes that this transaction will be submitted to.
		/// <p>
		/// Providing an explicit node account ID interferes with client-side load balancing of the network. By default, the
		/// SDK will pre-generate a transaction for 1/3 of the nodes on the network. If a node is down, busy, or otherwise
		/// reports a fatal error, the SDK will try again with a different node.
		/// </summary>
		public virtual IList<AccountId> NodeAccountIds
        {
			set => field = [.. value];
			get => field?.AsReadOnly() ?? [];
        }
		public virtual string CreateBytecode { get; set; } = string.Empty;
		public virtual string AppendBytecode { get; set; } = string.Empty;
		/// <summary>
		/// Set the account to which this contract will stake
		/// </summary>
		public virtual AccountId? StakedAccountId { get; set; }
		/// <summary>
		/// Set the node to which this contract will stake
		/// </summary>
		public virtual long? StakedNodeId { get; set; }
		/// <summary>
		/// If true, the contract declines receiving a staking reward. The default value is false.
		/// </summary>
		public virtual bool DeclineStakingReward { get; set; }
		/// <summary>
		/// Set the client that this transaction will be frozen with.
		/// </summary>
		public virtual Client? FreezeWithClient { get; set; }
		public virtual PrivateKey? SignPrivateKey { get; set; }
		public virtual PublicKey? SignPublicKey { get; set; }
		public virtual Func<byte[], byte[]>? TransactionSigner { get; set; }

        private void SplitBytecode()
        {
            if (Bytecode.Length > FILE_CREATE_MAX_BYTES)
            {
                CreateBytecode = Bytecode[0..FILE_CREATE_MAX_BYTES];
                AppendBytecode = Bytecode[..FILE_CREATE_MAX_BYTES];
            }
            else
            {
                CreateBytecode = Bytecode;
                AppendBytecode = string.Empty;
            }
        }
        private FileCreateTransaction CreateFileCreateTransaction(Client client)
        {
			FileCreateTransaction fileCreateTx = new ()
            {
				Contents_String = CreateBytecode,
				Keys = [client.OperatorPublicKey],
			};

            if (NodeAccountIds != null)
				fileCreateTx.SetNodeAccountIds(NodeAccountIds);

			return fileCreateTx;
        }
        private FileAppendTransaction CreateFileAppendTransaction(FileId fileId)
        {
			FileAppendTransaction fileAppendTx = new ()
            {
				FileId = fileId,
				Contents_String = AppendBytecode,
			};

            if (MaxChunks != null)
                fileAppendTx.SetMaxChunks(MaxChunks.Value);

            if (NodeAccountIds != null)
				fileAppendTx.SetNodeAccountIds(NodeAccountIds);

			return fileAppendTx;
        }
        private ContractCreateTransaction CreateContractCreateTransaction(FileId fileId)
        {
            ContractCreateTransaction transaction = new()
            {
				BytecodeFileId = fileId,
				ConstructorParameters_Bytes = ConstructorParameters,
				Gas = Gas,
				InitialBalance = InitialBalance,
				MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations,
				DeclineStakingReward = DeclineStakingReward,
				AdminKey = AdminKey,
				ProxyAccountId = ProxyAccountId,
				AutoRenewPeriod = AutoRenewPeriod,
				AutoRenewAccountId = AutoRenewAccountId,
				ContractMemo = ContractMemo,
				NodeAccountIds = [.. NodeAccountIds],
				StakedAccountId = StakedAccountId,
				StakedNodeId = StakedNodeId

			};

			if (FreezeWithClient != null)
				transaction.FreezeWith(FreezeWithClient);

            if (SignPrivateKey != null)
				transaction.Sign(SignPrivateKey);
            else if (SignPublicKey != null && TransactionSigner != null)
				transaction.SignWith(SignPublicKey, TransactionSigner);

			return transaction;
        }

  
		/// <summary>
		/// Create a new transaction receipt query.
		/// </summary>
		/// <param name="response">the transaction response</param>
		/// <returns>the receipt query</returns>
		public virtual TransactionReceiptQuery CreateTransactionReceiptQuery(TransactionResponse response)
        {
            return new TransactionReceiptQuery
            {
				NodeAccountIds = [response.NodeId],
				TransactionId = response.TransactionId,
			};
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
            return Execute(client, client.RequestTimeout);
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
                var fileId = CreateFileCreateTransaction(client).Execute(client, timeoutPerTransaction).GetReceipt(client, timeoutPerTransaction).FileId;
                ArgumentNullException.ThrowIfNull(fileId);
                if (AppendBytecode.Length > 0)
					CreateFileAppendTransaction(fileId).Execute(client, timeoutPerTransaction);

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
            return ExecuteAsync(client, client.RequestTimeout);
        }
        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <returns>the response</returns>
        public virtual async Task<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerTransaction)
        {
            SplitBytecode();

			TransactionResponse createFileCreateResponse = await CreateFileCreateTransaction(client).ExecuteAsync(client, timeoutPerTransaction);
			TransactionReceipt createFileCreateReceipt = await CreateTransactionReceiptQuery(createFileCreateResponse).ExecuteAsync(client, timeoutPerTransaction);

			TransactionResponse? transactionresponse2 = AppendBytecode.Length == 0
				? await Task.FromResult<TransactionResponse?>(null)
				: await CreateFileAppendTransaction(createFileCreateReceipt.FileId).ExecuteAsync(client, timeoutPerTransaction);

            TransactionResponse createContractCreateResponse = await CreateContractCreateTransaction(createFileCreateReceipt.FileId).ExecuteAsync(client, timeoutPerTransaction);
            TransactionReceipt createContractCreateReceipt = await CreateTransactionReceiptQuery(createContractCreateResponse).ExecuteAsync(client, timeoutPerTransaction);

            await new FileDeleteTransaction().SetFileId(createFileCreateReceipt.FileId).ExecuteAsync(client, timeoutPerTransaction);

            return createContractCreateResponse;
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
			constructorParameters = constructorParameters.CopyArray();
			return this;
		}
		/// <summary>
		/// Sets the parameters to pass to the constructor.
		/// </summary>
		/// <param name="constructorParameters">The contructor parameters</param>
		/// <returns>{@code this}</returns>
		public virtual ContractCreateFlow SetConstructorParameters(ContractFunctionParameters constructorParameters)
		{
			ArgumentNullException.ThrowIfNull(constructorParameters);
			return SetConstructorParameters(constructorParameters.ToBytes(null).ToByteArray());
		}
		/// <summary>
		/// Set the private key that this transaction will be signed with.
		/// </summary>
		/// <param name="privateKey">the private key used for signing</param>
		/// <returns>{@code this}</returns>
		public virtual ContractCreateFlow Sign(PrivateKey privateKey)
		{
			SignPrivateKey = privateKey;
			SignPublicKey = null;
			TransactionSigner = null;
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
			SignPublicKey = publicKey;
			TransactionSigner = transactionSigner;
			SignPrivateKey = null;
			return this;
		}
		/// <summary>
		/// Set the operator that this transaction will be signed with.
		/// </summary>
		/// <param name="client">the client with the transaction to execute</param>
		/// <returns>{@code this}</returns>
		public virtual ContractCreateFlow SignWithOperator(Client client)
		{
			SignPublicKey = client.Operator_.PublicKey;
			TransactionSigner = client.Operator_.TransactionSigner;
			SignPrivateKey = null;

			return this;
		}
	}
}