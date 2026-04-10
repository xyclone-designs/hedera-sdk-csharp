// SPDX-License-Identifier: Apache-2.0
using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.SDK.HBar
{
    /// <include file="Hbar.cs.xml" path='docs/member[@name="T:Hbar"]/*' />
    public sealed class Hbar : IComparable<Hbar>
    {
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.FromTinybars(0)"]/*' />
        public static readonly Hbar ZERO = Hbar.FromTinybars(0);
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.From(50000000000)"]/*' />
        public static readonly Hbar MAX = Hbar.From(50000000000);
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.From(-)"]/*' />
        public static readonly Hbar MIN = Hbar.From(-50000000000);
        private static readonly Regex FROM_STRING_PATTERN = new ("^((?:\\+|\\-)?\\d+(?:\\.\\d+)?)(\\ (tℏ|μℏ|mℏ|ℏ|kℏ|Mℏ|Gℏ))?$");
        private readonly long valueInTinybar;
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.#ctor(System.Int64)"]/*' />
        public Hbar(long amount) : this(amount, HbarUnit.HBAR) { }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.#ctor(BigDecimal)"]/*' />
        public Hbar(BigDecimal amount) : this(amount, HbarUnit.HBAR) { }
		/// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.#ctor(System.Int64,HbarUnit)"]/*' />
		internal Hbar(long amount, HbarUnit unit)
        {
            valueInTinybar = amount * unit.Tinybar;
        }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.#ctor(BigDecimal,HbarUnit)"]/*' />
        internal Hbar(BigDecimal amount, HbarUnit unit)
        {
            var tinybars = amount.Multiply(BigDecimal.ValueOf(unit.Tinybar));

            if (tinybars.DoubleValue() % 1 != 0)
            {
                throw new ArgumentException("Amount and Unit combination results in a fractional value for Tinybar.  Ensure Tinybar value is a whole number.");
            }

            valueInTinybar = tinybars.LongValue();
        }

        private static HbarUnit GetUnit(string symbolString)
        {
            foreach (HbarUnit unit in HbarUnit.Values())
            {
                if (unit.Symbol.Equals(symbolString))
                {
                    return unit;
                }
            }

            throw new ArgumentException("Attempted to convert string to Hbar, but unit symbol \"" + symbolString + "\" was not recognized");
        }

        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.FromString(System.String)"]/*' />
        public static Hbar FromString(string text)
        {
            if (FROM_STRING_PATTERN.Count(text) == 0)
				throw new ArgumentException("Attempted to convert string to Hbar, but \"" + text + "\" was not correctly formatted");

			string[] parts = text.Split(' ');
            return new Hbar(BigDecimal.Parse(parts[0]), parts.Length == 2 ? GetUnit(parts[1]) : HbarUnit.HBAR);
        }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.FromString(System.String,HbarUnit)"]/*' />
        public static Hbar FromString(string text, HbarUnit unit)
        {
            return new Hbar(BigDecimal.Parse(text.ToString()), unit);
        }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.From(System.Int64)"]/*' />
        public static Hbar From(long hbars)
        {
            return new Hbar(hbars, HbarUnit.HBAR);
        }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.From(System.Int64,HbarUnit)"]/*' />
        public static Hbar From(long amount, HbarUnit unit)
        {
            return new Hbar(amount, unit);
        }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.From(BigDecimal)"]/*' />
        public static Hbar From(BigDecimal hbars)
        {
            return new Hbar(hbars, HbarUnit.HBAR);
        }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.From(BigDecimal,HbarUnit)"]/*' />
        public static Hbar From(BigDecimal amount, HbarUnit unit)
        {
            return new Hbar(amount, unit);
        }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.FromTinybars(System.Int64)"]/*' />
        public static Hbar FromTinybars(long tinybars)
        {
            return new Hbar(tinybars, HbarUnit.TINYBAR);
        }
        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.FromTinybars(System.UInt64)"]/*' />
        public static Hbar FromTinybars(ulong tinybars)
        {
            return new Hbar((long)tinybars, HbarUnit.TINYBAR);
        }

        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.To(HbarUnit)"]/*' />
        public BigDecimal To(HbarUnit unit)
        {
            return BigDecimal.ValueOf(valueInTinybar).Divide(BigDecimal.ValueOf(unit.Tinybar));
        }

        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.ToTinybars"]/*' />
        public long ToTinybars()
        {
            return valueInTinybar;
        }

        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.GetValue"]/*' />
        public BigDecimal GetValue()
        {
            return To(HbarUnit.HBAR);
        }

        /// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.Negated"]/*' />
        public Hbar Negated()
        {
            return Hbar.FromTinybars(-valueInTinybar);
        }

		public int CompareTo(Hbar? o)
		{
			return valueInTinybar.CompareTo(o?.valueInTinybar);
		}

		/// <include file="Hbar.cs.xml" path='docs/member[@name="M:Hbar.ToString(HbarUnit)"]/*' />
		public string ToString(HbarUnit unit)
		{
			return To(unit).ToString();
		}

		public override string ToString()
        {
            if (valueInTinybar < 10000 && valueInTinybar > -10000)
				return valueInTinybar.ToString() + " " + HbarUnit.TINYBAR.Symbol;

			return To(HbarUnit.HBAR).ToString() + " " + HbarUnit.HBAR.Symbol;
        }
        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || GetType() != o?.GetType())
            {
                return false;
            }

            Hbar hbar = (Hbar)o;
            return valueInTinybar == hbar.valueInTinybar;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(valueInTinybar);
        }
    }
}