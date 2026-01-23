namespace System
{
	public readonly struct IntNN : IEquatable<IntNN>, IComparable<IntNN>
	{
		public IntNN(int val)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(val);

			Value = val;
		}

		public static IntNN Parse(string str)
		{
			return new IntNN(int.Parse(str));
		}

		public int Value { get; }

		public override bool Equals(IntNN other)
		{
			return Value.Equals(other.Value);
		}
		public int CompareTo(IntNN other)
		{
			return Value.CompareTo(other.Value);
		}

		public override int GetHashCode()
        {
			return Value.GetHashCode();
        }
		public override bool Equals(object? obj)
		{
			return ((IntNN?)obj)?.Value.Equals(Value) ?? false;
		}

		public static bool Equals(IntNN? a, IntNN? b)
		{
			return Equals(a?.Value, b?.Value);
		}
		public static int Compare(IntNN? a, IntNN? b)
		{
			return Compare(a?.Value, b?.Value);
		}

		public static bool operator ==(IntNN left, IntNN right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(IntNN left, IntNN right)
		{
			return !(left == right);
		}
		public static bool operator <(IntNN left, IntNN right)
		{
			return left.CompareTo(right) < 0;
		}
		public static bool operator <=(IntNN left, IntNN right)
		{
			return left.CompareTo(right) <= 0;
		}
		public static bool operator >(IntNN left, IntNN right)
		{
			return left.CompareTo(right) > 0;
		}
		public static bool operator >=(IntNN left, IntNN right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static implicit operator int(IntNN nn) => nn.Value;
		public static implicit operator IntNN(int val) => new(val);
    }
}