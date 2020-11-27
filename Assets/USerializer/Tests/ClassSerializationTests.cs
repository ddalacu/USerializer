using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.Serialization;
using USerialization;

[assembly: CustomSerializer(typeof(ExampleClassSerializer))]

namespace Tests
{

    [Serializable]
    public class ExampleClass
    {
        public ChildClass Property { get; set; }

        public ChildStruct ValueProp { get; set; }

        public int Val;

        [Serializable]
        public struct ChildStruct
        {
            public int Val1;
            public int Val2;
        }

        [Serializable]
        public class ChildClass
        {
            public float ValueOne;
            public float ValueTwo;
        }
    }


    public class ExampleClassSerializer : CustomSerializerBase<ExampleClass>
    {
        public override void LocalInit()
        {
            AddField(3, (ref ExampleClass obj, int val) =>
            {
                obj.Val = val;
            }, (ref ExampleClass obj) =>
            {
                return obj.Val;
            });

            AddField(1, (ref ExampleClass obj, ExampleClass.ChildClass val) =>
            {
                obj.Property = val;
            }, (ref ExampleClass obj) =>
            {
                return obj.Property;
            });

            AddField(2, (ref ExampleClass obj, ExampleClass.ChildStruct val) =>
            {
                obj.ValueProp = val;
            }, (ref ExampleClass obj) =>
            {
                return obj.ValueProp;
            });
        }
    }


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

        [Serializable]
        public class FormerlyClass1
        {
            public int IntValue;
        }

        [Serializable]
        public class FormerlyClass2
        {
            [SerializeField]
            [FormerlySerializedAs("IntValue")]
            private int _changedForm;

            public int Form
            {
                get => _changedForm;
                set => _changedForm = value;
            }
        }

        [Test]
        public void FormerlySerializedAs()
        {
            var class1 = new FormerlyClass1()
            {
                IntValue = 1234
            };

            var stream = new MemoryStream();

            BinaryUtility.Serialize(class1, stream);

            stream.Position = 0;

            BinaryUtility.TryDeserialize(stream, out FormerlyClass2 class2);

            Debug.Assert(class1.IntValue == class2.Form);
        }

        [Serializable]
        public class SkipFieldClass1
        {
            public int IntValue;
            public int IntValue2;
            public string Test;
        }

        [Serializable]
        public class SkipFieldClass2
        {
            public int IntValue;
            public byte IntValue2;
            public string Test;
        }

        [Test]
        public void SkipFieldNoErrors()
        {
            var class1 = new SkipFieldClass1()
            {
                IntValue = 1234,
                IntValue2 = 223,
                Test = "Lol"
            };

            var stream = new MemoryStream();

            BinaryUtility.Serialize(class1, stream);

            stream.Position = 0;

            BinaryUtility.TryDeserialize(stream, out SkipFieldClass2 class2);

            Debug.Assert(class1.IntValue == class2.IntValue);
            Debug.Assert(class1.Test == class2.Test);

            Debug.Assert(class2.IntValue2 == 0);
        }


        [Serializable]
        public class NestedCustom
        {
            public ExampleClass WithValue = new ExampleClass();
            public ExampleClass NullValue;
            public int Val;
        }

        [Test]
        public void CustomSerializers()
        {
            var exampleClass = new ExampleClass()
            {
                Property = new ExampleClass.ChildClass()
                {
                    ValueOne = 1,
                    ValueTwo = 2
                },
                ValueProp = new ExampleClass.ChildStruct()
                {
                    Val1 = 1234,
                    Val2 = -1234
                },
                Val = -112
            };

            var memStream = new MemoryStream();
            BinaryUtility.Serialize(exampleClass, memStream);


            Debug.Log(JsonUtility.ToJson(exampleClass.Property));
            Debug.Log(JsonUtility.ToJson(exampleClass.ValueProp));
            Debug.Log(exampleClass.Val);

            memStream.Position = 0;
            var deser = BinaryUtility.TryDeserialize(memStream, out ExampleClass result);
            Debug.Assert(deser);

            Debug.Log(JsonUtility.ToJson(result.Property));
            Debug.Log(JsonUtility.ToJson(result.ValueProp));
            Debug.Log(result.Val);

            var nested = new NestedCustom()
            {
                Val = 112,
                WithValue =new ExampleClass()
                {
                    Val = 999
                }
            };

            memStream = new MemoryStream();
            BinaryUtility.Serialize(nested, memStream);

            memStream.Position = 0;
            deser = BinaryUtility.TryDeserialize(memStream, out NestedCustom nestedResult);
            Debug.Assert(deser);

            Debug.Log(JsonUtility.ToJson(nested));
            Debug.Log(JsonUtility.ToJson(nestedResult));

        }

    }
}