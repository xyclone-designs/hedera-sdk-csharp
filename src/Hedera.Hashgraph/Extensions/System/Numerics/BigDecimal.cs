
namespace System.Numerics
{
	public readonly struct BigDecimal(BigInteger unscaledValue, int scale) : IEquatable<BigDecimal>
	{
		public BigInteger UnscaledValue { get; } = unscaledValue;
		public int Scale { get; } = scale;

		public static BigDecimal Parse(string s)
		{
			int decimalPoint = s.IndexOf('.');
			if (decimalPoint == -1) return new BigDecimal(BigInteger.Parse(s), 0);

			string unscaled = s.Remove(decimalPoint, 1);
			int scale = s.Length - decimalPoint - 1;
			return new BigDecimal(BigInteger.Parse(unscaled), scale);
		}
		public static BigDecimal ValueOf(double value)
		{
			// Simple conversion; for high-precision use string parsing
			string s = value.ToString("G17");
			return Parse(s);
		}

		public bool Equals(BigDecimal other)
		{
			return UnscaledValue == other.UnscaledValue && Scale == other.Scale;
		}
		public double DoubleValue() { return 0; } // TODO BigDecimal.DoubleValue
		public long LongValue() { return 0; } // TODO BigDecimal.LongValue
		public BigDecimal Divide(BigDecimal bigdecimal) { return this; } // TODO BigDecimal.Divide
		public BigDecimal Multiply(BigDecimal bigdecimal) { return this; } // TODO BigDecimal.Multiply

		public override bool Equals(object? obj)
		{
			return obj is BigDecimal @decimal && Equals(@decimal);
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(UnscaledValue, Scale);
		}
		public override string ToString()
		{
			string s = UnscaledValue.ToString();
			if (Scale == 0) return s;
			if (Scale > 0) return s.Insert(s.Length - Scale, ".");
			return s + new string('0', -Scale); // Handle negative scales
		}

		public static BigDecimal operator +(BigDecimal a, BigDecimal b)
		{
			// Align scales before adding
			int maxScale = Math.Max(a.Scale, b.Scale);
			
			BigInteger valA = a.UnscaledValue * BigInteger.Pow(10, maxScale - a.Scale);
			BigInteger valB = b.UnscaledValue * BigInteger.Pow(10, maxScale - b.Scale);

			return new BigDecimal(valA + valB, maxScale);
		}
		public static BigDecimal operator -(BigDecimal a, BigDecimal b)
		{
			// Align scales before adding
			int maxScale = Math.Max(a.Scale, b.Scale);

			BigInteger valA = a.UnscaledValue * BigInteger.Pow(10, maxScale - a.Scale);
			BigInteger valB = b.UnscaledValue * BigInteger.Pow(10, maxScale - b.Scale);

			return new BigDecimal(valA - valB, maxScale);
		}

		public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            return !(left == right);
        }
        
    }
}
