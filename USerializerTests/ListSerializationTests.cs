using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    public class ListSerializationTests
    {
        public void TestReuseBuffer<T>(params T[] elements)
        {
            var list = new List<T>()
            {
                Capacity = elements.Length * 4
            };

            for (int i = 0; i < elements.Length; i++)
            {
                list.Add(elements[i]);
            }

            var memStream = new MemoryStream();

            var listType = typeof(List<T>);
            var itemsMember = listType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = listType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            if (itemsMember == null || sizeMember == null)
                throw new InvalidOperationException("Could not find List internal fields.");

            var runtimeUtils = BinaryUtility.USerializer.RuntimeUtils;
            var itemsField = new FieldAccessHelper<List<T>, T[]>(itemsMember, runtimeUtils);
            var sizeField = new FieldAccessHelper<List<T>, int>(sizeMember, runtimeUtils);
            var array = itemsField.GetFieldRef(ref list);
            var count = sizeField.GetFieldRef(ref list);

            BinaryUtility.Serialize(list, memStream);

            for (var i = 0;
                 i < elements.Length;
                 i++) //fill some of the remaining space with non default values so we can check they are cleared
                list.Add(elements[i]);

            memStream.Position = 0;
            BinaryUtility.TryDeserialize(memStream, ref list);

            var deserializeArray = itemsField.GetFieldRef(ref list);
            var deserializeCount = sizeField.GetFieldRef(ref list);

            Assert.AreEqual(count, deserializeCount);
            Assert.AreSame(array, deserializeArray);

            for (var i = count; i < deserializeArray.Length; i++)
            {
                Console.WriteLine(deserializeArray[i]);
                Assert.AreEqual(default(T), deserializeArray[i]);
            }

            Assert.AreEqual(list.Capacity, elements.Length * 4);
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
        public void ReusePrimitiveBufferSuccess<T>(T dummy)
        {
            var elements = new T[4];
            for (int i = 0; i < 4; i++)
            {
                elements[i] = (T)Convert.ChangeType(i + 1, typeof(T));
            }

            TestReuseBuffer(elements);
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
        public void List_Primitive_SerializationWorks<T>(T dummy)
        {
            var initial = new List<T>();
            initial.Add((T)Convert.ChangeType(1, typeof(T)));
            initial.Add((T)Convert.ChangeType(2, typeof(T)));
            initial.Add((T)Convert.ChangeType(3, typeof(T)));
            
            var result = TestUtils.SerializeDeserializeTest(initial);
            
            Assert.AreEqual(initial.Count, result.Count);
            Assert.AreEqual(initial.ToArray(), result.ToArray());
        }

        [Serializable]
        public class Test
        {
            public int Value;
        }

        [Test]
        public void ReuseClassBufferSuccess()
        {
            TestReuseBuffer(
                new Test()
                {
                    Value = 1
                },
                new Test()
                {
                    Value = 2
                });
        }
    }
}