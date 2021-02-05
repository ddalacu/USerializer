using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

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
            var initial = new SimpleStruct()
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

            var result = TestUtils.SerializeDeserializeTest(initial);

            Debug.Assert(JsonUtility.ToJson(initial) == JsonUtility.ToJson(result));
        }


        [Test]
        public void NestedStructSerialization()
        {
            var initial = new SimpleStruct()
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
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            Debug.Assert(JsonUtility.ToJson(initial) == JsonUtility.ToJson(result));
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
            var initial = new SimpleStruct[]
            {
                a,
                b
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            //Debug.Log(TestUtils.UnitySerializeArray(initial));
            //Debug.Log(TestUtils.UnitySerializeArray(result));


            Debug.Assert(TestUtils.CompareSerializedContents(initial, result));
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
            var initial = new List<SimpleStruct>
            {
                a,
                b
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            Debug.Assert(TestUtils.CompareSerializedContents(initial, result));
        }

        [Serializable]
        public class ClassWithDateTime
        {
            public DateTime Time;
        }

        [Test]
        public void DateTimeSerialization()
        {
            var initial = new ClassWithDateTime()
            {
                Time = DateTime.Now
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            Debug.Assert(result.Time == initial.Time);
        }

    }
}