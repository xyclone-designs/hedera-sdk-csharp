// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hedera.Hashgraph.Tests.SDK.HBar
{
    public class HbarTest
    {
        private static readonly long fiftyGTinybar = 5000000000;
        private readonly Hbar fiftyHbar = Hbar.FromTinybars(fiftyGTinybar);
        private readonly Hbar hundredHbar = new Hbar(100);
        private readonly Hbar negativeFiftyHbar = new Hbar(-50);
        static IEnumerator<Arguments> GetValueConversions()
        {
            return List.Of(Arguments.Arguments(new BigDecimal(50000000), HbarUnit.MICROBAR), Arguments.Arguments(new BigDecimal(50000), HbarUnit.MILLIBAR), Arguments.Arguments(new BigDecimal(50), HbarUnit.HBAR), Arguments.Arguments(new BigDecimal("0.05"), HbarUnit.KILOBAR), Arguments.Arguments(new BigDecimal("0.00005"), HbarUnit.MEGABAR), Arguments.Arguments(new BigDecimal("0.00000005"), HbarUnit.GIGABAR)).Iterator();
        }

        public virtual void ShouldConstruct()
        {
            Assert.Equal(fiftyHbar.ToTinybars(), fiftyGTinybar);
            Assert.Equal(fiftyHbar.To(HbarUnit.HBAR), new BigDecimal(50));
            Assert.Equal(new Hbar(50).ToTinybars(), fiftyGTinybar);
            Assert.Equal(Hbar.FromTinybars(fiftyGTinybar).ToTinybars(), fiftyGTinybar);
        }

        public virtual void ShouldNotConstruct()
        {
            Exception exception = Assert.Throws<Exception>(() => new Hbar(new BigDecimal("0.1"), HbarUnit.TINYBAR));
        }

        public virtual void ShouldDisplay()
        {
            Assert.Equal(fiftyHbar.ToString(), "50 ℏ");
            Assert.Equal(negativeFiftyHbar.ToString(), "-50 ℏ");
            Assert.Equal(Hbar.FromTinybars(1).ToString(), "1 tℏ");
            Assert.Equal(Hbar.FromTinybars(1).Negated().ToString(), "-1 tℏ");
            Assert.Equal(Hbar.FromTinybars(1000).ToString(), "1000 tℏ");
            Assert.Equal(Hbar.FromTinybars(1000).Negated().ToString(), "-1000 tℏ");
        }

        public virtual void ShouldConvert(BigDecimal value, HbarUnit unit)
        {
            Assert.Equal(Hbar.From(value, unit), fiftyHbar);
            Assert.Equal(fiftyHbar.To(unit), value);
        }

        public virtual void ShouldCompare()
        {
            Assert.Equal(fiftyHbar, fiftyHbar);
            AssertThat(fiftyHbar).IsNotEqualTo(hundredHbar);
            Assert.Equal(fiftyHbar.CompareTo(new Hbar(50)), 0);
            Assert.True(fiftyHbar.CompareTo(hundredHbar) < 0);
            Assert.True(hundredHbar.CompareTo(fiftyHbar) > 0);
            Assert.True(fiftyHbar.CompareTo(negativeFiftyHbar) > 0);
        }

        public virtual void ConstructorWorks()
        {
            new Hbar(1);
        }

        public virtual void FromString()
        {
            Assert.Equal(Hbar.FromString("1").ToTinybars(), 100000000);
            Assert.Equal(Hbar.FromString("1 ℏ").ToTinybars(), 100000000);
            Assert.Equal(Hbar.FromString("1.5 mℏ").ToTinybars(), 150000);
            Assert.Equal(Hbar.FromString("+1.5 mℏ").ToTinybars(), 150000);
            Assert.Equal(Hbar.FromString("-1.5 mℏ").ToTinybars(), -150000);
            Assert.Equal(Hbar.FromString("+3").ToTinybars(), 300000000);
            Assert.Equal(Hbar.FromString("-3").ToTinybars(), -300000000);
            Assert.Throws<ArgumentException>(() =>
            {
                Hbar.FromString("1 h");
            });
            Assert.Throws<ArgumentException>(() =>
            {
                Hbar.FromString("1ℏ");
            });
        }

        public virtual void FromStringUnit()
        {
            Assert.Equal(Hbar.FromString("1", HbarUnit.TINYBAR).ToTinybars(), 1);
        }

        public virtual void From()
        {
            Assert.Equal(Hbar.From(1).ToTinybars(), 100000000);
        }

        public virtual void FromUnit()
        {
            Assert.Equal(Hbar.From(1, HbarUnit.TINYBAR).ToTinybars(), 1);
        }

        public virtual void GetValue()
        {
            Assert.Equal(new Hbar(1).GetValue(), BigDecimal.ValueOf(1));
        }

        public virtual void HasHashCode()
        {
            Assert.Equal(new Hbar(1).GetHashCode(), 100000031);
        }
    }
}