using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    public class NETRuntimeUtilsTests
    {
        [Test]
        public unsafe void GetFieldOffsetWorkForClass()
        {
            var utils = new NETRuntimeUtils();

            var instance = new TestClass()
            {
                IntField = 123
            };

            var classField = instance.GetType().GetField("IntField");
            var offset = utils.GetFieldOffset(classField);
            Assert.AreEqual(0, offset);

            ref var pinnable = ref Unsafe.As<TestClass, PinnableObject>(ref instance);
            
            fixed (byte* address = &pinnable.Pinnable)
            {
                var value = Unsafe.Read<int>(address + offset);
                Assert.AreEqual(123, value);
            }
        }

        [Test]
        public unsafe void GetFieldOffsetWorkForStruct()
        {
            var utils = new NETRuntimeUtils();

            var instance = new TestStruct()
            {
                IntField = 123
            };

            var classField = instance.GetType().GetField("IntField");
            var offset = utils.GetFieldOffset(classField);
            Assert.AreEqual(0, offset);
            
            var address = (byte*)Unsafe.AsPointer(ref instance);
            var value = Unsafe.Read<int>(address + offset);
            Console.WriteLine(value);
            Assert.AreEqual(123, value);
        }

        private class TestClass
        {
            public int IntField;
        }

        private struct TestStruct
        {
            public int IntField;
        }
    }
}