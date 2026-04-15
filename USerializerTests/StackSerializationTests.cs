using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace USerializerTests;

public class StackSerializationTests
{
    [Test]
    public void Stack_Int_SerializationWorks()
    {
        var initial = new Stack<int>();
        initial.Push(1);
        initial.Push(2);
        initial.Push(3);
        
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Count, Is.EqualTo(initial.Count));
        
        var initialArray = initial.ToArray();
        var resultArray = result.ToArray();
        
        Assert.That(resultArray, Is.EqualTo(initialArray));
    }

    [Test]
    public void Stack_String_SerializationWorks()
    {
        var initial = new Stack<string>();
        initial.Push("First");
        initial.Push("Second");
        initial.Push(null);
        initial.Push("Third");
        
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Count, Is.EqualTo(initial.Count));
        Assert.That(result.ToArray(), Is.EqualTo(initial.ToArray()));
    }

    [Test]
    public void Stack_Mixed_ReuseWorks()
    {
        var initial = new Stack<int>();
        initial.Push(1);
        initial.Push(2);

        var result = TestUtils.SerializeDeserializeTest(initial);
        Assert.That(result.ToArray(), Is.EqualTo(new[] { 2, 1 }));

        // Reuse stack with more elements
        result.Push(3);
        result.Push(4);
        result.Push(5);
        
        var initial2 = new Stack<int>();
        initial2.Push(10);
        initial2.Push(20);
        initial2.Push(30);

        // Serialize initial2 into result (which has 5 elements now)
        var memStream = new System.IO.MemoryStream();
        BinaryUtility.Serialize(initial2, memStream);
        memStream.Position = 0;
        
        BinaryUtility.TryDeserialize(memStream, ref result);
        
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.ToArray(), Is.EqualTo(new[] { 30, 20, 10 }));
    }

    [Test]
    public void Stack_Empty_SerializationWorks()
    {
        var initial = new Stack<int>();
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public void Stack_Null_SerializationThrows()
    {
        Stack<int> initial = null;
        var memStream = new System.IO.MemoryStream();
        Assert.Throws<ArgumentNullException>(() => BinaryUtility.Serialize(initial, memStream));
    }
}
