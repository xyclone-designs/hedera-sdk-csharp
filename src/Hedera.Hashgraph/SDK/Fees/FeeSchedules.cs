// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// This contains two Fee Schedules with expiry timestamp.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/currentandnextfeeschedule">Hedera Documentation</a>
    /// </summary>
    public class FeeSchedules
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public FeeSchedules() { }

		private FeeSchedule? _Current;
		private FeeSchedule? _Next;

		/// <summary>
		/// Create a fee schedules object from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the fee schedules object</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static FeeSchedules FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.CurrentAndNextFeeSchedule.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a fee schedules object from a protobuf.
		/// </summary>
		/// <param name="feeSchedules">the protobuf</param>
		/// <returns>                         the fee schedules object</returns>
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

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>                         byte array representation</returns>
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>                         protobuf representation</returns>
		public virtual Proto.CurrentAndNextFeeSchedule ToProtobuf()
        {
			Proto.CurrentAndNextFeeSchedule protobuf = new ();

            if (_Current != null) protobuf.CurrentFeeSchedule = _Current.ToProtobuf();
            if (_Next != null) protobuf.NextFeeSchedule = _Next.ToProtobuf();

            return protobuf;
        }
    }
}