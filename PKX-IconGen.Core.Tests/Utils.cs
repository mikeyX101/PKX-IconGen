using NUnit.Framework;
using PKXIconGen.Core;
using System;

namespace PKX_IconGen.Core.Tests
{
    public class UtilsTests
    {
        [Test]
        public void ConvertRangeIntTest()
        {
            int rangeStart = 0;
            int rangeEnd = 100;
            int newRangeStart = 0;
            int newRangeEnd = 250;
            int value = 50;
            int expectedResult = 125;

            Assert.AreEqual(expectedResult, Utils.ConvertRange(rangeStart, rangeEnd, newRangeStart, newRangeEnd, value));
        }

        [Test]
        public void ConvertRangeUIntTest()
        {
            uint rangeStart = 0;
            uint rangeEnd = 100;
            uint newRangeStart = 0;
            uint newRangeEnd = 250;
            uint value = 50;
            uint expectedResult = 125;

            Assert.AreEqual(expectedResult, Utils.ConvertRange(rangeStart, rangeEnd, newRangeStart, newRangeEnd, value));
        }

        [Test]
        public void ConvertRangeThrowsOnValueLesser()
        {
            int rangeStart = 0;
            int rangeEnd = 100;
            int newRangeStart = 0;
            int newRangeEnd = 250;
            int value = -1;

            Assert.Throws<ArgumentException>(() => Utils.ConvertRange(rangeStart, rangeEnd, newRangeStart, newRangeEnd, value));
        }

        [Test]
        public void ConvertRangeThrowsOnValueGreater()
        {
            int rangeStart = 0;
            int rangeEnd = 100;
            int newRangeStart = 0;
            int newRangeEnd = 250;
            int value = 101;

            Assert.Throws<ArgumentException>(() => Utils.ConvertRange(rangeStart, rangeEnd, newRangeStart, newRangeEnd, value));
        }

        [Test]
        public void ConvertRangeFloatTest()
        {
            float rangeStart = 0;
            float rangeEnd = 1;
            float newRangeStart = 0;
            float newRangeEnd = 100;
            float value = 0.50f;
            float expectedResult = 50;

            Assert.AreEqual(expectedResult, Utils.ConvertRange(rangeStart, rangeEnd, newRangeStart, newRangeEnd, value));
        }
    }
}
