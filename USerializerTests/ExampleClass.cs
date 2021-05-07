using System;
using System.Collections.Generic;
using USerialization;
using USerializerTests;

[assembly: CustomSerializer(typeof(ExampleClass), typeof(ExampleClassSerializer))]

namespace USerializerTests
{
    public class ExampleClassSerializer : CustomClassSerializer<ExampleClass>
    {
        public override void LocalInit(ClassMemberAdder<ExampleClass> adder)
        {
            adder.AddField(1, (obj, val) => obj.Val = val, obj => obj.Val);
            adder.AddField(2, nameof(ExampleClass.Val2));
            adder.AddField(3, (obj, val) => obj.Property = val, obj => obj.Property);
            adder.AddField(4, (obj, val) => obj.ValueProp = val, obj => obj.ValueProp);
            adder.AddBackingField(5, nameof(ExampleClass.Prop1));

            adder.AddField(6, nameof(ExampleClass.Me));

        }
    }

    [Serializable]
    public class ExampleClass : IEquatable<ExampleClass>
    {
        public ExampleClass Me;

        public ChildClass Property { get; set; }

        public ChildStruct ValueProp { get; set; }

        public int Val;

        public int Val2;

        public int Prop1 { get; set; }

        [Serializable]
        public struct ChildStruct : IEquatable<ChildStruct>
        {
            public int Val1;
            public int Val2;

            public bool Equals(ChildStruct other)
            {
                return Val1 == other.Val1 && Val2 == other.Val2;
            }

            public override bool Equals(object obj)
            {
                return obj is ChildStruct other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Val1, Val2);
            }

            public static bool operator ==(ChildStruct left, ChildStruct right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(ChildStruct left, ChildStruct right)
            {
                return !left.Equals(right);
            }
        }

        [Serializable]
        public class ChildClass : IEquatable<ChildClass>
        {
            public float ValueOne;
            public float ValueTwo;

            public bool Equals(ChildClass other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return ValueOne.Equals(other.ValueOne) && ValueTwo.Equals(other.ValueTwo);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ChildClass)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ValueOne, ValueTwo);
            }

            public static bool operator ==(ChildClass left, ChildClass right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(ChildClass left, ChildClass right)
            {
                return !Equals(left, right);
            }
        }

        public bool Equals(ExampleClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Val == other.Val && Val2 == other.Val2 &&
                   EqualityComparer<ChildClass>.Default.Equals(Property, other.Property) &&
                   EqualityComparer<ChildStruct>.Default.Equals(ValueProp, other.ValueProp) &&
                   Prop1 == other.Prop1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExampleClass)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Val, Val2, Property, ValueProp, Prop1);
        }

        public static bool operator ==(ExampleClass left, ExampleClass right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExampleClass left, ExampleClass right)
        {
            return !Equals(left, right);
        }
    }
}