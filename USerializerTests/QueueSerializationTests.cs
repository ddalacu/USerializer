using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace USerializerTests;

public class QueueSerializationTests
{
    [Test]
    public void Queue_Int_SerializationWorks()
    {
        var initial = new Queue<int>();
        initial.Enqueue(1);
        initial.Enqueue(2);
        initial.Enqueue(3);
        
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Count, Is.EqualTo(initial.Count));
        Assert.That(result.ToArray(), Is.EqualTo(initial.ToArray()));
    }

    [Test]
    public void Queue_String_SerializationWorks()
    {
        var initial = new Queue<string>();
        initial.Enqueue("First");
        initial.Enqueue("Second");
        initial.Enqueue(null);
        initial.Enqueue("Third");
        
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Count, Is.EqualTo(initial.Count));
        Assert.That(result.ToArray(), Is.EqualTo(initial.ToArray()));
    }

    [Test]
    public void Queue_Circular_SerializationWorks()
    {
        var initial = new Queue<int>(4);
        initial.Enqueue(1);
        initial.Enqueue(2);
        initial.Dequeue(); // head=1, tail=2, size=1
        initial.Enqueue(3);
        initial.Enqueue(4);
        initial.Enqueue(5); // tail should wrap around or at least head is not 0
        
        // At this point, depending on implementation, it might have wrapped.
        // Queue<int>(4) might allocate array of 4.
        // Enqueue 1, 2 -> [1, 2, 0, 0], head=0, tail=2, size=2
        // Dequeue -> [0, 2, 0, 0], head=1, tail=2, size=1
        // Enqueue 3, 4, 5 -> [5, 2, 3, 4], head=1, tail=1, size=4 (wrapped)
        
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Count, Is.EqualTo(initial.Count));
        Assert.That(result.ToArray(), Is.EqualTo(initial.ToArray()));
    }

    [Test]
    public void Queue_Mixed_ReuseWorks()
    {
        var initial = new Queue<int>();
        initial.Enqueue(1);
        initial.Enqueue(2);

        var result = TestUtils.SerializeDeserializeTest(initial);
        Assert.That(result.ToArray(), Is.EqualTo(new[] { 1, 2 }));

        // Reuse queue with more elements
        result.Enqueue(3);
        result.Enqueue(4);
        result.Enqueue(5);
        
        var initial2 = new Queue<int>();
        initial2.Enqueue(10);
        initial2.Enqueue(20);
        initial2.Enqueue(30);

        // Serialize initial2 into result (which has 5 elements now)
        var memStream = new MemoryStream();
        BinaryUtility.Serialize(initial2, memStream);
        memStream.Position = 0;
        
        BinaryUtility.TryDeserialize(memStream, ref result);
        
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.ToArray(), Is.EqualTo(new[] { 10, 20, 30 }));
    }

    [Test]
    public void Queue_Empty_SerializationWorks()
    {
        var initial = new Queue<int>();
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Count, Is.EqualTo(0));
    }
}
