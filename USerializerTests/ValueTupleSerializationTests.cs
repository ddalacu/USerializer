using System;
using NUnit.Framework;

namespace USerializerTests;

public class ValueTupleSerializationTests
{
    [Test]
    public void ValueTuple2_IntString_SerializationWorks()
    {
        var initial = (123, "Hello World");
        var result = TestUtils.SerializeDeserializeStructTest(initial);
        
        Assert.That(result.Item1, Is.EqualTo(initial.Item1));
        Assert.That(result.Item2, Is.EqualTo(initial.Item2));
    }

    [Test]
    public void ValueTuple3_StringIntBool_SerializationWorks()
    {
        var initial = ("Key", 456, true);
        var result = TestUtils.SerializeDeserializeStructTest(initial);
        
        Assert.That(result.Item1, Is.EqualTo(initial.Item1));
        Assert.That(result.Item2, Is.EqualTo(initial.Item2));
        Assert.That(result.Item3, Is.EqualTo(initial.Item3));
    }

    [Test]
    public void ValueTuple4_LongFloatDoubleChar_SerializationWorks()
    {
        var initial = (123456789L, 1.23f, 4.56d, 'A');
        var result = TestUtils.SerializeDeserializeStructTest(initial);
        
        Assert.That(result.Item1, Is.EqualTo(initial.Item1));
        Assert.That(result.Item2, Is.EqualTo(initial.Item2));
        Assert.That(result.Item3, Is.EqualTo(initial.Item3));
        Assert.That(result.Item4, Is.EqualTo(initial.Item4));
    }

    [Test]
    public void ValueTuple7_Mixed_SerializationWorks()
    {
        var initial = (1, 2, 3, 4, 5, 6, 7);
        var result = TestUtils.SerializeDeserializeStructTest(initial);
        
        Assert.That(result.Item1, Is.EqualTo(initial.Item1));
        Assert.That(result.Item2, Is.EqualTo(initial.Item2));
        Assert.That(result.Item3, Is.EqualTo(initial.Item3));
        Assert.That(result.Item4, Is.EqualTo(initial.Item4));
        Assert.That(result.Item5, Is.EqualTo(initial.Item5));
        Assert.That(result.Item6, Is.EqualTo(initial.Item6));
        Assert.That(result.Item7, Is.EqualTo(initial.Item7));
    }

    [Test]
    public void ValueTuple_InClass_SerializationWorks()
    {
        var initial = new TupleContainer
        {
            T2 = (789, "Value"),
            T3 = ("Next", 10, false)
        };
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.T2.Item1, Is.EqualTo(initial.T2.Item1));
        Assert.That(result.T2.Item2, Is.EqualTo(initial.T2.Item2));
        Assert.That(result.T3.Item1, Is.EqualTo(initial.T3.Item1));
        Assert.That(result.T3.Item2, Is.EqualTo(initial.T3.Item2));
        Assert.That(result.T3.Item3, Is.EqualTo(initial.T3.Item3));
    }

    [Serializable]
    public class TupleContainer
    {
        public (int, string) T2;
        public (string, int, bool) T3;
    }

    [Test]
    public void Tuple2_IntString_SerializationWorks()
    {
        var initial = Tuple.Create(123, "Hello World");
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Item1, Is.EqualTo(initial.Item1));
        Assert.That(result.Item2, Is.EqualTo(initial.Item2));
    }
}
