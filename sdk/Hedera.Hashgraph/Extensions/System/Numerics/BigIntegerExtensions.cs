
namespace System.Numerics
{
	public static class BigIntegerExtensions 
	{
		public static BigInteger Abs(this BigInteger value) { return BigInteger.Abs(value); }
		public static BigInteger Add(this BigInteger left, BigInteger right) { return BigInteger.Add(left, right); }
		public static BigInteger Subtract(this BigInteger left, BigInteger right) { return BigInteger.Subtract(left, right); }
		public static BigInteger Multiply(this BigInteger left, BigInteger right) { return BigInteger.Multiply(left, right); }
		public static BigInteger Divide(this BigInteger dividend, BigInteger divisor) { return BigInteger.Divide(dividend, divisor); }
		public static BigInteger Remainder(this BigInteger dividend, BigInteger divisor) { return BigInteger.Remainder(dividend, divisor); }
		public static BigInteger DivRem(this BigInteger dividend, BigInteger divisor, out BigInteger remainder) { return BigInteger.DivRem(dividend, divisor, out remainder); }
		public static BigInteger Negate(this BigInteger value) { return BigInteger.Negate(value); }
		public static BigInteger GreatestCommonDivisor(this BigInteger left, BigInteger right) { return BigInteger.GreatestCommonDivisor(left, right); }
		public static BigInteger Max(this BigInteger left, BigInteger right) { return BigInteger.Max(left, right); }
		public static BigInteger Min(this BigInteger left, BigInteger right) { return BigInteger.Min(left, right); }
		public static BigInteger ModPow(this BigInteger value, BigInteger exponent, BigInteger modulus) { return BigInteger.ModPow(value, exponent, modulus); }
		public static BigInteger Pow(this BigInteger value, int exponent) { return BigInteger.Pow(value, exponent); }

		public static double Log(this BigInteger value) { return BigInteger.Log(value); }
		public static double Log(this BigInteger value, double baseValue) { return BigInteger.Log(value, baseValue); }
		public static double Log10(this BigInteger value) { return BigInteger.Log10(value); }
	}
}