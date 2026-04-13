using System.Collections.Generic;
using NUnit.Framework;

namespace USerializerTests;

public class DictionarySerializeTests
{
    [Test]
    public void SerializeIntStringDictionary()
    {
        var dict = new Dictionary<int, string>
        {
            { 1, "One" },
            { 2, "Two" },
            { 3, "Three" }
        };

        var result = TestUtils.SerializeDeserializeTest(dict);

        Assert.NotNull(result);
        Assert.AreEqual(dict.Count, result.Count);
        foreach (var key in dict.Keys)
        {
            Assert.IsTrue(result.ContainsKey(key));
            Assert.AreEqual(dict[key], result[key]);
        }
    }

    [Test]
    public void SerializeStringIntDictionary()
    {
        var dict = new Dictionary<string, int>
        {
            { "One", 1 },
            { "Two", 2 },
            { "Three", 3 }
        };

        var result = TestUtils.SerializeDeserializeTest(dict);

        Assert.NotNull(result);
        Assert.AreEqual(dict.Count, result.Count);
        foreach (var key in dict.Keys)
        {
            Assert.IsTrue(result.ContainsKey(key));
            Assert.AreEqual(dict[key], result[key]);
        }
    }

    [Test]
    public void SerializeEmptyDictionary()
    {
        var dict = new Dictionary<int, int>();

        var result = TestUtils.SerializeDeserializeTest(dict);

        Assert.NotNull(result);
        Assert.AreEqual(0, result.Count);
    }
    
    [Test]
    public void SerializeNestedDictionary()
    {
        var dict = new Dictionary<int, Dictionary<string, int>>
        {
            { 1, new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
            { 2, new Dictionary<string, int> { { "c", 3 } } }
        };

        var result = TestUtils.SerializeDeserializeTest(dict);

        Assert.NotNull(result);
        Assert.AreEqual(dict.Count, result.Count);
        Assert.AreEqual(dict[1].Count, result[1].Count);
        Assert.AreEqual(dict[1]["a"], result[1]["a"]);
        Assert.AreEqual(dict[2]["c"], result[2]["c"]);
    }
}