using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace Tests
{
    public class StructSerializationTests
    {
        [Serializable]
        public struct SimpleStruct
        {
            public int IntValue;
            public float FloatValue;
            public bool BoolValue;
            public string StringValue;
            public string[] Strings;
            public ToBeReferenced Reference;
        }

        [Serializable]
        public class ToBeReferenced
        {
            public int IntValue;
            public float FloatValue;
            public bool BoolValue;
            public string StringValue;
            public string[] Strings;
        }

        [Test]
        public unsafe void SimpleStructSerialization()
        {
            var simpleStruct = new SimpleStruct()
            {
                IntValue = 123,
                FloatValue = 11.1f,
                BoolValue = true,
                StringValue = "Wtfaaaaaaaaaaaaaaaaaa",
                Strings = new string[]
                {
                    "one",
                    "two",
                    "three"
                }
            };

            TestUtils.SerializeDeserializeTest(simpleStruct);
        }


        [Test]
        public void NestedStructSerialization()
        {
            TestUtils.SerializeDeserializeTest(new SimpleStruct()
            {
                IntValue = 123,
                FloatValue = 11.1f,
                BoolValue = true,
                Reference = new ToBeReferenced()
                {
                    IntValue = 1,
                    StringValue = "uuu",
                    Strings = new string[]
                    {
                        "one",
                        "two",
                        "three"
                    }
                }
            });
        }

        [Test]
        public void SimpleStructArraySerialization()
        {
            var a = new SimpleStruct()
            {
                IntValue = 123,
                FloatValue = 11.1f,
                BoolValue = true
            };
            var b = new SimpleStruct()
            {
                IntValue = 222,
                FloatValue = 11.1f,
                BoolValue = true,
                Strings = new string[]
                {
                    "one",
                    "two",
                    "three"
                }
            };
            TestUtils.SerializeDeserializeTest(new SimpleStruct[]
            {
                a,
                b
            });
        }

        [Test]
        public void SimpleStructListSerialization()
        {
            var a = new SimpleStruct()
            {
                IntValue = 123,
                FloatValue = 11.1f,
                BoolValue = true
            };
            var b = new SimpleStruct()
            {
                IntValue = 222,
                FloatValue = 11.1f,
                BoolValue = true
            };
            TestUtils.SerializeDeserializeTest(new List<SimpleStruct>
            {
                a,
                b
            });
        }
    }
}