﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
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
        public class SimpleClass : IEquatable<SimpleClass>
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

            public bool Equals(SimpleClass other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return IntValue == other.IntValue &&
                       FloatValue.Equals(other.FloatValue) &&
                       BoolValue == other.BoolValue &&
                       StringValue == other.StringValue &&
                       TestUtils.EqualArrays(Strings, other.Strings) &&
                       EqualityComparer<SimpleClass>.Default.Equals(Reference, other.Reference) &&
                       EnumValue == other.EnumValue &&
                       TestUtils.EqualArrays(EnumArray, other.EnumArray);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SimpleClass)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(IntValue, FloatValue, BoolValue, StringValue, Strings, Reference, (int)EnumValue, EnumArray);
            }

            public static bool operator ==(SimpleClass left, SimpleClass right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(SimpleClass left, SimpleClass right)
            {
                return !Equals(left, right);
            }
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
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            EqualityComparer<SimpleClass>.Default.Equals(initial, result);
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

            EqualityComparer<SimpleClass>.Default.Equals(initial, result);
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

            Assert.True(TestUtils.EqualArrays(initial, result));
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

            Assert.True(TestUtils.EqualArrays(initial, result));
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

            Assert.True(class1.IntValue == class2.Form);
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
            Assert.True(stream.Position == end);

            Assert.True(class1.IntValue == class2.IntValue);
        }


        [Serializable]
        public class NestedCustom : IEquatable<NestedCustom>
        {
            public ExampleClass WithValue = new ExampleClass();
            public ExampleClass NullValue;
            public int Val;

            public bool Equals(NestedCustom other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return EqualityComparer<ExampleClass>.Default.Equals(WithValue, other.WithValue) &&
                       EqualityComparer<ExampleClass>.Default.Equals(NullValue, other.NullValue) &&
                       Val == other.Val;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((NestedCustom)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(WithValue, NullValue, Val);
            }

            public static bool operator ==(NestedCustom left, NestedCustom right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(NestedCustom left, NestedCustom right)
            {
                return !Equals(left, right);
            }
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

            Assert.True(result.Value == inst.Value);
            Assert.True(result.Ref.Value == inst.Ref.Value);
            Assert.True(result.Ref.Ref.Value == inst.Ref.Ref.Value);
            Assert.True(result.Ref.Ref.Ref == null);
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
            Assert.True(deser);
            Assert.True(EqualityComparer<ExampleClass.ChildClass>.Default.Equals(exampleClass.Property, result.Property));
            Assert.True(EqualityComparer<ExampleClass.ChildStruct>.Default.Equals(exampleClass.ValueProp, result.ValueProp));
            Assert.True(exampleClass.Val == result.Val);
            Assert.True(exampleClass.Val2 == result.Val2);
            Assert.True(exampleClass.Prop1 == result.Prop1);

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
            Assert.True(deser);

            Assert.True(EqualityComparer<NestedCustom>.Default.Equals(nested, nestedResult));
        }

        [Serializable]
        private class CircularClass
        {
            public CircularClass Other;
        }

        [Test]
        public void CircularReferences()
        {
            var a = new CircularClass();
            var b = new CircularClass();

            a.Other = b;
            b.Other = a;

            Assert.Throws<CircularReferenceException>(() =>
            {
                var stream = new MemoryStream();
                var serialized = BinaryUtility.Serialize(a, stream);
            });
        }

    }
}
