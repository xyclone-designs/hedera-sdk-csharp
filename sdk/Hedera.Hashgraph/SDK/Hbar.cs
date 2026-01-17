using System.Runtime.InteropServices;

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

        private static readonly Pattern FROM_STRING_PATTERN =
                Pattern.compile("^((?:\\+|\\-)?\\d+(?:\\.\\d+)?)(\\ (tℏ|μℏ|mℏ|ℏ|kℏ|Mℏ|Gℏ))?$");
        private readonly long valueInTinybar;

        /**
         * Constructs a new Hbar of the specified value.
         *
         * @param amount The amount of Hbar
         */
        public Hbar(long amount) {
            this(amount, HbarUnit.HBAR);
        }

        /**
         * Constructs a new hbar of the specified value in the specified unit.
         * {@link HbarUnit}
         *
         * @param amount                            the amount
         * @param unit                              the unit for amount
         */
        Hbar(long amount, HbarUnit unit) {
            valueInTinybar = amount * unit.tinybar;
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
        public Hbar(BigDecimal amount) {
            this(amount, HbarUnit.HBAR);
        }

        /**
         * Constructs a new hbar of the specified value in the specified unit.
         * {@link HbarUnit}
         *
         * @param amount                            the amount
         * @param unit                              the unit for amount
         */
        Hbar(BigDecimal amount, HbarUnit unit) {
            var tinybars = amount.multiply(BigDecimal.valueOf(unit.tinybar));

            if (tinybars.doubleValue() % 1 != 0) {
                throw new ArgumentException(
                        "Amount and Unit combination results in a fractional value for tinybar.  Ensure tinybar value is a whole number.");
            }

            valueInTinybar = tinybars.longValue();
        }

        private static HbarUnit GetUnit(string symbolString) {
            for (var unit : HbarUnit.values()) {
                if (unit.getSymbol().equals(symbolString)) {
                    return unit;
                }
            }
            throw new ArgumentException(
                    "Attempted to convert string to Hbar, but unit symbol \"" + symbolString + "\" was not recognized");
        }

        /**
         * Converts the provided string into an amount of hbars.
         *
         * @param text The string representing the amount of Hbar
         * @return {@link Hbar}
         */
        public static Hbar FromString(CharSequence text) {
            var matcher = FROM_STRING_PATTERN.matcher(text);
            if (!matcher.matches()) {
                throw new ArgumentException(
                        "Attempted to convert string to Hbar, but \"" + text + "\" was not correctly formatted");
            }
            var parts = Splitter.on(' ').splitToList(text.toString());
            return new Hbar(new BigDecimal(parts.get(0)), parts.size() == 2 ? getUnit(parts.get(1)) : HbarUnit.HBAR);
        }
        /**
         * Converts the provided string into an amount of hbars.
         *
         * @param text The string representing the amount of set units
         * @param unit The unit to convert from to Hbar
         * @return {@link Hbar}
         */
        public static Hbar FromString(CharSequence text, HbarUnit unit) {
            return new Hbar(new BigDecimal(text.toString()), unit);
        }
        /**
         * Returns an Hbar whose value is equal to the specified long.
         *
         * @param hbars The value of Hbar
         * @return {@link Hbar}
         */
        public static Hbar From(long hbars) 
        {
            return new Hbar(hbars, HbarUnit.HBAR);
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
         * @param hbars The BigDecimal representing the amount of Hbar
         * @return {@link Hbar}
         */
        public static Hbar From(BigDecimal hbars) 
        {
            return new Hbar(hbars, HbarUnit.HBAR);
        }
        /**
         * Returns an Hbar representing the value in the given units.
         *
         * @param amount The BigDecimal representing the amount of set units
         * @param unit   The unit to convert from to Hbar
         * @return {@link Hbar}
         */
        public static Hbar From(BigDecimal amount, HbarUnit unit) 
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
            return new Hbar(tinybars, HbarUnit.TINYBAR);
        }

        /**
         * Convert this hbar value to a different unit.
         *
         * @param unit The unit to convert to from Hbar
         * @return BigDecimal
         */
        public BigDecimal To(HbarUnit unit) {
            return BigDecimal.valueOf(valueInTinybar).divide(BigDecimal.valueOf(unit.tinybar), MathContext.UNLIMITED);
        }

        /**
         * Convert this hbar value to Tinybars.
         *
         * @return long
         */
        public long ToTinybars() {
            return valueInTinybar;
        }

        /**
         * Returns the number of Hbars.
         *
         * @return BigDecimal
         */
        public BigDecimal GetValue() {
            return to(HbarUnit.HBAR);
        }

        /**
         * Returns a Hbar whose value is {@code -this}.
         *
         * @return Hbar
         */
        public Hbar Negated() {
            return Hbar.FromTinybars(-valueInTinybar);
        }

        @Override
        public string ToString() {
            
        }

        /**
         * Convert hbar to string representation in specified units.
         *
         * @param unit                      the desired unit
         * @return                          the string representation
         */
        public string ToString(HbarUnit unit) {
            return to(unit).toString();
        }

        @Override
        public override bool Equals(object? obj) {
            if (this == o) {
                return true;
            }

            if (o == null || getClass() != o.getClass()) {
                return false;
            }

            Hbar hbar = (Hbar) o;
            return valueInTinybar == hbar.valueInTinybar;
        }

        @Override
        public int hashCode() {
            return Objects.hash(valueInTinybar);
        }

        @Override
        public int compareTo(Hbar o) {
            return long.compare(valueInTinybar, o.valueInTinybar);
        }

        public override string ToString()
        {
			if (valueInTinybar < 10_000 && valueInTinybar > -10_000)
			{
				return long.toString(this.valueInTinybar) + " " + HbarUnit.TINYBAR.getSymbol();
			}

			return To(HbarUnit.HBAR).ToString() + " " + HbarUnit.HBAR.getSymbol();
		}
    }

}