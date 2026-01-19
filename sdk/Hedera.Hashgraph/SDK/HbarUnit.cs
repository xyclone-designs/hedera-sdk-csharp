using System;

namespace Hedera.Hashgraph.SDK
{
	/// <summary>
	/// Common units of hbar; for the most part they follow SI prefix conventions.
	/// See <a href="https://docs.hedera.com/guides/docs/sdks/hbars#hbar-units">Hedera Documentation</a>
	/// </summary>
	public readonly struct HbarUnit : IEquatable<HbarUnit>
	{
		public static readonly HbarUnit Tinybar = new ("tℏ", 1, "tinybar");
		public static readonly HbarUnit Microbar = new ("μℏ", 100, "microbar");
		public static readonly HbarUnit Millibar = new ("mℏ", 100_000, "millibar");
		public static readonly HbarUnit Hbar = new ("ℏ", 100_000_000, "hbar");
		public static readonly HbarUnit Kilobar = new ("kℏ", 1000 * 100_000_000L, "kilobar");
		public static readonly HbarUnit Megabar = new ("Mℏ", 1_000_000 * 100_000_000L, "megabar");
		public static readonly HbarUnit Gigabar = new ("Gℏ", 1_000_000_000 * 100_000_000L, "gigabar");

		private readonly string _symbol;
		private readonly string _name;

		/// <summary>
		/// The atomic (smallest) unit of hbar.
		/// </summary>
		public long TinybarValue { get; }

		private HbarUnit(string symbol, long tinybar, string name)
		{
			TinybarValue = tinybar;

			_symbol = symbol;
			_name = name;
		}

		public string GetSymbol()
		{
			return _symbol; 
		}
		public bool Equals(HbarUnit other)
		{
			return TinybarValue == other.TinybarValue;
		}
		public override string ToString()
		{
			return _na;
		}
		public override int GetHashCode()
		{
			return TinybarValue.GetHashCode();
		}
		public override bool Equals(object? obj)
		{
			return obj is HbarUnit other && Equals(other);
		}
		public static bool operator ==(HbarUnit left, HbarUnit right) => left.Equals(right);
		public static bool operator !=(HbarUnit left, HbarUnit right) => !left.Equals(right);
	}
}
