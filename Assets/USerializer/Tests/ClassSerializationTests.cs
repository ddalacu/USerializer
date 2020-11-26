using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tests
{
    public class ClassSerializationTests
    {
        public enum TestEnum
        {
            One,
            Two,
            Three
        }

        [Serializable]
        public class SimpleClass
        {
            [FormerlySerializedAs("IntValue")]
            public int IntValue;

            public float FloatValue;
            public bool BoolValue;

            public string StringValue;

            public string[] Strings;
            public SimpleClass Reference;
            public TestEnum EnumValue;
            public TestEnum[] EnumArray;
        }

        [Test]
        public void SimpleClassSerialization()
        {
            var initial = new SimpleClass()
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
                },
                EnumValue = TestEnum.One,
                EnumArray = new TestEnum[]
                {
                    TestEnum.One,
                    TestEnum.Two
                }
            };
            var result = TestUtils.SerializeDeserializeTest(initial);
            Debug.Assert(JsonUtility.ToJson(initial) == JsonUtility.ToJson(result));
        }


        [Test]
        public void NestedClassSerialization()
        {
            var initial = new SimpleClass()
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
                    },
                    EnumValue = TestEnum.One,
                    EnumArray = new TestEnum[]
                    {
                        TestEnum.One,
                        TestEnum.Two
                    }
                }
            };

            var result = TestUtils.SerializeDeserializeTest(initial);

            Debug.Assert(JsonUtility.ToJson(initial) == JsonUtility.ToJson(result));
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
                },
                EnumValue = TestEnum.One,
                EnumArray = new TestEnum[]
                {
                    TestEnum.One,
                    TestEnum.Two
                }
            };
            var initial = new SimpleClass[]
            {
                a,
                b
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            //Debug.Log(UnitySerializeArray(initial));
            //Debug.Log(UnitySerializeArray(result));

            Debug.Assert(TestUtils.UnitySerializeArray(initial) == TestUtils.UnitySerializeArray(result));
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
                BoolValue = true,
                EnumValue = TestEnum.One,
                EnumArray = new TestEnum[]
                {
                    TestEnum.One,
                    TestEnum.Two
                }
            };

            var initial = new List<SimpleClass>
            {
                a,
                b
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            //Debug.Log(UnitySerializeArray(initial));
            //Debug.Log(UnitySerializeArray(result));

            Debug.Assert(TestUtils.UnitySerializeArray(initial) == TestUtils.UnitySerializeArray(result));
        }
    }
}