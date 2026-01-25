// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
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
        private ContractId contractId = null;
        private ContractId transferContractId = null;
        private AccountId transferAccountId = null;
        private bool permanentRemoval = null;
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
        ContractDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        ContractDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// Sets the contract ID which should be deleted.
        /// </summary>
        /// <param name="contractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractDeleteTransaction SetContractId(ContractId contractId)
        {
            Objects.RequireNonNull(contractId);
            RequireNotFrozen();
            contractId = contractId;
            return this;
        }

        /// <summary>
        /// Extract the transfer account id.
        /// </summary>
        /// <returns>                         the account id that will receive the remaining hbars</returns>
        public AccountId GetTransferAccountId()
        {
            return transferAccountId;
        }

        /// <summary>
        /// Sets the account ID which will receive all remaining hbars.
        /// <p>
        /// This is mutually exclusive with {@link #setTransferContractId(ContractId)}.
        /// </summary>
        /// <param name="transferAccountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractDeleteTransaction SetTransferAccountId(AccountId transferAccountId)
        {
            Objects.RequireNonNull(transferAccountId);
            RequireNotFrozen();
            transferAccountId = transferAccountId;
            return this;
        }

        /// <summary>
        /// Extract the transfer contract id.
        /// </summary>
        /// <returns>                         the contract id that will receive the remaining hbars</returns>
        public ContractId GetTransferContractId()
        {
            return transferContractId;
        }

        /// <summary>
        /// Sets the contract ID which will receive all remaining hbars.
        /// <p>
        /// This is mutually exclusive with {@link #setTransferAccountId(AccountId)}.
        /// </summary>
        /// <param name="transferContractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractDeleteTransaction SetTransferContractId(ContractId transferContractId)
        {
            Objects.RequireNonNull(transferContractId);
            RequireNotFrozen();
            transferContractId = transferContractId;
            return this;
        }

        /// <summary>
        /// Extract the permanent removal flag.
        /// </summary>
        /// <returns>                         the permanent removal flag</returns>
        public bool GetPermanentRemoval()
        {
            return permanentRemoval;
        }

        /// <summary>
        /// Sets the permanent removal flag.
        /// <p>
        /// This field is reserved for system use. User transactions setting this field
        /// may be rejected by the network with status {@link Status#PERMANENT_REMOVAL_REQUIRES_SYSTEM_INITIATION}.
        /// </summary>
        /// <param name="permanentRemoval">The permanent removal flag to be set</param>
        /// <returns>{@code this}</returns>
        public ContractDeleteTransaction SetPermanentRemoval(bool permanentRemoval)
        {
            RequireNotFrozen();
            permanentRemoval = permanentRemoval;
            return this;
        }

        /// <summary>
        /// Validates tha the contract id, transfer contract id and the transfer account id are valid.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        override void ValidateChecksums(Client client)
        {
            if (contractId != null)
            {
                contractId.ValidateChecksum(client);
            }

            if (transferContractId != null)
            {
                transferContractId.ValidateChecksum(client);
            }

            if (transferAccountId != null)
            {
                transferAccountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetDeleteContractMethod();
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetContractDeleteInstance();
            if (body.HasContractID())
            {
                contractId = ContractId.FromProtobuf(body.GetContractID());
            }

            if (body.HasTransferContractID())
            {
                transferContractId = ContractId.FromProtobuf(body.GetTransferContractID());
            }

            if (body.HasTransferAccountID())
            {
                transferAccountId = AccountId.FromProtobuf(body.GetTransferAccountID());
            }

            if (body.GetPermanentRemoval())
            {
                permanentRemoval = true;
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link ContractDeleteTransactionBody}</returns>
        ContractDeleteTransactionBody.Builder Build()
        {
            var builder = ContractDeleteTransactionBody.NewBuilder();
            if (contractId != null)
            {
                builder.SetContractID(contractId.ToProtobuf());
            }

            if (transferAccountId != null)
            {
                builder.SetTransferAccountID(transferAccountId.ToProtobuf());
            }

            if (transferContractId != null)
            {
                builder.SetTransferContractID(transferContractId.ToProtobuf());
            }

            if (permanentRemoval != null)
            {
                builder.SetPermanentRemoval(permanentRemoval.BooleanValue());
            }

            return builder;
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetContractDeleteInstance(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetContractDeleteInstance(Build());
        }
    }
}