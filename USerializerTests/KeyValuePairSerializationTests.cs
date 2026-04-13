using System.Collections.Generic;
using NUnit.Framework;

namespace USerializerTests;

public class KeyValuePairSerializationTests
{
    [Test]
    public void KeyValuePair_IntString_SerializationWorks()
    {
        var initial = new KeyValuePair<int, string>(123, "Hello World");
        var result = TestUtils.SerializeDeserializeStructTest(initial);
        
        Assert.That(result.Key, Is.EqualTo(initial.Key));
        Assert.That(result.Value, Is.EqualTo(initial.Value));
    }

    [Test]
    public void KeyValuePair_StringInt_SerializationWorks()
    {
        var initial = new KeyValuePair<string, int>("Key", 456);
        var result = TestUtils.SerializeDeserializeStructTest(initial);
        
        Assert.That(result.Key, Is.EqualTo(initial.Key));
        Assert.That(result.Value, Is.EqualTo(initial.Value));
    }

    [Test]
    public void KeyValuePair_InClass_SerializationWorks()
    {
        var initial = new KvpContainer
        {
            Pair = new KeyValuePair<int, string>(789, "Value")
        };
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Pair.Key, Is.EqualTo(initial.Pair.Key));
        Assert.That(result.Pair.Value, Is.EqualTo(initial.Pair.Value));
    }

    [System.Serializable]
    public class KvpContainer
    {
        public KeyValuePair<int, string> Pair;
    }
}