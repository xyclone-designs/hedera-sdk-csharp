namespace Hedera.Hashgraph.SDK
{
	public class PendingAirdropRecord
	{
		PendingAirdropRecord(PendingAirdropId pendingAirdropId, ulong pendingAirdropAmount)
		{
			PendingAirdropId = pendingAirdropId;
			PendingAirdropAmount = pendingAirdropAmount;
		}

		public PendingAirdropId PendingAirdropId { get; }
		public ulong PendingAirdropAmount { get; }

		public Proto.PendingAirdropRecord ToProtobuf()
		{
			return new Proto.PendingAirdropRecord
			{ 
				PendingAirdropId = PendingAirdropId.ToProtobuf(),
				PendingAirdropValue = new Proto.PendingAirdropValue
				{
					Amount = PendingAirdropAmount
				}
			};
		}

		public static PendingAirdropRecord FromProtobuf(Proto.PendingAirdropRecord pendingAirdropRecord)
		{
			return new PendingAirdropRecord(
				pendingAirdropAmount: pendingAirdropRecord.PendingAirdropValue.Amount,
				pendingAirdropId: PendingAirdropId.FromProtobuf(pendingAirdropRecord.PendingAirdropId));
		}
	}
}