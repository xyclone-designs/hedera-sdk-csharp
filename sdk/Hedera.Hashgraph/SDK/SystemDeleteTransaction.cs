using Hedera.Hashgraph.SDK;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata;

namespace Hedera.Hashgraph.SDK
{
	/**
 * @deprecated
 * This transaction is obsolete, not supported, and SHALL fail with a
 * pre-check result of `NOT_SUPPORTED`.
 *
 * Delete a file or contract bytecode as an administrative transaction.
 * > Note
 * >> A system delete/undelete for a `contractID` is not supported and
 * >> SHALL return `INVALID_FILE_ID` or `MISSING_ENTITY_ID`.
 *
 * This transaction MAY be reversed by the `systemUndelete` transaction.
 * A file deleted via `fileDelete`, however SHALL be irrecoverable.<br/>
 * This transaction MUST specify an expiration timestamp (with seconds
 * precision). The file SHALL be permanently removed from state when
 * network consensus time exceeds the specified expiration time.<br/>
 * This transaction MUST be signed by an Hedera administrative ("system")
 * account.
 *
 * ### What is a "system" file
 * A "system" file is any file with a file number less than or equal to the
 * current configuration value for `ledger.numReservedSystemEntities`,
 * typically `750`.
 *
 * ### Block Stream Effects
 * None
 */
	[Obsolete]
public sealed class SystemDeleteTransaction extends Transaction<SystemDeleteTransaction> {
		@Nullable

	private FileId fileId = null;

	@Nullable
	private ContractId contractId = null;

	@Nullable
	private DateTimeOffset expirationTime = null;

	/**
     * Constructor.
     */
	public SystemDeleteTransaction() { }

	/**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction)
     *            records
     * @       when there is an issue with the protobuf
     */
	SystemDeleteTransaction(
			LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)

			
	{
		super(txs);
		initFromTransactionBody();
	}

	/**
     * Constructor.
     *
     * @param txBody protobuf TransactionBody
     */
	SystemDeleteTransaction(Proto.TransactionBody txBody)
	{
		super(txBody);
		initFromTransactionBody();
	}

	/**
     * Extract the file id.
     *
     * @return                          the file id
     */
	@Nullable
	public readonly FileId getFileId()
	{
		return fileId;
	}

	/**
     * A file identifier.
     * <p>
     * The identified file MUST exist in the HFS.<br/>
     * The identified file MUST NOT be deleted.<br/>
     * The identified file MUST NOT be a "system" file.<br/>
     * This field is REQUIRED.
     *
     * Mutually exclusive with {@link #setContractId(ContractId)}.
     *
     * @param fileId The FileId to be set
     * @return {@code this}
     */
	public SystemDeleteTransaction setFileId(FileId fileId)
	{
		Objects.requireNonNull(fileId);
		requireNotFrozen();
		this.fileId = fileId;
		this.contractId = null; // Reset contractId
		return this;
	}

	/**
     * Extract the contract id.
     *
     * @return                          the contract id
     */
	@Nullable
	public ContractId getContractId()
	{
		return contractId;
	}

	/**
     * A contract identifier.
     * <p>
     * The identified contract MUST exist in network state.<br/>
     * The identified contract bytecode MUST NOT be deleted.<br/>
     * <p>
     * Mutually exclusive with {@link #setFileId(FileId)}.
     *
     * @param contractId The ContractId to be set
     * @return {@code this}
     */
	public SystemDeleteTransaction setContractId(ContractId contractId)
	{
		Objects.requireNonNull(contractId);
		requireNotFrozen();
		this.contractId = contractId;
		this.fileId = null; // Reset fileId
		return this;
	}

	/**
     * Extract the expiration time.
     *
     * @return                          the expiration time
     */
	@Nullable
	public DateTimeOffset getExpirationTime()
	{
		return expirationTime;
	}

	/**
     * A timestamp indicating when the file will be removed from state.
     * <p>
     * This value SHALL be expressed in seconds since the `epoch`. The `epoch`
     * SHALL be the UNIX epoch with 0 at `1970-01-01T00:00:00.000Z`.<br/>
     * This field is REQUIRED.
     *
     * @param expirationTime The DateTimeOffset to be set as expiration time
     * @return {@code this}
     */
	public SystemDeleteTransaction setExpirationTime(DateTimeOffset expirationTime)
	{
		Objects.requireNonNull(expirationTime);
		requireNotFrozen();

		this.expirationTime = expirationTime;

		return this;
	}

	/**
     * Build the transaction body.
     *
     * @return {@link Proto.SystemDeleteTransactionBody}
     */
	SystemDeleteTransactionBody.Builder build()
	{
		var builder = SystemDeleteTransactionBody.newBuilder();
		if (fileId != null)
		{
			builder.setFileID(fileId.ToProtobuf());
		}
		if (contractId != null)
		{
			builder.setContractID(contractId.ToProtobuf());
		}
		if (expirationTime != null)
		{
			builder.setExpirationTime(DateTimeOffsetConverter.toSecondsProtobuf(expirationTime));
		}

		return builder;
	}

	/**
     * Initialize from the transaction body.
     */
	void initFromTransactionBody()
	{
		var body = sourceTransactionBody.getSystemDelete();
		if (body.hasFileID())
		{
			fileId = FileId.FromProtobuf(body.getFileID());
		}
		if (body.hasContractID())
		{
			contractId = ContractId.FromProtobuf(body.getContractID());
		}
		if (body.hasExpirationTime())
		{
			expirationTime = DateTimeOffsetConverter.FromProtobuf(body.getExpirationTime());
		}
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (fileId != null) {
			fileId.validateChecksum(client);
		}

        if (contractId != null) {
			contractId.validateChecksum(client);
		}
	}

	@Override
	Task<Void> onExecuteAsync(Client client) {
		int modesEnabled = (fileId != null ? 1 : 0) + (contractId != null ? 1 : 0);
		if (modesEnabled != 1)
		{
			throw new IllegalStateException(
					"SystemDeleteTransaction must have exactly 1 of the following fields set: contractId, fileId");
		}
		return super.onExecuteAsync(client);
	}

	@Override
	MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
		if (fileId != null)
		{
			return FileServiceGrpc.getSystemDeleteMethod();
		}
		else
		{
			return SmartContractServiceGrpc.getSystemDeleteMethod();
		}
	}

	@Override
	void onFreeze(TransactionBody.Builder bodyBuilder)
	{
		bodyBuilder.setSystemDelete(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		scheduled.setSystemDelete(build());
	}
}

}