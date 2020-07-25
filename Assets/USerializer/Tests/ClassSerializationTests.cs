using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests
{
    public class ClassSerializationTests
    {
        [Serializable]
        public class SimpleClass
        {
            public int IntValue;
            public float FloatValue;
            public bool BoolValue;
            public string StringValue;
            public string[] Strings;
            public SimpleClass Reference;
        }

        [Test]
        public void SimpleClassSerialization()
        {
            TestUtils.SerializeDeserializeTest(new SimpleClass()
            {
                IntValue = 123,
                FloatValue = 11.1f,
                BoolValue = true,
                StringValue = "Wtf",
                Strings = new string[]
                {
                    "one",
                    "two",
                    "three"
                }
            });
        }


        [Test]
        public void NestedClassSerialization()
        {
            TestUtils.SerializeDeserializeTest(new SimpleClass()
            {
                IntValue = 123,
                FloatValue = 11.1f,
                BoolValue = true,
                Reference = new SimpleClass()
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
        public void SimpleClassArraySerialization()
        {
            var a = new SimpleClass()
            {
                IntValue = 123,
                FloatValue = 11.1f,
                BoolValue = true,
                StringValue = "XD"
            };
            var b = new SimpleClass()
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
            TestUtils.SerializeDeserializeTest(new SimpleClass[]
            {
                a,
                b
            });
        }

        [Test]
        public void SimpleClassListSerialization()
        {
            var a = new SimpleClass()
            {
                IntValue = 123,
                FloatValue = 11.1f,
                BoolValue = true
            };
            var b = new SimpleClass()
            {
                IntValue = 222,
                FloatValue = 11.1f,
                BoolValue = true
            };
            TestUtils.SerializeDeserializeTest(new List<SimpleClass>
            {
                a,
                b
            });
        }
    }
}