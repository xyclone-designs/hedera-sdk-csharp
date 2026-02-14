// SPDX-License-Identifier: Apache-2.0
using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.SDK.HBar
{
    /// <summary>
    /// Represents a quantity of hbar.
    /// <p>
    /// Implemented as a wrapper class to force handling of units. Direct interfacing with Hedera accepts amounts
    /// in Tinybars however the nominal unit is hbar.
    /// </summary>
    public sealed class Hbar : IComparable<Hbar>
    {
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
        private static readonly Regex FROM_STRING_PATTERN = new ("^((?:\\+|\\-)?\\d+(?:\\.\\d+)?)(\\ (tℏ|μℏ|mℏ|ℏ|kℏ|Mℏ|Gℏ))?$");
        private readonly long valueInTinybar;
        /// <summary>
        /// Constructs a new Hbar of the specified value.
        /// </summary>
        /// <param name="amount">The amount of Hbar</param>
        public Hbar(long amount) : this(amount, HbarUnit.HBAR) { }
        /// <summary>
        /// Constructs a new Hbar of the specified, possibly fractional value.
        /// <p>
        /// The equivalent amount in Tinybar must be an integer and fit in a {@code long} (64-bit signed integer).
        /// <p>
        /// E.g., {@code 1.23456789} is a valid amount of hbar but {@code 0.123456789} is not.
        /// </summary>
        /// <param name="amount">The amount of Hbar</param>
        public Hbar(BigDecimal amount) : this(amount, HbarUnit.HBAR) { }
		/// <summary>
		/// Constructs a new hbar of the specified value in the specified unit.
		/// {@link HbarUnit}
		/// </summary>
		/// <param name="amount">the amount</param>
		/// <param name="unit">the unit for amount</param>
		Hbar(long amount, HbarUnit unit)
        {
            valueInTinybar = amount * unit.Tinybar;
        }
        /// <summary>
        /// Constructs a new hbar of the specified value in the specified unit.
        /// {@link HbarUnit}
        /// </summary>
        /// <param name="amount">the amount</param>
        /// <param name="unit">the unit for amount</param>
        Hbar(BigDecimal amount, HbarUnit unit)
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

        /// <summary>
        /// Converts the provided string into an amount of hbars.
        /// </summary>
        /// <param name="text">The string representing the amount of Hbar</param>
        /// <returns>{@link Hbar}</returns>
        public static Hbar FromString(string text)
        {
            if (FROM_STRING_PATTERN.Count(text) == 0)
				throw new ArgumentException("Attempted to convert string to Hbar, but \"" + text + "\" was not correctly formatted");

			string[] parts = text.Split(' ');
            return new Hbar(BigDecimal.Parse(parts[0]), parts.Length == 2 ? GetUnit(parts[1]) : HbarUnit.HBAR);
        }
        /// <summary>
        /// Converts the provided string into an amount of hbars.
        /// </summary>
        /// <param name="text">The string representing the amount of set units</param>
        /// <param name="unit">The unit to convert from to Hbar</param>
        /// <returns>{@link Hbar}</returns>
        public static Hbar FromString(string text, HbarUnit unit)
        {
            return new Hbar(BigDecimal.Parse(text.ToString()), unit);
        }
        /// <summary>
        /// Returns an Hbar whose value is equal to the specified long.
        /// </summary>
        /// <param name="hbars">The value of Hbar</param>
        /// <returns>{@link Hbar}</returns>
        public static Hbar From(long hbars)
        {
            return new Hbar(hbars, HbarUnit.HBAR);
        }
        /// <summary>
        /// Returns an Hbar representing the value in the given units.
        /// </summary>
        /// <param name="amount">The long representing the amount of set units</param>
        /// <param name="unit">The unit to convert from to Hbar</param>
        /// <returns>{@link Hbar}</returns>
        public static Hbar From(long amount, HbarUnit unit)
        {
            return new Hbar(amount, unit);
        }
        /// <summary>
        /// Returns an Hbar whose value is equal to the specified long.
        /// </summary>
        /// <param name="hbars">The BigDecimal representing the amount of Hbar</param>
        /// <returns>{@link Hbar}</returns>
        public static Hbar From(BigDecimal hbars)
        {
            return new Hbar(hbars, HbarUnit.HBAR);
        }
        /// <summary>
        /// Returns an Hbar representing the value in the given units.
        /// </summary>
        /// <param name="amount">The BigDecimal representing the amount of set units</param>
        /// <param name="unit">The unit to convert from to Hbar</param>
        /// <returns>{@link Hbar}</returns>
        public static Hbar From(BigDecimal amount, HbarUnit unit)
        {
            return new Hbar(amount, unit);
        }
        /// <summary>
        /// Returns an Hbar converted from the specified number of Tinybars.
        /// </summary>
        /// <param name="Tinybars">The long representing the amount of Tinybar</param>
        /// <returns>{@link Hbar}</returns>
        public static Hbar FromTinybars(long tinybars)
        {
            return new Hbar(tinybars, HbarUnit.TINYBAR);
        }
        /// <summary>
        /// Returns an Hbar converted from the specified number of Tinybars.
        /// </summary>
        /// <param name="Tinybars">The long representing the amount of Tinybar</param>
        /// <returns>{@link Hbar}</returns>
        public static Hbar FromTinybars(ulong tinybars)
        {
            return new Hbar((long)tinybars, HbarUnit.TINYBAR);
        }

        /// <summary>
        /// Convert this hbar value to a different unit.
        /// </summary>
        /// <param name="unit">The unit to convert to from Hbar</param>
        /// <returns>BigDecimal</returns>
        public BigDecimal To(HbarUnit unit)
        {
            return BigDecimal.ValueOf(valueInTinybar).Divide(BigDecimal.ValueOf(unit.Tinybar), MathContext.UNLIMITED);
        }

        /// <summary>
        /// Convert this hbar value to Tinybars.
        /// </summary>
        /// <returns>long</returns>
        public long ToTinybars()
        {
            return valueInTinybar;
        }

        /// <summary>
        /// Returns the number of Hbars.
        /// </summary>
        /// <returns>BigDecimal</returns>
        public BigDecimal GetValue()
        {
            return To(HbarUnit.HBAR);
        }

        /// <summary>
        /// Returns a Hbar whose value is {@code -this}.
        /// </summary>
        /// <returns>Hbar</returns>
        public Hbar Negated()
        {
            return Hbar.FromTinybars(-valueInTinybar);
        }

		public int CompareTo(Hbar? o)
		{
			return valueInTinybar.CompareTo(o?.valueInTinybar);
		}

		/// <summary>
		/// Convert hbar to string representation in specified units.
		/// </summary>
		/// <param name="unit">the desired unit</param>
		/// <returns>                         the string representation</returns>
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