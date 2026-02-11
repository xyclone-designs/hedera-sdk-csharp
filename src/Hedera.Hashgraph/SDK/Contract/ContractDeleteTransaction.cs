// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// Delete a smart contract, and transfer any remaining HBAR balance to a
    /// designated account.
    /// 
    /// If this call succeeds then all subsequent calls to that smart contract
    /// SHALL execute the `0x0` opcode, as required for EVM equivalence.
    /// 
    /// ### Requirements
    ///  - An account or smart contract MUST be designated to receive all remaining
    ///    account balances.
    ///  - The smart contract MUST have an admin key set. If the contract does not
    ///    have `admin_key` set, then this transaction SHALL fail and response code
    ///    `MODIFYING_IMMUTABLE_CONTRACT` SHALL be set.
    ///  - If `admin_key` is, or contains, an empty `KeyList` key, it SHALL be
    ///    treated the same as an admin key that is not set.
    ///  - The `Key` set for `admin_key` on the smart contract MUST have a valid
    ///    signature set on this transaction.
    ///  - The designated receiving account MAY have `receiver_sig_required` set. If
    ///    that field is set, the receiver account MUST also sign this transaction.
    ///  - The field `permanent_removal` MUST NOT be set. That field is reserved for
    ///    internal system use when purging the smart contract from state. Any user
    ///    transaction with that field set SHALL be rejected and a response code
    ///    `PERMANENT_REMOVAL_REQUIRES_SYSTEM_INITIATION` SHALL be set.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public sealed class ContractDeleteTransaction : Transaction<ContractDeleteTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContractDeleteTransaction()
        {
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public ContractDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        public ContractDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// Sets the contract ID which should be deleted.
		/// </summary>
		public ContractId? ContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Sets the account ID which will receive all remaining hbars.
		/// <p>
		/// This is mutually exclusive with {@link #setTransferContractId(ContractId)}.
		/// </summary>
		public AccountId? TransferAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Sets the contract ID which will receive all remaining hbars.
		/// <p>
		/// This is mutually exclusive with {@link #setTransferAccountId(AccountId)}.
		/// </summary>
		public ContractId? TransferContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
        /// <summary>
        /// Sets the permanent removal flag.
        /// <p>
        /// This field is reserved for system use. User transactions setting this field
        /// may be rejected by the network with status {@link Status#PERMANENT_REMOVAL_REQUIRES_SYSTEM_INITIATION}.
        /// </summary>
        public bool? PermanentRemoval
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractDeleteInstance;

            if (body.ContractID is not null)
                ContractId = ContractId.FromProtobuf(body.ContractID);

            if (body.TransferContractID is not null)
                TransferContractId = ContractId.FromProtobuf(body.TransferContractID);

            if (body.TransferAccountID is not null)
                TransferAccountId = AccountId.FromProtobuf(body.TransferAccountID);

            if (body.PermanentRemoval)
				PermanentRemoval = true;
		}

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link ContractDeleteTransactionBody}</returns>
        public Proto.ContractDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.ContractDeleteTransactionBody();

            if (ContractId != null) builder.ContractID = ContractId.ToProtobuf();
            if (TransferAccountId != null) builder.TransferAccountID = TransferAccountId.ToProtobuf();
            if (TransferContractId != null) builder.TransferContractID = TransferContractId.ToProtobuf();
            if (PermanentRemoval != null) builder.PermanentRemoval = PermanentRemoval.Value;

            return builder;
        }

		/// <summary>
		/// Validates tha the contract id, transfer contract id and the transfer account id are valid.
		/// </summary>
		/// <param name="client">the configured client</param>
		/// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
		public override void ValidateChecksums(Client client)
		{
			ContractId?.ValidateChecksum(client);
			TransferContractId?.ValidateChecksum(client);
			TransferAccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractDeleteInstance = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractDeleteInstance = ToProtobuf();
        }

		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return SmartContractServiceGrpc.GetDeleteContractMethod();
		}

		public override void OnExecute(Client client)
        {
            throw new System.NotImplementedException();
        }
        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new System.NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new System.NotImplementedException();
        }
    }
}