using System;
using NUnit.Framework;

namespace USerializerTests;

public class GenericSerializationTests
{
    [Serializable]
    public class MyGenericClass<T>
    {
        public T Value;
    }
    
    [Test()]
    [TestCase("string")]
    [TestCase((double)123)]
    [TestCase((int)123)]
    [TestCase((float)123)]
    [TestCase((byte)123)]
    [TestCase((char)123)]
    public void GenericSerializationWorks<T>(T instance)
    {
        var initial = new MyGenericClass<T>()
        {
            Value = instance
        };

        var result = TestUtils.SerializeDeserializeTest(initial);
        Assert.That(initial.Value, Is.EqualTo(result.Value));
    }
}