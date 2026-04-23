// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="T:that"]/*' />
    // Re-use the WithExecute interface that was generated for Executable
    public class ContractCreateFlow
    {
        static readonly int FILE_CREATE_MAX_BYTES = 2048;

        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.Bytecode"]/*' />
        public virtual string Bytecode { get; set; } = string.Empty;
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.ToHexString(value)"]/*' />
		public virtual byte[] Bytecode_Bytes { set => Bytecode = Hex.ToHexString(value); }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.ToByteArray"]/*' />
		public virtual ByteString Bytecode_ByteString { set => Bytecode_Bytes = value.ToByteArray(); }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.MaxChunks"]/*' />
        public virtual int? MaxChunks { get; set; }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.AdminKey"]/*' />
        public virtual Key? AdminKey { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.Gas"]/*' />
		public virtual long Gas { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.InitialBalance"]/*' />
		public virtual Hbar InitialBalance { get; set; } = Hbar.ZERO;
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.ProxyAccountId"]/*' />
		public virtual AccountId? ProxyAccountId { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.MaxAutomaticTokenAssociations"]/*' />
		public virtual int MaxAutomaticTokenAssociations { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.AutoRenewPeriod"]/*' />
		public virtual TimeSpan? AutoRenewPeriod { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.AutoRenewAccountId"]/*' />
		public virtual AccountId? AutoRenewAccountId { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.ConstructorParameters"]/*' />
		public virtual byte[] ConstructorParameters { get; set; } = [];
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.CopyFrom(ConstructorParameters)"]/*' />
		public virtual ByteString ConstructorParameters_ByteString
		{
			get => ByteString.CopyFrom(ConstructorParameters);
		}
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.ContractMemo"]/*' />
		public virtual string? ContractMemo { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="T:that_2"]/*' />
		public virtual List<AccountId> NodeAccountIds
        {
			set => field = [.. value];
			get => field ?? [];
        }
		public virtual string CreateBytecode { get; set; } = string.Empty;
		public virtual string AppendBytecode { get; set; } = string.Empty;
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.StakedAccountId"]/*' />
		public virtual AccountId? StakedAccountId { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.StakedNodeId"]/*' />
		public virtual long? StakedNodeId { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.DeclineStakingReward"]/*' />
		public virtual bool DeclineStakingReward { get; set; }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="P:that.FreezeWithClient"]/*' />
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
                fileAppendTx.MaxChunks = MaxChunks.Value;

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
				NodeAccountIds = [.. NodeAccountIds],
				StakedAccountId = StakedAccountId,
				StakedNodeId = StakedNodeId
			};

			if (ContractMemo is not null)
				transaction.ContractMemo = ContractMemo;

			if (FreezeWithClient != null)
				transaction.FreezeWith(FreezeWithClient);

            if (SignPrivateKey != null)
				transaction.Sign(SignPrivateKey);
            else if (SignPublicKey != null && TransactionSigner != null)
				transaction.SignWith(SignPublicKey, TransactionSigner);

			return transaction;
        }

  
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.CreateTransactionReceiptQuery(TransactionResponse)"]/*' />
		public virtual TransactionReceiptQuery CreateTransactionReceiptQuery(TransactionResponse response)
        {
            return new TransactionReceiptQuery
            {
				NodeAccountIds = [response.NodeId],
				TransactionId = response.TransactionId,
			};
        }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.Execute(Client)"]/*' />
        public virtual TransactionResponse Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
        }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.Execute(Client,System.TimeSpan)"]/*' />
        public virtual TransactionResponse Execute(Client client, TimeSpan timeoutPerTransaction)
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
                new FileDeleteTransaction
				{
					FileId = fileId,

				}.Execute(client, timeoutPerTransaction);
                return response;
            }
            catch (ReceiptStatusException e)
            {
                throw new Exception(string.Empty, e);
            }
        }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.ExecuteAsync(Client)"]/*' />
        public virtual Task<TransactionResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.RequestTimeout);
        }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.ExecuteAsync(Client,System.TimeSpan)"]/*' />
        public virtual async Task<TransactionResponse> ExecuteAsync(Client client, TimeSpan timeoutPerTransaction)
        {
            SplitBytecode();

			TransactionResponse createFileCreateResponse = await CreateFileCreateTransaction(client).ExecuteAsync(client, timeoutPerTransaction);
			TransactionReceipt createFileCreateReceipt = await CreateTransactionReceiptQuery(createFileCreateResponse).ExecuteAsync(client, timeoutPerTransaction);

			TransactionResponse? transactionresponse2 = AppendBytecode.Length == 0
				? await Task.FromResult<TransactionResponse?>(null)
				: await CreateFileAppendTransaction(createFileCreateReceipt.FileId).ExecuteAsync(client, timeoutPerTransaction);

            TransactionResponse createContractCreateResponse = await CreateContractCreateTransaction(createFileCreateReceipt.FileId).ExecuteAsync(client, timeoutPerTransaction);
            TransactionReceipt createContractCreateReceipt = await CreateTransactionReceiptQuery(createContractCreateResponse).ExecuteAsync(client, timeoutPerTransaction);

            await new FileDeleteTransaction
			{
				FileId = createFileCreateReceipt.FileId

			}.ExecuteAsync(client, timeoutPerTransaction);

            return createContractCreateResponse;
        }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.ExecuteAsync(Client,System.Action{TransactionResponse,System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse?, Exception?> callback)
        {
            Utils.ActionHelper.Action(ExecuteAsync(client), callback);
        }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.ExecuteAsync(Client,System.TimeSpan,System.Action{TransactionResponse,System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, TimeSpan timeoutPerTransaction, Action<TransactionResponse?, Exception?> callback)
        {
            Utils.ActionHelper.Action(ExecuteAsync(client, timeoutPerTransaction), callback);
        }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.ExecuteAsync(Client,System.Action{TransactionResponse},System.Action{System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
        }
        /// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.ExecuteAsync(Client,System.TimeSpan,System.Action{TransactionResponse},System.Action{System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, TimeSpan timeoutPerTransaction, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(ExecuteAsync(client, timeoutPerTransaction), onSuccess, onFailure);
        }
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.SetConstructorParameters(System.Byte[])"]/*' />
		public virtual ContractCreateFlow SetConstructorParameters(byte[] constructorParameters)
		{
			constructorParameters = constructorParameters.CopyArray();
			return this;
		}
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.SetConstructorParameters(ContractFunctionParameters)"]/*' />
		public virtual ContractCreateFlow SetConstructorParameters(ContractFunctionParameters constructorParameters)
		{
			ArgumentNullException.ThrowIfNull(constructorParameters);
			return SetConstructorParameters(constructorParameters.ToBytes(null).ToByteArray());
		}
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.Sign(PrivateKey)"]/*' />
		public virtual ContractCreateFlow Sign(PrivateKey privateKey)
		{
			SignPrivateKey = privateKey;
			SignPublicKey = null;
			TransactionSigner = null;
			return this;
		}
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.SignWith(PublicKey,System.Func{System.Byte[],System.Byte[]})"]/*' />
		public virtual ContractCreateFlow SignWith(PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
		{
			SignPublicKey = publicKey;
			TransactionSigner = transactionSigner;
			SignPrivateKey = null;
			return this;
		}
		/// <include file="ContractCreateFlow.cs.xml" path='docs/member[@name="M:that.SignWithOperator(Client)"]/*' />
		public virtual ContractCreateFlow SignWithOperator(Client client)
		{
			SignPublicKey = client.Operator_.PublicKey;
			TransactionSigner = client.Operator_.TransactionSigner;
			SignPrivateKey = null;

			return this;
		}
	}
}