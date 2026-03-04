// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.HBar
{
	/// <include file="HbarUnit.cs.xml" path='docs/member[@name="T:HbarUnit"]/*' />
	public readonly struct HbarUnit
	{
		// Define the units as static readonly fields to mimic Java Enum constants
		public static readonly HbarUnit TINYBAR = new ("TINYBAR", "tℏ", 1L);
		public static readonly HbarUnit MICROBAR = new ("MICROBAR", "μℏ", 100L);
		public static readonly HbarUnit MILLIBAR = new ("MILLIBAR", "mℏ", 100_000L);
		public static readonly HbarUnit HBAR = new ("HBAR", "ℏ", 100_000_000L);
		public static readonly HbarUnit KILOBAR = new ("KILOBAR", "kℏ", 1000 * 100_000_000L);
		public static readonly HbarUnit MEGABAR = new ("MEGABAR", "Mℏ", 1_000_000 * 100_000_000L);
		public static readonly HbarUnit GIGABAR = new ("GIGABAR", "Gℏ", 1_000_000_000 * 100_000_000L);

		private readonly string _name;
		private readonly string _symbol;
		private readonly long _tinybar;

		// Private constructor ensures only the predefined units can be created
		private HbarUnit(string name, string symbol, long tinybar)
		{
			_name = name;
			_symbol = symbol;
			_tinybar = tinybar;
		}

		/// <include file="HbarUnit.cs.xml" path='docs/member[@name="P:HbarUnit.Tinybar"]/*' />
		public long Tinybar => _tinybar;

		/// <include file="HbarUnit.cs.xml" path='docs/member[@name="P:HbarUnit.Symbol"]/*' />
		public string Symbol => _symbol;

		/// <include file="HbarUnit.cs.xml" path='docs/member[@name="M:HbarUnit.ToString"]/*' />
		public override string ToString() => _name.ToLowerInvariant();

		public static IEnumerable<HbarUnit> Values()
		{
			yield return TINYBAR;
			yield return MICROBAR;
			yield return MILLIBAR;
			yield return HBAR;
			yield return KILOBAR;
			yield return MEGABAR;
			yield return GIGABAR;
		}
	}
}