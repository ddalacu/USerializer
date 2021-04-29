using System;
using System.Collections.Generic;
using NUnit.Framework;
using USerialization;

[assembly: CustomSerializer(typeof(USerializerTests.CustomStructSerializer))]


namespace USerializerTests
{
    public struct CustomStruct : IEquatable<CustomStruct>
    {
        public int Field;
        public int Property { get; set; }
        public int Property2 { get; set; }

        public bool Equals(CustomStruct other)
        {
            return Field == other.Field && Property == other.Property && Property2 == other.Property2;
        }

        public override bool Equals(object obj)
        {
            return obj is CustomStruct other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field, Property, Property2);
        }

        public static bool operator ==(CustomStruct left, CustomStruct right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CustomStruct left, CustomStruct right)
        {
            return !left.Equals(right);
        }
    }

    public class CustomStructSerializer : CustomStructSerializer<CustomStruct>
    {
        public override void LocalInit(StructMemberAdder<CustomStruct> adder)
        {
            adder.AddField(0, nameof(CustomStruct.Field));

            adder.AddField(1, (ref CustomStruct obj, int val) => obj.Property = val, (ref CustomStruct obj) => obj.Property);

            adder.AddBackingField(2, nameof(CustomStruct.Property2));

        }
    }

    public class StructSerializationTests
    {
        [Serializable]
        public struct SimpleStruct : IEqualityComparer<SimpleStruct>, IEquatable<SimpleStruct>
        {
            public int IntValue;
            public float FloatValue;
            public bool BoolValue;
            public string StringValue;
            public string[] Strings;
            public ToBeReferenced Reference;



            public bool Equals(SimpleStruct x, SimpleStruct y)
            {
                return x.IntValue == y.IntValue &&
                       x.FloatValue.Equals(y.FloatValue) &&
                       x.BoolValue == y.BoolValue &&
                       x.StringValue == y.StringValue &&
                       TestUtils.EqualArrays(x.Strings, y.Strings)
                       && EqualityComparer<ToBeReferenced>.Default.Equals(x.Reference, y.Reference);
            }

            public int GetHashCode(SimpleStruct obj)
            {
                return HashCode.Combine(obj.IntValue, obj.FloatValue, obj.BoolValue, obj.StringValue, obj.Strings, obj.Reference);
            }

            public bool Equals(SimpleStruct other)
            {
                return IntValue == other.IntValue &&
                       FloatValue.Equals(other.FloatValue) &&
                       BoolValue == other.BoolValue &&
                       StringValue == other.StringValue &&
                       TestUtils.EqualArrays(Strings, other.Strings)
                       && EqualityComparer<ToBeReferenced>.Default.Equals(Reference, other.Reference);
            }

            public override bool Equals(object obj)
            {
                return obj is SimpleStruct other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(IntValue, FloatValue, BoolValue, StringValue, Strings, Reference);
            }

            public static bool operator ==(SimpleStruct left, SimpleStruct right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(SimpleStruct left, SimpleStruct right)
            {
                return !left.Equals(right);
            }
        }

        [Serializable]
        public class ToBeReferenced : IEquatable<ToBeReferenced>
        {
            public int IntValue;
            public float FloatValue;
            public bool BoolValue;
            public string StringValue;
            public string[] Strings;


            public static bool Equals(ToBeReferenced x, ToBeReferenced y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                return x.IntValue == y.IntValue &&
                       x.FloatValue.Equals(y.FloatValue) &&
                       x.BoolValue == y.BoolValue &&
                       x.StringValue == y.StringValue &&
                       TestUtils.EqualArrays(x.Strings, y.Strings);
            }

            public bool Equals(ToBeReferenced other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;

                return IntValue == other.IntValue &&
                       FloatValue.Equals(other.FloatValue) &&
                       BoolValue == other.BoolValue &&
                       StringValue == other.StringValue &&
                       TestUtils.EqualArrays(Strings, other.Strings);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ToBeReferenced) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(IntValue, FloatValue, BoolValue, StringValue, Strings);
            }

            public static bool operator ==(ToBeReferenced left, ToBeReferenced right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(ToBeReferenced left, ToBeReferenced right)
            {
                return !Equals(left, right);
            }
        }

        [Test]
        public void SimpleStructSerialization()
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

            Assert.True(EqualityComparer<SimpleStruct>.Default.Equals(initial, result));
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
                    BoolValue = true,
                    FloatValue = 112,
                    Strings = new string[]
                    {
                        "one",
                        "two",
                        "three"
                    }
                }
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            Assert.True(EqualityComparer<SimpleStruct>.Default.Equals(initial, result));
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

            Assert.True(TestUtils.EqualArrays(initial, result));
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

            Assert.True(TestUtils.EqualArrays(initial, result));
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

            Assert.True(result.Time == initial.Time);
        }


        [Test]
        public void CustomStructSerialization()
        {
            var initial = new CustomStruct()
            {
                Field = 123,
                Property = 112,
                Property2 = 221
            };
            var result = TestUtils.SerializeDeserializeTest(initial);

            Assert.True(EqualityComparer<CustomStruct>.Default.Equals(initial, result));
        }

    }
}
