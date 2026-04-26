// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.Reference;
using Hedera.Hashgraph.SDK;
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
        static IEnumerator<object[]> GetValueConversions()
        {
            yield return [ new BigDecimal(50000000), HbarUnit.MICROBAR ]; 
            yield return [ new BigDecimal(50000), HbarUnit.MILLIBAR ]; 
            yield return [ new BigDecimal(50), HbarUnit.HBAR ]; 
            yield return [ new BigDecimal("0.05"), HbarUnit.KILOBAR ]; 
            yield return [ new BigDecimal("0.00005"), HbarUnit.MEGABAR ]; 
            yield return [ new BigDecimal("0.00000005"), HbarUnit.GIGABAR ];
        }

        [Fact]
        public virtual void ShouldConstruct()
        {
            Assert.Equal(fiftyHbar.ToTinybars(), fiftyGTinybar);
            Assert.Equal(fiftyHbar.To(HbarUnit.HBAR), new BigDecimal(50));
            Assert.Equal(new Hbar(50).ToTinybars(), fiftyGTinybar);
            Assert.Equal(Hbar.FromTinybars(fiftyGTinybar).ToTinybars(), fiftyGTinybar);
        }
        [Fact]
        public virtual void ShouldNotConstruct()
        {
            Exception exception = Assert.Throws<Exception>(() => new Hbar(BigDecimal.Parse("0.1"), HbarUnit.TINYBAR));
        }
        [Fact]
        public virtual void ShouldDisplay()
        {
            Assert.Equal(fiftyHbar.ToString(), "50 ℏ");
            Assert.Equal(negativeFiftyHbar.ToString(), "-50 ℏ");
            Assert.Equal(Hbar.FromTinybars(1).ToString(), "1 tℏ");
            Assert.Equal(Hbar.FromTinybars(1).Negated().ToString(), "-1 tℏ");
            Assert.Equal(Hbar.FromTinybars(1000).ToString(), "1000 tℏ");
            Assert.Equal(Hbar.FromTinybars(1000).Negated().ToString(), "-1000 tℏ");
        }
        [Theory]
        [MemberData(nameof(ShouldConvert_Data))]
        public virtual void ShouldConvert(BigDecimal value, HbarUnit unit)
        {
            Assert.Equal(Hbar.From(value, unit), fiftyHbar);
            Assert.Equal(fiftyHbar.To(unit), value);
        }
        public static IEnumerable<object?[]> ShouldConvert_Data() { yield return [BigDecimal.ValueOf(0), HbarUnit.HBAR]; }

        [Fact]
        public virtual void ShouldCompare()
        {
            Assert.Equal(fiftyHbar, fiftyHbar);
            Assert.NotEqual(fiftyHbar, hundredHbar);
            Assert.Equal(fiftyHbar.CompareTo(new Hbar(50)), 0);
            Assert.True(fiftyHbar.CompareTo(hundredHbar) < 0);
            Assert.True(hundredHbar.CompareTo(fiftyHbar) > 0);
            Assert.True(fiftyHbar.CompareTo(negativeFiftyHbar) > 0);
        }
        [Fact]
        public virtual void ConstructorWorks()
        {
            new Hbar(1);
        }
        [Fact]
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
        [Fact]
        public virtual void FromStringUnit()
        {
            Assert.Equal(Hbar.FromString("1", HbarUnit.TINYBAR).ToTinybars(), 1);
        }
        [Fact]
        public virtual void From()
        {
            Assert.Equal(Hbar.From(1).ToTinybars(), 100000000);
        }
        [Fact]
        public virtual void FromUnit()
        {
            Assert.Equal(Hbar.From(1, HbarUnit.TINYBAR).ToTinybars(), 1);
        }
        [Fact]
        public virtual void GetValue()
        {
            Assert.Equal(new Hbar(1).GetValue(), BigDecimal.ValueOf(1));
        }
        [Fact]
        public virtual void HasHashCode()
        {
            Assert.Equal(new Hbar(1).GetHashCode(), 100000031);
        }
    }
}