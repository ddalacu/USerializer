using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using USerialization;

namespace USerializerTests;

public class StackSerializationTests
{
    private void TestReuseBuffer<T>(params T[] elements)
    {
        // Create initial stack with some capacity
        var stack = new Stack<T>(elements.Length * 4);
        foreach (var e in elements)
        {
            stack.Push(e);
        }

        var memStream = new System.IO.MemoryStream();
        
        var stackType = typeof(Stack<T>);
        var itemsMember = stackType.GetField("_array", BindingFlags.Instance | BindingFlags.NonPublic);
        var sizeMember = stackType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);
        
        if (itemsMember == null || sizeMember == null)
            throw new InvalidOperationException("Could not find Stack internal fields.");

        var runtimeUtils = BinaryUtility.USerializer.RuntimeUtils;
        var itemsField = new FieldAccessHelper<object, Array>(itemsMember, runtimeUtils);
        var sizeField = new FieldAccessHelper<object, int>(sizeMember, runtimeUtils);

        object stackObj = stack;
        var arrayBefore = itemsField.GetFieldRef(ref stackObj);
        var sizeBefore = sizeField.GetFieldRef(ref stackObj);

        BinaryUtility.Serialize(stack, memStream);

        // Add some more elements to fill space so we can check if they are cleared
        for (int i = 0; i < elements.Length; i++)
        {
            stack.Push(elements[i]);
        }

        memStream.Position = 0;
        BinaryUtility.TryDeserialize(memStream, ref stack);

        stackObj = stack;
        var arrayAfter = itemsField.GetFieldRef(ref stackObj);
        var sizeAfter = sizeField.GetFieldRef(ref stackObj);

        Assert.That(sizeAfter, Is.EqualTo(sizeBefore));
        Assert.That(arrayAfter, Is.SameAs(arrayBefore));
        
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
    public void Stack_ReusePrimitiveBuffer_Works<T>(T dummy)
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
    public void Stack_ReuseClassBuffer_Works()
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
    public void Stack_Primitive_SerializationWorks<T>(T dummy)
    {
        var initial = new Stack<T>();
        initial.Push((T)Convert.ChangeType(1, typeof(T)));
        initial.Push((T)Convert.ChangeType(2, typeof(T)));
        initial.Push((T)Convert.ChangeType(3, typeof(T)));
        
        var result = TestUtils.SerializeDeserializeTest(initial);
        
        Assert.That(result.Count, Is.EqualTo(initial.Count));
        Assert.That(result.ToArray(), Is.EqualTo(initial.ToArray()));
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
