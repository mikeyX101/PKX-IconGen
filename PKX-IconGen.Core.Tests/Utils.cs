#region License
/*  PKX-IconGen.Core.Tests - PKX-IconGen Core Unit Tests
    Copyright (C) 2021-2022 Samuel Caron/mikeyX#4697

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
*/
#endregion

using System;
using NUnit.Framework;

namespace PKXIconGen.Core.Tests
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
