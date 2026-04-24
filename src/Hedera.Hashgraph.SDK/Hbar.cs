// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.Reference;
using Org.BouncyCastle.Bcpg;
using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="Hbar.cs.xml" path='docs/member[@name="T:Hbar"]/*' />
    public sealed class Hbar : IHbar<Hbar>
    {
        private static readonly Regex FROM_STRING_PATTERN = new("^((?:\\+|\\-)?\\d+(?:\\.\\d+)?)(\\ (tℏ|μℏ|mℏ|ℏ|kℏ|Mℏ|Gℏ))?$");

        /// <summary>
        /// A constant value of zero hbars.
        /// </summary>
        public static readonly Hbar ZERO = Hbar.FromTinybars(0);
        /// <summary>
        /// A constant value of the maximum number of hbars.
        /// </summary>
        public static readonly Hbar MAX = Hbar.From(50000000000);
        /// <summary>
        /// A constant value of the minimum number of hbars.
        /// </summary>
        public static readonly Hbar MIN = Hbar.From(-50000000000);
        private readonly long valueInTinybar;

        public Hbar(long amount) : this(amount, HbarUnit.HBAR) { }
        public Hbar(BigDecimal amount) : this(amount, HbarUnit.HBAR) { }

        internal Hbar(long amount, HbarUnit unit)
        {
            valueInTinybar = amount * unit.Tinybar;
        }
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

        public static Hbar FromString(string text)
        {
            if (FROM_STRING_PATTERN.Count(text) == 0)
                throw new ArgumentException("Attempted to convert string to Hbar, but \"" + text + "\" was not correctly formatted");

            string[] parts = text.Split(' ');
            return new Hbar(BigDecimal.Parse(parts[0]), parts.Length == 2 ? GetUnit(parts[1]) : HbarUnit.HBAR);
        }
        public static Hbar FromString(string text, HbarUnit unit)
        {
            return new Hbar(BigDecimal.Parse(text.ToString()), unit);
        }
        public static Hbar From(long hbars)
        {
            return new Hbar(hbars, HbarUnit.HBAR);
        }
        public static Hbar From(long amount, HbarUnit unit)
        {
            return new Hbar(amount, unit);
        }
        public static Hbar From(BigDecimal hbars)
        {
            return new Hbar(hbars, HbarUnit.HBAR);
        }
        public static Hbar From(BigDecimal amount, HbarUnit unit)
        {
            return new Hbar(amount, unit);
        }
        public static Hbar FromTinybars(long tinybars)
        {
            return new Hbar(tinybars, HbarUnit.TINYBAR);
        }
        public static Hbar FromTinybars(ulong tinybars)
        {
            return new Hbar((long)tinybars, HbarUnit.TINYBAR);
        }

        public long ToTinybars()
        {
            return valueInTinybar;
        }
        public BigDecimal To(HbarUnit unit)
        {
            return BigDecimal.ValueOf(valueInTinybar).Divide(BigDecimal.ValueOf(unit.Tinybar));
        }
        public BigDecimal GetValue()
        {
            return To(HbarUnit.HBAR);
        }

        public Hbar Negated()
        {
            return Hbar.FromTinybars(-valueInTinybar);
        }
        public int CompareTo(Hbar? o)
        {
            return valueInTinybar.CompareTo(o?.valueInTinybar);
        }
        public string ToString(HbarUnit unit)
        {
            return To(unit).ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(valueInTinybar);
        }
        public override bool Equals(object? o)
        {
            if (this == o)
                return true;

            if (o == null || GetType() != o?.GetType())
                return false;

            Hbar hbar = (Hbar)o;

            return valueInTinybar == hbar.valueInTinybar;
        }
        public override string ToString()
        {
            if (valueInTinybar < 10000 && valueInTinybar > -10000)
                return valueInTinybar.ToString() + " " + HbarUnit.TINYBAR.Symbol;

            return To(HbarUnit.HBAR).ToString() + " " + HbarUnit.HBAR.Symbol;
        }

        
    }
}