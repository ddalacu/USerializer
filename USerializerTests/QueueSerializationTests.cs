using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using USerialization;

namespace USerializerTests;

public class QueueSerializationTests
{
    private void TestReuseBuffer<T>(params T[] elements)
    {
        // Create initial queue with some capacity
        var queue = new Queue<T>(elements.Length * 4);
        foreach (var e in elements)
        {
            queue.Enqueue(e);
        }

        var memStream = new MemoryStream();
        
        var queueType = typeof(Queue<T>);
        var itemsMember = queueType.GetField("_array", BindingFlags.Instance | BindingFlags.NonPublic);
        var sizeMember = queueType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);
        
        if (itemsMember == null || sizeMember == null)
            throw new InvalidOperationException("Could not find Queue internal fields.");

        var runtimeUtils = BinaryUtility.USerializer.RuntimeUtils;
        var itemsField = new FieldAccessHelper<object, Array>(itemsMember, runtimeUtils);
        var sizeField = new FieldAccessHelper<object, int>(sizeMember, runtimeUtils);

        object queueObj = queue;
        var arrayBefore = itemsField.GetFieldRef(ref queueObj);
        var sizeBefore = sizeField.GetFieldRef(ref queueObj);

        BinaryUtility.Serialize(queue, memStream);

        // Add some more elements to fill space so we can check if they are cleared
        for (int i = 0; i < elements.Length; i++)
        {
            queue.Enqueue(elements[i]);
        }

        memStream.Position = 0;
        BinaryUtility.TryDeserialize(memStream, ref queue);

        queueObj = queue;
        var arrayAfter = itemsField.GetFieldRef(ref queueObj);
        var sizeAfter = sizeField.GetFieldRef(ref queueObj);

        Assert.That(sizeAfter, Is.EqualTo(sizeBefore));
        Assert.That(arrayAfter, Is.SameAs(arrayBefore));
        
        // After deserialization, QueueSerializer sets head=0 and tail=count
        // and it should have cleared any existing items if the size changed or just to be safe.
        // QueueSerializer.Read clears currentSize elements starting from currentHead.
        
        var arrayAfterTyped = (T[])arrayAfter;
        for (int i = sizeAfter; i < arrayAfterTyped.Length; i++)
        {
            Assert.That(arrayAfterTyped[i], Is.EqualTo(default(T)), $"Element at index {i} should be cleared");
        }
    }

    [Test]
    [TestCase(default(sbyte))]
    [TestCase(default(byte))]
    [TestCase(default(short))]
    [TestCase(default(ushort))]
    [TestCase(default(int))]
    [TestCase(default(uint))]
    [TestCase(default(long))]
    [TestCase(default(ulong))]
    [TestCase(default(char))]
    [TestCase(default(float))]
    [TestCase(default(double))]
    [TestCase(default(bool))]
    public void Queue_ReusePrimitiveBuffer_Works<T>(T dummy)
    {
        var elements = new T[4];
        for (int i = 0; i < 4; i++)
        {
            elements[i] = (T)Convert.ChangeType(i + 1, typeof(T));
        }
        TestReuseBuffer(elements);
    }

    [Serializable]
    public class TestObj
    {
        public int Value;
    }

    [Test]
    public void Queue_ReuseClassBuffer_Works()
    {
        TestReuseBuffer(new TestObj { Value = 1 }, new TestObj { Value = 2 });
    }
    [Test]
    [TestCase(default(sbyte))]
    [TestCase(default(byte))]
    [TestCase(default(short))]
    [TestCase(default(ushort))]
    [TestCase(default(int))]
    [TestCase(default(uint))]
    [TestCase(default(long))]
    [TestCase(default(ulong))]
    [TestCase(default(char))]
    [TestCase(default(float))]
    [TestCase(default(double))]
    [TestCase(default(bool))]
    public void Queue_Primitive_SerializationWorks<T>(T dummy)
    {
        var initial = new Queue<T>();
        initial.Enqueue((T)Convert.ChangeType(1, typeof(T)));
        initial.Enqueue((T)Convert.ChangeType(2, typeof(T)));
        initial.Enqueue((T)Convert.ChangeType(3, typeof(T)));
        
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
