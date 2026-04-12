// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Airdrops
{
    public class PendingAirdropRecord
    {
        public PendingAirdropRecord(PendingAirdropId pendingAirdropId, ulong pendingAirdropAmount)
        {
            PendingAirdropId = pendingAirdropId;
            PendingAirdropAmount = pendingAirdropAmount;
        }

		public virtual ulong PendingAirdropAmount { get; }
		public virtual PendingAirdropId PendingAirdropId { get; }
		public virtual Proto.Services.PendingAirdropRecord ToProtobuf()
        {
            return new Proto.Services.PendingAirdropRecord
            {
				PendingAirdropId = PendingAirdropId.ToProtobuf(),
				PendingAirdropValue = new Proto.Services.PendingAirdropValue
                {
                    Amount = PendingAirdropAmount
				},
			};
        }

        public static PendingAirdropRecord FromProtobuf(Proto.Services.PendingAirdropRecord pendingAirdropRecord)
        {
            return new PendingAirdropRecord(PendingAirdropId.FromProtobuf(pendingAirdropRecord.PendingAirdropId), pendingAirdropRecord.PendingAirdropValue.Amount);
        }
    }
}
