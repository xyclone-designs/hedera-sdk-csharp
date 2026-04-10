// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="FeeSchedules.cs.xml" path='docs/member[@name="T:FeeSchedules"]/*' />
    public class FeeSchedules
    {
        /// <include file="FeeSchedules.cs.xml" path='docs/member[@name="M:FeeSchedules.#ctor"]/*' />
        public FeeSchedules() { }

		private FeeSchedule? _Current;
		private FeeSchedule? _Next;

		/// <include file="FeeSchedules.cs.xml" path='docs/member[@name="M:FeeSchedules.FromBytes(System.Byte[])"]/*' />
		public static FeeSchedules FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.CurrentAndNextFeeSchedule.Parser.ParseFrom(bytes));
		}
		/// <include file="FeeSchedules.cs.xml" path='docs/member[@name="M:FeeSchedules.FromProtobuf(Proto.CurrentAndNextFeeSchedule)"]/*' />
		public static FeeSchedules FromProtobuf(Proto.CurrentAndNextFeeSchedule feeSchedules)
        {
            return new FeeSchedules
            {
                Current = feeSchedules.CurrentFeeSchedule is null ? null : FeeSchedule.FromProtobuf(feeSchedules.CurrentFeeSchedule),
                Next = feeSchedules.NextFeeSchedule is null ? null : FeeSchedule.FromProtobuf(feeSchedules.NextFeeSchedule),
			};
        }

		public virtual FeeSchedule? Current
		{
			get => _Current?.Clone() as FeeSchedule;
			set => _Current = value?.Clone() as FeeSchedule;
		}
		public virtual FeeSchedule? Next
		{
			get => _Next?.Clone() as FeeSchedule;
			set => _Next = value?.Clone() as FeeSchedule;
		}

		/// <include file="FeeSchedules.cs.xml" path='docs/member[@name="M:FeeSchedules.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="FeeSchedules.cs.xml" path='docs/member[@name="M:FeeSchedules.ToProtobuf"]/*' />
		public virtual Proto.CurrentAndNextFeeSchedule ToProtobuf()
        {
			Proto.CurrentAndNextFeeSchedule protobuf = new ();

            if (_Current != null) protobuf.CurrentFeeSchedule = _Current.ToProtobuf();
            if (_Next != null) protobuf.NextFeeSchedule = _Next.ToProtobuf();

            return protobuf;
        }
    }
}