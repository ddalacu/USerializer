using System;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    public class ArrayUtilsTests
    {
        [Test]
        public void CleanUnmanagedArray()
        {
            var array = new int[]
            {
                1,
                2,
                3,
                4
            };

            ArrayHelpers.CleanArray(array, 1, 2);

            Assert.AreEqual(array[0], 1);
            Assert.AreEqual(array[1], 0);
            Assert.AreEqual(array[2], 0);
            Assert.AreEqual(array[3], 4);
        }

        [Test]
        public unsafe void CleanArray()
        {
            var array = new string[]
            {
                "a",
                "b",
                "c",
                "d"
            };

            ArrayHelpers.CleanArray(array, 1, 2, (uint) sizeof(IntPtr));

            Assert.AreEqual(array[0], "a");
            Assert.AreEqual(array[1], null);
            Assert.AreEqual(array[2], null);
            Assert.AreEqual(array[3], "d");
        }
    }
}