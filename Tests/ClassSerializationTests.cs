using System;
using System.Collections.Generic;
using System.IO;
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

        public int Val2;

        public int Prop1 { get; set; }

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


    public class ExampleClassSerializer : CustomClassSerializer<ExampleClass>
    {
        public override void LocalInit(ClassMemberAdder<ExampleClass> adder)
        {
            adder.AddField(1, (obj, val) => obj.Val = val, obj => obj.Val);

            adder.AddField(2, nameof(ExampleClass.Val2));

            adder.AddField(3, (obj, val) => obj.Property = val, obj => obj.Property);
            adder.AddField(4, (obj, val) => obj.ValueProp = val, obj => obj.ValueProp);

            adder.AddBackingField(5, nameof(ExampleClass.Prop1));
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

            public Dictionary<int, int> IntDictionary = new Dictionary<int, int>();
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
                },
                IntDictionary = new Dictionary<int, int>()
                {
                    {1,2 },
                    {2,2 }
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

            Debug.Assert(TestUtils.CompareSerializedContents(initial, result));
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

            Debug.Assert(TestUtils.CompareSerializedContents(initial, result));
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
            public string Asd = "wtf";
            public int IntValue2;
            public string IntValue1 = "wtt";
            public string Test;
        }

        [Serializable]
        public class SkipFieldClass2
        {
            public int IntValue;
            public byte IntValue2;
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

            var end = stream.Position;
            stream.Position = 0;

            BinaryUtility.TryDeserialize(stream, out SkipFieldClass2 class2);
            Debug.Assert(stream.Position == end);

            Debug.Assert(class1.IntValue == class2.IntValue);
        }


        [Serializable]
        public class NestedCustom
        {
            public ExampleClass WithValue = new ExampleClass();
            public ExampleClass NullValue;
            public int Val;
        }

        [Serializable]
        public class A
        {
            public B Ref;
            public int Value = 0;
        }

        [Serializable]
        public class B
        {
            public A Ref;
            public int Value = 0;
        }

        [Test]
        public void CyclicTypeDependency()
        {
            //make sure when we have cyclical type dependencies it all works

            var inst = new A()
            {
                Value = 1,
                Ref = new B()
                {
                    Value = 2,
                    Ref = new A()
                    {
                        Value = 3
                    }
                }
            };

            var stream = new MemoryStream();
            BinaryUtility.Serialize(inst, stream);
            stream.Position = 0;
            BinaryUtility.TryDeserialize(stream, out A result);

            Debug.Assert(result.Value == inst.Value);
            Debug.Assert(result.Ref.Value == inst.Ref.Value);
            Debug.Assert(result.Ref.Ref.Value == inst.Ref.Ref.Value);
            Debug.Assert(result.Ref.Ref.Ref == null);
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
                Val = -112,
                Val2 = 112,
                Prop1 = 223
            };

            var memStream = new MemoryStream();
            BinaryUtility.Serialize(exampleClass, memStream);



            memStream.Position = 0;
            var deser = BinaryUtility.TryDeserialize(memStream, out ExampleClass result);
            Debug.Assert(deser);

            Debug.Assert(JsonUtility.ToJson(exampleClass.Property) == JsonUtility.ToJson(result.Property));
            Debug.Assert(JsonUtility.ToJson(exampleClass.ValueProp) == JsonUtility.ToJson(result.ValueProp));
            Debug.Assert(exampleClass.Val == result.Val);
            Debug.Assert(exampleClass.Val2 == result.Val2);
            Debug.Assert(exampleClass.Prop1 == result.Prop1);

            var nested = new NestedCustom()
            {
                Val = 112,
                WithValue = new ExampleClass()
                {
                    Val = 999
                }
            };

            memStream = new MemoryStream();
            BinaryUtility.Serialize(nested, memStream);

            memStream.Position = 0;
            deser = BinaryUtility.TryDeserialize(memStream, out NestedCustom nestedResult);
            Debug.Assert(deser);

            Debug.Assert(JsonUtility.ToJson(nested) == JsonUtility.ToJson(nestedResult));
        }

    }
}