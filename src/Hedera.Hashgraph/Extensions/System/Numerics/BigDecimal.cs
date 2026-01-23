
namespace System.Numerics
{
	public readonly struct BigDecimal : IEquatable<BigDecimal>
	{
		public BigInteger UnscaledValue { get; }
		public int Scale { get; }

		public BigDecimal(BigInteger unscaledValue, int scale)
		{
			UnscaledValue = unscaledValue;
			Scale = scale;
		}

		// Common conversion: Create from a double or long
		public static BigDecimal ValueOf(double value)
		{
			// Simple conversion; for high-precision use string parsing
			string s = value.ToString("G17");
			return Parse(s);
		}

		public static BigDecimal Parse(string s)
		{
			int decimalPoint = s.IndexOf('.');
			if (decimalPoint == -1) return new BigDecimal(BigInteger.Parse(s), 0);

			string unscaled = s.Remove(decimalPoint, 1);
			int scale = s.Length - decimalPoint - 1;
			return new BigDecimal(BigInteger.Parse(unscaled), scale);
		}

		public static BigDecimal operator +(BigDecimal a, BigDecimal b)
		{
			// Align scales before adding
			int maxScale = Math.Max(a.Scale, b.Scale);
			BigInteger valA = a.UnscaledValue * BigInteger.Pow(10, maxScale - a.Scale);
			BigInteger valB = b.UnscaledValue * BigInteger.Pow(10, maxScale - b.Scale);
			return new BigDecimal(valA + valB, maxScale);
		}

		public override string ToString()
		{
			string s = UnscaledValue.ToString();
			if (Scale == 0) return s;
			if (Scale > 0) return s.Insert(s.Length - Scale, ".");
			return s + new string('0', -Scale); // Handle negative scales
		}

		public bool Equals(BigDecimal other) => UnscaledValue == other.UnscaledValue && Scale == other.Scale;
	}

}
