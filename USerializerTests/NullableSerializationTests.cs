using System;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    [TestFixture]
    public class NullableSerializationTests
    {
        public struct SimpleStruct
        {
            public int Value;
            public string Name;

            public override bool Equals(object obj)
            {
                if (obj is SimpleStruct other)
                {
                    return Value == other.Value && Name == other.Name;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Value, Name);
            }
        }

        public enum TestEnum
        {
            Value1,
            Value2
        }

        [Serializable]
        public class ClassWithNullable
        {
            public int? NullableInt;
            public SimpleStruct? NullableStruct;
            public TestEnum? NullableEnum;

            public override bool Equals(object obj)
            {
                if (obj is ClassWithNullable other)
                {
                    return NullableInt == other.NullableInt &&
                           Nullable.Equals(NullableStruct, other.NullableStruct) &&
                           NullableEnum == other.NullableEnum;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(NullableInt, NullableStruct, NullableEnum);
            }
        }

        [Test]
        public void NullableIntSerialization()
        {
            int? input = 5;
            int? result = TestUtils.SerializeDeserializeStructTestInternal(input);
            Assert.AreEqual(input, result);

            input = null;
            result = TestUtils.SerializeDeserializeStructTestInternal(input);
            Assert.AreEqual(input, result);
        }

        [Test]
        public void NullableBoolSerialization()
        {
            bool? input = true;
            bool? result = TestUtils.SerializeDeserializeStructTestInternal(input);
            Assert.AreEqual(input, result);

            input = null;
            result = TestUtils.SerializeDeserializeStructTestInternal(input);
            Assert.AreEqual(input, result);
        }

        [Test]
        public void NullableEnumSerialization()
        {
            TestEnum? input = TestEnum.Value2;
            TestEnum? result = TestUtils.SerializeDeserializeStructTestInternal(input);
            Assert.AreEqual(input, result);

            input = null;
            result = TestUtils.SerializeDeserializeStructTestInternal(input);
            Assert.AreEqual(input, result);
        }

        [Test]
        public void NullableStructSerialization()
        {
            SimpleStruct? input = new SimpleStruct { Value = 42, Name = "Test" };
            SimpleStruct? result = TestUtils.SerializeDeserializeStructTestInternal(input);
            Assert.AreEqual(input, result);

            input = null;
            result = TestUtils.SerializeDeserializeStructTestInternal(input);
            Assert.AreEqual(input, result);
        }

        [Test]
        public void ClassWithNullableSerialization()
        {
            var input = new ClassWithNullable
            {
                NullableInt = 10,
                NullableStruct = new SimpleStruct { Value = 1, Name = "Nested" },
                NullableEnum = TestEnum.Value1
            };
            var result = TestUtils.SerializeDeserializeTest(input);
            Assert.AreEqual(input, result);

            input = new ClassWithNullable
            {
                NullableInt = null,
                NullableStruct = null,
                NullableEnum = null
            };
            result = TestUtils.SerializeDeserializeTest(input);
            Assert.AreEqual(input, result);
        }
        
        //check if null values are serialized correctly
        [Test]
        public void NullValuesSerialization()
        {
            var input = new ClassWithNullable
            {
                NullableInt = null,
                NullableStruct = null,
                NullableEnum = null
            };
            var result = TestUtils.SerializeDeserializeTest(input);
            Assert.AreEqual(input.NullableEnum, result.NullableEnum);
            Assert.AreEqual(input.NullableInt, result.NullableInt);
            Assert.AreEqual(input.NullableStruct, result.NullableStruct);
        }
    }
}
