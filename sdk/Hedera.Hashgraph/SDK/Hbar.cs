using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Represents a quantity of hbar.
 * <p>
 * Implemented as a wrapper class to force handling of units. Direct interfacing with Hedera accepts amounts
 * in tinybars however the nominal unit is hbar.
 */
    public sealed class Hbar : IComparable<Hbar> 
    {
        /**
         * A constant value of zero hbars.
         */
        public static readonly Hbar ZERO = Hbar.FromTinybars(0);
        /**
         * A constant value of the maximum number of hbars.
         */
        public static readonly Hbar MAX = Hbar.From(50_000_000_000L);
        /**
         * A constant value of the minimum number of hbars.
         */
        public static readonly Hbar MIN = Hbar.From(-50_000_000_000L);

        private static readonly Regex FROM_STRING_PATTERN = new ("^((?:\\+|\\-)?\\d+(?:\\.\\d+)?)(\\ (tℏ|μℏ|mℏ|ℏ|kℏ|Mℏ|Gℏ))?$");
        private readonly long valueInTinybar;

        /**
         * Constructs a new Hbar of the specified value.
         *
         * @param amount The amount of Hbar
         */
        public Hbar(long amount) : this(amount, HbarUnit.Hbar) { }

		/**
         * Constructs a new hbar of the specified value in the specified unit.
         * {@link HbarUnit}
         *
         * @param amount                            the amount
         * @param unit                              the unit for amount
         */
		Hbar(long amount, HbarUnit unit) 
        {
            valueInTinybar = amount * unit.TinybarValue;
        }

        /**
         * Constructs a new Hbar of the specified, possibly fractional value.
         * <p>
         * The equivalent amount in tinybar must be an integer and fit in a {@code long} (64-bit signed integer).
         * <p>
         * E.g., {@code 1.23456789} is a valid amount of hbar but {@code 0.123456789} is not.
         *
         * @param amount The amount of Hbar
         */
        public Hbar(decimal amount) : this(amount, HbarUnit.Hbar) { }

		/**
         * Constructs a new hbar of the specified value in the specified unit.
         * {@link HbarUnit}
         *
         * @param amount                            the amount
         * @param unit                              the unit for amount
         */
		Hbar(decimal amount, HbarUnit unit) {
            var tinybars = decimal.Multiply(amount, unit.TinybarValue);

            if (tinybars % 1 != 0) {
                throw new ArgumentException(
                        "Amount and Unit combination results in a fractional value for tinybar.  Ensure tinybar value is a whole number.");
            }

            valueInTinybar = (long)tinybars;
        }

        private static HbarUnit GetUnit(string symbolString) {
            for (var unit : HbarUnit.values()) {
                if (unit.getSymbol().equals(symbolString)) {
                    return unit;
                }
            }
            throw new ArgumentException("Attempted to convert string to Hbar, but unit symbol \"" + symbolString + "\" was not recognized");
        }

        /**
         * Converts the provided string into an amount of hbars.
         *
         * @param text The string representing the amount of Hbar
         * @return {@link Hbar}
         */
        public static Hbar FromString(string text)
        {
            var matcher = FROM_STRING_PATTERN.Matches(text);
            if (matcher.Count == 0) 
                throw new ArgumentException(
                        "Attempted to convert string to Hbar, but \"" + text + "\" was not correctly formatted");
            
            var parts = Splitter.on(' ').splitToList(text);
            return new Hbar(new decimal(parts.get(0)), parts.size() == 2 ? GetUnit(parts.get(1)) : HbarUnit.Hbar);
        }
        /**
         * Converts the provided string into an amount of hbars.
         *
         * @param text The string representing the amount of set units
         * @param unit The unit to convert from to Hbar
         * @return {@link Hbar}
         */
        public static Hbar FromString(string text, HbarUnit unit) {
            return new Hbar(decimal.Parse(text), unit);
        }
        /**
         * Returns an Hbar whose value is equal to the specified long.
         *
         * @param hbars The value of Hbar
         * @return {@link Hbar}
         */
        public static Hbar From(long hbars) 
        {
            return new Hbar(hbars, HbarUnit.Hbar);
        }
        /**
         * Returns an Hbar representing the value in the given units.
         *
         * @param amount The long representing the amount of set units
         * @param unit   The unit to convert from to Hbar
         * @return {@link Hbar}
         */
        public static Hbar From(long amount, HbarUnit unit) 
        {
            return new Hbar(amount, unit);
        }
        /**
         * Returns an Hbar whose value is equal to the specified long.
         *
         * @param hbars The decimal representing the amount of Hbar
         * @return {@link Hbar}
         */
        public static Hbar From(decimal hbars) 
        {
            return new Hbar(hbars, HbarUnit.Hbar);
        }
        /**
         * Returns an Hbar representing the value in the given units.
         *
         * @param amount The decimal representing the amount of set units
         * @param unit   The unit to convert from to Hbar
         * @return {@link Hbar}
         */
        public static Hbar From(decimal amount, HbarUnit unit) 
        {
            return new Hbar(amount, unit);
        }
        /**
         * Returns an Hbar converted from the specified number of tinybars.
         *
         * @param tinybars The long representing the amount of tinybar
         * @return {@link Hbar}
         */
        public static Hbar FromTinybars(long tinybars) 
        {
            return new Hbar(tinybars, HbarUnit.Tinybar);
        }

        /**
         * Convert this hbar value to a different unit.
         *
         * @param unit The unit to convert to from Hbar
         * @return decimal
         */
        public decimal To(HbarUnit unit) 
        {
            return decimal.Divide(valueInTinybar, unit.TinybarValue);
        }

        /**
         * Convert this hbar value to Tinybars.
         *
         * @return long
         */
        public long ToTinybars() 
        {
            return valueInTinybar;
        }

        /**
         * Returns the number of Hbars.
         *
         * @return decimal
         */
        public decimal GetValue()
        {
            return To(HbarUnit.Hbar);
        }

        /**
         * Returns a Hbar whose value is {@code -this}.
         *
         * @return Hbar
         */
        public Hbar Negated() 
        {
            return FromTinybars(-valueInTinybar);
        }

        

        /**
         * Convert hbar to string representation in specified units.
         *
         * @param unit                      the desired unit
         * @return                          the string representation
         */
        public string ToString(HbarUnit unit) {
            return To(unit).ToString();
        }

        public override bool Equals(object? obj) 
        {
            if (this == obj) return true;
            if (obj is not Hbar hbar) return false;

            return valueInTinybar == hbar.valueInTinybar;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(valueInTinybar);
        }



        public override string ToString()
        {
			if (valueInTinybar < 10_000 && valueInTinybar > -10_000)
			{
				return valueInTinybar + " " + HbarUnit.Tinybar.GetSymbol();
			}

			return To(HbarUnit.Hbar).ToString() + " " + HbarUnit.Hbar.GetSymbol();
		}

        public int CompareTo(Hbar? other)
        {
			return valueInTinybar.CompareTo(other?.valueInTinybar);
		}
    }

}