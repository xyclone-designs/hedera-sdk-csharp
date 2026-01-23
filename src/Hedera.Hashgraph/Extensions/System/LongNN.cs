namespace System
{
	public readonly struct LongNN : IEquatable<LongNN>, IComparable<LongNN>
	{
		public LongNN(long val)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(val);

			Value = val;
		}

		public static LongNN Parse(string str)
		{
			return new LongNN(long.Parse(str));
		}

		public long Value { get; }

		public override bool Equals(LongNN other)
		{
			return Value.Equals(other.Value);
		}
		public int CompareTo(LongNN other)
		{
			return Value.CompareTo(other.Value);
		}

		public override int GetHashCode()
        {
			return Value.GetHashCode();
        }
		public override bool Equals(object? obj)
		{
			return ((LongNN?)obj)?.Value.Equals(Value) ?? false;
		}

		public static bool Equals(LongNN? a, LongNN? b)
		{
			return Equals(a?.Value, b?.Value);
		}
		public static int Compare(LongNN? a, LongNN? b)
		{
			return Compare(a?.Value, b?.Value);
		}

		public static bool operator ==(LongNN left, LongNN right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(LongNN left, LongNN right)
		{
			return !(left == right);
		}
		public static bool operator <(LongNN left, LongNN right)
		{
			return left.CompareTo(right) < 0;
		}
		public static bool operator <=(LongNN left, LongNN right)
		{
			return left.CompareTo(right) <= 0;
		}
		public static bool operator >(LongNN left, LongNN right)
		{
			return left.CompareTo(right) > 0;
		}
		public static bool operator >=(LongNN left, LongNN right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static implicit operator long(LongNN nn) => nn.Value;
		public static implicit operator LongNN(long val) => new(val);
    }
}