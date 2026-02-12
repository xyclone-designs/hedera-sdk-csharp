// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.System
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// @deprecated
    /// This transaction is obsolete, not supported, and SHALL fail with a
    /// pre-check result of `NOT_SUPPORTED`.
    /// 
    /// Delete a file or contract bytecode as an administrative transaction.
    /// > Note
    /// >> A system delete/undelete for a `contractID` is not supported and
    /// >> SHALL return `INVALID_FILE_ID` or `MISSING_ENTITY_ID`.
    /// 
    /// This transaction MAY be reversed by the `systemUndelete` transaction.
    /// A file deleted via `fileDelete`, however SHALL be irrecoverable.<br/>
    /// This transaction MUST specify an expiration timestamp (with seconds
    /// precision). The file SHALL be permanently removed from state when
    /// network consensus time exceeds the specified expiration time.<br/>
    /// This transaction MUST be signed by an Hedera administrative ("system")
    /// account.
    /// 
    /// ### What is a "system" file
    /// A "system" file is any file with a file number less than or equal to the
    /// current configuration value for `ledger.numReservedSystemEntities`,
    /// typically `750`.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </remarks>
    [Obsolete("Obsolete")]
    public sealed class SystemDeleteTransaction : Transaction<SystemDeleteTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SystemDeleteTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		public SystemDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public SystemDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// A file identifier.
		/// <p>
		/// The identified file MUST exist in the HFS.<br/>
		/// The identified file MUST NOT be deleted.<br/>
		/// The identified file MUST NOT be a "system" file.<br/>
		/// This field is REQUIRED.
		/// 
		/// Mutually exclusive with {@link #setContractId(ContractId)}.
		/// </summary>
		public FileId? FileId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				ContractId = null; // Reset ContractId
			}
		}
        /// <summary>
        /// A contract identifier.
        /// <p>
        /// The identified contract MUST exist in network state.<br/>
        /// The identified contract bytecode MUST NOT be deleted.<br/>
        /// <p>
        /// Mutually exclusive with {@link #setFileId(FileId)}.
        /// </summary>
        public ContractId? ContractId
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
				FileId = null; // Reset ContractId
			}
        }
		/// <summary>
		/// A timestamp indicating when the file will be removed from state.
		/// <p>
		/// This value SHALL be expressed in seconds since the `epoch`. The `epoch`
		/// SHALL be the UNIX epoch with 0 at `1970-01-01T00:00:00.000Z`.<br/>
		/// This field is REQUIRED.
		/// </summary>
		public Timestamp? ExpirationTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.SystemDeleteTransactionBody}</returns>
        public Proto.SystemDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.SystemDeleteTransactionBody();

            if (FileId != null)
            {
                builder.FileID = FileId.ToProtobuf();
            }

            if (ContractId != null)
            {
                builder.ContractID = ContractId.ToProtobuf();
            }

            if (ExpirationTime != null)
            {
                builder.ExpirationTime = Utils.TimestampConverter.ToSecondsProtobuf(ExpirationTime);
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
			var body = SourceTransactionBody.SystemDelete;

			FileId = FileId.FromProtobuf(body.FileID);
			ContractId = ContractId.FromProtobuf(body.ContractID);
			ExpirationTime = Utils.TimestampConverter.FromProtobuf(body.ExpirationTime);
		}

        public override void ValidateChecksums(Client client)
        {
            FileId?.ValidateChecksum(client);
            ContractId?.ValidateChecksum(client);
        }
        public override Task OnExecuteAsync(Client client)
        {
            int modesEnabled = (FileId != null ? 1 : 0) + (ContractId != null ? 1 : 0);
            if (modesEnabled != 1)
            {
                throw new InvalidOperationException("SystemDeleteTransaction must have exactly 1 of the following fields set: ContractId, FileId");
            }

            return Task.CompletedTask;
        }
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SystemDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
			scheduled.SystemDelete = ToProtobuf();
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
            if (FileId is not null)
            {
                string methodname = nameof(Proto.FileService.FileServiceClient.systemDelete);

                return Proto.FileService.Descriptor.FindMethodByName(methodname);
            }
            else
            {
				string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.systemDelete);

				return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
			}
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}